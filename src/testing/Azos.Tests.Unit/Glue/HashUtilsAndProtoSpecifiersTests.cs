/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Scripting;

using Azos.Glue.Protocol;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE)]
    public class HashUtilsAndProtoSpecifiersTests
    {
        [Run]
        public void HashID_1()
        {
            var h1 = HashUtils.StringIDHash("Aum.Glue");
            var h2 = HashUtils.StringIDHash("Aum.Flue");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void HashID_2()
        {
            var h1 = HashUtils.StringIDHash("A");
            var h2 = HashUtils.StringIDHash("a");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void HashID_3()
        {
            var h1 = HashUtils.StringIDHash("Aum.Cluster.Glue");
            var h2 = HashUtils.StringIDHash("Aum.Cluster.App.Contracts");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void HashID_4()
        {
            var h1 = HashUtils.StringIDHash("Azos.Sky.Glue");
            var h2 = HashUtils.StringIDHash("Azos.Sky.Glue");

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreEqual(h1, h2);
        }


        [Run]
        public void TypeHash_1()
        {
            var h1 = HashUtils.TypeHash(typeof(INamed));
            var h2 = HashUtils.TypeHash(typeof(INamed));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreEqual(h1, h2);
        }

        [Run]
        public void TypeHash_2()
        {
            var h1 = HashUtils.TypeHash(typeof(INamed));
            var h2 = HashUtils.TypeHash(typeof(IOrdered));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void TypeHash_3()
        {
            var h1 = HashUtils.TypeHash(typeof(INamed));
            var h2 = HashUtils.TypeHash(typeof(Azos.Wave.WaveServer));

            Console.WriteLine(h1);
            Console.WriteLine(h2);

            Aver.AreNotEqual(h1, h2);
        }


        [Run]
        public void TypeSpec_1()
        {
            var s1 = new TypeSpec(typeof(INamed));
            var s2 = new TypeSpec(typeof(INamed));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsEqual(s1, s2);
            Aver.AreEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [Run]
        public void TypeSpec_2()
        {
            var s1 = new TypeSpec(typeof(INamed));
            var s2 = new TypeSpec(typeof(IOrdered));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsNotEqual(s1, s2);
            Aver.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }


        [Run]
        public void TypeSpec_3()
        {
            var s1 = new TypeSpec(typeof(Registry<Azos.Apps.Service>));
            var s2 = new TypeSpec(typeof(Registry<Azos.Glue.Binding>));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsNotEqual(s1, s2);
            Aver.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [Run]
        public void MethodSpec_1()
        {
            var s1 = new MethodSpec(typeof(Registry<Apps.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(Registry<Apps.Service>).GetMethod("Register"));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsEqual(s1, s2);
            Aver.AreEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [Run]
        public void MethodSpec_2()
        {
            var s1 = new MethodSpec(typeof(Registry<Azos.Apps.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(Registry<Azos.Apps.Service>).GetMethod("Unregister", new Type[]{typeof(string)}));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsNotEqual(s1, s2);
            Aver.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }

        [Run]
        public void MethodSpec_3()
        {
            var s1 = new MethodSpec(typeof(Registry<Apps.Service>).GetMethod("Register"));
            var s2 = new MethodSpec(typeof(DateUtils).GetMethod("ToSecondsSinceUnixEpochStart"));

            Console.WriteLine(s1);
            Console.WriteLine(s2);

            Aver.AreObjectsNotEqual(s1, s2);
            Aver.AreNotEqual( s1.GetHashCode(), s2.GetHashCode());
        }
    }
}
