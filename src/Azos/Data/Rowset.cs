
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Data
{
  /// <summary>
  /// Implements a rowset that supports row doc change logging and CRUD operations.
  /// Rowsets are not optimal for data replication as they perform linear search which is slow, however
  /// Rowset class supports sorting. In contrast, Tables are kind of rowsets that keep data pre-sorted by key
  /// thus facilitating quick searching
  /// </summary>
  [Serializable]
  public class Rowset : RowsetBase
  {
    #region .ctor
    /// <summary>
    /// Creates an empty rowset
    /// </summary>
    public Rowset(Schema schema) : base(schema)
    {
      m_List = new List<Doc>();
      m_SortFieldList = new List<string>();
    }


    /// <summary>
    /// Creates a shallow copy from another rowset, optionally applying a filter
    /// </summary>
    public Rowset(RowsetBase other, Func<Doc, bool> filter = null) : base(other.Schema)
    {
      if (filter == null)
        m_List = new List<Doc>(other.m_List);
      else
        m_List = other.Where(filter).ToList();

      m_SortFieldList = new List<string>();
    }


    #endregion

    #region Fields

    private string m_SortDefinition;
    internal List<string> m_SortFieldList;

    #endregion

    #region Properties

    /// <summary>
    /// Sort definition is a comma-separated field name list where every field may optionally be prefixed with
    /// `+` for ascending or `-` for descending sort order specifier. Example: "FirstName,-DOB"
    /// </summary>
    public string SortDefinition
    {
      get { return m_SortDefinition ?? string.Empty; }
      set
      {
        if (m_SortDefinition != value)
        {
          m_SortDefinition = value;
          m_SortFieldList.Clear();

          if (m_SortDefinition != null)
          {
            m_SortFieldList.AddRange(m_SortDefinition.Split(','));
            m_SortFieldList.RemoveAll(s => s.Trim().Length == 0);
            for (int i = 0; i < m_SortFieldList.Count; i++)
              m_SortFieldList[i] = m_SortFieldList[i].Trim();

            m_List.Sort(this);
          }
        }
      }
    }


    #endregion


    #region IComparer<Row> Members

    public override int Compare(Doc docA, Doc docB)
    {
      if (docA == null && docB != null) return -1;

      if (docA != null && docB == null) return 1;

      if (docA == null && docB == null) return 0;

      if (object.ReferenceEquals(docA, docB)) return 0;

      if (docA.Schema != docB.Schema) return 1;


      foreach (var sortDef in m_SortFieldList)
      {
        var sfld = sortDef.Trim();

        var desc = false;
        if (sfld.StartsWith("+"))
          sfld = sfld.Remove(0, 1);

        if (sfld.StartsWith("-"))
        {
          sfld = sfld.Remove(0, 1);
          desc = true;
        }

        var fld = m_Schema[sfld];
        if (fld == null) return 1;//safeguard

        var obj1 = docA[fld.Order] as IComparable;
        var obj2 = docB[fld.Order] as IComparable;

        if (obj1 == null && obj2 == null) continue;
        if (obj1 == null) return desc ? 1 : -1;
        if (obj2 == null) return desc ? -1 : 1;

        var result = desc ? -obj1.CompareTo(obj2) : obj1.CompareTo(obj2);
        if (result != 0) return result;
      }
      return 0;
    }

    #endregion


    #region Protected

    protected override int DoInsert(Doc doc)
    {
      m_List.Add(doc);
      return m_List.Count - 1;
    }

    #endregion

  }
}
