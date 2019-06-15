using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb
{
  /// <summary>
  /// Provides interface to run local `mongod` instance bundled with the application.
  /// This class is helpful for tasks such as organizing local key-ring buffers and other data storages
  /// in the environments where deployment and configuration of a separate MongoDb instance may not be feasible
  /// </summary>
  public class BundledMongoDb : DaemonWithInstrumentation<IApplicationComponent>
  {
    public const string PROCESS_CMD = "mongod";

    public const int STARTUP_TIMEOUT_MS_MIN = 3795;
    public const int STARTUP_TIMEOUT_MS_MAX = 1 * 60 * 1000;
    public const int STARTUP_TIMEOUT_MS_DFLT = 17539;

    public const int SHUTDOWN_TIMEOUT_MS_MIN = 1500;
    public const int SHUTDOWN_TIMEOUT_MS_MAX = 5 * 60 * 1000;
    public const int SHUTDOWN_TIMEOUT_MS_DFLT = 15379;


    public const int MONGO_PORT_DFLT = 1316;
    public const string MONGO_BIND_IP_DFLT = "localhost";

    public BundledMongoDb(IApplication app) : base(app) { }
    public BundledMongoDb(IApplicationComponent director) : base(director){ }


    private bool m_InstrumentationEnabled;
    private int m_StartupTimeoutMs = STARTUP_TIMEOUT_MS_DFLT;
    private int m_ShutdownTimeoutMs = SHUTDOWN_TIMEOUT_MS_DFLT;
    private Process m_ServerProcess;

    private int m_Mongo_port = MONGO_PORT_DFLT;
    private string m_Mongo_bind_ip = MONGO_BIND_IP_DFLT;
    private string m_Mongo_dbpath;
    private bool m_Mongo_quiet = true;
    private bool m_Mongo_directoryperdb = true;

    public override string ComponentLogTopic => MongoConsts.MONGO_TOPIC;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_DATA)]
    public override bool InstrumentationEnabled
    {
      get => m_InstrumentationEnabled;
      set => m_InstrumentationEnabled = value;
    }

    [Config(Default = STARTUP_TIMEOUT_MS_DFLT)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int StartupTimeoutMs
    {
      get => m_StartupTimeoutMs;
      set => m_StartupTimeoutMs = value.KeepBetween(STARTUP_TIMEOUT_MS_MIN, STARTUP_TIMEOUT_MS_MAX);
    }

    [Config(Default = SHUTDOWN_TIMEOUT_MS_DFLT)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int ShutdownTimeoutMs
    {
      get => m_ShutdownTimeoutMs;
      set => m_ShutdownTimeoutMs = value.KeepBetween(SHUTDOWN_TIMEOUT_MS_MIN, SHUTDOWN_TIMEOUT_MS_MAX);
    }


    /// <summary>
    /// Mongo port
    /// See: https://docs.mongodb.com/manual/reference/program/mongod/
    /// </summary>
    [Config("$mongo-port", Default = MONGO_PORT_DFLT)]
    public int Mongo_port
    {
      get => m_Mongo_port;
      set
      {
        CheckDaemonInactive();
        m_Mongo_port = value;
      }
    }

    /// <summary>
    /// The hostnames and/or IP addresses and/or full Unix domain socket paths on which mongod should listen for client connections.
    /// You may attach mongod to any interface. To bind to multiple addresses, enter a list of comma-separated values.
    /// Example: localhost,/tmp/mongod.sock
    /// See: https://docs.mongodb.com/manual/reference/program/mongod/
    /// </summary>
    [Config("$mongo-bind-ip", Default = MONGO_BIND_IP_DFLT)]
    public string Mongo_bind_ip
    {
      get => m_Mongo_bind_ip;
      set
      {
        CheckDaemonInactive();
        m_Mongo_bind_ip = value;
      }
    }

    /// <summary>
    /// When set suppresses output form database commands, repl activity etc.
    /// See: https://docs.mongodb.com/manual/reference/program/mongod/
    /// </summary>
    [Config("$mongo-quiet", Default = true)]
    public bool Mongo_quiet
    {
      get => m_Mongo_quiet;
      set
      {
        CheckDaemonInactive();
        m_Mongo_quiet = value;
      }
    }

    /// <summary>
    /// Sets the data directory, if null then uses default locations: /data/db on Linux and macOS, \data\db on Windows
    /// See: https://docs.mongodb.com/manual/reference/program/mongod/
    /// </summary>
    [Config("$mongo-quiet", Default = true)]
    public string Mongo_dbpath
    {
      get => m_Mongo_dbpath;
      set
      {
        CheckDaemonInactive();
        m_Mongo_dbpath = value;
      }
    }

    /// <summary>
    /// Uses a separate directory to store data for each database.
    /// See: https://docs.mongodb.com/manual/reference/program/mongod/
    /// </summary>
    [Config("$mongo-directoryperdb", Default = true)]
    public bool Mongo_directoyperdb
    {
      get => m_Mongo_directoryperdb;
      set
      {
        CheckDaemonInactive();
        m_Mongo_directoryperdb = value;
      }
    }

    /// <summary>
    /// Returns MongoDb connect string for the bundled instance. The daemon must be running or exception is thrown
    /// </summary>
    public string GetDatabaseConnectString(string dbName)
    {
      CheckDaemonActiveOrStarting();
      return GetDatabaseConnectStringUnsafe(dbName);
    }

    /// <summary>
    /// Returns MongoDB connection to the bundled instance for the specified database. The daemon must be running or exception is thrown
    /// </summary>
    public Database GetDatabase(string dbName)
    {
      CheckDaemonActiveOrStarting();
      return GetDatabaseUnsafe(dbName);
    }

    protected string GetDatabaseConnectStringUnsafe(string dbName)
    {
      return "mongo{{server='mongo://{0}:{1}' db='{2}'}}".Args(m_Mongo_bind_ip ?? "localhost", m_Mongo_port, dbName.NonBlank(nameof(dbName)));
    }

    protected Database GetDatabaseUnsafe(string dbName)
    {
      var cs = GetDatabaseConnectStringUnsafe(dbName);
      return App.GetMongoDatabaseFromConnectString(cs);
    }

    protected override void DoStart()
    {
      base.DoStart();
      var p = m_ServerProcess = new Process();
      //p.StartInfo.FileName = PROCESS_CMD;
      //p.StartInfo.WorkingDirectory = @"C:\mongo\4.0\bin";
      p.StartInfo.FileName = @"C:\mongo\4.0\bin\mongod.exe";

      //see:  https://docs.mongodb.com/manual/reference/program/mongod/
      var args = "--noauth --port {0}".Args(m_Mongo_port);

      if (m_Mongo_bind_ip.IsNotNullOrWhiteSpace())
        args += " --bind_ip \"{0}\"".Args(m_Mongo_bind_ip);

      if (m_Mongo_dbpath.IsNotNullOrWhiteSpace())
        args += " --dbpath \"{0}\"".Args(m_Mongo_dbpath);

      if (m_Mongo_quiet) args += " --quiet";

      if (m_Mongo_directoryperdb) args += " --directoryperdb";

      p.StartInfo.Arguments = args;
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.CreateNoWindow = true;
      p.StartInfo.RedirectStandardOutput = true;
      p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

      //p.OutputDataReceived += (sender, e) =>
      //{
      //  if (e.Data != null) m_BufferedOutput.AppendLine(e.Data);
      //};

      p.Start();//<===========
      p.BeginOutputReadLine();

      var sw = Stopwatch.StartNew();
      while(App.Active)
      {
        System.Threading.Thread.Sleep(2000);//give process ramp-up time

        if (m_ServerProcess.HasExited)
        {
          ensureProcessTermination("start", m_StartupTimeoutMs);
          AbortStart();
          throw new MongoDbConnectorException("Process crashed on start");
        }

        if (sw.ElapsedMilliseconds > m_StartupTimeoutMs)
        {
          ensureProcessTermination("start", m_StartupTimeoutMs);
          AbortStart();
          throw new MongoDbConnectorException("Process did not return success in the alloted time of {0}".Args(m_StartupTimeoutMs));
        }

        try
        {
           var db = GetDatabaseUnsafe("admin");
           db.Ping();//ensure the successful connection
           break;//success
        }
        catch(Exception error)
        {
           WriteLog(Log.MessageType.TraceD, nameof(DoStart), "Error trying to db.Ping() on start: "+error.ToMessageWithType(), error);
        }
      }
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      //dispatch serverShutdown command
      //use admin
      //db.shutdownServer()
      //https://docs.mongodb.com/manual/tutorial/manage-mongodb-processes/#terminate-mongod-processes


      //https://docs.mongodb.com/manual/reference/command/shutdown/#dbcmd.shutdown
      var db = App.GetMongoDatabaseFromConnectString(GetDatabaseConnectStringUnsafe("admin"));
      var body = new BSONDocument();
      body.Set(new BSONInt32Element("shutdown", 1));
      try
      {
      db.RunCommand(body);
      }
      catch(Exception error)
      {
        //log
      }
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();

      m_ServerProcess.WaitForExit(m_ShutdownTimeoutMs);

      ensureProcessTermination("stop", m_ShutdownTimeoutMs);
    }

    private void ensureProcessTermination(string operation, int timeout)
    {
      if (!m_ServerProcess.HasExited)
      {
        var rel = WriteLog(Log.MessageType.Critical,
                           nameof(DoWaitForCompleteStop),
                           "Bundled `{0}` process has not completed operation `{1}` within the allowed time frame of  {2} ms. Attempting to kill ...".Args(PROCESS_CMD, operation, timeout));

        m_ServerProcess.Kill();

        WriteLog(Log.MessageType.Critical, nameof(DoWaitForCompleteStop), "...killed", related: rel);
      }

      DisposeAndNull(ref m_ServerProcess);
    }

  }
}
