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
  /// Optionally performs injection of session.DataContextName from "wv-data-ctx" header (if present)
  /// </summary>
  public sealed class AuthorizeSessionFilter : SessionFilter
  {
    public AuthorizeSessionFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
    public AuthorizeSessionFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
    public AuthorizeSessionFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
    public AuthorizeSessionFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode) { ConfigAttribute.Apply(this, confNode); }


    /// <summary>
    /// When true, injects "wv-data-ctx" header into session.DataContextName property. Off by default
    /// </summary>
    [Config]
    public bool InjectDataContext { get; set; }


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

      if (InjectDataContext)
      {
        var dcn = work.Request.Headers[SysConsts.HEADER_DATA_CONTEXT];
        if (dcn!=null)
        {
          dcn = dcn.Trim().TakeFirstChars(1024);
          session.DataContextName = dcn;
        }
      }

      var hdr = work.Request.Headers[WebConsts.HTTP_HDR_AUTHORIZATION]?.TrimStart(' ');
      if (hdr.IsNullOrWhiteSpace()) return session;//unauthorized

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

      session.User = App.SecurityManager.Authenticate(credentials);//authenticate the user
      work.SetAuthenticated(session.User.IsAuthenticated);
      return session;
    }

  }
}
