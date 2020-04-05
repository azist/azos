using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Defines how generated files should be organized
  /// </summary>
  public enum GeneratedCodeOrganization
  {
    FilePerNamespace = 0,
    FilePerType,
    AllInOne
  }
}
