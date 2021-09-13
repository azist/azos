/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Azos.Security;
using Azos.Scripting;

namespace Azos.Tests.Integration.Wave
{
  public class WaveTestBase : IRunnableHook
  {
    #region Consts

      private const string DEFAULT_PROCESS_FILENAME = "WaveTestSite.exe";
      private const string WAVE_HTTP_ADDR = "http://localhost:8080";
      public static Uri S_WAVE_URI = new Uri(WAVE_HTTP_ADDR);

      protected const string INTEGRATIONN_TESTER = "IntegrationTester";

      protected const string INTEGRATION_HTTP_ADDR = WAVE_HTTP_ADDR + "/mvc/" + INTEGRATIONN_TESTER + "/";
      protected const string PAGES_HTTP_ADDR = WAVE_HTTP_ADDR + "/pages/";

      private const string WAVE_COOKIE_NAME = "ZEKRET";
      private const string WAVE_COOKIE_VALUE = "Hello";
      public static Cookie S_WAVE_COOKIE = new Cookie(WAVE_COOKIE_NAME, WAVE_COOKIE_VALUE);

    #endregion

    #region Pvt Fields

      private Process m_ServerProcess = new Process();

    #endregion

    #region Init/TearDown

    void IRunnableHook.Prologue(Runner runner, FID id)
    {

Aver.Fail("====================================== As of 20210730 This test suite is no longer supported and needs to be completely re-written in Sky ==================================================== ");


      try
      {
        ProcessStartInfo start;
        if (Platform.Abstraction.PlatformAbstractionLayer.IsNetCore)
          start = new ProcessStartInfo()
          {
            FileName = "dotnet",
            Arguments = "toy.dll wave -config toy-wave.laconf",
            RedirectStandardInput = true,
            UseShellExecute = false
          };
        else
          start = new ProcessStartInfo()
          {
            FileName = "toy",
            Arguments = "wave -config toy-wave.laconf",
            RedirectStandardInput = true,
            UseShellExecute = false
          };

        m_ServerProcess = new Process() { StartInfo = start };

        m_ServerProcess.Start();

        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
      }
      catch (Exception)
      {
        Console.WriteLine("The test must be executed with admin priviledges!");
        throw;
      }
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
Aver.Fail(" As of 20210730 This test suite is no longer supported and needs to be completely re-written in Sky ==================================================== ");

      m_ServerProcess.StandardInput.WriteLine(string.Empty);
      return false;
    }

    #endregion

    #region Protected

    protected virtual string ProcessFileName { get { return DEFAULT_PROCESS_FILENAME; } }

    protected WebClientCookied CreateWebClient()
    {
      var wc = new WebClientCookied();
      wc.CookieContainer.Add(S_WAVE_URI, S_WAVE_COOKIE);
      return wc;
    }

    #endregion
  }
}
