/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data.Business;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.AuthKit
{
  /// <summary>
  /// Outlines core functionality for working with user accounts.
  /// The logic is compatible with/based on MinIdp
  /// </summary>
  public interface IIdpUserCoreLogic : IBusinessLogic, IMinIdpStore
  {
  }
}
