/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Data.Directory
{
  partial class RemoteDirectory
  {
    public Task<Item> GetAsync(ItemId id, bool touch = false)
    {
      throw new NotImplementedException();
    }

    public Task Save(Item item)
    {
      throw new NotImplementedException();
    }

    public Task<bool> Touch(ItemId id)
    {
      throw new NotImplementedException();
    }

    public Task<bool> Delete(ItemId id)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Item>> Query(string itemType, string queryExpression)
    {
      throw new NotImplementedException();
    }

    public void TestConnection()
    {
      throw new NotImplementedException();
    }
  }
}
