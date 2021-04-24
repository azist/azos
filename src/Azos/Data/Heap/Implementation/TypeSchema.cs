/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.Conf;

namespace Azos.Data.Heap.Implementation
{
  /// <summary>
  /// Type registry which mounts and checks types: HeapObjects and HeapQueries
  /// </summary>
  internal class TypeSchema
  {
    public const string CONFIG_SCHEMA_SECTION = "schema";

    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_FILE_ATTR = "file";

    public TypeSchema(IConfigSectionNode cfg)
    {
      if (!cfg.NonEmpty(nameof(cfg)).IsSameName(CONFIG_SCHEMA_SECTION))
        cfg = cfg[CONFIG_SCHEMA_SECTION];

      cfg.NonEmpty(nameof(CONFIG_SCHEMA_SECTION));

      var lstObjects = new List<Type>();
      var lstQueries = new List<Type>();

      try
      {
        foreach(var node in cfg.ChildrenNamed(CONFIG_ASSEMBLY_SECTION))
        {
          var fn = node.ValOf(CONFIG_FILE_ATTR).NonBlank("{0}/${1}".Args(node.RootPath, CONFIG_FILE_ATTR));
          var asm = Assembly.LoadFrom(fn);
          load(asm);
        }
      }
      catch(Exception error)
      {
        throw new ConfigException("Bad config of {0}: {1}".Args(nameof(TypeSchema), error.ToMessageWithType()), error);
      }

      m_ObjectTypes = lstObjects.ToArray();
      m_QueryTypes = lstQueries.ToArray();
    }

    private void load(Assembly asm)
    {

    }

    private Type[] m_ObjectTypes;
    private Type[] m_QueryTypes;

    public IEnumerable<Type> ObjectTypes => m_ObjectTypes;
    public IEnumerable<Type> QueryTypes => m_QueryTypes;

  }
}
