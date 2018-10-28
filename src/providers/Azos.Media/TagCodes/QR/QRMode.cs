
/*
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */

namespace Azos.Media.TagCodes.QR
{
  public sealed class QRMode
  {
      #region Inner Types

        public enum EMode { TERMINATOR, NUMERIC, ALPHANUMERIC, STRUCTURED_APPEND, BYTE, ECI, KANJI, FNC1_FIRST_POSITION, FNC1_SECOND_POSITION, HANZI};

      #endregion

    #region Static

      public static readonly QRMode TERMINATOR = new QRMode( EMode.TERMINATOR, 0x00, new int[] { 0, 0, 0 });
      public static readonly QRMode NUMERIC = new QRMode(EMode.NUMERIC, 0x01, new int[] { 10, 12, 14 });
      public static readonly QRMode ALPHANUMERIC = new QRMode(EMode.ALPHANUMERIC, 0x02, new int[] { 9, 11, 13 });
      public static readonly QRMode STRUCTURED_APPEND = new QRMode(EMode.STRUCTURED_APPEND, 0x03, new int[] { 0, 0, 0 }); // Not supported
      public static readonly QRMode BYTE = new QRMode(EMode.BYTE, 0x04, new int[] { 8, 16, 16 });
      public static readonly QRMode ECI = new QRMode(EMode.ECI, 0x07, null); // character counts don't apply
      public static readonly QRMode KANJI = new QRMode(EMode.KANJI, 0x08, new int[] { 8, 10, 12 });
      public static readonly QRMode FNC1_FIRST_POSITION = new QRMode(EMode.FNC1_FIRST_POSITION, 0x05, null);
      public static readonly QRMode FNC1_SECOND_POSITION = new QRMode(EMode.FNC1_SECOND_POSITION, 0x09, null);
      public static readonly QRMode HANZI = new QRMode(EMode.HANZI, 0x0D, new int[] { 8, 10, 12 });

    #endregion

    #region .ctor

      private QRMode(EMode mode, int ordinal, int[] versionCharacterCount)
      {
        Mode = mode;
        ModeSignature = ordinal;
        VersionCharacterCount = versionCharacterCount;
      }

    #endregion

    #region Properties

      public readonly EMode Mode;
      public readonly int ModeSignature;
      public readonly int[] VersionCharacterCount;

    #endregion

    #region Public

      public int GetVersionCharacterCount(QRVersion version)
      {
         if (VersionCharacterCount == null)
           throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".appendBytes(version=>VersionCharacterCount!=null)");

         int number = version.Number;
         int offset;
         if (number <= 9)
         {
            offset = 0;
         }
         else if (number <= 26)
         {
            offset = 1;
         }
         else
         {
            offset = 2;
         }
         return VersionCharacterCount[offset];
      }

    #endregion

    #region Protected

      public override string ToString()
      {
        return Mode.ToString();
      }

    #endregion

  }//class

}
