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

    public const int SHUTDOWN_TIMEOUT_MS_MIN = 1500;
    public const int SHUTDOWN_TIMEOUT_MS_MAX = 5 * 60 * 1000;
    public const int SHUTDOWN_TIMEOUT_MS_DFLT = 5379;


    public const int MONGO_PORT_DFLT = 1316;
    public const string MONGO_BIND_IP_DFLT = "localhost";

    public BundledMongoDb(IApplication app) : base(app) { }
    public BundledMongoDb(IApplicationComponent director) : base(director){ }


    private bool m_InstrumentationEnabled;
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
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      //dispatch serverShutdown command
      //use admin
      //db.shutdownServer()
      //https://docs.mongodb.com/manual/tutorial/manage-mongodb-processes/#terminate-mongod-processes

      var db = App.GetMongoDatabaseFromConnectString("mongo{{server='mongo://{0}:{1}' db='admin'}}".Args(m_Mongo_bind_ip ?? "localhost", m_Mongo_port));
      var body = new BSONDocument();
      body.Set(new BSONInt32Element("shutdownServer", 1));
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

      if (!m_ServerProcess.HasExited)
      {
        var rel = WriteLog(Log.MessageType.Critical, nameof(DoWaitForCompleteStop), "Bundled `{0}` process has not exited after {1} ms shutdown timeout. Attempting to kill ...".Args(PROCESS_CMD, m_ShutdownTimeoutMs));
        m_ServerProcess.Kill();
        WriteLog(Log.MessageType.Critical, nameof(DoWaitForCompleteStop), "...killed", related: rel);
      }

      DisposeAndNull(ref m_ServerProcess);
    }

  }
}
