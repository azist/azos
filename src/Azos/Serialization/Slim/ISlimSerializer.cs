

namespace Azos.Serialization.Slim
{
    /// <summary>
    /// Denotes modes of handling type registry by Slim serializer
    /// </summary>
    public enum TypeRegistryMode
    {
      /// <summary>
      /// Type registry object is created for every Serialize/Deserialize call only
      /// cloning global types. This is the default mode which is thread-safe(many threads can call Serialize/Deserialize at the same time)
      /// </summary>
      PerCall=0,

      /// <summary>
      /// Type registry object is cloned from global types only once and it is retained after making calls.
      /// This is not a thread-safe mode, so only one thread may call Serialize/Deserialize at a time.
      /// This mode is beneficial for cases when many object instances of various types need to be transmitted
      /// so repeating their type names in every Ser/Deser is not efficient. In batch mode the type name is
      ///  written/read to/from stream only once, then type handles are transmitted instead thus saving space and
      ///  extra allocations
      /// </summary>
      Batch
    }

    /// <summary>
    /// Marker interface for formats based on Slim algorithm family
    /// </summary>
    public interface ISlimSerializer : ISerializer
    {
      /// <summary>
      /// Gets/sets how serializer handles type information between calls to Serialize/Deserialize.
      /// This property itself is not thread-safe, that is - it should be only set once by control/initiating thread
      /// </summary>
      TypeRegistryMode TypeMode { get; set;}

      /// <summary>
      /// Resets type registry state to initial state (which is based on global types) for TypeMode = "Batch",
      /// otherwise does nothing
      /// </summary>
      void ResetCallBatch();

      /// <summary>
      /// Returns true if last call to Serialize or Deserialize in batch mode added more types to type registry.
      /// This call is only valid in TypeMode = "Batch" and is inherently not thread-safe
      /// </summary>
      bool BatchTypesAdded { get;}
    }
}
