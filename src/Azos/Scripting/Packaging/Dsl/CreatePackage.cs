/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Scripting.Dsl;
using Azos.IO.Archiving;
using System.IO;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Keeps the state of package installation
  /// </summary>
  public sealed class PackageBuilder : DisposableObject
  {
    internal PackageBuilder(DefaultVolume volume, PackageCommandArchiveAppender appender)
    {
      m_Volume = volume.NonDisposed();
      m_Appender = appender.NonDisposed();
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Appender);
      DisposeAndNull(ref m_Volume);
    }

    private DefaultVolume m_Volume;
    private PackageCommandArchiveAppender m_Appender;

    public DefaultVolume Volume => m_Volume;
    public PackageCommandArchiveAppender Appender => m_Appender;
  }




  /// <summary>
  /// Create an installation package
  /// </summary>
  public sealed class CreatePackage : Step
  {
    public CreatePackage(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Global { get; set; }
    [Config] public string Local { get; set; }


    [Config] public string FilePath { get; set; }
    [Config] public bool DontOvewrite { get; set; }

    [Config] public int PageSizeBytes { get; set; }

    [Config] public string Label   { get; set; }
    [Config] public Atom   Channel { get; set; }

    [Config] public int VersionMajor { get; set; }
    [Config] public int VersionMinor { get; set; }
    [Config] public string Description { get; set; }

    [Config(Default = "gzip-max")]
    public string CompressionScheme { get; set; }

    [Config]
    public string EncryptionScheme { get; set; }

    [Config]
    public IConfigSectionNode AppSettings { get; set; }


    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      //You need to specify either global or local or both
      (Global.IsNotNullOrWhiteSpace() ||
       Local.IsNotNullOrWhiteSpace()).IsTrue("CreatePackage is assigned into Global or Local");

      var meta = VolumeMetadataBuilder.Make(Label.NonBlank(nameof(Label)), PackageCommandArchiveAppender.CONTENT_TYPE_PACKAGING, "package")
                                      .SetVersion(VersionMajor, VersionMinor)
                                      .SetChannel(Channel.HasRequiredValue(nameof(Channel)))
                                      .SetDescription(Description.Default(
                                        $"Package archive `{Label}` created on {Platform.Computer.HostName} at {App.TimeSource.UTCNow} by {Ambient.CurrentCallUser}"
                                       ))
                                      .SetCompressionScheme(CompressionScheme)
                                      .SetEncryptionScheme(EncryptionScheme)
                                      .SetApplicationSection(app =>
                                       {
                                         if (AppSettings != null && AppSettings.Exists)
                                         {
                                           app.MergeAttributes(AppSettings);
                                           app.MergeSections(AppSettings);
                                         }
                                       });

      var fs = new FileStream(FilePath.NonBlank(nameof(FilePath)),
                              DontOvewrite ? FileMode.CreateNew : FileMode.Create,
                              FileAccess.Write,
                              FileShare.None,
                              128 * 1024,
                              FileOptions.WriteThrough);

      var volume = new DefaultVolume(App.SecurityManager.Cryptography, meta, fs);

      volume.PageSizeBytes = PageSizeBytes <= 0 ?
                               PackageCommandArchiveAppender.DEFAULT_PAGE_SIZE :
                               PageSizeBytes.KeepBetween(PackageCommandArchiveAppender.MIN_PAGE_SIZE, PackageCommandArchiveAppender.MAX_PAGE_SIZE);

      var appender = new PackageCommandArchiveAppender(volume,
                                                       App.TimeSource,
                                                       App.AppId,
                                                       Platform.Computer.HostName);

      var builder = new PackageBuilder(volume, appender);


      if (Global.IsNotNullOrWhiteSpace())
      {
        Runner.GlobalState[Global] = builder;
      }

      if (Local.IsNotNullOrWhiteSpace())
      {
        state[Local] = builder;
      }

      return Task.FromResult<string>(null);
    }
  }//CreatePackage

}
