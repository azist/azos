/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos;
using Azos.Data;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;
using Azos.Text;

namespace WaveTestSite.Controllers
{
  public class LoadTester: Controller
  {
    #region Nested classes

      public enum PatientStatus { Admitted, Discharged};


      public class Patient: TypedDoc
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }

        [Field] public PatientStatus Status { get; set;}

        [Field] public DateTime AdmissionDate { get; set; }
        [Field] public DateTime? DischargeDate { get; set; }

        [Field] public decimal Balance { get; set;}
        [Field] public byte[] BinData{ get; set;}
      }

      public class Doctor: TypedDoc
      {
        [Field] public int ID { get; set; }
        [Field] public string Name { get; set; }
        [Field] public bool Certified { get; set; }
        [Field] public List<Patient> Patients { get; set; }
      }

    #endregion

    [Action]
    public string Echo(string input)
    {
      return "from server, received: "+input;
    }

    [Action]
    public object Doctors(int count = 1, int sleepMs=0, int minPatientCount=0, int maxPatientCount=10)
    {
    //  WorkContext.Response.Buffered = false;

      var result = new List<Doctor>();

      for(var i=0; i<count; i++)
      {
        var doctor = new Doctor
        {
          ID = i,
           Certified = i%2==0,
            Name = "Doctor # "+i, //NaturalTextGenerator.Generate(15),
             Patients = new List<Patient>()
        };

        var pcount = App.Random.NextScaledRandomInteger(minPatientCount, maxPatientCount);
        for (var j=0; j<pcount; j++)
        {
          var patient = new Patient
          {
             ID = i*1000+j,
              AdmissionDate = DateTime.Now,
               Balance = j*123.11m,
                DischargeDate = i%3==0?(DateTime?)null:DateTime.Now,
                  Name = "Patient # "+j,
                   Status = i%7==0?PatientStatus.Admitted:PatientStatus.Discharged,
          };

          patient.BinData = new byte[App.Random.NextScaledRandomInteger(0,7)];
          for(var k=0; k<patient.BinData.Length; k++)
           patient.BinData[k] = (byte)App.Random.NextScaledRandomInteger(0, 255);

          doctor.Patients.Add(patient);

        }

        if (sleepMs>0)//Simulate some kind of DB access
           System.Threading.Thread.Sleep(App.Random.NextScaledRandomInteger(0, sleepMs));

        result.Add(doctor);
      }


      return result;
    }



    //protected override System.Reflection.MethodInfo FindMatchingAction(Azos.Wave.WorkContext work, string action, out object[] args)
    //{
    //  return base.FindMatchingAction(work, action, out args);
    //}

    //multipart (byte array as well)
    //public object RowSet(different data types: decimal, bool, float, double, DateTime, TimeSpan and their Nullable versions)
    //public object RowSet(JSONDataMap row, int a, string b)
    //public object RowSet(int a, string b, JSONDataMap row)
    //public object RowSet(TestRow row, int a, string b)
    //match{is-local=false}
  }
}
