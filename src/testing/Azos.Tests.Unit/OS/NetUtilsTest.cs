/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Platform;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.OS
{

    [Runnable]
    public class NetUtilsTest
    {
        [Run]
        public void GetUniqueNetworkSignature()
        {
            var sig = NetworkUtils.GetMachineUniqueMACSignature();

            Console.WriteLine( sig );

            if (System.Environment.MachineName=="SEXTOD")
             Aver.AreEqual("9-08CC2E06-DD8F620D34473E5E", sig);
            else
             Aver.Fail("This test can only run on SEXTOD");
        }

        [Run]
        public void Computer_UniqueNetworkSignature()
        {
            var sig = Computer.UniqueNetworkSignature;

            Console.WriteLine( sig );

            if (System.Environment.MachineName=="SEXTOD")
             Aver.AreEqual("9-08CC2E06-DD8F620D34473E5E", sig);
            else
             Aver.Fail("This test can only run on SEXTOD");
        }

        [Run]
        public void HostNetInfo_1()
        {
            var hi = HostNetInfo.ForThisHost();

            Console.WriteLine(  hi.ToJson(JsonWritingOptions.PrettyPrint) );

            if (System.Environment.MachineName=="SEXTOD")
             Aver.IsTrue( hi.Adapters["{C93B4009-15C0-46A3-8C95-91610CAEBC4F}::Local Area Connection"].Addresses.ContainsName("192.168.1.70") );
            else
             Aver.Fail("This test can only run on SEXTOD");
        }
    }
}
