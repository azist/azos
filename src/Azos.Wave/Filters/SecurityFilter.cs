
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Security;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Checks permissions before doing work
  /// </summary>
  public sealed class SecurityFilter : WorkFilter
  {
    public const string CONFIG_BYPASS_SECTION = "bypass";

    #region .ctor
      public SecurityFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public SecurityFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) { ctor(confNode); }
      public SecurityFilter(WorkHandler handler, string name, int order) : base(handler, name, order){}
      public SecurityFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) { ctor(confNode); }

      private void ctor(IConfigSectionNode confNode)
      {
       var permsNode = confNode[Permission.CONFIG_PERMISSIONS_SECTION];
        if (permsNode.Exists)
          m_Permissions = Permission.MultipleFromConf(permsNode);

       foreach(var cn in confNode[CONFIG_BYPASS_SECTION].Children.Where(cn=>cn.IsSameName(WorkMatch.CONFIG_MATCH_SECTION)))
          if(!m_BypassMatches.Register( FactoryUtils.Make<WorkMatch>(cn, typeof(WorkMatch), args: new object[]{ cn })) )
            throw new WaveException(StringConsts.CONFIG_OTHER_DUPLICATE_MATCH_NAME_ERROR
                                         .Args(cn.AttrByName(Configuration.CONFIG_NAME_ATTR).Value,
                                                             "{0}".Args(GetType().FullName)));
      }

    #endregion

    #region Fields

      private IEnumerable<Permission> m_Permissions;
      private OrderedRegistry<WorkMatch> m_BypassMatches = new OrderedRegistry<WorkMatch>();

    #endregion

    #region Properties

      /// <summary>
      /// Gets/sets permissions that get checked by this filter instance
      /// </summary>
      public IEnumerable<Permission> Permissions
      {
        get{return m_Permissions;}
        set{m_Permissions = value;}
      }

    #endregion


    #region Protected

      protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
        if (m_Permissions!=null && m_Permissions.Any())
        {
          if (!m_BypassMatches.Any(match => match.Make(work)!=null))
          {
            work.NeedsSession();
            Permission.AuthorizeAndGuardAction(m_Permissions, "{0}({1})".Args(GetType().FullName, this.Name), work.Session);
          }
        }

        this.InvokeNextWorker(work, filters, thisFilterIndex);
      }

    #endregion

  }

}
