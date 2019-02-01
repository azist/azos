using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Azos.Apps
{
  /// <summary>
  /// Provides default resolution of app vars.
  /// </summary>
  /// <remarks>
  /// This was purposely made non-injectable static for security and design simplicity
  /// </remarks>
  public static class DefaultAppVarResolver
  {
    public const string CORE_CONSTS_PREFIX = "CoreConsts.";

    /// <summary>
    /// Application instance Guid with dashes
    /// </summary>
    public const string INSTANCE = "INSTANCE";

    /// <summary>
    /// Application instance Guid without dashes
    /// </summary>
    public const string INSTANCEX = "INSTANCEX";

    /// <summary>
    /// Generates new FID
    /// </summary>
    public const string FID = "FID";

    /// <summary>
    /// Generates new FID returning as X8 string
    /// </summary>
    public const string FIDX = "FIDX";

    public static bool ResolveNamedVar(IApplication app, string name, out string value)
    {
      app.NonNull(nameof(app));
      value = "";
      if (name.IsNullOrWhiteSpace()) return false;

      if (name.EqualsOrdIgnoreCase(FID))
      {
        value = Azos.FID.Generate().ToString();
        return true;
      }

      if (name.EqualsOrdIgnoreCase(FIDX))
      {
        value = Azos.FID.Generate().ID.ToString("X8");
        return true;
      }

      if (name.EqualsOrdIgnoreCase(INSTANCE))
      {
        value = app.InstanceID.ToString("D");// 00000000 - 0000 - 0000 - 0000 - 000000000000
        return true;
      }

      if (name.EqualsOrdIgnoreCase(INSTANCEX))
      {
        value = app.InstanceID.ToString("N");// 00000000000000000000000000000000
        return true;
      }

      if (name.EqualsOrdIgnoreCase(nameof(IApplication.StartTime)))
      {
        value = app.StartTime.ToString();
        return true;
      }


      if (name.StartsWith(CORE_CONSTS_PREFIX))
      {
        var sname = name.Substring(CORE_CONSTS_PREFIX.Length);
        var tp = typeof(CoreConsts);
        var fld = tp.GetField(sname, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static);
        if (fld != null)
        {
          var val = fld.GetValue(null);
          if (val != null)
          {
            value = val.ToString();
            return true;
          }
        }
      }

      return false;
    }
  }
}
