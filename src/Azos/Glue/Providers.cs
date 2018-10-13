
using Azos.Apps;
using Azos.Collections;

namespace Azos.Glue
{
    /// <summary>
    /// Represents a base type for providers - providers are facades for some
    /// low-level implementation that transports use, for example ZeroMQ.
    /// </summary>
    public abstract class Provider : GlueComponentService
    {
        private void __ctor()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new GlueException(StringConsts.CONFIGURATION_ENTITY_NAME_ERROR + this.GetType().FullName);
            Glue.RegisterProvider(this);
        }

        protected Provider(string name)
            : this((IGlueImplementation)ExecutionContext.Application.Glue, name)
        {
        }

        protected Provider(IGlueImplementation glue, string name = null) : base(glue, name)
        {
            __ctor();
        }

        protected override void Destructor()
        {
            base.Destructor();
            Glue.UnregisterProvider(this);
        }
    }

    /// <summary>
    /// A registry of Provider-derived instances
    /// </summary>
    public sealed class Providers : Registry<Provider>
    {
          public Providers()
          {

          }


    }




}
