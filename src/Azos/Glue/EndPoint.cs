/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;

namespace Azos.Glue
{

  /// <summary>
  /// Abstraction of server and client endpoints. And endpoint is a logically-connected entity per: ABC rule - Address/Binding/Contract(s)
  /// </summary>
  public abstract class EndPoint : DisposableObject
  {
    protected EndPoint(IGlue glue) //used by conf
    {
        m_Glue = glue as IGlueImplementation;
        m_Glue.NonNull(nameof(glue));
    }

    protected EndPoint(IGlue glue, Node node, Binding binding)
    {
      m_Glue = glue as IGlueImplementation;
      m_Glue.NonNull(nameof(glue));

      m_Node = node;
      m_Binding = binding ?? m_Glue.GetNodeBinding(node);

      if (m_Binding.Glue != m_Glue)
        throw new GlueException(StringConsts.GLUE_BINDING_GLUE_MISMATCH_ERROR.Args(m_Binding, m_Glue));
    }

    protected readonly IGlueImplementation m_Glue;

    protected Node m_Node;       //[A]ddress
    protected Binding m_Binding; //[B]inding

    /// <summary>
    /// References glue that this endpoint works under
    /// </summary>
    public IGlue Glue => m_Glue;

    /// <summary>
    /// Returns a node of this endpoint. "A" component of the "ABC" rule
    /// </summary>
    public Node Node => m_Node;

    /// <summary>
    /// Returns a binding of this endpoint. "B" component of the "ABC" rule
    /// </summary>
    public Binding Binding => m_Binding;
  }
}
