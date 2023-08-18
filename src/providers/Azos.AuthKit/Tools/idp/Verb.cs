/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.AuthKit.Tools.idp
{
  public abstract class Verb
  {
    protected Verb(IIdpUserAdminLogic logic, bool silent)
    {
      m_Silent = silent;
      m_Logic = logic;
    }

    protected readonly bool m_Silent;
    protected readonly IIdpUserAdminLogic m_Logic;

    public abstract void Run();
  }
}
