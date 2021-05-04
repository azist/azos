/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Transpilation
{
  /// <summary>
  /// Describes general transpiler interface
  /// </summary>
  public interface ITranspiler : ICodeProcessor
  {
    /// <summary>
    /// Lists source parser that supply parse tree for transpilation
    /// </summary>
    IParser SourceParser { get; }

    /// <summary>
    /// Indicates whether Transpile() already happened
    /// </summary>
    bool HasTranspiled { get; }

    /// <summary>
    /// Performs transpilation and sets HasTranspiled to true if it has not been performed yet
    /// </summary>
    void Transpile();

  }
}
