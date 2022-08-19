/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.BSON;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Provides an efficient data vector for marshalling and storage of todo's in queues.
  /// This type obviates the need of extra serialization for teleportation and storage of Todo instances.
  /// Special-purposed Glue binding is used to teleport TodoFrames and directly store them in queue without
  /// unnecessary intermediate serialization steps
  /// </summary>
  [Serializable]
  public struct TodoFrame
  {
    public const int SERIALIZER_DEFAULT = 0;
    public const int SERIALIZER_BSON = 1;

    public GDID ID;
    public Guid Type;
    public DateTime CreateTimestampUTC;
    public string ShardingKey;
    public string ParallelKey;
    public int Priority;
    public DateTime StartDate;
    public string CorrelationKey;

    public int State;
    public int Tries;

    public int Serializer;
    public byte[] Content;



    public bool Assigned { get { return !ID.IsZero; } }


    /// <summary>
    /// Internal. Returns the original instance that was passed to .ctor
    /// This allows to use this structure for dual purpose.
    /// </summary>
    [NonSerialized] internal readonly Todo ____CtorOriginal;

    /// <summary>
    /// Frames the Todo instance, pass serialize null to frame only Sys Fields without content
    /// </summary>
    public TodoFrame(Todo todo, int? serializer = SERIALIZER_DEFAULT)
    {
      if (todo == null)
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + "TodoFrame.ctor(todo==null)");

      ____CtorOriginal = todo;

      var qa = GuidTypeAttribute.GetGuidTypeAttribute<Todo, TodoQueueAttribute>(todo.GetType());

      if (serializer.HasValue)
      {
        if (serializer == SERIALIZER_DEFAULT) serializer = SERIALIZER_BSON;
        else
        if (serializer != SERIALIZER_BSON)//we only support BSON for now
          throw new WorkersException(StringConsts.TODO_FRAME_SER_NOT_SUPPORTED_ERROR.Args(todo.GetType().Name, serializer));

        byte[] content;
        try
        {
          var cdoc = DataDocConverter.DefaultInstance.DataDocToBSONDocument(todo, null);
          content = cdoc.WriteAsBSONToNewArray();
        }
        catch (Exception error)
        {
          throw new WorkersException(StringConsts.TODO_FRAME_SER_ERROR.Args(todo.GetType().FullName, error.ToMessageWithType()), error);
        }

        this.Serializer = serializer.Value;
        this.Content = content;
      }
      else
      {
        this.Serializer = 0;
        this.Content = null;
      }

      var t = todo.GetType();

      this.ID = todo.SysID;
      this.Type = qa.TypeGuid;
      this.CreateTimestampUTC = todo.SysCreateTimeStampUTC;
      this.ShardingKey = todo.SysShardingKey;
      this.ParallelKey = todo.SysParallelKey;
      this.Priority = todo.SysPriority;
      this.StartDate = todo.SysStartDate < todo.SysCreateTimeStampUTC ? todo.SysCreateTimeStampUTC : todo.SysStartDate;

      var ct = todo as CorrelatedTodo;
      this.CorrelationKey = ct!=null ? ct.SysCorrelationKey : null;

      this.State = todo.SysState.State;
      this.Tries = todo.SysTries;
    }

    //private static volatile Dictionary<string, Type> s_TypesCache = new Dictionary<string, Type>(StringComparer.Ordinal);

    /// <summary>
    /// Materializes the Todo instance represented by this frame in the scope of IGuidTypeResolver
    /// </summary>
    public Todo Materialize(IGuidTypeResolver resolver)
    {
      if (____CtorOriginal!=null) return ____CtorOriginal;

      //1. Resolve type
      var type = resolver.Resolve(this.Type);

      //3. Create TODO instance
      var result = (Todo)Azos.Serialization.SerializationUtils.MakeNewObjectInstance( type );
      result.____Deserialize( ID, CreateTimestampUTC );

      result.SysShardingKey = ShardingKey;
      result.SysParallelKey = ParallelKey;
      result.SysPriority    = Priority;
      result.SysStartDate   = StartDate;

      var ct = result as CorrelatedTodo;
      if (ct!=null)
        ct.SysCorrelationKey = CorrelationKey;

      result.SysTries = Tries;
      result.SysState = new Todo.ExecuteState(State, true);

      //4. Deserialize content
      if (Serializer!=TodoFrame.SERIALIZER_BSON) //for now only support this serializer
        throw new WorkersException(StringConsts.TODO_FRAME_DESER_NOT_SUPPORTED_ERROR.Args(type.Name, Serializer));

      try
      {
        var docContent = BSONDocument.FromArray(Content);
        DataDocConverter.DefaultInstance.BSONDocumentToDataDoc(docContent, result, null);
      }
      catch(Exception error)
      {
        throw new WorkersException(StringConsts.TODO_FRAME_DESER_ERROR.Args(type.Name, error.ToMessageWithType()), error);
      }

      return result;
    }


  }
}
