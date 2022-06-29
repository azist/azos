/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Azos.Platform;

namespace Azos.Conf
{
  /// <summary>
  /// Decorates classes or structs that act as a context object for macro evaluation - passed as context parameter to MacroRunner.Run(...context) method
  /// </summary>
  [AttributeUsage(AttributeTargets.Class |
                  AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
  public class ConfigMacroContextAttribute : Attribute
  {
    public ConfigMacroContextAttribute() {  }
  }


  /// <summary>
  /// Decorates constructors that can be used to create object values out of config nodes.
  /// The system tries to infer if a TYPE has either a .ctor(IConfigSectionNode) or .ctor(IConfigAttrbuteNode)
  /// decorated with this attribute. If yes, then these .ctors are used to make a proper object instance
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
  public sealed class ConfigCtorAttribute : Attribute
  {
    public ConfigCtorAttribute() { }
  }

  /// <summary>
  /// Specifies how to apply configuration values to classes/fields/props
  /// </summary>
  [AttributeUsage(AttributeTargets.Class |
                  AttributeTargets.Struct |
                  AttributeTargets.Field |
                  AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
  public class ConfigAttribute : Attribute
  {

    /// <summary>
    /// Decorates members so that path is defaulted from member's name with prepended '$' attribute symbol
    /// </summary>
    public ConfigAttribute() { }

    /// <summary>
    /// Decorates members that will be configured from supplied path
    /// </summary>
    public ConfigAttribute(string path)
    {
      Path = path;
    }

    /// <summary>
    /// Decorates members that will be configured from supplied path and defaulted in case
    ///  the supplied path does not resolve to existing node
    /// </summary>
    public ConfigAttribute(string path, object defaultValue)
    {
      Path = path;
      Default = defaultValue;
    }

    /// <summary>
    /// String path of configuration source i.e. 'log/machine-name'.
    /// Path is relative to item root
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Default value used when configuration does not specify any other value
    /// </summary>
    public object Default { get; set; }

    /// <summary>
    /// Takes verbatim value if true
    /// </summary>
    public bool Verbatim { get; set; }

    /// <summary>
    /// Applies config values to fields/properties as specified by config attributes
    /// </summary>
    public static T Apply<T>(T entity, IConfigSectionNode node)
    {
      return (T)Apply((object)entity, node);
    }

    /// <summary>
    /// Builds config section node by taking entity object fields/prop values marked with this attribute
    /// </summary>
    /// <remarks>
    /// Note: members with [Config(Path..)] are not a subject to automatic serialization and need to be serialized by code
    /// </remarks>
    public static ConfigSectionNode BuildNode(object entity, ConfigSectionNode parentNode, string name)
    {
      ConfigSectionNode result;

      void doOneValue(MemberInfo mi, object v)
      {
        if (v == null) return;

        var n = GetConfigPathsForMember(mi, justSingleMemberName: true);

        if (v is IConfigurationPersistent vPersistent)
        {
          vPersistent.PersistConfiguration(result, n);
          return;
        }

        if (v is IConfigSectionNode vNode)
        {
          result.AddChildNode(vNode);
          return;
        }

        result.AddAttributeNode(n, v);
      }


      if (entity == null || parentNode == null || !parentNode.Exists) return parentNode;

      result = parentNode.AddChildNode(name);

      var etp = entity.GetType();

      var members = getAllFieldsOrProps(etp);
      foreach (var mem in members)
      {
        var mattr = mem.GetCustomAttributes(typeof(ConfigAttribute), true).FirstOrDefault() as ConfigAttribute;
        if (mattr == null) continue;
        if (mattr.Path.IsNotNullOrWhiteSpace()) continue;//can not serialize by explicit path - you need to serialize that by hand

        if (mem.MemberType == MemberTypes.Field)
        {
          var finf = (FieldInfo)mem;
          var v = finf.GetValue(entity);
          doOneValue(finf, v);
        } else if (mem.MemberType == MemberTypes.Property)
        {
          var pinf = (PropertyInfo)mem;
          var v = pinf.GetValue(entity, null);
          doOneValue(pinf, v);
        }
      }//foreach

      return result;
    }

    /// <summary>
    /// Applies config values to fields/properties as specified by config attributes
    /// </summary>
    public static object Apply(object entity, IConfigSectionNode node)
    {
      if (entity == null || node == null) return entity;

      var etp = entity.GetType();

      //20130708 DKh - support for [ConfigMacroContext] injection
      var macroAttr = etp.GetCustomAttributes(typeof(ConfigMacroContextAttribute), true).FirstOrDefault() as ConfigMacroContextAttribute;
      if (macroAttr != null)
        node.Configuration.MacroRunnerContext = entity;
      //---

      var cattr = etp.GetCustomAttributes(typeof(ConfigAttribute), true).FirstOrDefault() as ConfigAttribute;

      if (cattr != null)//rebase root config node per supplied path
      {
        cattr.evalAttributeVars(etp);

        var path = cattr.Path ?? CoreConsts.NULL_STRING;
        node = node.Navigate(path) as ConfigSectionNode;
        if (node == null)
          throw new ConfigException(string.Format(StringConsts.CONFIGURATION_NAVIGATION_REQUIRED_ERROR, cattr.Path));
      }

      var members = getAllFieldsOrProps(etp);
      foreach (var mem in members)
      {
        var mattr = mem.GetCustomAttributes(typeof(ConfigAttribute), true).FirstOrDefault() as ConfigAttribute;
        if (mattr == null) continue;

        //default attribute name taken from member name if path==null
        if (string.IsNullOrWhiteSpace(mattr.Path))
          mattr.Path = GetConfigPathsForMember(mem);

        mattr.evalAttributeVars(etp);

        var mnode = node.Navigate(mattr.Path);

        if (mem.MemberType == MemberTypes.Field)
        {
          var finf = (FieldInfo)mem;

          if (typeof(IConfigSectionNode).IsAssignableFrom(finf.FieldType))
          {
            if (finf.IsInitOnly)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR, etp.FullName, finf.Name));

            var snode = mnode as IConfigSectionNode;
            if (snode == null)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_PATH_ICONFIGSECTION_SECTION_ERROR, etp.FullName, finf.Name));
            finf.SetValue(entity, mnode);
          }
          else
          if (typeof(IConfigurable).IsAssignableFrom(finf.FieldType))
          {
            var snode = mnode as IConfigSectionNode;
            if (snode == null)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_PATH_ICONFIGURABLE_SECTION_ERROR, etp.FullName, finf.Name));

            if (finf.GetValue(entity) != null)
              ((IConfigurable)finf.GetValue(entity)).Configure(snode);
          }
          else
          {
            if (finf.IsInitOnly)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR, etp.FullName, finf.Name));

            if (mnode.Exists && (mnode is IConfigSectionNode || mnode.VerbatimValue != null))
              finf.SetValue(entity, getVal(mnode, finf.FieldType, etp.FullName, finf.Name, mattr.Verbatim));
            else
             if (mattr.Default != null) finf.SetValue(entity, mattr.Default);

          }

        }
        else
        if (mem.MemberType == MemberTypes.Property)
        {
          var pinf = (PropertyInfo)mem;

          if (typeof(IConfigSectionNode).IsAssignableFrom(pinf.PropertyType))
          {
            if (!pinf.CanWrite)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR, etp.FullName, pinf.Name));

            var snode = mnode as IConfigSectionNode;
            if (snode == null)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_PATH_ICONFIGSECTION_SECTION_ERROR, etp.FullName, pinf.Name));
            pinf.SetValue(entity, mnode, null);
          }
          else
          if (typeof(IConfigurable).IsAssignableFrom(pinf.PropertyType))
          {
            var snode = mnode as IConfigSectionNode;
            if (snode == null)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_PATH_ICONFIGURABLE_SECTION_ERROR, etp.FullName, pinf.Name));

            if (pinf.GetValue(entity, null) != null)
              ((IConfigurable)pinf.GetValue(entity, null)).Configure(snode);
          }
          else
          {
            if (!pinf.CanWrite)
              throw new ConfigException(string.Format(StringConsts.CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR, etp.FullName, pinf.Name));

            if (mnode.Exists && (mnode is IConfigSectionNode || mnode.VerbatimValue != null))
              pinf.SetValue(entity, getVal(mnode, pinf.PropertyType, etp.FullName, pinf.Name, mattr.Verbatim), null);
            else
             if (mattr.Default != null) pinf.SetValue(entity, mattr.Default, null);
          }
        }

      }//for members

      return entity;
    }

    /// <summary>
    /// Generates 2 attribute and 2 section paths for named member. This first path is just the member name converted to lower case.
    /// The second path is "OR"ed with the first one and is taken from member name where all case transitions are prefixed with "-".
    /// For private fields 'm_' and 's_' prefixes are removed
    /// </summary>
    public static string GetConfigPathsForMember(MemberInfo member, bool justSingleMemberName = false)
    {
      var mn = member.Name;

      if (member is FieldInfo fi)
      {
        //the field prefixes are swallowed
        if (mn.Length > 2 && (mn.StartsWith("m_", StringComparison.InvariantCulture) ||
                            mn.StartsWith("s_", StringComparison.InvariantCulture))
           ) mn = mn.Remove(0, 2);
      }

      if (justSingleMemberName) return mn;

      var sb = new StringBuilder();
      var first = true;
      var plc = false;
      foreach (var c in mn)
      {
        var clc = char.IsLower(c);
        if (!first && !clc && plc && c != '-') sb.Append('-');
        sb.Append(c);
        first = false;
        plc = clc;
      }

      return "${0}|${1}|{0}|{1}".Args(mn.ToLowerInvariant(), sb.ToString().ToLowerInvariant());
    }

    private static IEnumerable<MemberInfo> getAllFieldsOrProps(Type t)
    {
      var result = new List<MemberInfo>(64);

      while (t != typeof(object))
      {
        var fields = t.GetFields(BindingFlags.NonPublic |
                                   BindingFlags.Public |
                                   BindingFlags.Instance |
                                   BindingFlags.DeclaredOnly);
        result.AddRange(fields);

        var props = t.GetProperties(BindingFlags.NonPublic |
                                       BindingFlags.Public |
                                       BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly);
        result.AddRange(props);

        t = t.BaseType;
      }

      return result;
    }

    private static readonly FiniteSetLookup<Type, ConstructorInfo> s_SectionCtors = new FiniteSetLookup<Type, ConstructorInfo>
    (t => t.GetConstructors().FirstOrDefault(ci =>
       {
         var ps = ci.GetParameters();
         if (ps.Length != 1) return false;
         if (!typeof(IConfigSectionNode).IsAssignableFrom(ps[0].ParameterType)) return false;
         if (ci.GetCustomAttribute<ConfigCtorAttribute>() == null) return false;
         return true;
       })
    );

    private static readonly FiniteSetLookup<Type, ConstructorInfo> s_AttrCtors = new FiniteSetLookup<Type, ConstructorInfo>
    (t => t.GetConstructors().FirstOrDefault(ci =>
      {
        var ps = ci.GetParameters();
        if (ps.Length != 1) return false;
        if (!typeof(IConfigAttrNode).IsAssignableFrom(ps[0].ParameterType)) return false;
        if (ci.GetCustomAttribute<ConfigCtorAttribute>() == null) return false;
        return true;
      })
    );

    private static object getVal(IConfigNode node, Type type, string tname, string mname, bool verbatim)
    {
      try
      {
        if (node is IConfigSectionNode nodeSection)
        {
          var ctor = s_SectionCtors[type];
          if (ctor != null)
          {
            var got = ctor.Invoke(new []{ nodeSection });
            return got;
          }
        }

        if (node is IConfigAttrNode nodeAttr)
        {
          var ctor = s_AttrCtors[type];
          if (ctor != null)
          {
            var got = ctor.Invoke(new []{ nodeAttr });
            return got;
          }
        }

        return node.ValueAsType(type, verbatim, strict: false);
      }
      catch (Exception error)
      {
        if (error is TargetInvocationException tie) error = tie.InnerException;
        throw new ConfigException(StringConsts.CONFIGURATION_ATTR_APPLY_VALUE_ERROR.Args(mname, tname, error.Message), error);
      }
    }

    private void evalAttributeVars(Type type)
    {
      if (Path == null) return;

      Path = Path.Replace("@type@", type.FullName);
      Path = Path.Replace("@assembly@", type.Assembly.GetName().Name);
    }

  }

}