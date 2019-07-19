/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;
using Azos.Security.Tokens;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class ClientTokenRingTests : IRunnableHook
  {
      private static string conf =
      @"
app
{
  security
  {
    cryptography
    {
      algorithm
      {
        name='aes1'  default=true
        type='Azos.Security.HMACAESCryptoMessageAlgorithm, Azos'
        hmac{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3'}
        aes{key='0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6,7,8,9,0,1'}
        aes{key='fe,e4,a2,36,4f,5a,6e,7f,82,90,40,13,24,30,40,5c,6d,72,38,b9,a0,91,22,37,e4,65,76,17,a8,f9,e0,c1'}
        aes{key='01,3e,a4,86,4a,5a,6e,7f,82,90,40,13,24,30,40,5c,7d,72,31,ab,07,00,03,37,e4,65,76,a7,a8,e4,dd,f7'}
      }
    }
  }//security
}//app
";
    private AzosApplication m_App;
    private ClientTokenRing m_Ring;

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      m_App = new AzosApplication(null, conf.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      m_Ring = new ClientTokenRing(m_App.SecurityManager);
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_Ring);
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public async Task AccessToken_1()
    {
      var token = m_Ring.GenerateNew<AccessToken>();
      token.IssuedBy = "a1";
      token.ClientId = "client123";
      token.SubjectSysAuthToken = new SysAuthToken("test", "test content").ToString();

      var issued = await m_Ring.PutAsync(token);
      Aver.IsNotNull(issued);

      "Issued {0} of {1} chars: {2}".See(token.GetType().Name, issued.Length, issued);

      var got = await m_Ring.GetAsync<AccessToken>(issued);
      Aver.IsNotNull(got);

      got.See();

      Aver.AreEqual(token.ClientId, got.ClientId);
      Aver.AreEqual(token.IssuedBy, got.IssuedBy);
      Aver.AreEqual(token.SubjectSysAuthToken, got.SubjectSysAuthToken);
    }

    [Run]
    public async Task ClientAccessCodeToken_1()
    {
      var token = m_Ring.GenerateNew<ClientAccessCodeToken>();
      token.IssuedBy = "a1";
      token.ClientId = "client123";
      token.RedirectURI  = "http://disney.com";
      token.State = "state789";
      token.SubjectSysAuthToken = new SysAuthToken("test", "test content").ToString();

      var issued = await m_Ring.PutAsync(token);
      Aver.IsNotNull(issued);

      "Issued {0} of {1} chars: {2}".See(token.GetType().Name, issued.Length, issued);

      var got = await m_Ring.GetAsync<ClientAccessCodeToken>(issued);
      Aver.IsNotNull(got);

      got.See();

      Aver.AreEqual(token.ClientId, got.ClientId);
      Aver.AreEqual(token.IssuedBy, got.IssuedBy);
      Aver.AreEqual(token.RedirectURI, got.RedirectURI);
      Aver.AreEqual(token.State, got.State);
      Aver.AreEqual(token.SubjectSysAuthToken, got.SubjectSysAuthToken);
    }

    [Run]
    public async Task ClientAccessCodeToken_2_DifferentType_Unsafe_Validate()
    {
      var token = m_Ring.GenerateNew<ClientAccessCodeToken>();
      token.IssuedBy = "a1";
      token.ClientId = "client123";
      token.RedirectURI = "http://disney.com";
      token.State = "state789";
      token.SubjectSysAuthToken = new SysAuthToken("test", "test content").ToString();

      var issued = await m_Ring.PutAsync(token);
      Aver.IsNotNull(issued);

      "Issued {0} of {1} chars: {2}".See(token.GetType().Name, issued.Length, issued);

      var got = await m_Ring.GetAsync<ClientRefreshCodeToken>(issued);
      Aver.IsNull(got); //because token did not pass validation as it was gotten into a different type

      got = await m_Ring.GetUnsafeAsync<ClientRefreshCodeToken>(issued); //<-----UNSAFE
      Aver.IsNotNull(got);//however unsafe version did return the token

      got.See();

      Aver.AreEqual(token.ClientId, got.ClientId);
      Aver.AreEqual(token.IssuedBy, got.IssuedBy);
      Aver.AreEqual(token.RedirectURI, got.RedirectURI);
      Aver.AreEqual(token.State, got.State);
      Aver.AreEqual(token.SubjectSysAuthToken, got.SubjectSysAuthToken);

      var ve = got.Validate(RingToken.PROTECTED_MSG_TARGET);
      Aver.IsNotNull(ve);
      Aver.IsTrue( ve is FieldValidationException );
      "Expected and got: {0}".See(ve.ToMessageWithType());
      Aver.IsTrue(ve.Message.Contains("Type Mismatch"));

    }


  }
}

/*
Example AccessToken:

U2hM-oR3nGSxW-A4ZMbhKsd5wvT4_n9Up2VCQ3dai2WoFKMgHluWvIkavf0k6bRTevzwR8ZmxQDh0RidZt438tcPlZSYgVr_eqcSoZwp1qF9tn0EmxAs_MGEUFNpROVHDAuXGcNxpg7K_NvtF3xsz3vfxrJgH9_lr1sqbclY_7juL1RjDJm5FblonzaRzEg7kdl5okjlFTWOfrRXW4YPo1Q4IwfD86DSKhuDTfeKQ68
KoJ6gqM_YgnVaJHtDYLDrboc4iQUGZULeRJxcwv_mzJdr4EBJxGSlpkqOM_Q7pmSjhG14tUIHK3qgzpfop7E6rrn_4Xm-i7CexyWMkcwI51k4oC6UsGn8ggjyrk7ftBbTnmGB0mTx9VBYG3NV6OGoMm6ptvxlHd0AauKPvDYfhxouVWG6KdFbbduCdRRWxA9wh-PO6c-t01JtGWLriSqX9EarBSK1V6ji7GbVoucWzk
ZMcpEJ9CnWlJWxhNjZI3vSH4eplyIfyikQtCGBqzFLL1m8KIgohP3diUlOHLZcmXLSkqCB4Xl2BVS8CGoYrWepI0GaFT8XtCEPW8L_6fXGb5yH48fd6Cu6Tc27LxyaM_e6QaaDVo9qwPenp6yNZRSi8Fb4JlL-O5D55JqvkEIi402nuwncCBIE4mveTs4CUjRgeqxNzolNaH8B8K1rM68hbLDJFzIBNeWHihuDF6eRQ
IjpVXv23Z2aiIzibjSl4q0haajedwBg6S6GCtUDz7ONkc-oEaDs6CSCMyRX-V30_BSNizYIaP0iW6SjcObFmOnSkMsPlrYvOaa75f35VWym1oC2En_uOHGthbNciA6-YNhHRXcgxYkRpGPJYJZZ-bsOtx9Ga_Tqi4F9pPyT2u9voPSLV-M7jksPRVLA0q_3f-Dgi-IGDdyT98lnp-D-2ddbCpKaD8MKQuxA9KGd7SWw
EQWbv_E-OjDSofnA-Ukmqxbi-MZmcXFwZ7JDBYgf0LqlAvz4twRcdo2QgpUK1iE8pcBS1PdyamJ9jbW2GbdzK1lSPjxbBsz_Tt0DAWS14wlJCjxH2FbaFNUwAuuuQIWvpxCLZkvZuongmsfZs0f2NOPTUNkwX9-n26XI2tglbLbNkR6Gwdm0oD0w3VRia9vjvHaBQuevClKGMxuMqaaq2-9advRdmHQb6KIzFwkLOdA


  */



