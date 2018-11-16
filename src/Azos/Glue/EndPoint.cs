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
           if (glue == null)
                glue = ExecutionContext.Application.Glue;

            m_Glue = (IGlueImplementation)glue;
        }

        protected EndPoint(IGlue glue, Node node, Binding binding)
        {
            if (glue == null)
                glue = ExecutionContext.Application.Glue;

            m_Glue = (IGlueImplementation)glue;

            m_Node = node;
            m_Binding = binding ?? m_Glue.GetNodeBinding(node);
        }

        protected IGlueImplementation m_Glue;



        protected Node m_Node;    //[A]ddress
        protected Binding m_Binding; //[B]inding




        /// <summary>
        /// References glue that this endpoint works under
        /// </summary>
        public IGlue Glue { get { return m_Glue; } }


        /// <summary>
        /// Returns a node of this endpoint. "A" component of the "ABC" rule
        /// </summary>
        public Node Node { get { return m_Node; } }

        /// <summary>
        /// Returns a binding of this endpoint. "B" component of the "ABC" rule
        /// </summary>
        public Binding Binding { get { return m_Binding; } }


    }



}
