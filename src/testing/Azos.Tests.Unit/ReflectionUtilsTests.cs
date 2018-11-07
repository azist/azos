/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Scripting;

namespace Azos.Tests.Unit
{
  [Runnable(category: TRUN.BASE_RUNNER, order: -1)]
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
      Console.WriteLine(d);
      Aver.AreEqual("Azos.Tests.Unit.ReflectionUtilsTests{Method 'method1'}", d);
    }

  }
}
