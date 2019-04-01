/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;


namespace Azos.Tests.Integration.CRUD
{
    public abstract class Perzon : TypedDoc
    {
        [Field(key: true)]
        public long? COUNTER {get; set;}

        [Field(required: true)]
        public string SSN {get; set;}

        [Field(required: true)]
        [Field("ORACLE", required: true, backendType: "date")]
        public DateTime DOB {get; set;}

        [Field]
        public string Address1 {get; set;}

        [Field]
        public string Address2 {get; set;}

        [Field]
        public string City {get; set;}

        [Field]
        public string State {get; set;}

        [Field]
        public string Zip {get; set;}

        [Field]
        public string Phone {get; set;}

        [Field]
        public string Years_In_Service {get; set;}

        [Field]
        public decimal Amount {get; set;}

        [Field]
        public string Note {get; set;}
    }

    [Serializable]
    public class Patient : Perzon
    {
        public Patient() {}

        [Field(required: true, backendName: "fname")]
        public string First_Name {get; set;}

        [Field(required: true, backendName: "lname")]
        public string Last_Name {get; set;}

        [Field(storeFlag: StoreFlag.None)]
        public string Marker {get; set;}

        [Field]
        public long? C_DOCTOR {get; set;}

        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public string Doctor_Phone {get; set;}

        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public string Doctor_ID {get; set;}
    }


    [Table(name: "tbl_Patient")] //different class write to the same table as Patient above
    [Serializable]
    public class SuperPatient : Patient
    {
        public SuperPatient() {}

        [Field(storeFlag: StoreFlag.OnlyLoad)]
        public bool Superman {get; set;}

    }


    [Serializable]
    [Table(name: "tbl_doctor")]
    public class Doctor : Perzon
    {
        public Doctor() {}

        [Field(required: true)]
        public string First_Name {get; set;}

        [Field(required: true)]
        public string Last_Name {get; set;}

        [Field(required: true)]
        public string NPI {get; set;}

        [Field]
        public bool Is_Certified {get; set;}
    }


    public class Types : TypedDoc
    {
      [Field(key: true)]
      public GDID GDID{ get; set;}

      [Field]
      public string Screen_Name{ get; set;}

      [Field]
      public string String_Name{ get; set;}

      [Field]
      public string Char_Name{ get; set;}

      [Field]
      public bool? Bool_Char{ get; set;}

      [Field]
      public bool? Bool_Bool{ get; set;}

      [Field]
      public decimal? Amount{ get; set;}


      [Field]
      public DateTime? DOB{ get; set;}

      [Field]
      public int? Age{ get; set;}
    }


    public class FullGDID : TypedDoc
    {
      [Field]
      public GDID GDID{ get; set;}

      [Field]
      public GDID VARGDID{ get; set;}


      [Field]
      public string String_Name{ get; set;}
    }


    [Serializable]
    [Table(name: "tbl_tuple")]
    public class TupleData : TypedDoc
    {
        public TupleData() {}

        [Field(required: true, key: true)]
        public long COUNTER {get; set;}

        [Field(required: true)]
        public string DATA {get; set;}
    }
}