/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.IO;
using Azos.Serialization.JSON;
using Azos.Collections;

namespace Azos.Tests.Unit.IO
{
    [Runnable(TRUN.BASE)]
    public class SlimFormatReadWrite
    {


        [Run("cnt=10")]
        [Run("cnt=20")]
        [Run("cnt=250")]
        [Run("cnt=512")]
        [Run("cnt=1024")]
        [Run("cnt=16000")]
        [Run("cnt=64000")]
        [Run("cnt=95000")]
        [Run("cnt=98000")]
        [Run("cnt=128000")]
        [Run("cnt=512000")]
        public void StringOfWideChars(int cnt)
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);

            var builder = new StringBuilder();
            for(var i=0; i<cnt; i++)
              builder.Append( i%3==0 ?  '久' : i%7==0 ? 'ﺉ' :  i%16==0 ? 'Ж' : '1' );
            var original = builder.ToString();

            Console.WriteLine(original.TakeFirstChars(125));

            w.Write( original );

            ms.Seek(0, SeekOrigin.Begin);


            var got = r.ReadString();

            Aver.AreEqual(original, got);
          }
        }


        [Run]
        public void PositiveInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);

            w.Write(0);  //1 byte
            w.Write(127); //2 byte
            w.Write(128); //2 bytes
            w.Write(int.MaxValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Aver.AreEqual(0, r.ReadInt());
             Aver.AreEqual(127, r.ReadInt());
             Aver.AreEqual(128, r.ReadInt());
             Aver.AreEqual(int.MaxValue, r.ReadInt());

             Aver.AreEqual(10, ms.Length);
          }
        }

        [Run]
        public void NegativeInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);

            w.Write(0);  //1 byte
            w.Write(-127); //2 byte
            w.Write(-128); //2 bytes
            w.Write(int.MinValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Aver.AreEqual(0, r.ReadInt());
             Aver.AreEqual(-127, r.ReadInt());
             Aver.AreEqual(-128, r.ReadInt());
             Aver.AreEqual(int.MinValue, r.ReadInt());

             Aver.AreEqual(10, ms.Length);
          }
        }

        [Run]
        public void NullableInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

             r.BindStream(ms);
            w.BindStream(ms);

            w.Write((int?)18);  //2 byte
            w.Write((int?)-127); //3 byte
            w.Write((int?)null); //1 bytes
            w.Write((int?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Aver.AreEqual(18, r.ReadNullableInt().Value);
             Aver.AreEqual(-127, r.ReadNullableInt().Value);
             Aver.AreEqual(false, r.ReadNullableInt().HasValue);
             Aver.AreEqual(255, r.ReadNullableInt().Value);

             Aver.AreEqual(9, ms.Length);
          }
        }




                                    [Run]
                                    public void PositiveLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                          r.BindStream(ms);
                                        w.BindStream(ms);

                                        w.Write(0L);  //1 byte
                                        w.Write(127L); //2 byte
                                        w.Write(128L); //2 bytes
                                        w.Write(long.MaxValue);//10 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Aver.AreEqual(0L, r.ReadLong());
                                         Aver.AreEqual(127L, r.ReadLong());
                                         Aver.AreEqual(128L, r.ReadLong());
                                         Aver.AreEqual(long.MaxValue, r.ReadLong());

                                         Aver.AreEqual(15, ms.Length);
                                      }
                                    }

                                    [Run]
                                    public void NegativeLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                         r.BindStream(ms);
                                        w.BindStream(ms);

                                        w.Write(0L);  //1 byte
                                        w.Write(-127L); //2 byte
                                        w.Write(-128L); //2 bytes
                                        w.Write(long.MinValue);//10 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Aver.AreEqual(0L, r.ReadLong());
                                         Aver.AreEqual(-127L, r.ReadLong());
                                         Aver.AreEqual(-128L, r.ReadLong());
                                         Aver.AreEqual(long.MinValue, r.ReadLong());

                                         Aver.AreEqual(15, ms.Length);
                                      }
                                    }

                                    [Run]
                                    public void NullableLong()
                                    {
                                      using(var ms = new MemoryStream())
                                      {
                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                          r.BindStream(ms);
                                        w.BindStream(ms);

                                        w.Write((long?)18);  //2 byte
                                        w.Write((long?)-127); //3 byte
                                        w.Write((long?)null); //1 bytes
                                        w.Write((long?)long.MaxValue);//11 bytes

                                        ms.Seek(0, SeekOrigin.Begin);

                                         Aver.AreEqual(18, r.ReadNullableLong().Value);
                                         Aver.AreEqual(-127, r.ReadNullableLong().Value);
                                         Aver.AreEqual(false, r.ReadNullableLong().HasValue);
                                         Aver.AreEqual(long.MaxValue, r.ReadNullableLong().Value);

                                         Aver.AreEqual(17, ms.Length);
                                      }
                                    }


        [Run]
        public void UInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((uint)0);  //1 byte
            w.Write((uint)127); //1 byte
            w.Write((uint)128); //2 bytes
            w.Write(uint.MaxValue);//5 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Aver.IsTrue(0 == r.ReadUInt());
             Aver.IsTrue(127 == r.ReadUInt());
             Aver.IsTrue(128 == r.ReadUInt());
             Aver.AreEqual(uint.MaxValue, r.ReadUInt());

             Aver.AreEqual(9, ms.Length);
          }
        }

        [Run]
        public void NullableUInt()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((uint?)18);  //2 byte
            w.Write((uint?)127); //2 byte
            w.Write((uint?)null); //1 bytes
            w.Write((uint?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

             Aver.IsTrue(18 == r.ReadNullableUInt().Value);
             Aver.IsTrue(127 == r.ReadNullableUInt().Value);
             Aver.AreEqual(false, r.ReadNullableUInt().HasValue);
             Aver.IsTrue(255 == r.ReadNullableUInt().Value);

             Aver.AreEqual(8, ms.Length);
          }
        }


                        [Run]
                        public void ULong()
                        {
                          using(var ms = new MemoryStream())
                          {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                              r.BindStream(ms);
                            w.BindStream(ms);

                            w.Write((ulong)0);  //1 byte
                            w.Write((ulong)127); //1 byte
                            w.Write((ulong)128); //2 bytes
                            w.Write(ulong.MaxValue);//10 bytes

                            ms.Seek(0, SeekOrigin.Begin);

                             Aver.IsTrue(  0 == r.ReadULong());
                             Aver.IsTrue(127 == r.ReadULong());
                             Aver.IsTrue(128 == r.ReadULong());
                             Aver.AreEqual(ulong.MaxValue, r.ReadULong());

                             Aver.AreEqual(14, ms.Length);
                          }
                        }

                        [Run]
                        public void NullableULong()
                        {
                          using(var ms = new MemoryStream())
                          {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                              r.BindStream(ms);
                            w.BindStream(ms);

                            w.Write((ulong?)18);  //2 byte
                            w.Write((ulong?)127); //2 byte
                            w.Write((ulong?)null); //1 bytes
                            w.Write((ulong?)255);//3 bytes

                            ms.Seek(0, SeekOrigin.Begin);

                             Aver.IsTrue( 18 == r.ReadNullableULong().Value);
                             Aver.IsTrue(127 == r.ReadNullableULong().Value);
                             Aver.AreEqual(false, r.ReadNullableULong().HasValue);
                             Aver.IsTrue(255 == r.ReadNullableULong().Value);

                             Aver.AreEqual(8, ms.Length);
                          }
                        }

        [Run]
        public void UShort()
        {
            using(var ms = new MemoryStream())
            {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((ushort)0);  //1 byte
            w.Write((ushort)127); //1 byte
            w.Write((ushort)128); //2 bytes
            w.Write(ushort.MaxValue);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(0, r.ReadUShort());
                Aver.AreEqual(127, r.ReadUShort());
                Aver.AreEqual(128, r.ReadUShort());
                Aver.AreEqual(ushort.MaxValue, r.ReadUShort());

                Aver.AreEqual(7, ms.Length);
            }
        }

        [Run]
        public void NullableUShort()
        {
            using(var ms = new MemoryStream())
            {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            w.Write((ushort?)18);  //2 byte
            w.Write((ushort?)127); //2 byte
            w.Write((ushort?)null); //1 bytes
            w.Write((ushort?)255);//3 bytes

            ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(18, r.ReadNullableUShort().Value);
                Aver.AreEqual(127, r.ReadNullableUShort().Value);
                Aver.AreEqual(false, r.ReadNullableUShort().HasValue);
                Aver.AreEqual(255, r.ReadNullableUShort().Value);

                Aver.AreEqual(8, ms.Length);
            }
        }



                                                                    [Run]
                                                                    public void PositiveShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                                                        r.BindStream(ms);
                                                                      w.BindStream(ms);

                                                                        w.Write((short)0);  //1 byte
                                                                        w.Write((short)127); //2 byte
                                                                        w.Write((short)128); //2 bytes
                                                                        w.Write(short.MaxValue);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Aver.AreEqual(0, r.ReadShort());
                                                                         Aver.AreEqual(127, r.ReadShort());
                                                                         Aver.AreEqual(128, r.ReadShort());
                                                                         Aver.AreEqual(short.MaxValue, r.ReadShort());

                                                                         Aver.AreEqual(8, ms.Length);
                                                                      }
                                                                    }

                                                                    [Run]
                                                                    public void NegativeShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                                                          r.BindStream(ms);
                                                                        w.BindStream(ms);

                                                                        w.Write((short)0);  //1 byte
                                                                        w.Write((short)-127); //2 byte
                                                                        w.Write((short)-128); //2 bytes
                                                                        w.Write(short.MinValue);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Aver.AreEqual(0, r.ReadShort());
                                                                         Aver.AreEqual(-127, r.ReadShort());
                                                                         Aver.AreEqual(-128, r.ReadShort());
                                                                         Aver.AreEqual(short.MinValue, r.ReadShort());

                                                                         Aver.AreEqual(8, ms.Length);
                                                                      }
                                                                    }

                                                                    [Run]
                                                                    public void NullableShort()
                                                                    {
                                                                      using(var ms = new MemoryStream())
                                                                      {
                                                                        var r = SlimFormat.Instance.MakeReadingStreamer();
                                                                        var w = SlimFormat.Instance.MakeWritingStreamer();

                                                                            r.BindStream(ms);
                                                                          w.BindStream(ms);

                                                                        w.Write((short?)18);  //2 byte
                                                                        w.Write((short?)-127); //3 byte
                                                                        w.Write((short?)null); //1 bytes
                                                                        w.Write((short?)255);//3 bytes

                                                                        ms.Seek(0, SeekOrigin.Begin);

                                                                         Aver.AreEqual(18, r.ReadNullableShort().Value);
                                                                         Aver.AreEqual(-127, r.ReadNullableShort().Value);
                                                                         Aver.AreEqual(false, r.ReadNullableShort().HasValue);
                                                                         Aver.AreEqual(255, r.ReadNullableShort().Value);

                                                                         Aver.AreEqual(9, ms.Length);
                                                                      }
                                                                    }






        [Run]
        public void _DateTime()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var now = App.LocalizedTime;

                w.Write(now);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(now, r.ReadDateTime());
            }
        }

        [Run]
        public void NullableDateTime()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var now = App.LocalizedTime;

                w.Write((DateTime?)null);
                w.Write((DateTime?)now);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(false, r.ReadNullableDateTime().HasValue);
                Aver.AreEqual(now, r.ReadNullableDateTime().Value);
            }
        }



        [Run]
        public void _TimeSpan()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var ts = TimeSpan.FromMilliseconds(25467);

                w.Write(ts);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(ts, r.ReadTimeSpan());
            }
        }

        [Run]
        public void NullableTimeSpan()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                 r.BindStream(ms);
            w.BindStream(ms);

                var ts = TimeSpan.FromMilliseconds(25467);

                w.Write((TimeSpan?)null);
                w.Write((TimeSpan?)ts);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(false, r.ReadNullableTimeSpan().HasValue);
                Aver.AreEqual(ts, r.ReadNullableTimeSpan().Value);
            }
        }



                    [Run]
                    public void _Guid()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var guid = Guid.NewGuid();

                            w.Write(guid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(guid, r.ReadGuid());
                        }
                    }

                    [Run]
                    public void NullableGuid()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var guid = Guid.NewGuid();

                            w.Write((Guid?)null);
                            w.Write((Guid?)guid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullableGuid().HasValue);
                            Aver.AreEqual(guid, r.ReadNullableGuid().Value);
                        }
                    }


                    [Run]
                    public void _GDID_1()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new Azos.DataAccess.Distributed.GDID(5, 123);

                            w.Write(gdid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(gdid, r.ReadGDID());
                        }
                    }

                    [Run]
                    public void _GDID_2()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new Azos.DataAccess.Distributed.GDID(11, 0xffffffffffffffe0);

                            w.Write(gdid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(gdid, r.ReadGDID());
                        }
                    }

                    [Run]
                    public void NullableGDID_1()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new Azos.DataAccess.Distributed.GDID(0, 123);

                            w.Write((Azos.DataAccess.Distributed.GDID?)null);
                            w.Write((Azos.DataAccess.Distributed.GDID?)gdid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullableGDID().HasValue);
                            Aver.AreEqual(gdid, r.ReadNullableGDID().Value);
                        }
                    }

                    [Run]
                    public void NullableGDID_2()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                            var gdid = new Azos.DataAccess.Distributed.GDID(12, 0xffffffffffffffe0);

                            w.Write((Azos.DataAccess.Distributed.GDID?)null);
                            w.Write((Azos.DataAccess.Distributed.GDID?)gdid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullableGDID().HasValue);
                            Aver.AreEqual(gdid, r.ReadNullableGDID().Value);
                        }
                    }





        [Run]
        public void _Bool()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                w.Write(true);
                w.Write(false);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(true, r.ReadBool());
                Aver.AreEqual(false, r.ReadBool());
            }
        }

        [Run]
        public void NullableBool()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                w.Write((bool?)null);
                w.Write((bool?)true);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(false, r.ReadNullableBool().HasValue);
                Aver.AreEqual(true, r.ReadNullableBool().Value);
            }
        }


        [Run]
        public void _Byte()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();


              r.BindStream(ms);
            w.BindStream(ms);

                w.Write((byte)2);
                w.Write((byte)0xff);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(2, r.ReadByte());
                Aver.AreEqual(0xff, r.ReadByte());
            }
        }

        [Run]
        public void NullableByte()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                w.Write((byte?)null);
                w.Write((byte?)19);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(false, r.ReadNullableByte().HasValue);
                Aver.AreEqual(19, r.ReadNullableByte().Value);
            }
        }



                    [Run]
                    public void _SByte()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                            w.Write((sbyte)-2);
                            w.Write((sbyte)0x4e);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(-2, r.ReadSByte());
                            Aver.AreEqual(0x4e, r.ReadSByte());
                        }
                    }

                    [Run]
                    public void NullableSByte()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();


              r.BindStream(ms);
            w.BindStream(ms);

                            w.Write((sbyte?)null);
                            w.Write((sbyte?)-19);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullableSByte().HasValue);
                            Aver.AreEqual(-19, r.ReadNullableSByte().Value);
                        }
                    }


            [Run]
            public void _Float()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();


              r.BindStream(ms);
            w.BindStream(ms);

                    w.Write((float)-2);
                    w.Write((float)0.1234);

                    ms.Seek(0, SeekOrigin.Begin);

                    Aver.AreEqual(-2f, r.ReadFloat());
                    Aver.AreEqual(0.1234f, r.ReadFloat());
                }
            }

            [Run]
            public void NullableFloat()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                    w.Write((float?)null);
                    w.Write((float?)0.6789);

                    ms.Seek(0, SeekOrigin.Begin);

                    Aver.AreEqual(false, r.ReadNullableFloat().HasValue);
                    Aver.AreEqual(0.6789f, r.ReadNullableFloat().Value);
                }
            }


            [Run]
            public void _Double()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                    w.Write((double)-2);
                    w.Write((double)0.1234);

                    ms.Seek(0, SeekOrigin.Begin);

                    Aver.AreEqual(-2, r.ReadDouble());
                    Aver.AreEqual(0.1234, r.ReadDouble());
                }
            }

            [Run]
            public void NullableDouble()
            {
                using(var ms = new MemoryStream())
                {
                    var r = SlimFormat.Instance.MakeReadingStreamer();
                    var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                    w.Write((double?)null);
                    w.Write((double?)0.6789);

                    ms.Seek(0, SeekOrigin.Begin);

                    Aver.AreEqual(false, r.ReadNullableDouble().HasValue);
                    Aver.AreEqual(0.6789, r.ReadNullableDouble().Value);
                }
            }



                [Run]
                public void _Decimal()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                        w.Write(-2m);
                        w.Write(0.1234m);

                        w.Write(789123612637621332.2390m);
                        w.Write(-789123612637621332.2390m);

                        w.Write(23123123213789123612637621332.001237182738m);
                        w.Write(-23123123213789123612637621332.001237182738m);

                        w.Write(0.123123213789123612637621332001237182738m);
                        w.Write(-0.123123213789123612637621332001237182738m);

                        w.Write(0.0000000000000000000000000073m);
                        w.Write(-0.0000000000000000000000000073m);

                        ms.Seek(0, SeekOrigin.Begin);

                        Aver.AreEqual(-2m, r.ReadDecimal());
                        Aver.AreEqual(0.1234m, r.ReadDecimal());

                        Aver.AreEqual(789123612637621332.2390m, r.ReadDecimal());
                        Aver.AreEqual(-789123612637621332.2390m, r.ReadDecimal());

                        Aver.AreEqual(23123123213789123612637621332.001237182738m, r.ReadDecimal());
                        Aver.AreEqual(-23123123213789123612637621332.001237182738m, r.ReadDecimal());

                        Aver.AreEqual(0.123123213789123612637621332001237182738m, r.ReadDecimal());
                        Aver.AreEqual(-0.123123213789123612637621332001237182738m, r.ReadDecimal());

                        Aver.AreEqual(0.0000000000000000000000000073m, r.ReadDecimal());
                        Aver.AreEqual(-0.0000000000000000000000000073m, r.ReadDecimal());
                    }
                }

                [Run]
                public void NullableDecimal()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                        w.Write((decimal?)null);
                        w.Write((decimal?)0.6789);

                        ms.Seek(0, SeekOrigin.Begin);

                        Aver.AreEqual(false, r.ReadNullableDecimal().HasValue);
                        Aver.AreEqual(0.6789, (double)r.ReadNullableDecimal().Value);
                    }
                }



                           [Run]
                            public void _Char()
                            {
                                using(var ms = new MemoryStream())
                                {
                                    var r = SlimFormat.Instance.MakeReadingStreamer();
                                    var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                                    w.Write('a');
                                    w.Write('b');

                                    ms.Seek(0, SeekOrigin.Begin);

                                    Aver.AreEqual('a', r.ReadChar());
                                    Aver.AreEqual('b', r.ReadChar());
                                }
                            }

                            [Run]
                            public void NullableChar()
                            {
                                using(var ms = new MemoryStream())
                                {
                                    var r = SlimFormat.Instance.MakeReadingStreamer();
                                    var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                                    w.Write((char?)null);
                                    w.Write((char?)'Z');

                                    ms.Seek(0, SeekOrigin.Begin);

                                    Aver.AreEqual(false, r.ReadNullableChar().HasValue);
                                    Aver.AreEqual('Z', r.ReadNullableChar().Value);
                                }
                            }


                [Run]
                public void MetaHandle()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);


                        w.Write(new MetaHandle(1));// 1 byte
                        w.Write(new MetaHandle(0xffff));// 3 byte
                        w.Write(new MetaHandle(0xffff, new VarIntStr("0123456789")));// 3 byte + 2 len + 10 byte  = 19
                        ms.Seek(0, SeekOrigin.Begin);

                        Aver.IsTrue(1 == r.ReadMetaHandle().Handle);
                        Aver.IsTrue(0xffff == r.ReadMetaHandle().Handle);

                        Aver.AreEqual(19,  ms.Length);
                    }
                }

                [Run]
                public void NullableMetaHandle()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

                        w.Write((MetaHandle?)null); //1 byte
                        w.Write((MetaHandle?)new MetaHandle(12, new VarIntStr("It works")));// 1(nnul) + 1(12) + 2(strlen) + 8(It works) = 12 bytes
                        ms.Seek(0, SeekOrigin.Begin);

                        Aver.AreEqual(false, r.ReadNullableMetaHandle().HasValue);

                        var mh = r.ReadNullableMetaHandle().Value;
                        Aver.IsTrue(12 == mh.Handle);
                        Aver.AreEqual("It works", mh.Metadata.Value.StringValue);

                        Aver.AreEqual(13,  ms.Length);
                    }
                }



        [Run]
        public void StringArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            var arr1 = new string[10];
            var arr2 = new string[3];
            string[] arr3 = null;

            arr1[0] = "AAA";
            arr2[2] = "zzz";

            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadStringArray();
            var r2 = r.ReadStringArray();
            var r3 = r.ReadStringArray();

             Aver.AreEqual("AAA", r1[0]);
             Aver.AreEqual("zzz", r2[2]);
             Aver.AreEqual(10, r1.Length);
             Aver.AreEqual(3, r2.Length);
             Aver.IsNull(r3);
          }
        }


        [Run]
        public void ByteArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

              r.BindStream(ms);
            w.BindStream(ms);

            var arr1 = new byte[]{12,56,11,254};
            var arr2 = new byte[]{};
            byte[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadByteArray();
            var r2 = r.ReadByteArray();
            var r3 = r.ReadByteArray();

             Aver.AreEqual(56, r1[1]);
             Aver.AreEqual(254, r1[3]);
             Aver.AreEqual(arr1.Length, r1.Length);
             Aver.AreEqual(arr2.Length, r2.Length);
             Aver.IsNull(r3);
          }
        }

        [Run]
        public void CharArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);


            var arr1 = new char[]{'a','b','c','z'};
            var arr2 = new char[]{};
            char[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadByteArray();
            var r2 = r.ReadByteArray();
            var r3 = r.ReadByteArray();

             Aver.AreEqual((byte)'b', r1[1]);
             Aver.AreEqual((byte)'z', r1[3]);
             Aver.AreEqual(arr1.Length, r1.Length);
             Aver.AreEqual(arr2.Length, r2.Length);
             Aver.IsNull(r3);
          }
        }


        [Run]
        public void IntArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);


            var arr1 = new int[]{1, 280, 4564};
            var arr2 = new int[]{};
            int[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadIntArray();
            var r2 = r.ReadIntArray();
            var r3 = r.ReadIntArray();

            Aver.AreEqual(1, r1[0]);
            Aver.AreEqual(4564, r1[2]);
            Aver.AreEqual(arr1.Length, r1.Length);
            Aver.AreEqual(arr2.Length, r2.Length);
            Aver.IsNull(r3);
          }
        }

        [Run]
        public void FloatArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);


            var arr1 = new float[]{1f, 280f, 4564.34f};
            var arr2 = new float[]{};
            float[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadFloatArray();
            var r2 = r.ReadFloatArray();
            var r3 = r.ReadFloatArray();

            Aver.AreEqual(1f, r1[0]);
            Aver.AreEqual(4564.34f, r1[2]);
            Aver.AreEqual(arr1.Length, r1.Length);
            Aver.AreEqual(arr2.Length, r2.Length);
            Aver.IsNull(r3);
          }
        }

        [Run]
        public void DoubleArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);


            var arr1 = new double[]{1d, 280d, 4564.34d};
            var arr2 = new double[]{};
            float[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadDoubleArray();
            var r2 = r.ReadDoubleArray();
            var r3 = r.ReadDoubleArray();

            Aver.AreEqual(1d, r1[0]);
            Aver.AreEqual(4564.34d, r1[2]);
            Aver.AreEqual(arr1.Length, r1.Length);
            Aver.AreEqual(arr2.Length, r2.Length);
            Aver.IsNull(r3);
          }
        }

        [Run]
        public void DecimalArray()
        {
          using(var ms = new MemoryStream())
          {
            var r = SlimFormat.Instance.MakeReadingStreamer();
            var w = SlimFormat.Instance.MakeWritingStreamer();

            r.BindStream(ms);
            w.BindStream(ms);


            var arr1 = new decimal[]{1m, 280m, 4564.34m};
            var arr2 = new decimal[]{};
            float[] arr3 = null;


            w.Write(arr1);
            w.Write(arr2);
            w.Write(arr3);

            ms.Seek(0, SeekOrigin.Begin);

            var r1 = r.ReadDecimalArray();
            var r2 = r.ReadDecimalArray();
            var r3 = r.ReadDecimalArray();

            Aver.AreEqual(1m, r1[0]);
            Aver.AreEqual(4564.34m, r1[2]);
            Aver.AreEqual(arr1.Length, r1.Length);
            Aver.AreEqual(arr2.Length, r2.Length);
            Aver.IsNull(r3);
          }
        }


                    [Run]
                    public void TypeSpec()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var spec = new Azos.Glue.Protocol.TypeSpec(typeof(System.Collections.Generic.List<int>));

                            w.Write(spec);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreObjectsEqual(spec, r.ReadTypeSpec());
                        }
                    }

                    [Run]
                    public void MethodSpec()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var spec = new Azos.Glue.Protocol.MethodSpec(typeof(System.Collections.Generic.List<int>).GetMethod("Clear"));

                            w.Write(spec);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreObjectsEqual(spec, r.ReadMethodSpec());
                        }
                    }

                    [Run]
                    public void FID()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var fid = Azos.FID.Generate();

                            w.Write(fid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(fid, r.ReadFID());
                        }
                    }

                    [Run]
                    public void _FID()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var fid = Azos.FID.Generate();

                            w.Write((Azos.FID?)null);
                            w.Write(fid);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullableFID().HasValue);
                            Aver.AreEqual(fid, r.ReadFID());
                        }
                    }


                    [Run]
                    public void PilePointer()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var pp = new Azos.Apps.Pile.PilePointer(10,20,30);

                            w.Write(pp);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(pp, r.ReadPilePointer());
                        }
                    }

                    [Run]
                    public void _PilePointer()
                    {
                        using(var ms = new MemoryStream())
                        {
                            var r = SlimFormat.Instance.MakeReadingStreamer();
                            var w = SlimFormat.Instance.MakeWritingStreamer();

                            r.BindStream(ms);
                            w.BindStream(ms);

                            var pp = new Azos.Apps.Pile.PilePointer(10,20,30);

                            w.Write((Azos.Apps.Pile.PilePointer?)null);
                            w.Write(pp);

                            ms.Seek(0, SeekOrigin.Begin);

                            Aver.AreEqual(false, r.ReadNullablePilePointer().HasValue);
                            Aver.AreEqual(pp, r.ReadPilePointer());
                        }
                    }


        [Run]
        public void NLSMap()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                r.BindStream(ms);
                w.BindStream(ms);

                var map = new NLSMap(
        @"nls{
            eng{n='Cream>Serum>Day' d='Daily Serum Care'}
            rus{n='Крем>Серум>Дневной' d='Дневной Уход Серум'}
            deu{n='Ein Drek'}
          }".AsLaconicConfig(handling: ConvertErrorHandling.Throw));

                w.Write(map);

                ms.Position = 0;

                var map2 = r.ReadNLSMap();

                Aver.IsNotNull(map2);
                Aver.AreEqual(3, map2.Count);
                Aver.AreEqual("Cream>Serum>Day", map2["ENG"].Name);
                Aver.AreEqual("Крем>Серум>Дневной", map2["rus"].Name);
                Aver.AreEqual("Ein Drek", map2["dEu"].Name);

                Aver.AreEqual("Daily Serum Care", map2["ENG"].Description);
                Aver.AreEqual("Дневной Уход Серум", map2["rus"].Description);
                Aver.AreEqual(null, map2["dEu"].Description);

                ms.Position = 0;
                NLSMap nullmap = new NLSMap();
                w.Write(nullmap);
                ms.Position = 0;

                var map3 = r.ReadNLSMap();
                Aver.IsNull( map3.m_Data);
                Aver.AreEqual(0 , map3.Count);


            }
        }

                [Run]
                public void _NLSMap()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

                        r.BindStream(ms);
                        w.BindStream(ms);

                        NLSMap? map = new NLSMap(
                @"nls{
                    eng{n='Cream>Serum>Day' d='Daily Serum Care'}
                    rus{n='Крем>Серум>Дневной' d='Дневной Уход Серум'}
                    deu{n='Ein Drek'}
                  }".AsLaconicConfig(handling: ConvertErrorHandling.Throw));

                        w.Write(map);

                        ms.Position = 0;

                        var map2 = r.ReadNullableNLSMap();

                        Aver.IsTrue(map2.HasValue);
                        Aver.AreEqual(3, map2.Value.Count);
                        Aver.AreEqual("Cream>Serum>Day", map2.Value["ENG"].Name);
                        Aver.AreEqual("Крем>Серум>Дневной", map2.Value["rus"].Name);
                        Aver.AreEqual("Ein Drek", map2.Value["dEu"].Name);


                        ms.Position = 0;
                        NLSMap? nullmap = null;
                        w.Write(nullmap);
                        ms.Position = 0;

                        var map3 = r.ReadNullableNLSMap();
                        Aver.IsFalse( map3.HasValue );
                    }
                }


                [Run]
                public void StringMap()
                {
                    using(var ms = new MemoryStream())
                    {
                        var r = SlimFormat.Instance.MakeReadingStreamer();
                        var w = SlimFormat.Instance.MakeWritingStreamer();

                        r.BindStream(ms);
                        w.BindStream(ms);

                        var mapS = new StringMap(true)
                        {
                           {"a", "Alex"},
                           {"b","Boris"}
                        };

                        var mapI = new StringMap(false)
                        {
                           {"a", "Alex"},
                           {"b","Boris"},
                           {"c","Chuck"}
                        };



                        w.Write(mapS);
                        w.Write(mapI);

                        ms.Seek(0, SeekOrigin.Begin);


                        var mapS2 = r.ReadStringMap();
                        Aver.IsTrue(mapS2.CaseSensitive);
                        Aver.AreEqual(2, mapS2.Count);
                        Aver.AreEqual("Alex", mapS2["a"]);
                        Aver.AreEqual("Boris", mapS2["b"]);

                        var mapI2 = r.ReadStringMap();
                        Aver.IsFalse(mapI2.CaseSensitive);
                        Aver.AreEqual(3, mapI2.Count);
                        Aver.AreEqual("Alex", mapI2["a"]);
                        Aver.AreEqual("Boris", mapI2["b"]);
                        Aver.AreEqual("Chuck", mapI2["c"]);
                    }
                }


        [Run]
        public void Amount()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                r.BindStream(ms);
                w.BindStream(ms);

                var a1 = new Azos.Financial.Amount("usd", 23672209.243m);

                w.Write(a1);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.AreEqual(a1, r.ReadAmount());
            }
        }

        [Run]
        public void NullableAmount()
        {
            using(var ms = new MemoryStream())
            {
                var r = SlimFormat.Instance.MakeReadingStreamer();
                var w = SlimFormat.Instance.MakeWritingStreamer();

                r.BindStream(ms);
                w.BindStream(ms);

                var a1 = new Azos.Financial.Amount("usd", 23672209.243m);

                w.Write((Azos.Financial.Amount?)null);
                w.Write((Azos.Financial.Amount?)a1);

                ms.Seek(0, SeekOrigin.Begin);

                Aver.IsFalse( r.ReadNullableAmount().HasValue );
                Aver.AreEqual(a1, r.ReadNullableAmount());
            }
        }

    }
}
