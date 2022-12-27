/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;

namespace Azos.Sky.Cms
{
  /// <summary>
  /// Defines a facade for consuming content from a content management system.
  /// This facade only provides read-only CMS access for consumption (hence the name).
  /// Facades are usually modules -they get installed in App chassis at app boot
  /// </summary>
  public interface ICmsFacade : IApplicationComponent
  {
    /// <summary>
    /// Returns global default language information (regardless of portal). By default this is set to ENG/English
    /// </summary>
    LangInfo DefaultGlobalLanguage { get; }

    /// <summary>
    /// Returns the ids of all known portals
    /// </summary>
    Task<IEnumerable<string>> GetAllPortalsAsync();

    /// <summary>
    /// Returns all languages that this CMS supports for portal.
    /// This method is typically used to show language selector on the UI
    /// </summary>
    /// <param name="portal">Required portal name</param>
    /// <returns>Enumerable of LangInfo entries throws if portal is not found</returns>
    Task<IEnumerable<LangInfo>> GetAllSupportedLanguagesAsync(string portal);

    /// <summary>
    /// Retrieves a content block by the specified id per optional lang iso code.
    /// Note: The caller authorization is checked per target resource permissions requirement (if any)
    /// </summary>
    /// <param name="id">ContentID of a block to retrieve</param>
    /// <param name="isoLang">Iso language code for content, if null then default language (Generic English) is returned</param>
    /// <param name="caching">Optional cache handling specification</param>
    /// <returns>
    /// Content or Null if it is not found. Authorization exceptions if caller
    /// does not have permissions required (if any) by the target content
    /// </returns>
    /// <remarks>
    /// Checks authorization, then tries to synchronously take the request from cache, and when not found locally
    /// only then fires up an async IO call
    /// </remarks>
    Task<Content> GetContentAsync(ContentId id, Atom? isoLang = null, ICacheParams caching = null);
  }
}
