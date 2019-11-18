/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Client
{
  /// <summary>
  /// Extensions address cross-cutting concerns (e.g. logging) and provide extensions for various activities
  /// </summary>
  public interface IAspect : INamed, IOrdered
  {
  }
}
