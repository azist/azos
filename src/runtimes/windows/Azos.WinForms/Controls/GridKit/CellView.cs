/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Azos.WinForms.Elements;

namespace Azos.WinForms.Controls.GridKit
{
  /// <summary>
  /// Provides a viewport for grid cells
  /// </summary>
  public class CellView : ElementHostControl
  {

     internal CellView():base()
     {
       //for(var x=0; x<1600; x+= 48)
       //  for(var y=0; y<1800; y+= 16)
       //  {
       //  var elm =  new CheckBoxElement(this);// new TextLabelElement(this);
       //  elm.Region = new Rectangle(x,y, 46, 14);
       // // elm.Text = "Cell " + x+"x"+y;
       //  elm.Visible = true;
       //  }
     }

     /// <summary>
     /// Points to parent Grid that hosts this cell view
     /// </summary>
     public Grid Grid{ get { return (Grid)Parent; } }
  }
}
