/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

         private void validateMetabase(ValidationContext ctx)
         {
            if (validate_Contracts(ctx)) validate_GDIDAuthorities(ctx);
            validate_Platforms(ctx);
            validate_Networks(ctx);
            validate_Catalogs(ctx);
         }


         private bool validate_Contracts(ValidationContext ctx)
         {
            var output = ctx.Output;

            try
            {
              var instance = Contracts.ServiceClientHub.Instance;
            }
            catch(Exception error)
            {
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_CONTRACTS_SERVICE_HUB_ERROR+error.ToMessageWithType()));
              return false;
            }

            foreach(var mapping in Contracts.ServiceClientHub.Instance.CachedMap)
            {
              if (mapping.Local.Service.IsNullOrWhiteSpace())
               output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, null, null, " Contract mapping '{0}' does not specify local service name".Args(mapping) ));

              if (mapping.Global.Service.IsNullOrWhiteSpace())
               output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, null, null, " Contract mapping '{0}' does not specify global service name".Args(mapping) ));
            }

            return true;
         }


         private void validate_GDIDAuthorities(ValidationContext ctx)
         {
            var output = ctx.Output;

            var authorities = GDIDAuthorities;
            if (!authorities.Any())
            {
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Warning, null, null, StringConsts.METABASE_GDID_AUTHORITIES_NONE_DEFINED_WARNING) );
              return;
            }

            if (authorities.Count() != authorities.Distinct().Count())
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_GDID_AUTHORITIES_DUPLICATION_ERROR) );


            foreach(var authority in authorities)
            {
              var host = CatalogReg[authority.Name] as SectionHost;
              if (host==null)
              {
               output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_GDID_AUTHORITY_BAD_HOST_ERROR.Args(authority.Name)) );
               continue;
              }

              if (!host.Role.AppNames.Any(n=>INVSTRCMP.Equals(n, SysConsts.APP_NAME_GDIDA)))
                output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_GDID_AUTHORITY_HOST_NOT_AGDIDA_ERROR.Args(authority.Name, host.Role)) );

              try
              {
                Contracts.ServiceClientHub.TestSetupOf<Contracts.IGdidAuthorityClient>(host.RegionPath);
              }
              catch(Exception error)
              {
                output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_GDID_AUTHORITY_SVC_RESOLUTION_ERROR.Args(authority.Name, error.ToMessageWithType())) );
              }
            }
         }


         private void validate_Platforms(ValidationContext ctx)
         {
            var output = ctx.Output;

            try//PLATFORMS
            {
              foreach(var pn in PlatformConfNodes)
              {
                var name = pn.AttrByName(CONFIG_NAME_ATTR).Value;
                if (name.IsNullOrWhiteSpace())
                 output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NAME_ATTR_UNDEFINED_ERROR + "platforms" ) );

                if (!name.IsValidName())
                 output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_ENTITY_NAME_INVALID_ERROR.Args("platforms", name) ) );
              }


              //Duplicate platform name
              if (PlatformNames.Distinct(INVSTRCMP).Count()!=PlatformNames.Count())
                output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_PLATFORM_NAME_DUPLICATION_ERROR ) );

              //Platform/OS names
              foreach(var os in OSNames)
              {
                if (!os.IsValidName())
                 output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_ENTITY_NAME_INVALID_ERROR.Args("OS", os) ) );

                try
                {
                  if (INVSTRCMP.Equals(os,BinCatalog.PackageInfo.ANY))
                    throw new MetabaseException(StringConsts.METABASE_PLATFORM_OS_RESERVED_NAME_ERROR.Args(os));
                  GetOSConfNode(os);
                }
                catch(Exception error)
                {
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, error.ToMessageWithType(), error) );
                }
              }
            }
            catch(Exception general)
            {
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, general.ToMessageWithType(), general) );
            }
         }



         private void validate_Networks(ValidationContext ctx)
         {
            var output = ctx.Output;

            try
            {
                foreach(var node in NetworkConfNodes)
                {
                 var name = node.AttrByName(CONFIG_NAME_ATTR).Value;
                 if (name.IsNullOrWhiteSpace())
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NAME_ATTR_UNDEFINED_ERROR + "networks" ) );

                 if (!name.IsValidName())
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_ENTITY_NAME_INVALID_ERROR.Args("networks", name) ) );
                }


                //Duplicate network name
                if (NetworkNames.Distinct(INVSTRCMP).Count()!=NetworkNames.Count())
                  output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_NAME_DUPLICATION_ERROR ) );

                //Duplicate service name under network or binding under network/service
                foreach(var net in NetworkNames)
                {
                  if (GetNetworkSvcNames(net).Distinct(INVSTRCMP).Count()!=GetNetworkSvcNames(net).Count())
                   output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_SVC_NAME_DUPLICATION_ERROR.Args(net) ) );

                  if (GetNetworkGroupNames(net).Distinct(INVSTRCMP).Count()!=GetNetworkGroupNames(net).Count())
                   output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_GRP_NAME_DUPLICATION_ERROR.Args(net) ) );

                  var services = GetNetworkSvcNodes(net);

                  if (services.Count()==0)
                    output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_NO_SVC_ERROR.Args(net) ) );
                  foreach(var svcNode in services)
                  {
                    var svc = svcNode.AttrByName(CONFIG_NAME_ATTR).Value;
                    if (svc.IsNullOrWhiteSpace())
                      output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NAME_ATTR_UNDEFINED_ERROR + "network: '{0}' service".Args(net) ) );

                    if (!svc.IsValidName())
                      output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_ENTITY_NAME_INVALID_ERROR.Args("network: '{0}' service".Args(net), svc) ) );


                    if (GetNetworkSvcBindingNames(net, svc).Distinct(INVSTRCMP).Count()!=GetNetworkSvcBindingNames(net, svc).Count())
                      output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_SVC_BINDING_NAME_DUPLICATION_ERROR.Args(net, svc) ) );


                    var bindings = GetNetworkSvcBindingNodes(net,svc);
                    if (bindings.Count()==0)
                      output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NETWORK_NO_SVC_BINDINGS_ERROR.Args(net, svc) ) );

                    var db = svcNode.AttrByName(CONFIG_DEFAULT_BINDING_ATTR).Value;
                    if (db.IsNotNullOrWhiteSpace())
                      if (!bindings.Any(n=>n.IsSameName(db)))
                        output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null,
                                            StringConsts.METABASE_NETWORK_SVC_DEFAULT_BINDING_NAME_ERROR.Args(net, svc, db) ) );
                  }

                  var groups = GetNetworkGroupNodes(net);
                  foreach(var node in groups)
                  {
                   var grpName = node.AttrByName(CONFIG_NAME_ATTR).Value;
                   if (grpName.IsNullOrWhiteSpace())
                    output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_NAME_ATTR_UNDEFINED_ERROR + "network: '{0}' group".Args(net) ) );

                   if (!grpName.IsValidName())
                    output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, StringConsts.METABASE_ENTITY_NAME_INVALID_ERROR.Args("network: '{0}' group".Args(net), grpName) ) );

                  }
                }
            }
            catch(Exception general)
            {
              output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, general.ToMessageWithType(), general) );
            }
         }



         private void validate_Catalogs(ValidationContext ctx)
         {
             foreach(var catalog in Catalogs)
              try
              {
                catalog.Validate(ctx);
              }
              catch(Exception error)
              {
                ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, null, null, error.ToMessageWithType(), error) );
              }
         }

}}
