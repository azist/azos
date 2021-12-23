/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Server
{
  /// <summary>
  /// Handles the core functionality of identity provider implementation
  /// </summary>
  public interface IIdpHandlerLogic : IBusinessLogic
  {
    /// <summary>
    /// Registry of login providers
    /// </summary>
    IRegistry<LoginProvider> Providers {  get; }

    /// <summary>
    /// Parses the supplied login string expressed in EntityId format.
    /// The string has to be formatted as EntityId or plain string which then assumes defaults.
    /// The EntityId.System is Provider.Name, and EntityId.Type is login type
    /// </summary>
    EntityId ParseId(string id);

    /// <summary>
    /// Parses the supplied uri expressed in EntityId format.
    /// The string has to be formatted as EntityId or plain string which then assumes defaults.
    /// The EntityId.System is Provider.Name, and EntityId.Type is login type
    /// </summary>
    EntityId ParseUri(string uri);

    string SysTokenCryptoAlgorithmName { get; }
    double SysTokenLifespanHours       { get; }

   // (ConfigVector props, ConfigVector rights) CalculateEffectivePolicies(ConfigVector userProps, ConfigVector loginProps);

    string MakeSystemTokenData(GDID gUser, JsonDataMap data = null);
    //JsonDataMap CheckSystemToken(string token)
  }

}
