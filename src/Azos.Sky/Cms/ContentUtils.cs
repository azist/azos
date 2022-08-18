/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Linq;

using Azos.Web;

namespace Azos.Sky.Cms
{
  /// <summary>
  /// Provides various extensions related to Content
  /// </summary>
  public static class ContentUtils
  {
    /// <summary>
    /// Tries to map the content type to the local Mapping object, returning ContentType.Mapping.GENERIC_BINARY
    /// if the specific mapping could not be made
    /// </summary>
    /// <param name="content">A non null content object returned from the Cms</param>
    /// <param name="app">
    /// The application context under which the mapping is performed, if
    /// null the ambient app context is used
    /// </param>
    public static ContentType.Mapping GetTypeMapping(Content content, IApplication app = null)
    {
      content.NonNull(nameof(content));
      if (app == null) app = Apps.ExecutionContext.Application;
      var mapping =  app.GetContentTypeMappings().MapContentType(content.ContentType).FirstOrDefault();
      if (mapping == null) mapping = ContentType.Mapping.GENERIC_BINARY;
      return mapping;
    }


  }
}
