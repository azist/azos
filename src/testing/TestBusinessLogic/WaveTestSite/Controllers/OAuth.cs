using System;
using System.Collections.Generic;
using System.Text;

using Azos.Security.Services;

namespace WaveTestSite.Controllers
{
  public class OAuth : OAuthControllerBase
  {
    public OAuth() : base() { }
    public OAuth(IOAuthModule oauth) : base(oauth){ }
  }
}
