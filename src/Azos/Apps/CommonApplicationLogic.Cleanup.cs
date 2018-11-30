/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Log;

namespace Azos.Apps { partial class CommonApplicationLogic {

    protected virtual void CleanupApplication()
    {
      var exceptions = new List<Exception>();

      lock(m_FinishNotifiables)
      {
          string name = CoreConsts.UNKNOWN;
          foreach(var notifiable in m_FinishNotifiables)
          try
          {
              name = notifiable.Name ?? notifiable.GetType().FullName;
              notifiable.ApplicationFinishBeforeCleanup(this);
          }
          catch(Exception error)
          {
              error = new AzosException(StringConsts.APP_FINISH_NOTIFIABLE_BEFORE_ERROR.Args(name, error.ToMessageWithType()), error);
              exceptions.Add(error);
              WriteLog(MessageType.Error, "CleanupApplication()", error.Message);
          }
      }

      try
      {
        DoModuleBeforeCleanupApplication();
        DoCleanupApplication(); //<----------------------------------------------
      }
      finally
      {
        ExecutionContext.__UnbindApplication(this);
      }

      lock(m_FinishNotifiables)
      {
          string name = CoreConsts.UNKNOWN;
          foreach(var notifiable in m_FinishNotifiables)
          try
          {
              name = notifiable.Name ?? notifiable.GetType().FullName;
              notifiable.ApplicationFinishAfterCleanup(this);
          }
          catch(Exception error)
          {
            error = new AzosException(StringConsts.APP_FINISH_NOTIFIABLE_AFTER_ERROR.Args(name, error.ToMessageWithType()), error);
            exceptions.Add(error);
            //log not available at this point
          }
      }

      if (exceptions.Count>0)
      {
          var text= new StringBuilder();

          text.AppendLine(StringConsts.APP_FINISH_NOTIFIABLES_ERROR);

          foreach(var exception in exceptions)
              text.AppendLine( exception.ToMessageWithType());

          throw new AzosException(text.ToString());
      }

    }


    protected virtual void DoModuleBeforeCleanupApplication()
    {
        const string FROM = "app.mod.cleanup";

        if (m_Module!=null)
        {
          WriteLog(MessageType.Info, FROM, "Call module root .ApplicationBeforeCleanup()");
          try
          {
            m_Module.ApplicationBeforeCleanup(this);
          }
          catch(Exception error)
          {
            WriteLog(MessageType.CatastrophicError, FROM, "Error in call module root .ApplicationBeforeCleanup()" + error.ToMessageWithType(), error);
          }
        }
    }

    const string CLEANUP_FROM = "app.cleanup";

    protected virtual void DoCleanupApplication()
    {
      //the order of root component cleanup is the reverse of boot:

      CleanupComponent(ref m_DependencyInjector, "DI");           //10. di
      CleanupComponent(ref m_Glue, "Glue");                       //9.  glue
      CleanupComponent(ref m_ObjectStore, "Objectstore");         //8.  object store
      CleanupComponent(ref m_DataStore, "DataStore");             //7.  data store
      CleanupComponent(ref m_Instrumentation, "Instrumentation"); //6.  instrumentation
      CleanupComponent(ref m_EventTimer, "EventTimer");           //5.  event scheduler/bg jobs
      CleanupComponent(ref m_SecurityManager, "Secman");          //4.  security context
      CleanupComponent(ref m_TimeSource , "TimeSource");          //3.  start accurate time asap
      CleanupComponent(ref m_Module, "Module");                   //2.  other services may use module references
      CleanupComponent(ref m_Log, "Log");                         //1.  must be the last one so others can log
    }


    protected virtual void CleanupComponent<T>(ref T cmp, string name) where T : class, IDisposable
    {
      if (cmp==null) return;

      WriteLog(MessageType.Trace, CLEANUP_FROM, "Finalizing {0}".Args(name));
      try
      {
        //if (cmp is Daemon daemon) <- needs CS 7.2 due to <T>
        var daemon = cmp as Daemon;
        if (daemon!=null)
        {
          daemon.WaitForCompleteStop();
          WriteLog(MessageType.Trace, CLEANUP_FROM, "{0} daemon stopped".Args(name));
        }

        DisposeAndNull(ref cmp);
        WriteLog(MessageType.Trace, CLEANUP_FROM, "{0} disposed".Args(name));
      }
      catch (Exception error)
      {
        //don't throw just log
        WriteLog(MessageType.CatastrophicError, CLEANUP_FROM, StringConsts.APP_CLEANUP_COMPONENT_ERROR.Args(name, error.ToMessageWithType()), error);
      }
    }

}}
