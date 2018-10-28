/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Data.Access.MongoDb;


namespace Azos.Log.Sinks
{
    /// <summary>
    /// Implements destination that sends log messages into MongoDB
    /// </summary>
    public class MongoDbSink : Sink
    {

             /// <summary>
            /// Creates a new instance of destination that stores log MongoDB
            /// </summary>
            public MongoDbSink() : base(null)
            {
            }

            /// <summary>
            /// Creates a new instance of destination that stores log MongoDB
            /// </summary>
            public MongoDbSink(string name, string connectString, string dbName, string collectionName = null) : base(name)
            {
              m_DataStore.ConnectString = connectString;
              m_DataStore.DatabaseName = dbName;
              m_DataStore.CollectionName = collectionName;
            }



        private MongoDbLogMessageDataStore m_DataStore = new MongoDbLogMessageDataStore();


        /// <summary>
        /// Refrences an underlying data store
        /// </summary>
        public MongoDbLogMessageDataStore DataStore
        {
          get { return m_DataStore; }
        }


        public override void Open()
        {
            base.Open();
            if (TestOnStart) m_DataStore.TestConnection();
        }



        protected override void DoConfigure(Conf.IConfigSectionNode node)
        {
            base.DoConfigure(node);
            ConfigAttribute.Apply(m_DataStore, node);
        }

        protected override void DoSend(Message entry)
        {
          m_DataStore.SendMessage(entry);
        }
    }
}
