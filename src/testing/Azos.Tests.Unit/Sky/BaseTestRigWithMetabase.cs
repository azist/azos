/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Scripting;

using Azos.Sky.Apps;
using Azos.Sky.Metabase;


using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;
using Azos.Apps;

namespace Azos.Tests.Unit.Sky
{
  public abstract class BaseTestRigWithMetabase : IRunnableHook
  {
      private FileSystem m_FS;
      private Metabank m_Metabank;
      private SkyApplication m_TestApp;


      public Metabank Metabase { get{ return m_Metabank;}}

      void IRunnableHook.Prologue(Runner runner, FID id)
      {
        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "RigSetup()..."));
        m_FS = new LocalFileSystem(NOPApplication.Instance);
        m_Metabank =  new Metabank(m_FS, null, TestSources.RPATH);
        m_TestApp = new SkyApplication(NOPApplication.Instance, SystemApplicationType.TestRig, m_Metabank, TestSources.THIS_HOST, true, null, null);

        DoRigSetup();

        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "...RigSetup() DONE"));
      }

      bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
      {
        Console.WriteLine("{0}.{1}".Args(GetType().FullName, "RigTearDown()..."));

        DoRigTearDown();

        DisposableObject.DisposeAndNull(ref m_TestApp);
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
