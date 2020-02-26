/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;

using Azos.Conf;
using Azos.Apps;

namespace Azos.Tests.Unit.AppModel
{
    [Runnable(TRUN.BASE, 3)]
    public class FinishNotifiablesTests
    {

        public static string RESULT;


        [Run]
        public void StartFinish()
        {
            var confSource=@" app{  starters{ starter{ type='Azos.Tests.Unit.AppModel.MySuperStarter, Azos.Tests.Unit'} }    }";
            RESULT = "";
            var conf = LaconicConfiguration.CreateFromString(confSource);
            using( var app = new AzosApplication(null, conf.Root))
            {

            }

            Aver.AreEqual("ABCD", RESULT);

        }



    }


            public class MySuperStarter: IApplicationStarter
            {

                public bool ApplicationStartBreakOnException
                {
                    get { return false; }
                }

                public void ApplicationStartBeforeInit(IApplication application)
                {
                   FinishNotifiablesTests.RESULT += "A";
                }

                public void ApplicationStartAfterInit(IApplication application)
                {
                   FinishNotifiablesTests.RESULT += "B";
                   ((IApplicationImplementation)application).RegisterAppFinishNotifiable( new SuperEnder());
                }

                public void Configure(IConfigSectionNode node)
                {

                }

                public string Name
                {
                    get { return "SuperStarter"; }
                }
            }

            public class SuperEnder : IApplicationFinishNotifiable
            {

                public void ApplicationFinishBeforeCleanup(IApplication application)
                {
                    FinishNotifiablesTests.RESULT += "C";
                }

                public void ApplicationFinishAfterCleanup(IApplication application)
                {
                    FinishNotifiablesTests.RESULT += "D";
                }

                public string Name
                {
                    get { return "SuperEnder"; }
                }
            }


}