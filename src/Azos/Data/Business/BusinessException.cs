using System;
using System.Runtime.Serialization;
using Azos.Serialization.JSON;

namespace Azos.Data.Business
{
  /// <summary>
  /// Thrown to indicate issues related to data/business processing
  /// </summary>
  [Serializable]
  public class BusinessException : AzosException, IHttpStatusProvider, IExternalStatusProvider
  {
    public BusinessException() { }
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception inner) : base(message, inner) { }
    protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode
     => InnerException.SearchThisOrInnerExceptionOf<IHttpStatusProvider>()?.HttpStatusCode ?? 500;

    public string HttpStatusDescription
     => InnerException.SearchThisOrInnerExceptionOf<IHttpStatusProvider>()?.HttpStatusDescription ?? "Processing error";

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = new JsonDataMap();
      result["ns"] = "business";
      result["code"] = Code;

      var esp = this.InnerException.SearchThisOrInnerExceptionOf<IExternalStatusProvider>();

      if (esp != null)
      {
        var errorData = esp.ProvideExternalStatus(includeDump);
        if (errorData != null)
          result["error"] = errorData;

        return result;
      }

      return result;
    }
  }
}
