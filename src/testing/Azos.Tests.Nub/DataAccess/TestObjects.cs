/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;

namespace Azos.Tests.Nub.DataAccess
{
  [Serializable]
  [Schema(targetName: "SPARTA_SYSTEM", name: "dimperson")]
  public class Person : TypedDoc
  {
    public Person() { }

    [Field(required: true, key: true)]
    public string ID { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field(required: true)]
    public string LastName { get; set; }

    [Field(valueList: "GOOD,BAD,UGLY")]
    public string Classification { get; set; }

    [Field(required: true, min: "01/01/1900")]
    [Field(targetName: "SPARTA_SYSTEM", required: true, backendName: "brthdt", min: "01/01/1800")]
    public DateTime DOB { get; set; }

    [Field]
    [Field(targetName: "ORACLE", backendName: "empl_yrs")]
    [Field(targetName: "SPARTA_SYSTEM", backendName: "tenure")]
    public int? YearsWithCompany { get; set; }

    [Field(required: true)]
    public int? YearsInSpace { get; set; }

    [Field(min: 0d, max: "1000000")]
    public decimal Amount { get; set; }

    [Field(maxLength: 25)]
    public string Description { get; set; }

    [Field]
    public bool GoodPerson { get; set; }

    [Field]
    public double LuckRatio { get; set; }
  }


  [Serializable]
  public class WithCompositeKey : TypedDoc
  {
    public WithCompositeKey() { }

    [Field(required: true, key: true)]
    public string ID { get; set; }

    [Field(required: true, key: true)]
    public DateTime StartDate { get; set; }

    [Field(required: true)]
    public string Description { get; set; }
  }


  [Serializable]
  public class HistoryItem : TypedDoc
  {
    public HistoryItem() { }

    [Field(required: true, key: true)]
    public string ID { get; set; }

    [Field(required: true, key: true)]
    public DateTime StartDate { get; set; }

    [Field(required: true)]
    public string Description { get; set; }

    public override ValidState Validate(ValidState state, string scope = null)
    {
      state = base.Validate(state, scope);
      if (state.ShouldStop) return state;

      if (!Description.Contains("Chaplin"))
        state = new ValidState(state, new FieldValidationException("Chaplin is required in description", "Description"));

      return state;
    }
  }


  [Serializable]
  public class PersonWithNesting : Person
  {
    public PersonWithNesting() { }

    [Field(required: true)]
    public List<HistoryItem> History1 { get; set; }

    [Field(required: true)]
    public HistoryItem[] History2 { get; set; }

    [Field]
    public HistoryItem LatestHistory { get; set; }
  }


  [Serializable]
  public class ExtendedPerson : Person
  {
    [Field]
    public string Info { get; set; }

    [Field]
    public long Count { get; set; }

    [Field]
    public Person Parent { get; set; }

    [Field]
    public List<Person> Children { get; set; }
  }


  [Serializable]
  public class Empty : TypedDoc
  {
    public Empty() { }
  }

}
