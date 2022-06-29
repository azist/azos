using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Data.Access;

namespace Azos.Tests.Integration.CRUD.Queries
{
  /// <summary>
  /// An example of custom handler which executes the query command
  /// </summary>
  public class CustomTestCommandHandler : CrudQueryHandler
  {
    public CustomTestCommandHandler(ICrudDataStore store, string name) : base(store, name)
    {
    }

    public override RowsetBase Execute(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
     => ExecuteAsync(context, query, oneRow).GetAwaiter().GetResult();

    public async override Task<RowsetBase> ExecuteAsync(ICrudQueryExecutionContext context, Query query, bool oneRow = false)
    {
      await Task.Delay(100);
      var result = new Rowset(Schema.GetForTypedDoc<Patient>());

      var p = query.FindParamByName("Msg")?.Value.AsString();

      result.Insert( new Patient{ First_Name = "Jack", Last_Name = "Nice", Address1 = p } );
      result.Insert( new Patient { First_Name = "Mary", Last_Name = "Dice", Address1 = p } );
      return result;
    }

    public override Doc ExecuteProcedure(ICrudQueryExecutionContext context, Query query)
     => throw new NotImplementedException();

    public override Task<Doc> ExecuteProcedureAsync(ICrudQueryExecutionContext context, Query query)
     => throw new NotImplementedException();

    public override Schema GetSchema(ICrudQueryExecutionContext context, Query query)
     => GetSchemaAsync(context, query).GetAwaiter().GetResult();

    public async override Task<Schema> GetSchemaAsync(ICrudQueryExecutionContext context, Query query)
    {
      await Task.Delay(100);
      return Schema.GetForTypedDoc<Patient>();
    }

    public override Cursor OpenCursor(ICrudQueryExecutionContext context, Query query)
     => throw new NotImplementedException();

    public override Task<Cursor> OpenCursorAsync(ICrudQueryExecutionContext context, Query query)
     => throw new NotImplementedException();
  }
}
