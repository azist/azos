/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Collections;

namespace Azos.Security
{
  static partial class TheSafe
  {
    public const string ALGORITHM_NAME_NOP = "nop";
    public const string ALGORITHM_NAME_DEFAULT = "default";

    static TheSafe()
    {
      s_Algorithms = new Registry<Algorithm>();
      s_Algorithms.Register(NopAlgorithm.Instance);
     // var dalg = new DefaultAlgorithm();
     // s_Algorithms.Register(dalg);
    }

    private static Registry<Algorithm> s_Algorithms;

    /// <summary> Uniform abstraction for safe algorithms which cipher and decipher values</summary>
    public abstract class Algorithm : INamed
    {
      protected Algorithm(string name) => m_Name = name.NonBlank(nameof(name));
      private readonly string m_Name;
      public string Name => m_Name;
      public abstract byte[] Cipher(byte[] value);
      public abstract byte[] Decipher(byte[] value);
    }

    /// <summary>
    /// Algorithm which does nothing
    /// </summary>
    public sealed class NopAlgorithm : Algorithm
    {
      private static readonly NopAlgorithm s_Instance = new NopAlgorithm();
      public static NopAlgorithm Instance => s_Instance;
      private NopAlgorithm() : base(ALGORITHM_NAME_NOP){ }
      public override byte[] Cipher(byte[] value) => value;
      public override byte[] Decipher(byte[] value) => value;
    }
  }
}
