/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;

using Azos.Erlang;

namespace Azos.Tests.Unit.Erlang
{
  [Runnable]
  public class ErlTermFixture
  {
    private static readonly ErlAtom A = new ErlAtom("A");
    private static readonly ErlAtom B = new ErlAtom("B");
    private static readonly ErlAtom M = new ErlAtom("M");
    private static readonly ErlAtom N = new ErlAtom("N");
    private static readonly ErlAtom X = new ErlAtom("X");

    [Run]
    public void AtomTableTest()
    {
      Aver.AreEqual(0, AtomTable.Instance[string.Empty]);

      Aver.AreEqual(1, AtomTable.Instance["true"]);

      Aver.AreEqual(2, AtomTable.Instance["false"]);

      Aver.AreEqual(1, ErlAtom.True.Index);
      Aver.AreEqual(2, ErlAtom.False.Index);

      bool found = AtomTable.Instance.IndexOf("ok") != -1;
      int count = AtomTable.Instance.Count;

      var am_ok = new ErlAtom("ok");

      Aver.AreEqual(found ? am_ok.Index : AtomTable.Instance.Count - 1, am_ok.Index);
      Aver.AreEqual(found ? count : count + 1, AtomTable.Instance.Count);
    }

    [Run]
    public void ErlAtomTest()
    {
      var am_test = new ErlAtom("test");
      Aver.IsTrue(am_test.Equals(new ErlAtom("test")));
      Aver.AreObjectsEqual(am_test, new ErlAtom("test"));
      Aver.AreEqual("test", am_test.Value);
      Aver.AreEqual("test", am_test.ToString());
      Aver.IsTrue(am_test.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlAtom == am_test.TypeOrder);

      Aver.IsTrue(am_test.Matches(new ErlAtom("test")));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(am_test.Match(new ErlAtom("test"))));

      var am_Test = new ErlAtom("Test");
      Aver.AreEqual("'Test'", am_Test.ToString());
      Aver.AreEqual(4, am_Test.Length);
      Aver.AreObjectsNotEqual(am_test, am_Test);

      IErlObject temp = null;
      Aver.IsFalse(am_test.Subst(ref temp, new ErlVarBind()));

      Aver.IsTrue(am_Test.Visit(true, (acc, o) => acc));

