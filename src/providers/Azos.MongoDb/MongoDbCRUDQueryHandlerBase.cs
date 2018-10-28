/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb
{
    /// <summary>
    /// A base for ICRUDQueryHandler-derivatives for mongo
    /// </summary>
    public abstract class MongoDbCRUDQueryHandlerBase : CRUDQueryHandler<MongoDbDataStore>
    {
        #region .ctor
            public MongoDbCRUDQueryHandlerBase(MongoDbDataStore store, string name) : base(store, name) { }
            public MongoDbCRUDQueryHandlerBase(MongoDbDataStore store, QuerySource source) : base(store, source) { }
        #endregion

        #region ICRUDQueryHandler
            public DataDocConverter Converter { get { return Store.Converter; } }


            public override Schema GetSchema(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public override Task<Schema> GetSchemaAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.GetSchema(context, query));
            }


            public override RowsetBase Execute(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
            {
              throw new NotImplementedException();
            }

            public override Task<RowsetBase> ExecuteAsync(ICRUDQueryExecutionContext context, Query query, bool oneRow = false)
            {
              return TaskUtils.AsCompletedTask( () => this.Execute(context, query, oneRow));
            }


            public override Cursor OpenCursor(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public override Task<Cursor> OpenCursorAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.OpenCursor(context, query) );
            }

            public override int ExecuteWithoutFetch(ICRUDQueryExecutionContext context, Query query)
            {
              throw new NotImplementedException();
            }

            public override Task<int> ExecuteWithoutFetchAsync(ICRUDQueryExecutionContext context, Query query)
            {
              return TaskUtils.AsCompletedTask( () => this.ExecuteWithoutFetch(context, query));
            }
        #endregion

        #region protected utility
          public Connector.Query MakeQuery(Connector.Database db, Query query, QuerySource source, out Connector.Collection collection)
          {
            if (source.ModifyTarget.IsNullOrWhiteSpace())
              throw new MongoDbDataAccessException(StringConsts.QUERY_MODIFY_TARGET_MISSING_ERROR + "\n" + Source.OriginalSource);

            collection = db[source.ModifyTarget];

            return MakeQuery(query, source);
          }

          public Connector.Query MakeQuery(Query query, QuerySource source)
          {
            var args = query.Select(p =>
            {
              var elm = Converter.ConvertCLRtoBSON(p.Name, p.Value, Store.TargetName);
              return new TemplateArg(p.Name, elm.ElementType, elm.ObjectValue);
            }).ToArray();

            return new Connector.Query(source.StatementSource, true, args);
          }

          protected Rowset MapBSONArrayToRowset(BSONArrayElement rowsetData, Type tDoc)
          {
            Schema schema = null;
            if(tDoc != null && typeof(TypedDoc).IsAssignableFrom(tDoc))
              schema = Schema.GetForTypedDoc(tDoc);

            Rowset result = null;
            if(schema != null)
              result = new Rowset(schema);

            for(var i = 0; i < rowsetData.Value.Length; i++)
            {
              var rowDoc = (rowsetData.Value[i] as BSONDocumentElement);
              if (rowDoc == null) continue;

              if (schema == null)
              {
                schema = Converter.InferSchemaFromBSONDocument(rowDoc.Value);
                result = new Rowset(schema);
              }

              var row = Doc.MakeDoc(schema, tDoc);
              Converter.BSONDocumentToDataDoc(rowDoc.Value, row, Store.TargetName);
              result.Add(row);
            }
            return result;
          }
        #endregion
    }
}
