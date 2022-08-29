/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Scripting.Dsl;
using Azos.IO.Archiving;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Keeps the state of package building: maintains Volume, Appender and state.
  /// This instance is created by <see cref="CreatePackage"/> Dsl step and
  /// it gets owned by current call stack frame which is freed upon scope exit
  /// </summary>
  public sealed class PackageBuilder : DisposableObject
  {
    /// <summary>
    /// Tries to find a PackageBuilder of a specified name on a call stack of frames.
    /// Returns null if not found
    /// </summary>
    public static PackageBuilder TryGet(string name)
     => StepRunner.Frame.Current?.All.FirstOrDefault(o => (o is PackageBuilder pb) && pb.Name.EqualsOrdIgnoreCase(name.NonBlank(nameof(name)))) as PackageBuilder;

    /// <summary>
    /// Tries to find a PackageBuilder by name on a call stack of frames.
    /// Throws if such PackageBuildere is not found and dependency could not be satisfied
    /// </summary>
    public static PackageBuilder Get(string name)
     => TryGet(name).NonNull("Satisfied dependency on `PackageBuilder('{0}')` loaded by `{1}` step".Args(
                             name,
                             nameof(CreatePackage)));

    internal PackageBuilder(string name, DefaultVolume volume, PackageCommandArchiveAppender appender)
    {
      m_Name = name.NonBlank(nameof(name));
      m_Volume = volume.NonDisposed();
      m_Appender = appender.NonDisposed();
      StepRunner.Frame.Current.Owned.Add(this);
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Appender);
      DisposeAndNull(ref m_Volume);
    }

    private string m_Name;
    private DefaultVolume m_Volume;
    private PackageCommandArchiveAppender m_Appender;


    public string Name => m_Name;
    public DefaultVolume Volume => m_Volume;
    public PackageCommandArchiveAppender Appender => m_Appender;
  }




  /// <summary>
  /// Create an installation package
  /// </summary>
  public sealed class CreatePackage : Step
  {
    public CreatePackage(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

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
      //You need to specify label/name
      Label.NonBlank(nameof(Label));

      var existing = PackageBuilder.TryGet(Label);
      (existing == null).IsTrue("Unique open package label");


      var meta = VolumeMetadataBuilder.Make(Label, PackageCommandArchiveAppender.CONTENT_TYPE_PACKAGING, "package")
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

      //gets registered on call stack
      var builder = new PackageBuilder(Label, volume, appender);

      //sets the default builder
      Runner.SetResult(builder);

      return Task.FromResult<string>(null);
    }
  }//CreatePackage

}
