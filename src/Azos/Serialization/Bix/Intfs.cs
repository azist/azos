/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Serialization.Bix
{
  public interface IBixWritable
  {
    /// <summary>
    /// Writes custom representation of this implementing instance.
    /// You must include the TypeCode yourself.
    /// Return true if custom writing succeeded, or false to resort to default writing
    /// </summary>
    bool WriteToBix(BixWriter writer, BixContext ctx);
  }

  public interface IBixReadable
  {
    bool ReadFromBix(BixReader reader, BixContext ctx);
  }
}
