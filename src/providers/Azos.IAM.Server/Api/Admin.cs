using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;
using Azos.IAM.Protocol;
using Azos.Serialization.JSON;
using Azos.Wave;
using Azos.Wave.Mvc;

namespace Azos.IAM.Server.Api
{
  [NoCache]
  public class Admin : ApiProtocolController
  {
    [Inject] IAdminLogic m_Logic;

    [ActionOnPost]
    public async Task<object> Filter(IBusinessFilterModel filter) => await ApplyFilter(filter);

    [ActionOnGet]
    public async Task<object> EntityBody(string tEntity, GDID gEntity)
    {
      switch(tEntity)
      {            //use reflection to map the type to generic instead of switch
        case "group": return await m_Logic.GetEntityBodyAsync<GroupEntityBody>(gEntity);
      }
      throw new NotImplementedException();
    }

    [ActionOnPost]
    public async Task<object> Change(ChangeForm change) => await SaveEdit(change);


    #region dynamic binding
    private static readonly MethodInfo MI_FILTER = typeof(Admin).GetMethod("Filter");
    private static readonly MethodInfo MI_CHANGE = typeof(Admin).GetMethod("Change");

    protected override MethodInfo FindMatchingAction(WorkContext work, string action, out object[] args)
    {
      if (action.IsOneOf("filter", "change"))
      {
        var body = work.RequestBodyAsJSONDataMap;
        var t = body[Form.JSON_TYPE_PROPERTY].AsString();
        if (t.IsNotNullOrWhiteSpace())
        {
          var tp = Assembly.GetExecutingAssembly().GetType("Azos.IAM.Protocol.{0}".Args(t), false, true);
          if (tp == null)
          {
            args = new[] { JsonReader.ToDoc(tp, body) };
            return action.EqualsOrdIgnoreCase("filter") ? MI_FILTER : MI_CHANGE;
          }
        }
      }
      return base.FindMatchingAction(work, action, out args);
    }
    #endregion

  }
}
