/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Scripting;
using Azos.Geometry;

namespace Azos.Tests.Nub.Geometry
{
    [Runnable]
    public class LatLngTests
    {
       [Run]
        public void FromDegreeString_ToString()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");

           Console.WriteLine(cleveland);
           Aver.AreEqual("41°29'13'', -81°38'26''", cleveland.ToString());
        }

        [Run]
        public void FromDecimalString_Distance_CLE_LA()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var losangeles = new LatLng("34.1610243,-117.9465513");

           Console.WriteLine(cleveland);
           Console.WriteLine(losangeles);

           var dist = cleveland.HaversineEarthDistanceKm(losangeles);

           Console.WriteLine(dist);
           Aver.AreEqual(3265, (int)dist);
        }

        [Run]
        public void FromDecimalString_Distance_LA_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var losangeles = new LatLng("34.1610243,-117.9465513");

           Console.WriteLine(cleveland);
           Console.WriteLine(losangeles);

           var dist = losangeles.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Aver.AreEqual(3265, (int)dist);
        }


        [Run]
        public void FromDegreeString_Distance_CLE_LA()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var losangeles = new LatLng("34°9'40'', -117°56'48''");

           Console.WriteLine(cleveland);
           Console.WriteLine(losangeles);

           var dist = cleveland.HaversineEarthDistanceKm(losangeles);

           Console.WriteLine(dist);
           Aver.AreEqual(3265, (int)dist);
        }


        [Run]
        public void FromDecimalString_Distance_CLE_MOSCOW()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var moscow = new LatLng("55.7530361,37.6217305");

           Console.WriteLine(cleveland);
           Console.WriteLine(moscow);

           var dist = cleveland.HaversineEarthDistanceKm(moscow);

           Console.WriteLine(dist);
           Aver.AreEqual(7786, (int)dist);
        }

        [Run]
        public void FromDecimalString_Distance_MOSCOW_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var moscow = new LatLng("55.7530361,37.6217305");

           Console.WriteLine(cleveland);
           Console.WriteLine(moscow);

           var dist = moscow.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Aver.AreEqual(7786, (int)dist);
        }

        [Run]
        public void FromDegreeString_Distance_CLE_MOSCOW()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var moscow = new LatLng("55°45'11'', 37°37'18''");

           Console.WriteLine(cleveland);
           Console.WriteLine(moscow);

           var dist = cleveland.HaversineEarthDistanceKm(moscow);

           Console.WriteLine(dist);
           Aver.AreEqual(7786, (int)dist);
        }



        [Run]
        public void FromDecimalString_Distance_MELBOURNE_CLE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var melbourne = new LatLng("-37.5210205,144.7461265");

           Console.WriteLine(cleveland);
           Console.WriteLine(melbourne);

           var dist = melbourne.HaversineEarthDistanceKm(cleveland);

           Console.WriteLine(dist);
           Aver.AreEqual(16058, (int)dist);
        }

        [Run]
        public void FromDecimalString_Distance_CLE_MELBOURNE()
        {
           var cleveland = new LatLng("41.4868145,-81.6406292");
           var melbourne = new LatLng("-37.5210205,144.7461265");

           Console.WriteLine(cleveland);
           Console.WriteLine(melbourne);

           var dist = cleveland.HaversineEarthDistanceKm(melbourne);

           Console.WriteLine(dist);
           Aver.AreEqual(16058, (int)dist);
        }

        [Run]
        public void FromDegreeString_Distance_CLE_MELBOURNE()
        {
           var cleveland = new LatLng("41°29'13'', -81°38'26''");
           var melbourne = new LatLng("-37°31'16'', 144°44'46''");

           Console.WriteLine(cleveland);
           Console.WriteLine(melbourne);

           var dist = cleveland.HaversineEarthDistanceKm(melbourne);

           Console.WriteLine(dist);
           Aver.AreEqual(16058, (int)dist);
        }
    }
}