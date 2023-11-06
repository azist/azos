/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class SchemaSerialization01Tests
  {
    [Schema(Description = "Test document A")]
    [Schema(targetName: "oldShit", Description = "Test document A in the old system")]
    public class DocA : TypedDoc
    {
      [Field(required: true, Description = "Patient first name", MetadataContent = "a=1 b=true")]
      [Field("rabel1", null, MinLength = 5, MetadataContent ="c=-9 b=false")]
      [Field(targetName: "ORACLE", MinLength = 5, MetadataContent = "c=-9 b=false")]
      public string FirstName{ get; set; }


      [Field]
      public List<DocA> Subdocuments{ get; set; }

      [Field]
      public Relative Father { get; set; }

      [Field]
      public Relative Mother { get; set; }
    }

    [Schema(name: "sss")]
    public class Relative : TypedDoc
    {
      [Field(required: true, Description = "First name")]
      public string FirstName { get; set; }

      [Field(required: true, Description = "Lastname")]
      public string LastName { get; set; }

      [Field(Description = "DOB")]
      public DateTime? DOB { get; set; }
    }


    [Run]
    public void Case01()
    {
      var got = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(Schema.GetForTypedDoc<DocA>()), "BarMarLey");
      got.See();
    }

  }
}
