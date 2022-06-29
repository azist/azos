/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Azos.Data
{
  internal struct iListReadOnly : IList
  {
    public iListReadOnly(List<Doc> data) { m_Data = data; }

    private List<Doc> m_Data;

    public int Add(object value) => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();

    public bool Contains(object value) => m_Data.Contains((Doc)value);

    public int IndexOf(object value) => m_Data.IndexOf((Doc)value);

    public void Insert(int index, object value) => throw new NotImplementedException();

    public bool IsFixedSize => true;

    public bool IsReadOnly => true;

    public void Remove(object value) => throw new NotImplementedException();

    public void RemoveAt(int index) => throw new NotImplementedException();

    public object this[int index]
    {
      get
      {
        return m_Data[index];
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public void CopyTo(Array array, int index) => throw new NotImplementedException();

    public int Count => m_Data.Count;

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    public IEnumerator GetEnumerator() => m_Data.GetEnumerator();
  }
}
