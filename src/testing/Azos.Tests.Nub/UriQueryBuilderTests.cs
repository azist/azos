/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class UriQueryBuilderTests
  {
    [Run]
    public void Test_1()
    {
      var sut = new UriQueryBuilder();
      Aver.AreEqual("", sut.ToString());
    }

    [Run]
    public void Test_2()
    {
      var sut = new UriQueryBuilder("yahoo.com");
      Aver.AreEqual("yahoo.com", sut.ToString());
    }

    [Run]
    public void Test_3()
    {
      var sut = new UriQueryBuilder{{"a", 1}};
      Aver.AreEqual("a=1", sut.ToString());
    }

    [Run]
    public void Test_4()
    {
      var sut = new UriQueryBuilder("yahoo.com") { { "a", 1 } };
      Aver.AreEqual("yahoo.com?a=1", sut.ToString());
    }

    [Run]
    public void Test_5()
    {
      var sut = new UriQueryBuilder("yahoo.com") { { "a", 1 }, { "b", 2 } };
      Aver.AreEqual("yahoo.com?a=1&b=2", sut.ToString());
    }

    [Run]
    public void Test_6()
    {
      var sut = new UriQueryBuilder("yahoo.com") { { "a", 1 }, { "b", null } };
      Aver.AreEqual("yahoo.com?a=1&b", sut.ToString());
    }

    [Run]
    public void Test_7()
    {
      var sut = new UriQueryBuilder("yahoo.com") { { "a", 1 }, { "b", null }, { "c", "yes" } };
      Aver.AreEqual("yahoo.com?a=1&b&c=yes", sut.ToString());
    }

    [Run]
    public void Test_8()
    {
      var sut = new UriQueryBuilder("yahoo.com") { { "a b", 1 }, { "c d", "Lenin is alive!" }};
      Aver.AreEqual("yahoo.com?a%20b=1&c%20d=Lenin%20is%20alive%21", sut.ToString());
    }

    [Run]
    public void Test_9()
    {
      var sut = new UriQueryBuilder("http://yahoo.com") { { "topic", "РБМК Реактор" }};
      Aver.AreEqual("http://yahoo.com?topic=%D0%A0%D0%91%D0%9C%D0%9A%20%D0%A0%D0%B5%D0%B0%D0%BA%D1%82%D0%BE%D1%80", sut.ToString());

      var uri = new Uri(sut.ToString());
      Aver.AreEqual("?topic=РБМК Реактор", Uri.UnescapeDataString(uri.Query));
    }

    [Run]
    public void Test_10()
    {
      var sut = new UriQueryBuilder("http://yahoo.com") { { "eq", "32 * sin(x) ^ 2   /   cos(y)-2 ^ e" } };
      Aver.AreEqual("http://yahoo.com?eq=32%20%2A%20sin%28x%29%20%5E%202%20%20%20%2F%20%20%20cos%28y%29-2%20%5E%20e", sut.ToString());

      var uri = new Uri(sut.ToString());
      Aver.AreEqual("?eq=32 * sin(x) ^ 2   /   cos(y)-2 ^ e", Uri.UnescapeDataString(uri.Query));
    }

    [Run] //#912
    public void Test_11_existing_parametrs()
    {
      var sut = new UriQueryBuilder("http://yahoo.com?a=1&b=2") { { "c", "c_value" } };
      Aver.AreEqual("http://yahoo.com?a=1&b=2&c=c_value", sut.ToString());

      sut = new UriQueryBuilder("http://yahoo.com") { { "c", "c_value" } };
      Aver.AreEqual("http://yahoo.com?c=c_value", sut.ToString());

      sut = new UriQueryBuilder("snake") { { "c", "c_value" } };
      Aver.AreEqual("snake?c=c_value", sut.ToString());

      sut = new UriQueryBuilder("http://yahoo.com/snake") { { "c", "c_value" } };
      Aver.AreEqual("http://yahoo.com/snake?c=c_value", sut.ToString());


      sut = new UriQueryBuilder("http://yahoo.com/snake?toady=true") { { "c", "c_value" } };
      Aver.AreEqual("http://yahoo.com/snake?toady=true&c=c_value", sut.ToString());

      sut = new UriQueryBuilder("http://yahoo.com/snake?toady=true") { { "c", "c_value" }, { "meduza", " Sometimes  " } };
      Aver.AreEqual("http://yahoo.com/snake?toady=true&c=c_value&meduza=%20Sometimes%20%20", sut.ToString());

      sut = new UriQueryBuilder(null) { { "c", "c_value" }, { "z", "345" } };
      Aver.AreEqual("c=c_value&z=345", sut.ToString());

    }

  }
}