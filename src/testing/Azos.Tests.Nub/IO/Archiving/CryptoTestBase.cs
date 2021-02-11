/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.IO.Archiving
{
  /// <summary>
  /// Provides base for writing archive test with crypto app api
  /// </summary>
  public abstract class CryptoTestBase : IRunnableHook
  {
    public static string APP_CRYPTO_CONF =
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
        hmac{key='base64:Mrb6L5KuBwuUsDwMiIgQvwi2Wmh6vWtUWS102C3Ep00fI1vKPEH2xnqbUPqF6NrOAa1WrPoSuI0lxpSEkr7kXw'}
        aes {key= 'base64:4SzhSsfx3426ZQaWzrK2CHtggblhCQPGyhDY6djtWFE'}
      }

      algorithm
      {
        name='aes2'  default=true
        type='Azos.Security.HMACAESCryptoMessageAlgorithm, Azos'
        hmac{key='base64:kDCgyN6ZjYW_5DbxvCeIQepo73ZHcSTA-jXkRZrRxJD4a-Nb_xjL2peKn2qPexB74b473OSkpu7eZfM62SbTEQ'}
        hmac{key='base64:1qMjqA5f12Yb7ZYIEgWBgfTxbRh-RuSJAoX6GzfQJjVQ6EILUy17jBf_Nrpp3HDtbeGdqcI1BMpPG2jUTHlZdw'}
        aes {key='base64:QdAo-dae468rStzc5EFc_BIcxdG99mhrPNYGPvd3mTk'}
        aes {key='base64:lnRTG7SPRLUWDhJ3yGPk8mf70sFJWpoi4qV91LHI_p4'}
      }
    }
  }
}";

    public static string APP_BAD_CRYPTO_CONF =
    @"   /* this algorithms swap the keys to imitate a different keyset */
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
        hmac{key='base64:kDCgyN6ZjYW_5DbxvCeIQepo73ZHcSTA-jXkRZrRxJD4a-Nb_xjL2peKn2qPexB74b473OSkpu7eZfM62SbTEQ'}
        aes {key='base64:lnRTG7SPRLUWDhJ3yGPk8mf70sFJWpoi4qV91LHI_p4'}
      }

      algorithm
      {
        name='aes2'  default=true
        type='Azos.Security.HMACAESCryptoMessageAlgorithm, Azos'
        hmac{key='base64:Mrb6L5KuBwuUsDwMiIgQvwi2Wmh6vWtUWS102C3Ep00fI1vKPEH2xnqbUPqF6NrOAa1WrPoSuI0lxpSEkr7kXw'}
        aes {key='base64:QdAo-dae468rStzc5EFc_BIcxdG99mhrPNYGPvd3mTk'}
        aes {key= 'base64:4SzhSsfx3426ZQaWzrK2CHtggblhCQPGyhDY6djtWFE'}
      }
    }
  }
}";

    private AzosApplication m_App;
    protected IApplication App => m_App;

    void IRunnableHook.Prologue(Runner runner, FID id)
     => m_App = new AzosApplication(true, null, APP_CRYPTO_CONF.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }

    public ICryptoManager CryptoMan => m_App.SecurityManager.Cryptography;
  }
}
