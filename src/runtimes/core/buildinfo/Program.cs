/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

namespace buildinfo
{
  /* CORE UTILITY must be foirst build artifact in build graph, must not use any references */
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("BuildSeed={0}", System.Environment.TickCount);
      Console.WriteLine("Computer={0}",  System.Environment.MachineName);
      Console.WriteLine("User={0}",      System.Environment.UserName);
      Console.WriteLine("OS={0}",        System.Environment.OSVersion.Platform);
      Console.WriteLine("OSVer={0}",     System.Environment.OSVersion.VersionString);
      Console.WriteLine("UTCTime={0}",  EncodeDateTime(DateTime.UtcNow));

      System.Environment.ExitCode = 0;
    }

    public static string EncodeDateTime(DateTime data)
    {
      using(var wri = new StringWriter())
      {
        var year = data.Year;
        if (year>999) wri.Write(year);
        else if (year>99) { wri.Write('0'); wri.Write(year); }
        else if (year>9) { wri.Write("00"); wri.Write(year); }
        else if (year>0) { wri.Write("000"); wri.Write(year); }

        wri.Write('-');

        var month = data.Month;
        if (month>9) wri.Write(month);
        else { wri.Write('0'); wri.Write(month); }

        wri.Write('-');

        var day = data.Day;
        if (day>9) wri.Write(day);
        else { wri.Write('0'); wri.Write(day); }

        wri.Write('T');

        var hour = data.Hour;
        if (hour>9) wri.Write(hour);
        else { wri.Write('0'); wri.Write(hour); }

        wri.Write(':');

        var minute = data.Minute;
        if (minute>9) wri.Write(minute);
        else { wri.Write('0'); wri.Write(minute); }

        wri.Write(':');

        var second = data.Second;
        if (second>9) wri.Write(second);
        else { wri.Write('0'); wri.Write(second); }

        var ms = data.Millisecond;
        if (ms>0)
        {
            wri.Write('.');

            if (ms>99) wri.Write(ms);
            else if (ms>9) { wri.Write('0'); wri.Write(ms); }
            else { wri.Write("00"); wri.Write(ms); }
        }


        wri.Write('Z');//UTC

        wri.Flush();

        return wri.ToString();
      }
    }


  }
}
