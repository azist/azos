/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NFX.Security.CAPTCHA;

namespace WinFormsTest
{
  public partial class BlankForm : Form
  {
    public BlankForm()
    {
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      GC.Collect();
    }

    private PuzzleKeypad m_Keypad;

    private void button4_Click(object sender, EventArgs e)
    {
      m_Keypad = new PuzzleKeypad(tbCode.Text, puzzleBoxWidth: 12);
      pic.Image = m_Keypad.DefaultRender();

    }

    private List<Point> m_Clicked = new List<Point>();

    private void pic_MouseClick(object sender, MouseEventArgs e)
    {
      m_Clicked.Add(new Point(e.X, e.Y));
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
      if (m_Keypad==null) return;

      Text = "Deciphered: "+ m_Keypad.DecipherCoordinates(m_Clicked);
      m_Clicked.Clear();
    }
  }
}
