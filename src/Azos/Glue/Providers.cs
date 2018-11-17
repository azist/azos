/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Collections;

namespace Azos.Glue
{
#warning Needs revision - this can be superseded with modules
    /// <summary>
    /// Represents a base type for providers - providers are facades for some
    /// low-level implementation that transports use, for example ZeroMQ.
    /// </summary>
    public abstract class Provider : GlueComponent
    {
      protected Provider(IApplication app, IGlueImplementation glue, string name = null) : base(app, glue, name)
      {
         if (string.IsNullOrWhiteSpace(Name))
             throw new GlueException(StringConsts.CONFIGURATION_ENTITY_NAME_ERROR + this.GetType().FullName);
         Glue.RegisterProvider(this);
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
