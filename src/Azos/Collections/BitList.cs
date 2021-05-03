/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.Collections;

namespace Azos.Collections
{
  /// <summary>
  /// Provides bit array with automatic resizing
  /// </summary>
  public class BitList : IEquatable<BitList>
  {
    #region .ctor

    public BitList(int initSize = 0)
    {
      m_BitArray = new BitArray(initSize);
      m_Size = initSize;
    }

    #endregion

    #region Pvt/Prot/Int Fields

    private BitArray m_BitArray;
    private int m_Size;

    #endregion

    #region Properties

    public int Size { get { return m_Size; } }

    public int ByteSize { get { return (Size + 7) >> 3; } }

    public bool this[int i]
    {
      get { return m_BitArray[i]; }
      set { m_BitArray[i] = value; }
    }

    #endregion

    #region Public

    public void AppendBit(bool bit)
    {
      ensureCapacity(++m_Size);
      m_BitArray[m_Size - 1] = bit;
    }

    public void AppendBits(int value, int numBits)
    {
      if (numBits < 0 || numBits >= 8 * sizeof(int))
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".AppendBits(numBits>=0|<8*sizeof(int))");

      ensureCapacity(m_Size + numBits);
      for (int leftBitIndex = numBits; leftBitIndex > 0; leftBitIndex--)
      {
        int bitValue = (value >> (leftBitIndex - 1)) & 0x01;
        AppendBit(bitValue == 1);
      }
    }

    public void AppendBitList(BitList other)
    {
      ensureCapacity(m_Size + other.Size);
      for (int i = 0; i < other.m_Size; i++)
        AppendBit(other.m_BitArray[i]);
    }

    public void Xor(BitList other)
    {
      if (m_BitArray.Length != other.m_BitArray.Length)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Xor(other.Length!=this.Length)");

      m_BitArray.Xor(other.m_BitArray);
    }

    public void GetBytes(byte[] buf, int bitOffset, int offset, int numBytes)
    {
      for (int i = 0; i < numBytes; i++)
      {
        int theByte = 0;
        for (int j = 0; j < 8; j++)
        {
          if (this[bitOffset])
          {
            theByte |= 1 << (7 - j);
          }
          bitOffset++;
        }
        buf[offset + i] = (byte)theByte;
      }
    }

    public override string ToString()
    {
      StringBuilder b = new StringBuilder();

      for (int i = 0; i < m_Size; i++)
      {
        if (i > 0 && i % 8 == 0)
          b.Append(' ');

        bool elem = m_BitArray[i];

        b.Append(elem ? '1' : '0');
      }

      return b.ToString();
    }

    public override int GetHashCode()
    {
      return m_BitArray.GetHashCode();
    }

    public bool Equals(BitList otherObj)
    {
      return m_BitArray.Equals(otherObj.m_BitArray);
    }

    #endregion

    #region .pvt. impl.

    private void ensureCapacity(int size)
    {
      if (size >= m_BitArray.Length)
      {
        int newLength = 1 << (int)Math.Ceiling(Math.Log(size, 2));
        m_BitArray.Length = newLength;
      }
    }

    #endregion

  }//class
}
