using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security.Services
{
  public abstract class TokenRing : DaemonWithInstrumentation<IOAuthManager>, ITokenRingImplementation
  {
    public TokenRing(IOAuthManagerImplementation director) : base(director){ }

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set;}

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    #region ITokenRing
    public abstract TToken GenerateNewToken<TToken>() where TToken : RingToken;

    public abstract Task InvalidateAccessToken(string accessToken);

    public abstract Task InvalidateClient(string clientID);

    public abstract Task InvalidateSubject(AuthenticationToken token);

    public abstract Task<AccessToken> IssueAccessToken(User userClient, User targetUser);

    public abstract Task<ClientAccessCodeToken> LookupClientAccessCodeAsync(string accessCode);

    public abstract AuthenticationToken? MapAccessToken(string accessToken);

    public abstract AuthenticationToken TargetAuthenticationTokenFromContent(string content);

    public string TargetAuthenticationTokenToContent(AuthenticationToken token)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
