/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

using Azos.Data.Access.MongoDb.Connector;
using Azos.Sky.Mdb;

namespace Azos.Sky.MongoDb
{
  /// <summary>
  /// Thrown to indicate mongo-device-related problems (server side)
  /// </summary>
  [Serializable]
  public class MongoDeviceException : MdbException
  {
    public MongoDeviceException() : base() { }
    public MongoDeviceException(string message) : base(message) { }
    public MongoDeviceException(string message, Exception inner) : base(message, inner) { }
    protected MongoDeviceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


  /// <summary>
  /// Thrown to indicate mongo-device-related problems (server side)
  /// </summary>
  [Serializable]
  public class MongoDeviceCRUDException : MongoDeviceException
  {
    public const string RESULT_FLD_NAME = "MDCE-R";

    public readonly CRUDResult Result;

    public MongoDeviceCRUDException() : base() { }
    public MongoDeviceCRUDException(CRUDResult result, string message) : base(makeMessage(result, message))
    {
      Result = result;
    }

    public MongoDeviceCRUDException(CRUDResult result, string message, Exception inner) : base(makeMessage(result, message), inner)
    {
      Result = result;
    }

    protected MongoDeviceCRUDException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      Result = (CRUDResult)info.GetValue(RESULT_FLD_NAME, typeof(CRUDResult));
    }

    private static string makeMessage(CRUDResult crud, string message)
    {
      var result = message + Environment.NewLine;

      if (crud.WriteErrors != null)
        foreach (var err in crud.WriteErrors)
          result += "Code: {0}, Index: {1}, Msg: '{2}', Info: '{3}'{4}".Args(err.Code, err.Index, err.Message, err.Info, Environment.NewLine);

      return result;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue(RESULT_FLD_NAME, Result);
    }
  }
}
