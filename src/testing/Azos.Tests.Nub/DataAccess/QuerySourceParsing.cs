/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Data.Access;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class QuerySourceParsing
  {
    [Run]
    public void WithoutPRAGMA_1()
    {
      var src = "abc";

      var qs = new QuerySource("1", src);

      Aver.IsFalse(qs.HasPragma);
      Aver.IsTrue(qs.ReadOnly);
      Aver.AreEqual("abc", qs.OriginalSource);
      Aver.AreEqual("abc", qs.StatementSource);
    }

    [Run]
    public void WithoutPRAGMA_2()
    {
      var src =
@"a123
b
c
d
e
f
g
h
j
k";

      var qs = new QuerySource("1", src);

      Aver.IsFalse(qs.HasPragma);
      Aver.IsTrue(qs.ReadOnly);
      Aver.AreEqual("a123", qs.OriginalSource.ReadLine());
      Aver.AreEqual("a123", qs.StatementSource.ReadLine());
    }

    [Run]
    public void PRAGMA_1_Modifiable()
    {
      var src =
@"#pragma
modify=tbl_patient
key=counter,ssn
ignore=marker
load=counter
@last_name=lname
@first_name=fname
.last_name=This is description of last name
invisible=marker,counter,c_doctor

select
 1 as marker,
 t1.counter,
 t1.ssn,
 t1.lname as last_name,
 t1.fname as first_name,
 t1.c_doctor,
 t2.phone as doctor_phone,
 t2.NPI	as doctor_id
from
 tbl_patient t1
  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
where
 t1.lname like ?LN";

      var qs = new QuerySource("1", src);

      Aver.IsTrue(qs.HasPragma);
      Aver.IsFalse(qs.ReadOnly);
      Aver.AreEqual("tbl_patient", qs.ModifyTarget);
      Aver.AreEqual(true, qs.ColumnDefs["counter"].Key);
      Aver.AreEqual(true, qs.ColumnDefs["ssn"].Key);
      Aver.AreEqual("lname", qs.ColumnDefs["last_name"].BackendName);
      Aver.AreEqual("fname", qs.ColumnDefs["first_name"].BackendName);
      Aver.AreEqual("This is description of last name", qs.ColumnDefs["last_name"].Description);
      Aver.IsTrue(StoreFlag.OnlyLoad == qs.ColumnDefs["counter"].StoreFlag);
      Aver.IsTrue(StoreFlag.None == qs.ColumnDefs["marker"].StoreFlag);

      Aver.IsTrue(StoreFlag.LoadAndStore == qs.ColumnDefs["ssn"].StoreFlag);

      Aver.IsFalse(qs.ColumnDefs["marker"].Visible);
      Aver.IsFalse(qs.ColumnDefs["counter"].Visible);
      Aver.IsFalse(qs.ColumnDefs["c_doctor"].Visible);
      Aver.IsTrue(qs.ColumnDefs["ssn"].Visible);
      Aver.IsTrue(qs.ColumnDefs["last_name"].Visible);

      Aver.AreEqual(
@"select
 1 as marker,
 t1.counter,
 t1.ssn,
 t1.lname as last_name,
 t1.fname as first_name,
 t1.c_doctor,
 t2.phone as doctor_phone,
 t2.NPI	as doctor_id
from
 tbl_patient t1
  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
where
 t1.lname like ?LN
".ToWindowsLines(), qs.StatementSource.ToWindowsLines());
    }

    [Run]
    public void PRAGMA_2_nonModifiable()
    {
      var src =
@"#pragma
key=counter,ssn
ignore=marker
load=counter
@last_name=lname
@first_name=fname
.last_name=This is description of last name

select
 1 as marker,
 t1.counter,
 t1.ssn,
 t1.lname as last_name,
 t1.fname as first_name,
 t1.c_doctor,
 t2.phone as doctor_phone,
 t2.NPI	as doctor_id
from
 tbl_patient t1
  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
where
 t1.lname like ?LN";

      var qs = new QuerySource("1", src);

      Aver.IsTrue(qs.HasPragma);
      Aver.IsTrue(qs.ReadOnly);
      Aver.AreEqual(string.Empty, qs.ModifyTarget);
      Aver.AreEqual(true, qs.ColumnDefs["counter"].Key);
      Aver.AreEqual(true, qs.ColumnDefs["ssn"].Key);
      Aver.AreEqual("lname", qs.ColumnDefs["last_name"].BackendName);
      Aver.AreEqual("fname", qs.ColumnDefs["first_name"].BackendName);
      Aver.AreEqual("This is description of last name", qs.ColumnDefs["last_name"].Description);
      Aver.IsTrue(StoreFlag.OnlyLoad == qs.ColumnDefs["counter"].StoreFlag);
      Aver.IsTrue(StoreFlag.None == qs.ColumnDefs["marker"].StoreFlag);

      Aver.IsTrue(StoreFlag.LoadAndStore == qs.ColumnDefs["ssn"].StoreFlag);

      Aver.AreEqual(
@"select
 1 as marker,
 t1.counter,
 t1.ssn,
 t1.lname as last_name,
 t1.fname as first_name,
 t1.c_doctor,
 t2.phone as doctor_phone,
 t2.NPI	as doctor_id
from
 tbl_patient t1
  left outer join tbl_doctor t2 on t1.c_doctor = t2.counter
where
 t1.lname like ?LN
".ToWindowsLines(), qs.StatementSource.ToWindowsLines());
    }

  }
}
