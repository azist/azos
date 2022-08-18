/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;

namespace Azos.Web.Cms.Default
{
  /// <summary>
  /// Represents a CMS System Source where content is fetched from
  /// </summary>
  public interface ICmsSource: IApplicationComponent, IConfigurable, IDisposable
  {
    /// <summary>
    /// Fetches all language data for all portals
    /// </summary>
    /// <remarks>Implementation does not need to cache the result, as the caller does the caching already</remarks>
    Task<Dictionary<string, IEnumerable<LangInfo>>> FetchAllLangDataAsync();

    /// <summary>
    /// Fetches content from the backing source
    /// </summary>
    /// <remarks>
    /// The in-memory caching is already done by CmsFacade so the implementation of this method
    /// should go and fetch content directly without extra in-memory caching.
    /// </remarks>
    Task<Content> FetchContentAsync(ContentId id, Atom isoLang, DateTime utcNow, ICacheParams caching);
  }

}
