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
                                   bool includeAmorphousData = true,
                                   bool invokeAmorphousAfterLoad = true,
                                   Func<string, Schema.FieldDef, bool> fieldFilter = null,
                                   Func<string, string, bool> amorphousFieldFilter = null) where TDoc : Doc
    {
      if (source==null) return null;
      var copy = Doc.MakeDoc(source.Schema, source.GetType());//must be GetType() not typeof() as we want to clone possibly more derived row as specified by the instance
      source.CopyFields(copy, includeAmorphousData, invokeAmorphousAfterLoad, fieldFilter, amorphousFieldFilter);
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
    public static TDoc LoadDoc<TDoc>(this ICRUDOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadDoc(ICRUDOperations==null | query==null)");

      return operations.LoadOneRow(query) as TDoc;
    }

    /// <summary>
    /// Async version - loads one doc cast per Query(T) or null
    /// </summary>
    public static Task<TDoc> LoadDocAsync<TDoc>(this ICRUDOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadDocAsync(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowAsync(query)
                       .ContinueWith<TDoc>( (antecedent) => antecedent.Result as TRow);
    }

    /// <summary>
    /// Loads docset with docs cast per Query(T) or empty enum
    /// </summary>
    public static IEnumerable<TDoc> LoadEnumerable<TDoc>(this ICRUDOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadEnumerable(ICRUDOperations==null | query==null)");

      return operations.LoadOneRowset(query).AsEnumerableOf<TDoc>();
    }

    /// <summary>
    /// Async version - loads docset with rows cast per Query(T) or empty enum
    /// </summary>
    public static Task<IEnumerable<TDoc>> LoadEnumerableAsync<TDoc>(this ICRUDOperations operations, Query<TDoc> query) where TDoc : Doc
    {
      if (operations==null || query==null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"LoadEnumerableAsync(ICRUDOperations==null | query==null)");

      return operations.LoadOneDocsetAsync(query)
                       .ContinueWith<IEnumerable<TDoc>>( (antecedent) => antecedent.Result.AsEnumerableOf<TDoc>());
    }

  }
}
