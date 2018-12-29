/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Azos.Apps;
using Azos.Data;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Scripting;
using Azos.Serialization.BSON;

namespace Azos.Tests.Integration.MongoDb
{
  [Runnable]
  public class BasicConnectorFunctionality
  {


    [Run]
    public void AllocClient()
    {
      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var server = client.DefaultLocalServer;
        Aver.IsNotNull( server );
        Aver.IsTrue( object.ReferenceEquals(server, client[server.Node]) );

        var db = server["db1"];
        Aver.IsNotNull( db );
        Aver.IsTrue( object.ReferenceEquals(db, server[db.Name]) );

        var collection = db["t1"];
        Aver.IsNotNull( collection );
        Aver.IsTrue( object.ReferenceEquals(collection, db[collection.Name]) );

        var query = new Query();
        var result = collection.FindOne( query );
      }
    }


    [Run]
    public void CollectionLifecycle()
    {

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();
        db["t2"].Drop();
        db["t3"].Drop();

        var collectionCount = db.GetCollectionNames().Length;

        Aver.IsTrue(collectionCount == 0 || collectionCount == 1);

        Aver.AreEqual(1, db["t1"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                              .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Aver.AreEqual(1, db["t2"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                              .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        var collections = db.GetCollectionNames();
        collectionCount = collections.Length;

        // Different MongoDb Server versions treat system collection differently (newer versions dont count)
        Aver.IsTrue(collectionCount == 2 || collectionCount == 3);
        Aver.IsTrue( collections.Contains("t1"));
        Aver.IsTrue( collections.Contains("t2"));
      }
    }


    [Run]
    public void InsertDelete()
    {

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        Aver.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONInt32Element("_id", 1))
                                                        .Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Aver.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONInt32Element("_id", 2))
                                                        .Set( new BSONStringElement("val", "two"))
                           ).TotalDocumentsAffected);

        Aver.AreEqual(2, t1.Count());
        Aver.AreEqual( 1, t1.DeleteOne(Query.ID_EQ_Int32(1)).TotalDocumentsAffected );

        Aver.AreEqual(1, t1.Count());
        Aver.AreEqual( 1, t1.DeleteOne(Query.ID_EQ_Int32(2)).TotalDocumentsAffected );

