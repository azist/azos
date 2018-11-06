/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Conf;

namespace Azos.Tests.Unit
{
    /// <summary>
    /// Test the async/await kind of tests
    /// </summary>
    [Runnable(category: TRUN.BASE_RUNNER, order: -1)]
    public class RunnerAsyncTests
    {
      private async Task delay(int ms = 100) => await Task.Delay(ms);
      private async Task<T> delayReturning<T>(T ret, int ms = 100)
      {
        await Task.Delay(ms);
        return ret;
      }

      [Run(order: -1000)]
      public async void Empty_async_void()
      {
        await delay();
      }

      [Run(order: -900)]
      public async Task Empty_async_task()
      {
        await delay();
      }

      [Run(order: -800)]
      public async Task<int> Empty_async_taskT()
      {
        await delay();
        return 5;
      }

      [Run(order: -100)]
      public async void MultipleDelayReturning_async_void()
      {
        var x = await delayReturning(123, 150);
        var y = await delayReturning(7, 5);
        Aver.AreEqual(130, x+y);
      }

      [Run(order: -90)]
      public async Task MultipleDelayReturning_async_task()
      {
        var x = await delayReturning(123, 150);
        var y = await delayReturning(7, 5);
        Aver.AreEqual(130, x + y);
      }

      [Run(order: -80)]
      public async Task<int> MultipleDelayReturning_async_taskT_sequential()
      {
        var x = await delayReturning(123, 150);
        var y = await delayReturning(7, 5);
        var result = x+ y;
        Aver.AreEqual(130, result);
        return result;
      }

      [Run(order: -70)]
      public async Task<int> MultipleDelayReturning_async_taskT_parallel()
      {
        var x = delayReturning(123, 150);
        var y = delayReturning(7, 5);
        await Task.WhenAll(x, y);
        var result = await x + await y;
        Aver.AreEqual(130, result);
        return result;
      }

      /// <summary>
      /// Makes sure that the whole async chain gets executed correctly
      /// </summary>
      [Run(order: -10)]
      [Run, Aver.Throws(typeof(NotSupportedException), ExactType = true, Message = "Special Text 1", MsgMatch = Aver.ThrowsAttribute.MatchType.Exact)]
      public async void Multiple_async_void()
      {
        await delay(10);
        Aver.Equals(1, 1);
        await delay(20);
        Aver.Equals(2, 2);
        await delay(30);
        Console.WriteLine("This gets printed)");
        throw new NotSupportedException("Special Text 1");
      }

      /// <summary>
      /// Makes sure that the whole async chain gets executed correctly
      /// </summary>
      [Run(order: -5)]
      [Run, Aver.Throws(typeof(NotSupportedException), ExactType = true, Message = "Special Text 2", MsgMatch = Aver.ThrowsAttribute.MatchType.Exact)]
      public async Task Multiple_async_task()
      {
        await delay(10);
        Aver.Equals(1, 1);
        await delay(20);
        Aver.Equals(2, 2);
        await delay(30);
        Console.WriteLine("This gets printed)");
        throw new NotSupportedException("Special Text 2");
      }

      [Run("a=1 b=-1 sect{a=100 b=200}")]
      public async void Section(int a, int b, IConfigSectionNode sect)
      {
          Aver.AreEqual(1, a);
          Aver.AreEqual(-1, b);
          await delay();
          Aver.AreEqual(100, sect.AttrByName("a").ValueAsInt());
          Aver.AreEqual(200, sect.AttrByName("b").ValueAsInt());
      }

      [Run("a{ x=10 y=20 log{ good=true }}")]
      public async void InjectConfig_IConfigSectionNode(int x, int y, IConfigSectionNode log)
      {
          Aver.AreEqual(10, x);
          Aver.AreEqual(20, y);
          Aver.IsNotNull(log);
          await delay();
          Aver.IsTrue( log is ConfigSectionNode );
          Aver.AreEqual(1, log.AttrCount );
          Aver.IsTrue( log.AttrByName("good").ValueAsBool(false) );
      }

      public class Person : IConfigurable
      {
        [Config] public string Name { get; set;}
        [Config] public int Age { get; set;}
        public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
      }

      [Run("case1", "a{ expectName='aaa' expectAge=-125 person{ name='aaa' age=-125}}")]
      [Run("case2", "expectName='kozel' expectAge=125 person{ name='kozel' age=125}")]
      public async void InjectComplexType(string expectName, int expectAge, Person person)
      {
        Aver.IsNotNull(person);
        await delay();
        Aver.AreEqual(expectName, person.Name);
        Aver.AreEqual(expectAge, person.Age);
      }

      [Run("case1", "a{ expectName='aaa' expectAge=-125 person{ name='aaa' age=-125}}")]
      [Run("case2", "expectName='kozel' expectAge=125 person{ name='kozel' age=125}")]
      public async void InjectComplexType_delayReturning(string expectName, int expectAge, Person person)
      {
        Aver.IsNotNull(person);

        var gotName = await delayReturning(person.Name);
        Aver.AreEqual(expectName, gotName);

        var gotAge = await delayReturning(person.Age);
        Aver.AreEqual(expectAge, gotAge);
      }

      [Run("case1", "a{ expectName='aaa' expectAge=-125 person{ name='aaa' age=-125}}")]
      [Run("case2", "expectName='Josh' expectAge=1125 person{ name='Josh' age=1125}")]
      public async Task<Person> InjectComplexType_delayReturning_taskT(string expectName, int expectAge, Person person)
      {
        Aver.IsNotNull(person);

        var gotName = await delayReturning(person.Name);
        Aver.AreEqual(expectName, gotName);

        var gotAge = await delayReturning(person.Age);
        Aver.AreEqual(expectAge, gotAge);

        return person;//this goes nowhere as expected
      }

  }
}
