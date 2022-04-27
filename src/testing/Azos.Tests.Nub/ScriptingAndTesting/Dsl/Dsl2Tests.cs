/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.ScriptingAndTesting.Dsl
{
  [Runnable]
  public class Dsl2Tests
  {

    private string get_TEZT_JSON_01()
    {
      return JsonWriter.Write(
        new object[]
        {
          new
          {
            server = "Rinaldo",
            drinks = new
            {
              wines = new string[] { "fine", "mad dog", "cheap" },
              beers = new string[] { "cheer", "corona", "XX" },
              sodas = new string[] { "cherry cola", "orange", "grape" }
            },
            appetizers = new string[] { "chips", "cheese fries", "pickled pigs feet"}
          }
        }
        , JsonWritingOptions.PrettyPrintRowsAsMap
        );
    }

    [Run]
    public async Task GetTeztJson01()
    {
      get_TEZT_JSON_01().See();
    }

    public const string JSON_LOAD_ITERATE = @"
      script
      {
        type-path='Azos.Scripting.Dsl, Azos;Azos.Data.Dsl, Azos'

        do{ type='JsonObjectLoader' name=d1 fileName='JSON_01.json'}

        do
        {
          type='ForEachDatum'
          from=d1
          into='item'
          body
          {
            do{type='Set' global=obj to='local.item'}
            do{type='Set' global=server to='global.obj.server'}
            do{type='See' format='Server is: {~global.server}'}
          }
        }

        //do{ type='DumpGlobalState'}
        //do{ type='DumpLocalState'}

      }
    ";

    [Run]
    public async Task JsonLoadIterate()
    {
      var fn = "JSON_01.json";
      saveJsonFile(fn, get_TEZT_JSON_01()); // ********** SAVE FILE **********

      var runnable = new StepRunner(NOPApplication.Instance, JSON_LOAD_ITERATE.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
      var state = await runnable.RunAsync();

      delJsonFile(fn); // ********** DEL FILE **********

      Aver.AreEqual("Rinaldo", runnable.GlobalState["server"].AsString());
    }

    #region Private Utility Methods

    private void saveJsonFile(string fileName, string json)
    {
      if(File.Exists(fileName))File.Delete(fileName);
      File.WriteAllText(fileName, json);
    }

    private void delJsonFile(string fileName)
    {
      if (File.Exists(fileName)) File.Delete(fileName);
    }

    #endregion

  }
}
