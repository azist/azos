
using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Data.Access
{
  /// <summary>
  /// Represents data store that does not do anything
  /// </summary>
  public sealed class NOPDataStore : ApplicationComponent, IDataStoreImplementation
  {
    private static NOPDataStore s_Instance = new NOPDataStore();


    private NOPDataStore(): base(){}

    /// <summary>
    /// Returns a singlelton instance of the data store that does not do anything
    /// </summary>
    public static NOPDataStore Instance
    {
      get { return s_Instance; }
    }

    public string TargetName{ get{ return TargetedAttribute.ANY_TARGET;}}


         public bool InstrumentationEnabled{ get{return false;} set{}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get{ return null;}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups){ return null;}
         public bool ExternalGetParameter(string name, out object value, params string[] groups)
         {
           value = null;
           return false;
         }
         public bool ExternalSetParameter(string name, object value, params string[] groups)
         {
           return false;
         }

    public StoreLogLevel LogLevel { get; set;}

    public void TestConnection()
    {

    }

    public void Configure(IConfigSectionNode node)
    {
    }
  }
}
