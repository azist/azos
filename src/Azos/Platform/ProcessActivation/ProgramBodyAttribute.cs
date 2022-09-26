/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Platform.ProcessActivation
{
  /// <summary>
  /// Decorates program body classes with data necessary to delegate process activation.
  /// You specify name/s to use for process activation. If process has more than one name,
  /// separate strings with comas
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class ProgramBodyAttribute : Attribute
  {
    public ProgramBodyAttribute(string names)
    {
      m_Names = names.NonBlank(nameof(names))
                     .Split(',',';')
                     .Where(s => s.IsNotNullOrWhiteSpace())
                     .Select(s => s.ToLowerInvariant().Trim())
                     .Distinct()
                     .ToArray();
      (m_Names.Length > 0).IsTrue("Defined names");
    }

    private string[] m_Names;

    public IEnumerable<string> Names => m_Names;

    /// <summary>
    /// Optional process description
    /// </summary>
    public string Description { get; set; }
  }
}
