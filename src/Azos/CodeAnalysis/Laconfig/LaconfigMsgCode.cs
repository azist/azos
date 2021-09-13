/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Message codes for Laconfig code processors
  /// </summary>
  public enum LaconfigMsgCode
  {
    INFOS = 0,

    WARNING = 100,

    ERRORS = 1000,
        eUnterminatedString,
        ePrematureEOF,
        eUnterminatedComment,
        eInvalidStringEscape,
        eSectionNameExpected,
        eSectionOpenBraceExpected,
        eSectionOrAttributeNameExpected,
        eSectionOrAttributeValueExpected,
        eContentPastRootSection,
        eSyntaxError
  }
}
