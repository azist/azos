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
using Azos.Collections;

namespace Azos.Wave.Mvc
{

  /// <summary>
  /// Decorates MVC Actions
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public abstract class ActionBaseAttribute : Attribute
  {
    /// <summary>
    /// When set, specifies the invocation name override, null by default which means that the name of decorated member should be used
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Dictates the match making order of actions within the group
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Returns true if default parameter binder should not perform indirect value conversions, i.e. integer tick number as date time.
    /// False by default
    /// </summary>
    public bool StrictParamBinding { get; set; }

    /// <summary>
    /// Returns ordered matches for this action empty enum
    /// </summary>
    public abstract IEnumerable<WorkMatch> Matches {  get; }
  }


  /// <summary>
  /// Decorates MVC Actions
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionAttribute : ActionBaseAttribute
  {
    public ActionAttribute() { }

    private OrderedRegistry<WorkMatch> m_Matches;

    /// <summary>
    /// Returns match script in Laconic config format if the one was supplied or empty string
    /// </summary>
    public string MatchScript { get; set; }

    /// <summary>
    /// Returns ordered matches configured from script or empty enum
    /// </summary>
    public override IEnumerable<WorkMatch> Matches
    {
      get
      {
        if (m_Matches==null)
        {
          m_Matches = new OrderedRegistry<WorkMatch>();
          if (MatchScript.IsNotNullOrWhiteSpace())
          {
            ConfigSectionNode root;
            try
            {
              root = LaconicConfiguration.CreateFromString("root{{{0}}}".Args(MatchScript)).Root;
            }
            catch(Exception error)
            {
              throw new WaveException(StringConsts.MVC_ACTION_ATTR_MATCH_PARSING_ERROR.Args(MatchScript, error.ToMessageWithType()), error);
            }

            var children = root.Children.Where(cn => cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION));
            if (!children.Any()) children = new []{root};

            foreach (var cn in children)
              if(!m_Matches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
               throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR.Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value, "ActionAttribute"));
          }
          else
           m_Matches.Register(new WorkMatch("*",0));

        }
        return m_Matches.OrderedValues;
      }
    }

  }


  /// <summary>
  /// Decorates MVC Actions which happen on HTTP GET
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionOnGetAttribute : ActionBaseAttribute
  {
    public ActionOnGetAttribute() { }

    private WorkMatch m_Match = new WorkMatch("*", 0) { Methods = WebConsts.HTTP_GET };

    public override IEnumerable<WorkMatch> Matches { get { yield return m_Match; } }
  }

  /// <summary>
  /// Decorates MVC Actions which happen on HTTP POST
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionOnPostAttribute : ActionBaseAttribute
  {
    public ActionOnPostAttribute() { }

    private WorkMatch m_Match = new WorkMatch("*", 0) { Methods = WebConsts.HTTP_POST };

    public override IEnumerable<WorkMatch> Matches { get{ yield return m_Match;} }
  }

  /// <summary>
  /// Decorates MVC Actions which happen on HTTP PUT
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionOnPutAttribute : ActionBaseAttribute
  {
    public ActionOnPutAttribute() { }

    private WorkMatch m_Match = new WorkMatch("*", 0) { Methods = WebConsts.HTTP_PUT };

    public override IEnumerable<WorkMatch> Matches { get { yield return m_Match; } }
  }

  /// <summary>
  /// Decorates MVC Actions which happen on HTTP PATCH
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionOnPatchAttribute : ActionBaseAttribute
  {
    public ActionOnPatchAttribute() { }

    private WorkMatch m_Match = new WorkMatch("*", 0) { Methods = WebConsts.HTTP_PATCH };

    public override IEnumerable<WorkMatch> Matches { get { yield return m_Match; } }
  }

  /// <summary>
  /// Decorates MVC Actions which happen on HTTP DELETE
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
  public sealed class ActionOnDeleteAttribute : ActionBaseAttribute
  {
    public ActionOnDeleteAttribute() { }

    private WorkMatch m_Match = new WorkMatch("*", 0) { Methods = WebConsts.HTTP_DELETE };

    public override IEnumerable<WorkMatch> Matches { get { yield return m_Match; } }
  }


}
