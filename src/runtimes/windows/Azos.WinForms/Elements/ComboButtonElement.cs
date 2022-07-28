/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.WinForms.Elements
{
  /// <summary>
  /// Represents a combo box button element
  /// </summary>
  public class ComboButtonElement : PressableElement
  {
    #region .ctor

      public ComboButtonElement(ElementHostControl host)
        : base(host)
      {
      }

    #endregion


    #region Private Fields

    #endregion


    #region Properties

    #endregion


    #region Events


    #endregion


    #region Public

    #endregion


    #region Protected


        protected internal override void Paint(System.Drawing.Graphics gr)
        {
          base.Paint(gr);
          //BaseApplication.Theme.PartRenderer.ComboButton(gr,
          //                                          Region,
          //                                          MouseIsOver,
          //                                          Pressed,
          //                                          FieldControlContext);
        }


    #endregion


    #region Private Utils



    #endregion

  }

}
