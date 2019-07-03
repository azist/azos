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
using Azos.Instrumentation;

namespace Azos.Security.Services
{
  /// <summary>
  /// Implements token ring which stores tokens in protected messages on the client
  /// </summary>
  public sealed class ClientTokenRing : DaemonWithInstrumentation<IApplicationComponent>, ITokenRingImplementation
  {
    public ClientTokenRing(IApplicationComponent director) : base(director) {}

    protected override void Destructor()
    {
      base.Destructor();
    }

    private string m_IssuerName;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set;}

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    /// <summary> Establishes the issuer name used for new token production </summary>
    [Config]
    public string IssuerName
    {
      get => m_IssuerName.IsNotNullOrWhiteSpace() ? m_IssuerName : Log.Message.DefaultHostName;
      set => m_IssuerName = value;
    }


    #region ITokenRing

    private JsonWritingOptions m_JsonOptions = new JsonWritingOptions{
      RowsAsMap = true,
      RowMapTargetName = RingToken.PROTECTED_MSG_TARGET
    };

    public Task<TToken> GetAsync<TToken>(string token) where TToken : RingToken
    {
      var deciphered = App.SecurityManager.PublicUnprotectMap(token);

      if (deciphered == null) return Task.FromResult<TToken>(null);

      var  got = JsonReader.ToDoc<TToken>(deciphered, nameBinding:  JsonReader.NameBinding.ByBackendName(RingToken.PROTECTED_MSG_TARGET));

      return Task.FromResult(got);
    }

    public Task<string> PutAsync(RingToken token)
    {
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
      var len = token.TokenByteStrength;

      //1. Guid pad is used as a RNG source based on MAC addr/clock
      //https://en.wikipedia.org/w/index.php?title=Universally_unique_identifier&oldid=755882275#Random_UUID_probability_of_duplicates
      var guid = Guid.NewGuid();
      var guidpad = guid.ToNetworkByteOrder();//16 bytes

      //2. Random token body
      var rnd = App.SecurityManager.Cryptography.GenerateRandomBytes(len);

      //3. Concat GUid pad with key
      var btoken = guidpad.AppendToNew(rnd);
      token.ID = Convert.ToBase64String(btoken, Base64FormattingOptions.None);

      token.IssuedBy = this.IssuerName;
      token.IssueUtc = App.TimeSource.UTCNow;
      token.ExpireUtc = token.IssueUtc.Value.AddSeconds(token.TokenDefaultExpirationSeconds);

      return token;
    }

    #endregion

  }
}
