/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

namespace Azos.Conf
{
#pragma warning disable 0169

  /// <summary>
  /// Represents a piece of logic that can be applied to different entities declaratively as attribute or through configuration.
  /// Behaviors are a form of inversion-of-control that allows to configure entities by code which itself can be injected by name from configuration.
  /// Behaviors are a form of aspect-oriented programming as they allow to proclaim certain "behavior" that knows how to implement itself on various
  ///  application components (i.e. some behavior may inject Glue message inspector or log destination).
  /// Behaviors can be used to enforce policies by performing pre-run checks  and throw exceptions if certain required providers are not injected/configured
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
  public abstract class Behavior : Attribute, IConfigurable
  {
    #region CONSTS

    public const string CONFIG_BEHAVIORS_SECTION = "behaviors";
    public const string CONFIG_BEHAVIOR_SECTION = "behavior";
    public const string CONFIG_CASCADE_ATTR = "cascade";

    #endregion


    #region Static

    /// <summary>
    /// Applies behaviors to instance as configured from config section node
    /// </summary>
    public static void ApplyConfiguredBehaviors(object target, IConfigSectionNode node)
    {
      if (target == null || node == null) return;
      if (typeof(Behavior).IsAssignableFrom(target.GetType())) return;

      string descr = string.Empty;
      try
      {
        var firstLevel = true;

        while (node.Exists)
        {
          var bnodes = node[CONFIG_BEHAVIORS_SECTION]
                        .Children
                        .Where(c => c.IsSameName(CONFIG_BEHAVIOR_SECTION) && (firstLevel || c.AttrByName(CONFIG_CASCADE_ATTR).ValueAsBool(false)))
                        .OrderBy(c => c.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt());

          foreach (var bnode in bnodes)
          {
            descr = " config path: '{0}', type: '{1}'".Args(bnode.RootPath,
                                     bnode.AttrByName(FactoryUtils.CONFIG_TYPE_ATTR)
                                          .ValueAsString(CoreConsts.NULL_STRING));

            var behavior = FactoryUtils.MakeAndConfigure<Behavior>(bnode);
            behavior.Apply(target);
          }

          node = node.Parent;
          firstLevel = false;
        }
      }
      catch (Exception error)
      {
        throw new BehaviorApplyException(StringConsts.CONFIG_BEHAVIOR_APPLY_ERROR.Args(descr, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Applies behaviors as declared using attributes on target instance's type
    /// </summary>
    public static void ApplyBehaviorAttributes(object target)
    {
      var tp = target.GetType();
      while (tp != null)
      {
        var attrs = tp.GetCustomAttributes(typeof(Behavior), false).Cast<Behavior>().OrderBy(b => b.m_Order);
        foreach (var behavior in attrs)
          behavior.Apply(target);
        tp = tp.BaseType;
      }
    }

    #endregion

    /// <summary>
    /// Creates a new Behavior
    /// </summary>
    protected Behavior() { }

    /// <summary>
    /// Creates a new Behavior with a specified order value
    /// </summary>
    protected Behavior(int order)
    {
      m_Order = order;
    }

    private int m_Order;

    [Config("$" + CONFIG_CASCADE_ATTR)]
    private bool m_Cascade;

    /// <summary>
    /// Returns application order
    /// </summary>
    public int Order => m_Order;

    /// <summary>
    /// Returns true when this instance was configured to cascade down the configuration tree - applied to child items
    /// </summary>
    public bool Cascade => m_Cascade;

    /// <summary>
    /// Override to apply particular behavior to the target
    /// </summary>
    public abstract void Apply(object target);

    /// <summary>
    /// Configures this behavior using the config section node
    /// </summary>
    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

  }
}