/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Azos.Serialization.BSON;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Financial;

namespace Azos.Tests.Unit.Serialization
{
  /// <summary>
  /// See BSON spec http://bsonspec.org/spec.html
  /// </summary>
  [Runnable(TRUN.BASE)]
  public class BSON
  {
    #region Serialization

    /// <summary>
    /// {} (empty document)
    /// </summary>
    [Run]
    public void WriteEmptyDocument()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 5); // ensure document length is 5 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(5))); // ensure content length is 1
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 5); // ensure whole document readed
      }
    }

    /// <summary>
    /// { pi: 3.14159265358979 }
    /// </summary>
    [Run]
    public void WriteSingleDouble()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONDoubleElement("pi", Math.PI));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 17); // ensure document length is 17 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(17))); // ensure content length is 17
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Double); // ensure element type is double 0x01
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(2), Encoding.UTF8.GetBytes("pi"))); // ensure element name is 'pi'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(Math.PI))); // ensure element value is Math.PI
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 17); // ensure whole document readed
      }
    }

    /// <summary>
    /// { greetings: "Hello World!" }
    /// </summary>
    [Run]
    public void WriteSingleString()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("greetings", "Hello World!"));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 33); // ensure document length is 33 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(33))); // ensure content length is 33
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String); // ensure element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(9), Encoding.UTF8.GetBytes("greetings"))); // ensure element name is 'greetings'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(13))); // ensure string content length is 13
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(12), Encoding.UTF8.GetBytes("Hello World!"))); // ensure element value is 'Hello World!'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string value terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 33); // ensure whole document readed
      }
    }

    /// <summary>
    /// { lucky: 7 }
    /// </summary>
    [Run]
    public void WriteSingleInt32()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt32Element("lucky", 7));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 16); // ensure document length is 16 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(16))); // ensure content length is 16
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32); // ensure element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("lucky"))); // ensure element name is 'lucky'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(7))); // ensure element value is 7
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 16); // ensure whole document readed
      }
    }

    /// <summary>
    /// { solarSystemDiameter: 10000000000000 }
    /// </summary>
    [Run]
    public void WriteSingleInt64()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt64Element("solarSystemDiameter", 10000000000000));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 34); // ensure document length is 34 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(34))); // ensure content length is 34
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64); // ensure element type is int64 0x12
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(19), Encoding.UTF8.GetBytes("solarSystemDiameter"))); // ensure element name is 'solarSystemDiameter'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(10000000000000))); // ensure element value is 10000000000000
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'fruits': ['apple, 'orange', 'plum'] } --> { 'fruits': { '0': 'apple', '1': 'orange', '2': 'plum' } }
    /// </summary>
    [Run]
    public void WriteStringArray()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new[] { new BSONStringElement("apple"), new BSONStringElement("orange"), new BSONStringElement("plum") };
        root.Set(new BSONArrayElement("fruits", array));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 57); // ensure document length is 57 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(57)));        // document's content length is 57
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                    // element type is array 0x04
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("fruits"))); // element name is 'fruits'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                     // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(44)));        // array's content length is 44

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);               // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0")));    // element name is '0'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(6)));       // string content length is 6
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("apple"))); // string content length is 'apple'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                   // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1")));      // element name is '1'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                     // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(7)));         // string content length is 7
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("orange"))); // string content length is 'orange'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                     // last byte is terminator 0x00

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2")));     // element name is '2'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(5)));        // string content length is 5
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("plum")));   // string content length is 'plum'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                  // last byte is terminator 0x00

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 57); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of int32
    /// { 'years': [1963, 1984, 2015] } --> { 'years': { '0': 1963, '1': 1984, '2': 2015 } }
    /// </summary>
    [Run]
    public void WriteInt32Array()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new[] { new BSONInt32Element(1963), new BSONInt32Element(1984), new BSONInt32Element(2015) };
        root.Set(new BSONArrayElement("years", array));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 38); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(38)));       // document's content length is 38
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                 // element type is array 0x04
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("years"))); // element name is 'years'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                  // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(26)));       // array's content length is 26

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0"))); // element name is '0'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(1963))); // value is 1963

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1"))); // element name is '1'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(1984))); // value is 1984

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2"))); // element name is '2'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                              // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(2015))); // value is 2015

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'stuff': ['apple, 3, 2.14] } --> { 'stuff': { '0': 'apple', '1': 3, '2': 2.14 } }
    /// </summary>
    [Run]
    public void WriteStringInt32DoubleMixedArray()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var array = new BSONElement[] { new BSONStringElement("apple"), new BSONInt32Element(3), new BSONDoubleElement(2.14D) };
        root.Set(new BSONArrayElement("stuff", array));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 48); // ensure document length is 48 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(48)));       // document's content length is 48
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Array);                   // element type is array 0x04
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("stuff"))); // element name is 'stuff'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                    // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(36)));       // array's content length is 36

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                  // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("0")));     // element name is '0'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                    // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(6)));        // string content length is 6
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("apple"))); // string content length is 'apple'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                    // last byte is terminator 0x00

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);               // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("1"))); // element name is '1'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(3)));    // value is 3

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Double);               // element type is double 0x01
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("2")));  // element name is '2'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                 // last byte is terminator 0x00
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(2.14D))); // value is 2.14

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 48); // ensure whole document readed
      }
    }

    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    [Run]
    public void WriteStringAndInt32Pair()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("name", "Gagarin"));
        root.Set(new BSONInt32Element("birth", 1934));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 34); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(34))); // ensure content length is 34
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String); // ensure element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("name"))); // ensure element name is 'name'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(8))); // ensure string content length is 8
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(7), Encoding.UTF8.GetBytes("Gagarin")));  // ensure element value is 'Gagarin'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32); // ensure element type is int 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("birth"))); // ensure element name is 'birth'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(1934))); // ensure element value is int 1934
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// { nested: { capital: "Moscow" } }
    /// </summary>
    [Run]
    public void WriteNestedDocument()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var nested = new BSONDocument();
        nested.Set(new BSONStringElement("capital", "Moscow"));
        root.Set(new BSONDocumentElement("nested", nested));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 38); // ensure document length is 38 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(sizeof(int)), BitConverter.GetBytes(38))); // content length is 38
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Document);                    // element type is document 0x03
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("nested")));    // element name is 'nested'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                        // string name terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(25)));         // nested document length is 25
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.String);                    // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(7), Encoding.UTF8.GetBytes("capital"))); // element name is 'capital'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(7)));          // string length is 7
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("Moscow")));  // element value is 'Moscow'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { binary: <bytes from 'This is binary data'> }
    /// </summary>
    [Run]
    public void WriteSingleBinaryData()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var data = Encoding.UTF8.GetBytes("This is binary data");
        var root = new BSONDocument();
        var binary = new BSONBinary(BSONBinaryType.BinaryOld, data);
        root.Set(new BSONBinaryElement("binary", binary));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 37); // ensure document length is 37 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(37))); // content length is 37
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Binary); // element type is binary 0x05
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("binary"))); // element name is 'binary'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(19))); // byte length is 19
        Aver.AreEqual(reader.ReadByte(), (byte)BSONBinaryType.BinaryOld); // binary type is BinaryOld 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(19), data)); // byte content is correct

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 37); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { objectId: <bytes from hex '507f1f77bcf86cd799439011'> }
    /// </summary>
    [Run]
    public void WriteSingleObjectId()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var hex = "507f1f77bcf86cd799439011";
        var data = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        var root = new BSONDocument();
        var objectId = new BSONObjectID(data);
        root.Set(new BSONObjectIDElement("objectId", objectId));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 27); // ensure document length is 27 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(27))); // content length is 27
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.ObjectID); // element type is objectID 0x07
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), Encoding.UTF8.GetBytes("objectId"))); // element name is 'objectId'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);  // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(12), data)); // byte content is correct

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 27); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { booleanTrue: true }
    /// </summary>
    [Run]
    public void WriteSingleBooleanTrue()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONBooleanElement("booleanTrue", true));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 19); // ensure document length is 19 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(19))); // content length is 19
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Boolean); // element type is boolean 0x08
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(11), Encoding.UTF8.GetBytes("booleanTrue"))); // element name is 'booleanTrue'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // string name terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)BSONBoolean.True); // byte content is correct

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 19); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { booleanFalse: false }
    /// </summary>
    [Run]
    public void WriteSingleBooleanFalse()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONBooleanElement("booleanFalse", false));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 20); // ensure document length is 20 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(20))); // content length is 20
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Boolean); // element type is boolean 0x08
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(12), Encoding.UTF8.GetBytes("booleanFalse"))); // element name is 'booleanFalse'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // string name terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)BSONBoolean.False); // byte content is correct

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    }

    /// <summary>
    /// { null: null }
    /// </summary>
    [Run]
    public void WriteSingleNull()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONNullElement("null"));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 11); // ensure document length is 11 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(11))); // content length is 11
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Null); // element type is null 0x0a
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("null"))); // element name is 'null'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // string name terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 11); // ensure whole document readed
      }
    }

    /// <summary>
    /// { now: <DateTime from 635000000000000000 ticks> }  ({"now":{"$date":"2013-03-27T13:53:20.000Z"}})
    /// </summary>
    [Run]
    public void WriteSingleDateTime()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var now = new DateTime(635000000000000000).ToUniversalTime();
        var root = new BSONDocument();
        root.Set(new BSONDateTimeElement("now", now));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 18); // ensure document length is 18 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(18))); // ensure content length is 18
        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.DateTime); // ensure element type is DateTime 0x09
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("now"))); // ensure element name is 'now'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // ensure string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(now.ToMillisecondsSinceUnixEpochStart()))); // ensure element value is correct
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 18); // ensure whole document readed
      }
    }

    [Run]
    public void WriteAndReadSingleDateTime()
    {
      using (var stream = new MemoryStream())
      {
        var now = new DateTime(2010, 10, 12,  11, 20, 12, DateTimeKind.Local);
        var root = new BSONDocument();
        root.Set(new BSONDateTimeElement("mydate", now));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 21); // ensure document length is 21 bytes

        stream.Seek(0, SeekOrigin.Begin);

        var root2 = new BSONDocument(stream);
        Aver.AreEqual(stream.Position, 21); // ensure whole document read

        Aver.AreEqual(1, root2.Count); // ensure whole document read
        Aver.AreEqual(now.ToUniversalTime(), ((BSONDateTimeElement)root2["mydate"]).Value);
      }
    }

    /// <summary>
    /// { email: <pattern='^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$' options=I,M,U> }
    /// </summary>
    [Run]
    public void WriteSingleRegularExpression()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var pattern = @"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$";
        var options = BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M |BSONRegularExpressionOptions.U;
        var regex = new BSONRegularExpression(pattern, options);
        root.Set(new BSONRegularExpressionElement("email", regex));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 55); // ensure document length is 55 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(55)));       // ensure content length is 55
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.RegularExpression);     // ensure element type is RegularExpression 0x0b
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("email"))); // ensure element name is 'email'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                  // ensure string name terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(38), Encoding.UTF8.GetBytes(pattern))); // ensure element value is pattern
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string value terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("imu"))); // ensure element value is options in BSON format
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                // ensure string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 55); // ensure whole document readed
      }
    }

    /// <summary>
    /// { code: "function(){var x=1;var y='abc';return 1;};" }
    /// </summary>
    [Run]
    public void WriteSingleJavaScript()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var code = "function(){var x=1;var y='abc';return 1;};";
        root.Set(new BSONJavaScriptElement("code", code));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 58); // ensure document length is 58 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(58)));      // content length is 58
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.JavaScript);           // element type is JavaScript 0x0d
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("code"))); // element name is 'code'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                 // string name terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(43)));     // js code content length is 43
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(42), Encoding.UTF8.GetBytes(code))); // element value is code
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                // string value terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                // last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 58); // ensure whole document readed
      }
    }

    /// <summary>
    /// { codeWithScope: "function(){var x=1;var y='abc';return z;}; <with scope: z=23>" }
    /// </summary>
    [Run]
    public void WriteSingleJavaScriptWithScope()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var code = "function(){var x=1;var y='abc';return z;};";
        var scope = new BSONDocument();
        scope.Set(new BSONInt32Element("z", 23));
        var jsWithScope = new BSONCodeWithScope(code, scope);
        root.Set(new BSONJavaScriptWithScopeElement("codeWithScope", jsWithScope));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 83); // ensure document length is 83 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(83)));       // content length is 83
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.JavaScriptWithScope);   // element type is JavaScriptWithScope 0x0f
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(13), Encoding.UTF8.GetBytes("codeWithScope"))); // element name is 'codeWithScope'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                  // string name terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(63)));     // full content length is 63
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(43)));     // content length is 43
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(42), Encoding.UTF8.GetBytes(code))); // element value is code
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                // string value terminator 0x00 is present

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(12)));   // full scope content length is 12
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);             // element type is int 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(1), Encoding.UTF8.GetBytes("z"))); // element name is 'z'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                              // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(23)));   // z variable value is 23

        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00
        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 83); // ensure whole document readed
      }
    }

    /// <summary>
    /// { stamp: <seconds since Unix epoch to DateTime from 635000000000000000 ticks with 123 increment> }
    /// </summary>
    [Run]
    public void WriteSingleTimestamp()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        var now = new DateTime(635000000000000000).ToUniversalTime();
        var stamp = new BSONTimestamp(now, 123);
        root.Set(new BSONTimestampElement("stamp", stamp));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 20); // ensure document length is 20 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(20)));       // content length is 20
        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.TimeStamp);            // element type is TimeStamp 0x11
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("stamp"))); // element name is 'now'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                                 // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(123)));      // increment is correct
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes((int)now.ToSecondsSinceUnixEpochStart()))); // datetime is correct
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                                 // last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    }

    /// <summary>
    /// { minkey: <minkey> }
    /// </summary>
    [Run]
    public void WriteSingleMinKey()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONMinKeyElement("minkey"));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 13); // ensure document length is 13 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(13)));        // ensure content length is 13
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.MinKey);                 // ensure element type is MinKey 0xff
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("minkey"))); // ensure element name is 'minkey'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string name terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    }

    /// <summary>
    /// { maxkey: <maxkey> }
    /// </summary>
    [Run]
    public void WriteSingleMaxKey()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONMaxKeyElement("maxkey"));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 13); // ensure document length is 13 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(13))); // ensure content length is 13
        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.MaxKey);                 // ensure element type is MaxKey 0x7f
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("maxkey"))); // ensure element name is 'maxkey'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure string name terminator 0x00 is present
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                   // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    }

    /// <summary>
    /// {
    ///   eng: "hello",
    ///   rus: "привет",
    ///   chi: "你好",
    ///   jap: "こんにちは",
    ///   gre: "γεια σας",
    ///   alb: "përshëndetje",
    ///   arm: "բարեւ Ձեզ",
    ///   vie: "xin chào",
    ///   por: "Olá",
    ///   ukr: "Привіт",
    ///   ger: "wünsche"
    /// }
    /// </summary>
    [Run]
    public void WriteUnicodeStrings()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONStringElement("eng", "hello"));
        root.Set(new BSONStringElement("rus", "привет"));
        root.Set(new BSONStringElement("chi", "你好"));
        root.Set(new BSONStringElement("jap", "こんにちは"));
        root.Set(new BSONStringElement("gre", "γεια σας"));
        root.Set(new BSONStringElement("alb", "përshëndetje"));
        root.Set(new BSONStringElement("arm", "բարեւ Ձեզ"));
        root.Set(new BSONStringElement("vie", "xin chào"));
        root.Set(new BSONStringElement("por", "Olá"));
        root.Set(new BSONStringElement("ukr", "Привіт"));
        root.Set(new BSONStringElement("ger", "wünsche"));
        root.WriteAsBSON(stream);
        Aver.AreEqual(stream.Position, 232); // ensure document length is 33 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(232)));    // content length is 232

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("eng"))); // element name is 'eng'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(6)));      // string content length is 6
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(5), Encoding.UTF8.GetBytes("hello")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("rus"))); // element name is 'rus'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(13)));     // string content length is 13
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(12), Encoding.UTF8.GetBytes("привет")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("chi"))); // element name is 'chi'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(7)));      // string content length is 7
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("你好")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("jap"))); // element name is 'jap'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(16)));     // string content length is 16
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(15), Encoding.UTF8.GetBytes("こんにちは")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("gre"))); // element name is 'gre'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(16)));     // string content length is 16
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(15), Encoding.UTF8.GetBytes("γεια σας")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("alb"))); // element name is 'alb'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(15)));     // string content length is 15
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(14), Encoding.UTF8.GetBytes("përshëndetje")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("arm"))); // element name is 'arm'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(18)));     // string content length is 18
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(17), Encoding.UTF8.GetBytes("բարեւ Ձեզ")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("vie"))); // element name is 'vie'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(10)));     // string content length is 10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(9), Encoding.UTF8.GetBytes("xin chào")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("por"))); // element name is 'por'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(5)));      // string content length is 5
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("Olá")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("ukr"))); // element name is 'ukr'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(13)));     // string content length is 13
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(12), Encoding.UTF8.GetBytes("Привіт")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String);             // element type is string 0x02
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(3), Encoding.UTF8.GetBytes("ger"))); // element name is 'ger'
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(9)));      // string content length is 9
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), Encoding.UTF8.GetBytes("wünsche")));
        Aver.AreEqual(reader.ReadByte(), (byte) 0x00);                               // string value terminator 0x00 is present

        Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 232); // ensure whole document readed
      }
    }

    /// <summary>
    /// (Longrunning!)
    /// Ensures that simple string element is correctly serialized for all string lengths
    /// in range from 0 to 10 Kb
    /// { vary: "aaaa...[n of 'a' chars]" }
    /// </summary>
    [Run]
    public void WriteReadStringsWithDifferentLengths()
    {
      Parallel.For(0, 10*1024, i =>
      {
        using (var stream = new MemoryStream())
        using (var reader = new BinaryReader(stream))
        {
          // Write

          var root = new BSONDocument();
          var value = new string('a', i);
          root.Set(new BSONStringElement("vary", value));
          root.WriteAsBSON(stream);

          Aver.AreEqual(stream.Position, 16 + i); // ensure document length is 16+i bytes

          stream.Seek(0, SeekOrigin.Begin);

          Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(16 + i))); // content length is 16+i
          Aver.AreEqual(reader.ReadByte(), (byte) BSONElementType.String); // element type is string 0x02
          Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), Encoding.UTF8.GetBytes("vary"))); // element name is 'vary'
          Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // string name terminator 0x00 is present
          Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(i + 1))); // string content length is 13
          Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(i), Encoding.UTF8.GetBytes(value))); // element value is value
          Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // string value terminator 0x00 is present
          Aver.AreEqual(reader.ReadByte(), (byte) 0x00); // last byte is terminator 0x00

          Aver.AreEqual(stream.Position, 16 + i); // ensure whole document readed
          stream.Position = 0;

          // Read

          var deser = new BSONDocument(stream);

          Aver.AreEqual(deser.ByteSize, 16 + i);
          Aver.AreEqual(deser.Count, 1);

          var element = deser["vary"] as BSONStringElement;
          Aver.IsNotNull(element);
          Aver.IsTrue(element.ElementType == BSONElementType.String);
          Aver.AreEqual(element.Name, "vary");
          Aver.AreEqual(element.Value, value);
          Aver.AreEqual(stream.Position, 16 + i); // ensure whole document readed
          stream.Position = 0;
        }
      });
    }

    /// <summary>
    /// { intMin: <int.MinValue>, intMax: <int.MaxValue>, longMin: <long.MinValue>, longMax: <long.MaxValue> }
    /// </summary>
    [Run]
    public void WriteBigIntegers()
    {
      using (var stream = new MemoryStream())
      using (var reader = new BinaryReader(stream))
      {
        var root = new BSONDocument();
        root.Set(new BSONInt32Element("intMin", int.MinValue));
        root.Set(new BSONInt32Element("intMax", int.MaxValue));
        root.Set(new BSONInt64Element("longMin", long.MinValue));
        root.Set(new BSONInt64Element("longMax", long.MaxValue));
        root.WriteAsBSON(stream);

        Aver.AreEqual(stream.Position, 63); // ensure document length is 63 bytes

        stream.Seek(0, SeekOrigin.Begin);

        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(63)));           // content length is 63

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);                     // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("intMin")));    // element name is 'intMin'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(int.MinValue))); // element value is int.MinValue

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int32);                     // element type is int32 0x10
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(6), Encoding.UTF8.GetBytes("intMax")));    // element name is 'intMax'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                      // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(4), BitConverter.GetBytes(int.MaxValue))); // element value is int.MaxValue

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64);                      // element type is int64 0x12
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(7), Encoding.UTF8.GetBytes("longMin")));    // element name is 'longMin'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                       // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(long.MinValue))); // element value is long.MinValue

        Aver.AreEqual(reader.ReadByte(), (byte)BSONElementType.Int64);                      // element type is int64 0x12
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(7), Encoding.UTF8.GetBytes("longMax")));    // element name is 'longMax'
        Aver.AreEqual(reader.ReadByte(), (byte)0x00);                                       // string name terminator 0x00 is present
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(reader.ReadBytes(8), BitConverter.GetBytes(long.MaxValue))); // element value is long.MaxValue


        Aver.AreEqual(reader.ReadByte(), (byte)0x00); // ensure last byte is terminator 0x00

        Aver.AreEqual(stream.Position, 63); // ensure whole document readed
      }
    }

    #endregion


    #region WriteRead

    //20170429 DKh
    [Run("cnt=100")]
    [Run("cnt=256")]
    [Run("cnt=512")]
    [Run("cnt=1024")]
    [Run("cnt=65536")]
    [Run("cnt=524288")]
    public void WriteReadLongStringOfLongChars(int cnt)
    {
      using (var stream = new MemoryStream())
      {
        var doc1 = new BSONDocument();

        doc1.Set( new BSONStringElement("a", new string('久', cnt)));

        doc1.WriteAsBSON(stream);
        stream.Position = 0;

        var doc2 = new BSONDocument(stream);

        Aver.AreEqual(1, doc2.Count);
        Aver.AreEqual( cnt, ((BSONStringElement)doc2["a"]).Value.Length );
      }
    }


    [Run]
    public void WriteReadLongMulticulturalString()
    {
      using (var stream = new MemoryStream())
      {
        var doc1 = new BSONDocument();

        var original = "就是巴尼宝贝儿吧，俺说。有什么怪事儿或是好事儿吗？ когда американские авианосцы 'Уинсон' и 'Мидуэй' приблизились 지구상의　３대 we have solved the problem";

        doc1.Set( new BSONStringElement("a", original));

        doc1.WriteAsBSON(stream);
        stream.Position = 0;

        var doc2 = new BSONDocument(stream);

        Aver.AreEqual(1, doc2.Count);
        Aver.AreEqual( original, ((BSONStringElement)doc2["a"]).Value );
      }
    }


    [Run]
    public void WriteReadStream()
    {
      using (var stream = new MemoryStream())
      {
        var doc1 = new BSONDocument();

        doc1.Set( new BSONStringElement("a", "abcd"));
        doc1.Set( new BSONInt32Element("b", 234));
        doc1.Set( new BSONNullElement("c"));

        doc1.WriteAsBSON(stream);
        stream.Position = 0;

        var doc2 = new BSONDocument(stream);

        Aver.AreEqual(3, doc2.Count);
        Aver.AreEqual("abcd", ((BSONStringElement)doc2["a"]).Value);
        Aver.AreEqual(234, ((BSONInt32Element)doc2["b"]).Value);
        Aver.IsNull(((BSONNullElement)doc2["c"]).ObjectValue);
      }
    }

    [Run]
    public void WriteReadBuffer()
    {
      var doc1 = new BSONDocument();

      doc1.Set( new BSONStringElement("a", "abcd"));
      doc1.Set( new BSONInt32Element("b", 234));
      doc1.Set( new BSONNullElement("c"));

      var buf =  doc1.WriteAsBSONToNewArray();

      using (var stream = new MemoryStream(buf))
      {
        var doc2 = new BSONDocument(stream);

        Aver.AreEqual(3, doc2.Count);
        Aver.AreEqual("abcd", ((BSONStringElement)doc2["a"]).Value);
        Aver.AreEqual(234, ((BSONInt32Element)doc2["b"]).Value);
        Aver.IsNull(((BSONNullElement)doc2["c"]).ObjectValue);
      }
    }


    #endregion


    #region Deserialization

    /// <summary>
    /// {} (empty document)
    /// </summary>
    [Run]
    public void ReadEmptyDocument()
    {
      var src = Convert.FromBase64String(@"BQAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 5);
        Aver.AreEqual(root.Count, 0);
        Aver.AreEqual(stream.Position, 5); // ensure whole document readed
      }
    }

    /// <summary>
    /// { pi: 3.14159265358979 }
    /// </summary>
    [Run]
    public void ReadSingleDouble()
    {
      var src = Convert.FromBase64String(@"EQAAAAFwaQAYLURU+yEJQAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 17);
        Aver.AreEqual(root.Count, 1);

        var element = root["pi"] as BSONDoubleElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Double);
        Aver.AreEqual(element.Name, "pi");
        Aver.IsTrue(Math.Abs(element.Value - Math.PI) < double.Epsilon);
        Aver.AreEqual(stream.Position, 17); // ensure whole document readed
      }
    }

    /// <summary>
    /// { greetings: "Hello World!" }
    /// </summary>
    [Run]
    public void ReadSingleString()
    {
      var src = Convert.FromBase64String(@"IQAAAAJncmVldGluZ3MADQAAAEhlbGxvIFdvcmxkIQAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 33);
        Aver.AreEqual(root.Count, 1);

        var element = root["greetings"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "greetings");
        Aver.AreEqual(element.Value, "Hello World!");
        Aver.AreEqual(stream.Position, 33); // ensure whole document readed
      }
    }

    /// <summary>
    /// { lucky: 7 }
    /// </summary>
    [Run]
    public void ReadSingleInt32()
    {
      var src = Convert.FromBase64String(@"EAAAABBsdWNreQAHAAAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 16);
        Aver.AreEqual(root.Count, 1);

        var element = root["lucky"] as BSONInt32Element;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Int32);
        Aver.AreEqual(element.Name, "lucky");
        Aver.AreEqual(element.Value, 7);
        Aver.AreEqual(stream.Position, 16); // ensure whole document readed
      }
    }

    /// <summary>
    /// { solarSystemDiameter: 10000000000000 }
    /// </summary>
    [Run]
    public void ReadSingleInt64()
    {
      var src = Convert.FromBase64String(@"IgAAABJzb2xhclN5c3RlbURpYW1ldGVyAACgck4YCQAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 34);
        Aver.AreEqual(root.Count, 1);

        var element = root["solarSystemDiameter"] as BSONInt64Element;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Int64);
        Aver.AreEqual(element.Name, "solarSystemDiameter");
        Aver.AreEqual(element.Value, 10000000000000);
        Aver.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'fruits': ['apple, 'orange', 'plum'] } --> { 'fruits': { '0': 'apple', '1': 'orange', '2': 'plum' } }
    /// </summary>
    [Run]
    public void ReadStringArray()
    {
      var src = Convert.FromBase64String(@"OQAAAARmcnVpdHMALAAAAAIwAAYAAABhcHBsZQACMQAHAAAAb3JhbmdlAAIyAAUAAABwbHVtAAAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 57);
        Aver.AreEqual(root.Count, 1);

        var element = root["fruits"] as BSONArrayElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "fruits");
        Aver.IsTrue(element.ElementType == BSONElementType.Array);
        Aver.IsNotNull(element.Value);
        Aver.AreEqual(element.Value.Length, 3);

        var item1 = element.Value[0] as BSONStringElement;
        Aver.IsNotNull(item1);
        Aver.IsTrue(item1.IsArrayElement);
        Aver.IsTrue(item1.ElementType == BSONElementType.String);
        Aver.AreEqual(item1.Value, "apple");

        var item2 = element.Value[1] as BSONStringElement;
        Aver.IsNotNull(item2);
        Aver.IsTrue(item2.IsArrayElement);
        Aver.IsTrue(item2.ElementType == BSONElementType.String);
        Aver.AreEqual(item2.Value, "orange");

        var item3 = element.Value[2] as BSONStringElement;
        Aver.IsNotNull(item3);
        Aver.IsTrue(item3.ElementType == BSONElementType.String);
        Aver.IsTrue(item3.IsArrayElement);
        Aver.AreEqual(item3.Value, "plum");

        Aver.AreEqual(stream.Position, 57); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of int32
    /// { 'years': [1963, 1984, 2015] } --> { 'years': { '0': 1963, '1': 1984, '2': 2015 } }
    /// </summary>
    [Run]
    public void ReadInt32Array()
    {
      var src = Convert.FromBase64String(@"JgAAAAR5ZWFycwAaAAAAEDAAqwcAABAxAMAHAAAQMgDfBwAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 38);
        Aver.AreEqual(root.Count, 1);

        var element = root["years"] as BSONArrayElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "years");
        Aver.IsTrue(element.ElementType == BSONElementType.Array);
        Aver.IsNotNull(element.Value);
        Aver.AreEqual(element.Value.Length, 3);

        var item1 = element.Value[0] as BSONInt32Element;
        Aver.IsNotNull(item1);
        Aver.IsTrue(item1.ElementType == BSONElementType.Int32);
        Aver.IsTrue(item1.IsArrayElement);
        Aver.AreEqual(item1.Value, 1963);

        var item2 = element.Value[1] as BSONInt32Element;
        Aver.IsNotNull(item2);
        Aver.IsTrue(item2.ElementType == BSONElementType.Int32);
        Aver.IsTrue(item2.IsArrayElement);
        Aver.AreEqual(item2.Value, 1984);

        var item3 = element.Value[2] as BSONInt32Element;
        Aver.IsNotNull(item3);
        Aver.IsTrue(item3.ElementType == BSONElementType.Int32);
        Aver.IsTrue(item3.IsArrayElement);
        Aver.AreEqual(item3.Value, 2015);

        Aver.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    /// Array of strings
    /// { 'stuff': ['apple, 3, 2.14] } --> { 'stuff': { '0': 'apple', '1': 3, '2': 2.14 } }
    /// </summary>
    [Run]
    public void ReadStringInt32DoubleMixedArray()
    {
      var src = Convert.FromBase64String(@"MAAAAARzdHVmZgAkAAAAAjAABgAAAGFwcGxlABAxAAMAAAABMgAfhetRuB4BQAAA");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 48);
        Aver.AreEqual(root.Count, 1);

        var element = root["stuff"] as BSONArrayElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "stuff");
        Aver.IsTrue(element.ElementType == BSONElementType.Array);
        Aver.IsNotNull(element.Value);
        Aver.AreEqual(element.Value.Length, 3);

        var item1 = element.Value[0] as BSONStringElement;
        Aver.IsNotNull(item1);
        Aver.IsTrue(item1.IsArrayElement);
        Aver.IsTrue(item1.ElementType == BSONElementType.String);
        Aver.AreEqual(item1.Value, "apple");

        var item2 = element.Value[1] as BSONInt32Element;
        Aver.IsNotNull(item2);
        Aver.IsTrue(item2.IsArrayElement);
        Aver.IsTrue(item2.ElementType == BSONElementType.Int32);
        Aver.AreEqual(item2.Value, 3);

        var item3 = element.Value[2] as BSONDoubleElement;
        Aver.IsNotNull(item3);
        Aver.IsTrue(item3.IsArrayElement);
        Aver.IsTrue(item3.ElementType == BSONElementType.Double);
        Aver.AreEqual(item3.Value, 2.14D);

        Aver.AreEqual(stream.Position, 48); // ensure whole document readed
      }
    }

    /// <summary>
    /// { name: "Gagarin", birth: 1934 }
    /// </summary>
    [Run]
    public void ReadStringAndInt32Pair()
    {
      var src = Convert.FromBase64String(@"IgAAAAJuYW1lAAgAAABHYWdhcmluABBiaXJ0aACOBwAAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 34);
        Aver.AreEqual(root.Count, 2);

        var element1 = root["name"] as BSONStringElement;
        Aver.IsNotNull(element1);
        Aver.IsTrue(element1.ElementType == BSONElementType.String);
        Aver.AreEqual(element1.Name, "name");
        Aver.AreEqual(element1.Value, "Gagarin");

        var element2 = root["birth"] as BSONInt32Element;
        Aver.IsNotNull(element2);
        Aver.IsTrue(element2.ElementType == BSONElementType.Int32);
        Aver.AreEqual(element2.Name, "birth");
        Aver.AreEqual(element2.Value, 1934);

        Aver.AreEqual(stream.Position, 34); // ensure whole document readed
      }
    }

    /// <summary>
    /// { nested: { capital: "Moscow" } }
    /// </summary>
    [Run]
    public void ReadNestedDocument()
    {
      var src = Convert.FromBase64String(@"JgAAAANuZXN0ZWQAGQAAAAJjYXBpdGFsAAcAAABNb3Njb3cAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 38);
        Aver.AreEqual(root.Count, 1);

        var element = root["nested"] as BSONDocumentElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "nested");
        Aver.IsNotNull(element.Value);
        Aver.IsTrue(element.ElementType == BSONElementType.Document);
        Aver.AreEqual(element.Value.Count, 1);

        var nested = element.Value["capital"] as BSONStringElement;
        Aver.IsNotNull(nested);
        Aver.IsTrue(nested.ElementType == BSONElementType.String);
        Aver.AreEqual(nested.Name, "capital");
        Aver.AreEqual(nested.Value, "Moscow");

        Aver.AreEqual(stream.Position, 38); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { binary: <bytes from 'This is binary data'> }
    /// </summary>
    [Run]
    public void ReadSingleBinaryData()
    {
      var src = Convert.FromBase64String(@"JQAAAAViaW5hcnkAEwAAAAJUaGlzIGlzIGJpbmFyeSBkYXRhAA==");

      using (var stream = new MemoryStream(src))
      {
        var data = Encoding.UTF8.GetBytes("This is binary data");
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 37);
        Aver.AreEqual(root.Count, 1);

        var element = root["binary"] as BSONBinaryElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Binary);
        Aver.AreEqual(element.Name, "binary");
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(element.Value.Data, data));
        Aver.IsTrue(element.Value.Type == BSONBinaryType.BinaryOld);

        Aver.AreEqual(stream.Position, 37); // ensure whole document readed
      }
    }

    /// <summary>
    ///  { objectId: <bytes from hex '507f1f77bcf86cd799439011'> }
    /// </summary>
    [Run]
    public void ReadSingleObjectId()
    {
      var src = Convert.FromBase64String(@"GwAAAAdvYmplY3RJZABQfx93vPhs15lDkBEA");

      using (var stream = new MemoryStream(src))
      {
        var hex = "507f1f77bcf86cd799439011";
        var data = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 27);
        Aver.AreEqual(root.Count, 1);

        var element = root["objectId"] as BSONObjectIDElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.ObjectID);
        Aver.AreEqual(element.Name, "objectId");
        Aver.IsTrue(IOMiscUtils.MemBufferEquals(element.Value.Bytes, data));

        Aver.AreEqual(stream.Position, 27); // ensure whole document readed
      }
    }

    /// <summary>
    /// { booleanTrue: true }
    /// </summary>
    [Run]
    public void ReadSingleBooleanTrue()
    {
      var src = Convert.FromBase64String(@"EwAAAAhib29sZWFuVHJ1ZQABAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 19);
        Aver.AreEqual(root.Count, 1);

        var element = root["booleanTrue"] as BSONBooleanElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Boolean);
        Aver.AreEqual(element.Name, "booleanTrue");
        Aver.AreEqual(element.Value, true);

        Aver.AreEqual(stream.Position, 19); // ensure whole document readed
      }
    }

    /// <summary>
    /// { booleanFalse: false }
    /// </summary>
    [Run]
    public void ReadSingleBooleanFalse()
    {
      var src = Convert.FromBase64String(@"FAAAAAhib29sZWFuRmFsc2UAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 20);
        Aver.AreEqual(root.Count, 1);

        var element = root["booleanFalse"] as BSONBooleanElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Boolean);
        Aver.AreEqual(element.Name, "booleanFalse");
        Aver.AreEqual(element.Value, false);

        Aver.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    }

    /// <summary>
    /// { null: null }
    /// </summary>
    [Run]
    public void ReadSingleNull()
    {
      var src = Convert.FromBase64String(@"CwAAAApudWxsAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 11);
        Aver.AreEqual(root.Count, 1);

        var element = root["null"] as BSONNullElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.Null);
        Aver.AreEqual(element.Name, "null");
      }
    }

    /// <summary>
    /// { now: <DateTime from 635000000000000000 ticks> }
    /// </summary>
    [Run]
    public void ReadSingleDateTime()
    {
      var src = Convert.FromBase64String(@"EgAAAAlub3cAAKDErD0BAAAA");

      using (var stream = new MemoryStream(src))
      {
        var now = new DateTime(635000000000000000, DateTimeKind.Utc);
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 18);
        Aver.AreEqual(root.Count, 1);

        var element = root["now"] as BSONDateTimeElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.DateTime);
        Aver.AreEqual(element.Name, "now");
        Aver.AreEqual(element.Value, now);

        Aver.AreEqual(stream.Position, 18); // ensure whole document readed
      }
    }

    /// <summary>
    /// { email: <pattern='^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$' options=I,M,U> }
    /// </summary>
    [Run]
    public void ReadSingleRegularExpression()
    {
      var src = Convert.FromBase64String(@"NwAAAAtlbWFpbABeWy0uXHddK0AoPzpbYS16XGRdezIsfVwuKStbYS16XXsyLDZ9JABpbXUAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var pattern = @"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$";
        var options = BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M |BSONRegularExpressionOptions.U;

        Aver.AreEqual(root.ByteSize, 55);
        Aver.AreEqual(root.Count, 1);

        var element = root["email"] as BSONRegularExpressionElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "email");
        Aver.IsTrue(element.ElementType == BSONElementType.RegularExpression);
        Aver.AreEqual(element.Value.Pattern, pattern);
        Aver.IsTrue(element.Value.Options == options);

        Aver.AreEqual(stream.Position, 55); // ensure whole document readed
      }
    }

    /// <summary>
    /// { code: "function(){var x=1;var y='abc';return 1;};" }
    /// </summary>
    [Run]
    public void ReadSingleJavaScript()
    {
      var src = Convert.FromBase64String(@"OgAAAA1jb2RlACsAAABmdW5jdGlvbigpe3ZhciB4PTE7dmFyIHk9J2FiYyc7cmV0dXJuIDE7fTsAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var code = "function(){var x=1;var y='abc';return 1;};";

        Aver.AreEqual(root.ByteSize, 58);
        Aver.AreEqual(root.Count, 1);

        var element = root["code"] as BSONJavaScriptElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.JavaScript);
        Aver.AreEqual(element.Name, "code");
        Aver.AreEqual(element.Value, code);

        Aver.AreEqual(stream.Position, 58); // ensure whole document readed
      }
    }

    /// <summary>
    /// { codeWithScope: "function(){var x=1;var y='abc';return z;}; <with scope: z=23>" }
    /// </summary>
    [Run]
    public void ReadSingleJavaScriptWithScope()
    {
      var src = Convert.FromBase64String(@"UwAAAA9jb2RlV2l0aFNjb3BlAD8AAAArAAAAZnVuY3Rpb24oKXt2YXIgeD0xO3ZhciB5PSdhYmMnO3JldHVybiB6O307AAwAAAAQegAXAAAAAAA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var code = "function(){var x=1;var y='abc';return z;};";

        Aver.AreEqual(root.ByteSize, 83);
        Aver.AreEqual(root.Count, 1);

        var element = root["codeWithScope"] as BSONJavaScriptWithScopeElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.JavaScriptWithScope);
        Aver.AreEqual(element.Name, "codeWithScope");
        Aver.AreEqual(element.Value.Code, code);

        var scope = element.Value.Scope;
        Aver.IsNotNull(scope);
        Aver.AreEqual(scope.Count, 1);

        var scopeVar = scope["z"] as BSONInt32Element;
        Aver.IsNotNull(scopeVar);
        Aver.IsTrue(scopeVar.ElementType == BSONElementType.Int32);
        Aver.AreEqual(scopeVar.Name, "z");
        Aver.AreEqual(scopeVar.Value, 23);

        Aver.AreEqual(stream.Position, 83); // ensure whole document readed
      }
    }

    /// <summary>
    /// { stamp: <seconds since Unix epoch to DateTime from 635000000000000000 ticks with 123 increment> }
    /// </summary>
    [Run]
    public void ReadSingleTimestamp()
    {
      var src = Convert.FromBase64String(@"FAAAABFzdGFtcAB7AAAAACRTUQA=");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);
        var now = new DateTime(635000000000000000, DateTimeKind.Utc);
        var increment = 123;

        Aver.AreEqual(root.ByteSize, 20);
        Aver.AreEqual(root.Count, 1);

        var element = root["stamp"] as BSONTimestampElement;
        Aver.IsNotNull(element);
        Aver.AreEqual(element.Name, "stamp");
        Aver.IsTrue(element.ElementType == BSONElementType.TimeStamp);
        Aver.AreEqual(element.Value.EpochSeconds, now.ToSecondsSinceUnixEpochStart());
        Aver.IsTrue(element.Value.Increment == increment);

        Aver.AreEqual(stream.Position, 20); // ensure whole document readed
      }
    }

    /// <summary>
    /// { maxkey: <maxkey> }
    /// </summary>
    [Run]
    public void ReadSingleMaxKey()
    {
      var src = Convert.FromBase64String(@"DQAAAH9tYXhrZXkAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 13);
        Aver.AreEqual(root.Count, 1);

        var element = root["maxkey"] as BSONMaxKeyElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.MaxKey);
        Aver.AreEqual(element.Name, "maxkey");

        Aver.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    }

    /// <summary>
    /// { minkey: <minkey> }
    /// </summary>
    [Run]
    public void ReadSingleMinKey()
    {
      var src = Convert.FromBase64String(@"DQAAAP9taW5rZXkAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 13);
        Aver.AreEqual(root.Count, 1);

        var element = root["minkey"] as BSONMinKeyElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.MinKey);
        Aver.AreEqual(element.Name, "minkey");

        Aver.AreEqual(stream.Position, 13); // ensure whole document readed
      }
    }

    [Run]
    public void WriteReadSingleDateTime()
    {
      using (var stream = new MemoryStream())
      {
        var now = new DateTime(2015, 08, 26, 14, 23, 56);
        var bson1 = new BSONDocument();
        bson1.Set(new BSONDateTimeElement("now", now));
        bson1.WriteAsBSON(stream);

        stream.Position = 0;

        var bson2 = new BSONDocument(stream);

        var now1 = ((BSONDateTimeElement)bson1["now"]).Value;
        var now2 = ((BSONDateTimeElement)bson2["now"]).Value;

        Console.WriteLine("{0} {1}", now1, now1.Kind);
        Console.WriteLine("{0} {1}", now2, now2.Kind);

        Aver.AreEqual(now1.ToUniversalTime(), now2);
      }
    }

    /// <summary>
    /// {
    ///   eng: "hello",
    ///   rus: "привет",
    ///   chi: "你好",
    ///   jap: "こんにちは",
    ///   gre: "γεια σας",
    ///   alb: "përshëndetje",
    ///   arm: "բարեւ Ձեզ",
    ///   vie: "xin chào",
    ///   por: "Olá",
    ///   ukr: "Привіт",
    ///   ger: "wünsche"
    /// }
    /// </summary>
    [Run]
    public void ReadUnicodeStrings()
    {
      var src = Convert.FromBase64String(@"6AAAAAJlbmcABgAAAGhlbGxvAAJydXMADQAAANC/0YDQuNCy0LXRggACY2hpAAcAAADkvaDlpb0AAmphcAAQAAAA44GT44KT44Gr44Gh44GvAAJncmUAEAAAAM6zzrXOuc6xIM+DzrHPggACYWxiAA8AAABww6tyc2jDq25kZXRqZQACYXJtABIAAADVotWh1oDVpdaCINWB1aXVpgACdmllAAoAAAB4aW4gY2jDoG8AAnBvcgAFAAAAT2zDoQACdWtyAA0AAADQn9GA0LjQstGW0YIAAmdlcgAJAAAAd8O8bnNjaGUAAA==");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 232);
        Aver.AreEqual(root.Count, 11);

        var element = root["eng"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "eng");
        Aver.AreEqual(element.Value, "hello");

        element = root["rus"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "rus");
        Aver.AreEqual(element.Value, "привет");

        element = root["chi"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "chi");
        Aver.AreEqual(element.Value, "你好");

        element = root["jap"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "jap");
        Aver.AreEqual(element.Value, "こんにちは");

        element = root["gre"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "gre");
        Aver.AreEqual(element.Value, "γεια σας");

        element = root["alb"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "alb");
        Aver.AreEqual(element.Value, "përshëndetje");

        element = root["arm"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "arm");
        Aver.AreEqual(element.Value, "բարեւ Ձեզ");

        element = root["vie"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "vie");
        Aver.AreEqual(element.Value, "xin chào");

        element = root["por"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "por");
        Aver.AreEqual(element.Value, "Olá");

        element = root["ukr"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "ukr");
        Aver.AreEqual(element.Value, "Привіт");

        element = root["ger"] as BSONStringElement;
        Aver.IsNotNull(element);
        Aver.IsTrue(element.ElementType == BSONElementType.String);
        Aver.AreEqual(element.Name, "ger");
        Aver.AreEqual(element.Value, "wünsche");

        Aver.AreEqual(stream.Position, 232); // ensure whole document readed
      }
    }

    /// <summary>
    /// { intMin: <int.MinValue>, intMax: <int.MaxValue>, longMin: <long.MinValue>, longMax: <long.MaxValue> }
    /// </summary>
    [Run]
    public void ReadBigIntegers()
    {
      var src = Convert.FromBase64String(@"PwAAABBpbnRNaW4AAAAAgBBpbnRNYXgA////fxJsb25nTWluAAAAAAAAAACAEmxvbmdNYXgA/////////38A");

      using (var stream = new MemoryStream(src))
      {
        var root = new BSONDocument(stream);

        Aver.AreEqual(root.ByteSize, 63);
        Aver.AreEqual(root.Count, 4);

        var element1 = root["intMin"] as BSONInt32Element;
        Aver.IsNotNull(element1);
        Aver.IsTrue(element1.ElementType == BSONElementType.Int32);
        Aver.AreEqual(element1.Name, "intMin");
        Aver.AreEqual(element1.Value, int.MinValue);

        var element2 = root["intMax"] as BSONInt32Element;
        Aver.IsNotNull(element2);
        Aver.IsTrue(element2.ElementType == BSONElementType.Int32);
        Aver.AreEqual(element2.Name, "intMax");
        Aver.AreEqual(element2.Value, int.MaxValue);

        var element3 = root["longMin"] as BSONInt64Element;
        Aver.IsNotNull(element3);
        Aver.IsTrue(element3.ElementType == BSONElementType.Int64);
        Aver.AreEqual(element3.Name, "longMin");
        Aver.AreEqual(element3.Value, long.MinValue);

        var element4 = root["longMax"] as BSONInt64Element;
        Aver.IsNotNull(element4);
        Aver.IsTrue(element4.ElementType == BSONElementType.Int64);
        Aver.AreEqual(element4.Name, "longMax");
        Aver.AreEqual(element4.Value, long.MaxValue);

        Aver.AreEqual(stream.Position, 63); // ensure whole document readed
      }
    }

    #endregion

    #region IConvertable

    [Run]
    public void TestInt32ElementIConvertable()
    {
      var element = new BSONInt32Element("name", 1256);

      var bl     = element.AsBool();
      var bt     = element.AsByte((byte)23);
      var chr    = element.AsChar();
      var date   = element.AsDateTime();
      var decim  = element.AsDecimal();
      var doubl  = element.AsDouble();
      var enm    = element.AsEnum(BSONElementType.Null);
      var gdid   = element.AsGDID();
      var guid   = element.AsGUID(Guid.Empty);
      var int16  = element.AsShort();
      var int32  = element.AsInt();
      var int64  = element.AsLong();
      var lac    = element.AsLaconicConfig(null);
      var sbt    = element.AsSByte((sbyte)23);
      var single = element.AsFloat();
      var str    = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      var n_bl     = element.AsNullableBool();
      var n_bt     = element.AsNullableByte((byte)23);
      var n_chr    = element.AsNullableChar();
      var n_date   = element.AsNullableDateTime();
      var n_decim  = element.AsNullableDecimal();
      var n_doubl  = element.AsNullableDouble();
      var n_ts    = element.AsNullableTimeSpan(null);
      var n_gdid   = element.AsNullableGDID(new GDID(1,2,3));
      var n_guid   = element.AsNullableGUID(Guid.Empty);
      var n_int16  = element.AsNullableShort();
      var n_int32  = element.AsNullableInt();
      var n_int64  = element.AsNullableLong();
      var n_sbt    = element.AsNullableSByte((sbyte)23);
      var n_single = element.AsNullableFloat();
      var n_uint16 = element.AsNullableUShort();
      var n_uint32 = element.AsNullableUInt();
      var n_uint64 = element.AsNullableULong();

      Aver.AreEqual(bl, true);
      Aver.AreEqual(bt, (byte)23);
      Aver.AreEqual(chr, Convert.ToChar(1256));
      Aver.AreEqual(date, new DateTime(1256));
      Aver.AreEqual(decim, 1256);
      Aver.AreEqual(doubl, 1256.0D);
      Aver.IsTrue(enm == (BSONElementType)1256);
      Aver.IsNull(lac);
      Aver.AreEqual(gdid, new GDID(0, 0, 1256));
      Aver.AreEqual(guid, Guid.Empty);
      Aver.AreEqual(int16, 1256);
      Aver.AreEqual(int32, 1256);
      Aver.AreEqual(int64, 1256);
      Aver.IsTrue(sbt == (byte)23);
      Aver.AreEqual(single, 1256.0F);
      Aver.AreEqual(str, "1256");
      Aver.AreEqual(uint16, 1256);
      Aver.IsTrue(uint32 == 1256);
      Aver.IsTrue(uint64 == 1256);

      Aver.AreEqual(n_bl, true);
      Aver.AreEqual(n_bt, (byte)23);
      Aver.AreEqual(n_chr, Convert.ToChar(1256));
      Aver.AreEqual(n_date, new DateTime(1256));
      Aver.AreEqual(n_decim, 1256);
      Aver.AreEqual(n_doubl, 1256.0D);
      Aver.AreEqual(n_ts, TimeSpan.FromTicks(1256));
      Aver.AreEqual(n_gdid, new GDID(0, 0, 1256));
      Aver.AreEqual(n_guid, Guid.Empty);
      Aver.AreEqual(n_int16, 1256);
      Aver.AreEqual(n_int32, 1256);
      Aver.AreEqual(n_int64, 1256);
      Aver.IsTrue(n_sbt == (byte)23);
      Aver.AreEqual(n_single, 1256.0F);
      Aver.AreEqual(n_uint16, 1256);
      Aver.AreEqual(n_uint32, 1256);
      Aver.AreEqual(n_uint64, 1256);
    }

    [Run]
    public void TestInt64ElementIConvertable()
    {
      var element = new BSONInt64Element("name", 1256000000000);

      var bl = element.AsBool();
      var date = element.AsDateTime();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int64 = element.AsLong();
      var single = element.AsFloat();
      var str = element.AsString();
      var uint64 = element.AsULong();

      Aver.AreEqual(bl, true);
      Aver.AreEqual(date, new DateTime(1256000000000));
      Aver.AreEqual(decim, 1256000000000);
      Aver.AreEqual(doubl, 1256000000000.0D);
      Aver.AreEqual(int64, 1256000000000);
      Aver.AreEqual(single, 1256000000000);
      Aver.AreEqual(str, "1256000000000");
      Aver.IsTrue(uint64 == 1256000000000);
    }

    [Run]
    public void TestDoubleElementIConvertable()
    {
      var element = new BSONDoubleElement("name", 1256.1234D);

      var bl = element.AsBool();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat();
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Aver.AreEqual(bl, true);
      Aver.AreEqual(decim, 1256.1234M);
      Aver.AreEqual(doubl, 1256.1234D);
      Aver.AreEqual(int16, 1256);
      Aver.AreEqual(int32, 1256);
      Aver.AreEqual(int64, 1256);
      Aver.AreEqual(single, 1256.1234F);
      Aver.AreEqual((1256.1234D).ToString(), str);
      Aver.AreEqual(uint16, 1256);
      Aver.IsTrue(uint32 == 1256);
      Aver.IsTrue(uint64 == 1256);
    }

    [Run]
    public void TestStringElementConvertable()
    {
      var element = new BSONStringElement("name", "1256");

      var bl = element.AsBool();
      var chr = element.AsChar();
      var date = element.AsDateTime();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat();
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Aver.AreEqual(bl, true);
      Aver.AreEqual(chr, '1');
      Aver.AreEqual(date, new DateTime(1256));
      Aver.AreEqual(decim, 1256);
      Aver.AreEqual(doubl, 1256.0D);
      Aver.AreEqual(int16, 1256);
      Aver.AreEqual(int32, 1256);
      Aver.AreEqual(int64, 1256);
      Aver.AreEqual(single, 1256.0F);
      Aver.AreEqual(str, "1256");
      Aver.AreEqual(uint16, 1256);
      Aver.IsTrue(uint32 == 1256);
      Aver.IsTrue(uint64 == 1256);
    }

    [Run]
    public void TestBooleanElementConvertable()
    {
      var element = new BSONBooleanElement("name", true);
      var bl = element.AsBool();
      var decim = element.AsDecimal();
      var doubl = element.AsDouble();
      var int16 = element.AsShort();
      var int32 = element.AsInt();
      var int64 = element.AsLong();
      var single = element.AsFloat();
      var str = element.AsString();
      var uint16 = element.AsUShort();
      var uint32 = element.AsUInt();
      var uint64 = element.AsULong();

      Aver.AreEqual(bl, true);
      Aver.AreEqual(decim, 1);
      Aver.AreEqual(doubl, 1);
      Aver.AreEqual(int16, 1);
      Aver.AreEqual(int32, 1);
      Aver.AreEqual(int64, 1);
      Aver.AreEqual(single, 1);
      Aver.AreEqual(str, "True");
      Aver.AreEqual(uint16, 1);
      Aver.IsTrue(uint32 == 1);
      Aver.IsTrue(uint64 == 1);
    }

    [Run]
    public void TestDateTimeElementIConvertable()
    {
      var value = new DateTime(2015, 1, 21, 3, 4, 5);
      var element = new BSONDateTimeElement("name", value);

      var bl = element.AsBool();
      var date = element.AsDateTime();
      var str = element.AsString(null);
      Aver.AreEqual(bl, true);
      Aver.AreEqual(date, value);
    }

    #endregion

    #region Templatization

    [Run]
    public void Templatization_QuerySinglePrimitiveTypes()
    {
      var qry1 = new BSONDocument("{ age: '$$count' }", true,
                            new TemplateArg("count", BSONElementType.Int32, 67));
      var qry2 = new BSONDocument("{ max: '$$long' }", true,
                            new TemplateArg("long", BSONElementType.Int64, long.MaxValue));
      var qry3 = new BSONDocument("{ array: '$$items' }", true,
                            new TemplateArg("items", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("name", "value"),
                                new BSONDoubleElement("name", -1.2345D),
                                new BSONInt32Element("name", 2000000000)
                              }));
      var qry4 = new BSONDocument("{ why: '$$answer' }", true,
                            new TemplateArg("answer", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ qty: '$$value' }", true,
                            new TemplateArg("value", BSONElementType.Double, 123456.789012D));

      Aver.AreEqual(qry1.Count, 1);
      Aver.IsNotNull(qry1["age"]);
      Aver.IsTrue(qry1["age"] is BSONInt32Element);
      Aver.AreEqual(qry1["age"].ObjectValue.AsInt(), 67);

      Aver.AreEqual(qry2.Count, 1);
      Aver.IsNotNull(qry2["max"]);
      Aver.IsTrue(qry2["max"] is BSONInt64Element);
      Aver.AreEqual(qry2["max"].ObjectValue.AsLong(), long.MaxValue);

      Aver.AreEqual(qry3.Count, 1);
      Aver.IsNotNull(qry3["array"]);
      Aver.IsTrue(qry3["array"] is BSONArrayElement);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Aver.IsNotNull(elements);
      Aver.AreEqual(elements.Length, 3);
      Aver.IsTrue(elements[0] is BSONStringElement);
      Aver.IsTrue(elements[1] is BSONDoubleElement);
      Aver.IsTrue(elements[2] is BSONInt32Element);
      Aver.AreEqual(elements[0].ObjectValue.AsString(), "value");
      Aver.AreEqual(elements[1].ObjectValue.AsDouble(), -1.2345D);
      Aver.AreEqual(elements[2].ObjectValue.AsInt(), 2000000000);

      Aver.AreEqual(qry4.Count, 1);
      Aver.IsNotNull(qry4["why"]);
      Aver.IsTrue(qry4["why"] is BSONBooleanElement);
      Aver.AreEqual(qry4["why"].ObjectValue.AsBool(), true);

      Aver.AreEqual(qry5.Count, 1);
      Aver.IsNotNull(qry5["qty"]);
      Aver.IsTrue(qry5["qty"] is BSONDoubleElement);
      Aver.AreEqual(qry5["qty"].ObjectValue.AsDouble(), 123456.789012D);
    }

    [Run]
    public void Templatization_SinglePrimitiveNames()
    {
      var qry1 = new BSONDocument("{ '$$age': 67 }", true,
                            new TemplateArg("age", BSONElementType.String, "myage"));
      var qry2 = new BSONDocument("{ '$$max': 9223372036854775807 }", true,
                            new TemplateArg("max", BSONElementType.String, "longMax"));
      var qry3 = new BSONDocument("{ '$$items': [1, '2', 3] }", true,
                            new TemplateArg("items", BSONElementType.String, "array"));
      var qry4 = new BSONDocument("{ '$$why': true }", true,
                            new TemplateArg("why", BSONElementType.String, "whyTrue"));
      var qry5 = new BSONDocument("{ '$$qty': 123456.789012 }", true,
                            new TemplateArg("qty", BSONElementType.String, "qtyName"));

      Aver.AreEqual(qry1.Count, 1);
      Aver.IsNotNull(qry1["myage"]);
      Aver.IsTrue(qry1["myage"] is BSONInt32Element);
      Aver.AreEqual(qry1["myage"].ObjectValue.AsInt(), 67);

      Aver.AreEqual(qry2.Count, 1);
      Aver.IsNotNull(qry2["longMax"]);
      Aver.IsTrue(qry2["longMax"] is BSONInt64Element);
      Aver.AreEqual(qry2["longMax"].ObjectValue.AsLong(), long.MaxValue);

      Aver.AreEqual(qry3.Count, 1);
      Aver.IsNotNull(qry3["array"]);
      Aver.IsTrue(qry3["array"] is BSONArrayElement);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Aver.IsNotNull(elements);
      Aver.AreEqual(elements.Length, 3);
      Aver.IsTrue(elements[0] is BSONInt32Element);
      Aver.IsTrue(elements[1] is BSONStringElement);
      Aver.IsTrue(elements[2] is BSONInt32Element);
      Aver.AreEqual(elements[0].ObjectValue.AsInt(), 1);
      Aver.AreEqual(elements[1].ObjectValue.AsString(), "2");
      Aver.AreEqual(elements[2].ObjectValue.AsInt(), 3);

      Aver.AreEqual(qry4.Count, 1);
      Aver.IsNotNull(qry4["whyTrue"]);
      Aver.IsTrue(qry4["whyTrue"] is BSONBooleanElement);
      Aver.AreEqual(qry4["whyTrue"].ObjectValue.AsBool(), true);

      Aver.AreEqual(qry5.Count, 1);
      Aver.IsNotNull(qry5["qtyName"]);
      Aver.IsTrue(qry5["qtyName"] is BSONDoubleElement);
      Aver.AreEqual(qry5["qtyName"].ObjectValue.AsDouble(), 123456.789012D);
    }

    [Run]
    public void Templatization_SinglePrimitiveNamesAndValues()
    {
      var qry1 = new BSONDocument("{ '$$age': '$$ageValue' }", true,
                            new TemplateArg("age", BSONElementType.String, "myage"),
                            new TemplateArg("ageValue", BSONElementType.Int32, 30));
      var qry2 = new BSONDocument("{ '$$max': '$$maxValue' }", true,
                            new TemplateArg("max", BSONElementType.String, "longMax"),
                            new TemplateArg("maxValue", BSONElementType.Int64, long.MaxValue));
      var qry3 = new BSONDocument("{ '$$items': '$$arrayValue' }", true,
                            new TemplateArg("items", BSONElementType.String, "array"),
                            new TemplateArg("arrayValue", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("name", "value"),
                                new BSONDoubleElement("name", -1.2345D),
                                new BSONInt32Element("name", 2000000000)
                              }));
      var qry4 = new BSONDocument("{ '$$why': '$$whyValue' }", true,
                            new TemplateArg("why", BSONElementType.String, "whyTrue"),
                            new TemplateArg("whyValue", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ '$$qty': '$$qtyValue' }", true,
                            new TemplateArg("qty", BSONElementType.String, "qtyName"),
                            new TemplateArg("qtyValue", BSONElementType.Double, 123456.789012D));

      Aver.AreEqual(qry1.Count, 1);
      Aver.IsNotNull(qry1["myage"]);
      Aver.IsTrue(qry1["myage"] is BSONInt32Element);
      Aver.AreEqual(qry1["myage"].ObjectValue.AsInt(), 30);

      Aver.AreEqual(qry2.Count, 1);
      Aver.IsNotNull(qry2["longMax"]);
      Aver.IsTrue(qry2["longMax"] is BSONInt64Element);
      Aver.AreEqual(qry2["longMax"].ObjectValue.AsLong().AsLong(), long.MaxValue);

      Aver.AreEqual(qry3.Count, 1);
      Aver.IsNotNull(qry3["array"]);
      Aver.IsTrue(qry3["array"] is BSONArrayElement);
      var elements = ((BSONArrayElement)qry3["array"]).Value;
      Aver.IsNotNull(elements);
      Aver.AreEqual(elements.Length, 3);
      Aver.IsTrue(elements[0] is BSONStringElement);
      Aver.IsTrue(elements[1] is BSONDoubleElement);
      Aver.IsTrue(elements[2] is BSONInt32Element);
      Aver.AreEqual(elements[0].ObjectValue.AsString(), "value");
      Aver.AreEqual(elements[1].ObjectValue.AsDouble(), -1.2345D);
      Aver.AreEqual(elements[2].ObjectValue.AsInt(), 2000000000);

      Aver.AreEqual(qry4.Count, 1);
      Aver.IsNotNull(qry4["whyTrue"]);
      Aver.IsTrue(qry4["whyTrue"] is BSONBooleanElement);
      Aver.AreEqual(qry4["whyTrue"].ObjectValue.AsBool(), true);

      Aver.AreEqual(qry5.Count, 1);
      Aver.IsNotNull(qry5["qtyName"]);
      Aver.IsTrue(qry5["qtyName"] is BSONDoubleElement);
      Aver.AreEqual(qry5["qtyName"].ObjectValue.AsDouble(), 123456.789012D);
    }

    [Run]
    public void Templatization_QuerySingleObjects()
    {
      var qry0 = new BSONDocument("{ '$$docName': { '$$intName': '$$intValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc0"),
                            new TemplateArg("intName", BSONElementType.String, "int"),
                            new TemplateArg("intValue", BSONElementType.Int32, int.MinValue));
      var qry1 = new BSONDocument("{ '$$docName': { '$$longName': '$$longValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc1"),
                            new TemplateArg("longName", BSONElementType.String, "long"),
                            new TemplateArg("longValue", BSONElementType.Int64, long.MinValue));
      var qry2 = new BSONDocument("{ '$$docName': { '$$stringName': '$$stringValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc2"),
                            new TemplateArg("stringName", BSONElementType.String, "string"),
                            new TemplateArg("stringValue", BSONElementType.String, "Hello world!"));
      var qry3 = new BSONDocument("{ '$$docName': { '$$arrayName': '$$arrayValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc3"),
                            new TemplateArg("arrayName", BSONElementType.String, "array"),
                            new TemplateArg("arrayValue", BSONElementType.Array,
                              new BSONElement[]
                              {
                                new BSONStringElement("value"),
                                new BSONDoubleElement(-1.2345D),
                                new BSONInt32Element(2000000000)
                              }));
      var qry4 = new BSONDocument("{ '$$docName': { '$$boolName': '$$boolValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc4"),
                            new TemplateArg("boolName", BSONElementType.String, "bool"),
                            new TemplateArg("boolValue", BSONElementType.Boolean, true));
      var qry5 = new BSONDocument("{ '$$docName': { '$$doubleName': '$$doubleValue' } }", true,
                            new TemplateArg("docName", BSONElementType.String, "doc5"),
                            new TemplateArg("doubleName", BSONElementType.String, "double"),
                            new TemplateArg("doubleValue", BSONElementType.Double, double.MinValue));

      Aver.AreEqual(qry0.Count, 1);
      Aver.IsNotNull(qry0["doc0"]);
      Aver.IsTrue(qry0["doc0"] is BSONDocumentElement);
      var doc0 = (BSONDocumentElement)qry0["doc0"];
      Aver.IsNotNull(doc0.Value);
      Aver.AreEqual(doc0.Value.Count, 1);
      Aver.IsNotNull(doc0.Value["int"]);
      Aver.IsTrue(doc0.Value["int"] is BSONInt32Element);
      Aver.AreEqual(doc0.Value["int"].ObjectValue.AsInt(), int.MinValue);

      Aver.AreEqual(qry1.Count, 1);
      Aver.IsNotNull(qry1["doc1"]);
      Aver.IsTrue(qry1["doc1"] is BSONDocumentElement);
      var doc1 = (BSONDocumentElement)qry1["doc1"];
      Aver.IsNotNull(doc1.Value);
      Aver.AreEqual(doc1.Value.Count, 1);
      Aver.IsNotNull(doc1.Value["long"]);
      Aver.IsTrue(doc1.Value["long"] is BSONInt64Element);
      Aver.AreEqual(doc1.Value["long"].ObjectValue.AsLong(), long.MinValue);

      Aver.AreEqual(qry2.Count, 1);
      Aver.IsNotNull(qry2["doc2"]);
      Aver.IsTrue(qry2["doc2"] is BSONDocumentElement);
      var doc2 = (BSONDocumentElement)qry2["doc2"];
      Aver.IsNotNull(doc2.Value);
      Aver.AreEqual(doc2.Value.Count, 1);
      Aver.IsNotNull(doc2.Value["string"]);
      Aver.IsTrue(doc2.Value["string"] is BSONStringElement);
      Aver.AreEqual(doc2.Value["string"].ObjectValue.AsString(), "Hello world!");

      Aver.AreEqual(qry3.Count, 1);
      Aver.IsNotNull(qry3["doc3"]);
      Aver.IsTrue(qry3["doc3"] is BSONDocumentElement);
      var doc3 = (BSONDocumentElement)qry3["doc3"];
      Aver.IsNotNull(doc3.Value);
      Aver.AreEqual(doc3.Value.Count, 1);
      Aver.IsNotNull(doc3.Value["array"]);
      Aver.IsTrue(doc3.Value["array"] is BSONArrayElement);
      var array = ((BSONArrayElement)doc3.Value["array"]).Value;
      Aver.AreEqual(array.Length, 3);
      Aver.IsTrue(array[0] is BSONStringElement);
      Aver.IsTrue(array[1] is BSONDoubleElement);
      Aver.IsTrue(array[2] is BSONInt32Element );
      Aver.AreEqual(array[0].ObjectValue.AsString(), "value");
      Aver.AreEqual(array[1].ObjectValue.AsDouble(), -1.2345D);
      Aver.AreEqual(array[2].ObjectValue.AsInt(), 2000000000);

      Aver.AreEqual(qry4.Count, 1);
      Aver.IsNotNull(qry4["doc4"]);
      Aver.IsTrue(qry4["doc4"] is BSONDocumentElement);
      var doc4 = (BSONDocumentElement)qry4["doc4"];
      Aver.IsNotNull(doc4.Value);
      Aver.AreEqual(doc4.Value.Count, 1);
      Aver.IsNotNull(doc4.Value["bool"]);
      Aver.IsTrue(doc4.Value["bool"] is BSONBooleanElement);
      Aver.AreEqual(doc4.Value["bool"].ObjectValue.AsBool(), true);

      Aver.AreEqual(qry5.Count, 1);
      Aver.IsNotNull(qry5["doc5"]);
      Aver.IsTrue(qry5["doc5"] is BSONDocumentElement);
      var doc5 = (BSONDocumentElement)qry5["doc5"];
      Aver.IsNotNull(doc5.Value);
      Aver.AreEqual(doc5.Value.Count, 1);
      Aver.IsNotNull(doc5.Value["double"]);
      Aver.IsTrue(doc5.Value["double"] is BSONDoubleElement);
      Aver.AreEqual(doc5.Value["double"].ObjectValue.AsDouble(), double.MinValue);
    }

    [Run]
    public void Templatization_ArrayOfUnicodeStringValues()
    {
      var qry0 = new BSONDocument("{ '$$unicode': [ '$$eng', '$$rus', '$$chi', '$$jap', '$$gre', '$$alb', '$$arm', '$$vie', '$$por', '$$ukr', '$$ger' ] }", true,
                           new TemplateArg("unicode", BSONElementType.String, "strings"),
                           new TemplateArg("eng", BSONElementType.String, "hello"),
                           new TemplateArg("rus", BSONElementType.String, "привет"),
                           new TemplateArg("chi", BSONElementType.String, "你好"),
                           new TemplateArg("jap", BSONElementType.String, "こんにちは"),
                           new TemplateArg("gre", BSONElementType.String, "γεια σας"),
                           new TemplateArg("alb", BSONElementType.String, "përshëndetje"),
                           new TemplateArg("arm", BSONElementType.String, "բարեւ Ձեզ"),
                           new TemplateArg("vie", BSONElementType.String, "xin chào"),
                           new TemplateArg("por", BSONElementType.String, "Olá"),
                           new TemplateArg("ukr", BSONElementType.String, "Привіт"),
                           new TemplateArg("ger", BSONElementType.String, "wünsche"));

      Aver.AreEqual(qry0.Count, 1);
      Aver.IsNotNull(qry0["strings"]);
      Aver.IsTrue(qry0["strings"] is BSONArrayElement);
      var array = ((BSONArrayElement)qry0["strings"]).Value;
      Aver.IsNotNull(array);
      Aver.AreEqual(array.Length, 11);
      Aver.AreEqual(((BSONStringElement)array[0]).Value, "hello");
      Aver.AreEqual(((BSONStringElement)array[1]).Value, "привет");
      Aver.AreEqual(((BSONStringElement)array[2]).Value, "你好");
      Aver.AreEqual(((BSONStringElement)array[3]).Value, "こんにちは");
      Aver.AreEqual(((BSONStringElement)array[4]).Value, "γεια σας");
      Aver.AreEqual(((BSONStringElement)array[5]).Value, "përshëndetje");
      Aver.AreEqual(((BSONStringElement)array[6]).Value, "բարեւ Ձեզ");
      Aver.AreEqual(((BSONStringElement)array[7]).Value, "xin chào");
      Aver.AreEqual(((BSONStringElement)array[8]).Value, "Olá");
      Aver.AreEqual(((BSONStringElement)array[9]).Value, "Привіт");
      Aver.AreEqual(((BSONStringElement)array[10]).Value, "wünsche");
    }

    [Run]
    public void Templatization_ArrayOfUnicodeStringNames()
    {
      var qry0 = new BSONDocument("{ '$$eng': 'eng', '$$rus': 'rus', '$$chi': 'chi', '$$jap': 'jap', '$$gre': 'gre', '$$alb': 'alb', '$$arm': 'arm', '$$vie': 'vie', '$$por': 'por', '$$ukr': 'ukr', '$$ger': 'ger' }", true,
                           new TemplateArg("eng", BSONElementType.String, "hello"),
                           new TemplateArg("rus", BSONElementType.String, "привет"),
                           new TemplateArg("chi", BSONElementType.String, "你好"),
                           new TemplateArg("jap", BSONElementType.String, "こんにちは"),
                           new TemplateArg("gre", BSONElementType.String, "γεια σας"),
                           new TemplateArg("alb", BSONElementType.String, "përshëndetje"),
                           new TemplateArg("arm", BSONElementType.String, "բարեւ Ձեզ"),
                           new TemplateArg("vie", BSONElementType.String, "xin chào"),
                           new TemplateArg("por", BSONElementType.String, "Olá"),
                           new TemplateArg("ukr", BSONElementType.String, "Привіт"),
                           new TemplateArg("ger", BSONElementType.String, "wünsche"));

      Aver.AreEqual(qry0.Count, 11);
      Aver.IsNotNull(qry0["hello"]);
      Aver.IsTrue(qry0["hello"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["hello"]).Value, "eng");

      Aver.IsNotNull(qry0["привет"]);
      Aver.IsTrue(qry0["привет"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["привет"]).Value, "rus");

      Aver.IsNotNull(qry0["你好"]);
      Aver.IsTrue(qry0["你好"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["你好"]).Value, "chi");

      Aver.IsNotNull(qry0["こんにちは"]);
      Aver.IsTrue(qry0["こんにちは"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["こんにちは"]).Value, "jap");

      Aver.IsNotNull(qry0["γεια σας"]);
      Aver.IsTrue(qry0["γεια σας"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["γεια σας"]).Value, "gre");

      Aver.IsNotNull(qry0["përshëndetje"]);
      Aver.IsTrue(qry0["përshëndetje"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["përshëndetje"]).Value, "alb");

      Aver.IsNotNull(qry0["բարեւ Ձեզ"]);
      Aver.IsTrue(qry0["բարեւ Ձեզ"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["բարեւ Ձեզ"]).Value, "arm");

      Aver.IsNotNull(qry0["xin chào"]);
      Aver.IsTrue(qry0["xin chào"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["xin chào"]).Value, "vie");

      Aver.IsNotNull(qry0["Olá"]);
      Aver.IsTrue(qry0["Olá"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["Olá"]).Value, "por");

      Aver.IsNotNull(qry0["Привіт"]);
      Aver.IsTrue(qry0["Привіт"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["Привіт"]).Value, "ukr");

      Aver.IsNotNull(qry0["wünsche"]);
      Aver.IsTrue(qry0["wünsche"] is BSONStringElement);
      Aver.AreEqual(((BSONStringElement)qry0["wünsche"]).Value, "ger");
    }

    [Run]
    public void Templatization_ComplexObjectNoTemplate()
    {
      var qry0 = new BSONDocument(
        "{" +
          "item1: 23," +
          "item2: [1, 'こん好արüвіт', 123.456], " +
          "item3: { item31: false, item32: [true, true, false], item33: {} }," +
          "item4: {" +
            "item41: [1, 2, 3]," +
            "item42: false," +
            "item43: -123.4567," +
            "item44: 'こんこんвапаъü'," +
            "item45: { item451: [2], item452: true, item453: {} }" +
          "} "+
        "}", true);

      Aver.AreEqual(qry0.Count, 4);

      Aver.AreEqual(((BSONInt32Element)qry0["item1"]).Value, 23);

      var item2 = ((BSONArrayElement)qry0["item2"]).Value;
      Aver.AreEqual(item2.Length, 3);
      Aver.AreEqual(((BSONInt32Element)item2[0]).Value, 1);
      Aver.AreEqual(((BSONStringElement)item2[1]).Value, "こん好արüвіт");
      Aver.AreEqual(((BSONDoubleElement)item2[2]).Value, 123.456D);

      var item3 = ((BSONDocumentElement)qry0["item3"]).Value;
      Aver.AreEqual(item3.Count, 3);
      Aver.AreEqual(((BSONBooleanElement)item3["item31"]).Value, false);
      var arr = ((BSONArrayElement)item3["item32"]).Value;
      Aver.AreEqual(arr.Length, 3);
      Aver.AreEqual(((BSONBooleanElement)arr[0]).Value, true);
      Aver.AreEqual(((BSONBooleanElement)arr[1]).Value, true);
      Aver.AreEqual(((BSONBooleanElement)arr[2]).Value, false);
      var item33 = ((BSONDocumentElement)item3["item33"]).Value;
      Aver.AreEqual(item33.Count, 0);

      var item4 = ((BSONDocumentElement)qry0["item4"]).Value;
      Aver.AreEqual(item4.Count, 5);
      var item41 = ((BSONArrayElement)item4["item41"]).Value;
      Aver.AreEqual(item41.Length, 3);
      Aver.AreEqual(((BSONInt32Element)item41[0]).Value, 1);
      Aver.AreEqual(((BSONInt32Element)item41[1]).Value, 2);
      Aver.AreEqual(((BSONInt32Element)item41[2]).Value, 3);
      Aver.AreEqual(((BSONBooleanElement)item4["item42"]).Value, false);
      Aver.AreEqual(((BSONDoubleElement)item4["item43"]).Value, -123.4567D);
      Aver.AreEqual(((BSONStringElement)item4["item44"]).Value, "こんこんвапаъü");

      var item45 = ((BSONDocumentElement)item4["item45"]).Value;
      Aver.AreEqual(item45.Count, 3);
      var item451 = ((BSONArrayElement)item45["item451"]).Value;
      Aver.AreEqual(item451.Length, 1);
      Aver.AreEqual(((BSONInt32Element)item451[0]).Value, 2);
      Aver.AreEqual(((BSONBooleanElement)item45["item452"]).Value, true);

      var item453 =  ((BSONDocumentElement)item45["item453"]).Value;
      Aver.AreEqual(item453.Count, 0);
    }

    [Run]
    public void Templatization_QueryComplexObject()
    {
      var qry0 = new BSONDocument(
        "{" +
          "'$$item1': 23," +
          "item2: [1, '$$item21', 123.456], " +
          "'$$item3': { item31: '$$false', item32: '$$array', item33: {} }," +
          "'$$item4': {" +
            "'$$item41': [1, 2, 3]," +
            "'$$item42': false," +
            "item43: '$$double'," +
            "item44: 'こんこんвапаъü'," +
            "item45: { item451: '$$array2', item452: true, item453: {} }" +
          "} "+
        "}", true,
        new TemplateArg("item1", BSONElementType.String, "item1"),
        new TemplateArg("item21", BSONElementType.String, "こん好արüвіт"),
        new TemplateArg("item3", BSONElementType.String, "item3"),
        new TemplateArg("false", BSONElementType.Boolean, false),
        new TemplateArg("array", BSONElementType.Array,
          new BSONElement[]
          {
            new BSONBooleanElement(true),
            new BSONBooleanElement(true),
            new BSONBooleanElement(false)
          }),
        new TemplateArg("item4", BSONElementType.String, "item4"),
        new TemplateArg("item41", BSONElementType.String, "item41"),
        new TemplateArg("item42", BSONElementType.String, "item42"),
        new TemplateArg("double", BSONElementType.Double, -123.4567),
        new TemplateArg("array2", BSONElementType.Array,
          new BSONElement[]
          {
            new BSONInt64Element(2),
          })
        );

      Aver.AreEqual(qry0.Count, 4);

      Aver.AreEqual(((BSONInt32Element)qry0["item1"]).Value, 23);

      var item2 = ((BSONArrayElement)qry0["item2"]).Value;
      Aver.AreEqual(item2.Length, 3);
      Aver.AreEqual(((BSONInt32Element)item2[0]).Value, 1);
      Aver.AreEqual(((BSONStringElement)item2[1]).Value, "こん好արüвіт");
      Aver.AreEqual(((BSONDoubleElement)item2[2]).Value, 123.456D);

      var item3 = ((BSONDocumentElement)qry0["item3"]).Value;
      Aver.AreEqual(item3.Count, 3);
      Aver.AreEqual(((BSONBooleanElement)item3["item31"]).Value, false);
      var arr = ((BSONArrayElement)item3["item32"]).Value;
      Aver.AreEqual(arr.Length, 3);
      Aver.AreEqual(((BSONBooleanElement)arr[0]).Value, true);
      Aver.AreEqual(((BSONBooleanElement)arr[1]).Value, true);
      Aver.AreEqual(((BSONBooleanElement)arr[2]).Value, false);
      var item33 =  ((BSONDocumentElement)item3["item33"]).Value;
      Aver.AreEqual(item33.Count, 0);

      var item4 = ((BSONDocumentElement)qry0["item4"]).Value;
      Aver.AreEqual(item4.Count, 5);
      var item41 = ((BSONArrayElement)item4["item41"]).Value;
      Aver.AreEqual(item41.Length, 3);
      Aver.AreEqual(((BSONInt32Element)item41[0]).Value, 1);
      Aver.AreEqual(((BSONInt32Element)item41[1]).Value, 2);
      Aver.AreEqual(((BSONInt32Element)item41[2]).Value, 3);
      Aver.AreEqual(((BSONBooleanElement)item4["item42"]).Value, false);
      Aver.AreEqual(((BSONDoubleElement)item4["item43"]).Value, -123.4567D);
      Aver.AreEqual(((BSONStringElement)item4["item44"]).Value, "こんこんвапаъü");

      var item45 = ((BSONDocumentElement)item4["item45"]).Value;
      Aver.AreEqual(item45.Count, 3);
      var item451 = ((BSONArrayElement)item45["item451"]).Value;
      Aver.AreEqual(item451.Length, 1);
      Aver.AreEqual(((BSONInt64Element)item451[0]).Value, 2);
      Aver.AreEqual(((BSONBooleanElement)item45["item452"]).Value, true);

      var item453 =  ((BSONDocumentElement)item45["item453"]).Value;
      Aver.AreEqual(item453.Count, 0);
    }

    [Run]
    public void Templatization_QuerySinglePrimitiveTypes_Inference()
    {
      var qryInt = new BSONDocument("{ int: '$$value' }",    true, new TemplateArg("value", int.MinValue));
      var qryLong = new BSONDocument("{ long: '$$value' }",   true, new TemplateArg("value", long.MaxValue));
      var qryBool = new BSONDocument("{ bool: '$$value' }",   true, new TemplateArg("value", true));
      var qryDouble = new BSONDocument("{ double: '$$value' }", true, new TemplateArg("value", double.Epsilon));
      var qryString = new BSONDocument("{ string: '$$value' }", true, new TemplateArg("value", "string"));
      var qryArray = new BSONDocument("{ array: '$$value' }", true, new TemplateArg("value", new object[] { "string", int.MaxValue, false }));
      var gdid = new GDID(uint.MaxValue, GDID.AUTHORITY_MAX, GDID.COUNTER_MAX);
      var qryGDID = new BSONDocument("{ gdid: '$$value' }", true, new TemplateArg("value", gdid));
      var dec = 150666333000.1234M;
      var qryDecimal = new BSONDocument("{ decimal: '$$value' }", true, new TemplateArg("value", dec));
      var amount = new Amount("RUB", dec);
      var qryAmount = new BSONDocument("{ amount: '$$value' }", true, new TemplateArg("value", amount));

      Aver.AreEqual(qryInt.Count, 1);
      Aver.IsNotNull(qryInt["int"]);
      Aver.IsTrue(qryInt["int"] is BSONInt32Element);
      Aver.AreEqual(int.MinValue, qryInt["int"].ObjectValue.AsInt());

      Aver.AreEqual(qryLong.Count, 1);
      Aver.IsNotNull(qryLong["long"]);
      Aver.IsTrue(qryLong["long"] is BSONInt64Element);
      Aver.AreEqual(long.MaxValue, qryLong["long"].ObjectValue.AsLong());

      Aver.AreEqual(qryBool.Count, 1);
      Aver.IsNotNull(qryBool["bool"]);
      Aver.IsTrue(qryBool["bool"] is BSONBooleanElement);
      Aver.AreEqual(true, qryBool["bool"].ObjectValue.AsBool());

      Aver.AreEqual(qryDouble.Count, 1);
      Aver.IsNotNull(qryDouble["double"]);
      Aver.IsTrue(qryDouble["double"] is BSONDoubleElement);
      Aver.AreEqual(double.Epsilon, qryDouble["double"].ObjectValue.AsDouble());

      Aver.AreEqual(qryString.Count, 1);
      Aver.IsNotNull(qryString["string"]);
      Aver.IsTrue(qryString["string"] is BSONStringElement);
      Aver.AreEqual("string", qryString["string"].ObjectValue.AsString());

      Aver.AreEqual(qryArray.Count, 1);
      Aver.IsNotNull(qryArray["array"]);
      Aver.IsTrue(qryArray["array"] is BSONArrayElement);
      var elements = ((BSONArrayElement)qryArray["array"]).Value;
      Aver.IsNotNull(elements);
      Aver.AreEqual(elements.Length, 3);
      Aver.IsTrue(elements[0] is BSONStringElement);
      Aver.IsTrue(elements[1] is BSONInt32Element);
      Aver.IsTrue(elements[2] is BSONBooleanElement);
      Aver.AreEqual("string", elements[0].ObjectValue.AsString());
      Aver.AreEqual(int.MaxValue, elements[1].ObjectValue.AsInt());
      Aver.AreEqual(false, elements[2].ObjectValue.AsBool());

      Aver.AreEqual(qryGDID.Count, 1);
      Aver.IsNotNull(qryGDID["gdid"]);
      Aver.IsTrue(qryGDID["gdid"] is BSONBinaryElement);
      var binGDID = ((BSONBinaryElement)qryGDID["gdid"]).Value.Data;
      var expectedGDID = ((BSONBinaryElement)RowConverter.GDID_CLRtoBSON("gdid", gdid)).Value.Data;
      Aver.IsTrue(IOMiscUtils.MemBufferEquals(expectedGDID, binGDID));

      Aver.AreEqual(qryDecimal.Count, 1);
      Aver.IsNotNull(qryDecimal["decimal"]);
      Aver.IsTrue(qryDecimal["decimal"] is BSONInt64Element);
      Aver.AreObjectsEqual(RowConverter.Decimal_CLRtoBSON("decimal", dec).ObjectValue, qryDecimal["decimal"].ObjectValue);

      Aver.AreEqual(qryAmount.Count, 1);
      Aver.IsNotNull(qryAmount["amount"]);
      Aver.IsTrue(qryAmount["amount"] is BSONDocumentElement);
      var docAmount = ((BSONDocumentElement)qryAmount["amount"]).Value;
      Aver.AreEqual("RUB", docAmount["c"].ObjectValue.AsString());
      Aver.AreObjectsEqual(RowConverter.Decimal_CLRtoBSON("decimal", dec).ObjectValue, docAmount["v"].ObjectValue);
    }

    [Run]
    [Aver.Throws(typeof(BSONException), Message = StringConsts.BSON_TEMPLATE_ARG_DEPTH_EXCEEDED, MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
    public void Templatization_Recursive()
    {
      var map1 = new JSONDataMap { { "a", 1 } };
      var map2 = new JSONDataMap { { "b", 2 }, { "c", map1 } };
      map1.Add("d", map2);

      var qry = new BSONDocument("{ rec: '$$value' }", true, new TemplateArg("value", map1));
    }

    #endregion
  }
}