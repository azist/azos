using System;
using System.Collections.Generic;

using Azos.Data;

namespace Azos.Wave.CMS
{
  [Serializable]
  public class DirectoryResource : Resource
  {

     IEnumerable<string> GetConfigNames(ICacheParams caching = null)
     {
       return null;
     }

     IEnumerable<string> GetFileNames(ICacheParams caching = null)
     {
       return null;
     }

     /// <summary>
     /// Returns Config by name or null
     /// </summary>
     public ConfigResource GetConfig(string name, ICacheParams caching = null)
     {
       return null;
     }


     /// <summary>
     /// Returns Config by name or null
     /// </summary>
     public FileResource GetFile(string name, ICacheParams caching = null)
     {
       return null;
     }


  }
}
