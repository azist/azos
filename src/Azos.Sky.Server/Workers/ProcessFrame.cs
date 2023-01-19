/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Serialization.BSON;

namespace Azos.Sky.Workers
{

  /// <summary>
  /// Denotes process statuses: Created, Started, Finished etc..
  /// </summary>
  public enum ProcessStatus
  {
    Created,
    Started,
    Finished,
    Canceled,
    Terminated,
    Failed
  }


  /// <summary>
  /// Provides an efficient data vector for marshalling and storage of process.
  /// This type obviates the need of extra serialization for teleportation and storage of Process instances.
  /// Special-purposed Glue binding is used to teleport ProcessFrames and directly store them without
  /// unnecessary intermediate serialization steps
  /// </summary>
  [Serializable]
  public struct ProcessFrame
  {
    public const int SERIALIZER_DEFAULT = 0;
    public const int SERIALIZER_BSON = 1;

    public Guid Type;
    public ProcessDescriptor Descriptor;

    public int Serializer;
    public byte[] Content;

    /// <summary>
    /// Internal. Returns the original instance that was passed to .ctor
    /// This allows to use this structure for dual purpose.
    /// </summary>
    [NonSerialized] internal readonly Process ____CtorOriginal;

    /// <summary>
    /// Frames the Todo instance, pass serialize null to frame only Sys Fields without content
    /// </summary>
    public ProcessFrame(Process process, int? serializer = SERIALIZER_DEFAULT)
    {
      if (process == null)
        throw new WorkersException(ServerStringConsts.ARGUMENT_ERROR + "ProcessFrame.ctor(process==null)");

      ____CtorOriginal = process;

      var qa = GuidTypeAttribute.GetGuidTypeAttribute<Process, ProcessAttribute>(process.GetType());

      if (serializer.HasValue)
      {
        if (serializer == SERIALIZER_DEFAULT) serializer = SERIALIZER_BSON;
        else
        if (serializer != SERIALIZER_BSON)//we only support BSON for now
          throw new WorkersException(ServerStringConsts.PROCESS_FRAME_SER_NOT_SUPPORTED_ERROR.Args(process.GetType().Name, serializer));

        byte[] content;
        try
        {
          var cdoc = DataDocConverter.DefaultInstance.DataDocToBSONDocument(process, null);
          content = cdoc.WriteAsBSONToNewArray();
        }
        catch (Exception error)
        {
          throw new WorkersException(ServerStringConsts.PROCESS_FRAME_SER_ERROR.Args(process.GetType().FullName, error.ToMessageWithType()), error);
        }

        this.Serializer = serializer.Value;
        this.Content = content;
      }
      else
      {
        this.Serializer = 0;
        this.Content = null;
      }

      this.Type = qa.TypeGuid;
      this.Descriptor = process.SysDescriptor;
    }

    //private static volatile Dictionary<string, Type> s_TypesCache = new Dictionary<string, Type>(StringComparer.Ordinal);

    /// <summary>
    /// Materializes the Process instance represented by this frame in the scope of IGuidTypeResolver
    /// </summary>
    public Process Materialize(IGuidTypeResolver resolver)
    {
      if (____CtorOriginal!=null) return ____CtorOriginal;

      //1. Resolve type
      var type = resolver.Resolve(this.Type);

      //3. Create Process instance
      var result = (Process)Azos.Serialization.SerializationUtils.MakeNewObjectInstance( type );
      result.____Deserialize(Descriptor);

      //4. Deserialize content
      if (Serializer!=ProcessFrame.SERIALIZER_BSON) //for now only support this serializer
        throw new WorkersException(ServerStringConsts.PROCESS_FRAME_DESER_NOT_SUPPORTED_ERROR.Args(type.Name, Serializer));

      try
      {
        var docContent = BSONDocument.FromArray(Content);
        DataDocConverter.DefaultInstance.BSONDocumentToDataDoc(docContent, result, null);
      }
      catch(Exception error)
      {
        throw new WorkersException(ServerStringConsts.PROCESS_FRAME_DESER_ERROR.Args(type.Name, error.ToMessageWithType()), error);
      }

      return result;
    }
  }
}
