/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos
{
  /// <summary>
  /// Denotes entities capable of providing HttpStatusCode.
  /// This interface is typically used on custom exceptions so their
  /// existence map to Http statuses, such as security exception to 403 etc.
  /// </summary>
  public interface IHttpStatusProvider
  {
    int HttpStatusCode {  get; }
    string HttpStatusDescription {  get; }
  }
}
