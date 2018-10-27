/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Unit.AppModel
{
    [Runnable(TRUN.BASE, 3)]
    public class StarterTests
    {

        public static string RESULT;


        [Run]
        public void Starter1()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit'}
        starter{ name='Boris' type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit'}
    }

 }
 ";
            RESULT = "";
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

            Aver.AreEqual("Alex Before;Boris Before;Alex After;Boris After;", RESULT);

        }

        [Run]
        public void Starter2_WithException_NoBreak()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit'}
        starter{ name='Boris' type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit'}
    }

 }
 ";
            RESULT = null;//NULL REFERENCE should happen
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

            Aver.IsNull( RESULT); //no exception

        }

        [Run]
        [Aver.Throws(typeof(AzosException),
                           Message="Error calling Starter.ApplicationStartBeforeInit() 'Alex'. Exception: [System.NullReferenceException]",
                           MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
        public void Starter2_WithException_Break()
        {
 var confSource=@"
 nfx{
    starters{
        starter{ name='Alex'  type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit' application-start-break-on-exception='true'}
        starter{ name='Boris' type='Azos.Tests.Unit.AppModel.MyStarter, Azos.Tests.Unit'}
    }

 }
 ";
            RESULT = null;//NULL REFERENCE should happen
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new ServiceBaseApplication(null, conf.Root))
            {

            }

        }


    }


            public class MyStarter: IApplicationStarter
            {

                [Config]
                public bool ApplicationStartBreakOnException
                {
                    get;
                    set;
                }

                public void ApplicationStartBeforeInit(IApplication application)
                {
                   StarterTests.RESULT.Trim();//causes null reference
                   StarterTests.RESULT += "{0} Before;".Args(Name);
                }

                public void ApplicationStartAfterInit(IApplication application)
                {
                   StarterTests.RESULT.Trim();//causes null reference
                   StarterTests.RESULT += "{0} After;".Args(Name);
                }

                public void Configure(IConfigSectionNode node)
                {
                    ConfigAttribute.Apply(this, node);
                }

                [Config]
                public string Name
                {
                    get;
                    set;
                }
            }


}