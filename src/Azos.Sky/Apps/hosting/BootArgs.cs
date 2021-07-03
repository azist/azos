/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using System.Collections.Generic;
using System.Linq;

namespace Azos.Sky.Apps.Hosting
{
  /// <summary>
  /// Assists app host in interpreting boot arguments
  /// </summary>
  /// <example>
  /// <code>
  /// #run console app XYZ
  /// $ host xyz.laconf
  ///
  /// #run app with governor port 49101
  /// $ host governor 49101 xyz.laconf
  /// $ host governor 49101 -config xyz.laconf
  ///
  /// # run host as daemon with governor at port 49101
  /// $ host daemon governor 49101
  /// </code>
  /// </example>
  public sealed class BootArgs
  {
    public const string DAEMON = "daemon";
    public const string GOV = "governor";

    public BootArgs(string[] args)
    {
      m_Original = args.NonNull(nameof(args));
      m_ForApplication = parse(args);
    }

    private string[] m_Original;
    private string[] m_ForApplication;
    private bool m_IsDaemon;
    private int m_GovPort;

    /// <summary>
    /// Original command line args as supplied in .ctor
    /// </summary>
    public string[] Original => m_Original;

    /// <summary>
    /// Command line arguments stripped of system boot args (as if they were never specified)
    /// as supplied for app container
    /// </summary>
    public string[] ForApplication => m_ForApplication;

    /// <summary>
    /// True if the process is a daemon, such as windows service
    /// </summary>
    public bool IsDaemon => m_IsDaemon;


    /// <summary>
    /// True if the process has a governor at the specified port
    /// </summary>
    public bool IsGoverned => m_GovPort > 0;

    /// <summary>
    /// If greater than zero, then the process is being launched under parent governor process
    /// and this process is expected to establish a SIPC (Simple IPC) communication with the governor at
    /// this specified port on local loop TCP interface
    /// </summary>
    public int GovernorPort => m_GovPort;


    private string[] parse(string[] args)
    {
      var result = parseDirectives(args);

      //If you want to treat single argument as an argument and not config then use config like so:
      //$ host argument.laconf -config host.laconf

      if (result.Count == 1 && Conf.Configuration.IsSupportedFormat(System.IO.Path.GetExtension(result[0])))
      {
        result.Insert(0, "-{0}".Args(Azos.Apps.CommonApplicationLogic.CONFIG_SWITCH));
      }

      return result.ToArray();
    }

    private List<string> parseDirectives(string[] args)
    {
      if (args.Length == 0) return new List<string>();

      if (args[0].EqualsOrdIgnoreCase(DAEMON))  //daemon
      {
        if (args.Length > 2)  // daemon governor 1234
        {
          if (args[1].EqualsOrdIgnoreCase(GOV) && int.TryParse(args[2], out var port))
          {
            m_IsDaemon = true;
            m_GovPort = port;
            return args.Skip(3).ToList();
          }
        }
        m_IsDaemon = true;
        return args.Skip(1).ToList();
      }

      if (args.Length > 1)
      {
        if (args[0].EqualsOrdIgnoreCase(GOV) && int.TryParse(args[1], out var port)) //daemon 1234
        {
          m_GovPort = port;
          return args.Skip(2).ToList();
        }
      }

      return args.ToList();
    }
  }
}
