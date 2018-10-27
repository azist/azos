/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.WinForms.Elements
{
  public class RadioButtonElement : CheckableElement
  {
    #region .ctor

          public RadioButtonElement(ElementHostControl host) : base(host)
          {
          }

    #endregion


    #region Private Fields

      private object m_Key;

    #endregion


    #region Properties

      public object Key
      {
        get { return m_Key; }
        set { m_Key = value;}
      }

    #endregion


    #region Public


    #endregion


    #region Protected

      protected internal override void Paint(System.Drawing.Graphics gr)
      {
        //BaseApplication.Theme.PartRenderer.RadioButton(gr, Region, MouseIsOver, FieldControlContext, Checked);
      }

    #endregion


    #region Private Utils


    #endregion

  }

}
