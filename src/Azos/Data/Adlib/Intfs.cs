/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Collections;

namespace Azos.Data.Adlib
{
  public interface IAdlib
  {
    /// <summary>
    /// Gets a section (like a named collection of library items) by name or throws
    /// </summary>
    Section this[string name] { get; }

    /// <summary>
    /// Gets all sections
    /// </summary>
    IRegistry<Section> Sections { get; }
  }

  public interface IAdlibLogic : IAdlib, IModule
  {
  }

  public class Section : INamed
  {
    public string Name => throw new NotImplementedException();

    //IEnumerable<ItemInfo> GetListAsync(ItemFilter filter)//bool fetchContent
    //Item GetAsync(GDID gItem)
    //public ChangeResult SaveAsync(ItemEntity item){ }
    //public ChangeResult DeleteAsync(GDID gItem);
  }


}
