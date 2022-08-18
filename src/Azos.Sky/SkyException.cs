/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Sky
{
  /// <summary> Marker interfaces for all Sky OS exceptions  </summary>
  public interface ISkyException { }

  /// <summary> Base exception thrown by the Sky OS </summary>
  [Serializable]
  public class SkyException : AzosException, ISkyException
  {
    public const string SENDER_FLD_NAME = "SKYE-S";
    public const string TOPIC_FLD_NAME = "SKYE-T";

    public static string DefaultSender;
    public static string DefaultTopic;

    public readonly string Sender;
    public readonly string Topic;


    public SkyException()
    {
      Sender = DefaultSender;
      Topic = DefaultTopic;
    }

    public SkyException(int code)
    {
      Code = code;
      Sender = DefaultSender;
      Topic = DefaultTopic;
    }

    public SkyException(int code, string message) : this(message, null, code, null, null) {}
    public SkyException(string message) : this(message, null, 0, null, null) { }
    public SkyException(string message, Exception inner) : this(message, inner, 0, null, null) { }

    public SkyException(string message, Exception inner, int code, string sender, string topic) : base(message, inner)
    {
      Code = code;
      Sender = sender ?? DefaultSender;
      Topic = topic ?? DefaultTopic;
    }

    protected SkyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      Sender = info.GetString(SENDER_FLD_NAME);
      Topic = info.GetString(TOPIC_FLD_NAME);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(SENDER_FLD_NAME, Sender);
      info.AddValue(TOPIC_FLD_NAME, Topic);
      base.GetObjectData(info, context);
    }
  }
}
