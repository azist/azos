/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;

namespace Azos.Tests.Integration.Glue
{
  public class JokeHelper
  {
    const string CONFIG_STR = @"
app
{
  disk-root=$(~AZOS_TEST_ROOT)\
  log-root=$(\$disk-root)
  log-csv='Azos.Log.Destinations.CSVFileDestination, Azos'
  debug-default-action='Log,Throw'

  glue
  {
    bindings
    {
      binding
      {
        name='sync'
        type='Azos.Glue.Native.SyncBinding'
        max-msg-size='65535'

        client-inspectors
        {
          inspector { type='BusinessLogic.TextInfoReporter, BusinessLogic' }
        }

        client-transport
        {
          rcv-buf-size='8192'
          snd-buf-size='8192'
          rcv-timeout='10000'
          snd-timeout='10000'
        }
      }

      binding
      {
        name='mpx'
        type='Azos.Glue.Native.MpxBinding, Azos'
      }

      binding
      {
        name='inproc'
        type='Azos.Glue.Native.InProcBinding, Azos'
      }
    }

    servers
    {
      server
      {
        name='local'
        node='inproc://localhost'
        contract-servers='BusinessLogic.Server.JokeServer, BusinessLogic; BusinessLogic.Server.JokeCalculatorServer, BusinessLogic'
      }
    }
  }
}
";

    public static AzosApplication MakeApp()
    {
      var configuration = LaconicConfiguration.CreateFromString(CONFIG_STR);

      return new AzosApplication(new string[] { }, configuration.Root);
    }
  }
}
