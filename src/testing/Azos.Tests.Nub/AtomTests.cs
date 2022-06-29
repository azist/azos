/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class AtomTests
  {
    [Run]
    public void Zero()
    {
      var x = new Atom(0);
      Aver.IsTrue( x.IsZero );
      Aver.AreEqual(0ul, x.ID);
      Aver.IsTrue( x.IsValid );
    }

    [Run]
    public void Zero_ToString_Value()
    {
      var x = new Atom(0);
      Aver.IsNull( x.ToString());
      Aver.IsNull( x.Value );
    }

    [Run]
    public void Zero_Equality()
    {
      var x = new Atom(0);
      var y = new Atom(0);
      Aver.AreEqual(x, y);
      Aver.IsTrue(x==y);
    }

    [Run]
    public void Zero_InEquality()
    {
      var x = new Atom(0);
      var y = new Atom(1);
      Aver.AreNotEqual(x, y);
      Aver.IsTrue(x != y);
    }

    [Run]
    public void Test_ToString()
    {
      var x = new Atom(0x3041304130413041ul);
      Aver.AreEqual("A0A0A0A0", x.ToString());
    }

    [Run]
    public void Encode_ToString()
    {
      var x = Atom.Encode("ALEX");
      Aver.AreEqual("ALEX", x.ToString());
    }

    [Run]
    public void Encode_Decode()
    {
      var x = Atom.Encode("ALEX1234");
      var y = new Atom(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Hashcodes()
    {
      var x = Atom.Encode("ALEX1234");
      var y = new Atom(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.AreEqual(x.GetDistributedStableHash(), y.GetDistributedStableHash());
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Encode_Null()
    {
      var x = Atom.Encode(null);
      Aver.AreEqual(0ul, x.ID);
      Aver.IsTrue(x.IsZero);
      Aver.IsNull(x.ToString());
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "blank")]
    public void Error_Empty()
    {
       var x = Atom.Encode("");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "1 and 8")]
    public void Error_ToLong()
    {
      var x = Atom.Encode("123456789");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "![0..9")]
    public void Error_NonAscii()
    {
      var x = Atom.Encode("ag²■");
    }

    [Run]
    public void VarLength()
    {
      var x = Atom.Encode("a");
      x.ToString().See();
      var y = Atom.Encode("ab");
      y.ToString().See();

      Aver.AreEqual(0x61ul, x.ID);
      Aver.AreEqual(0x6261ul, y.ID);
    }

    [Run]
    public void IsValid()
    {
      var x = new Atom(0);
      Aver.IsTrue(x.IsZero);
      Aver.IsTrue(x.IsValid);

      x = new Atom(0xffff);
      Aver.IsFalse(x.IsValid);
      Aver.Throws<AzosException>(()=>x.Value.ToLower());
    }

    [Run]
    public void ValueInterning()
    {
      var x = Atom.Encode("abc");
      var y = Atom.Encode("abc");
      var z = new Atom(x.ID);

      Aver.AreEqual(x, y);
      Aver.AreEqual(x, z);

      Aver.AreEqual("abc", x.Value);
      Aver.AreEqual("abc", y.Value);
      Aver.AreEqual("abc", z.Value);

      Aver.AreNotSameRef("abc", x.Value);
      Aver.AreSameRef(x.Value, y.Value);
      Aver.AreSameRef(x.Value, z.Value);
    }

    [Run]
    public void TryEncode()
    {
      Atom x;
      Aver.IsTrue( Atom.TryEncode("abc", out x) );
      Aver.AreEqual("abc", x.Value);

      Aver.IsFalse(Atom.TryEncode("ab * c", out x));
      Aver.IsTrue(x.IsZero);
      Aver.AreEqual(null, x.Value);
    }

    [Run]
    public void TryEncodeValueOrId()
    {
      Atom x;
      Aver.IsTrue(Atom.TryEncodeValueOrId("abc", out x));
      Aver.AreEqual("abc", x.Value);

      Aver.IsFalse(Atom.TryEncodeValueOrId("ab * c", out x));
      Aver.IsTrue(x.IsZero);
      Aver.AreEqual(null, x.Value);

      Aver.IsTrue(Atom.TryEncodeValueOrId("#0x3031", out x));
      Aver.AreEqual("10", x.Value);

      Aver.IsTrue(Atom.TryEncodeValueOrId("#12337", out x));
      Aver.AreEqual("10", x.Value);
    }

    [Run]
    public void Test_Length()
    {
      Aver.AreEqual(0, new Atom().Length);
      Aver.AreEqual(0, new Atom(0).Length);

      var x = Atom.Encode("a");
      Aver.AreEqual(1, x.Length);

      x = Atom.Encode("abc");
      Aver.AreEqual(3, x.Length);

      x = Atom.Encode("abcdef");
      Aver.AreEqual(6, x.Length);

      x = Atom.Encode("abc-def");
      Aver.AreEqual(7, x.Length);

      x = Atom.Encode("abc-def0");
      Aver.AreEqual(8, x.Length);

      x = new Atom(0xFFFFFFFFFFFFFFFFul);
      Aver.AreEqual(8, x.Length);
      Aver.IsFalse(x.IsValid);

      x = new Atom(0xFFul);
      Aver.AreEqual(1, x.Length);
      Aver.IsFalse(x.IsValid);

      x = new Atom(0xFF0101ul);
      Aver.AreEqual(3, x.Length);
      Aver.IsFalse(x.IsValid);
    }

    [Run]
    public void Test_Validation001()
    {
      var x = Atom.Encode("abc");
      var v = new Data.ValidState(null, Data.ValidErrorMode.Batch);
      v = x.Validate(v);

      Aver.IsFalse(v.HasErrors);
    }

    [Run]
    public void Test_Validation002()
    {
      var x = new Atom(0xfffffff);
      var v = new Data.ValidState(null, Data.ValidErrorMode.Batch);
      v = x.Validate(v);

      Aver.IsTrue(v.HasErrors);
      v.Error.See();
    }

    public class Tezt : TypedDoc
    {
      [Field]                  public Atom F1{ get; set;}
      [Field(Required = true)] public Atom F2 { get; set; }
      [Field(MinLength = 3)]   public Atom F3 { get; set; }
      [Field(MaxLength = 2)]   public Atom F4 { get; set; }
    }

    [Run]
    public void Test_Validation003()
    {
      var d = new Tezt
      {
        F1 = new Atom(0xfffff),
        F2 = Atom.Encode("a"),
        F3 = Atom.Encode("ab"),
        F4 = Atom.Encode("abcd"),
      };

      var state = d.Validate(new ValidState(null, ValidErrorMode.Batch));
      Aver.IsTrue(state.HasErrors);

      var batch = state.Error as ValidationBatchException;
      Aver.IsNotNull(batch);

      var errors = batch.Batch.OfType<FieldValidationException>();

      errors.See();

      Aver.AreEqual(3, errors.Count());

      Aver.IsTrue(errors.First( e => e.FieldName.EqualsOrdSenseCase("F1") ).ClientMessage.Contains("Invalid"));
      Aver.IsTrue(errors.First( e => e.FieldName.EqualsOrdSenseCase("F3") ).ClientMessage.Contains("shorter"));
      Aver.IsTrue(errors.First( e => e.FieldName.EqualsOrdSenseCase("F4") ).ClientMessage.Contains("exceeds"));

      d.F2 = new Atom();

      state = d.Validate(new ValidState(null, ValidErrorMode.Batch));
      Aver.IsTrue(state.HasErrors);

      batch = state.Error as ValidationBatchException;
      Aver.IsNotNull(batch);

      errors = batch.Batch.OfType<FieldValidationException>();

      errors.See();

      Aver.AreEqual(4, errors.Count());
      Aver.IsTrue(errors.First(e => e.FieldName.EqualsOrdSenseCase("F2")).ClientMessage.Contains("required"));
    }


  }
}
