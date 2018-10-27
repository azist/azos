/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Templatization;

namespace Azos.Tests.Unit.Templatization
{
  public class TeztTemplate : Template<object, StringRenderingTarget, object>
  {
       protected TeztTemplate():base()
       {

       }

       protected TeztTemplate(object context) : base(context)
       {

       }

       public override bool CanReuseInstance
       {
         get { return true; }
       }
  }
}
