/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
