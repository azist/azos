/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.Kdb
{
  public static class KdbConstraints
  {
    public const int MAX_TABLE_NAME_LEN = 32;
    public const int MAX_KEY_LEN = 255;

    public static void CheckKey(byte[] key, string opName)
    {
      if (key == null || key.Length == 0) throw new KdbException(ServerStringConsts.KDB_KEY_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = key.Length;
      if (len > MAX_KEY_LEN) throw new KdbException(ServerStringConsts.KDB_KEY_MAX_LEN_ERROR.Args(opName, len, MAX_KEY_LEN));
    }

    public static void CheckTableName(string table, string opName)
    {
      if (table.IsNullOrWhiteSpace()) throw new KdbException(ServerStringConsts.KDB_TABLE_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = table.Length;
      if (len > MAX_TABLE_NAME_LEN) throw new KdbException(ServerStringConsts.KDB_TABLE_MAX_LEN_ERROR.Args(opName, len, MAX_TABLE_NAME_LEN));
      for(var i = 0; i < len; i++)
      {
        var c = table[i];
        if ((c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (i > 0 && c >= '0' && c <= '9') ||
            c == '_') continue;

        throw new KdbException(ServerStringConsts.KDB_TABLE_CHARACTER_ERROR.Args(opName, table));
      }
    }

  }
}
