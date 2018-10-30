using System;

using Azos.Glue;
using Azos.Serialization.JSON;
using Azos.Security.Permissions.Admin;

namespace Azos.Sky.Contracts
{
    /// <summary>
    /// Represents a contract for working with remote entities using terminal/command approach
    /// </summary>
    [Glued]
    [AuthenticationSupport]
    [RemoteTerminalOperatorPermission]
    [LifeCycle(ServerInstanceMode.Stateful, SysConsts.REMOTE_TERMINAL_TIMEOUT_MS)]
    public interface IRemoteTerminal : ISkyService
    {
        [Constructor]
        RemoteTerminalInfo Connect(string who);

        string Execute(string command);

        [Destructor]
        string Disconnect();
    }

    /// <summary>
    /// Contract for client of IRemoteTerminal svc
    /// </summary>
    public interface IRemoteTerminalClient : ISkyServiceClient, IRemoteTerminal {  }


    /// <summary>
    /// Provides info about remote terminal to connecting clients
    /// </summary>
    [Serializable]
    public sealed class RemoteTerminalInfo
    {
      public RemoteTerminalInfo(){}
      public RemoteTerminalInfo(JSONDataMap map)
      {
        TerminalName    = map["TerminalName"].AsString();
        WelcomeMsg      = map["WelcomeMsg"].AsString();
        Host            = map["Host"].AsString();
        AppName         = map["AppName"].AsString();
        ServerLocalTime = map["ServerLocalTime"].AsDateTime();
        ServerUTCTime   = map["ServerUTCTime"].AsDateTime();
      }

      public string TerminalName      { get; internal set;}
      public string WelcomeMsg        { get; internal set;}
      public string Host              { get; internal set;}
      public string AppName           { get; internal set;}
      public DateTime ServerLocalTime { get; internal set;}
      public DateTime ServerUTCTime   { get; internal set;}
    }
}
