/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;

namespace WinFormsTest.Glue
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
        name='async'
        type='Azos.Glue.Native.AsyncSlimBinding'
        io-buf-size='16384'
        max-async-io-ops='10000'

        client-transport
        {
            rcv-buf-size='131072'
            snd-buf-size='131072'
            Zno-delay='false'
        }
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
        contract-servers='TestServer.Glue.JokeServer, TestServer; TestServer.Glue.JokeCalculatorServer, TestServer'
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
