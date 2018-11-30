/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Conf;
using Azos.Log;
using Azos.Text;
using Azos.Serialization.CSV;

namespace Azos.Web.GeoLookup
{
  /// <summary>
  /// Represents a service that can lookup country/city names by  domain names/ip addresses.
  /// This implementation uses free data from: http://dev.maxmind.com/geoip/geoip2/geolite2/.
  /// Must include MaxMind attribution on the public site that uses this data (see License section on maxmind.com)
  /// </summary>
  public sealed class GeoLookupService : Daemon, IGeoLookup
  {
    #region CONSTS

      public const string CONFIG_GEO_LOOKUP_SECTION = "geo-lookup";

    #endregion

    #region .ctor

      public GeoLookupService(IApplication app) : base(app) { }
      public GeoLookupService(IApplicationComponent director) : base(director) { }

    #endregion

    #region Fields

      private bool m_CancelStart;
      private string m_DataPath;
      private LookupResolution m_Resolution = LookupResolution.Country;

      private SubnetTree<IPSubnetBlock> m_SubnetBST;
      private Dictionary<SealedString, Location> m_Locations;

    #endregion

    #region Properties


    public override string ComponentLogTopic => CoreConsts.GEO_TOPIC;

    /// <summary>
    /// Returns true to indicate that service has loaded and ready to serve data
    /// </summary>
    public bool Available { get{ return Status== DaemonStatus.Active;} }


      /// <summary>
      /// Specifies where the data is
      /// </summary>
      [Config]
      public string DataPath
      {
        get { return m_DataPath ?? string.Empty; }
        set
        {
          CheckDaemonInactive();
          m_DataPath = value;
        }
      }

      /// <summary>
      /// Specifies what resolution service provides
      /// </summary>
      [Config]
      public LookupResolution Resolution
      {
        get { return m_Resolution; }
        set
        {
          CheckDaemonInactive();
          m_Resolution = value;
        }
      }

      /// <summary>
      /// Returns true to indicate that previous attempt to start service - load data, was canceled
      /// </summary>
      public bool StartCanceled { get{ return m_CancelStart; } }

    #endregion

    #region Public

      /// <summary>
      /// Tries to lookup the location by ip/dns name. Returns null if no match could be made
      /// </summary>
      public GeoEntity Lookup(IPAddress address)
      {
        if (Status!=DaemonStatus.Active || address==null) return null;

        var block = m_SubnetBST[new Subnet(address)];
        Location location;
        m_Locations.TryGetValue(block.LocationID, out location);
        return new GeoEntity(address, block, location);
      }

      /// <summary>
      /// Cancels service start. This method may be needed when Start() blocks for a long time due to large volumes of data
      /// </summary>
      public void CancelStart()
      {
        m_CancelStart = true;
      }

    #endregion

    #region Protected


      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null || !node.Exists)
          node = App.ConfigRoot[CONFIG_GEO_LOOKUP_SECTION];

