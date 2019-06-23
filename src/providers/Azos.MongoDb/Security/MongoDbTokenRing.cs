using System;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data.Access.MongoDb;

namespace Azos.Security.Services
{
  /// <summary>
  /// Stores tokens in MongoDb instance
  /// </summary>
  public class MongoDbTokenRing : TokenRing
  {
    public const string CONFIG_MONGO_SECTION = "mongo";

    public MongoDbTokenRing(IApplication app) : base(app) { }
    public MongoDbTokenRing(IOAuthManagerImplementation director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_DBMS);
    }

    private BundledMongoDb m_DBMS;
    private MongoDbDataStore m_DataStore;//todo how to take connection from embedded instance?

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

    }

    protected override void DoStart()
    {
      base.DoStart();
      m_DBMS.Start();
    }

    protected override void DoSignalStop()
    {
      m_DBMS?.SignalStop();
      base.DoSignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      m_DBMS?.WaitForCompleteStop();
    }

    public override TToken GenerateNewToken<TToken>()
    {
      throw new NotImplementedException();
    }

    public override Task InvalidateAccessToken(string accessToken)
    {
      throw new NotImplementedException();
    }

    public override Task InvalidateClient(string clientID)
    {
      throw new NotImplementedException();
    }

    public override Task InvalidateSubject(AuthenticationToken token)
    {
      throw new NotImplementedException();
    }

    public override Task<AccessToken> IssueAccessToken(User userClient, User targetUser)
    {
      throw new NotImplementedException();
    }

    public override Task<ClientAccessCodeToken> LookupClientAccessCodeAsync(string accessCode)
    {
      throw new NotImplementedException();
    }

    public override AuthenticationToken MapSubjectAuthenticationTokenFromContent(string content)
    {
      throw new NotImplementedException();
    }

    public override string MapSubjectAuthenticationTokenToContent(AuthenticationToken token)
    {
      throw new NotImplementedException();
    }

    protected override AccessToken DoFetchAccessToken(string accessToken, DateTime utcNow)
    {
      throw new NotImplementedException();
    }
    #endregion

  }
}
