/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos.Data.Business;
using Azos.Data.Idgen;

using MySqlConnector;

namespace Azos.Data.Access.MySql
{
  /// <summary>
  /// Provides extension methods for working with MySql-based backends
  /// </summary>
  public static class MySqlExtensions
  {
    /// <summary>
    /// Gets App chassis
    /// </summary>
    public static IApplication GetApp(this MySqlCrudQueryExecutionContext context)
     => context.NonNull(nameof(context)).DataStore.NonNull("ctx.ds").App;

    /// <summary>
    /// Returns current UTC Now using app precision time source
    /// </summary>
    public static DateTime GetUtcNow(this MySqlCrudQueryExecutionContext context) => context.GetApp().GetUtcNow();

    /// <summary>
    /// Returns app cloud origin
    /// </summary>
    public static Atom GetCloudOrigin(this MySqlCrudQueryExecutionContext context) => context.GetApp().GetCloudOrigin();

    /// <summary>
    /// Returns IGdidProvider
    /// </summary>
    public static IGdidProvider GetGdidProvider(this MySqlCrudQueryExecutionContext context) => context.GetApp().GetGdidProvider();

    /// <summary>
    /// Creates new version stamp - an instance of VersionInfo object initialized with callers context
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="gdidScopeName">GDID generation scope name - use EntityIds.ID_NS* constant</param>
    /// <param name="gdidSeqName">GDID generation sequence name - use EntityIds.ID_SEQ* constant</param>
    /// <param name="mode">Form model mode</param>
    /// <returns>New instance of VersionInfo with generated GDID and other fields stamped from context</returns>
    public static VersionInfo MakeVersionInfo(this MySqlCrudQueryExecutionContext context, string gdidScopeName, string gdidSeqName, FormMode mode)
      => context.GetApp().MakeVersionInfo(gdidScopeName, gdidSeqName, mode);


    /// <summary>
    /// Creates new version stamp - an instance of VersionInfo object initialized with callers context
    /// </summary>
    /// <param name="context">Operation context</param>
    /// <param name="gVersion">GDID allocated externally</param>
    /// <param name="mode">Form model mode</param>
    /// <returns>New instance of VersionInfo with generated GDID and other fields stamped from context</returns>
    public static VersionInfo MakeVersionInfo(this MySqlCrudQueryExecutionContext context, GDID gVersion,  FormMode mode)
      => context.GetApp().MakeVersionInfo(gVersion, mode);

    /// <summary>
    /// Executes the specified number of command steps by calling `ExecuteNonQuery` for each under a transaction
    /// committing it on success or rolling back on any failure.
    /// The timeout is expressed IN SECONDS (MSFT legacy behavior).
    /// Returns total rows affected across all call steps
    /// </summary>
    public static async Task<long> ExecuteCompoundCommand(this MySqlCrudQueryExecutionContext context,
                                                          int timeoutSec,
                                                          System.Data.IsolationLevel isolation,
                                                          params Action<MySqlCommand>[] steps)
    {
      var result = 0L;

      using (var cmd = context.NonNull(nameof(context)).Connection.CreateCommand())
      {
        cmd.CommandTimeout = timeoutSec;//in Seconds
        using (var tx = context.Connection.BeginTransaction(isolation))
        {
          try
          {
            foreach (var step in steps.NonNull(nameof(steps)))
            {
              cmd.CommandText = string.Empty;
              cmd.Parameters.Clear();
              step(cmd);
              cmd.Transaction = tx;
              context.ConvertParameters(cmd.Parameters);
              var affected = await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
              result += affected;
            }
            await tx.CommitAsync().ConfigureAwait(false);
            GeneratorUtils.LogCommand(context.DataStore, "ExecCompoundProc-ok", cmd, null);
          }
          catch (Exception error)
          {
            await tx.RollbackAsync().ConfigureAwait(false);
            GeneratorUtils.LogCommand(context.DataStore, "ExecCompoundProc-error", cmd, error);
            throw;
          }
        }
      }

      return result;
    }


    /// <summary>
    /// Adds MySQL parameters with corresponding names for VersionInfo object.
    /// You can optionally override parameter names, otherwise the default ones are used
    /// </summary>
    public static void MapVersionToSqlParameters(this MySqlCommand cmd,
                                                 VersionInfo version,
                                                 string p_gdid = null,
                                                 string p_utc = null,
                                                 string p_origin = null,
                                                 string p_actor = null,
                                                 string p_state = null)
    {
      var pars = cmd.NonNull(nameof(cmd)).Parameters;
      pars.AddWithValue(p_gdid.Default("gdid"), version.NonNull(nameof(version)).G_Version);
      pars.AddWithValue(p_utc.Default("version_utc"), version.Utc);
      pars.AddWithValue(p_origin.Default("version_origin"), version.Origin);
      pars.AddWithValue(p_actor.Default("version_actor"), version.Actor);
      pars.AddWithValue(p_state.Default("version_state"), VersionInfo.MapCanonicalState(version.State));
    }


  }
}
