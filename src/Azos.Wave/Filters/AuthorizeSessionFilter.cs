/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Security;
using Azos.Conf;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Handles Authorization header with Basic and Bearer schemes creating IDPasswordCredentials or BearerCredentials respectively.
  /// Optionally performs injection of session.DataContextName from "wv-data-ctx" header (if present).
  /// This filter is typically used for API server development
  /// </summary>
  public sealed class AuthorizeSessionFilter : SessionFilter
  {
    public AuthorizeSessionFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
    public AuthorizeSessionFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
    public AuthorizeSessionFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
    public AuthorizeSessionFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode) { ConfigAttribute.Apply(this, confNode); }


    /// <summary>
    /// When set, reads the named request header and injects its content into session's DataContextName property
    /// </summary>
    [Config]
    public string DataContextHeader { get; set; }


    /// <summary>
    /// When set, gets used instead of the standard `Authorization` header first, this way
    /// applications may be called from browsers with MODHeader modules which should not affect other
    /// applications sensitive to standard Authorization header
    /// </summary>
    [Config]
    public string AltAuthorizationHeader { get ; set;}


    /// <summary>
    /// When set will bump the named Gate var on every bad auth request which did not produce a Valid user
    /// </summary>
    [Config]
    public string GateBadAuthVar{ get; set; }


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
      const string BASIC = WebConsts.AUTH_SCHEME_BASIC + " ";
      const string BEARER = WebConsts.AUTH_SCHEME_BEARER + " ";

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

      if (AltAuthorizationHeader.IsNotNullOrWhiteSpace())
      {
        hdr = work.Request.Headers[AltAuthorizationHeader]?.TrimStart(' ');
      }

      if (hdr.IsNullOrWhiteSpace())
      {
        hdr = work.Request.Headers[WebConsts.HTTP_HDR_AUTHORIZATION]?.TrimStart(' ');
        if (hdr.IsNullOrWhiteSpace()) return session;//unauthorized
      }

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
          var bearer = hdr.Substring(BEARER.Length).Trim();
          credentials = new BearerCredentials(bearer);
        }
      }
      catch { }

      if (credentials==null)
        throw HTTPStatusException.BadRequest_400("Bad [Authorization] header");

      var user = App.SecurityManager.Authenticate(credentials);//authenticate the user
      session.User = user;
      work.SetAuthenticated(user.IsAuthenticated);

      //gate traffic
      if (!user.IsAuthenticated && NetGate!=null && NetGate.Enabled)
      {
        var vn = GateBadAuthVar;
        if (vn.IsNotNullOrWhiteSpace())
        {
          NetGate.IncreaseVariable(IO.Net.Gate.TrafficDirection.Incoming,
                                     work.EffectiveCallerIPEndPoint.Address.ToString(),
                                     vn,
                                     1);
        }
      }

      return session;
    }

  }
}
