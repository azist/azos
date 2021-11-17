/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Serialization.JSON;

namespace Azos.IO
{
  /// <summary>
  /// Writes primitives to stream
  /// </summary>
  public abstract class WritingStreamer : Streamer
  {
    #region .ctor
    protected WritingStreamer(Encoding encoding=null) : base(encoding)
    {
    }
    #endregion


    #region Public
    public abstract void Flush();

    public abstract void Write(bool value);
    public abstract void Write(bool? value);


    public void Write(byte value)
    {
      m_Stream.WriteByte(value);
    }

    public abstract void Write(byte? value);



    public abstract void Write(byte[] buffer);
    public abstract void Write(int[] value);
    public abstract void Write(long[] value);
    public abstract void Write(double[] value);
    public abstract void Write(float[] value);
    public abstract void Write(decimal[] value);


    public abstract void Write(char ch);
    public abstract void Write(char? value);


    public abstract void Write(char[] buffer);
    public abstract void Write(string[] array);

    public abstract void Write(decimal value);
    public abstract void Write(decimal? value);



    public abstract void Write(double value);
    public abstract void Write(double? value);

    public abstract void Write(float value);
    public abstract void Write(float? value);

    public abstract void Write(int value);
    public abstract void Write(int? value);

    public abstract void Write(long value);
    public abstract void Write(long? value);

    public abstract void Write(sbyte value);
    public abstract void Write(sbyte? value);

    public abstract void Write(short value);
    public abstract void Write(short? value);

    public abstract void Write(string value);

    public abstract void Write(uint value);
    public abstract void Write(uint? value);

    public abstract void Write(ulong value);
    public abstract void Write(ulong? value);

    public abstract void Write(ushort value);
    public abstract void Write(ushort? value);

    public abstract void Write(MetaHandle value);
    public abstract void Write(MetaHandle? value);

    public abstract void Write(DateTime value);
    public abstract void Write(DateTime? value);

    public abstract void Write(DateTimeOffset value);
    public abstract void Write(DateTimeOffset? value);

    public abstract void Write(TimeSpan value);
    public abstract void Write(TimeSpan? value);

    public abstract void Write(Guid value);
    public abstract void Write(Guid? value);

    public abstract void Write(Data.GDID value);
    public abstract void Write(Data.GDID? value);

    public abstract void Write(Glue.Protocol.TypeSpec spec);
    public abstract void Write(Glue.Protocol.MethodSpec spec);

    public abstract void Write(FID value);
    public abstract void Write(FID? value);

    public abstract void Write(Pile.PilePointer value);
    public abstract void Write(Pile.PilePointer? value);

    public abstract void Write(VarIntStr value);
    public abstract void Write(VarIntStr? value);

    public abstract void Write(NLSMap map);
    public abstract void Write(NLSMap? map);

    public abstract void Write(Financial.Amount value);
    public abstract void Write(Financial.Amount? value);

    public abstract void Write(Collections.StringMap map);

    public abstract void Write(Atom value);
    public abstract void Write(Atom? value);

    #endregion
  }
}
