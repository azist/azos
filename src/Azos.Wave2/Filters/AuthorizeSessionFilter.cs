/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Security;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Handles Authorization header with Basic and Bearer schemes creating IDPasswordCredentials or BearerCredentials respectively.
  /// Optionally performs injection of session.DataContextName from DEFAULT_DATA_CONTEXT_HEADER ("wv-data-ctx") header (if present).
  /// This filter is typically used for API server development
  /// </summary>
  public sealed class AuthorizeSessionFilter : SessionFilter
  {
    private const string BASIC = WebConsts.AUTH_SCHEME_BASIC + " ";
    private const string BEARER = WebConsts.AUTH_SCHEME_BEARER + " ";
    private const string SYSTOKEN = WebConsts.AUTH_SCHEME_SYSTOKEN + " ";

    public AuthorizeSessionFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
    public AuthorizeSessionFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
    public AuthorizeSessionFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
    public AuthorizeSessionFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode) { ConfigAttribute.Apply(this, confNode); }


    /// <summary>
    /// When set, reads the named request header and injects its content into session's DataContextName property
    /// </summary>
    [Config]
    [ExternalParameter("DataContextHeader",
                        ExternalParameterSecurityCheck.OnSet, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public string DataContextHeader { get; set; }


    /// <summary>
    /// When set, gets used instead of the standard `Authorization` header first, this way
    /// applications may be called from browsers with MODHeader modules which should not affect other
    /// applications sensitive to standard Authorization header
    /// </summary>
    [Config]
    [ExternalParameter("AltAuthorizationHeader",
                        ExternalParameterSecurityCheck.OnSet, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public string AltAuthorizationHeader { get ; set;}


    /// <summary>
    /// When set will bump the named Gate var on every bad auth request which did not produce a Valid user
    /// </summary>
    [Config]
    [ExternalParameter("GateBadAuthVar",
                        ExternalParameterSecurityCheck.OnSet, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public string GateBadAuthVar{ get; set; }


    /// <summary>
    /// WARNING: Use extreme caution!!! This property is used ONLY in cases of corporate-paralyzed environments.
    /// Sets default impersonation authorization header value which is used as the last resort while
    /// reading Authorization header. The system would use this value only WHEN no other Alt/Authorization headers supplied.
    /// Used to mock authentication of clients temporarily incapable of sending a true Authorization header.
    /// This property should never be set in production systems as it is basically a bypass of all security.
    /// The PRACTICAL need for this setting arose due to problems while setting system integration channels
    /// with a 3rd party due to exorbitant amounts of corporate bureaucracy, and inability to quickly re-deploy
    /// software with a different configuration. When set, provides the value as-if supplied by Authorization header coming
    /// in HttpRequest. During integration periods, the callers may temporarily be unable to send real Authorization header.
    /// </summary>
    [Config]
    [ExternalParameter("DefaultImpersonationAuthorizationHeaderValue",
                        ExternalParameterSecurityCheck.OnSet,
                        CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public string DefaultImpersonationAuthorizationHeaderValue { get; set; }



    /// <summary>
    /// When set and passed at the beginning of the Bearer credentials, treats bearer payload as payload with BASIC scheme.
    /// This is useful for temp integrations with various parties which demand sending authorization with Bearer scheme,
    /// yet IDP for token authorization is not available yet
    /// </summary>
    [Config]
    [ExternalParameter("BearerBasicPrefix",
                        ExternalParameterSecurityCheck.OnSet,
                        CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public string BearerBasicPrefix
    {
      get;
      set;
    }

    /// <summary>
    /// When set to true (false by default) enables `Authorization: Systoken {token}` scheme
    /// authentication via SysAuthTokens. This should ONLY be enabled inside the internal service perimeter and not for the
    /// externally/publicly-exposed services, as SysAuthToken is purely an internal system authentication method.
    /// </summary>
    [Config]
    [ExternalParameter("EnableSystemTokens",
                        ExternalParameterSecurityCheck.OnSet,
                        CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public bool EnableSystemTokens
    {
      get;
      set;
    }



    //disregard onlyExisting parameter, for APIs the session context is ephemeral
    protected internal override void FetchExistingOrMakeNewSession(WorkContext work, bool onlyExisting = false)
     => base.FetchExistingOrMakeNewSession(work, false);

    // There is nothing to extract
    protected override Guid? ExtractSessionID(WorkContext work, out ulong idSecret)
    {
      idSecret = 0;
      return null;
    }

    // Does not apply
    protected override WaveSession TryMakeSessionFromExistingLongTermToken(WorkContext work) => null;

    // Nowhere to store
    protected override void StowSession(WorkContext work)
     => Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(null);

    //We always make a new in-memory ephemeral session which gets collected right after this request
    protected override WaveSession MakeNewSessionInstance(WorkContext work)
    {
      //Always create new session
      var session = base.MakeNewSessionInstance(work);

      //try to inject session.DataContextName
      var dch = DataContextHeader;
      if (dch.IsNotNullOrWhiteSpace())
      {
        var dcn = work.Request.Headers[dch];
        if (dcn.IsNotNullOrWhiteSpace())
        {
          dcn = dcn.Trim().TakeFirstChars(1024);//hard limit safeguard
          session.DataContextName = dcn;
        }
      }

      string hdr = null;

      var altHdrName = AltAuthorizationHeader;
      if (altHdrName.IsNotNullOrWhiteSpace())
      {
        hdr = work.Request.Headers[altHdrName]?.TrimStart(' ');
      }

      if (hdr.IsNullOrWhiteSpace())
      {
        //real AUTHORIZATION header
        hdr = work.Request.Headers[WebConsts.HTTP_HDR_AUTHORIZATION]?.TrimStart(' ');
        if (hdr.IsNullOrWhiteSpace())
        {
          var mockHdrName = DefaultImpersonationAuthorizationHeaderValue;
          if (mockHdrName.IsNotNullOrEmpty())
           hdr = mockHdrName;
          else
           return session;//unauthorized
        }
      }

      User user;
      if (EnableSystemTokens && hdr.StartsWith(SYSTOKEN, StringComparison.OrdinalIgnoreCase))
      {
        var sysTokenContent = hdr.Substring(SYSTOKEN.Length).Trim();

        if (sysTokenContent.IsNullOrWhiteSpace() || // empty or null tokens treated as empty
            !SysAuthToken.TryParse(sysTokenContent, out var sysToken))
          throw HTTPStatusException.BadRequest_400("Bad [Authorization] header systoken");

        user = App.SecurityManager.Authenticate(sysToken);//authenticate the user using Systoken
      }
      else//credentials
      {
        Credentials credentials = null;

        try
        {
          if (hdr.StartsWith(BASIC, StringComparison.OrdinalIgnoreCase))
          {
            var basic = hdr.Substring(BASIC.Length).Trim();
            credentials = IDPasswordCredentials.FromBasicAuth(basic);
          }
          else if (hdr.StartsWith(BEARER, StringComparison.OrdinalIgnoreCase))
          {
            var pfxBasic = BearerBasicPrefix;
            var bearer = hdr.Substring(BEARER.Length).Trim();
            if (pfxBasic.IsNotNullOrWhiteSpace() && bearer.IsNotNullOrWhiteSpace() && bearer.StartsWith(pfxBasic))
            {
              var basicContent = bearer.Substring(pfxBasic.Length).Trim();
              credentials = IDPasswordCredentials.FromBasicAuth(basicContent);
            }
            else
            {
              credentials = new BearerCredentials(bearer);
            }
          }
        }
        catch { }

        if (credentials==null)
          throw HTTPStatusException.BadRequest_400("Bad [Authorization] header");

        user = App.SecurityManager.Authenticate(credentials);//authenticate the user
      }

      session.User = user;//<===========================================================I
      work.SetAuthenticated(user.IsAuthenticated);

      //gate bad traffic
      var gate = NetGate;
      if (!user.IsAuthenticated && gate != null && gate.Enabled)
      {
        var vname = GateBadAuthVar;
        if (vname.IsNotNullOrWhiteSpace())
        {
          gate.IncreaseVariable(IO.Net.Gate.TrafficDirection.Incoming,
                                work.EffectiveCallerIPEndPoint.Address.ToString(),
                                vname,
                                1);
        }
      }

      return session;
    }

  }
}
