/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Financial;
using Azos.Data;
using Azos.Serialization.Arow;
using Azos.Serialization.JSON;
using Azos.Collections;
using Azos.Pile;
using System.Linq;
using Azos.Scripting;

namespace Azos.Tests.Nub.Serialization
{


//////#warning FInish after JSOn -> doc refactoring
//////  /// <summary>
//////  /// Covers all primitive and intrinsic types
//////  /// </summary>
//////  [Runnable]
//////  public class AllTypesDocJsonTests : AmorphousTypedDoc
//////  {
//////    [Run]
//////    public void Json()
//////    {
//////      var d1 = new AllTypesDoc();
//////      d1.Populate();

//////      var jsonString = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
//////      Console.WriteLine(jsonString);
//////      // System.IO.File.WriteAllText("c:\\azos\\jzon.txt", jsonString);
//////      var jsonMap = jsonString.JsonToDataObject() as JsonDataMap;


//////      var d2 = new AllTypesDoc();
//////      JsonReader.ToDoc(d2, jsonMap);

//////      d1.AverEquality(d2);
//////    }
//////  }
}
