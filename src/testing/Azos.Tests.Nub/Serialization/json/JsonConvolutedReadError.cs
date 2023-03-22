/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization.json
{

  /// <summary>
  /// AZ Issue #836
  /// </summary>
  [Runnable]
  public class JsonConvolutedReadError
  {
    [Run]
    public void Test01()
    {
      var json = Azos.Platform.EmbeddedResource.GetText(typeof(JsonConvolutedReadError), "bad-json-body.txt");
      var got = json.JsonToDataObject();
      got.See();

      var json2 = got.ToJson();
      var got2 = json2.JsonToDataObject();
      got2.See();

    }

  }
}
