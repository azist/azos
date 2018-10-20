
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
