/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps.Injection;
using Azos.Conf;


namespace Azos.Sky.Metabase
{
  /// <summary>
  /// Provides shortcut access to mounted metabank file system
  /// </summary>
  public sealed class MetabankFileConfigNodeProvider : IConfigNodeProvider
  {
    [InjectSingleton] Metabank m_Metabank;

    [Config]
    public string File { get; set; }

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);

    public ConfigSectionNode ProvideConfigNode(object context = null)
    {
      if (File.IsNullOrWhiteSpace()) return null;

      return m_Metabank.NonNull(nameof(m_Metabank))
                       .GetConfigFromExistingFile(File)
                       .Root;
    }
  }
}
