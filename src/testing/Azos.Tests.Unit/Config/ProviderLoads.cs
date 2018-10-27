/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Unit.Config
{
    [Runnable(TRUN.BASE)]
    public class ProviderLoads
    {

    static string conf_xml = @"
 <root>
     <behaviors>
       <behavior type='USA' />
     </behaviors>
 </root>
";

 static string conf_laconic = @"
 root{
     behaviors{
       behavior{ type='USA' }
     }
 }
";


        [Run]
        public void ProviderLoadFromString_XML()
        {
           test( Configuration.ProviderLoadFromString(conf_xml, "xml") );
        }

        [Run]
        public void ProviderLoadFromString_Laconic()
        {
           test( Configuration.ProviderLoadFromString(conf_laconic, "laconf") );
        }

        private void test(Configuration config)
        {
           Aver.AreEqual( "USA", config.Root.Navigate("behaviors/behavior/$type").Value);
        }

    }

}
