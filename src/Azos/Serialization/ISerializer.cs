
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azos.Serialization
{

    /// <summary>
    /// Denotes ser/deser operations
    /// </summary>
    public enum SerializationOperation
    {
      /// <summary>
      /// Serializing object to stream
      /// </summary>
      Serializing,

      /// <summary>
      /// Deserializing object from stream
      /// </summary>
      Deserializing
    };


    /// <summary>
    /// Describes an entity that can serialize and deserialize objects
    /// </summary>
    public interface ISerializer
    {
       void Serialize(Stream stream, object root);
       object Deserialize(Stream stream);

       /// <summary>
       /// Indicates whether Serialize/Deserialize may be called by multiple threads at the same time
       /// </summary>
       bool IsThreadSafe{get;}
    }


    /// <summary>
    /// Describes an entity that can serialize and deserialize objects and can be disposed
    /// </summary>
    public interface IDisposableSerializer : ISerializer, IDisposable
    {

    }




}