        Aver.AreEqual(0, t1.Count());
      }

    }

    [Run]
    public void InsertWithoutID()
    {

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        db["t1"].Drop();

        var t1 = db["t1"];

        Aver.AreEqual(1, t1.Insert( new BSONDocument().Set( new BSONStringElement("val", "one"))
                           ).TotalDocumentsAffected);

        Aver.AreEqual(1, t1.Count());
      }

    }

    [Run]
    public void CollectionDrop()
    {

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var collection = client.DefaultLocalServer["db1"]["ToDrop"];
        var doc1 = new BSONDocument();

        doc1.Set( new BSONStringElement("_id", "id1"))
            .Set( new BSONStringElement("val", "My value"))
            .Set( new BSONInt32Element("age", 125));

        var r = collection.Insert(doc1);
        Aver.AreEqual(1, r.TotalDocumentsAffected);

        collection.Drop();
        Aver.IsTrue( collection.Disposed );
      }

    }

    [Run]
    public void Ping()
    {

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var db = client.DefaultLocalServer["db1"];
        db.Ping();
      }
    }


    [Run]
    public void Ping_Parallel()
    {
      const int CNT = 123000;

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var db = client.DefaultLocalServer["db1"];

        var sw = Stopwatch.StartNew();
        Parallel.For(0, CNT, (i) => db.Ping());
        var e =sw.ElapsedMilliseconds;

        Console.WriteLine("Ping Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec".Args(CNT, e, CNT / (e/1000d)) );
      }
    }

    [Run("maxConnections=0")]
    [Run("maxConnections=2")]
    public void Insert_FindOne_Parallel(int maxConnections)
    {
      const int CNT = 75000;

      using(var client= new MongoClient(NOPApplication.Instance, "My Test"))
      {
        var server = client.DefaultLocalServer;
        server.MaxConnections = maxConnections;
        var db = server["db1"];
        db["t1"].Drop();
        var t1 = db["t1"];


        var sw = Stopwatch.StartNew();
        Parallel.For(0, CNT, (i) =>
        {
          Aver.AreEqual(1, db["t1"].Insert( new BSONDocument().Set( new BSONInt32Element("_id", i))
                                                                .Set( new BSONStringElement("val", "num-"+i.ToString()))
                           ).TotalDocumentsAffected);
        });

        var e1 =sw.ElapsedMilliseconds;

        Aver.AreEqual(CNT, t1.Count());

        Console.WriteLine("Insert Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec   MAX CONNECTIONS={3} ".Args(CNT, e1, CNT / (e1/1000d), maxConnections ));

        sw.Restart();
        Parallel.For(0, CNT, (i) =>
        {
          var got = db["t1"].FindOne(Query.ID_EQ_Int32(i));
          Aver.IsNotNull( got );
          Aver.AreEqual("num-"+i.ToString(), got["val"].AsString());
        });

        var e2 =sw.ElapsedMilliseconds;

        Console.WriteLine("FindOne Parallel: {0:n0} times in {1:n0} ms at {2:n0} ops/sec   MAX CONNECTIONS={3} ".Args(CNT, e2, CNT / (e2/1000d), maxConnections ));
      }
    }

    [Run]
    public void Insert_Find_PrimitiveTypes()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item1 = new BSONDocument().Set(new BSONInt32Element("int", int.MaxValue));
        var item2 = new BSONDocument().Set(new BSONStringElement("string", "min"));
        var item3 = new BSONDocument().Set(new BSONBooleanElement("bool", true));
        var item4 = new BSONDocument().Set(new BSONDateTimeElement("datetime", new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc)));
        var item5 = new BSONDocument().Set(new BSONNullElement("null"));
        var item6 = new BSONDocument().Set(new BSONArrayElement("array",
                                              new BSONElement[]
                                              {
                                                new BSONInt32Element(int.MaxValue),
                                                new BSONInt32Element(int.MinValue)
                                              }));
        var item7 = new BSONDocument().Set(new BSONBinaryElement("binary", new BSONBinary(BSONBinaryType.UserDefined, Encoding.UTF8.GetBytes("Hello world"))));
        var item8 = new BSONDocument().Set(new BSONDocumentElement("document",
                                              new BSONDocument().Set(new BSONInt64Element("innerlong", long.MinValue))));
        var item9 = new BSONDocument().Set(new BSONDoubleElement("double", -123.456D));
        var item10 = new BSONDocument().Set(new BSONInt64Element("long", long.MaxValue));
        var item11 = new BSONDocument().Set(new BSONJavaScriptElement("js", "function(a){var x = a;return x;}"));
        var item12 = new BSONDocument().Set(new BSONJavaScriptWithScopeElement("jswithscope",
                                              new BSONCodeWithScope("function(a){var x = a;return x+z;}",
                                                                     new BSONDocument().Set(new BSONInt32Element("z", 12)))));
        var item13 = new BSONDocument().Set(new BSONMaxKeyElement("maxkey"));
        var item14 = new BSONDocument().Set(new BSONMinKeyElement("minkey"));
        var item15 = new BSONDocument().Set(new BSONObjectIDElement("oid", new BSONObjectID(1, 2, 3, 400)));
        var item16 = new BSONDocument().Set(new BSONRegularExpressionElement("regex",
                                              new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M)));
        var item17 = new BSONDocument().Set(new BSONTimestampElement("timestamp", new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345)));

        Aver.AreEqual(1, collection.Insert(item1).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item2).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item3).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item4).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item5).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item6).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item7).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item8).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item9).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item10).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item11).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item12).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item13).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item14).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item15).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item16).TotalDocumentsAffected);
        Aver.AreEqual(1, collection.Insert(item17).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONInt32Element)all.Current["int"]).Value, int.MaxValue);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONStringElement)all.Current["string"]).Value, "min");

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONBooleanElement)all.Current["bool"]).Value, true);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONDateTimeElement)all.Current["datetime"]).Value, new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc));

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.IsTrue(all.Current["null"] is BSONNullElement);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        var array = ((BSONArrayElement)all.Current["array"]).Value;
        Aver.AreEqual(array.Length, 2);
        Aver.AreEqual(((BSONInt32Element)array[0]).Value, int.MaxValue);
        Aver.AreEqual(((BSONInt32Element)array[1]).Value, int.MinValue);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        var binary = ((BSONBinaryElement)all.Current["binary"]).Value;
        Aver.IsTrue(binary.Data.MemBufferEquals(Encoding.UTF8.GetBytes("Hello world")));
        Aver.IsTrue(binary.Type == BSONBinaryType.UserDefined);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        var doc = ((BSONDocumentElement)all.Current["document"]).Value;
        Aver.AreEqual(doc.Count, 1);
        Aver.AreEqual(((BSONInt64Element)doc["innerlong"]).Value, long.MinValue);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONDoubleElement)all.Current["double"]).Value, -123.456D);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONInt64Element)all.Current["long"]).Value, long.MaxValue);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreEqual(((BSONJavaScriptElement)all.Current["js"]).Value, "function(a){var x = a;return x;}");

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        var jsScope = ((BSONJavaScriptWithScopeElement)all.Current["jswithscope"]).Value;
        Aver.AreEqual(jsScope.Code, "function(a){var x = a;return x+z;}");
        Aver.AreEqual(jsScope.Scope.Count, 1);
        Aver.AreEqual(((BSONInt32Element)jsScope.Scope["z"]).Value, 12);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.IsTrue(all.Current["maxkey"] is BSONMaxKeyElement);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.IsTrue(all.Current["minkey"] is BSONMinKeyElement);

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        var oid = ((BSONObjectIDElement)all.Current["oid"]).Value;
        Aver.IsTrue(oid.Bytes.MemBufferEquals(new BSONObjectID(1, 2, 3, 400).Bytes));

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreObjectsEqual(((BSONRegularExpressionElement)all.Current["regex"]).Value,
                        new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M));

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 2);
        Aver.AreObjectsEqual(((BSONTimestampElement)all.Current["timestamp"]).Value,
                        new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345));


        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Insert_Find_PrimitiveTypesSingleEntry()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item = new BSONDocument().Set(new BSONInt32Element("int", int.MaxValue))
                                     .Set(new BSONStringElement("string", "min"))
                                     .Set(new BSONBooleanElement("bool", true))
                                     .Set(new BSONDateTimeElement("datetime", new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc)))
                                     .Set(new BSONNullElement("null"))
                                     .Set(new BSONArrayElement("array",
                                              new BSONElement[]
                                              {
                                                new BSONInt32Element(int.MaxValue),
                                                new BSONInt32Element(int.MinValue)
                                              }))
                                     .Set(new BSONBinaryElement("binary", new BSONBinary(BSONBinaryType.UserDefined, Encoding.UTF8.GetBytes("Hello world"))))
                                     .Set(new BSONDocumentElement("document",
                                            new BSONDocument().Set(new BSONInt64Element("innerlong", long.MinValue))))
                                     .Set(new BSONDoubleElement("double", -123.456D))
                                     .Set(new BSONInt64Element("long", long.MaxValue))
                                     .Set(new BSONJavaScriptElement("js", "function(a){var x = a;return x;}"))
                                     .Set(new BSONJavaScriptWithScopeElement("jswithscope",
                                              new BSONCodeWithScope("function(a){var x = a;return x+z;}",
                                                                     new BSONDocument().Set(new BSONInt32Element("z", 12)))))
                                     .Set(new BSONMaxKeyElement("maxkey"))
                                     .Set(new BSONMinKeyElement("minkey"))
                                     .Set(new BSONObjectIDElement("oid", new BSONObjectID(1, 2, 3, 400)))
                                     .Set(new BSONRegularExpressionElement("regex",
                                           new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M)))
                                     .Set(new BSONTimestampElement("timestamp", new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345)));

        Aver.AreEqual(1, collection.Insert(item).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 18);
        Aver.AreEqual(((BSONInt32Element)all.Current["int"]).Value, int.MaxValue);
        Aver.AreEqual(((BSONStringElement)all.Current["string"]).Value, "min");
        Aver.AreEqual(((BSONBooleanElement)all.Current["bool"]).Value, true);
        Aver.AreEqual(((BSONDateTimeElement)all.Current["datetime"]).Value, new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc));
        Aver.IsTrue(all.Current["null"] is BSONNullElement);
        var array = ((BSONArrayElement)all.Current["array"]).Value;
        Aver.AreEqual(array.Length, 2);
        Aver.AreEqual(((BSONInt32Element)array[0]).Value, int.MaxValue);
        Aver.AreEqual(((BSONInt32Element)array[1]).Value, int.MinValue);
        var binary = ((BSONBinaryElement)all.Current["binary"]).Value;
        Aver.IsTrue(binary.Data.MemBufferEquals(Encoding.UTF8.GetBytes("Hello world")));
        Aver.IsTrue(binary.Type == BSONBinaryType.UserDefined);
        var doc = ((BSONDocumentElement)all.Current["document"]).Value;
        Aver.AreEqual(doc.Count, 1);
        Aver.AreEqual(((BSONInt64Element)doc["innerlong"]).Value, long.MinValue);
        Aver.AreEqual(((BSONDoubleElement)all.Current["double"]).Value, -123.456D);
        Aver.AreEqual(((BSONInt64Element)all.Current["long"]).Value, long.MaxValue);
        Aver.AreEqual(((BSONJavaScriptElement)all.Current["js"]).Value, "function(a){var x = a;return x;}");
        var jsScope = ((BSONJavaScriptWithScopeElement)all.Current["jswithscope"]).Value;
        Aver.AreEqual(jsScope.Code, "function(a){var x = a;return x+z;}");
        Aver.AreEqual(jsScope.Scope.Count, 1);
        Aver.AreEqual(((BSONInt32Element)jsScope.Scope["z"]).Value, 12);
        Aver.IsTrue(all.Current["maxkey"] is BSONMaxKeyElement);
        Aver.IsTrue(all.Current["minkey"] is BSONMinKeyElement);
        var oid = ((BSONObjectIDElement)all.Current["oid"]).Value;
        Aver.IsTrue(oid.Bytes.MemBufferEquals(new BSONObjectID(1, 2, 3, 400).Bytes));
        Aver.AreObjectsEqual(((BSONRegularExpressionElement)all.Current["regex"]).Value,
                             new BSONRegularExpression(@"^[-.\w]+@(?:[a-z\d]{2,}\.)+[a-z]{2,6}$", BSONRegularExpressionOptions.I | BSONRegularExpressionOptions.M));
        Aver.AreObjectsEqual(((BSONTimestampElement)all.Current["timestamp"]).Value,
                             new BSONTimestamp(new DateTime(2000, 1, 4, 12, 34, 56, DateTimeKind.Utc), 12345));

        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Insert_Find_UnicodeStings()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        var item = new BSONDocument().Set(new BSONStringElement("eng", "hello"))
          .Set(new BSONStringElement("rus", "привет"))
          .Set(new BSONStringElement("chi", "你好"))
          .Set(new BSONStringElement("jap", "こんにちは"))
          .Set(new BSONStringElement("gre", "γεια σας"))
          .Set(new BSONStringElement("alb", "përshëndetje"))
          .Set(new BSONStringElement("arm", "բարեւ Ձեզ"))
          .Set(new BSONStringElement("vie", "xin chào"))
          .Set(new BSONStringElement("por", "Olá"))
          .Set(new BSONStringElement("ukr", "Привіт"))
          .Set(new BSONStringElement("ger", "wünsche"));

        Aver.AreEqual(1, collection.Insert(item).TotalDocumentsAffected);

        var all = collection.Find(new Query());

        all.MoveNext();
        Aver.AreEqual(all.Current.Count, 12);
        Aver.AreEqual(((BSONStringElement)all.Current["eng"]).Value, "hello");
        Aver.AreEqual(((BSONStringElement)all.Current["rus"]).Value, "привет");
        Aver.AreEqual(((BSONStringElement)all.Current["chi"]).Value, "你好");
        Aver.AreEqual(((BSONStringElement)all.Current["jap"]).Value, "こんにちは");
        Aver.AreEqual(((BSONStringElement)all.Current["gre"]).Value, "γεια σας");
        Aver.AreEqual(((BSONStringElement)all.Current["alb"]).Value, "përshëndetje");
        Aver.AreEqual(((BSONStringElement)all.Current["arm"]).Value, "բարեւ Ձեզ");
        Aver.AreEqual(((BSONStringElement)all.Current["vie"]).Value, "xin chào");
        Aver.AreEqual(((BSONStringElement)all.Current["por"]).Value, "Olá");
        Aver.AreEqual(((BSONStringElement)all.Current["ukr"]).Value, "Привіт");
        Aver.AreEqual(((BSONStringElement)all.Current["ger"]).Value, "wünsche");

        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Find_Comparison_DateTime()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        foreach (var day in Enumerable.Range(1, 26))
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "Ivan"+day.ToString()))
                                    .Set(new BSONDateTimeElement("dt", new DateTime(2017, 3, day, 0, 0, 0, DateTimeKind.Utc))))
                                       .TotalDocumentsAffected);
        }

        var query =  new Query(
        @"{
            'dt': {'$lte': '$$DT'}
          }",
          false,
          new TemplateArg("DT", BSONElementType.DateTime, new DateTime(2017, 3, 2, 0, 0, 0, DateTimeKind.Utc))
        );
        var lt1 = collection.Find(query);
        lt1.MoveNext();

        Aver.IsNotNull(lt1.Current);
        Aver.AreObjectsEqual("Ivan1", lt1.Current["name"].ObjectValue);
        Aver.AreObjectsEqual(new DateTime(2017, 3, 1, 0, 0, 0, DateTimeKind.Utc), lt1.Current["dt"].ObjectValue);

        lt1.MoveNext();
        Aver.AreObjectsEqual("Ivan2", lt1.Current["name"].ObjectValue);
        Aver.AreObjectsEqual(new DateTime(2017, 3, 2, 0, 0, 0, DateTimeKind.Utc), lt1.Current["dt"].ObjectValue);

        lt1.MoveNext();
        Aver.AreEqual(true, lt1.EOF);
      }
    }

    [Run]
    public void Find_Comparison_Int32()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        foreach (var age in Enumerable.Range(1, 100))
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", Guid.NewGuid().ToString()))
                                    .Set(new BSONInt32Element("age", age)))
                                       .TotalDocumentsAffected);
        }

        var lt1 = collection.Find(new Query(@"{ age: { $lt: 1}}", false));
        var lt5 = collection.Find(new Query(@"{ age: { '$lt': 5}}", false));
        var gte93 = collection.Find(new Query(@"{ age: { '$gte': 93}}", false));

        lt1.MoveNext();
        Aver.AreEqual(true, lt1.EOF);

        for (int i = 1; i < 5; i++)
        {
          lt5.MoveNext();
          Aver.AreEqual(lt5.Current.Count, 3);
          Aver.IsTrue(lt5.Current["name"] is BSONStringElement);
          Aver.AreEqual(((BSONInt32Element)lt5.Current["age"]).Value, i);
        }
        lt5.MoveNext();
        Aver.AreEqual(true, lt5.EOF);

        for (int i = 93; i <= 100; i++)
        {
          gte93.MoveNext();
          Aver.AreEqual(gte93.Current.Count, 3);
          Aver.IsTrue(gte93.Current["name"] is BSONStringElement);
          Aver.AreEqual(((BSONInt32Element)gte93.Current["age"]).Value, i);
        }
        gte93.MoveNext();
        Aver.AreEqual(true, gte93.EOF);
      }
    }

    [Run]
    public void Update_SimpleStringInt23Entries()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        foreach (var age in Enumerable.Range(1, 10))
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + age))
                                    .Set(new BSONInt32Element("age", age)))
                                       .TotalDocumentsAffected);
        }

        var result1 = collection.Update(new UpdateEntry(new Query("{ name: 'People1'}", false), new Query("{age: 100}", false), false, false));
        var update1 = collection.Find(new Query(@"{ age: 100}", false));

        var result2 = collection.Update(new UpdateEntry(new Query("{ age: { '$lt': 3 }}", false), new Query("{name: 'update2'}", false), false, false));
        var update2 = collection.Find(new Query(@"{name: 'update2'}", false));

        var result3 = collection.Update(new UpdateEntry(new Query("{ '$and': [ { age: { '$gte': 3 }}, { age: { '$lt': 6 }} ]}", false), new Query("{ '$set': {name: 'update3'}}", false), true, false));
        var update3 = collection.Find(new Query(@"{name: 'update3'}", false));

        var result4 = collection.Update(new UpdateEntry(new Query("{ age: -1}", false), new Query("{ '$set': {name: 'update4'}}", false), true, false));
        var update4 = collection.Find(new Query(@"{name: 'update4'}", false));

        var result5 = collection.Update(new UpdateEntry(new Query("{ age: -1}", false), new Query("{ '$set': {name: 'update5'}}", false), true, true));
        var update5 = collection.Find(new Query(@"{name: 'update5'}", false));

        var result6 = collection.Update(new UpdateEntry(new Query("{ '$or': [ {age: 6}, {age: 7}, {age: 8} ]}", false), new Query("{ '$set': {name: 'update6'}}", false), true, false));
        var update6 = collection.Find(new Query(@"{name: 'update6'}", false));

        Aver.AreEqual(result1.TotalDocumentsUpdatedAffected, 1);
        Aver.AreEqual(result1.TotalDocumentsAffected, 1);
        Aver.IsNull(result1.Upserted);
        Aver.IsNull(result1.WriteErrors);
        update1.MoveNext();
        Aver.AreEqual(update1.Current.Count, 2);
        Aver.AreEqual(((BSONInt32Element)update1.Current["age"]).Value,100);
        update1.MoveNext();
        Aver.AreEqual(true, update1.EOF);

        Aver.AreEqual(result2.TotalDocumentsUpdatedAffected, 1);
        Aver.AreEqual(result2.TotalDocumentsAffected, 1);
        Aver.IsNull(result2.Upserted);
        Aver.IsNull(result2.WriteErrors);
        update2.MoveNext();
        Aver.AreEqual(update2.Current.Count, 2);
        Aver.AreEqual(((BSONStringElement)update2.Current["name"]).Value, "update2");
        update2.MoveNext();
        Aver.AreEqual(true, update2.EOF);

        Aver.AreEqual(result3.TotalDocumentsUpdatedAffected, 3);
        Aver.AreEqual(result3.TotalDocumentsAffected, 3);
        Aver.IsNull(result3.Upserted);
        Aver.IsNull(result3.WriteErrors);
        for (int i = 3; i < 6; i++)
        {
          update3.MoveNext();
          Aver.AreEqual(update3.Current.Count, 3);
          Aver.AreEqual(((BSONStringElement)update3.Current["name"]).Value, "update3");
          Aver.AreEqual(((BSONInt32Element)update3.Current["age"]).Value, i);
        }
        update3.MoveNext();
        Aver.AreEqual(true, update3.EOF);

        Aver.AreEqual(result4.TotalDocumentsUpdatedAffected, 0);
        Aver.AreEqual(result4.TotalDocumentsAffected, 0);
        Aver.IsNull(result4.Upserted);
        Aver.IsNull(result4.WriteErrors);
        update4.MoveNext();
        Aver.AreEqual(true, update4.EOF);

        Aver.AreEqual(result5.TotalDocumentsUpdatedAffected, 0);
        Aver.AreEqual(result5.TotalDocumentsAffected, 1);
        Aver.AreEqual(result5.Upserted.Length, 1);
        Aver.IsNull(result5.WriteErrors);
        update5.MoveNext();
        Aver.AreEqual(update5.Current.Count, 3);
        Aver.AreEqual(((BSONStringElement)update5.Current["name"]).Value, "update5");
        Aver.AreEqual(((BSONInt32Element)update5.Current["age"]).Value, -1);
        update5.MoveNext();
        Aver.AreEqual(true, update5.EOF);

        Aver.AreEqual(result6.TotalDocumentsUpdatedAffected, 3);
        Aver.AreEqual(result6.TotalDocumentsAffected, 3);
        Aver.IsNull(result6.Upserted);
        Aver.IsNull(result6.WriteErrors);
        for (int i = 6; i <= 8; i++)
        {
          update6.MoveNext();
          Aver.AreEqual(update6.Current.Count, 3);
          Aver.AreEqual(((BSONStringElement)update6.Current["name"]).Value, "update6");
          Aver.AreEqual(((BSONInt32Element)update6.Current["age"]).Value, i);
        }
        update6.MoveNext();
        Aver.AreEqual(true, update6.EOF);
      }
    }

    [Run]
    public void Update_Parallel_SimpleStringInt23Entries()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 75000;
        const int CHUNK = 1500;

        foreach (var value in Enumerable.Range(0, CNT))
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + value))
                                    .Set(new BSONInt32Element("value", value)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, CNT/CHUNK, i =>
        {
          var result = collection.Update(new UpdateEntry(new Query("{ '$and': [ { value: {'$gte':"+i*CHUNK+"}}, { value: {'$lt':"+(i+1)*CHUNK+"}} ]}", false),
                                                         new Query("{ '$set': {name: 'updated'}}", false), true, false));
          Aver.AreEqual(result.TotalDocumentsUpdatedAffected, CHUNK);
          Aver.AreEqual(result.TotalDocumentsAffected, CHUNK);
          Aver.IsNull(result.Upserted);
          Aver.IsNull(result.WriteErrors);
        });

        var updated = collection.Find(new Query(@"{name: 'updated'}", false));
        for (int i = 0; i < CNT; i++)
        {
          updated.MoveNext();
          Aver.AreEqual(updated.Current.Count, 3);
          Aver.AreEqual(((BSONStringElement)updated.Current["name"]).Value, "updated");
        }
        updated.MoveNext();
        Aver.AreEqual(true, updated.EOF);
      }
    }

    [Run]
    public void Save_AsInsert()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "People"+i)));
          Aver.AreEqual(1, result.TotalDocumentsAffected);
          Aver.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Aver.AreEqual(1, result.Upserted.Length);
          Aver.IsNull(result.WriteErrors);
        }

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<10; i++)
        {
          all.MoveNext();
          Aver.AreEqual(all.Current.Count, 2);
          Aver.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Aver.IsTrue(((BSONObjectIDElement)all.Current["_id"]).Value.Bytes.MemBufferEquals(new BSONObjectID(1,2,3, (uint)i).Bytes));
        }
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Save_AsInsertAndUpdate()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "People"+i)));
          Aver.AreEqual(1, result.TotalDocumentsAffected);
          Aver.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Aver.AreEqual(1, result.Upserted.Length);
          Aver.IsNull(result.WriteErrors);
        }

        for (int i=5; i<10; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                                                         .Set(new BSONStringElement("name", "saved")));
          Aver.AreEqual(1, result.TotalDocumentsAffected);
          Aver.AreEqual(1, result.TotalDocumentsUpdatedAffected);
          Aver.IsNull(result.Upserted);
          Aver.IsNull(result.WriteErrors);
        }

        for (int i=10; i<15; i++)
        {
          var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3,(uint)i)))
                                                         .Set(new BSONStringElement("name", "saved")));
          Aver.AreEqual(1, result.TotalDocumentsAffected);
          Aver.AreEqual(0, result.TotalDocumentsUpdatedAffected);
          Aver.AreEqual(1, result.Upserted.Length);
          Aver.IsNull(result.WriteErrors);
        }

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<15; i++)
        {
          all.MoveNext();
          Aver.AreEqual(all.Current.Count, 2);
          Aver.IsTrue(((BSONObjectIDElement)all.Current["_id"]).Value.Bytes.MemBufferEquals(new BSONObjectID(1,2,3,(uint)i).Bytes));
          Aver.AreEqual(((BSONStringElement)all.Current["name"]).Value, i<5 ? "People" + i : "saved");
        }
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Save_Parallel_AsInsertAndUpdate()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 50000;
        const int SAVE_CNT = 75000;
        const int CHUNK = 2000;

        for (int i=0; i<CNT; i++)
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1,2,3, (uint)i)))
                  .Set(new BSONStringElement("name", "People" + i)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, SAVE_CNT/CHUNK, i =>
        {
          for (int j = 0; j < CHUNK; j++)
          {
            var increment = CHUNK*i+j;
            var result = collection.Save(new BSONDocument().Set(new BSONObjectIDElement("_id", new BSONObjectID(1, 2, 3, (uint)increment)))
                                                           .Set(new BSONStringElement("name", "saved")));
            Aver.AreEqual(result.TotalDocumentsAffected, 1);
            Aver.IsNull(result.WriteErrors);
            if (increment < CNT)
            {
              Aver.IsNull(result.Upserted);
              Aver.AreEqual(result.TotalDocumentsUpdatedAffected, 1);
            }
            else
            {
              Aver.AreEqual(result.Upserted.Length, 1);
              Aver.AreEqual(result.TotalDocumentsUpdatedAffected, 0);
            }
          }
        });

        var all = collection.Find(new Query(@"{}", false));
        for (int i=0; i<SAVE_CNT; i++)
        {
          all.MoveNext();
          Aver.AreEqual(all.Current.Count, 2);
          Aver.AreEqual(((BSONStringElement)all.Current["name"]).Value, "saved");
        }
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Delete_NoLimit()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<100; i++)
        {
          collection.Insert(new BSONDocument().Set(new BSONStringElement("name", "People" + i))
                                              .Set(new BSONInt32Element("value", i)));
        }

        var result = collection.Delete(new DeleteEntry(
                                         new Query("{ '$or': [{ name: 'People0' }, { '$and': [{ value: { '$gte': 1 }}, { value: { '$lt': 50 }}] }] }", false),
                                         DeleteLimit.None));
        var all = collection.Find(new Query(@"{}", false));

        Aver.AreEqual(50, result.TotalDocumentsAffected);
        Aver.AreEqual(0, result.TotalDocumentsUpdatedAffected);
        Aver.IsNull(result.Upserted);
        Aver.IsNull(result.WriteErrors);
        for (int i=50; i<100; i++)
        {
          all.MoveNext();
          Aver.AreEqual(all.Current.Count, 3);
          Aver.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Aver.AreEqual(((BSONInt32Element)all.Current["value"]).Value, i);
        }
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Delete_OnlyFirstMatch()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        for (int i=0; i<100; i++)
        {
          collection.Insert(new BSONDocument().Set(new BSONStringElement("name", "People" + i))
                                              .Set(new BSONInt32Element("value", i)));
        }

        var result1 = collection.Delete(new DeleteEntry(
                                         new Query("{ value: { '$gte': 0 }}", false),
                                         DeleteLimit.OnlyFirstMatch));
        var result2 = collection.Delete(new DeleteEntry(
                                         new Query("{ value: { '$lt': 1000 }}", false),
                                         DeleteLimit.OnlyFirstMatch));
        var all = collection.Find(new Query(@"{}", false));

        Aver.AreEqual(1, result1.TotalDocumentsAffected);
        Aver.AreEqual(0, result1.TotalDocumentsUpdatedAffected);
        Aver.IsNull(result1.Upserted);
        Aver.IsNull(result1.WriteErrors);
        Aver.AreEqual(1, result2.TotalDocumentsAffected);
        Aver.AreEqual(0, result2.TotalDocumentsUpdatedAffected);
        Aver.IsNull(result2.Upserted);
        Aver.IsNull(result2.WriteErrors);
        for (int i=2; i<100; i++)
        {
          all.MoveNext();
          Aver.AreEqual(all.Current.Count, 3);
          Aver.AreEqual(((BSONStringElement)all.Current["name"]).Value, "People"+i);
          Aver.AreEqual(((BSONInt32Element)all.Current["value"]).Value, i);
        }
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }

    [Run]
    public void Delete_Parallel()
    {
      using (var client = new MongoClient(NOPApplication.Instance, "My client"))
      {
        var db = client.DefaultLocalServer["db1"];
        db["t1"].Drop();
        var collection = db["t1"];

        const int CNT = 100000;
        const int CHUNK = 5000;

        foreach (var value in Enumerable.Range(0, CNT))
        {
          Aver.AreEqual(1, collection.Insert(
                  new BSONDocument().Set(new BSONStringElement("name", "People" + value))
                                    .Set(new BSONInt32Element("value", value)))
                                       .TotalDocumentsAffected);
        }

        Parallel.For(0, CNT/CHUNK, i =>
        {
          var result = collection.Delete(new DeleteEntry(new Query("{ '$and': [ { value: {'$gte':"+i*CHUNK+"}}, { value: {'$lt':"+(i+1)*CHUNK+"}} ]}", false),
                                                        DeleteLimit.None));
          Aver.AreEqual(result.TotalDocumentsUpdatedAffected, 0);
          Aver.AreEqual(result.TotalDocumentsAffected, CHUNK);
          Aver.IsNull(result.Upserted);
          Aver.IsNull(result.WriteErrors);
        });

        var all = collection.Find(new Query(@"{}", false));
        all.MoveNext();
        Aver.AreEqual(true, all.EOF);
      }
    }
  }
}
