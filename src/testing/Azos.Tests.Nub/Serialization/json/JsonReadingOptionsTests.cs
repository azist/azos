/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;
using Azos.CodeAnalysis.Source;
using Azos.Conf;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonReadingOptionsTests
  {
    [Run("depth1", @"
      json='{a0:{a1:{a2:{a3: 567}}}}'
      ecode='eGraphDepthLimit'
      pass{ max-depth=4 }
      fail{ max-depth=3 }")]

    [Run("depth2", @"
      json='{a0:[{a2:{a3: 567}}]}'
      ecode='eGraphDepthLimit'
      pass{ max-depth=4 }
      fail{ max-depth=3 }")]

    [Run("maxlen", @"
      json='{a: 1, b: 2, c: 3, ""long"": ""value"", arr: [null, null,null, null, null, null,null, null,null, null,null, null,null, null]}'
      pass{ max-char-length=150 }
      fail{ max-char-length=100 }")]

    [Run("maxobjects", @"
      json='{a: 1, b: {}, c: { d: {}}}'
      pass{ max-objects=20 }
      fail{ max-objects=2 }")]

    [Run("maxarrays", @"
      json='{a: 1, b: [1,2,3], c: [ [ ] ]}'
      pass{ max-arrays=20 }
      fail{ max-arrays=2 }")]

    [Run("maxobjitems", @"
      json='{a: 1, b: 2, c: 3, d: 4, e: 5}'
      pass{ max-object-items=20 }
      fail{ max-object-items=2 }")]

    [Run("maxarrayitems", @"
      json='{a: 1, b: [1,2,null,4,5,6]}'
      pass{ max-array-items=8 }
      fail{ max-array-items=5 }")]

    [Run("maxkeylength", @"
      json='{a: 1, ThisKeyNameIsVeryLong: 2}'
      pass{ max-key-length=25 }
      fail{ max-key-length=20 }")]

    [Run("maxstringlength", @"
      json='{a: 1, b: ""This is a very very long and nasty string!""}'
      pass{ max-string-length=45 }
      fail{ max-string-length=40 }")]

    [Run("maxcommentlength", @"
      json='{a: 1, /* This is a very very long and nasty comment! */ b: 2}'
      pass{ max-comment-length=48 }
      fail{ max-comment-length=40 }")]
    public async Task TestCase(string json, IConfigSectionNode pass, IConfigSectionNode fail, string ecode = "eLimitExceeded")
    {
      #region Part 1 - Sync test
      var got = JsonReader.Deserialize(json, ropt: null) as JsonDataMap;//pases with default/null options
      Aver.IsNotNull(got);
      got.See();

      var optPass = new JsonReadingOptions(pass);
      got = JsonReader.Deserialize(json, optPass) as JsonDataMap;
      Aver.IsNotNull(got);

      var optFail = new JsonReadingOptions(fail);
      try
      {
        got = JsonReader.Deserialize(json, optFail) as JsonDataMap;
        Aver.Fail("SYNC Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Sync Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains(ecode), "Expected error code: " + ecode);
      }
      #endregion

      #region Part 2 - Async test
      var jsonSource = new StringSource(json);
      got = await JsonReader.DeserializeAsync(jsonSource, null) as JsonDataMap;
      Aver.IsNotNull(got);
      // got.See();
      got = await JsonReader.DeserializeAsync(jsonSource.Reset(), optPass) as JsonDataMap;
      Aver.IsNotNull(got);
      //  got.See();
      try
      {
        got = await JsonReader.DeserializeAsync(jsonSource.Reset(), optFail) as JsonDataMap;
        Aver.Fail("ASYNC Cant be here");
      }
      catch (JSONDeserializationException jde)
      {
        "Async Expected and got: {0}".SeeArgs(jde.ToMessageWithType());
        Aver.IsTrue(jde.Message.Contains(ecode), "Expected error code: " + ecode);
      }
      #endregion
    }
  }
}
