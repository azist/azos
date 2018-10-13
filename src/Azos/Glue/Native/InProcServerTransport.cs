
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue.Protocol;

namespace Azos.Glue.Native
{
    /// <summary>
    /// Provides server-side functionality for synchronous communication pattern based on
    ///  in-memory message exchange without serialization
    /// </summary>
    public class InProcServerTransport : ServerTransport<InProcBinding>
    {
        #region .ctor

           public InProcServerTransport(InProcBinding binding, ServerEndPoint serverEndpoint) : base(binding, serverEndpoint)
           {
             Node = serverEndpoint.Node;
           }

        #endregion

        #region Fields/Props

        #endregion

        #region Protected

            protected override bool DoSendResponse(ResponseMsg response)
            {
              throw new InvalidGlueOperationException(StringConsts.OPERATION_NOT_SUPPORTED_ERROR + GetType().FullName+".SendResponse()");
            }

        #endregion
    }
}
