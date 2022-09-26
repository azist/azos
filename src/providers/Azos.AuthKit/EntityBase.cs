/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.AuthKit
{
  public abstract class EntityBase<TSaveLogic, TSaveResult> : PersistedEntity<TSaveLogic, TSaveResult> where TSaveLogic : class, IBusinessLogic
  {
    /// <summary>
    /// Realm is an implicit ambient context which drives security checks,
    /// therefore here we provide a convenience accessor only.
    /// </summary>
    public Atom Realm => Ambient.CurrentCallSession.GetAtomDataContextName();

    protected override Task DoBeforeSaveAsync()
    {
      // Not needed as we override the logic below because we skip Constraints.GDID_RESERVED_ID_COUNT gdids
      ////await base.DoBeforeSaveAsync().ConfigureAwait(false);

      //Generate new GDID only AFTER all checks are passed not to waste gdid instance
      //in case of validation errors
      if (FormMode == FormMode.Insert && m_GdidGenerator != null)
      {
        do Gdid = m_GdidGenerator.Provider.GenerateGdidFor(this.GetType());
        while (Gdid.Authority == Constraints.GDID_RESERVED_ID_AUTHORITY && Gdid.Counter < Constraints.GDID_RESERVED_ID_COUNT);//skip COUNT reserved IDs
      }

      return Task.CompletedTask;
    }
  }
}
