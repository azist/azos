/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using Azos.Apps;
using Azos.Log;

using Azos.Sky.Metabase;

namespace Azos.Sky.Apps.HostGovernor
{
  /// <summary>
  /// Represents an application managed by the HostGovernorService instance - the Sky application that gets installed/updated/executed by
  ///  the Sky Governor Process. The standard service's Start/Stop commands launch the actual application process
  /// </summary>
  public sealed class ManagedApp : Daemon<HostGovernorService>
  {
      #region CONSTS

        public const int APP_PROCESS_LAUNCH_TIMEOUT_MS = 20000;

      #endregion


      #region .ctor
        internal ManagedApp(HostGovernorService director, Metabank.SectionRole.AppInfo appInfo) : base(director)
        {
          Name = appInfo.ToString();
          m_AppInfo = appInfo;
          m_Packages = SkySystem.HostMetabaseSection.GetAppPackages(appInfo.Name).ToList();
        }
      #endregion

      #region Fields
        private Metabank.SectionRole.AppInfo m_AppInfo;
        internal int m_StartOrder;
        private List<Metabank.SectionApplication.AppPackage> m_Packages;

        private Process m_Process;


      #endregion

      #region Properties

        /// <summary>
        /// Returns the AppInfo as feteched from the metabase
        /// </summary>
        public Metabank.SectionRole.AppInfo AppInfo { get{ return m_AppInfo;}}


        /// <summary>
        /// Returns packages that this application have
        /// </summary>
        public IEnumerable<Metabank.SectionApplication.AppPackage> Packages { get{return m_Packages;}}

        /// <summary>
        /// Returns the start order of this app - when it was launched relative to others
        /// </summary>
        public int StartOrder { get{ return m_StartOrder;}}


        /// <summary>
        /// Returns executable launch command obtained from the role, or if it is blank from app itself
        /// </summary>
        public string ExeFile
        {
          get
          {
            var result = AppInfo.ExeFile;
            if (result.IsNotNullOrWhiteSpace()) return result;
            result = SkySystem.Metabase.CatalogApp.Applications[AppInfo.Name].ExeFile;
            return result;
          }
        }

        /// <summary>
        /// Returns executable launch command arguments obtained from the role, or if it is blank from app itself
        /// </summary>
        public string ExeArgs
        {
          get
          {
            var result = AppInfo.ExeArgs;
            if (result.IsNotNullOrWhiteSpace()) return result;
            result = SkySystem.Metabase.CatalogApp.Applications[AppInfo.Name].ExeArgs;
            return result;
          }
        }


      #endregion

      #region Public


      #endregion

      #region Protected

        protected override void DoStart()
        {
          try
          {
            startProcess();
          }
          catch(Exception error)
          {
            AbortStart();
            log(MessageType.CatastrophicError, "DoStart()", "Svc start leaked: " + error.ToMessageWithType(), error);
            throw error;
          }
        }



        protected override void DoSignalStop()
        {
          try
          {
            if (m_Process!=null)
            {
              if (!m_Process.HasExited)
              {
                log(MessageType.Info, "DoSignalStop()", "Sending application process a line to gracefully exit");
                m_Process.StandardInput.WriteLine("");//Gracefully tells Application to exit
              }
            }
          }
          catch(Exception error)
          {
            log(MessageType.CatastrophicError, "DoSignalStop()", "Svc signal stop leaked: " + error.ToMessageWithType(), error);
            throw error;
          }
        }

        protected override void DoWaitForCompleteStop()
        {
          try
          {
              closeProcess();
          }
          catch(Exception error)
          {
            log(MessageType.CatastrophicError, "DoWaitForCompleteStop()", "Svc stop leaked: " + error.ToMessageWithType(), error);
            throw error;
          }
        }

      #endregion

      #region .pvt .impl

        private void processExited(object sender, EventArgs args)
        {
          if (Status != DaemonStatus.Active) return;//do not use Running here as it also checks for Starting

          try
          {
            closeProcess();
          }
          catch(Exception error)
          {
            log(MessageType.CatastrophicError, "processExited()", "Process exited leaked: " + error.ToMessageWithType(), error);
            throw error;
          }
        }

        private void startProcess()
        {
           var rel = Guid.NewGuid();
           var exe = System.IO.Path.Combine(ComponentDirector.RunPath, ExeFile);
           var args = ExeArgs;

            m_Process = new Process();
            m_Process.StartInfo.FileName = exe;
            m_Process.StartInfo.WorkingDirectory = ComponentDirector.RunPath;
            m_Process.StartInfo.Arguments = args;
            m_Process.StartInfo.UseShellExecute = false;
            m_Process.StartInfo.CreateNoWindow = true;
            m_Process.StartInfo.RedirectStandardInput = true;
            m_Process.StartInfo.RedirectStandardOutput = true;
            m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            m_Process.EnableRaisingEvents = true;//this must be true to get events
            m_Process.Exited += processExited;


            log(MessageType.Info, "startProcess()", "Starting '{0}'/'{1}'".Args(exe, args), null, rel);
            m_Process.Start();
            log(MessageType.Info, "startProcess()", "Process Started. Waiting for OK.", null, rel);

            var watch = Stopwatch.StartNew();
            while (!m_Process.HasExited &&
                    (m_Process.StandardOutput==null || m_Process.StandardOutput.EndOfStream) &&
                    watch.ElapsedMilliseconds < APP_PROCESS_LAUNCH_TIMEOUT_MS)
            {
              Thread.Sleep(500);
            }

            if (m_Process.HasExited)
              throw new AHGOVException(StringConsts.AHGOV_APP_PROCESS_CRASHED_AT_STARTUP_ERROR.Args(Name, exe, args));

            if (m_Process.StandardOutput==null)
              throw new AHGOVException(StringConsts.AHGOV_APP_PROCESS_STD_OUT_NULL_ERROR.Args(Name, exe, args));

            if (!m_Process.StandardOutput.EndOfStream)
            {
                if (m_Process.StandardOutput.Read()=='O' &&
                    m_Process.StandardOutput.Read()=='K' &&
                    m_Process.StandardOutput.Read()=='.')
                {
                    log(MessageType.Info, "startProcess()", "Started and returned OK. '{0}'/'{1}'".Args(exe, args), null, rel);
                    return;//success
                }
            }

            throw new AHGOVException(StringConsts.AHGOV_APP_PROCESS_NO_SUCCESS_AT_STARTUP_ERROR.Args(Name, exe, args));
        }

        private void closeProcess()
        {
            var rel = Guid.NewGuid();

            if (m_Process!=null)
            {
              if (!m_Process.HasExited)
              {
                log(MessageType.Info, "closeProcess()", "Waiting for app process to exit...", null, rel);
             //   m_Process.StandardInput.WriteLine("");//Gracefully tells Application to exit
                m_Process.WaitForExit();
                log(MessageType.Info, "closeProcess()", "App process exited", null, rel);
              }
              m_Process.Close();
              m_Process = null;
            }
        }


        internal void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
        {
           ComponentDirector.log(type, "ManagedApp({0}).{1}".Args(Name, from), text, error, related);
        }
      #endregion
  }
}
