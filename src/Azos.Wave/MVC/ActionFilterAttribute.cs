/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using Azos.Conf;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// General ancestor for MVC Action Filters - get invoked before and after actions
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  [CustomMetadata(@"category='action-filter'")]
  public abstract class ActionFilterAttribute : Attribute, IInstanceCustomMetadataProvider
  {

    /// <summary>
    /// Dictates the call order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Override to add logic/filtering right before the invocation of action method.
    /// Return TRUE to indicate that request has already been handled and no need to call method body and AfterActionInvocation in which case
    ///  return result via 'out result' parameter
    /// </summary>
    protected internal abstract bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);

    /// <summary>
    /// Override to add logic/filtering right after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal abstract bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);

    /// <summary>
    /// Override to add logic/filtering finally after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal abstract void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result);

    public virtual bool ShouldProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot)
      => false;

    public virtual ConfigSectionNode ProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      => dataRoot;
  }

}
