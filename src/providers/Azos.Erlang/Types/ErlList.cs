/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

// Author:  Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/List.cs
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Erlang
{
  public class ErlList : ErlTupleBase
  {
    #region Static

      public static readonly ErlList Empty = new ErlList();

    #endregion

    #region .ctor

      private ErlList() : base() { }

      /// <summary>
      /// Create an Erlang string from the given string
      /// </summary>
      public ErlList(IErlObject[] items, bool clone = true) : base(items, clone) { }

      public ErlList(List<IErlObject> items, bool clone = true) : base(items, clone) { }

      public ErlList(ErlList list) : base((ErlTupleBase)list) { }

      public ErlList(params object[] items) : base(items) { }

      public static ErlList Create(params object[] items)
      {
        return new ErlList(items);
      }

    #endregion

    #region Public

      public override ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlList; } }

      /// <summary>
      /// Implicit conversion of atom to string
      /// </summary>
      public static implicit operator List<IErlObject>(ErlList a)
      {
        return a.m_Items;
      }

      public static bool operator ==(ErlList lhs, IErlObject rhs)
      {
        return (object)lhs == null ? (object)rhs == null : lhs.Equals(rhs);
      }
      public static bool operator !=(ErlList lhs, IErlObject rhs) { return !(lhs == rhs); }

      public override bool Equals(object o)
      {
        return o is IErlObject && Equals((IErlObject)o);
      }

      /// <summary>
      /// Determine if two Erlang objects are equal
      /// </summary>
      public override bool Equals(IErlObject o)
      {
        if (o is ErlList) return Equals((ErlList)o);
        if (o is ErlString)
        {
          var rhs = ((ErlString)o).Value;
          if (rhs.Length != m_Items.Count) return false;
          return !rhs.Where((t, i) => !m_Items[i].IsInt() || (int)t != m_Items[i].ValueAsInt).Any();
        }
        return false;
      }

      /// <summary>
      /// Determine if two Erlang lists are equal
      /// </summary>
      public bool Equals(ErlList o) { return m_Items.SequenceEqual(o.m_Items); }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override string ValueAsString
      {
        get
        {
          if (m_Items.Count == 0) return "";
          if (m_Items.Select(i => !(i is ErlByte)).Any())
            throw new ErlIncompatibleTypesException(this, typeof(string));
          return Encoding.UTF8.GetString(m_Items.Select(i => (byte)((ErlByte)i).ValueAsInt).ToArray());
        }
      }

    #endregion

    #region Protected

      protected override char OpenBracket { get { return '['; } }
      protected override char CloseBracket { get { return ']'; } }

      protected override ErlTupleBase MakeInstance() { return new ErlList(); }

    #endregion
  }
}