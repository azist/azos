/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Provides base implementation for various ICryptoMessageAlgorithmImplementation
  /// </summary>
  public abstract class CryptoMessageAlgorithm : ApplicationComponent<ICryptoManager>, ICryptoMessageAlgorithmImplementation
  {
    public const string CONFIG_KEY_ATTR = "key";

    protected CryptoMessageAlgorithm(ICryptoManagerImplementation director, IConfigSectionNode config) : base(director)
    {
      ConfigAttribute.Apply(this, config.NonEmpty("{0}.ctor(config=null|!Exists)".Args(GetType().Name)));
    }
#pragma warning disable 0649
    [Config]
    private string m_Name;
#pragma warning restore 0649

    public string Name => m_Name;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public abstract CryptoMessageAlgorithmFlags Flags  { get; }

    [Config]
    public CryptoMessageAlgorithmAudience Audience { get; set; }

    [Config("$default|$is-default")]
    public bool IsDefault { get; set; }

    public abstract byte[] Protect(ArraySegment<byte> originalMessage);

    public abstract byte[] Unprotect(ArraySegment<byte> protectedMessage);


    protected byte[][] BuildKeysFromConfig(IConfigSectionNode config, string sectionName, int len)
    {
      var result = config.Children
                          .Where(c => c.IsSameName(sectionName) && c.ValOf(CONFIG_KEY_ATTR).IsNotNullOrWhiteSpace())
                          .Select(c => c.AttrByName(CONFIG_KEY_ATTR).ValueAsByteArray(null) ??
                                       throw new SecurityException("{0} config section `{1}` does not contain valid key byte array".Args(GetType().Name, c.RootPath)))
                          .ToArray();
      if (result.Length == 0) throw new SecurityException("{0} config section `{1}` must contain at least one key entry".Args(GetType().Name, sectionName));

      foreach (var a in result)
      {
        if (a.Length != len) throw new SecurityException("{0} config section `{1}` all keys must be of {2} bytes in length".Args(GetType().Name, sectionName, len));
        if (result.Any(a2 => a2 != a && a.MemBufferEquals(a2)))
          throw new SecurityException("{0} config section `{1}` contains duplicate keys".Args(GetType().Name, sectionName));
      }

      return result;
    }

  }
}
