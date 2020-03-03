/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Data.Access.MongoDb.Connector;

namespace Azos.Data.Access.MongoDb
{
  /// <summary>
  /// Implements MongoDB store base functionality
  /// Connect string takes form of:
  /// <code>
  /// mongo{server="mongo://localhost:27017" db="myDB"}
  /// </code>
  /// </summary>
  public abstract class MongoDbDataStoreBase : ApplicationComponent, IDataStoreImplementation
  {
    #region CONST



    #endregion

    #region .ctor/.dctor

      protected MongoDbDataStoreBase(IApplication app) : base(app)
      {
      }

      protected MongoDbDataStoreBase(IApplicationComponent director) : base(director)
      {
      }

    #endregion


    #region Private Fields

      private string m_ConnectString;
      private string m_DatabaseName;

      private string m_TargetName;
      private bool m_InstrumentationEnabled;
    #endregion


    #region IInstrumentation

      public string Name { get{return GetType().FullName;}}

      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public bool InstrumentationEnabled{ get{return m_InstrumentationEnabled;} set{m_InstrumentationEnabled = value;}}

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      {
        return ExternalParameterAttribute.GetParameters(this, groups);
      }

      /// <summary>
      /// Gets external parameter value returning true if parameter was found
      /// </summary>
      public bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
          return ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);
      }

      /// <summary>
      /// Sets external parameter value returning true if parameter was found and set
      /// </summary>
      public bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        return ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
      }

    #endregion

    #region Properties

      public override string ComponentLogTopic => MongoConsts.MONGO_TOPIC;

      /// <summary>
      /// Get/Sets MongoDB database connection string
      /// </summary>
      [Config("$connect-string")]
      public string ConnectString
      {
        get
        {
           return m_ConnectString ?? string.Empty;
        }
        set
        {
             if (m_ConnectString != value)
             {
               m_ConnectString = value;
             }
        }
      }

      /// <summary>
      /// Get/Sets MongoDB database name
      /// </summary>
      [Config("$db-name")]
      public string DatabaseName
      {
        get
        {
           return m_DatabaseName ?? string.Empty;
        }
        set
        {
             if (m_DatabaseName != value)
             {
               m_DatabaseName = value;
             }
        }
      }

      [Config]
      public StoreLogLevel LogLevel { get; set;}

      [Config]
      public string TargetName
      {
         get{ return m_TargetName.IsNullOrWhiteSpace() ? "MongoDB" : m_TargetName;}
         set{ m_TargetName = value;}
      }

    #endregion

    #region Public
      public void TestConnection()
      {
        if (string.IsNullOrEmpty(m_ConnectString) || string.IsNullOrEmpty(m_DatabaseName)) return;

        try
        {
          var db = GetDatabase();
          db.Ping();
        }
        catch (Exception error)
        {
          throw new MongoDbDataAccessException(StringConsts.CONNECTION_TEST_FAILED_ERROR.Args(error.ToMessageWithType()), error);
        }
      }

    #endregion

    #region IConfigurable Members

      public virtual void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);
      }

    #endregion

    #region Protected

      /// <summary>
      /// Gets appropriate database. It does not need to be disposed
      /// </summary>
      protected virtual Database GetDatabase()
      {
        //cstring may either have server cs, or contain laconic config  mongo{server='' database=''}
        var cstring = m_ConnectString;
        var dbn = m_DatabaseName;

        //Try to override from the context
        var ctx = CRUDOperationCallContext.Current;
        if (ctx!=null)
        {
          if (ctx.ConnectString.IsNotNullOrWhiteSpace()) cstring = ctx.ConnectString;
          if (ctx.DatabaseName.IsNotNullOrWhiteSpace()) dbn = ctx.DatabaseName;
        }

        //2. try to parse as laconic
        var lactxt = cstring;
        if (lactxt!=null)
        {
          lactxt = lactxt.Trim();
          if (lactxt.StartsWith(MongoClient.CONFIG_CS_ROOT_SECTION + "{"))//quick reject filter
          {
            var root = lactxt.AsLaconicConfig();
            if (root!=null)
            {
              cstring = root.AttrByName(MongoClient.CONFIG_CS_SERVER_ATTR).Value;
              if (dbn.IsNullOrWhiteSpace())
                dbn = root.AttrByName(MongoClient.CONFIG_CS_DB_ATTR).Value;
            }
          }
        }

        var client = App.GetDefaultMongoClient();
        var server = client[ new Glue.Node(cstring)];
        var database = server[dbn];
        return database;
      }


    #endregion
  }
}
