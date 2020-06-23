/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;


namespace Azos.Data.Access
{
  /// <summary>
  /// Represents a buffer-less unidirectional reader that binds IEnumerable(Doc) and the backend resource
  /// (such as SQLReader or other object which is internal to the backend).
  /// The cursor is NOT thread-safe and must be disposed properly by closing all resources associated with it.
  /// Only one iteration (one call to GetEnumerator) is possible
  /// </summary>
  public abstract class Cursor : DisposableObject, IEnumerable<Doc>
  {
    protected class enumerator : IEnumerator<Doc>
    {
      internal enumerator(Cursor cursor, IEnumerator<Doc> original)
      {
        Original = original;
        Cursor = cursor;
      }

      private readonly IEnumerator<Doc> Original;
      private readonly Cursor Cursor;

      public Doc Current => Original.Current;

      public void Dispose()
      {
        Original.Dispose();
        Cursor.Dispose();
      }

      object System.Collections.IEnumerator.Current => Original.Current;

      public bool MoveNext() => Original.MoveNext();
      public void Reset() => Original.Reset();
    }

    /// <summary>
    /// This method is not intended to be called by application developers
    /// </summary>
    protected Cursor(IEnumerable<Doc> source)
    {
      m_Source = source;
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Enumerator);
    }

    protected IEnumerable<Doc>  m_Source;
    protected enumerator  m_Enumerator;


    public virtual IEnumerator<Doc> GetEnumerator()
    {
      EnsureObjectNotDisposed();

      if (m_Enumerator!=null) throw new DataAccessException(StringConsts.CRUD_CURSOR_ALREADY_ENUMERATED_ERROR);

      m_Enumerator = new enumerator(this, m_Source.GetEnumerator());
      return m_Enumerator;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
  }

  /// <summary>
  /// Represents a cursor that basically does nothing else but
  /// passes through control to source IEnumerable(Row)
  /// </summary>
  public sealed class PassthroughCursor : Cursor
  {
    public PassthroughCursor(IEnumerable<Doc> source) : base(source)
    {
    }
  }

}
