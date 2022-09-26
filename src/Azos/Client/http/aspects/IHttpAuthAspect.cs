/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;

namespace Azos.Client
{
  /// <summary>
  /// Provides values for injection into Authorization header at runtime,
  /// e.g. OAuth and other dynamic client auth settings, where the header is NOT hard-coded and need to be obtained/refreshed
  /// periodically or with every call
  /// </summary>
  public interface IHttpAuthAspect : IAspect
  {
    Task<string> ObtainAuthorizationHeaderAsync(HttpEndpoint endpoint, object identityContext);
  }
}
