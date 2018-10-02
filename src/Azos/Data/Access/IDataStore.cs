
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access
{
      /// <summary>
      /// Represents a store that can save and retrieve data
      /// </summary>
      public interface IDataStore : IApplicationComponent
      {
         /// <summary>
         /// Returns the name of the underlying store technology, i.e. "ORACLE".
         /// This property is used by some metadata-based validation logic which is target-dependent
         /// </summary>
         string TargetName{get;}

         /// <summary>
         /// Tests connection and throws an exception if connection could not be established
         /// </summary>
         void TestConnection();
      }


      /// <summary>
      /// Represents a store that can save and retrieve data
      /// </summary>
      public interface IDataStoreImplementation : IDataStore, IDisposable, IConfigurable, IInstrumentable
      {
         /// <summary>
         /// Defines log level for data stores
         /// </summary>
         StoreLogLevel LogLevel { get; set; }
      }


}
