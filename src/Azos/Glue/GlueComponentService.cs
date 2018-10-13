
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

using Azos.Log;

namespace Azos.Glue
{
    /// <summary>
    /// Provides base functionality for internal glue component implementations
    /// </summary>
    public abstract class GlueComponentService : ServiceWithInstrumentationBase<IGlueImplementation>
    {
        internal GlueComponentService(IGlueImplementation glue) : base(glue)
        {
        }

        protected GlueComponentService(IGlueImplementation glue, string name)
            : base(glue)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = Guid.NewGuid().ToString();

            base.Name = name;
        }

        #region Fields
          private bool m_InstrumentationEnabled;
        #endregion

        #region Properties



            public IGlueImplementation Glue { get { return ComponentDirector; } }

            /// <summary>
            /// Implements IInstrumentable
            /// </summary>
            [Config(Default=false)]
            [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_GLUE, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
            public override bool InstrumentationEnabled
            {
              get { return m_InstrumentationEnabled;}
              set { m_InstrumentationEnabled = value;}
            }

        #endregion


        #region Public

          public void WriteLog(
              LogSrc source, MessageType type, string msg, string from = null,
              Exception exception = null, string pars = null, Guid? relatedTo = null)
          {
            var pass = false;

            if (source== LogSrc.Server) pass= type >= Glue.ServerLogLevel;
            else
            if (source== LogSrc.Client) pass= type >= Glue.ClientLogLevel;
            else
             pass = (type >= Glue.ClientLogLevel) || (type >= Glue.ServerLogLevel);


            if ( pass )
                App.Log.Write( new Message
                {
                   Type = type,
                   From = string.Format("{0}:{1}.'{2}' {3}", source, GetType().Name, Name, from ?? string.Empty),
                   Topic = CoreConsts.GLUE_TOPIC,
                   Source = (int)source,
                   Text = msg,
                   Exception = exception,
                   RelatedTo = relatedTo.HasValue ? relatedTo.Value : Guid.Empty,
                   Parameters = pars
                });

          }

        #endregion


    }
}
