/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFX;
using Azos.Apps;
using Azos.Log;

using Azos.Scripting;

namespace Azos.Tests.Unit
{
    [Runnable]
    public class ScopeTest
    {
        [Run]
        public void ScopeExitTest()
        {
            int  n = 1;
            bool b = true;
            string s0 = "ok";

            {
                using (Scope.OnExit(() => n = 1))
                    n = 2;

                Aver.AreEqual(1, n);
            }

            {
                using (Scope.OnExit<int>(n, (x) => n = x))
                    n = 2;

                Aver.AreEqual(1, n);
            }

            {
                using (Scope.OnExit<int,bool>(n, b, (x,y) => {n=x; b=y;}))
                {
                    n = 5;
                    b = false;
                }

                Aver.AreEqual(1, n);
                Aver.AreEqual(true, b);
            }

            {
                using (Scope.OnExit<int,bool,string>(n, b, s0, (x,y,s) => {n=x; b=y; s0=s;}))
                {
                    n = 5;
                    b = false;
                    s0 = "bad";
                }

                Aver.AreEqual(1, n);
                Aver.AreEqual(true, b);
                Aver.AreEqual("ok", s0);
            }

        }
    }
}
