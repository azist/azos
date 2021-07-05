/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using System.Collections.Generic;
using System.Linq;

using Azos.Glue;

namespace Azos.Apps.Hosting
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
  /// $ host gov://49101:app1/1 xyz.laconf
  /// $ host gov://49101:app1/1 -config xyz.laconf
  ///
  /// # run host as daemon with governor at port 49101
  /// $ host daemon gov://49101:app1/1
  ///
  /// # run host as daemon with governor at host `mix01` port 49101
  /// $ host daemon gov://mix01#49101:app1/1
  /// </code>
  /// </example>
  public sealed class BootArgs
  {
    public const string DAEMON = "daemon";
    public const string GOV_BINDING = "gov";

    public BootArgs(string[] args)
    {
      m_Original = args.NonNull(nameof(args));
      m_ForApplication = parse(args);
    }

    private string[] m_Original;
    private string[] m_ForApplication;
    private bool m_IsDaemon;
    private string m_GovHost;
    private int m_GovPort;
    private string m_GovApp;

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
    /// True if the process has a governor at the specified node
    /// </summary>
    public bool IsGoverned => m_GovPort > 0;

    /// <summary>
    /// If processed launched with `gov://[{host}#]{port}:{app}` pragma, then the process is being launched under parent governor process
    /// and this process is expected to establish a SIPC (Simple IPC) communication with the governor at
    /// this specified port on the specified host or local loop TCP interface
    /// </summary>
    public string GovHost => m_GovHost;

    /// <summary>
    /// If processed launched with `gov://[{host}#]{port}:{app}` pragma, then the process is being launched under parent governor process
    /// and this process is expected to establish a SIPC (Simple IPC) communication with the governor at
    /// this specified port on the specified host or local loop TCP interface
    /// </summary>
    public int GovPort  => m_GovPort;

    /// <summary>
    /// If processed launched with `gov://[{host}#]{port}:{app}` pragma, then the process is being launched under parent governor process
    /// and this process is expected to establish a SIPC (Simple IPC) communication with the governor at
    /// this specified port on the specified host or local loop TCP interface
    /// </summary>
    public string GovApp => m_GovApp;


    private string[] parse(string[] args)
    {
      var result = parseDirectives(args);

      //If you want to treat single argument as an argument and not config then use config like so:
      //$ host argument.laconf -config host.laconf

      if (result.Count == 1 && Conf.Configuration.IsSupportedFormat(System.IO.Path.GetExtension(result[0])))
      {
        result.Insert(0, "-{0}".Args(CommonApplicationLogic.CONFIG_SWITCH));
      }

      return result.ToArray();
    }

    private List<string> parseDirectives(string[] args)
    {
      if (args.Length == 0) return new List<string>();

      if (args[0].EqualsOrdIgnoreCase(DAEMON))  //daemon
      {
        if (args.Length > 1)  // daemon gov://1234:app1
        {
          if (tryParseGov(args[1]))
          {
            m_IsDaemon = true;
            return args.Skip(2).ToList();
          }
        }

        m_IsDaemon = true;
        return args.Skip(1).ToList();
      }

      if (args.Length > 0)
      {
        if (tryParseGov(args[0]))
        {
          return args.Skip(1).ToList();
        }
      }

      return args.ToList();
    }

    private bool tryParseGov(string uri)
    {
      var node = new Node(uri);
      if (!node.Binding.EqualsOrdIgnoreCase(GOV_BINDING)) return false;

      var host = node.Host;
      if (host.IsNullOrWhiteSpace()) return false;

      var app = node.Service;
      if (app.IsNullOrWhiteSpace()) return false;

      var port = 0;

      var kvp = host.SplitKVP('#');
      if (kvp.Key.IsNullOrWhiteSpace()) return false;

      if (kvp.Value.IsNullOrWhiteSpace())
      {
        host = null;
        if (!int.TryParse(kvp.Key, out port)) return false;
      }
      else
      {
        host = kvp.Key;
        if (!int.TryParse(kvp.Value, out port)) return false;
      }

      m_GovHost = host;
      m_GovPort = port;
      m_GovApp = app;

      return true;
    }


  }
}
