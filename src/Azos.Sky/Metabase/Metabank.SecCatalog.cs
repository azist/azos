
namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

    /// <summary>
    /// Represents a system catalog of security metadata
    /// </summary>
    public sealed class SecCatalog : SystemCatalog
    {
      #region CONSTS

      #endregion

      #region .ctor
          internal SecCatalog(Metabank bank) : base(bank, SEC_CATALOG)
          {

          }

      #endregion

      #region Fields


      #endregion

      #region Properties


      #endregion

      #region Public

        public override void Validate(ValidationContext ctx)
        {

        }

      #endregion
    }

}}
