/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Apps.Volatile
{

  /// <summary>
  /// Represents status of ObjectStoreEntry
  /// </summary>
  public enum ObjectStoreEntryStatus { Normal, CheckedOut, ChekedIn, Deleted }

  /// <summary>
  /// Internal framework class that stores data in ObjectStoreService
  /// </summary>
  public class ObjectStoreEntry
  {
    public ObjectStoreEntryStatus Status;

    public DateTime LastTime;

    public Guid Key;
    public object Value;

    public int CheckoutCount;

    public int MsTimeout;


    public override string ToString()
    {
      return Key.ToString();
    }

    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      var o = obj as ObjectStoreEntry;

      if (o == null) return false;

      return Key.Equals(o.Key);
    }

  }

  internal class Bucket : Dictionary<Guid, ObjectStoreEntry>
  {
     public Bucket() : base(0xff) {} //capacity

     public DateTime LastAcquire = DateTime.MinValue;
  }

}
