using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access.Oracle.Instrumentation
{
  //[AdvancedSystemAdministration(4)]
  public class PipeSql : ExternalCallRequest<OracleDataStoreBase>
  {
    public PipeSql(OracleDataStoreBase store) : base (store){ }


    [Config]
    public string CommandText{ get; set; }

    [Config("p|par|prms|pars|params|parameters")]
    public IConfigSectionNode Parameters { get; set;}

    public override ExternalCallResponse Describe()
    {
      throw new NotImplementedException();
    }

    public override ExternalCallResponse Execute()
    {
      var connection = Context.GetConnection().GetAwaiter().GetResult();
     // connection.Test();
      return new ExternalCallResponse();
    }

  }
}
