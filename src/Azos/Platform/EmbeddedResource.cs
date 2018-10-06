/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Azos.Platform
{

  /// <summary>
  ///  Fetches resources such as script statement text by scriptName from assembly resource stream.
  ///  Mostly used for SQL and JavaScript but maybe used for any text retrieval.
  ///  This class is 100% safe for multi-threading operations.
  ///  Script texts are cached in ram for faster subsequent access.
  /// </summary>
  public static class EmbeddedResource
  {
    private static object s_CacheLock = new object();
    private static Dictionary<string, string> s_Cache = new Dictionary<string, string>();

    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> string sql = typeof(SomeType).GetText("SQL.User.Insert.sql");</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.SQL.User.Insert.sql"
    /// </summary>
    public static string GetText(this Type scopingType, string scriptName)
    {
      string result = null;

      var entryName = "text://"+scopingType.Namespace + "::" + scriptName;


      if (s_Cache.TryGetValue(entryName, out result)) return result;

        lock(s_CacheLock)
        {
          var dict = new Dictionary<string,string>(s_Cache);
          if (dict.TryGetValue(entryName, out result)) return result;

          try
          {
            using (Stream stream = scopingType.Assembly.GetManifestResourceStream(scopingType, scriptName))
             using (TextReader reader = new StreamReader(stream))
               result = reader.ReadToEnd();
          }
          catch
          {
             //this will throw when resource is not found - this is VERY slow
          }

          dict[entryName] = result;
          s_Cache = dict;
        }

      return result;
    }


    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> using (var stream = typeof(SomeType).GetBinary("My.Picture.gif")){...}</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.My.Picture.gif"
    /// </summary>
    public static Stream GetBinaryStream(this Type scopingType, string resourceName)
    {
       return scopingType.Assembly.GetManifestResourceStream(scopingType, resourceName);
    }

    /// <summary>
    /// Pass a type and resource path rooted at type's namespace, for example
    ///  given <code> using (var stream = typeof(SomeType).GetBinary("My.Picture.gif")){...}</code>
    ///  If "SomeType" is declared in "TestApp.Types", then statement's resource will have to be embedded under resource named:
    ///   "TestApp.Types.My.Picture.gif"
    /// </summary>
    public static byte[] GetBinaryContent(this Type scopingType, string resourceName)
    {
      using(var ms = new MemoryStream())
        using(var str = scopingType.GetBinaryStream(resourceName))
        {
          str.CopyTo(ms);
          return ms.ToArray();
        }
    }


  }
}
