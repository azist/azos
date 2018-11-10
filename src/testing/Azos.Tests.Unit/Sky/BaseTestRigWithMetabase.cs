using System;
using Azos.Scripting;

using Azos.Sky.Apps;
using Azos.Sky.Metabase;


using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;

namespace Azos.Tests.Unit.Sky
{
  public abstract class BaseTestRigWithMetabase : IRunnableHook
  {
      private FileSystem m_FS;
      private Metabank m_Metabank;
      private IDisposable m_TestSession;


      public Metabank Metabase { get{ return m_Metabank;}}

      void IRunnableHook.Prologue(Runner runner, FID id)
      {
        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "RigSetup()..."));
        m_FS = new LocalFileSystem(null);
        m_Metabank =  new Metabank(m_FS, null, TestSources.RPATH);
        m_TestSession = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, m_Metabank, TestSources.THIS_HOST);

        DoRigSetup();

        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "...RigSetup() DONE"));
      }

      bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
      {
        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "RigTearDown()..."));

        DoRigTearDown();

        DisposableObject.DisposeAndNull(ref m_TestSession);
        DisposableObject.DisposeAndNull(ref m_Metabank);
        DisposableObject.DisposeAndNull(ref m_FS);
        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "...RigTearDown() DONE"));

        return false;
      }


      protected virtual void DoRigSetup()
      {

      }

      protected virtual void DoRigTearDown()
      {

      }

  }
}
