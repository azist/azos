using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ard
{
  public partial class ARDService : ServiceBase
  {
    public const string SERVICE_NAME        = "ARDService";
    public const string SERVICE_DESCRIPTION = "Azos Sky Root Daemon Service";
    public const string DISPLAY_NAME        = "Azos Sky Root Daemon";

    public ARDService()
    {
      ServiceName = SERVICE_NAME;
    }

    private bool m_FirstLaunchOK;
    private bool m_Stopping;
    private string m_HGOVExecutable;
    private Process m_HGOVProcess;

    protected override void OnStart(string[] args)
    {
      try
      {
        m_HGOVExecutable = Utils.GetAHGOVExecutablePath();

        startProcess(true);
        m_FirstLaunchOK = true;
      }
      catch (Exception error)
      {
        EventLog.WriteEntry(string.Format("ARD Service start failed with exception '{0}'. Message: {1}", error.GetType().FullName, error.Message), EventLogEntryType.Error);
        throw error;
      }
    }

    protected override void OnStop()
    {
      m_Stopping = true;
      closeProcess();
    }


    private void hgovProcessExited(object sender, EventArgs args)
    {
      if (!m_FirstLaunchOK) return;//if this is not a subsequent restart
      if (m_Stopping) return;//if the Win Service stopping then do not restart anything


      EventLog.WriteEntry("AHGOV exited. Will check for newer version now and the respawn the AHGOV");
      closeProcess();

      var updateProblem = false;

      //need to find latest version, then rename it into "RUN"
      var updateDir = Utils.GetUpdateDir();
      if (Utils.UpdatePathValid(updateDir))
      {
        EventLog.WriteEntry("Newer version of run packages found. Replacing current RUN dir with: " + updateDir);
        try
        {
          Utils.ReplaceRUN_With_UPDATE(updateDir);
        }
        catch (Exception error)
        {
          updateProblem = true;
          EventLog.WriteEntry(string.Format("ARD Service AHGOV replace RUN with UPDATE failed with exception: '{0}'", error.Message), EventLogEntryType.Error);
        }
      }//else, if there is nothing to update, just restart
      try
      {
        startProcess(false, updateProblem);
      }
      catch (Exception error)
      {
        EventLog.WriteEntry(string.Format("ARD Service AHGOV respawn failed with exception '{0}'. Message: {1}", error.GetType().FullName, error.Message), EventLogEntryType.Error);
        this.Stop();
      }
    }

    private void startProcess(bool onStart, bool updateProblem = false)
    {
      string args;

      if (updateProblem)
        args = string.Format("-{0} -{1}", Utils.ARD_PARENT_CMD_PARAM, Utils.ARD_UPDATE_PROBLEM_CMD_PARAM);
      else
        args = string.Format("-{0}", Utils.ARD_PARENT_CMD_PARAM);

      m_HGOVProcess = new Process();
      m_HGOVProcess.StartInfo.FileName = m_HGOVExecutable;
      m_HGOVProcess.StartInfo.Arguments = args;
      m_HGOVProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(m_HGOVExecutable);
      m_HGOVProcess.StartInfo.UseShellExecute = false;
      m_HGOVProcess.StartInfo.CreateNoWindow = true;
      m_HGOVProcess.StartInfo.RedirectStandardInput = true;
      m_HGOVProcess.StartInfo.RedirectStandardOutput = true;
      m_HGOVProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      m_HGOVProcess.EnableRaisingEvents = true;//this must be true to get events
      m_HGOVProcess.Exited += hgovProcessExited;

      EventLog.WriteEntry("Starting AHGOV: " + m_HGOVExecutable);
      m_HGOVProcess.Start();
      EventLog.WriteEntry("AHGOV process started, Waiting for OK..");

      if (onStart && !Environment.UserInteractive) this.RequestAdditionalTime(2000);

      const int timeout = 15000;
      var watch = Stopwatch.StartNew();

      while (!m_HGOVProcess.HasExited &&
              m_HGOVProcess.StandardOutput.EndOfStream &&
              watch.ElapsedMilliseconds < timeout)
      {
        if (onStart && !Environment.UserInteractive) this.RequestAdditionalTime(1000);
        Thread.Sleep(500);
      }

      if (m_HGOVProcess.HasExited) throw new Exception("HGOV process crashed while startup, see its logs");
      if (!m_HGOVProcess.StandardOutput.EndOfStream)
      {
        if (m_HGOVProcess.StandardOutput.Read() == 'O' &&
            m_HGOVProcess.StandardOutput.Read() == 'K' &&
            m_HGOVProcess.StandardOutput.Read() == '.')
        {
          EventLog.WriteEntry("AHGOV process started, and returned OK.");
          return;//success
        }
      }

      throw new Exception("HGOV did not return success code, see its logs (OR maybe something was written to STDOUT by mistake before OK?)");
    }

    private void closeProcess()
    {
      if (m_HGOVProcess != null)
      {
        if (!m_HGOVProcess.HasExited)
        {
          EventLog.WriteEntry("Sending AHGOV a line to gracefully exit and waiting...");
          m_HGOVProcess.StandardInput.WriteLine("");//Gracefully tells AHGOV to exit
          m_HGOVProcess.WaitForExit();
          EventLog.WriteEntry("AHGOV Exited");
        }
        m_HGOVProcess.Close();
        m_HGOVProcess = null;
      }
    }

    internal void TestStartupAndStop(string[] args)
    {
      Console.WriteLine("Waiting for line to terminate...");
      try
      {
        this.OnStart(args);
        Console.ReadLine();
      }
      finally
      {
        this.OnStop();
      }
    }
  }
}