
using System;

namespace Azos.Pile
{
  /// <summary>
  /// Represents a PilePointer scoped by PileID. PileID uniquelly identifies the pile on the pointed-to machine.
  /// Piled is used for deferred load/deserialization from pile. This is a useful optimization technique for tree serialization,
  /// on deserialization Piled is returned, without having to deserialize the whole sub-graph of objects.
  /// Upon the first access to Pile.Value the Get from pile takes place transparently.
  /// When writing to pile, the Piled instance is written as a separate Pile entity with a distinct pointer
  /// </summary>
  public struct Piled<T> : IEquatable<Piled<T>>
  {
    /// <summary>
    /// Returns a -1:-1 non-valid pointer (either local or distributed)
    /// </summary>
    public static Piled<T> Invalid{ get{ return new Piled<T>(-1, PilePointer.Invalid);} }

    /// <summary>
    /// Creates distributed pointer
    /// </summary>
    public Piled(int pileId, PilePointer ptr)
    {
      PileID = pileId;
      Pointer = ptr;
      m_ValueLoaded = false;
      m_Value = default(T);
    }

    /// <summary>
    /// PileID uniquely identifies Pile instance on this or remote machine (if Pointer is distributed)
    /// </summary>
    public readonly int PileID;

    /// <summary>
    /// Pointer within the pile identoified by PileID
    /// </summary>
    public readonly PilePointer Pointer;

    [NonSerialized]
    private bool m_ValueLoaded;
    private T m_Value;


    public T Value
    {
      get
      {
        if (!m_ValueLoaded)
        {
          var pile = PileInstances.Map(PileID);
          if (pile==null)
            throw new PileException(StringConsts.PILE_AV_BAD_PILEID_ERROR.Args(this));

          m_Value = (T)pile.Get(Pointer);
          m_ValueLoaded = true;
        }

        return m_Value;
      }

      set
      {
        m_Value = value;
        m_ValueLoaded = true;
      }
    }



    /// <summary>
    /// Returns true if PileID and Pointer hold values, however this does not mean that pointed-to data exists
    /// </summary>
    public bool Valid{ get{ return PileID > 0 && Pointer.Valid;}}


    public override int GetHashCode()
    {
      return Pointer.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is Piled<T>)
       return this.Equals((Piled<T>)obj);

      return false;
    }

    public bool Equals(Piled<T> other)
    {
      return (this.PileID == other.PileID) &&
             (this.Pointer == other.Pointer);
    }

    public override string ToString()
    {
      return Pointer.ToString() + "@" + PileID;
    }

    public static bool operator ==(Piled<T> l, Piled<T> r)
    {
      return l.Equals(r);
    }

    public static bool operator !=(Piled<T> l, Piled<T> r)
    {
      return !l.Equals(r);
    }
  }
}
