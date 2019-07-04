using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data.Access.MongoDb;

namespace Azos.Security.Services
{
  /// <summary>
  /// Stores tokens in MongoDb instance
  /// </summary>
  public class MongoDbTokenRing : ServerTokenRingBase
  {
    public const string CONFIG_MONGO_SECTION = "mongo";

    public MongoDbTokenRing(IApplicationComponent director) : base(director) { }

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

    public override Task<TToken> GetAsync<TToken>(string token)
    {
      throw new NotImplementedException();
    }

    public override Task<TToken> GetUnsafeAsync<TToken>(string token)
    {
      throw new NotImplementedException();
    }

    public override Task<string> PutAsync(RingToken token)
    {
      throw new NotImplementedException();
    }

    public override Task Blacklist(IConfigSectionNode selector)
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
