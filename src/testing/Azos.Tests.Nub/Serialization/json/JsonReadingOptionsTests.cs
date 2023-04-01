/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;
using Azos.CodeAnalysis.Source;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonReadingOptionsTests
  {
    [Run]
    public void MaxDepth_1()
    {
      var json = @"{a0:{a1:{a2:{a3: 567}}}}";
      var got = JsonReader.Deserialize(json, null) as JsonDataMap;
     // got.See();

      var roptLimit4 = new JsonReadingOptions(){ MaxDepth = 4};
      got = JsonReader.Deserialize(json, roptLimit4) as JsonDataMap;
    //  got.See();

      var roptLimit3 = new JsonReadingOptions() { MaxDepth = 3 };
      try
      {
        got = JsonReader.Deserialize(json, roptLimit3) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch(JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eGraphDepthLimit"));
      }
    }

    [Run]
    public async Task MaxDepthAsync_1()
    {
      var json = new StringSource(@"{a0:{a1:{a2:{a3: 567}}}}");

      var got = await JsonReader.DeserializeAsync(json, null) as JsonDataMap;
    //  got.See();

      var roptLimit4 = new JsonReadingOptions() { MaxDepth = 4 };
      got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit4) as JsonDataMap;

      var roptLimit3 = new JsonReadingOptions() { MaxDepth = 3 };
      try
      {
        got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit3) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eGraphDepthLimit"));
      }
    }

    [Run]
    public void MaxDepth_2()
    {
      var json = @"{a0:[{a2:{a3: 567}}]}";
      var got = json.JsonToDataObject(null) as JsonDataMap;

      var roptLimit4 = new JsonReadingOptions() { MaxDepth = 4 };
      got = json.JsonToDataObject(roptLimit4) as JsonDataMap;

      var roptLimit3 = new JsonReadingOptions() { MaxDepth = 3 };
      try
      {
        got = json.JsonToDataObject(roptLimit3) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eGraphDepthLimit"));
      }
    }

    [Run]
    public async Task MaxDepthAsync_2()
    {
      var json = new StringSource(@"{a0:[{a2:{a3: 567}}]}");
      var got = await JsonReader.DeserializeAsync(json, null) as JsonDataMap;

      var roptLimit4 = new JsonReadingOptions() { MaxDepth = 4 };
      got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit4) as JsonDataMap;

      var roptLimit3 = new JsonReadingOptions() { MaxDepth = 3 };
      try
      {
        got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit3) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eGraphDepthLimit"));
      }
    }

    [Run]
    public void MaxCharLength_1()
    {
      var json = @"{a: 1, b: 2, c: 3, 'long': 'value', arr: [null, null,null, null,null, null,null, null,null, null,null, null,null, null]}";
      var got = json.JsonToDataObject(null) as JsonDataMap;
      // got.See();

      var roptLimit500 = new JsonReadingOptions() { MaxCharLength = 500 };
      got = json.JsonToDataObject(roptLimit500) as JsonDataMap;
      //  got.See();

      var roptLimit100 = new JsonReadingOptions() { MaxCharLength = 100 };
      try
      {
        got = json.JsonToDataObject(roptLimit100) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eLimitExceeded"));
      }
    }

    [Run]
    public async Task MaxCharLengthAsync_1()
    {
      var json = new StringSource(@"{a: 1, b: 2, c: 3, 'long': 'value', arr: [null, null,null, null,null, null,null, null,null, null,null, null,null, null]}");
      var got = await JsonReader.DeserializeAsync(json, null) as JsonDataMap;
      // got.See();

      var roptLimit500 = new JsonReadingOptions() { MaxCharLength = 500 };
      got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit500) as JsonDataMap;
      //  got.See();

      var roptLimit100 = new JsonReadingOptions() { MaxCharLength = 100 };
      try
      {
        got = await JsonReader.DeserializeAsync(json.Reset(), roptLimit100) as JsonDataMap;
        Aver.Fail("Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains("eLimitExceeded"));
      }
    }


  }
}
