/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Security.Cryptography;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;

namespace Azos.Security
{
  static partial class TheSafe
  {
    public const string ALGORITHM_NAME_NOP = "nop";
    public const string ALGORITHM_NAME_DEFAULT = "default";

    static TheSafe()
    {
      s_CryptoRnd = new RNGCryptoServiceProvider();
      s_Algorithms = new Registry<Algorithm>();
      s_Algorithms.Register(NopAlgorithm.Instance);

      //todo REad all algorithms from IConfigSectionNode
      // var dalg = new DefaultAlgorithm();
      // s_Algorithms.Register(dalg);
    }

    private static readonly RNGCryptoServiceProvider s_CryptoRnd;
    private static readonly Registry<Algorithm> s_Algorithms;

    internal static byte[] GenerateRandomBytes(int count)
    {
      (count > 0).IsTrue("Count > 0");
      var rnd = new byte[count];
      s_CryptoRnd.GetBytes(rnd);
      return rnd;
    }

    /// <summary> Uniform abstraction for safe algorithms which cipher and decipher values</summary>
    public abstract class Algorithm : INamed
    {
      public const string CONFIG_KEY_ATTR = "key";

      protected Algorithm(string name, IConfigSectionNode config)
      {
        m_Name = name.NonBlank(nameof(name));
        if (config != null && config.Exists)
        {
          ConfigAttribute.Apply(this, config);
        }
      }
      private readonly string m_Name;
      public string Name => m_Name;
      public abstract byte[] Cipher(byte[] originalValue);
      public abstract byte[] Decipher(byte[] protectedValue);

      /// <summary>
      /// Populates key array of the specified length. No duplicate keys are allowed
      /// </summary>
      protected byte[][] BuildKeysFromConfig(IConfigSectionNode config, string sectionName, int len)
      {
        var result = config.NonEmpty(nameof(config))
                           .Children
                           .Where(c => c.IsSameName(sectionName) && c.Of(CONFIG_KEY_ATTR).VerbatimValue.IsNotNullOrWhiteSpace())
                           .Select(c => BuildKeyFromConfig(c, len))
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

      protected byte[] BuildKeyFromConfig(IConfigSectionNode keyNode, int len)
      {
        var result = keyNode.NonEmpty(nameof(keyNode))
                            .AttrByName(CONFIG_KEY_ATTR)
                            .ValueAsByteArray(null, verbatim: true) //respects hex or base64: format
                            ??
                            throw new SecurityException("{0} config section `{1}` does not contain a valid key byte array".Args(GetType().Name, keyNode.RootPath));

        if (result.Length != len) throw new SecurityException("{0} config section `{1}` keys must be of {2} bytes in length".Args(GetType().Name, keyNode.RootPath, len));

        return result;
      }
    }

    /// <summary>
    /// Algorithm which does nothing
    /// </summary>
    public sealed class NopAlgorithm : Algorithm
    {
      private static readonly NopAlgorithm s_Instance = new NopAlgorithm();
      public static NopAlgorithm Instance => s_Instance;
      private NopAlgorithm() : base(ALGORITHM_NAME_NOP, null){ }
      public override byte[] Cipher(byte[] originalValue) => originalValue;
      public override byte[] Decipher(byte[] protectedValue) => protectedValue;
    }
  }
}
