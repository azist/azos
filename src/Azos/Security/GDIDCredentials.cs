/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data.Distributed;

namespace Azos.Security
{
  /// <summary>
  /// Represents credentials based on Global Distributed ID
  /// </summary>
  [Serializable]
  public class GDIDCredentials : Credentials
  {
     public GDIDCredentials(GDID gdid)
     {
       m_GDID = gdid;
     }


     private GDID m_GDID;


     public GDID GDID
     {
       get { return m_GDID; }
     }

     public override void Forget()
     {
     }

     public override string ToString()
     {
       return "{0}({1})".Args(GetType().Name, GDID);
     }


  }
}
