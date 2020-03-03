using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocValidationLengthTests
  {
    [Run]
    public void Any()
    {
      var sut= new LengthDoc{ };
      Aver.IsNull(sut.Validate());
      sut.AnyString =" joijwoijedojiweoijdiwejlfgjl;djgl;kjdlk;gjldkjflgkjldkjfglkdfjlgjklfd ";
      Aver.IsNull(sut.Validate());

      sut.AnyStringList = new List<string>{"a","b","c","d","e", "a", "b", "c", "d", "e"};
      Aver.IsNull(sut.Validate());

      sut.AnyStringArray = new [] { "a", "b", "c", "d", "e", "a", "b", "c", "d", "e" };
      Aver.IsNull(sut.Validate());
    }


    [Run]
    public void Min_01()
    {
      var sut = new LengthDoc { MinString = "a234" };
      Aver.IsNull(sut.Validate());
      sut.MinString = "1";
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinString), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 3"));
    }

    [Run]
    public void Min_02()
    {
      var sut = new LengthDoc { MinStringList = new List<string>{"a", "2","3","4","5","6"} };
      Aver.IsNull(sut.Validate());

      sut.MinStringList = new List<string> { "a" };
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinStringList), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 5"));
    }

    [Run]
    public void Min_03()
    {
      var sut = new LengthDoc { MinStringArray = new [] { "a" , "2", "3", "4", "5", "6","7","8" } };
      Aver.IsNull(sut.Validate());

      sut.MinStringArray = new [] { "a" };
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinStringArray), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 7"));
    }


    [Run]
    public void Max_01()
    {
      var sut = new LengthDoc { MaxString = "are" };
      Aver.IsNull(sut.Validate());

      sut.MaxString = "areitpie[it[perip";
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MaxString), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 3"));
    }

    [Run]
    public void Max_02()
    {
      var sut = new LengthDoc { MaxStringList = new List<string> { "a", "d", "dferfr" } };
      Aver.IsNull(sut.Validate());

      sut.MaxStringList = new List<string> { "a", "d", "dferfr", "aa" };
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MaxStringList), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 3"));
    }

    [Run]
    public void Max_03()
    {
      var sut = new LengthDoc { MaxStringArray = new[] { "a", "d", "dferfr" } };
      Aver.IsNull(sut.Validate());

      sut.MaxStringArray = new [] { "a", "d", "dferfr", "aa" };
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MaxStringArray), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 3"));
    }



    [Run]
    public void MinMax_01()
    {
      var sut = new LengthDoc { MinMaxString = "are" };
      Aver.IsNull(sut.Validate());
      sut.MinMaxString = "123456789";
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxString), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 7"));

      sut.MinMaxString = "1";
      ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxString), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 3"));
    }

    [Run]
    public void MinMax_02()
    {
      var sut = new LengthDoc { MinMaxStringList = new List<string>{"are", "aaa", "ddddd"} };
      Aver.IsNull(sut.Validate());
      sut.MinMaxStringList = new List<string> { "1", "2", "3","4","5","6","7","8" };

      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxStringList), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 7"));

      sut.MinMaxStringList = new List<string> { "1" };
      ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxStringList), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 3"));
    }

    [Run]
    public void MinMax_03()
    {
      var sut = new LengthDoc { MinMaxStringArray = new []{ "are", "aaa", "ddddd" } };
      Aver.IsNull(sut.Validate());
      sut.MinMaxStringArray = new [] { "1", "2", "3", "4", "5", "6", "7", "8" };
      var ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxStringArray), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("exceeds max length of 7"));

      sut.MinMaxStringArray = new []{ "1" };
      ve = sut.Validate() as FieldValidationException;
      Aver.AreEqual(nameof(LengthDoc.MinMaxStringArray), ve.FieldName);
      ve.ClientMessage.See();
      Aver.IsTrue(ve.ClientMessage.Contains("shorter than min length of 3"));
    }



    public class LengthDoc : TypedDoc
    {
      [Field]
      public string AnyString{ get; set; }

      [Field]
      public List<string> AnyStringList { get; set; }

      [Field]
      public string[] AnyStringArray { get; set; }

      [Field(minLength: 3)]
      public string MinString { get; set; }

      [Field(minLength: 5)]
      public List<string> MinStringList { get; set; }

      [Field(minLength: 7)]
      public string[] MinStringArray { get; set; }

      [Field(maxLength: 3)]
      public string MaxString { get; set; }

      [Field(maxLength: 3)]
      public List<string> MaxStringList { get; set; }

      [Field(maxLength: 3)]
      public string[] MaxStringArray { get; set; }

      [Field(minLength: 3, maxLength: 7)]
      public string MinMaxString { get; set; }

      [Field(minLength: 3, maxLength: 7)]
      public List<string> MinMaxStringList { get; set; }

      [Field(minLength: 3, maxLength: 7)]
      public string[] MinMaxStringArray { get; set; }

    }

  }
}
