/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.IO.Archiving;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Writes log messages into log archive
  /// </summary>
  public class ArchiveSink : VfsSink
  {
    protected ArchiveSink(ISinkOwner owner) : base(owner) { }
    protected ArchiveSink(ISinkOwner owner, string name, int order) : base(owner, name, order) { }

    private DefaultVolume m_Volume;
    private LogMessageArchiveAppender m_Appender;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public int ArchiveVersionMajor { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public int ArchiveVersionMinor { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public Atom ArchiveChannel { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public string ArchiveCompressionScheme { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public string ArchiveEncryptionScheme { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public string ArchiveDescription { get; set; }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
    public IConfigSectionNode ArchiveApplicationSection { get; set; }



    protected sealed override void DoOpenStream()
    {
      var meta = VolumeMetadataBuilder.Make(Name)
                                      .SetVersion(ArchiveVersionMajor, ArchiveVersionMinor)
                                      .SetChannel(ArchiveChannel)
                                      .SetCompressionScheme(ArchiveCompressionScheme)
                                      .SetEncryptionScheme(ArchiveEncryptionScheme)
                                      .SetDescription(ArchiveDescription)
                                      .SetApplicationSection(anode =>
                                       {
                                         var app = ArchiveApplicationSection;
                                         if (app != null)
                                         {
                                           anode.MergeAttributes(app);
                                           anode.MergeSections(app);
                                         }
                                       });
      DoOpenArchive(meta);
    }

    protected virtual void DoOpenArchive(VolumeMetadataBuilder meta)
    {
      m_Volume = new DefaultVolume(App.SecurityManager.Cryptography, meta, m_Stream, ownsStream: false);
      m_Appender = new LogMessageArchiveAppender(m_Volume, App.TimeSource, App.AppId, Platform.Computer.HostName);
    }

    protected sealed override void DoCloseStream()
    {
      DoCloseArchive();
      DisposeAndNull(ref m_Volume);
    }

    protected virtual void DoCloseArchive()
    {
      DisposeAndNull(ref m_Appender);
      DisposeAndNull(ref m_Volume);
    }

    protected override void DoWriteMessage(Message msg)
    {
      m_Appender.Append(msg);
    }
  }
}
