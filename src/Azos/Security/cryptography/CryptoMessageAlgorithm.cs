/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Provides base implementation for various ICryptoMessageAlgorithmImplementation
  /// </summary>
  public abstract class CryptoMessageAlgorithm : ApplicationComponent, ICryptoMessageAlgorithmImplementation
  {
    protected CryptoMessageAlgorithm(ICryptoManagerImplementation director, IConfigSectionNode config) : base(director)
    {
      ConfigAttribute.Apply(this, config);
    }

    [Config]
    private string m_Name;

    public string Name => m_Name;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public abstract CryptoMessageAlgorithmFlags Flags  { get; }

    [Config]
    public CryptoMessageAlgorithmAudience Audience { get; set; }

    [Config]
    public bool IsDefault { get; set; }

    public abstract byte[] Protect(ArraySegment<byte> originalMessage);

    public abstract byte[] Unprotect(ArraySegment<byte> protectedMessage);
  }
}
