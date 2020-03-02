/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Serialization.JSON;
using Azos.Web;
using Azos.Security;

using Oracle.ManagedDataAccess.Client;

namespace Azos.Data.Access.Oracle.Instrumentation
{
  /// <summary>
  /// Provides direct ORCL sql execution capability
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public sealed class DirectSql : ExternalCallRequest<OracleDataStoreBase>
  {
    public const int MAX = 1000;

    public DirectSql(OracleDataStoreBase store) : base (store){ }


    [Config]
    public string SQL{ get; set; }

    [Config("p|par|prms|pars|params|parameters")]
    public IConfigSectionNode Parameters { get; set;}

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"Pipes SQL directly into ORCL for execution.
supply Sql content via `SQL` attribute.
The maximum row count is capped at {0}".Args(MAX));

    public override ExternalCallResponse Execute()
    {
      var rows = new JsonDataArray();

      using(var connection = Context.GetConnection().GetAwaiter().GetResult())
      {
        using(var cmd = connection.CreateCommand())
        {
          cmd.CommandText = SQL;
          bindParams(cmd);
          using(var reader = cmd.ExecuteReader())
          {
            var cnt = 0;
            while(cnt<MAX && reader.Read())
            {
              cnt++;
              var row = new JsonDataMap();
              rows.Add(row);
              for(var i=0; i<reader.FieldCount; i++)
              {
                var n = reader.GetName(i);
                var v = reader.GetValue(i);
                if (v is DBNull) v = null;

                row[n] = v;
              }
            }
          }
        }
      }

      var json = rows.ToJson(JsonWritingOptions.PrettyPrintASCII);
      return new ExternalCallResponse(ContentType.JSON, json);
    }

    private void bindParams(OracleCommand cmd)
    {
      if (Parameters != null && Parameters.Exists) return;
      //cmd.Parameters.AddWithValue(p.Name, p.Value);
    }

  }
}
