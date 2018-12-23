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
    public MongoDbSink(ISinkOwner owner) : base(owner)
    =>
      m_DataStore = new MongoDbLogMessageDataStore(this);

    public MongoDbSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    =>
      m_DataStore = new MongoDbLogMessageDataStore(this);


    private MongoDbLogMessageDataStore m_DataStore;

    protected override void DoStart()
    {
      base.DoStart();
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
