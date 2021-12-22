/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

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
    (string provider, Atom loginType, string parsedId) ParseId(string id);
    (string provider, Atom loginType, string parsedUri) ParseUri(string uri);

    string SysTokenCryptoAlgorithmName { get; }
    double SysTokenLifespanHours       { get; }

   // (ConfigVector props, ConfigVector rights) CalculateEffectivePolicies(ConfigVector userProps, ConfigVector loginProps);

    string MakeSystemTokenData(GDID gUser, JsonDataMap data = null);
    //JsonDataMap CheckSystemToken(string token)
  }
}
