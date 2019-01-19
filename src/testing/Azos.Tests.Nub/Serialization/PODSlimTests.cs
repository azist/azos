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

using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
    //public class Kozel
    //{
    //  public int Age;
    //  public string Name;
    //  public DateTime DOB;
    //}


    [Runnable]
    public class PODSlimTests
    {
      //[Run]
      //public void TempTest()  //DKh: dont refactor - this is very temporary test for me locally
      //{
      //  using (var ms = new FileStream(@"c:\nfx\TEMP_1.POD", FileMode.Create))
      //  {
      //    var s = new SlimSerializer();


      //    var data = new Kozel() { Age=15, Name="Egor", DOB = DateTime.Now};
      //    s.Serialize(ms, data);
      //  }
      //}



        [Run]
        public void ComplexObjectWithArrays_1()
        {
          using(var ms = new MemoryStream())
          {
            var s = new PODSlimSerializer();

            var data = new DataObject();
            data.Populate();
            s.Serialize(ms, data);

            ms.Seek(0, SeekOrigin.Begin);

            var result = s.Deserialize(ms);

            Aver.IsTrue( data.Equals( result));

          }
        }

        [Run]
        public void RootCompositeWriteRead_BusinessFamily()
        {
            using(var ms = new MemoryStream())//new FileStream(@"c:\nfx\TEMP.POD", FileMode.Create))// new MemoryStream())
            {
                var s = new PODSlimSerializer();

                var originalData =
                            new TestBusinessFamily{
                                Husband = new TestPerson{ Name = "Kolyan Zver'", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.5489},
                                Wife = new TestPerson{ Name = "Feiga Pozman", DOB = DateTime.Now, Assets=578, IsRegistered=false, Luck=250.489},
                                Kid = new TestPerson{ Name = "Mykola Zver'", DOB = DateTime.Now, Assets=12, IsRegistered=true, Luck=350.189},
                                Assets = 9000000000,
                                IsCertified = true
                            };

                    s.Serialize(ms, originalData);

                    ms.Seek(0, SeekOrigin.Begin);

                    var convertedData = s.Deserialize(ms);

                Aver.IsFalse( object.ReferenceEquals(originalData, convertedData) );

                Aver.IsTrue (originalData.Equals( convertedData ) );
            }
        }


        [Run]
        public void RootCompositeWriteRead_PersonList()
        {
            using(var ms = new MemoryStream())
            {
                var s = new PODSlimSerializer();

                var originalData = new List<TestPerson>
                                    {
                                        new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.89},
                                        new TestPerson{ Name = "Zmeyan", DOB = DateTime.Now.AddYears(-25), Assets=50000000, IsRegistered=false, Luck=283.4},

                                    };
                s.Serialize(ms, originalData);

                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = (List<TestPerson>)s.Deserialize(ms);

                Aver.IsFalse( object.ReferenceEquals(originalData, convertedData) );

                Aver.AreEqual(originalData.Count, convertedData.Count);

                Aver.IsTrue (originalData[0].Equals( convertedData[0] ) );

                Aver.IsTrue (originalData[1].Equals( convertedData[1] ) );
            }
        }


        [Run]
        public void RootCompositeWriteRead_tuple()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                    var originalData = new Tuple<int, string>(5,"yes");

                    s.Serialize(ms, originalData);

                    ms.Seek(0, SeekOrigin.Begin);

                    var convertedData = (Tuple<int, string>)s.Deserialize(ms);

                    Aver.IsFalse( object.ReferenceEquals(originalData, convertedData) );

                    Aver.AreEqual(5, convertedData.Item1);
                    Aver.AreEqual("yes", convertedData.Item2);
           }
        }


        [Run]
        public void RootSimpleWriteRead_string()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = "hello Dolly!";

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }


        [Run]
        public void RootSimpleWriteRead_bool()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = true;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_decimal()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = 125000m;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_nullabledecimal_1()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                decimal? originalData = 125000m;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_nullabledecimal_2()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                decimal? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_nullabledatetime_1()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                DateTime? originalData = DateTime.Now;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_nullabledatetime_2()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                DateTime? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }


        [Run]
        public void RootSimpleWriteRead_nullabletimespan_1()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                TimeSpan? originalData = TimeSpan.FromHours(12.57);

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }

        [Run]
        public void RootSimpleWriteRead_nullabletimespan_2()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                TimeSpan? originalData = null;

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms);
                Aver.AreObjectsEqual(originalData, convertedData);
           }
        }



        [Run]
        public void RootArrayWriteRead_1D()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new int[100];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as int[];
                Aver.IsFalse(object.ReferenceEquals(originalData, convertedData) );
                Aver.IsTrue(originalData.SequenceEqual(convertedData));
           }
        }

        [Run]
        public void RootArrayWriteRead_2D()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new int[100, 15];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as int[,];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }

        [Run]
        public void RootArrayWriteRead_3D()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new int[22, 15, 4];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as int[,,];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }

        [Run]
        public void RootArrayWriteRead_4D()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new int[3, 5, 2, 8];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as int[,,,];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }

        [Run]
        public void RootArrayWriteRead_5D()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new int[10, 4, 6, 2, 2];

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as int[,,,,];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }


        [Run]
        public void RootArrayWriteRead_1D_datetime()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new DateTime[100];
                var sd = DateTime.UtcNow;
                for(var i=0; i<originalData.Length; i++)
                {
                  originalData[i] = sd;
                  sd = sd.AddHours(i+(i*0.01));
                }

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as DateTime[];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }

        [Run]
        public void RootArrayWriteRead_1D_nullabledatetime()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new DateTime?[100];
                var sd = DateTime.UtcNow;
                for(var i=0; i<originalData.Length; i++)
                    if ((i%2)==0)
                    {
                        originalData[i] = sd;
                        sd = sd.AddHours(i+(i*0.01));
                    }

                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms) as DateTime?[];
                Aver.AreNotSameRef( originalData, convertedData );
                Aver.AreArraysEquivalent(originalData, convertedData);
           }
        }


        [Run]
        public void DeserializationTransform1()
        {
           using(var ms = new MemoryStream())
           {
                var s = new PODSlimSerializer();

                var originalData = new PODTest_Ver1
                {
                    Name = "Xerson Person",
                    Description = "Some description",
                    Age = 25
                };


                s.Serialize(ms, originalData);
                ms.Seek(0, SeekOrigin.Begin);

                var convertedData = s.Deserialize(ms, new PODTestVersionUpgradeStrategy());

                Aver.IsTrue( convertedData is PODTest_Ver2);

                var ver2 = convertedData as PODTest_Ver2;

                Aver.AreEqual( originalData.Name, ver2.Name);
                Aver.AreEqual( originalData.Description, ver2.Description);
                Aver.AreEqual( originalData.Age, ver2.AgeAsOfToday);
                Aver.AreEqual( DateTime.Now.AddYears(-originalData.Age).Year, ver2.DOB.Year);
           }
        }


    }
}
