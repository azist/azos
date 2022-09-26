/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.IO.Archiving;
using Azos.Serialization.Bix;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Reads archives of Command items
  /// </summary>
  [ContentTypeSupport(PackageCommandArchiveAppender.CONTENT_TYPE_PACKAGING)]
  public sealed class PackageCommandArchiveReader : ArchiveBixReader<Command>
  {
    public PackageCommandArchiveReader(IVolume volume, IGuidTypeResolver resolver) : base(volume)
    {
      m_Resolver = resolver.NonNull(nameof(resolver));
    }

    private IGuidTypeResolver m_Resolver;

    public override Command MaterializeBix(BixReader reader)
    {
      if (!reader.ReadBool()) return null;//NULL

      //read type identity
      var typeGuid = reader.ReadGuid();

      //resolve type identity into type object (or throw)
      var tTarget = m_Resolver.Resolve(typeGuid).IsOfType<Command>($"Guid {typeGuid} -> Command");

      var result = (Command)Azos.Serialization.SerializationUtils.MakeNewObjectInstance(tTarget);
      result.Deserialize(reader);
      return result;
    }
  }
}
