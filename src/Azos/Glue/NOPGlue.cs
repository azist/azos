/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Glue.Protocol;

namespace Azos.Glue
{
    public class NOPGlue : ApplicationComponent, IGlueImplementation
    {

         internal NOPGlue(IApplication app) : base(app) {}

         public bool Active => false;

         public override string ComponentLogTopic => CoreConsts.GLUE_TOPIC;
         public bool InstrumentationEnabled{ get{return false;} set{}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get{ return null;}}
         public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups){ return null;}
         public bool ExternalGetParameter(string name, out object value, params string[] groups)
         {
           value = null;
           return false;
         }
         public bool ExternalSetParameter(string name, object value, params string[] groups)
         {
           return false;
         }


        public void RegisterProvider(Provider p)
        {
        }

        public void UnregisterProvider(Provider p)
        {
        }

        public void RegisterBinding(Binding b)
        {
        }

        public void UnregisterBinding(Binding b)
        {
        }

        public void RegisterServerEndpoint(ServerEndPoint ep)
        {

        }

        public void UnregisterServerEndpoint(ServerEndPoint ep)
        {

        }

        public Binding GetNodeBinding(Node node)
        {
            return null;
        }

        public Binding GetNodeBinding(string node)
        {
            return null;
        }

        public IRegistry<Provider> Providers
        {
            get { return new Registry<Provider>(); }
        }

        public IRegistry<Binding> Bindings
        {
            get { return new Registry<Binding>(); }
        }

        public IRegistry<ServerEndPoint> Servers
        {
            get { return new Registry<ServerEndPoint>(); }
        }


        public OrderedRegistry<IClientMsgInspector> ClientMsgInspectors { get { return new OrderedRegistry<IClientMsgInspector>(); } }

        public OrderedRegistry<IServerMsgInspector> ServerMsgInspectors { get { return new OrderedRegistry<IServerMsgInspector>(); } }


        public int DefaultDispatchTimeoutMs
        {
            get { return Implementation.GlueDaemon.DEFAULT_DISPATCH_TIMEOUT_MS; }
            set {}
        }

        public int DefaultTimeoutMs
        {
            get { return Implementation.GlueDaemon.DEFAULT_TIMEOUT_MS; }
            set {}
        }

         public int ServerInstanceLockTimeoutMs
        {
            get { return Implementation.GlueDaemon.DEFAULT_SERVER_INSTANCE_LOCK_TIMEOUT_MS; }
            set {}
        }

        public void Configure(Conf.IConfigSectionNode node)
        {

        }


        public RequestMsg ClientDispatchingRequest(ClientEndPoint client, RequestMsg request)
        {
          return request;
        }

        public void ClientDispatchedRequest(ClientEndPoint client, RequestMsg request, CallSlot callSlot)
        {

        }

        public void ClientDeliverAsyncResponse(ResponseMsg response)
        {

        }

        public void ServerDispatchRequest(RequestMsg request)
        {
        }

        public ResponseMsg ServerHandleRequest(RequestMsg request)
        {
            return null;
        }

        public ResponseMsg ServerHandleRequestFailure(FID reqID, bool oneWay, Exception failure, object bindingSpecCtx)
        {
            return null;
        }

        public IApplication Application
        {
            get { return ExecutionContext.Application; }
        }

        public Conf.IConfigSectionNode GlueConfiguration
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public Conf.IConfigSectionNode ProvidersConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Conf.IConfigSectionNode> ProviderConfigurations
        {
            get { return Enumerable.Empty<Conf.IConfigSectionNode>(); }
        }

        public Conf.IConfigSectionNode BindingsConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Conf.IConfigSectionNode> BindingConfigurations
        {
            get { return Enumerable.Empty<Conf.IConfigSectionNode>(); }
        }

        public Conf.IConfigSectionNode ServersConfigurationSection
        {
            get { return Application.ConfigRoot.Configuration.EmptySection; }
        }

        public IEnumerable<Conf.IConfigSectionNode> ServerConfigurations
        {
            get { return Enumerable.Empty<Conf.IConfigSectionNode>(); }
        }


        public Log.MessageType ClientLogLevel
        {
            get { return Log.MessageType.CatastrophicError; }
            set{}
        }

        public Log.MessageType ServerLogLevel
        {
            get { return Log.MessageType.CatastrophicError; }
            set{}
        }

        public Time.TimeLocation TimeLocation
        {
            get { return App.TimeLocation; }
        }

        public DateTime LocalizedTime
        {
            get { return App.LocalizedTime; }
        }

        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            return App.UniversalTimeToLocalizedTime(utc);
        }

        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            return App.LocalizedTimeToUniversalTime(local);
        }

        public void SubscribeCallSlotWithTaskReactor(CallSlot call)
        {
        }
  }
}
