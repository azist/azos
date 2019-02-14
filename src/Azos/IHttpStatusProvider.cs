
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
