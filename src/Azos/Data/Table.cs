/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Data
{
  /// <summary>
  /// Implements a master table. Tables are rowsets that are pre-sorted by keys.
  /// They are used in scenarios when in-memory data replication is needed.
  /// A table supports efficient FindKey() operation but does not support sorting.
  /// This class is not thread-safe.
  /// </summary>
  [Serializable]
  public class Table : RowsetBase
  {
    #region .ctor

    /// <summary>
    /// Creates an empty table
    /// </summary>
    public Table(Schema schema) : base(schema)
    {
      m_List = new List<Doc>();
    }

    /// <summary>
    /// Creates a shallow copy from another table, optionally applying a filter
    /// </summary>
    public Table(Table other, Func<Doc, bool> filter = null) : base(other.Schema)
    {
      if (filter == null)
        m_List = new List<Doc>(other.m_List);
      else
        m_List = other.Where(filter).ToList();
    }

    /// <summary>
    /// Creates a shallow copy from another rowset resorting data per schema key definition, optionally applying a filter
    /// </summary>
    public Table(Rowset other, Func<Doc, bool> filter = null) : base(other.Schema)
    {
      m_List = new List<Doc>();

      var src = filter == null ? other : other.Where(filter);

      foreach (var doc in src)
        Insert(doc);
    }

    #endregion

    #region Protected

    /// <summary>
    /// Performs binary search on a sorted table
    /// </summary>
    protected override int SearchForDoc(Doc doc, out int index)
    {
      int top = 0;
      int bottom = m_List.Count - 1;

      index = 0;

      while (top <= bottom)
      {

        index = top + ((bottom - top) / 2);

        int cmpResult = Compare(doc, m_List[index]);

        if (cmpResult == 0)
        {
          return index;
        }
        if (cmpResult > 0)
        {
          top = index + 1;
          index = top;
        }
        else
        {
          bottom = index - 1;
        }
      }

      return -1;
    }

    #endregion

    #region IComparer<Doc> Members

    /// <summary>
    /// Compares two rows based on their key fields. Always compares in ascending direction
    /// </summary>
    public override int Compare(Doc rowA, Doc rowB)
    {
      return CompareRows(m_Schema, rowA, rowB);
    }

    /// <summary>
    /// Compares two rows based on their key fields. Always compares in ascending direction
    /// </summary>
    public static int CompareRows(Schema schema, Doc rowA, Doc rowB)
    {
      if (rowA == null && rowB != null) return -1;

      if (rowA != null && rowB == null) return 1;

      if (rowA == null && rowB == null) return 0;

      if (object.ReferenceEquals(rowA, rowB)) return 0;

      if (rowA.Schema != rowB.Schema) return 1;


      foreach (var fld in schema.AnyTargetKeyFieldDefs)
      {
        var obj1 = rowA[fld.Order] as IComparable;
        var obj2 = rowB[fld.Order] as IComparable;

        if (obj1 == null && obj2 == null) continue;
        if (obj1 == null) return -1;
        if (obj2 == null) return +1;

        var result = obj1.CompareTo(obj2);
        if (result != 0) return result;
      }

      return 0;
    }

    #endregion

    #region IList

    /// <summary>
    /// This is IList member implementation, index is ignored
    /// </summary>
    public override void Insert(int index, Doc item)
      => Insert(item);

    /// <summary>
    /// This method does not support setting rows in table
    /// </summary>
    public override Doc this[int index]
    {
      get
      {
        return m_List[index];
      }
      set
      {
        throw new DataException(StringConsts.CRUD_OPERATION_NOT_SUPPORTED_ERROR + "Table[idx].set()");
      }
    }

    #endregion
  }
}
