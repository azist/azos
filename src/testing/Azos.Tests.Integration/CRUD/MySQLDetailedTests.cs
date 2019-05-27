/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Access.MySql;
using Azos.Scripting;
using Azos.Apps;

namespace Azos.Tests.Integration.CRUD
{
  [Runnable]
  public class MySQLDetailedTests : IRunnableHook, IRunHook
  {
    private string m_ConnectionString;

    #region IRunnable

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var connectionConf = @"app{ conneсt-string=$(~AZOS_TEST_MYSQL_CONNECT)}".AsLaconicConfig();
      m_ConnectionString = connectionConf.AttrByIndex(0).ValueAsString();
      using(var cnn = new MySqlConnection(m_ConnectionString))
      {
        cnn.Open();
        using(var cmd = cnn.CreateCommand())
        {
          cmd.CommandText =
@"DROP TABLE IF EXISTS `tbl_employee`;
CREATE TABLE `tbl_employee` (
  `GDID` BINARY(12) NOT NULL,
  `NAME` VARCHAR(100) NOT NULL,
  `DOB` DATE NOT NULL,
  `DPT` CHAR(4) NOT NULL,
  `CODE` CHAR(36) NULL DEFAULT NULL,
  `MGR` CHAR(1) NOT NULL DEFAULT 'F',
  `EXP` INT(4) NULL DEFAULT NULL,
  `RATE` DOUBLE NULL DEFAULT NULL,
  `SAL` DECIMAL(12,2) NULL DEFAULT NULL,
  `NOTE` TEXT NULL,
  PRIMARY KEY (`GDID`));";
          cmd.ExecuteNonQuery();
        }
      }
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      return false;
    }

    bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      using(var cnn = new MySqlConnection(m_ConnectionString))
      {
        cnn.Open();
        using(var cmd = cnn.CreateCommand())
        {
          cmd.CommandText = "TRUNCATE TBL_PATIENT; TRUNCATE TBL_DOCTOR; TRUNCATE TBL_TUPLE; TRUNCATE tbl_employee;";
          cmd.ExecuteNonQuery();
        }
      }
      return false;
    }

    bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
    {
      return false;
    }

    #endregion

    [Run]
    public void Delete_TypedRow()
    {
      var row = makePatient();

      using (var ds = makeDataStore())
      {
        ds.Insert(row);

        var qry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Ivanov") };
        var result = ds.LoadEnumerable(qry).ToArray();
        Aver.AreEqual(1, result.Length);

        ds.Delete(result[0]);

        result = ds.LoadEnumerable(qry).ToArray();
        Aver.AreEqual(0, result.Length);
      }
    }

    [Run]
    public void UpdateWithoutFetch()
    {
      var row = makePatient();

      using (var ds = makeDataStore())
      {
        ds.Insert(row);

        // update amount to 110.99M
        var qry = new Query("CRUD.Patient.UpdateAmount")
        {
          new Query.Param("pAmount", 110.99M),
          new Query.Param("pSSN", row.SSN)
        };
        ds.ExecuteWithoutFetch(qry);

        var loadQry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Ivanov") };
        var result = ds.LoadDoc(loadQry);

        Aver.IsTrue(result is Patient);
        Aver.AreEqual(110.99M, result.Amount);
      }
    }

    [Run]
    public void UpsertRow_Insert()
    {
      var row1 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Ivanov",
        DOB = new DateTime(1980, 8, 29),
        SSN = "123456",
        NPI = "5478",
        Amount = 10.23M
      };
      var row2 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Petrovich",
        DOB = new DateTime(1970, 3, 8),
        SSN = "156486",
        NPI = "5278",
        Amount = 10.23M
      };

      using (var ds = makeDataStore())
      {
        ds.Insert(row1);

        var loadQry = new Query<Doctor>("CRUD.Doctor.List") { new Query.Param("pSSN", "%") };
        var result1 = ds.LoadOneRowset(loadQry).ToList();

        Aver.AreEqual(1, result1.Count);

        ds.Upsert(row2);
        var result2 = ds.LoadEnumerable(loadQry).OrderBy(r => r["COUNTER"].AsLong()).ToArray();

        Aver.AreEqual(2, result2.Length);
        Aver.AreObjectsEqual(row1.SSN, result2[0]["SSN"]);
        Aver.AreObjectsEqual(row2.SSN, result2[1]["SSN"]);
      }
    }

    [Run]
    public void UpsertRow_Update()
    {
      var row1 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Ivanov",
        DOB = new DateTime(1980, 8, 29),
        SSN = "123456",
        NPI = "5478",
        Amount = 10.23M
      };
      var row2 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Petrovich",
        DOB = new DateTime(1970, 3, 8),
        SSN = "123456",
        NPI = "5278",
        Amount = 10.23M
      };

      using (var ds = makeDataStore())
      {
        ds.Insert(row1);

        var loadQry = new Query<Doctor>("CRUD.Queries.Doctor.List") { new Query.Param("pSSN", "%") };
        var result = ds.LoadDoc(loadQry);

        Aver.AreEqual(row1.Last_Name, result.Last_Name);

        // updates due to unique index on SSN column
        ds.Upsert(row2);
        result = ds.LoadDoc(loadQry);

        Aver.AreEqual(row2.Last_Name, result.Last_Name);
      }
    }

    [Run]
    public void ManyRowsInsertAndRead()
    {
      using (var ds = makeDataStore())
      {
        var patients = new List<Patient>();
        for(var i = 0; i < 10000; i++)
        {
          var row = makePatient("Ivanov" + i);
          patients.Add(row);
          ds.Insert(row);
        }

        var qry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Ivanov%") };
        var result = ds.LoadEnumerable(qry).OrderBy(p => p.COUNTER);
        Aver.IsTrue(patients.Select(p => p.Last_Name).SequenceEqual(result.Select(r => r.Last_Name)));
      }
    }

    [Run]
    [Aver.Throws(ExceptionType = typeof(MySqlDataAccessException), Message = "Duplicate entry '123456'")]
    public void IndexViolation ()
    {
      var row1 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Ivanov",
        DOB = new DateTime(1980, 8, 29),
        SSN = "123456",
        NPI = "5478",
        Amount = 10.23M
      };
      var row2 = new Doctor
      {
        First_Name = "Oleg",
        Last_Name = "Petrovich",
        DOB = new DateTime(1970, 3, 8),
        SSN = "123456",
        NPI = "5278",
        Amount = 10.23M
      };

      using (var ds = makeDataStore())
      {
        ds.Insert(row1);
        ds.Insert(row2);
      }
    }

    [Run]
    public void JoinedRows()
    {
      var doctor = new Doctor
      {
        First_Name = "Nikolai",
        Last_Name = "Petrovich",
        DOB = new DateTime(1970, 3, 8),
        Phone = "555666777",
        SSN = "293488",
        NPI = "5278",
        Amount = 10.23M
      };
      var patient1 =  makePatient("Esenin");
      var patient2 = makePatient("Gogol");

      using (var ds = makeDataStore())
      {
        ds.Insert(doctor);

        var docQry = new Query<Doctor>("CRUD.Queries.Doctor.List") { new Query.Param("pSSN", "%") };
        var doc = ds.LoadDoc(docQry);
        patient1.C_DOCTOR = doc.COUNTER;

        ds.Insert(patient1);
        ds.Insert(patient2);

        var qry = new Query<Patient>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%") };
        var result = ds.LoadEnumerable(qry).OrderBy(p => p.COUNTER).ToArray();

        Aver.AreEqual(2, result.Length);
        Aver.AreEqual(doc.COUNTER, result[0].C_DOCTOR);
        Aver.AreEqual(doctor.NPI, result[0].Doctor_ID);
        Aver.AreEqual(doctor.Phone, result[0].Doctor_Phone);
        Aver.IsNull(result[0].Marker);
        Aver.AreEqual(0, result[1].C_DOCTOR);
        Aver.IsNull(result[1].Doctor_ID);
        Aver.IsNull(result[1].Doctor_Phone);
      }
    }

    [Run]
    public void PartialUpdate()
    {
      var patient = makePatient("Petrov");
      patient.City = "Stambul";
      patient.COUNTER = 1;

      using (var ds = makeDataStore())
      {
        ds.Insert(patient);

        patient.Last_Name = "Ivanov";
        patient.SSN = "345678";
        patient.City = "Salem";
        patient.Address1 = "22, Lenin str";

        ds.Update(patient, filter: "SSN,Address1".OnlyTheseFields());
        var qry = new Query<Patient>("CRUD.Queries.Patient.List") { new Query.Param("LN", "%") };
        var result = ds.LoadDoc(qry);

        Aver.AreEqual("Stambul", result.City);
        Aver.AreEqual("Petrov", result.Last_Name);
        Aver.AreEqual("345678", result.SSN);
        Aver.AreEqual("22, Lenin str", result.Address1);
      }
    }

    [Run]
    public void Cursor_RequestWithCondition()
    {
      var patient1 = makePatient("Zorkin");
      var patient2 = makePatient("Tokugava");

      using (var ds = makeDataStore())
      {
        ds.Insert(patient1);
        ds.Insert(patient2);

        var qry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Tokugava") };

        using (var cursor = ds.OpenCursor(qry))
        {
          Aver.IsFalse(cursor.Disposed);
          var cnt = 0;
          foreach(var patient in cursor.AsEnumerableOf<Patient>())
          {
            Aver.AreEqual(patient2.Last_Name, patient.Last_Name);
            Aver.AreEqual(patient2.SSN, patient.SSN);
            cnt++;
          }
          Aver.AreEqual(1, cnt);
        }
      }
    }

    [Run]
    public void ParallelDatastores1()
    {
      Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism=36}, (i) =>
      {
        var row = makePatient("Ivanov" + i.ToString());
        row.SSN = (10000 + i).ToString();
        using (var ds = makeDataStore())
        {
          ds.Insert(row);
          Thread.Sleep(Ambient.Random.NextScaledRandomInteger(1000, 3000));

          var qry = new Query("CRUD.Patient.UpdateAmount")
          {
            new Query.Param("pAmount", 100M + i),
            new Query.Param("pSSN", row.SSN)
          };
          ds.ExecuteWithoutFetch(qry);
          Thread.Sleep(Ambient.Random.NextScaledRandomInteger(1000, 3000));

          var listQry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Ivanov" + i.ToString())};
          var result = ds.LoadDoc(listQry);

          Aver.AreEqual(row.Last_Name, result.Last_Name);
          Aver.AreEqual(row.SSN, result.SSN);
          Aver.AreEqual(100M + i, result.Amount);
        }
      });
    }

    [Run]
    public void ParallelDatastores2()
    {
      Parallel.For(0, 100, new ParallelOptions { MaxDegreeOfParallelism=36}, (i) =>
      {
        var row = makePatient("Ivanov" + i.ToString());
        row.SSN = (10000 + i).ToString();
        using (var ds = makeDataStore())
        {
          ds.Insert(row);

          var qry = new Query("CRUD.Patient.UpdateAmount")
          {
            new Query.Param("pAmount", 100M + i),
            new Query.Param("pSSN", row.SSN)
          };
          ds.ExecuteWithoutFetch(qry);

          var listQry = new Query<Patient>("CRUD.Patient.List") { new Query.Param("LN", "Ivanov" + i.ToString())};
          var result = ds.LoadDoc(listQry);

          Aver.AreEqual(row.Last_Name, result.Last_Name);
          Aver.AreEqual(row.SSN, result.SSN);
          Aver.AreEqual(100M + i, result.Amount);
        }
      });
    }

    [Run]
    public void Employees1()
    {
      var cnt = 100;
      var list = new List<Employee>();
      for(var i = 0; i < cnt; i++)
        list.Add(makeEmployee(new GDID(0, 1, (ulong)i)));

      using(var ds = makeDataStore())
      {
        list.ForEach(e => ds.Insert(e));

        var fetchQry = new Query<Employee>("CRUD.Queries.Employee.FetchAll");
        var employees = ds.LoadEnumerable(fetchQry);

        Aver.AreEqual(list.Count(), employees.Count());
        Employee row;
        foreach(var emp in list)
        {
          row = employees.FirstOrDefault(e => e.GDID == emp.GDID);
          Aver.IsNotNull(row);
          Aver.IsTrue(emp.Equals(row));
        }

        var deleteQry = new Query<Employee>("CRUD.Queries.Employee.DeleteBySalaryMax")
                        { new Query.Param("pSalary", 200.50M) };
        ds.ExecuteWithoutFetch(deleteQry);

        var rest = list.Where(e => e.Salary < 200.50M);
        employees = ds.LoadEnumerable(fetchQry);
        Aver.AreEqual(rest.Count(), employees.Count());
        foreach(var emp in rest)
        {
          row = employees.FirstOrDefault(e => e.GDID == emp.GDID);
          Aver.IsNotNull(row);
          Aver.IsTrue(emp.Equals(row));
        }

        // try load deleted
        var fetchByGDIDQry = new Query<Employee>("CRUD.Queries.Employee.FetchByGDID")
                             { new Query.Param("pGDID", list.First(e=>e.Salary > 210).GDID) };
        row = ds.LoadDoc(fetchByGDIDQry);
        Aver.IsNull(row);

        fetchByGDIDQry = new Query<Employee>("CRUD.Queries.Employee.FetchByGDID")
                         { new Query.Param("pGDID", rest.First().GDID) };

        row = ds.LoadDoc(fetchByGDIDQry);
        Aver.IsNotNull(row);
        Aver.IsTrue(rest.First().Equals(row));
        var code = Guid.NewGuid();
        row.Code = code;
        ds.Update(row);

        row = ds.LoadDoc(fetchByGDIDQry);
        Aver.IsNotNull(row);
        Aver.AreObjectsEqual(code, row.Code);
      }
    }

    [Run]
    public void Employees2()
    {
      var threadCnt = 20;
      var iterations = 100;
      var cnt = 100;

      var data = new BlockingCollection<Employee>();
      var deleted = new BlockingCollection<Employee>();
      for(var i = 0; i < threadCnt; i++)
      {
        for(var j = 0; j < cnt; j++)
        {
          var emp = makeEmployee(new GDID((uint)(i + 1), 0, (ulong)j));
          emp.Department = "DPT" + (i % 2).ToString();
          data.Add(emp);
        }
      }

      // initial population
      using (var ds = makeDataStore())
        data.ForEach(e => ds.Insert(e));

      using (var ds = makeDataStore())
      {
        Parallel.For(0, threadCnt, (i) =>
        {
          var department = "DPT" + (i % 2).ToString();
          for (var ii = 0; ii < iterations; ii++)
          {
            var fetchQry = new Query<Employee>("CRUD.Queries.Employee.FetchByDepartment")
                           { new Query.Param("pDepartment", department) };
            var stored = ds.LoadEnumerable(fetchQry);

            if (stored.Any())
            {
              var idx = Ambient.Random.NextScaledRandomInteger(0, stored.Count());
              var toLoad = stored.ElementAt(idx);
              var emp = data.First(e => e.GDID == toLoad.GDID);

              var fetchOne = new Query<Employee>("CRUD.Queries.Employee.FetchByGDID")
                             { new Query.Param("pGDID", toLoad.GDID) };
              var row = ds.LoadDoc(fetchOne);
              if (row == null) continue; // element was deleted from database by another thread
              Aver.IsTrue(emp.Equals(row));

              // random deletion
              if (Ambient.Random.NextScaledRandomInteger(0, 2) == 1)
              {
                ds.Delete(row);
                deleted.Add(row);
              }
            }

            // random insertion
            if (Ambient.Random.NextScaledRandomInteger(0, 2) == 1)
            {
              var newRow = makeEmployee(new GDID((uint)(i + 1), 0, (ulong)(cnt + ii + 1)));
              newRow.Department = department;
              data.Add(newRow);
              ds.Insert(newRow);
            }
          }
        });
      }

      var resultData = new List<Employee>();
      foreach(var emp in data)
      {
        if (!deleted.Any(e => e.GDID == emp.GDID))
        {
          resultData.Add(emp);
        }
      }

      using (var ds = makeDataStore())
      {
        var fetchQry = new Query<Employee>("CRUD.Queries.Employee.FetchAll");
        var employees = ds.LoadEnumerable(fetchQry);
        Aver.AreEqual(resultData.Count(), employees.Count());

        foreach(var emp in resultData)
        {
          var row = employees.FirstOrDefault(e => e.GDID == emp.GDID);
          Aver.IsNotNull(row);
          Aver.IsTrue(emp.Equals(row));
        }
      }
    }

    [Run]
    public void Employees3()
    {
      var threadCnt = 20;
      var iterations = 100;
      var cnt = 100;

      var data = new BlockingCollection<Employee>();
      var deleted = new BlockingCollection<Employee>();
      for(var i = 0; i < threadCnt; i++)
      {
        for(var j = 0; j < cnt; j++)
        {
          var emp = makeEmployee(new GDID((uint)(i + 1), 0, (ulong)j));
          emp.Department = "DPT" + (i % 2).ToString();
          data.Add(emp);
        }
      }

      // initial population
      using (var ds = makeDataStore())
      {
        data.ForEach(e => ds.Insert(e));
      }

      Parallel.For(0, threadCnt, (i) =>
      {
        using (var ds = makeDataStore())
        {
          var department = "DPT" + (i % 2).ToString();
          for (var ii = 0; ii < iterations; ii++)
          {
            var fetchQry = new Query<Employee>("CRUD.Queries.Employee.FetchByDepartment")
                            { new Query.Param("pDepartment", department) };
            var stored = ds.LoadEnumerable(fetchQry);

            if (stored.Any())
            {
              var idx = Ambient.Random.NextScaledRandomInteger(0, stored.Count());
              var toLoad = stored.ElementAt(idx);
              var emp = data.First(e => e.GDID == toLoad.GDID);

              var fetchOne = new Query<Employee>("CRUD.Queries.Employee.FetchByGDID")
                              { new Query.Param("pGDID", toLoad.GDID) };
              var row = ds.LoadDoc(fetchOne);
              if (row == null) continue; // element was deleted from database by another thread
              Aver.IsTrue(emp.Equals(row));

              // random deletion
              if (Ambient.Random.NextScaledRandomInteger(0, 2) == 1)
              {
                ds.Delete(row);
                deleted.Add(row);
              }
            }

            // random insertion
            if (Ambient.Random.NextScaledRandomInteger(0, 2) == 1)
            {
              var newRow = makeEmployee(new GDID((uint)(i + 1), 0, (ulong)(cnt + ii + 1)));
              newRow.Department = department;
              data.Add(newRow);
              ds.Insert(newRow);
            }
          }
        }
      });

      var resultData = new List<Employee>();
      foreach(var emp in data)
      {
        if (!deleted.Any(e => e.GDID == emp.GDID))
        {
          resultData.Add(emp);
        }
      }

      using (var ds = makeDataStore())
      {
        var fetchQry = new Query<Employee>("CRUD.Queries.Employee.FetchAll");
        var employees = ds.LoadEnumerable(fetchQry);
        Aver.AreEqual(resultData.Count(), employees.Count());

        foreach(var emp in resultData)
        {
          var row = employees.FirstOrDefault(e => e.GDID == emp.GDID);
          Aver.IsNotNull(row);
          Aver.IsTrue(emp.Equals(row));
        }
      }
    }

    [Run]
    public void UpdateEmployeeRowsAffected()
    {
      var row = makeEmployee(new GDID(0, 0, 1));
      using(var ds = makeDataStore())
      {
        var affected = ds.Insert(row);
        Aver.AreEqual(1, affected, "Insert"); // notice: insert new row - affected=1

        affected = ds.Update(row);
        Aver.AreEqual(0, affected, "UpdateSame"); // updating that has not changed - affected=0

        row.Name = "Pupkin";
        affected = ds.Update(row);
        Aver.AreEqual(1, affected, "UpdateDifferent"); // affected=1 because we changed Name and updated
      }
    }

    [Run]
    public void UpsertEmployeeRowsAffected()
    {
      var row = makeEmployee(new GDID(0, 0, 2));
      using(var ds = makeDataStore())
      {
        var affected = ds.Upsert(row);
        Aver.AreEqual(1, affected, "InitUpsert"); // note: upserted noneexistent row - 1 inserted

        affected = ds.Upsert(row);
        Aver.AreEqual(0, affected, "UpsertSame"); // nothing affected as row has not changed

        row.Name = "Pupkin";
        affected = ds.Upsert(row);
        Aver.AreEqual(2, affected, "UpsertDifferent");  // checked existing and updated MySQL returns 2 on row merge
      }
    }

    [Run]
    public void UpdateUnknownRowsAffected()
    {
      var row = makeEmployee(new GDID(0, 0, 1));
      using(var ds = makeDataStore())
      {
        var affected = ds.Insert(row);
        Aver.AreEqual(1, affected, "Insert");

        row.GDID = new GDID(0, 0, 2);
        affected = ds.Update(row);
        Aver.AreEqual(0, affected, "UpdateUnknown");

        affected = ds.Upsert(row);
        Aver.AreEqual(1, affected, "Upsert");

        affected = ds.Delete(row);
        Aver.AreEqual(1, affected, "Delete");

        affected = ds.Update(row);
        Aver.AreEqual(0, affected, "UpdateDeleted");

        affected = ds.Delete(row);
        Aver.AreEqual(0, affected, "DeleteDeleted");
      }
    }

    [Run]
    public void UpdateUnknownWithoutFetchRowsAffected()
    {
      var row = makeEmployee(new GDID(0, 0, 1));
      using(var ds = makeDataStore())
      {
        var affected = ds.Insert(row);
        Aver.AreEqual(1, affected, "Insert");

        var qry = new Query("CRUD.Queries.Employee.UpdateByGDID")
        {
          new Query.Param("pGDID", new GDID(0, 0, 2))
        };
        affected = ds.ExecuteWithoutFetch(qry);
        Aver.AreEqual(0, affected, "UpdateUnknown");

        qry = new Query("CRUD.Queries.Employee.UpdateByGDID")
        {
          new Query.Param("pGDID", new GDID(0, 0, 1))
        };
        affected = ds.ExecuteWithoutFetch(qry);
        Aver.AreEqual(1, affected, "UpdateExisting");
      }
    }

    #region .pvt

    private MySqlDataStore makeDataStore()
    {
      var datastore = new MySqlDataStore(NOPApplication.Instance, m_ConnectionString);
      datastore.QueryResolver.ScriptAssembly = "Azos.Tests.Integration";
      return datastore;
    }

    private Employee makeEmployee(GDID gdid)
    {
      var counter = gdid.Counter;
      var dob = new DateTime(Ambient.Random.NextScaledRandomInteger(1970, 1990),
                             Ambient.Random.NextScaledRandomInteger(1, 12),
                             Ambient.Random.NextScaledRandomInteger(1, 28));
      var salary = Ambient.Random.NextScaledRandomDouble(100, 300).AsDecimal();
      return new Employee
        {
          GDID = gdid,
          Name = Text.NaturalTextGenerator.GenerateFullName(),
          Code = Guid.NewGuid(),
          DOB = dob,
          Department = "DPT" + (counter % 3).ToString(),
          IsManager = (counter % 10) == 0,
          Experience = Ambient.Random.NextScaledRandomInteger(10, 20),
          Rate = Ambient.Random.NextScaledRandomDouble(1.0, 2.0),
          Salary = Math.Round(salary, 2),
          Note = "This is employee #" + gdid.ToString()
        };
    }

    private Patient makePatient(string ln = "Ivanov")
    {
      return new Patient
        {
          First_Name = "Oleg",
          Last_Name = ln,
          DOB = new DateTime(1980, 8, 29),
          SSN = Ambient.Random.NextScaledRandomInteger(100000, 500000).ToString(),
          Amount = 10.23M
        };
    }

    #endregion


    #region Inner

    [Serializable]
    [Table(name: "tbl_employee")]
    public class Employee : TypedDoc
    {
      [Field(required: true, key: true)]
      public GDID GDID { get; set; }

      [Field(required: true)]
      public string Name { get; set; }

      [Field(required: true)]
      public DateTime DOB { get; set; }

      [Field(required: true, backendName: "DPT")]
      public string Department { get; set; }

      [Field]
      public Guid Code { get; set; }

      [Field(backendName: "MGR")]
      public bool IsManager { get; set; }

      [Field(backendName: "EXP")]
      public int Experience { get; set; }

      [Field]
      public double Rate { get; set; }

      [Field(backendName: "SAL")]
      public decimal Salary { get; set; }

      [Field]
      public string Note { get; set; }

      public override bool Equals(Doc other)
      {
        var o = other as Employee;
        if (o == null) return false;

        Rate = Math.Round(Rate, 6);
        o.Rate = Math.Round(o.Rate, 6);
        return this.SequenceEqual(o);
      }
    }

    #endregion
  }
}
