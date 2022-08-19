/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Represents MVC action result that returns ROW as JSON object for WV.RecordModel(...) ctor init
  /// </summary>
  public struct ClientRecord : IActionResult
  {
    public ClientRecord(Doc doc,
                        Exception validationError,
                        Atom isoLang,
                        string recID = null,
                        string target = null,
                        Client.ModelFieldValueListLookupFunc valueListLookupFunc = null)
    {
      RecID = recID;
      Doc = doc;
      ValidationError = validationError;
      Target = target;
      IsoLang = isoLang;
      ValueListLookupFunc = valueListLookupFunc;
    }

    public ClientRecord(Doc doc,
                        Exception validationError,
                        Func<Schema.FieldDef, JsonDataMap> simpleValueListLookupFunc,
                        Atom isoLang,
                        string recID = null,
                        string target = null)
    {
      RecID = recID;
      Doc = doc;
      ValidationError = validationError;
      Target = target;
      IsoLang = isoLang;
      if (simpleValueListLookupFunc!=null)
        ValueListLookupFunc = (_sender, _row, _def, _target, _iso) => simpleValueListLookupFunc(_def);
      else
        ValueListLookupFunc = null;
    }

    public readonly string RecID;
    public readonly Doc Doc;
    public readonly Exception ValidationError;
    public readonly string Target;
    public readonly Atom IsoLang;
    public readonly Client.ModelFieldValueListLookupFunc ValueListLookupFunc;


    public Task ExecuteAsync(Controller controller, WorkContext work)
    {
      var gen = (work.Portal != null) ? work.Portal.RecordModelGenerator
                                      : Client.RecordModelGenerator.DefaultInstance;

      return work.Response.WriteJsonAsync( gen.RowToRecordInitJSON(Doc, ValidationError, IsoLang, RecID, Target, ValueListLookupFunc) );
    }
  }
}
