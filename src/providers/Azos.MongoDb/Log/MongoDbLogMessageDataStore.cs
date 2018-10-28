/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Log;
using Azos.Conf;
using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb
{
    /// <summary>
    /// Implements a store that sends log messages into MongoDB
    /// </summary>
    public class MongoDbLogMessageDataStore : MongoDbDataStoreBase
    {
        #region CONSTS

          public const string CONFIG_COLLECTION_NAME_DEFAULT = "nfx_log";

        #endregion


        #region .ctor/.dctor

          public MongoDbLogMessageDataStore()
          {
          }

          public MongoDbLogMessageDataStore(string connectString, string dbName, string collectionName)
          {
            ConnectString = connectString;
            DatabaseName = dbName;
            CollectionName = collectionName;
          }

        #endregion

        #region PrivateFields

          private string m_CollectionName;

        #endregion

        #region Properties

          /// <summary>
          /// Gets/sets collection name used for logging
          /// </summary>
          [Config("$collection")]
          public string CollectionName
          {
            get { return m_CollectionName ?? CONFIG_COLLECTION_NAME_DEFAULT; }
            set { m_CollectionName = value; }
          }

        #endregion


        #region Public

           /// <summary>
           /// Inserts log message into MongoDB
           /// </summary>
           public void SendMessage(Message msg)
           {
              var db = GetDatabase();
              var col = db[CollectionName];
              col.Insert(docFromMessage(msg));
           }

        #endregion

          #region .pvt

                  private BSONDocument docFromMessage(Message msg)
                  {
                    var doc = new BSONDocument();

                    var rc = new DataDocConverter();

                    doc.Set(new BSONStringElement("Guid", msg.Guid.ToString("N")));
                    doc.Set(new BSONStringElement("RelatedTo", msg.RelatedTo.ToString("N")));
                    doc.Set(new BSONStringElement("Type", msg.Type.ToString()));
                    doc.Set(new BSONInt32Element("Source", msg.Source));
                    doc.Set(new BSONInt64Element("TimeStamp", msg.TimeStamp.Ticks));
                    doc.Set(new BSONStringElement("Host", msg.Host));
                    doc.Set(new BSONStringElement("From", msg.From));
                    doc.Set(new BSONStringElement("Topic", msg.Topic));
                    doc.Set(new BSONStringElement("Text", msg.Text));
                    doc.Set(new BSONStringElement("Parameters", msg.Parameters));
                    doc.Set(new BSONStringElement("Exception", msg.Exception.ToMessageWithType()));

                    return doc;
                  }

          #endregion

    }
}
