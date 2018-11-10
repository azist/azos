using System;
using System.Linq;
using System.Text;

using Azos.Glue;

namespace Azos.Sky
{
  /// <summary>
  /// Provides common extensions methods for Sky
  /// </summary>
  public static class SkyExtensions
  {
      /// <summary>
      /// Tries to resolve mnemonic name of the Sky service into port, i.e. "hgov" into int port number
      /// </summary>
      public static Node ToResolvedServiceNode(this Node node, bool appTerminal = false)
      {
        if (!node.Assigned ) return node;

        if (node.Binding.IsNullOrWhiteSpace())
          node = new Node("{0}://{1}:{2}".Args(SysConsts.DEFAULT_BINDING, node.Host, node.Service));

        int p;
        if (int.TryParse(node.Service, out p)) return node;

        var sync = node.Binding.Trim().EqualsIgnoreCase(SysConsts.SYNC_BINDING);
        var port = ServiceNameToPort(node.Service, sync, appTerminal);
        return new Node("{0}://{1}:{2}".Args(node.Binding, node.Host, port));
      }

      /// <summary>
      /// Tries to resolve mnemonic name of the Sky service into port, i.e. "hgov" into int port number
      /// </summary>
      public static Node ToResolvedServiceNode(this string connectString, bool appTerminal = false)
      {
        return ToResolvedServiceNode(new Node(connectString), appTerminal);
      }

      /// <summary>
      /// Translates mnemonic name of the major service (i.e. "hgov") into its default port
      /// </summary>
      public static int ServiceNameToPort(string service, bool sync, bool appTerminal)
      {
        var result = SysConsts.DEFAULT_ZONE_GOV_SVC_SYNC_PORT;
        if (service.IsNotNullOrWhiteSpace())
        {
          service = service.Trim().ToLowerInvariant();
          if (service=="ahgov" ||
              service=="hgov" ||
              service=="hgv" ||
              service=="h") result = SysConsts.DEFAULT_HOST_GOV_SVC_SYNC_PORT;

          else if (service=="azgov" ||
                   service=="zgov" ||
                   service=="zgv" ||
                   service=="z") result = SysConsts.DEFAULT_ZONE_GOV_SVC_SYNC_PORT;

          else if (service=="agdida" ||
                   service == "gdida" ||
                   service == "gdid" ||
                   service=="id" ||
                   service=="g") result = SysConsts.DEFAULT_GDID_AUTH_SVC_SYNC_PORT;

          else if (service=="aws" ||
                   service=="www" ||
                   service=="web" ||
                   service=="http") result = SysConsts.DEFAULT_AWS_SVC_SYNC_PORT;

          else if (service=="aph" ||
                   service=="proc" ||
                   service=="ph") result = SysConsts.DEFAULT_PH_SVC_SYNC_PORT;

          else if (service=="log") result = SysConsts.DEFAULT_LOG_SVC_SYNC_PORT;

          else if (service=="telem" ||
                   service=="telemetry" ||
                   service=="tlm") result = SysConsts.DEFAULT_TELEMETRY_SVC_SYNC_PORT;

          else if (service=="wm" ||
                   service=="msg" ||
                   service=="wmsg") result = SysConsts.DEFAULT_WEB_MESSAGE_SYSTEM_SVC_APPTERM_PORT;

          else if (service=="ash" || service=="sh")
          {
            if (appTerminal) return SysConsts.DEFAULT_ASH_APPTERM_PORT;
            else throw new SkyException("Not supported ASH service");
          }
          else throw new SkyException("Not supported service: `{0}`".Args(service));
        }

        return appTerminal ? result + SysConsts.APP_TERMINAL_PORT_OFFSET : sync ? result : result+1;
      }


      /// <summary>
      /// Checks two strings for region paths that reference the same regional entity disregarding entity extensions (such as '.r' or '.noc').
      /// Note: this method DOES NOT check whether this path resolves to actual catalog entity as it only compares names.
      /// This function should be used in conjunction with  GetRegionPathHashCode() while implementing Equals/GetHashCode.
      /// Ignores dynamic host name suffixes
      /// </summary>
      public static bool IsSameRegionPath(this string path1, string path2)
      {
         if (path1==null || path2==null) return false;

         var segs1 = path1.Split('/').Where(s=>s.IsNotNullOrWhiteSpace()).ToList();
         var segs2 = path2.Split('/').Where(s=>s.IsNotNullOrWhiteSpace()).ToList();

         if (segs1.Count!=segs2.Count) return false;

         for(var i=0; i<segs1.Count; i++)
         {
            var seg1 = segs1[i].Trim();
            var seg2 = segs2[i].Trim();

            if (i==segs1.Count-1)//last segment
            {
              var si = seg1.LastIndexOf(Metabase.Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR); if (si>0) seg1 = seg1.Substring(0, si).Trim();
                  si = seg2.LastIndexOf(Metabase.Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR); if (si>0) seg2 = seg2.Substring(0, si).Trim();
            }

            var di = seg1.LastIndexOf('.'); if (di>0) seg1 = seg1.Substring(0, di);
                di = seg2.LastIndexOf('.'); if (di>0) seg2 = seg2.Substring(0, di);

            if (!seg1.EqualsIgnoreCase( seg2 )) return false;
         }

         return true;
      }

      /// <summary>
      /// Deletes region extensions (such as '.r' or '.noc') from path
      /// Ignores dynamic host name suffixes
      /// </summary>
      public static string StripPathOfRegionExtensions(this string path)
      {
         if (path==null) return null;

         var segs = path.Split('/').Where(s=>s.IsNotNullOrWhiteSpace()).ToList();
         var result = new StringBuilder();
         for(var i=0; i<segs.Count; i++)
         {
            var seg = segs[i];

            if (i==segs.Count-1)//last segment
            {
              var si = seg.LastIndexOf(Metabase.Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR);
              if (si>0) seg = seg.Substring(0, si).Trim();
            }

            var di = seg.LastIndexOf('.');
            if (di>0) seg = seg.Substring(0, di);

            if (i>0) result.Append('/');
            result.Append(seg.Trim());
         }

         return result.ToString();
      }

      /// <summary>
      /// Computes has code for region path string disregarding case and extensions (such as '.r' or '.noc')
      /// Note: This function should be used in conjunction with  IsSameRegionPath() while implementing Equals/GetHashCode
      /// Ignores dynamic host name suffixes
      /// </summary>
      public static int GetRegionPathHashCode(this string path)
      {
         var stripped = path.StripPathOfRegionExtensions();
         if (stripped==null) return 0;
         return stripped.ToLowerInvariant().GetHashCode();
      }


      /// <summary>
      /// Returns true if the supplied string is a valid name
      /// a name that does not contain SysConsts.NAME_INVALID_CHARS
      /// </summary>
      public static bool IsValidName(this string name)
      {
        if (name==null) return false;

        var started = false;
        var ws = false;

          for(var i=0; i<name.Length; i++)
          {
            var c = name[i];
            ws = Char.IsWhiteSpace(c);
            if (!started && ws) return false;//no leading whitespace
            if (!ws) started = true;
            if (SysConsts.NAME_INVALID_CHARS.Contains(c)) return false;
          }

        if (!started || ws) return false;//trailing whitespace

        return true;
      }
  }
}
