/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.Glue.Native;
using Azos.Glue;
using Azos.IO;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE)]
    public class NodeTests
    {
        [Run]
        public void Node1()
        {
            var n = new Node("http://server:9045");
            Aver.AreEqual("server", n.Host);
            Aver.AreEqual("http", n.Binding);
            Aver.AreEqual("9045", n.Service);
        }

        [Run]
        public void Node2()
        {
            var n = new Node("http://server=127.0.0.1;interface=eth0:hgov");
            Aver.AreEqual("server=127.0.0.1;interface=eth0", n.Host);
            Aver.AreEqual("http", n.Binding);
            Aver.AreEqual("hgov", n.Service);
        }


        [Run]
        public void Node3()
        {
            var n = new Node("server:1891");
            Aver.AreEqual("server", n.Host);
            Aver.AreEqual(string.Empty, n.Binding);
            Aver.AreEqual("1891", n.Service);
        }

        [Run]
        public void Node4()
        {
            var n = new Node("http://server");
            Aver.AreEqual("server", n.Host);
            Aver.AreEqual("http", n.Binding);
            Aver.AreEqual(string.Empty, n.Service);
        }

        [Run]
        public void Node5()
        {
            var n = new Node("server");
            Aver.AreEqual("server", n.Host);
            Aver.AreEqual(string.Empty, n.Binding);
            Aver.AreEqual(string.Empty, n.Service);
        }

    }
}
