/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;

namespace Azos
{
  /// <summary>
  /// Provides various extension methods for construction of FieldFilterFunc and casting rowsets
  /// </summary>
  public static class DataUtils
  {
    private static readonly char[] FIELD_DELIMITER = {',',';','|'};


    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc OnlyTheseFields(this string fields, bool caseSensitive = false)
    {
      if (fields.IsNullOrWhiteSpace()) return (row, key, fdef) => false;
      var names = fields.Split(FIELD_DELIMITER).Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => names.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc OnlyTheseFields(this IEnumerable<string> fields, bool caseSensitive = false)
    {
      if (fields==null) return (row, key, fdef) => false;
      var names = fields.Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => fields.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc AllButTheseFields(this string fields, bool caseSensitive = false)
    {
      if (fields.IsNullOrWhiteSpace()) return (row, key, fdef) => false;
      var names = fields.Split(FIELD_DELIMITER).Select( n => n.Trim());
      return (row, key, fdef) => !names.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Converts field names separated by ',' or ';' into a FieldFilterFunction
    /// </summary>
    public static FieldFilterFunc AllButTheseFields(this IEnumerable<string> fields, bool caseSensitive = false)
    {
      if (fields==null) return (row, key, fdef) => false;
      var names = fields.Where(n => n.IsNotNullOrWhiteSpace()).Select( n => n.Trim());
      return (row, key, fdef) => !fields.Contains(fdef.Name, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase);
    }


    /// <summary>
    /// If source is not null, creates a shallow clone using 'source.CopyFields(copy)'
    /// </summary>
    public static TDoc Clone<TDoc>(this TDoc source,
                                   string targetName = null,
                                   bool includeAmorphousData = true,
                                   bool invokeAmorphousAfterLoad = true,
                                   Func<string, Schema.FieldDef, bool> fieldFilter = null,
                                   Func<string, string, bool> amorphousFieldFilter = null) where TDoc : Doc
    {
      if (source==null) return null;
      var copy = Doc.MakeDoc(source.Schema, source.GetType());//must be GetType() not typeof() as we want to clone possibly more derived row as specified by the instance
      source.CopyFields(copy, targetName, includeAmorphousData, invokeAmorphousAfterLoad, fieldFilter, amorphousFieldFilter);
      return (TDoc)copy;
    }


    /// <summary>
    /// Casts enumerable of rows (such as rowset) to the specified row type, returning empty enumerable if the source is null
    /// </summary>
    public static IEnumerable<TDoc> AsEnumerableOf<TDoc>(this IEnumerable<Doc> source) where TDoc : Doc
    {
      if (source==null) return Enumerable.Empty<TDoc>();

      return source.Cast<TDoc>();
    }

    /// <summary>
    /// Loads one document cast per Query(T) or null
    /// </summary>
    public static TDoc LoadDoc<TDoc>(this ICrudOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadDoc(ICRUDOperations==null | query==null)");

      return operations.LoadOneDoc(query) as TDoc;
    }

    /// <summary>
    /// Async version - loads one doc cast per Query(T) or null
    /// </summary>
    public static async Task<TDoc> LoadDocAsync<TDoc>(this ICrudOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadDocAsync(ICRUDOperations==null | query==null)");

      var got = await operations.LoadOneDocAsync(query).ConfigureAwait(false);

      return got as TDoc;
    }

    /// <summary>
    /// Loads docset with docs cast per Query(T) or empty enum
    /// </summary>
    public static IEnumerable<TDoc> LoadEnumerable<TDoc>(this ICrudOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadEnumerable(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowset(query).AsEnumerableOf<TDoc>();
    }

    /// <summary>
    /// Async version - loads docset with rows cast per Query(T) or empty enum
    /// </summary>
    public static async Task<IEnumerable<TDoc>> LoadEnumerableAsync<TDoc>(this ICrudOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadEnumerableAsync(ICRUDOperations==null | query==null)");

      var got = await operations.LoadOneRowsetAsync(query).ConfigureAwait(false);

      return got.AsEnumerableOf<TDoc>();
    }

    /// <summary>
    /// Perform app context injection and calls Validate() on a data Doc
    /// </summary>
    public static Exception Validate(this Doc doc, IApplication app, string targetName = null)
      => app.NonNull(nameof(app)).InjectInto(doc.NonNull(nameof(doc))).Validate(targetName);

    /// <summary>
    /// Perform app context injection and calls SaveAsync() on a data Form<typeparamref name="TSaveResult"/>
    /// </summary>
    public static Task<SaveResult<TSaveResult>> SaveAsync<TSaveResult>(this Form<TSaveResult> form, IApplication app)
      => app.NonNull(nameof(app)).InjectInto(form.NonNull(nameof(form))).SaveAsync();

    /// <summary>
    /// Gets query parameter value by name and casts it to the specified type.
    /// The value must be populated (may not be null)
    /// </summary>
    public static T GetParameterValueAs<T>(this Query qry, string pName)
    {
      var what = $"Query `{qry.NonNull(nameof(qry)).Name}`[`{pName}`]";
      return qry[pName.NonBlank(nameof(pName))].NonNull(what).Value.CastTo<T>(what);
    }

    /// <summary>
    /// Gets query parameter value by name and casts it to the specified type.
    /// If the parameter value is null, DBNull or AbsentValue then returns `defaultValue`,
    /// otherwise casts existing value throwing if it is not type-castable to the requested type
    /// </summary>
    public static T GetParameterValueOrDefaultAs<T>(this Query qry, string pName, T defaultValue)
    {
      var what = $"Query `{qry.NonNull(nameof(qry)).Name}`[`{pName}`]";
      var pv = qry[pName.NonBlank(nameof(pName))].NonNull(what).Value;
      if (pv == null || pv is DBNull || pv is AbsentValue) return defaultValue;
      return pv.CastTo<T>(what);
    }

    /// <summary>
    /// Gets query parameter value by name and casts it to the specified type.
    /// If such named parameter does not exists or its value is null, DBNull or AbsentValue then returns `defaultValue`,
    /// otherwise casts existing value throwing if it is not type-castable to the requested type
    /// </summary>
    public static T GetOptionalParameterValueOrDefaultAs<T>(this Query qry, string pName, T defaultValue)
    {
      var p = qry.NonNull(nameof(Query))[pName.NonBlank(nameof(pName))];
      if (p == null || p.Value == null || p.Value is DBNull || p.Value is AbsentValue) return defaultValue;
      return p.Value.CastTo<T>($"Query `{qry.Name}`[`{pName}`]");
    }

    /// <summary>
    /// Creates new version stamp - an instance of VersionInfo object initialized with callers context
    /// </summary>
    /// <param name="app">Operation context</param>
    /// <param name="gdidScopeName">GDID generation scope name - use EntityIds.ID_NS* constant</param>
    /// <param name="gdidSeqName">GDID generation sequence name - use EntityIds.ID_SEQ* constant</param>
    /// <param name="mode">Form model mode</param>
    /// <returns>New instance of VersionInfo with generated GDID and other fields stamped from context</returns>
    public static VersionInfo MakeVersionInfo(this IApplication app, string gdidScopeName, string gdidSeqName, FormMode mode)
    {
      var state = VersionInfo.DataState.Undefined;
      switch (mode)
      {
        case FormMode.Insert: state = VersionInfo.DataState.Created; break;
        case FormMode.Update: state = VersionInfo.DataState.Updated; break;
        case FormMode.Delete: state = VersionInfo.DataState.Deleted; break;
        default: false.IsTrue("FormMode=C|U|D"); break;
      }

      var result = new VersionInfo
      {
        G_Version = app.GetGdidProvider()
                       .GenerateOneGdid(gdidScopeName, gdidSeqName),
        State = state,
        Utc = app.GetUtcNow(),
        Origin = app.GetCloudOrigin(),
        Actor = Canonical.EntityIds.OfUser()
      };
      return result;
    }

    /// <summary>
    /// If NoCache is true then returns `CacheParams.NoCache` otherwise returns default (null by dflt)
    /// </summary>
    public static ICacheParams NoOrDefaultCache(this bool noCache, ICacheParams dflt = null) => noCache ? CacheParams.NoCache : dflt;
  }
}
