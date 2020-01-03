using System;
using System.Runtime.Serialization;

namespace Azos.Data.Business
{
  /// <summary>
  /// Thrown to indicate issues related to data/business processing
  /// </summary>
  [Serializable]
  public class BusinessException : AzosException, IHttpStatusProvider
  {
    public BusinessException() { }
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception inner) : base(message, inner) { }
    protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => InnerException is IHttpStatusProvider hsp ? hsp.HttpStatusCode : 500;
    public string HttpStatusDescription => InnerException is IHttpStatusProvider hsp ? hsp.HttpStatusDescription : "Processing error";
  }
}
