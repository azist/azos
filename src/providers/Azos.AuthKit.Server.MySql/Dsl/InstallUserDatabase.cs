/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Data;
using Azos.Platform;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

using MySqlConnector;

namespace Azos.AuthKit.Server.MySql.Dsl
{
  public sealed class InstallUserDatabase : Step
  {
    public InstallUserDatabase(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string MySqlConnectString { get; set; }

    [Config]
    public string DbName { get; set; }

    [Config]
    public string SkipDbCreation { get; set; }

    [Config]
    public string SkipDdl { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var rel = Guid.NewGuid();
      var cs = Eval(MySqlConnectString.NonBlank(nameof(MySqlConnectString)), state);
      using (var cnn = new MySqlConnection(cs))
      {
        cnn.Open();
        doConnectionWork(cnn, rel, state);
      }

      return Task.FromResult<string>(null);
    }

    private void doConnectionWork(MySqlConnection cnn, Guid rel, JsonDataMap state)
    {
      using (var cmd = cnn.CreateCommand())
      {
        //Step 1. Create database
        if (!Eval(SkipDbCreation, state).AsBool(false))
        {
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Will create database", related: rel);
          createDatabase(cmd, rel, state);
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Db created", related: rel);
        }

        //Step 2. Create DDL
        if (!Eval(SkipDdl, state).AsBool(false))
        {
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "Will run DDL", related: rel);
          createDdl(cmd, rel, state);
          WriteLog(Log.MessageType.Info, nameof(doConnectionWork), "DDL ran", related: rel);
        }
      }
    }

    private string getDbn(JsonDataMap state)
    {
      return Eval(DbName.NonBlank(nameof(DbName)), state).NonBlankMinMax(5, 32, nameof(DbName));
    }

    private void createDatabase(MySqlCommand cmd, Guid rel, JsonDataMap state)
    {
      var ddl = typeof(MySqlUserStore).GetText("ddl.db_ddl.sql");
      var dbn = getDbn(state);
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Db is: {0}".Args(dbn), related: rel);

      ddl = ddl.Args(dbn);
      cmd.CommandText = ddl;
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Starting cmd exec...", related: rel, pars: ddl);

      sql(cmd, nameof(createDatabase), rel);
    }

    private void createDdl(MySqlCommand cmd, Guid rel, JsonDataMap state)
    {
      var dbn = getDbn(state);
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Set db to: {0}".Args(dbn), related: rel);
      var ddl = "use `{0}`";
      WriteLog(Log.MessageType.Info, nameof(createDatabase), "Starting cmd exec...", related: rel, pars: ddl);
      cmd.CommandText = ddl;
      sql(cmd, nameof(createDdl), rel);


      ddl = typeof(MySqlUserStore).GetText("ddl.idp_ddl.sql");
      cmd.CommandText = ddl;
      sql(cmd, nameof(createDdl), rel);
    }

    private void sql(MySqlCommand cmd, string from, Guid rel)
    {
      try
      {
        cmd.ExecuteNonQuery();
        WriteLog(Log.MessageType.Info, from, "...Done", related: rel);
      }
      catch (Exception error)
      {
        WriteLog(Log.MessageType.CatastrophicError, from, "Error: {0}".Args(error.ToMessageWithType()), related: rel, error: error);
        throw;
      }
    }
  }
}

