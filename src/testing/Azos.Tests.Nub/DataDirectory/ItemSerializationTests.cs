using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data.Directory;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataDirectory
{
  [Runnable]
  public class ItemSerializationTests
  {
    [Run]
    public void Test1()
    {
      var item = new Item(new ItemId("abc", new Data.GDID(12, 12121)))
      {
        Data = "data string content"
      };

      var json = JsonWriter.Write(item);
      json.See();

      var got = new Item();
      got.ReadAsJson(json.JsonToDataObject(), false, null);

      got.See();


    }
  }
}
