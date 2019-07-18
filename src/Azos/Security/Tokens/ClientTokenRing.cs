/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Security.Tokens
{
  /// <summary>
  /// Implements token ring which stores tokens in protected messages on the client
  /// </summary>
  public sealed class ClientTokenRing : ApplicationComponent, ITokenRingImplementation
  {
    public ClientTokenRing(IApplicationComponent director) : base(director) {}

    protected override void Destructor()
    {
      base.Destructor();
    }

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

      var ve = content.Validate(RingToken.PROTECTED_MSG_TARGET);
      if (ve!=null) return null;

      return content;
    }

    public Task<TToken> GetUnsafeAsync<TToken>(string token) where TToken : RingToken
    {
      var deciphered = App.SecurityManager.PublicUnprotectMap(token.NonBlank(nameof(token)));

     //// Console.WriteLine(deciphered.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap));

      //protected message integrity check will return null if token was tampered
      if (deciphered == null) return Task.FromResult<TToken>(null);

      var content = JsonReader.ToDoc<TToken>(deciphered, nameBinding: JsonReader.NameBinding.ByBackendName(RingToken.PROTECTED_MSG_TARGET));

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

    public Task Blacklist(IConfigSectionNode selector)
    {
     //todo Do we add blacklist table here?
      return Task.CompletedTask;
    }

    public TToken GenerateNew<TToken>() where TToken : RingToken
    {
      var token = Activator.CreateInstance<TToken>();
      token.Type = typeof(TToken).Name;

      //Guid is all that is used for client-side tokens ignoring token byte strength. The GUid serves as an additional nonce
      var guid = Guid.NewGuid().ToNetworkByteOrder();//16 bytes
      token.ID = guid.ToWebSafeBase64();
      token.IssuedBy = this.IssuerName;
      token.IssueUtcTimestamp = token.VersionUtcTimestamp = App.TimeSource.UTCNow;
      token.ExpireUtcTimestamp = token.IssueUtcTimestamp.Value.AddSeconds(token.TokenDefaultExpirationSeconds);

      return token;
    }

    #endregion

  }
}
