
using System;

using Azos.Data;

namespace TestBusinessLogic
{
  public class SimplePersonDoc : TypedDoc
  {
    [Field]public GDID ID { get; set; }
    [Field]public string Name{get; set;}
    [Field]public int Age{ get;set;}
    [Field]public bool Bool1{ get;set;}
    [Field]public string Str1{ get;set;}
    [Field]public string Str2{ get;set;}
    [Field]public DateTime Date{ get;set;}
    [Field]public double Salary{ get;set;}
  }
}
