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

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Represents a package read from command archive on disk/memory stream etc...
  /// This class must be deterministically disposed
  /// </summary>
  public sealed class Package : DisposableObject
  {
    public Package FromFile(IApplication app, string fileName)
     => FromFile(app.NonNull(nameof(app)).SecurityManager.Cryptography, fileName);

    public Package FromFile(Security.ICryptoManager crypto, string fileName)
    {
      crypto.NonNull(nameof(crypto));
      fileName.NonBlank(nameof(fileName));
      var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
      var volume = new DefaultVolume(crypto, fileStream, ownsStream: true);
      var result = new Package(volume);
      return result;
    }

    public Package FromStream(IApplication app, Stream stream, bool ownsStream = true)
      => FromStream(app.NonNull(nameof(app)).SecurityManager.Cryptography, stream, ownsStream);

    public Package FromStream(Security.ICryptoManager crypto, Stream stream, bool ownsStream = true)
    {
      crypto.NonNull(nameof(crypto));
      stream.NonNull(nameof(stream));
      var volume = new DefaultVolume(crypto, stream, ownsStream);
      var result = new Package(volume);
      return result;
    }

    public Package(IVolume volume, Apps.IGuidTypeResolver typeResolver = null)
    {
      m_Volume = volume.NonNull(nameof(volume));

      if (typeResolver == null)
      {
        typeResolver = new Apps.GuidTypeResolver<Command, PackageCommandAttribute>(Installer.BUILT_IN_COMMANDS);
      }

      m_Reader = new PackageCommandArchiveReader(volume, typeResolver);
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Volume);
    }

    private IVolume m_Volume;
    private PackageCommandArchiveReader m_Reader;

    /// <summary>
    /// Accesses volume information, e.g. `Volume.Metadata`
    /// </summary>
    public IVolume Volume => m_Volume.NonDisposed("volume");

    /// <summary>
    /// Returns all commands contained by the package
    /// </summary>
    public IEnumerable<Command> Commands => m_Reader.NonDisposed("reader").All;
  }
}
