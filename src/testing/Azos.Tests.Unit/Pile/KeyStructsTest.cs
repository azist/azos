/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using NFX;
using Azos.Apps.Pile;
using Azos.DataAccess.Distributed;
using Azos.Scripting;


namespace Azos.Tests.Unit.AppModel.Pile
{
  [Runnable(TRUN.BASE)]
  public class KeyStructTest
  {

      [Run]
      [Aver.Throws(typeof(AzosException))]
      public void GDIDWithISOKey_BadCtor_1()
      {
        var k = new GDIDWithISOKey(new GDID(10, 20), "ertewrtewrte");
      }

      [Run]
      public void GDIDWithISOKey_CreateEquate_1()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "eNG");
        Aver.AreEqual("ENG", k1.ISOCode);
        Aver.AreEqual("ENG", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void GDIDWithISOKey_CreateEquate_2()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "ua");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void GDIDWithISOKey_CreateNotEquate_1()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 21), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void GDIDWithISOKey_CreateNotEquate_2()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "eng");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "fra");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void GDIDWithISOKey_CreateNotEquate_3()
      {
        var k1 = new GDIDWithISOKey(new GDID(10, 20), "en");
        var k2 = new GDIDWithISOKey(new GDID(10, 20), "fr");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void GDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<GDIDWithISOKey, string>();

        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "eng"), "123eng");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "deu"), "123deu");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "eN"), "123en");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "dE"), "123de");
        dict.Add(new GDIDWithISOKey(new GDID(1, 123), "ua"), "123ua");

        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "eng"), "345eng");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "deu"), "345deu");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "eN"), "345en");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "dE"), "345de");
        dict.Add(new GDIDWithISOKey(new GDID(1, 345), "ua"), "345ua");

        Aver.AreEqual("123eng", dict[new GDIDWithISOKey(new GDID(1, 123), "eng")]);
        Aver.AreEqual("123deu", dict[new GDIDWithISOKey(new GDID(1, 123), "deu")]);
        Aver.AreEqual("123eng", dict[new GDIDWithISOKey(new GDID(1, 123), "ENG")]);
        Aver.AreEqual("123deu", dict[new GDIDWithISOKey(new GDID(1, 123), "DEU")]);

        Aver.AreEqual("123en", dict[new GDIDWithISOKey(new GDID(1, 123), "en")]);
        Aver.AreEqual("123de", dict[new GDIDWithISOKey(new GDID(1, 123), "de")]);
        Aver.AreEqual("123ua", dict[new GDIDWithISOKey(new GDID(1, 123), "ua")]);

        Aver.AreEqual("123en", dict[new GDIDWithISOKey(new GDID(1, 123), "EN")]);
        Aver.AreEqual("123de", dict[new GDIDWithISOKey(new GDID(1, 123), "DE")]);
        Aver.AreEqual("123ua", dict[new GDIDWithISOKey(new GDID(1, 123), "UA")]);


        Aver.AreEqual("345eng", dict[new GDIDWithISOKey(new GDID(1, 345), "eng")]);
        Aver.AreEqual("345deu", dict[new GDIDWithISOKey(new GDID(1, 345), "deu")]);
        Aver.AreEqual("345eng", dict[new GDIDWithISOKey(new GDID(1, 345), "ENG")]);
        Aver.AreEqual("345deu", dict[new GDIDWithISOKey(new GDID(1, 345), "DEU")]);

        Aver.AreEqual("345en", dict[new GDIDWithISOKey(new GDID(1, 345), "en")]);
        Aver.AreEqual("345de", dict[new GDIDWithISOKey(new GDID(1, 345), "de")]);
        Aver.AreEqual("345ua", dict[new GDIDWithISOKey(new GDID(1, 345), "ua")]);

        Aver.AreEqual("345en", dict[new GDIDWithISOKey(new GDID(1, 345), "EN")]);
        Aver.AreEqual("345de", dict[new GDIDWithISOKey(new GDID(1, 345), "DE")]);
        Aver.AreEqual("345ua", dict[new GDIDWithISOKey(new GDID(1, 345), "UA")]);


        Aver.IsTrue( dict.ContainsKey(new GDIDWithISOKey(new GDID(1, 123), "UA")));
        Aver.IsFalse( dict.ContainsKey(new GDIDWithISOKey(new GDID(1, 122), "UA")));
        Aver.IsFalse( dict.ContainsKey(new GDIDWithISOKey(new GDID(21, 123), "UA")));
      }







      [Run]
      [Aver.Throws(typeof(AzosException))]
      public void DatedGDIDWithISOKey_BadCtor_1()
      {
        var k = new DatedGDIDWithISOKey(DateTime.Now, new GDID(10, 20), "ertewrtewrte");
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eNG");
        Aver.AreEqual("ENG", k1.ISOCode);
        Aver.AreEqual("ENG", k2.ISOCode);

        Aver.AreEqual(1980, k1.DateTime.Year);
        Aver.AreEqual(10,   k1.DateTime.Month);
        Aver.AreEqual(2,    k1.DateTime.Day);

        Aver.AreEqual(1980, k2.DateTime.Year);
        Aver.AreEqual(10,   k2.DateTime.Month);
        Aver.AreEqual(2,    k2.DateTime.Day);

        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "ua");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 14, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 2, 0, 18, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt1, new GDID(10, 20), "ua");
        var k2 = new DatedGDIDWithISOKey(dt2, new GDID(10, 20), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateNotEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 21), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateNotEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "eng");
        var k2 = new DatedGDIDWithISOKey(dt, new GDID(10, 20), "fra");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_CreateNotEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new DatedGDIDWithISOKey(dt1, new GDID(10, 20), "en");
        var k2 = new DatedGDIDWithISOKey(dt2, new GDID(10, 20), "en");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void DatedGDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<DatedGDIDWithISOKey, string>();

        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);



        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eng"), "123eng");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "deu"), "123deu");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eN"), "123en");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "dE"), "123de");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ua"), "123ua");

        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eng"), "345eng");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "deu"), "345deu");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eN"), "345en");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "dE"), "345de");
        dict.Add(new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ua"), "345ua");

        Aver.AreEqual("123eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "eng")]);
        Aver.AreEqual("123deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "deu")]);
        Aver.AreEqual("123eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ENG")]);
        Aver.AreEqual("123deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "DEU")]);

        Aver.AreEqual("123en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "en")]);
        Aver.AreEqual("123de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "de")]);
        Aver.AreEqual("123ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "ua")]);

        Aver.AreEqual("123en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "EN")]);
        Aver.AreEqual("123de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "DE")]);
        Aver.AreEqual("123ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "UA")]);


        Aver.AreEqual("345eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "eng")]);
        Aver.AreEqual("345deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "deu")]);
        Aver.AreEqual("345eng", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ENG")]);
        Aver.AreEqual("345deu", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "DEU")]);

        Aver.AreEqual("345en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "en")]);
        Aver.AreEqual("345de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "de")]);
        Aver.AreEqual("345ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "ua")]);

        Aver.AreEqual("345en", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "EN")]);
        Aver.AreEqual("345de", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "DE")]);
        Aver.AreEqual("345ua", dict[new DatedGDIDWithISOKey(dt1, new GDID(1, 345), "UA")]);


        Aver.IsTrue ( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(1, 123), "UA")));
        Aver.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt2, new GDID(1, 123), "UA")));
        Aver.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(1, 122), "UA")));
        Aver.IsFalse( dict.ContainsKey(new DatedGDIDWithISOKey(dt1, new GDID(21, 123), "UA")));
      }


      [Run]
      [Aver.Throws(typeof(AzosException))]
      public void TwoGDIDLongWithISOKey_BadCtor_1()
      {
        var k = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10,30), 123, "ertewrtewrte");
      }

      [Run]
      public void TwoGDIDLongWithISOKey_CreateEquate_1()
      {
        var k1 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), 123, "eng");
        var k2 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), 123, "eNG");
        Aver.AreEqual("ENG", k1.ISOCode);
        Aver.AreEqual("ENG", k2.ISOCode);

        Aver.AreEqual(123, k1.PAYLOAD);
        Aver.AreEqual(123, k2.PAYLOAD);

        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDLongWithISOKey_CreateNotEquate_1()
      {
        var k1 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), 123, "eng");
        var k2 = new TwoGDIDLongWithISOKey(new GDID(10, 21), new GDID(10, 30), 123, "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDLongWithISOKey_CreateNotEquate_2()
      {
        var k1 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), 123, "eng");
        var k2 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(11, 30), 123, "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDLongWithISOKey_CreateNotEquate_3()
      {
        var k1 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), 123, "eng");
        var k2 = new TwoGDIDLongWithISOKey(new GDID(10, 20), new GDID(10, 30), -123, "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDLongWithISOKey_Dictionary()
      {
        var dict = new Dictionary<TwoGDIDLongWithISOKey, string>();

        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 1, "eng"), "123eng");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 2, "deu"), "123deu");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 3, "eN"), "123en");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 4, "dE"), "123de");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 5, "ua"), "123ua");

        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -1, "eng"), "345eng");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -2, "deu"), "345deu");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -3, "eN"), "345en");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -4, "dE"), "345de");
        dict.Add(new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -5, "ua"), "345ua");

        Aver.AreEqual("123eng", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 1, "eng")]);
        Aver.AreEqual("123deu", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 2, "deu")]);
        Aver.AreEqual("123eng", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 1, "ENG")]);
        Aver.AreEqual("123deu", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 2, "DEU")]);

        Aver.AreEqual("123en", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 3, "en")]);
        Aver.AreEqual("123de", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 4, "de")]);
        Aver.AreEqual("123ua", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 5, "ua")]);

        Aver.AreEqual("123en", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 3, "EN")]);
        Aver.AreEqual("123de", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 4, "DE")]);
        Aver.AreEqual("123ua", dict[new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 5, "UA")]);


        Aver.AreEqual("345eng", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -1, "eng")]);
        Aver.AreEqual("345deu", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -2, "deu")]);
        Aver.AreEqual("345eng", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -1, "ENG")]);
        Aver.AreEqual("345deu", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -2, "DEU")]);

        Aver.AreEqual("345en", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -3, "en")]);
        Aver.AreEqual("345de", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -4, "de")]);
        Aver.AreEqual("345ua", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -5, "ua")]);

        Aver.AreEqual("345en", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -3, "EN")]);
        Aver.AreEqual("345de", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -4, "DE")]);
        Aver.AreEqual("345ua", dict[new TwoGDIDLongWithISOKey(new GDID(1, 345), new GDID(10, 30), -5, "UA")]);


        Aver.IsTrue ( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30), 5, "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(20, 40), 5, "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(1, 321), new GDID(10, 30), 5, "UA")));

        Aver.IsFalse( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 31),  5, "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(1, 123), new GDID(10, 30),  6, "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDLongWithISOKey(new GDID(21, 123), new GDID(10, 30), 5, "UA")));
      }


      [Run]
      [Aver.Throws(typeof(AzosException))]
      public void Dated2GDIDWithISOKey_BadCtor_1()
      {
        var k = new Dated2GDIDWithISOKey(DateTime.Now, new GDID(10, 20), new GDID(10,30), "ertewrtewrte");
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eNG");
        Aver.AreEqual("ENG", k1.ISOCode);
        Aver.AreEqual("ENG", k2.ISOCode);

        Aver.AreEqual(1980, k1.DateTime.Year);
        Aver.AreEqual(10,   k1.DateTime.Month);
        Aver.AreEqual(2,    k1.DateTime.Day);

        Aver.AreEqual(1980, k2.DateTime.Year);
        Aver.AreEqual(10,   k2.DateTime.Month);
        Aver.AreEqual(2,    k2.DateTime.Day);

        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 14, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 2, 0, 18, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt1, new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new Dated2GDIDWithISOKey(dt2, new GDID(10, 20), new GDID(10, 30), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Run]
      public void Dated2GDIDWithISOKey_CreateNotEquate_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 21), new GDID(10, 30), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateNotEquate_1_1()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 31), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateNotEquate_2()
      {
        var dt = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new Dated2GDIDWithISOKey(dt, new GDID(10, 20), new GDID(10, 30), "fra");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void Dated2GDIDWithISOKey_CreateNotEquate_3()
      {
        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);

        var k1 = new Dated2GDIDWithISOKey(dt1, new GDID(10, 20), new GDID(10, 30), "en");
        var k2 = new Dated2GDIDWithISOKey(dt2, new GDID(10, 20), new GDID(10, 30), "en");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Run]
      public void Dated2GDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<Dated2GDIDWithISOKey, string>();

        var dt1 = new DateTime(1980, 10, 2, 0, 0, 0, DateTimeKind.Utc);
        var dt2 = new DateTime(1980, 10, 3, 0, 0, 0, DateTimeKind.Utc);



        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eng"), "123eng");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "deu"), "123deu");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eN"), "123en");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "dE"), "123de");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ua"), "123ua");

        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eng"), "345eng");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "deu"), "345deu");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eN"), "345en");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "dE"), "345de");
        dict.Add(new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ua"), "345ua");

        Aver.AreEqual("123eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "eng")]);
        Aver.AreEqual("123deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "deu")]);
        Aver.AreEqual("123eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ENG")]);
        Aver.AreEqual("123deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "DEU")]);

        Aver.AreEqual("123en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "en")]);
        Aver.AreEqual("123de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "de")]);
        Aver.AreEqual("123ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "ua")]);

        Aver.AreEqual("123en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "EN")]);
        Aver.AreEqual("123de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "DE")]);
        Aver.AreEqual("123ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "UA")]);


        Aver.AreEqual("345eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "eng")]);
        Aver.AreEqual("345deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "deu")]);
        Aver.AreEqual("345eng", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ENG")]);
        Aver.AreEqual("345deu", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "DEU")]);

        Aver.AreEqual("345en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "en")]);
        Aver.AreEqual("345de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "de")]);
        Aver.AreEqual("345ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "ua")]);

        Aver.AreEqual("345en", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "EN")]);
        Aver.AreEqual("345de", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "DE")]);
        Aver.AreEqual("345ua", dict[new Dated2GDIDWithISOKey(dt1, new GDID(1, 345), new GDID(10, 30), "UA")]);


        Aver.IsTrue ( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(10, 30), "UA")));
        Aver.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 123), new GDID(20, 40), "UA")));
        Aver.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 321), new GDID(10, 30), "UA")));

        Aver.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt2, new GDID(1, 123), new GDID(10, 30), "UA")));
        Aver.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(1, 122), new GDID(10, 30), "UA")));
        Aver.IsFalse( dict.ContainsKey(new Dated2GDIDWithISOKey(dt1, new GDID(21, 123), new GDID(10, 30), "UA")));
      }









      [Run]
      [Aver.Throws(typeof(AzosException))]
      public void TwoGDIDWithISOKey_BadCtor_1()
      {
        var k = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10,30), "ertewrtewrte");
      }

      [Run]
      public void TwoGDIDWithISOKey_CreateEquate_1()
      {

        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eNG");
        Aver.AreEqual("ENG", k1.ISOCode);
        Aver.AreEqual("ENG", k2.ISOCode);

        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDWithISOKey_CreateEquate_2()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDWithISOKey_CreateEquate_3()
      {

        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "ua");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "UA");
        Aver.AreEqual("UA", k1.ISOCode);
        Aver.AreEqual("UA", k2.ISOCode);
        Aver.AreEqual(k1, k2);

        Aver.IsTrue(k1.Equals(k2));
        var o = k2;
        Aver.IsTrue(k1.Equals(o));

        Aver.AreEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }


      [Run]
      public void TwoGDIDWithISOKey_CreateNotEquate_1()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 21), new GDID(10, 30), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDWithISOKey_CreateNotEquate_1_1()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 31), "eNG");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDWithISOKey_CreateNotEquate_2()
      {
        var k1 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "eng");
        var k2 = new TwoGDIDWithISOKey(new GDID(10, 20), new GDID(10, 30), "fra");
        Aver.AreNotEqual(k1, k2);

        Aver.IsFalse(k1.Equals(k2));
        var o = k2;
        Aver.IsFalse(k1.Equals(o));

        Aver.AreNotEqual(k1.GetHashCode(), k2.GetHashCode());
        Aver.AreNotEqual(k1.GetDistributedStableHash(), k2.GetDistributedStableHash());
        Console.WriteLine(k1.ToString());
      }

      [Run]
      public void TwoGDIDWithISOKey_Dictionary()
      {
        var dict = new Dictionary<TwoGDIDWithISOKey, string>();


        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "eng"), "123eng");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "deu"), "123deu");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "eN"), "123en");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "dE"), "123de");
        dict.Add(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "ua"), "123ua");

        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "eng"), "345eng");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "deu"), "345deu");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "eN"), "345en");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "dE"), "345de");
        dict.Add(new TwoGDIDWithISOKey(new GDID(1, 345), new GDID(10, 30), "ua"), "345ua");

        Aver.AreEqual("123eng", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "eng")]);
        Aver.AreEqual("123deu", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "deu")]);
        Aver.AreEqual("123eng", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "ENG")]);
        Aver.AreEqual("123deu", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "DEU")]);

        Aver.AreEqual("123en", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "en")]);
        Aver.AreEqual("123de", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "de")]);
        Aver.AreEqual("123ua", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "ua")]);

        Aver.AreEqual("123en", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "EN")]);
        Aver.AreEqual("123de", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "DE")]);
        Aver.AreEqual("123ua", dict[new TwoGDIDWithISOKey(new GDID(1, 123), new GDID(10, 30), "UA")]);


        Aver.AreEqual("345eng", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "eng")]);
        Aver.AreEqual("345deu", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "deu")]);
        Aver.AreEqual("345eng", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "ENG")]);
        Aver.AreEqual("345deu", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "DEU")]);

        Aver.AreEqual("345en", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "en")]);
        Aver.AreEqual("345de", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "de")]);
        Aver.AreEqual("345ua", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "ua")]);

        Aver.AreEqual("345en", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "EN")]);
        Aver.AreEqual("345de", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "DE")]);
        Aver.AreEqual("345ua", dict[new TwoGDIDWithISOKey( new GDID(1, 345), new GDID(10, 30), "UA")]);


        Aver.IsTrue ( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(10, 30), "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 123), new GDID(20, 40), "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 321), new GDID(10, 30), "UA")));

        Aver.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(1, 122), new GDID(10, 30), "UA")));
        Aver.IsFalse( dict.ContainsKey(new TwoGDIDWithISOKey( new GDID(21, 123), new GDID(10, 30), "UA")));
      }

      [Run]
      public void GDIDWithStrHash_Equals()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        Aver.AreEqual(g1, g2);
        Aver.AreEqual(g1.GetHashCode(), g2.GetHashCode());
        Aver.AreEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Run]
      public void GDIDWithStrHash_NotEquals()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "this iS my long line");
        Aver.AreNotEqual(g1, g2);
        Aver.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Aver.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Run]
      public void GDIDWithStrHash_NotEquals2()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "this is my long line");
        var g2 = new GDIDWithStrHash(new GDID(1, 121), "this is my long line");
        Aver.AreNotEqual(g1, g2);
        Aver.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Aver.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Run]
      public void GDIDWithStrHash_NotEquals3()
      {
        var g1 = new GDIDWithStrHash(new GDID(1, 123), "lenen");
        var g2 = new GDIDWithStrHash(new GDID(1, 123), "lenin");
        Aver.AreNotEqual(g1, g2);
        Aver.AreNotEqual(g1.GetHashCode(), g2.GetHashCode());
        Aver.AreNotEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }


      [Run]
      public void GDIDWithInt_Equals()
      {
        var g1 = new GDIDWithInt(new GDID(1, 123), 100);
        var g2 = new GDIDWithInt(new GDID(1, 123), 100);
        Aver.AreEqual(g1, g2);
        Aver.AreEqual(g1.GetHashCode(), g2.GetHashCode());
        Aver.AreEqual(g1.GetDistributedStableHash(), g2.GetDistributedStableHash());
      }

      [Run]
      public void GDIDWithInt_NotEquals()
      {
        var g1 = new GDIDWithInt(new GDID(1,123), 341);
        var g2 = new GDIDWithInt(new GDID(1,123), 143);
        Aver.AreNotEqual(g1,g2);
        Aver.AreNotEqual(g1.GetHashCode(),g2.GetHashCode());
        Aver.AreNotEqual(g1.GetDistributedStableHash(),g2.GetDistributedStableHash());
      }

      [Run]
      public void GDIDWithInt_NotEquals2()
      {
        var g1 = new GDIDWithInt(new GDID(1,123), 341);
        var g2 = new GDIDWithInt(new GDID(1,214), 341);
        Aver.AreNotEqual(g1,g2);
        Aver.AreNotEqual(g1.GetHashCode(),g2.GetHashCode());
        Aver.AreNotEqual(g1.GetDistributedStableHash(),g2.GetDistributedStableHash());
      }
  }
}