        ConfigAttribute.Apply(this, node);
      }


      protected override void DoStart()
      {
        if (m_Resolution!= LookupResolution.Country && m_Resolution!= LookupResolution.City)
          throw new GeoException(StringConsts.GEO_LOOKUP_SVC_RESOLUTION_ERROR.Args(m_Resolution));

        if (!Directory.Exists(m_DataPath))
          throw new GeoException(StringConsts.GEO_LOOKUP_SVC_PATH_ERROR.Args(m_DataPath ?? StringConsts.UNKNOWN));

        var fnBlocks    = Path.Combine(m_DataPath, "GeoLite2-{0}-Blocks-IPv6.csv".Args(m_Resolution));
        var fnBlocksV4 = Path.Combine(m_DataPath, "GeoLite2-{0}-Blocks-IPv4.csv".Args(m_Resolution));
        var fnLocations = Path.Combine(m_DataPath, "GeoLite2-{0}-Locations-en.csv".Args(m_Resolution));

        if (!File.Exists(fnBlocks))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnBlocks));
        if (!File.Exists(fnBlocksV4))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnBlocksV4));
        if (!File.Exists(fnLocations))
            throw new GeoException(StringConsts.GEO_LOOKUP_SVC_DATA_FILE_ERROR.Args(fnLocations));

        m_CancelStart = false;
        m_Locations = new Dictionary<SealedString, Location>();

        try
        {
            const int MAX_PARSE_ERRORS = 8;

            var tree = new BinaryTree<Subnet, IPSubnetBlock>();
            var scope = new SealedString.Scope();
            foreach (var blocksFn in new[] { fnBlocks, fnBlocksV4 })
            {
              using (var stream = new FileStream(blocksFn, FileMode.Open, FileAccess.Read, FileShare.Read, 4*1024*1024))
              {
                int errors = 0;
                try
                {
                  foreach (var row in stream.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 10))
                  {
                    if (m_CancelStart || !App.Active) break;
                    var arr = row.ToArray();
                    var block = new IPSubnetBlock(
                      scope.Seal(arr[0]),
                      scope.Seal(arr[1]),
                      scope.Seal(arr[2]),
                      scope.Seal(arr[3]),
                      arr[4].AsBool(),
                      arr[5].AsBool(),
                      scope.Seal(arr[6]),
                      arr[7].AsFloat(),
                      arr[8].AsFloat());

                    tree[new Subnet(block.Subnet.Value, true)] = block;
                  }
                }
                catch (Exception error)
                {
                  WriteLog(MessageType.Error, "DoStart('{0}')".Args(blocksFn), "Line: {0} {1}".Args(0/*line*/, error.ToMessageWithType()), error);
                  errors++;
                  if (errors > MAX_PARSE_ERRORS)
                  {
                    WriteLog(MessageType.CatastrophicError, "DoStart('{0}')".Args(blocksFn), "Errors > {0}. Aborting file '{1}' import".Args(MAX_PARSE_ERRORS, blocksFn));
                    break;
                  }
                }
              }
            }
            m_SubnetBST = new SubnetTree<IPSubnetBlock>(tree.BuildIndex());

            using (var stream = new FileStream(fnLocations, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * 1024 * 1024))
            {
              try
              {
                foreach (var row in stream.AsCharEnumerable().ParseCSV(skipHeader: true, columns: 13))
                {
                  if (m_CancelStart || !App.Active) break;
                  var arr = row.ToArray();
                  var location = new Location(
                    scope.Seal(arr[0]),
                    scope.Seal(arr[1]),
                    scope.Seal(arr[2]),
                    scope.Seal(arr[3]),
                    scope.Seal(arr[4]),
                    scope.Seal(arr[5]),
                    scope.Seal(arr[6]),
                    scope.Seal(arr[7]),
                    scope.Seal(arr[8]),
                    scope.Seal(arr[9]),
                    scope.Seal(arr[10]),
                    scope.Seal(arr[11]),
                    scope.Seal(arr[12]));
                  m_Locations.Add(location.ID, location);
                }
              }
              catch (CSVParserException error)
              {
                WriteLog(MessageType.Error, "DoStart('{0}')".Args(fnLocations), "Line: {0} Column: {1} {2}".Args(error.Line, error.Column, error.ToMessageWithType()), error);
              }
              catch (Exception error)
              {
                WriteLog(MessageType.Error, "DoStart('{0}')".Args(fnLocations), "{1}".Args(error.ToMessageWithType()), error);
              }
            }
        }
        catch
        {
          m_SubnetBST = null;
          m_Locations = null;
          m_SubnetBST = null;
          throw;
        }

        if (m_CancelStart) throw new GeoException(StringConsts.GEO_LOOKUP_SVC_CANCELED_ERROR);
      }

      protected override void DoWaitForCompleteStop()
      {
        m_SubnetBST = null;
        m_Locations = null;
      }

    #endregion

  }
}
