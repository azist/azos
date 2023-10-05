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
    protected Verb(IIdpUserAdminLogic logic) => m_Logic = logic;
    private readonly IIdpUserAdminLogic m_Logic;

    protected IApplication App => m_Logic.App;
    protected IIdpUserAdminLogic Logic => m_Logic;
    protected bool IsSilent => App.CommandArgs["s", "silent"].Exists;
    protected bool IsJson => App.CommandArgs["json"].Exists;

    public abstract void Run();
  }
}
