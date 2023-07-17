/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Security
{
  static partial class TheSafe
  {
    public const string ENV_VAR_SKY_SAFE = "SKY_SAFE";
    public const string ENV_VAR_SKY_SAFE_PWD = "SKY_SAFE_PWD";
    public const string FILE_NAME_SAFE_1 = ".skysafe.safe";
    public const string FILE_NAME_SAFE_2 = ".skysafe";
    public const string ALGORITHM_NAME_NOP = "nop";

    public const string CONFIG_ALGORITHM_SECTION = "algorithm";

    static TheSafe()
    {
      s_CryptoRnd = new RNGCryptoServiceProvider();
      s_Algorithms = new Registry<Algorithm>();
      s_Algorithms.Register(NopAlgorithm.Instance);
    }

    private static readonly RNGCryptoServiceProvider s_CryptoRnd;
    private static readonly Registry<Algorithm> s_Algorithms;

    private static Algorithm getAlgorithmOrDefault(string algorithmName)
    {
      if (algorithmName.IsNullOrWhiteSpace()) return s_Algorithms.FirstOrDefault(one => one.Default);
      return s_Algorithms[algorithmName];
    }

    /// <summary>
    /// Initializes The Safe with the specified config vector.
    /// If passed config is null, then the system tries to locate and read `.skysafe`/`.skysafe.safe` files
    /// looking in pointed to directory by environment variables in succession: `SKY_SAFE`,`SKY_HOME`, then
    /// starting at app executable entry point and continuing its search through parent directories.
    /// Pass `keep=true` to add algorithms to existing safe configuration
    /// </summary>
    /// <param name="cfg">Configuration to use as init vector. If null, then the system uses <see cref="GetDefaultConfiguration"/></param>
    /// <param name="onlyWhenHasNotInitBefore">
    /// False by default, when true will ONLY do anything if this method has not been called before this call,
    /// or it has not added any algorithms
    /// </param>
    /// <param name="keep">When true will add extra (if any) algorithms in the supplied config vector to the safe algo registry</param>
    public static void Init(IConfigSectionNode cfg = null, bool onlyWhenHasNotInitBefore = false,  bool keep = false)
    {
      if (onlyWhenHasNotInitBefore && s_Algorithms.Count > 1/*nop algorithm is always present*/) return;

      if (cfg == null || !cfg.Exists)
      {
        cfg = GetDefaultConfiguration();
      }

      if (!keep)
      {
        s_Algorithms.Clear();
        s_Algorithms.Register(NopAlgorithm.Instance);
      }

      foreach (var nAlgo in cfg.Children.Where(c => c.IsSameName(CONFIG_ALGORITHM_SECTION)))
      {
        var algo = FactoryUtils.Make<Algorithm>(nAlgo, args: new[] { nAlgo });
        if (!keep && !s_Algorithms.Register(algo))
        {
          throw new SecurityException("Algorithm `{0}` is already registered".Args(algo.Name));
        }
      }//foreach
    }

    /// <summary>
    /// Returns default configuration read from `.skysafe` or `.skysafe.safe` file.
    /// The file search starts at `SKY_SAFE` env var if it is set, otherwise the env var
    /// `SKY_HOME` is probed, otherwise the search is performed starting at the app
    /// entry point executable root and continuing file search in its parent directories
    /// </summary>
    public static IConfigSectionNode GetDefaultConfiguration()
    {
      IConfigSectionNode loadFile(string fn)
      {
        var raw = File.ReadAllBytes(fn);
        if (raw.Length == 0) return null;
        var isSecure = HasPreamble(raw, 0);

        if (isSecure)
        {
          var pwd = Environment.GetEnvironmentVariable(ENV_VAR_SKY_SAFE_PWD);

          if (pwd == null) throw new SecurityException("Environment variable `{0}` must be set to access safe config file".Args(ENV_VAR_SKY_SAFE_PWD));

          var laconf = UnprotectString(raw, pwd);
          if (laconf == null) throw new SecurityException("Failure reading safe config file. Revise env variable `{0}`".Args(ENV_VAR_SKY_SAFE_PWD));
          return LaconicConfiguration.CreateFromString(laconf).Root;
        }
        else
        {
          return new LaconicConfiguration(fn).Root;
        }
      }

      IConfigSectionNode load(string dirPath)
      {
        var fn = Path.Combine(dirPath, FILE_NAME_SAFE_1);
        if (File.Exists(fn)) return loadFile(fn);

        fn = Path.Combine(dirPath, FILE_NAME_SAFE_2);
        if (File.Exists(fn)) return loadFile(fn);

        return null;
      }

      try
      {
        //1. Try SKY_SAFE path
        var varSkySafe = Environment.GetEnvironmentVariable(ENV_VAR_SKY_SAFE);
        if (varSkySafe.IsNotNullOrWhiteSpace() && Directory.Exists(varSkySafe))
        {
          var result = load(varSkySafe);
          if (result != null) return result;
        }

        //2. Try SKY_HOME path
        var varSkyHome = Environment.GetEnvironmentVariable(CoreConsts.SKY_HOME);
        if (varSkyHome.IsNotNullOrWhiteSpace() && Directory.Exists(varSkyHome))
        {
          var result = load(varSkyHome);
          if (result != null) return result;
        }

        //3. Look in app diretory
        var path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        while(true)
        {
          var result = load(path);
          if (result != null) return result;

          path = Path.GetDirectoryName(path);
          if (path.IsNullOrWhiteSpace() || path == Path.GetPathRoot(path)) break;
        }

        return Configuration.NewEmptyRoot();
      }
      catch(Exception error)
      {
        throw new SecurityException($"Error in TheSafe.{nameof(GetDefaultConfiguration)}(): {error.ToMessageWithType()}", error);
      }
    }


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

      [Config]
      private bool m_Default;

      public string Name => m_Name;
      public bool Default => m_Default;

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
