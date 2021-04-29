/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class ReflectionUtilsTests
  {
    public void method1(int a, char b, bool c) { }
    public void method2(long a, ref double b) { }
    public void method3(int a, bool b, char c, int d, double e, decimal f) { }
    public async void method_avoid() { await Task.Delay(1); }
    public async Task method_atask() { await Task.Delay(1); }

    [Run]
    public void OfSignature()
    {
      var mi1 = GetType().GetMethod("method1");
      var mi2 = GetType().GetMethod("method2");
      Aver.IsTrue( mi1.OfSignature( new Type[]{typeof(int), typeof(char), typeof(bool) }));
      Aver.IsFalse(mi1.OfSignature(new Type[] { typeof(int), typeof(bool), typeof(char) }));
      Aver.IsFalse(mi1.OfSignature(Type.EmptyTypes));

      Aver.IsTrue(mi2.OfSignature(new Type[] { typeof(long), typeof(double).MakeByRefType()}));
      Aver.IsFalse(mi2.OfSignature(new Type[] { typeof(int), typeof(char), typeof(bool) }));
      Aver.IsFalse(mi2.OfSignature(Type.EmptyTypes));
    }

    [Run]
    public void IndexOfArg()
    {
      var mi1 = GetType().GetMethod("method1");
      Aver.AreEqual(0, mi1.IndexOfArg(typeof(int), "a"));
      Aver.AreEqual(-1, mi1.IndexOfArg(typeof(int), "b"));
      Aver.AreEqual(1, mi1.IndexOfArg(typeof(char), "b"));

      var mi3 = GetType().GetMethod("method3");
      Aver.AreEqual(0, mi3.IndexOfArg(typeof(int), "a"));
      Aver.AreEqual(-1, mi3.IndexOfArg(typeof(int), "b"));
      Aver.AreEqual(1, mi3.IndexOfArg(typeof(bool), "b"));
      Aver.AreEqual(2, mi3.IndexOfArg(typeof(char), "c"));
      Aver.AreEqual(-1, mi3.IndexOfArg(typeof(char), "d"));
      Aver.AreEqual(5, mi3.IndexOfArg(typeof(decimal), "f"));
    }

    [Run]
    public void IsAsyncMethod()
    {
      var mi1 = GetType().GetMethod("method1");
      var mi2 = GetType().GetMethod("method2");
      var mi3 = GetType().GetMethod("method_avoid");
      var mi4 = GetType().GetMethod("method_atask");

      Aver.IsFalse( mi1.IsAsyncMethod() );
      Aver.IsFalse( mi2.IsAsyncMethod() );
      Aver.IsTrue( mi3.IsAsyncMethod() );
      Aver.IsTrue( mi4.IsAsyncMethod() );
    }

    [Run]
    public void ToDescription()
    {
      var d = GetType().GetMethod("method1").ToDescription();
      d.See();
      Aver.AreEqual("Azos.Tests.Nub.ReflectionUtilsTests{Method 'method1'}", d);
    }

    public class A   { public override string ToString(){ return "a";} }
    public class B:A { public override string ToString() { return base.ToString()+"b"; } }
    public class C:B { }
    public class D:C { public override string ToString() { return "d"; } }
    public class E:D { public override string ToString() { return "e"; } }

    public class F : E { public virtual string ToString(bool x) { return x.ToString(); } }

    public class G1 : F { public override string ToString() { return "g1"; } }
    public class G2 : F { public new virtual string ToString() { return "g2"; } }

    public class H1 : G1 { public override string ToString() { return "h1"; } }
    public class H2 : G2 { public override string ToString() { return "h2"; } }

    [Run]
    public void FindImmediateBaseForThisOverride_1()
    {
      var mi = typeof(E).GetMethod("ToString");
      var baze = mi.FindImmediateBaseForThisOverride();
      var verybase = mi.GetBaseDefinition();
      Aver.IsTrue( baze == typeof(D).GetMethod("ToString"));
      Aver.IsTrue(verybase == typeof(object).GetMethod("ToString"));
    }

    [Run]
    public void FindImmediateBaseForThisOverride_2()
    {
      var mi = typeof(D).GetMethod("ToString");
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsTrue(baze == typeof(B).GetMethod("ToString"));
    }

    [Run]
    public void FindImmediateBaseForThisOverride_3()
    {
      var mi = typeof(F).GetMethod("ToString", new []{typeof(bool)});
      Aver.IsNotNull(mi);
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsNull(baze);
    }

    [Run]
    public void FindImmediateBaseForThisOverride_4()
    {
      var mi = typeof(G1).GetMethod("ToString", new Type[]{});
      Aver.IsNotNull(mi);
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsTrue(baze == typeof(E).GetMethod("ToString"));
    }

    [Run]
    public void FindImmediateBaseForThisOverride_5()
    {
      var mi = typeof(G2).GetMethod("ToString", new Type[] { });
      Aver.IsNotNull(mi);
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsNull(baze);
    }

    [Run]
    public void FindImmediateBaseForThisOverride_6()
    {
      var mi = typeof(H1).GetMethod("ToString", new Type[] { });
      Aver.IsNotNull(mi);
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsTrue(baze == typeof(G1).GetMethod("ToString", new Type[] { }));
    }

    [Run]
    public void FindImmediateBaseForThisOverride_7()
    {
      var mi = typeof(H2).GetMethod("ToString", new Type[] { });
      Aver.IsNotNull(mi);
      var baze = mi.FindImmediateBaseForThisOverride();
      Aver.IsTrue(baze == typeof(G2).GetMethod("ToString", new Type[] { }));
    }

  }
}
