/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos;
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Log;

namespace TestBusinessLogic
{
  public class SimplePersonDoc : TypedDoc
  {
    public SimplePersonDoc() { }

    //Used by unit testing injection of dependencies
    public SimplePersonDoc(ILog log) => m_Log = log;

    [Field]public GDID ID { get; set; }
    [Field]public string Name{get; set;}
    [Field]public int Age{ get;set;}
    [Field]public bool Bool1{ get;set;}
    [Field]public string Str1{ get;set;}
    [Field]public string Str2{ get;set;}
    [Field]public DateTime Date{ get;set;}
    [Field]public double Salary{ get;set;}

    [Inject] private ILog m_Log;

    public override ValidState Validate(ValidState state)
    {
      m_Log.NonNull(nameof(m_Log)).Write(MessageType.Info, "Validate was called");
      return base.Validate(state);
    }
  }
}
