/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;

using Azos.Scripting;

using Azos.IO;
using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
    [Runnable]
    public class SlimObjectGraphTests
    {

        public class ObjectA
        {
           public ObjectA Another1;
           public ObjectA Another2;
           public ObjectA Another3;
           public ObjectA Another4;
           public ObjectA Another5;
           public ObjectA Another6;
           public ObjectA Another7;
           public ObjectA Another8;
           public ObjectA Another9;
           public ObjectA Another10;

           public int AField;
        }

        public class ObjectB : ObjectA
        {
           public int BField;
        }




        [Run]
        public void T01()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA(){ AField = -890};

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA));

            Aver.AreEqual(-890, deser.AField);
            Aver.IsNull( deser.Another1 );
            Aver.IsNull( deser.Another2 );
            Aver.IsNull( deser.Another3 );
            Aver.IsNull( deser.Another4 );
            Aver.IsNull( deser.Another5 );
            Aver.IsNull( deser.Another6 );
            Aver.IsNull( deser.Another7 );
            Aver.IsNull( deser.Another8 );
            Aver.IsNull( deser.Another9 );
            Aver.IsNull( deser.Another10 );
          }
        }


        [Run]
        public void T02()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 7892}
            };

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA));
            Aver.AreEqual(2345,  deser.AField );

            Aver.IsNotNull( deser.Another1 );
            Aver.AreEqual(7892,  deser.Another1.AField );
            Aver.IsNull( deser.Another2 );
            Aver.IsNull( deser.Another3 );
            Aver.IsNull( deser.Another4 );
            Aver.IsNull( deser.Another5 );
            Aver.IsNull( deser.Another6 );
            Aver.IsNull( deser.Another7 );
            Aver.IsNull( deser.Another8 );
            Aver.IsNull( deser.Another9 );
            Aver.IsNull( deser.Another10 );

          }
        }


        [Run]
        public void T03()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 9001},
              Another2 = new ObjectA{ AField = 9002},
              Another3 = new ObjectA{ AField = 9003},
              Another4 = new ObjectA{ AField = 9004},
              Another5 = new ObjectA{ AField = 9005},
              Another6 = new ObjectA{ AField = 9006},
              Another7 = new ObjectA{ AField = 9007},
              Another8 = new ObjectA{ AField = 9008},
              Another9 = new ObjectA{ AField = 9009},
              Another10 = new ObjectA{ AField = 9010},
            };

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA));
            Aver.AreEqual(2345,  deser.AField );

            Aver.IsNotNull( deser.Another1 );
            Aver.IsNotNull( deser.Another1 );
            Aver.IsNotNull( deser.Another2 );
            Aver.IsNotNull( deser.Another3 );
            Aver.IsNotNull( deser.Another4 );
            Aver.IsNotNull( deser.Another5 );
            Aver.IsNotNull( deser.Another6 );
            Aver.IsNotNull( deser.Another7 );
            Aver.IsNotNull( deser.Another8 );
            Aver.IsNotNull( deser.Another9 );
            Aver.IsNotNull( deser.Another10 );

             Aver.AreEqual(9001,  deser.Another1.AField );
             Aver.AreEqual(9002,  deser.Another2.AField );
             Aver.AreEqual(9003,  deser.Another3.AField );
             Aver.AreEqual(9004,  deser.Another4.AField );
             Aver.AreEqual(9005,  deser.Another5.AField );
             Aver.AreEqual(9006,  deser.Another6.AField );
             Aver.AreEqual(9007,  deser.Another7.AField );
             Aver.AreEqual(9008,  deser.Another8.AField );
             Aver.AreEqual(9009,  deser.Another9.AField );
             Aver.AreEqual(9010,  deser.Another10.AField );
          }
        }


        [Run]
        public void T04()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA
            {
              AField = 2345,
              Another1 = new ObjectA{ AField = 7892},
              Another2 = new ObjectB{ AField = 5678, BField = -12}
            };

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA));
            Aver.AreEqual(2345,  deser.AField );

            Aver.IsNotNull( deser.Another1 );
            Aver.AreEqual(7892,  deser.Another1.AField );

            Aver.IsNotNull( deser.Another2 );
            Aver.AreEqual(5678,  deser.Another2.AField );

            Aver.IsTrue( deser.Another2.GetType() == typeof(ObjectB));
            Aver.AreEqual(-12,  ((ObjectB)deser.Another2).BField );

            Aver.IsNull( deser.Another3 );
            Aver.IsNull( deser.Another4 );
            Aver.IsNull( deser.Another5 );
            Aver.IsNull( deser.Another6 );
            Aver.IsNull( deser.Another7 );
            Aver.IsNull( deser.Another8 );
            Aver.IsNull( deser.Another9 );
            Aver.IsNull( deser.Another10 );

          }
        }



        [Run]
        public void T05()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA();

             root.AField = 2345;
             root.Another1 = new ObjectA{ AField = 27892};
             root.Another2 = new ObjectB{ AField = -278, BField = -12, Another1 = root};

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA;

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA));
            Aver.AreEqual(2345,  deser.AField );

            Aver.IsNotNull( deser.Another1 );
            Aver.IsTrue( deser.Another1.GetType() == typeof(ObjectA));
            Aver.AreEqual(27892,  deser.Another1.AField );

            Aver.IsNotNull( deser.Another2 );
            Aver.AreEqual(-278,  deser.Another2.AField );

            Aver.IsTrue( deser.Another2.GetType() == typeof(ObjectB));
            Aver.AreEqual(-12,  ((ObjectB)deser.Another2).BField );

            Aver.IsNotNull( deser.Another2.Another1 );
            Aver.IsTrue( object.ReferenceEquals(deser, deser.Another2.Another1));
            Aver.IsTrue( deser.Another2.GetType() == typeof(ObjectB));

            Aver.IsNull( deser.Another3 );
            Aver.IsNull( deser.Another4 );
            Aver.IsNull( deser.Another5 );
            Aver.IsNull( deser.Another6 );
            Aver.IsNull( deser.Another7 );
            Aver.IsNull( deser.Another8 );
            Aver.IsNull( deser.Another9 );
            Aver.IsNull( deser.Another10 );

          }
        }



         [Run]
        public void T06()
        {
          using(var ms = new MemoryStream())
          {
            var s = new SlimSerializer(SlimFormat.Instance);

            var root = new ObjectA[3];

            root[0] = null;

            root[1] = new ObjectA();
            root[2] = new ObjectB();

             root[1].AField = 2345;
             root[1].Another1 = new ObjectA{ AField = 27892};
             root[1].Another2 = new ObjectB{ AField = -278, BField = -12, Another1 = root[1]};

             root[2].AField = 2345;
             ((ObjectB)root[2]).BField = 900333;
             root[2].Another1 = new ObjectA{ AField = 8000000};
             root[2].Another2 = new ObjectB{ AField = -278, BField = -1532, Another1 = root[2]};
             root[2].Another9 = root[1];

            s.Serialize(ms, root);

            ms.Position = 0;


            var deser = s.Deserialize(ms) as ObjectA[];

            Aver.IsNotNull( deser );
            Aver.IsTrue( deser.GetType() == typeof(ObjectA[]));
            Aver.AreEqual( 3, deser.Length);
            Aver.IsNull(deser[0]);
            Aver.IsNotNull(deser[1]);
            Aver.IsNotNull(deser[2]);


            Aver.AreEqual(2345,  deser[1].AField );
            Aver.IsNotNull( deser[1].Another1 );
            Aver.IsTrue( deser[1].Another1.GetType() == typeof(ObjectA));
            Aver.AreEqual(27892,  deser[1].Another1.AField );

            Aver.IsNotNull( deser[1].Another2 );
            Aver.AreEqual(-278,  deser[1].Another2.AField );

            Aver.IsTrue( deser[1].Another2.GetType() == typeof(ObjectB));
            Aver.AreEqual(-12,  ((ObjectB)deser[1].Another2).BField );

            Aver.IsNotNull( deser[1].Another2.Another1 );
            Aver.IsTrue( object.ReferenceEquals(deser[1], deser[1].Another2.Another1));
            Aver.IsTrue( deser[1].Another2.GetType() == typeof(ObjectB));

            Aver.IsNull( deser[1].Another3 );
            Aver.IsNull( deser[1].Another4 );
            Aver.IsNull( deser[1].Another5 );
            Aver.IsNull( deser[1].Another6 );
            Aver.IsNull( deser[1].Another7 );
            Aver.IsNull( deser[1].Another8 );
            Aver.IsNull( deser[1].Another9 );
            Aver.IsNull( deser[1].Another10 );





            Aver.AreEqual(2345,  deser[2].AField );
            Aver.AreEqual(900333,  ((ObjectB)deser[2]).BField );
            Aver.IsNotNull( deser[2].Another1 );
            Aver.IsTrue( deser[2].Another1.GetType() == typeof(ObjectA));
            Aver.AreEqual(8000000,  deser[2].Another1.AField );

            Aver.IsNotNull( deser[2].Another2 );
            Aver.AreEqual(-278,  deser[2].Another2.AField );

            Aver.IsTrue( deser[2].Another2.GetType() == typeof(ObjectB));
            Aver.AreEqual(-1532,  ((ObjectB)deser[2].Another2).BField );

            Aver.IsNotNull( deser[2].Another2.Another1 );
            Aver.IsTrue( object.ReferenceEquals(deser[2], deser[2].Another2.Another1));
            Aver.IsTrue( deser[2].Another2.GetType() == typeof(ObjectB));

            Aver.IsNull( deser[2].Another3 );
            Aver.IsNull( deser[2].Another4 );
            Aver.IsNull( deser[2].Another5 );
            Aver.IsNull( deser[2].Another6 );
            Aver.IsNull( deser[2].Another7 );
            Aver.IsNull( deser[2].Another8 );
            Aver.IsNotNull( deser[2].Another9 );
            Aver.IsNull( deser[2].Another10 );

            Aver.IsTrue( object.ReferenceEquals(deser[1], deser[2].Another9) );


          }
        }


    }
}
