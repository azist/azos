/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using Azos.Serialization.JSON;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Represents a payload attached to the <see cref="HeapObject"/> graph.
  /// Attachments are needed for optimization as non-essential data such as BLOBs
  /// and other large objest do not need to be materialized while replicating data.
  /// This class alows us to defer the materialization of possibly large payload to a later point
  /// </summary>
  public class Attached : IJsonReadable, IJsonWritable
  {
    protected Attached()
    {
      m_Id = Guid.NewGuid();
    }

    protected Attached(Guid id, Doc doc)
    {
      m_Id = id;
      m_Materialized = true;
      m_Doc = doc;//may be null
    }

    private Guid m_Id;
    private bool m_Materialized;
    private Doc m_Doc;


    /// <summary>
    /// Id of the document attachment within the <see cref="HeapObject"/>
    /// </summary>
    public Guid Id => m_Id;

    /// <summary>
    /// True if the Document was materialized from the store
    /// </summary>
    public bool Materialized => m_Materialized;

    /// <summary>
    /// Returns materialized document or null. Even when <see cref="Materialized"/> is true, this property can still be null if
    /// nothing was materialized
    /// </summary>
    public Doc Doc => m_Doc;


    /// <summary>
    /// This method is called by runtime and biz developes should rarely use
    /// </summary>
    public void MaterializeWith(Doc doc)
    {
      m_Materialized = true;
      m_Doc = doc;
    }


    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string str && Guid.TryParse(str, out var guid))
      {
        this.m_Id = guid;
        return (true, this);
      }

      return (false, null);
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      JsonWriter.EncodeString(wri, m_Id.ToString("D", System.Globalization.CultureInfo.InvariantCulture), options);
    }
  }

  public sealed class Attached<T> : Attached where T : Doc
  {
    public Attached() : base() { }
    public Attached(Guid id, Doc doc) : base(id, doc) { }
  }
}
