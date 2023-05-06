/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;

namespace Azos.Sky.Blob.Server
{
  internal sealed class Router : ApplicationComponent
  {
    sealed class Space : IAtomNamed
    {
      private Atom m_Name;

      public Atom Name => m_Name;
    }

    sealed class Volume : IAtomNamed
    {
      private Atom m_Name;

      public Atom Name => m_Name;

      /// <summary>
      /// Physical provider
      /// </summary>
      public IBlobStoreProvider Provider => null;

      public Shard[] IndexData => null;
      public Shard[] BlobData => null;
    }

    sealed class Shard : IOrdered
    {
      public int Order => throw new NotImplementedException();
    }


    public Router(BlobStoreServerLogic director) : base(director) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref m_LogArchiveGraph);
      base.Destructor();
    }

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

  }
}
