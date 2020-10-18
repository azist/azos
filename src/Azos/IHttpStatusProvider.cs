/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.JSON;

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

  /// <summary>
  /// Denotes entities capable of providing their status data to external callers,
  /// for example this is used for custom exceptions which respond with a detailed
  /// list of causes, such as validation exceptions include failed schema/field names etc.
  /// The status data get disclosed to callers even if exception developer details are not disclosed,
  /// for example when `show-dump`=false stack traces and other development-related details are not included,
  /// however the result map entries which describe the essence of the error are still added
  /// </summary>
  public interface IExternalStatusProvider
  {
    /// <summary>
    /// Provides data describing the entity status/state.
    /// Implementor may provide logic to conditionally disclose details
    /// based on Session/Principal context
    /// </summary>
    /// <param name="includeDump">True to include dev dump, such as stack traces</param>
    /// <returns>JsonDataMap filled with data or null if there is nothing to provide</returns>
    JsonDataMap ProvideExternalStatus(bool includeDump);
  }
}
