using System;
using System.Collections.Generic;
using System.Text;

using Azos.Security.Services;
using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  [NoCache]
  public class OAuth : OAuthControllerBase
  {
    public OAuth() : base() { }
    public OAuth(IOAuthModule oauth) : base(oauth){ }
  }
}
