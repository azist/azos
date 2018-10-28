/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

// Author:  Serge Aleynikov <saleyn@gmail.com>
// Created: 2013-08-26
// This code is derived from:
// https://github.com/saleyn/otp.net/blob/master/Otp/Erlang/Port.cs
using System;

namespace Azos.Erlang
{

  /// <summary>
  /// Provides a C# representation of Erlang integer types
  /// </summary>
  public struct ErlPort : IErlObject<ErlPort>
  {
  #region Static

    /// <summary>
    /// Special value non-existing Port used for "null" pid comparison
    /// </summary>
    public static readonly ErlPort Null = new ErlPort(string.Empty, 0, 0);

  #endregion

  #region .ctor

    /// <summary>
    /// Create an Erlang port from the given values
    /// </summary>
    public ErlPort(string node, int id, int creation)
            : this(new ErlAtom(node), id, creation)
    { }

    /// <summary>
    /// Create an Erlang port from the given values
    /// </summary>
    public ErlPort(ErlAtom node, int id, int creation)
    {
      Node = node;
      Id = id & 0x0fffffff; /* 28 bits */
      Creation = creation & 0x3;
    }

  #endregion

  #region Fields

    public readonly ErlAtom Node;
    public readonly int Id;
    public readonly int Creation;

  #endregion

  #region Props

    public ErlTypeOrder TypeOrder { get { return ErlTypeOrder.ErlPort; } }

    public bool IsScalar { get { return true; } }

    public bool Empty { get { return Equals(Null); } }

    public ErlPort Value { get { return this; } }

    public object      ValueAsObject   { get { return this; } }
    public int         ValueAsInt      { get { throw new ErlIncompatibleTypesException(this, typeof(int)); } }
    public long        ValueAsLong     { get { throw new ErlIncompatibleTypesException(this, typeof(long)); } }
    public decimal     ValueAsDecimal  { get { throw new ErlIncompatibleTypesException(this, typeof(decimal)); } }
    public DateTime    ValueAsDateTime { get { throw new ErlIncompatibleTypesException(this, typeof(DateTime)); } }
    public TimeSpan    ValueAsTimeSpan { get { throw new ErlIncompatibleTypesException(this, typeof(TimeSpan)); } }
    public double      ValueAsDouble   { get { throw new ErlIncompatibleTypesException(this, typeof(double)); } }
    public string      ValueAsString   { get { return ToString(); } }
    public bool        ValueAsBool     { get { throw new ErlIncompatibleTypesException(this, typeof(bool)); } }
    public char        ValueAsChar     { get { throw new ErlIncompatibleTypesException(this, typeof(char)); } }
    public byte[]      ValueAsByteArray{ get { throw new ErlIncompatibleTypesException(this, typeof(byte[])); } }

  #endregion

  #region Public

    public static bool operator ==(ErlPort lhs, ErlPort rhs) { return lhs.Equals(rhs); }
    public static bool operator !=(ErlPort lhs, ErlPort rhs) { return !lhs.Equals(rhs); }

    public override string ToString()
    {
      return string.Format("#Port<{0}.{1}>", Node, Id);
    }

    /// <summary>
    /// Determine if this instance equals to the object
    /// </summary>
    public override bool Equals(object o)
    {
      return (o is ErlPort) && Equals((ErlPort)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(IErlObject o)
    {
      return (o is ErlPort) && Equals((ErlPort)o);
    }

    /// <summary>
    /// Determine if two instances are equal
    /// </summary>
    public bool Equals(ErlPort port)
    {
      return (Creation == port.Creation) && (Id == port.Id) && Node.Equals(port.Node);
    }

    /// <summary>
    /// Get internal hash code
    /// </summary>
    public override int GetHashCode()
    {
      return Node.GetHashCode() * Id;
    }

    /// <summary>
    /// Compare this instance to the object.
    /// Negative value means that the value is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(object obj)
    {
      IErlObject o = obj as IErlObject;
      return o == null ? -1 : CompareTo(o);
    }

    /// <summary>
    /// Compare this instance to the IErlObject.
    /// Negative value means that the value is less than obj, positive - greater than the obj
    /// </summary>
    public int CompareTo(IErlObject obj)
    {
      if (!(obj is ErlPort))
        return TypeOrder < obj.TypeOrder ? -1 : 1;

      var rhs = (ErlPort)obj;
      int n = Node.CompareTo(rhs.Node);
      if (n != 0) return n;
      n = Id.CompareTo(rhs.Id);
      if (n != 0) return n;
      return Creation.CompareTo(rhs.Creation);
    }

    /// <summary>
    /// Clone an instance of the object (non-scalar immutable objects are copied by reference)
    /// </summary>
    public IErlObject Clone() { return this; }  // Scalar Value is immutable
    object ICloneable.Clone() { return Clone(); }

    public bool Subst(ref IErlObject term, ErlVarBind binding)
    {
      return false;
    }

    /// <summary>
    /// Execute fun for every nested term
    /// </summary>
    public TAccumulate Visit<TAccumulate>(TAccumulate acc, Func<TAccumulate, IErlObject, TAccumulate> fun)
    {
      return fun(acc, this);
    }

    /// <summary>
    /// Perform pattern match on this Erlang term returning null if match fails
    /// or a dictionary of matched variables bound in the pattern
    /// </summary>
    public ErlVarBind Match(IErlObject pattern)
    {
      return pattern is ErlVar
          ? pattern.Match(this)
          : Equals(pattern) ? new ErlVarBind() : null;
    }

    /// <summary>
    /// Perform pattern match on this Erlang term, storing matched variables
    /// found in the pattern into the binding.
    /// </summary>
    public bool Match(IErlObject pattern, ErlVarBind binding)
    {
      return pattern is ErlVar ? pattern.Match(this, binding) : Equals(pattern);
    }

    /// <summary>
    /// Perform pattern match on this Erlang term without binding any variables
    /// </summary>
    public bool Matches(IErlObject pattern)
    {
      return pattern is ErlVar ? pattern.Matches(this) : Equals(pattern);
    }

  #endregion
  }
}