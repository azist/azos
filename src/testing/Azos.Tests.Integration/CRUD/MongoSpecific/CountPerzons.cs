/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Diagnostics;

using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MongoDb;

namespace Azos.Tests.Integration.CRUD.MongoSpecific
{
  /// <summary>
  /// Performs a server-side cursor count
  /// </summary>
  public class CountPerzons : MongoDbCRUDQueryHandlerBase
  {

    public CountPerzons(MongoDbDataStore store, string name) : base(store, new QuerySource(name,
        @"#pragma
modify=MyPerzon

{'Age': {'$gt': '$$fromAge', '$lt': '$$toAge'}}"))
    { }


    public override RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
    {
      var ctx = (MongoDbCRUDQueryExecutionContext)context;

      Azos.Data.Access.MongoDb.Connector.Collection collection;
      var qry = MakeQuery(ctx.Database, query, Source, out collection);

      var rrow = new TResult();

      var sw = Stopwatch.StartNew();

      rrow.Count = collection.Count(qry);//Performs server-side count over query

      rrow.Interval = sw.Elapsed;

      var result = new Rowset(Schema.GetForTypedDoc(typeof(TResult)));
      result.Add(rrow);
      return result;
    }

    public class TResult : TypedDoc
    {
      [Field]
      public long Count{ get; set;}

      [Field]
      public TimeSpan Interval { get; set;}
    }


  }
}
