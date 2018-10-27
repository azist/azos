/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
