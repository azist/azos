/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb
{
    /// <summary>
    /// Executes MongoDB CRUD script-based queries
    /// </summary>
    public sealed class MongoDbCRUDScriptQueryHandler : MongoDbCRUDQueryHandlerBase
    {
        public const string QUERY_PARAM_SKIP_COUNT  = "__SKIP_COUNT";
        public const string QUERY_PARAM_FETCH_BY    = "__FETCH_BY";
        public const string QUERY_PARAM_FETCH_LIMIT = "__FETCH_LIMIT";

        public MongoDbCRUDScriptQueryHandler(MongoDbDataStore store, QuerySource source) : base(store, source)
        {
        }


        public override Schema GetSchema(ICrudQueryExecutionContext context, Query query)
        {
          var ctx = (MongoDbCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          var doc = collection.FindOne(qry);
          if (doc==null) return null;

          return Store.Converter.InferSchemaFromBSONDocument(doc);
        }


        public override RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
        {
          var ctx = (MongoDbCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          Schema schema = null;
          var dtp = query.ResultDocType;
          if (dtp!=null && typeof(TypedDoc).IsAssignableFrom(dtp))
            schema = Schema.GetForTypedDoc(query.ResultDocType);


          Rowset result = null;
          if (schema!=null)
            result = new Rowset(schema);

          var p = query[QUERY_PARAM_SKIP_COUNT];
          var skipCount  = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_BY];
          var fetchBy    = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_LIMIT];
          var fetchLimit = p!=null ? p.Value.AsInt(-1) : -1;

          using(var cursor = collection.Find(qry, skipCount, oneRow ? 1: fetchBy))
            foreach(var doc in cursor)
            {
              if (schema==null)
              {
                schema = Store.Converter.InferSchemaFromBSONDocument(doc);
                result = new Rowset(schema);
              }

              var row = Doc.MakeDoc(schema, query.ResultDocType);
              Store.Converter.BSONDocumentToDataDoc(doc, row, Store.TargetName);
              result.Add( row );

              if (fetchLimit>0 && result.Count>=fetchLimit) break;
            }

          return result;
        }

        public override Cursor OpenCursor(ICrudQueryExecutionContext context, Query query)
        {
          var ctx = (MongoDbCRUDQueryExecutionContext)context;

          Connector.Collection collection;
          var qry = MakeQuery(ctx.Database, query, Source, out collection );

          Schema schema = null;
          var dtp = query.ResultDocType;
          if (dtp!=null && typeof(TypedDoc).IsAssignableFrom(dtp))
            schema = Schema.GetForTypedDoc(query.ResultDocType);


          var p = query[QUERY_PARAM_SKIP_COUNT];
          var skipCount  = p!=null ? p.Value.AsInt(0) : 0;

          p = query[QUERY_PARAM_FETCH_BY];
          var fetchBy    = p!=null ? p.Value.AsInt(0) : 0;

          var mcursor = collection.Find(qry, skipCount, fetchBy);
          var enumerable = enumOpenCursor(schema, query, mcursor);

          return new MongoDbCursor( mcursor, enumerable );
        }

            private IEnumerable<Doc> enumOpenCursor(Schema schema, Query query, Connector.Cursor mcursor)
            {
              using(mcursor)
                foreach(var bdoc in mcursor)
                {
                  if (schema==null)
                  {
                    schema = Store.Converter.InferSchemaFromBSONDocument(bdoc);
                  }

                  var ddoc = Doc.MakeDoc(schema, query.ResultDocType);
                  Store.Converter.BSONDocumentToDataDoc(bdoc, ddoc, Store.TargetName);
                  yield return ddoc;
                }
            }


        public override Doc ExecuteProcedure(ICrudQueryExecutionContext context, Query query)
        {
            var ctx = (MongoDbCRUDQueryExecutionContext)context;

            var qry = MakeQuery(query, Source);

            var affected = ctx.Database.RunCommand(qry) != null ? 1 : 0;

            return new RowsAffectedDoc(affected);
        }
    }
}
