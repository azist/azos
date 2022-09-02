/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.IO.Archiving;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Appends commands into package archives
  /// </summary>
  [ContentTypeSupport(PackageCommandArchiveAppender.CONTENT_TYPE_PACKAGING)]
  public sealed class PackageCommandArchiveAppender : ArchiveBixAppender<Command>
  {
    public const int MIN_PAGE_SIZE = Package.MIN_CHUNK_FILE_SIZE_BYTES * 10;
    public const int MAX_PAGE_SIZE = Package.MAX_CHUNK_FILE_SIZE_BYTES * 10;
    public const int DEFAULT_PAGE_SIZE = Package.DEFAULT_CHUNK_FILE_SIZE_BYTES * 10;
    public const string CONTENT_TYPE_PACKAGING = "bix/scripting-package";

    public PackageCommandArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<Command, Bookmark> onPageCommit = null)
           : base(volume, time, app, host, onPageCommit){ }

    protected override void DoSerializeBix(BixWriter wri, Command command)
    {
      if (command == null)
      {
        wri.Write(false); //NULL
        return;
      }
      else
      {
        wri.Write(true); // NON-NULL

        //get command type decoration
        var attr = Apps.GuidTypeAttribute.GetGuidTypeAttribute<Command, PackageCommandAttribute>(command.GetType());

        //write type identity of the command, so it can be polymorphically read back
        wri.Write(attr.TypeGuid);

        command.Serialize(wri);
      }
    }
  }
}
