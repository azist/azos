/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Security.Tokens
{
  /// <summary>
  /// Implements token ring which stores tokens in protected messages on the client
  /// </summary>
  public sealed class ClientTokenRing : ApplicationComponent, ITokenRingImplementation
  {
    public ClientTokenRing(IApplicationComponent director) : base(director)
    {
      m_Deleted = new CappedSet<string>(this, StringComparer.OrdinalIgnoreCase);
      m_Deleted.SizeLimit = 1024 * 1024;
      m_Deleted.TimeLimitSec =  8 * // hrs
                               60 * // minutes
                               60;  // seconds
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Deleted);
      base.Destructor();
    }

    private CappedSet<string> m_Deleted;
    private string m_IssuerName;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    /// <summary> Establishes the issuer name used for new token production </summary>
    [Config]
    public string IssuerName
    {
      get => m_IssuerName.IsNotNullOrWhiteSpace() ? m_IssuerName : Log.Message.DefaultHostName;
      set => m_IssuerName = value;
    }

    void IConfigurable.Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }


    #region ITokenRing

    private JsonWritingOptions m_JsonOptions = new JsonWritingOptions{
      RowsAsMap = true,
      RowMapTargetName = RingToken.PROTECTED_MSG_TARGET
    };

    public async Task<TToken> GetAsync<TToken>(string token) where TToken : RingToken
    {
      var content = await GetUnsafeAsync<TToken>(token);

      if (content==null) return null; //fix #183

      try
      {
        var ve = content.Validate(RingToken.PROTECTED_MSG_TARGET);
        if (ve!=null)
        {
          WriteLog(Log.MessageType.TraceErrors, nameof(GetAsync), "Validate() returned: " + ve.ToMessageWithType(), ve);
          return null;
        }
      }
      catch (Exception error)
      {
        WriteLog(Log.MessageType.TraceErrors, nameof(GetAsync), "Validate() leaked: " + error.ToMessageWithType(), error);
        return null;
      }

      return content;
    }

    public Task<TToken> GetUnsafeAsync<TToken>(string token) where TToken : RingToken
    {
      if (token.IsNullOrWhiteSpace() || m_Deleted.Contains(token)) return Task.FromResult<TToken>(null);

      var deciphered = App.SecurityManager.PublicUnprotectMap(token.NonBlank(nameof(token)));

      //protected message integrity check will return null if token was tampered
      if (deciphered == null) return Task.FromResult<TToken>(null);

      TToken content = null;
      try
      {
        content = JsonReader.ToDoc<TToken>(deciphered, nameBinding: JsonReader.NameBinding.ByBackendName(RingToken.PROTECTED_MSG_TARGET));
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.TraceErrors, nameof(GetUnsafeAsync), "ToDoc() leaked: "+error.ToMessageWithType(), error);
        return Task.FromResult<TToken>(null);
      }

      //check expiration date
      var expire = content.ExpireUtcTimestamp;
      if (!expire.HasValue || expire < App.TimeSource.UTCNow) return Task.FromResult<TToken>(null);

      return Task.FromResult(content);
    }

    public Task<string> PutAsync(RingToken token)
    {
      var verr = token.NonNull(nameof(token)).Validate(RingToken.PROTECTED_MSG_TARGET);
      if (verr!=null)
        throw new SecurityException("Invalid token state: " + verr.ToMessageWithType(), verr);

      var ciphered = App.SecurityManager.PublicProtectAsString(token, m_JsonOptions);
      return Task.FromResult(ciphered);
    }

    public Task DeleteAsync(string token)
    {
      m_Deleted.Put(token);
      return Task.CompletedTask;
    }

    public Task Blacklist(IConfigSectionNode selector)
    {
     //todo Do we add blacklist table here?
      return Task.CompletedTask;
    }

    public TToken GenerateNew<TToken>(int expireInSeconds = 0) where TToken : RingToken
    {
      var token = Activator.CreateInstance<TToken>();
      token.Type = typeof(TToken).Name;

      //Guid is all that is used for client-side tokens ignoring token byte strength. The GUid serves as an additional nonce
      var guid = Guid.NewGuid().ToNetworkByteOrder();//16 bytes
      token.ID = guid.ToWebSafeBase64();
      token.IssuedBy = this.IssuerName;
      var now = App.TimeSource.UTCNow;
      token.IssueUtcTimestamp = now;
      token.VersionUtcTimestamp = now;
      token.ExpireUtcTimestamp = now.AddSeconds(expireInSeconds > 0 ? expireInSeconds : token.TokenDefaultExpirationSeconds);

      return token;
    }

    #endregion

  }
}
