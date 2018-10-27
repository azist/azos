/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Azos.Scripting;

using Azos.Parsing;


namespace Azos.Tests.Unit.Parsing
{
    [Runnable]
    public class NatTxtGen
    {
        [Run("sz=25")]
        [Run("sz=50")]
        [Run("sz=155")]
        public void GenerateUpToSize(int sz)
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate(sz);
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length > 0);
            Aver.IsTrue(txt.Length <= sz);
          }
        }

        [Run]
        public void GenerateDefault()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length > 4);

          }
        }

        [Run]
        public void GenerateRandomSizes()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.Generate(0);
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length > 4);

          }
        }


        [Run("min=6 max=10")]
        [Run("min=8 max=20")]
        public void GenerateWordSizes(int min, int max)
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateWord(min, max);
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= min);
            Aver.IsTrue(txt.Length <= max);
          }
        }

        [Run]
        public void GenerateDefaultWordSizes()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateWord();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 4);
            Aver.IsTrue(txt.Length <= 20);
          }
        }

        [Run]
        public void GenerateLastNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateLastName();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 20);
          }
        }

        [Run]
        public void GenerateFirstNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFirstName();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 20);
          }
        }

        [Run]
        public void GenerateFulltNames_woMiddle()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFullName();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 6);
            Aver.IsTrue(txt.Length <= 40);
          }
        }

         [Run]
        public void GenerateFulltNames_withMiddle()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateFullName(true);
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 6);
            Aver.IsTrue(txt.Length <= 40);
          }
        }

        [Run]
        public void GenerateCityNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateCityName();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 40);
          }
        }

        [Run("cnt=   1000")]
        [Run("cnt=  10000")]
        [Run("cnt= 100000")]
        [Run("cnt=1000000")]
        [Run("cnt=5000000")]
        public void AnalyzeUniqueness(int CNT)
        {
          var fnames = new List<string>();
          var lnames = new List<string>();
          var flnames = new List<string>();
          var cities = new List<string>();

          for (var i=0; i<CNT; i++)
          {
            var fn = NaturalTextGenerator.GenerateFirstName();
            fnames.Add( fn );
            var ln = NaturalTextGenerator.GenerateLastName();
            lnames.Add( ln );
            flnames.Add(fn+ " " + ln);
            cities.Add( NaturalTextGenerator.GenerateCityName() );
          }
          Console.WriteLine("Generated {0:n0} times", CNT);
          Console.WriteLine("----------------------------");
          var dfn = fnames.Distinct().Count();
          var pfn = 100d * (dfn / (double)CNT);
          Console.WriteLine(" First names {0:n0} unique {1:n3}%", dfn, pfn);

          var dln = lnames.Distinct().Count();
          var pln = 100d * (dln / (double)CNT);
          Console.WriteLine(" Last names {0:n0} unique {1:n3}%", dln, pln);

          var dfln = flnames.Distinct().Count();
          var pfln = 100d * (dfln / (double)CNT);
          Console.WriteLine(" First+Last names {0:n0} unique {1:n3}%", dfln, pfln);
          Aver.IsTrue( pfln > 85d);//85% uniqueness

          var dct = cities.Distinct().Count();
          var pct = 100d * (dct / (double)CNT);
          Console.WriteLine(" Cities {0:n0} unique {1:n3}%", dct, pct);
          Console.WriteLine();
        }


        [Run]
        public void GenerateUSCityStateZipNames()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateUSCityStateZip();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 50);
          }
        }

        [Run]
        public void GenerateAddressLines()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateAddressLine();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 100);
          }
        }

        [Run]
        public void GenerateEMails()
        {
          for (var i=0; i<100; i++)
          {
            var txt = NaturalTextGenerator.GenerateEMail();
            Console.WriteLine( txt );
            Aver.IsTrue( txt.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(txt.Length >= 3);
            Aver.IsTrue(txt.Length <= 100);
          }
        }

        [Run]
        public void GenerateFullInfo()
        {
          for (var i=0; i<100; i++)
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
            Console.WriteLine( name );
            Console.WriteLine( addr );
            Console.WriteLine( csz );
            Console.WriteLine("-------------------------------------------" );
            Aver.IsTrue( name.IsNotNullOrWhiteSpace() );
            Aver.IsTrue( addr.IsNotNullOrWhiteSpace() );
            Aver.IsTrue( csz.IsNotNullOrWhiteSpace() );
            Aver.IsTrue(name.Length >= 3);
            Aver.IsTrue(name.Length <= 100);
            Aver.IsTrue(addr.Length >= 3);
            Aver.IsTrue(addr.Length <= 100);
            Aver.IsTrue(csz.Length >= 3);
            Aver.IsTrue(csz.Length <= 100);
          }
        }


        [Run]
        public void PerfFullInfo()
        {
          const int CNT = 1000000;
          var sw = Stopwatch.StartNew();
          for (var i=0; i<CNT; i++)
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
          }

          var elapsed = sw.ElapsedMilliseconds;
          var ops = CNT / (elapsed / 1000d);

          Console.WriteLine("Genereated {0} full infos, in {1:n0} ms at {2:n0} ops/sec", CNT, elapsed, ops);
          Aver.IsTrue( ops > 180000);//180,000 ops/sec
        }


        [Run]
        public void PerfFullInfo_Parallel()
        {
          const int CNT = 3000000;
          var sw = Stopwatch.StartNew();
          System.Threading.Tasks.Parallel.For(0, CNT,
          (i)=>
          {
            var name = NaturalTextGenerator.GenerateFirstName()+" "+NaturalTextGenerator.GenerateLastName();
            var addr = NaturalTextGenerator.GenerateAddressLine();
            var csz = NaturalTextGenerator.GenerateUSCityStateZip();
          });

          var elapsed = sw.ElapsedMilliseconds;
          var ops = CNT / (elapsed / 1000d);

          Aver.IsTrue( ops > 750000);//750,000 ops/sec

          Console.WriteLine("Genereated {0} full infos, in {1:n0} ms at {2:n0} ops/sec", CNT, elapsed, ops);
        }



    }
}