      doesNotThrow(() => { var x = am_test.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsDouble; });
      doesNotThrow(() => { var x = am_test.ValueAsString; });
      doesNotThrow(() => { var x = am_test.ValueAsBool; });
      Aver.AreEqual('a', new ErlAtom("a").ValueAsChar);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = am_test.ValueAsByteArray; });

      string s = am_test;  // Implicit conversion
      Aver.AreEqual("test", s);

      ErlAtom a = "abc";   // Implicit conversion
      Aver.AreEqual("abc", a.Value);
    }

    [Run]
    public void ErlBinaryTest()
    {
      {
        var tt = new byte[] { 10, 11, 12 };
        var t1 = new ErlBinary(tt, false);
        tt[0] = 20;
        Aver.AreEqual(20, t1.Value[0]);

        var bb = new byte[] { 10, 11, 12 };
        var t2 = new ErlBinary(bb);
        bb[0] = 20;
        Aver.AreEqual(10, t2.Value[0]);
      }

      var tb = new byte[] { 1, 2, 3 };
      var t = new ErlBinary(tb);

      Aver.IsTrue(t.Equals(new ErlBinary(new byte[] { 1, 2, 3 })));
      Aver.AreObjectsEqual(t, new ErlBinary(tb));
      Aver.IsTrue(new ErlBinary(new byte[] { 1, 2 }).CompareTo(t) < 0);
      Aver.IsTrue(tb.MemBufferEquals(t.Value));
      Aver.IsTrue(t.ValueAsBool);
      Aver.IsFalse(new ErlBinary(new byte[] { }).ValueAsBool);
      Aver.AreEqual("<<1,2,3>>", t.ToString());
      Aver.AreEqual("<<1,2,3>>", t.ToBinaryString());
      Aver.IsFalse(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlBinary == t.TypeOrder);

      var bbb = new ErlBinary(new byte[] { 97, 98, 99, 10, 49, 50, 51 });
      Aver.AreEqual("<<\"abc\n123\">>", bbb.ToString());
      Aver.AreEqual("<<\"abc\n123\">>", bbb.ToPrintableString());
      Aver.AreEqual("<<\"abc...\">>", bbb.ToPrintableString(6));
      Aver.AreEqual("<<97,98,99,10,49,50,51>>", bbb.ToBinaryString());
      Aver.AreEqual("<<97,98...>>", bbb.ToBinaryString(10));

      Aver.IsTrue(t.Matches(new ErlBinary(new byte[] { 1, 2, 3 })));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlBinary(new byte[] { 1, 2, 3 }))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      doesNotThrow(() => { var x = t.ValueAsString; });
      doesNotThrow(() => { var x = t.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      doesNotThrow(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(3, t.Visit(0, (acc, o) => acc + ((ErlBinary)o).Length));

      byte[] b = t; Aver.IsTrue(tb.MemBufferEquals(b));
    }

    [Run]
    public void ErlBooleanTest()
    {
      var t = new ErlBoolean(true);
      Aver.IsTrue(t.Equals(new ErlBoolean(true)));
      Aver.IsFalse(t.Equals(new ErlBoolean(false)));
      Aver.AreObjectsEqual(t, new ErlBoolean(true));
      Aver.AreEqual(-1, new ErlBoolean(false).CompareTo(t));
      Aver.AreEqual(true, t.Value);
      Aver.AreEqual(1, t.ValueAsInt);
      Aver.AreEqual(1, t.ValueAsLong);
      Aver.AreEqual("true", t.ToString());
      Aver.AreEqual("false", new ErlBoolean(false).ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlBoolean == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlBoolean(true)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlBoolean(true))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.AreEqual(1, t.ValueAsInt);
      Aver.AreEqual(1, t.ValueAsLong);
      Aver.AreEqual(1, t.ValueAsDecimal);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.AreEqual(1.0, t.ValueAsDouble);
      Aver.AreEqual("True", t.ValueAsString);
      Aver.AreEqual(true, t.ValueAsBool);
      Aver.AreEqual('T', t.ValueAsChar);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.IsTrue(t.Visit(false, (acc, o) => o.ValueAsBool));

      bool n = t;             // Implicit conversion
      Aver.AreEqual(true, n);
      ErlBoolean a = true;    // Implicit conversion
      Aver.AreEqual(true, a.Value);
    }

    [Run]
    public void ErlByteTest()
    {
      var t = new ErlByte(10);
      Aver.IsTrue(t.Equals(new ErlByte(10)));
      Aver.AreObjectsEqual(t, new ErlByte(10));
      Aver.IsTrue(new ErlByte(1).CompareTo(t) < 0);
      Aver.AreEqual(10, t.Value);
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual("10", t.ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlByte == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlByte(10)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlByte(10))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual(10, t.ValueAsDecimal);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.AreEqual(10.0, t.ValueAsDouble);
      Aver.AreEqual("10", t.ValueAsString);
      Aver.AreEqual(true, t.ValueAsBool);
      Aver.AreEqual('\n', t.ValueAsChar);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(10, t.Visit(0, (acc, o) => o.ValueAsInt));

      char n = (char)t; Aver.AreEqual('\n', n);
      byte m = t; Aver.AreEqual(10, m);
      ErlByte b = 10; Aver.AreEqual(10, b.Value);
      ErlByte k = (ErlByte)10; Aver.AreEqual(10, k.Value);
      ErlByte z = (ErlByte)'\n'; Aver.AreEqual(10, k.Value);

      {
        var bind = new ErlVarBind();
        Aver.IsTrue(b.Match(new ErlLong(10), bind));
        Aver.IsTrue(new ErlLong(10).Match(b, bind));
        b = 111;
        Aver.IsTrue(b.Match(new ErlLong(111), bind));
        Aver.IsTrue(new ErlLong(111).Match(b, bind));
      }
    }

    [Run]
    public void ErlDoubleTest()
    {
      var t = new ErlDouble(10.128d);
      Aver.IsTrue(t.Equals(new ErlDouble(10.128d)));
      Aver.AreEqual(1, t.CompareTo(new ErlDouble(-1.1)));
      Aver.AreObjectsEqual(t, new ErlDouble(10.128d));
      Aver.AreEqual(0, t.CompareTo(new ErlDouble(10.128d)));
      Aver.IsTrue(10.128d == t);
      Aver.IsTrue(t == 10.128d);
      Aver.AreEqual(10.128d, t.Value);
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual(10.128d, t.ValueAsDouble);
      Aver.AreEqual("10.128", t.ToString());
      Aver.AreEqual("1.1", new ErlDouble(1.1).ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlDouble == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlDouble(10.128d)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlDouble(10.128d))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual(10.128m, t.ValueAsDecimal);
      doesNotThrow(() => { var x = t.ValueAsDateTime; });
      doesNotThrow(() => { var x = t.ValueAsTimeSpan; });
      Aver.AreEqual(10.128d, t.ValueAsDouble);
      Aver.AreEqual("10.128", t.ValueAsString);
      Aver.AreEqual(true, t.ValueAsBool);
      Aver.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(10 * 10), t.ValueAsDateTime);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(10.128d, t.Visit(0.0, (acc, o) => o.ValueAsDouble));

      double n = t;             // Implicit conversion
      Aver.AreEqual(10.128d, n);
      ErlDouble a = 10.128d;    // Implicit conversion
      Aver.AreEqual(10.128d, a.Value);
    }

    [Run]
    public void ErlListTest()
    {
      var l = new ErlList("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));
      var r = new ErlList("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));

      Aver.AreEqual(7, l.Count);
      Aver.IsTrue(ErlTypeOrder.ErlString == l[0].TypeOrder);
      Aver.AreEqual("test", l[0].ValueAsString);
      Aver.IsTrue(ErlTypeOrder.ErlLong == l[1].TypeOrder);
      Aver.AreEqual(1, l[1].ValueAsInt);
      Aver.IsTrue(ErlTypeOrder.ErlDouble == l[2].TypeOrder);
      Aver.AreEqual(1.1, l[2].ValueAsDouble);
      Aver.IsTrue(ErlTypeOrder.ErlBoolean == l[3].TypeOrder);
      Aver.AreEqual(true, l[3].ValueAsBool);
      Aver.IsTrue(ErlTypeOrder.ErlByte == l[4].TypeOrder);
      Aver.AreEqual(255, l[4].ValueAsInt);
      Aver.IsTrue(ErlTypeOrder.ErlByte == l[5].TypeOrder);
      Aver.AreEqual('x', l[5].ValueAsChar);
      Aver.IsTrue(ErlTypeOrder.ErlAtom == l[6].TypeOrder);
      Aver.AreEqual("a", l[6].ValueAsString);

      Aver.IsTrue(l.Matches(r));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(l.Match(r)));

      Aver.AreObjectsEqual(l, r);
      Aver.IsTrue(l.Equals(r));
      Aver.AreEqual("[\"test\",1,1.1,true,255,120,a]", l.ToString());
      Aver.IsFalse(l.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlList == l.TypeOrder);

      IErlObject temp = null;
      Aver.IsFalse(l.Subst(ref temp, new ErlVarBind()));
      Aver.IsTrue(new ErlList(new ErlVar(X), true, 1).Subst(ref temp, new ErlVarBind { { X, new ErlLong(10) } }));
      Aver.AreEqual("[10,true,1]", temp.ToString());

      Aver.AreEqual(1, l.Visit(0, (acc, o) => acc + (o is ErlAtom ? 1 : 0)));

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      doesNotThrow(() => { var x = l.ValueAsObject; });
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsInt);
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsLong);
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsDecimal);
      Aver.AreEqual(d, new ErlList(d.ToString())[0].ValueAsDateTime);
      Aver.AreEqual(ts, new ErlList(ts.ToString())[0].ValueAsTimeSpan);
      Aver.AreEqual(1.0, new ErlList("1.0")[0].ValueAsDouble);
      Aver.AreEqual("a", new ErlList("a")[0].ValueAsString);
      Aver.IsTrue(new ErlList("true")[0].ValueAsBool);
      Aver.IsFalse(new ErlList("xxxx")[0].ValueAsBool);

      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDouble; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsString; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsByteArray; });

      List<IErlObject> s = l;
      Aver.AreObjectsEqual(l.Value, s);
      Aver.IsFalse(new ErlTuple(1, 1.0, "a").Equals(new ErlList(1, 1.0, "a")));
      Aver.IsFalse(new ErlTuple(1, 1.0, "a") == new ErlList(1, 1.0, "a"));
    }

    [Run]
    public void ErlLongTest()
    {
      var t = new ErlLong(10);
      Aver.IsTrue(t.Equals(new ErlLong(10)));
      Aver.AreObjectsEqual(t, new ErlLong(10));
      Aver.AreEqual(-1, new ErlAtom("ok").CompareTo(t));
      Aver.IsTrue(10 == t);
      Aver.IsTrue((byte)10 == t);
      Aver.IsTrue((long)10 == t);
      Aver.AreEqual(10, t.Value);
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual("10", t.ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlLong == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlLong(10)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlLong(10))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.AreEqual(10, t.ValueAsInt);
      Aver.AreEqual(10, t.ValueAsLong);
      Aver.AreEqual(10, t.ValueAsDecimal);
      Aver.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(10 * 10), t.ValueAsDateTime);
      Aver.AreEqual(new TimeSpan(0, 0, 10), t.ValueAsTimeSpan);
      Aver.AreEqual(10.0, t.ValueAsDouble);
      Aver.AreEqual("10", t.ValueAsString);
      Aver.AreEqual(true, t.ValueAsBool);
      Aver.AreEqual('\n', t.ValueAsChar);
      Aver.Throws<ErlException>(() => { var x = new ErlLong(256).ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });


      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(10, t.Visit(0, (acc, o) => acc + o.ValueAsInt));

      int n = (int)t; Aver.AreEqual(10, n);
      long m = t; Aver.AreEqual(10, m);
      ErlLong a = 100;        // Implicit conversion
      Aver.AreEqual(100, a.Value);

      Aver.AreObjectsEqual(new ErlByte(127), new ErlLong(127));
      Aver.AreObjectsEqual(new ErlByte(255), new ErlLong(255));
      Aver.AreObjectsEqual(new ErlByte(0), new ErlLong(0));
    }

    [Run]
    public void ErlPidTest()
    {
      var t = new ErlPid("test", 10, 3, 1);
      Aver.AreEqual("test", t.Node.Value);
      Aver.IsTrue(t.Equals(new ErlPid("test", 10, 3, 1)));
      Aver.AreObjectsEqual(t, new ErlPid("test", 10, 3, 1));
      Aver.AreEqual(1, new ErlPid("tesu", 10, 3, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPid("tess", 10, 3, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPid("test", 9, 3, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlPid("test", 12, 4, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPid("test", 10, 2, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlPid("test", 10, 4, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPid("test", 10, 3, 0).CompareTo(t));
      Aver.AreEqual(1, new ErlPid("test", 10, 3, 2).CompareTo(t));
      Aver.AreEqual("#Pid<test.10.3.1>", t.ToString());
      Aver.AreEqual("#Pid<test.10.3.1>", t.ValueAsString);
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlPid == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlPid("test", 10, 3, 1)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlPid("test", 10, 3, 1))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Aver.AreEqual("#Pid<test.10.3.1>", t.ValueAsString);


      var r = ErlPid.Parse("#Pid<test.10.3.1>");
      Aver.AreObjectsEqual(new ErlAtom("test"), r.Node);
      Aver.AreEqual(10, r.Id);
      Aver.AreEqual(3, r.Serial);
      Aver.AreEqual(1, r.Creation);


      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      Aver.IsTrue(new ErlPid("test", 0, 0, 0).Empty);

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(true, t.Visit(false, (acc, o) => acc |= o is ErlPid));
    }

    [Run]
    public void ErlPortTest()
    {
      var t = new ErlPort("test", 10, 1);
      Aver.IsTrue(t.Equals(new ErlPort("test", 10, 1)));
      Aver.AreObjectsEqual(t, new ErlPort("test", 10, 1));
      Aver.AreEqual(1, new ErlPort("tesu", 10, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPort("tess", 10, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPort("test", 9, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlPort("test", 12, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlPort("test", 10, 0).CompareTo(t));
      Aver.AreEqual(1, new ErlPort("test", 10, 2).CompareTo(t));
      Aver.AreEqual("#Port<test.10>", t.ToString());
      Aver.AreEqual("#Port<test.10>", t.ValueAsString);
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlPort == t.TypeOrder);

      Aver.IsTrue(t.Matches(new ErlPort("test", 10, 1)));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlPort("test", 10, 1))));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Aver.AreEqual("#Port<test.10>", t.ValueAsString);
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.IsTrue(t.Visit(false, (acc, o) => acc |= o is ErlPort));
    }

    [Run]
    public void ErlRefTest()
    {
      var ids = new int[] { 5, 6, 7 };
      var t = new ErlRef("test", 5, 6, 7, 1);
      var t1 = new ErlRef("test", ids, 1);

      Aver.AreObjectsEqual(t, t1);

      Aver.IsTrue(t.Equals(new ErlRef("test", ids, 1)));
      Aver.AreObjectsEqual(t, new ErlRef("test", ids, 1));
      Aver.AreEqual(1, new ErlRef("tesu", new int[] { 5, 6, 7 }, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlRef("tess", new int[] { 5, 6, 7 }, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlRef("test", new int[] { 4, 6, 7 }, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlRef("test", new int[] { 8, 6, 7 }, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlRef("test", new int[] { 5, 4, 7 }, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlRef("test", new int[] { 5, 8, 7 }, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlRef("test", new int[] { 5, 6, 4 }, 1).CompareTo(t));
      Aver.AreEqual(1, new ErlRef("test", new int[] { 5, 6, 9 }, 1).CompareTo(t));
      Aver.AreEqual(-1, new ErlRef("test", new int[] { 5, 6, 7 }, 0).CompareTo(t));
      Aver.AreEqual(1, new ErlRef("test", new int[] { 5, 6, 7 }, 2).CompareTo(t));
      Aver.AreEqual("#Ref<test.5.6.7.1>", t.ToString());
      Aver.AreEqual("#Ref<test.5.6.7.1>", t.ValueAsString);
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlRef == t.TypeOrder);

      Aver.IsTrue(t.Matches(t1));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(t1)));

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Aver.AreEqual("#Ref<test.5.6.7.1>", t.ValueAsString);

      var r = ErlRef.Parse("#Ref<test.5.6.7.1>");
      Aver.AreObjectsEqual(new ErlAtom("test"), r.Node);
      Aver.IsTrue(5 == r.Ids[0]);
      Aver.IsTrue(6 == r.Ids[1]);
      Aver.IsTrue(7 == r.Ids[2]);
      Aver.IsTrue(1 == r.Creation);

      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.IsTrue(t.Visit(false, (acc, o) => acc |= o is ErlRef));
    }

    [Run]
    public void ErlStringTest()
    {
      var t = new ErlString("test");
      Aver.IsTrue(t.Equals(new ErlString("test")));
      Aver.AreObjectsEqual(t, new ErlString("test"));
      Aver.AreEqual("test", t.Value);
      Aver.AreEqual("\"test\"", t.ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlString == t.TypeOrder);

      Aver.AreNotEqual("test", new ErlString("Test").ToString());

      Aver.IsTrue(t.Matches(new ErlString("test")));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(t.Match(new ErlString("test"))));

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(4, t.Visit(0, (acc, o) => acc + t.ValueAsString.Length));

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      doesNotThrow(() => { var x = t.ValueAsObject; });
      Aver.AreEqual(1, new ErlString("1").ValueAsInt);
      Aver.AreEqual(1, new ErlString("1").ValueAsLong);
      Aver.AreEqual(1, new ErlString("1").ValueAsDecimal);
      Aver.AreEqual(d, new ErlString(d.ToString()).ValueAsDateTime);
      Aver.AreEqual(ts, new ErlString(ts.ToString()).ValueAsTimeSpan);
      Aver.AreEqual(1.0, new ErlString("1.0").ValueAsDouble);
      Aver.AreEqual("a", new ErlString("a").ValueAsString);
      Aver.IsTrue(new ErlString("true").ValueAsBool);
      Aver.IsFalse(new ErlString("xxxx").ValueAsBool);
      Aver.AreEqual('a', new ErlString("a").ValueAsChar);
      Aver.Throws<ErlException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });

      string s = t;           // Implicit conversion
      Aver.AreEqual("test", s);
      ErlString a = "abc";    // Implicit conversion
      Aver.AreEqual("abc", a.Value);
    }

    [Run]
    public void ErlTupleTest()
    {
      var l = new ErlTuple("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));
      var r = new ErlTuple("test", 1, 1.1, true, (byte)255, 'x', new ErlAtom("a"));

      Aver.AreEqual(7, l.Count);
      Aver.IsTrue(ErlTypeOrder.ErlString == l[0].TypeOrder);
      Aver.AreEqual("test", l[0].ValueAsString);
      Aver.IsTrue(ErlTypeOrder.ErlLong == l[1].TypeOrder);
      Aver.AreEqual(1, l[1].ValueAsInt);
      Aver.IsTrue(ErlTypeOrder.ErlDouble == l[2].TypeOrder);
      Aver.AreEqual(1.1, l[2].ValueAsDouble);
      Aver.IsTrue(ErlTypeOrder.ErlBoolean == l[3].TypeOrder);
      Aver.AreEqual(true, l[3].ValueAsBool);
      Aver.IsTrue(ErlTypeOrder.ErlByte == l[4].TypeOrder);
      Aver.AreEqual(255, l[4].ValueAsInt);
      Aver.IsTrue(ErlTypeOrder.ErlByte == l[5].TypeOrder);
      Aver.AreEqual('x', l[5].ValueAsChar);
      Aver.IsTrue(ErlTypeOrder.ErlAtom == l[6].TypeOrder);
      Aver.AreEqual("a", l[6].ValueAsString);

      Aver.IsTrue(l.Matches(r));
      Aver.IsTrue(new ErlVarBind().SequenceEqual(l.Match(r)));

      Aver.AreObjectsEqual(l, r);
      Aver.IsTrue(l.Equals(r));
      Aver.AreEqual("{\"test\",1,1.1,true,255,120,a}", l.ToString());
      Aver.IsFalse(l.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlTuple == l.TypeOrder);

      IErlObject temp = null;
      Aver.IsFalse(l.Subst(ref temp, new ErlVarBind()));
      Aver.AreEqual(1, l.Visit(0, (acc, o) => acc + (o is ErlAtom ? 1 : 0)));
      Aver.IsTrue(new ErlTuple(new ErlVar(X), true, 1).Subst(ref temp, new ErlVarBind { { X, new ErlLong(10) } }));
      Aver.AreEqual("{10,true,1}", temp.ToString());

      var d = new DateTime(2013, 1, 2);
      var ts = new TimeSpan(1, 2, 3);

      doesNotThrow(() => { var x = l.ValueAsObject; });
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsInt);
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsLong);
      Aver.AreEqual(1, new ErlList("1")[0].ValueAsDecimal);
      Aver.AreEqual(d, new ErlList(d.ToString())[0].ValueAsDateTime);
      Aver.AreEqual(ts, new ErlList(ts.ToString())[0].ValueAsTimeSpan);
      Aver.AreEqual(1.0, new ErlList("1.0")[0].ValueAsDouble);
      Aver.AreEqual("a", new ErlList("a")[0].ValueAsString);
      Aver.IsTrue(new ErlList("true")[0].ValueAsBool);
      Aver.IsFalse(new ErlList("xxxx")[0].ValueAsBool);

      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsDouble; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsString; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = l.ValueAsByteArray; });


      List<IErlObject> s = l;
      Aver.AreObjectsEqual(l.Value, s);

      Aver.IsFalse(new ErlList(1, 1.0, "a").Equals(new ErlTuple(1, 1.0, "a")));
      Aver.IsFalse(new ErlList(1, 1.0, "a") == new ErlTuple(1, 1.0, "a"));
      Aver.IsTrue(new ErlList(1, 1.0, "a") == new ErlList(1, 1.0, "a"));
    }

    [Run]
    public void ErlVarTest()
    {
      var t = ErlVar.Any;
      Aver.IsFalse(t.Equals(new ErlVar(ConstAtoms.ANY)));
      Aver.AreObjectsEqual(ConstAtoms.ANY, t.Name);
      Aver.IsTrue(ErlTypeOrder.ErlObject == t.ValueType);

      t = new ErlVar(N, ErlTypeOrder.ErlLong);
      Aver.AreEqual("N", t.Name.Value);
      Aver.IsTrue(ErlTypeOrder.ErlLong == t.ValueType);

      {
        var bind = new ErlVarBind();
        Aver.IsTrue(t.Match(new ErlByte(10), bind));
        Aver.AreEqual(10, bind["N"].ValueAsLong);
        bind.Clear();
        var q = new ErlVar("N", ErlTypeOrder.ErlByte);
        Aver.IsTrue(q.Match(new ErlLong(111), bind));
        Aver.AreEqual(111, bind["N"].ValueAsLong);
      }

      Aver.IsFalse(t.Matches(new ErlVar()));
      Aver.IsFalse(new ErlVar(A).Matches(new ErlVar(A)));
      Aver.IsFalse(new ErlVar(A).Matches(new ErlVar(B)));
      Aver.IsTrue(new ErlVarBind { { N, (ErlLong)10 } }.SequenceEqual(t.Match((ErlLong)10)));
      Aver.IsTrue(new ErlVarBind { { A, (ErlLong)10 } }.SequenceEqual(new ErlVar(A).Match((ErlLong)10)));

      Aver.AreEqual(-1, new ErlAtom("ok").CompareTo(t));
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsObject; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsInt; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsLong; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDecimal; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDateTime; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsTimeSpan; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsDouble; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsString; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsBool; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsChar; });
      Aver.Throws<ErlIncompatibleTypesException>(() => { var x = t.ValueAsByteArray; });
      Aver.AreEqual("N::int()", t.ToString());
      Aver.IsTrue(t.IsScalar);
      Aver.IsTrue(ErlTypeOrder.ErlVar == t.TypeOrder);

      IErlObject temp = null;
      Aver.IsFalse(t.Subst(ref temp, new ErlVarBind { { M, new ErlLong(100) } }));
      Aver.IsTrue(t.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));
      Aver.AreEqual(new ErlLong(100), temp);

      temp = new ErlVar(M, ErlTypeOrder.ErlLong);
      Aver.IsTrue(temp.Subst(ref temp, new ErlVarBind { { M, new ErlLong(100) } }));
      Aver.IsTrue(ErlTypeOrder.ErlVar == t.Visit(ErlTypeOrder.ErlByte, (acc, o) => ((ErlVar)o).TypeOrder));
      Aver.AreEqual(new ErlLong(100), temp);

      temp = new ErlVar(N, ErlTypeOrder.ErlObject);
      Aver.IsTrue(temp.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));

      // Invalid variable type
      temp = new ErlVar(N, ErlTypeOrder.ErlAtom);
      Aver.Throws<ErlException>(() => temp.Subst(ref temp, new ErlVarBind { { N, new ErlLong(100) } }));
    }

    [Run]
    public void ErlVarBindTest()
    {
      var bind = new ErlVarBind
            {
                {new ErlVar("A", ErlTypeOrder.ErlLong),    10},
                {new ErlVar("B", ErlTypeOrder.ErlAtom), "abc"},
                { "C", ErlTypeOrder.ErlDouble, 5}
            };

      Aver.AreEqual((IErlObject)(new ErlLong(10)), bind["A"]);
      Aver.AreEqual((IErlObject)(new ErlAtom("abc")), bind["B"]);
      Aver.AreEqual((IErlObject)(new ErlDouble(5.0)), bind["C"]);

      var term = Azos.Erlang.ErlObject.Parse("{ok, {A::int(), [sasha, B::atom(), C::float()]}}");
      var set = term.Visit(new SortedSet<ErlVar>(), (a, o) => { if (o is ErlVar) a.Add((ErlVar)o); return a; });

      IErlObject res = null;
      Aver.IsTrue(term.Subst(ref res, bind));
      Aver.AreEqual("{ok, {10, [sasha, abc, 5.0]}}".ToErlObject(), res);
    }

    [Run]
    public void ErlTypeConversionTest()
    {
      Aver.AreEqual((IErlObject)new ErlAtom("abc"), "abc".ToErlObject(ErlTypeOrder.ErlAtom));
      Aver.AreEqual((IErlObject)new ErlString("abc"), "abc".ToErlObject(ErlTypeOrder.ErlString));
      Aver.AreEqual((IErlObject)new ErlLong(10), 10.ToErlObject(ErlTypeOrder.ErlLong));
      Aver.AreEqual((IErlObject)new ErlDouble(10), 10.ToErlObject(ErlTypeOrder.ErlDouble));
      Aver.AreEqual((IErlObject)new ErlBoolean(true), true.ToErlObject(ErlTypeOrder.ErlBoolean));
      Aver.AreEqual((IErlObject)new ErlBoolean(true), 10.ToErlObject(ErlTypeOrder.ErlBoolean));
      Aver.AreEqual((IErlObject)new ErlBinary(new byte[] { 1, 2, 3 }), (new byte[] { 1, 2, 3 }).ToErlObject(ErlTypeOrder.ErlBinary));
      Aver.AreEqual((IErlObject)new ErlList(1, 2, 3), (new List<int> { 1, 2, 3 }).ToErlObject(ErlTypeOrder.ErlList));
    }

    private void doesNotThrow(Action a)
    {
      try
      {
        a();
        Aver.Pass();
      }
      catch (Exception error)
      {
        Aver.Fail("Exception {0} was thrown: {1}".Args(error.GetType().ToString(), error.Message));
      }
    }
  }
}
