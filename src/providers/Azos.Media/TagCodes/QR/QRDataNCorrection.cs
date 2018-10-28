/*
 *Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */

namespace Azos.Media.TagCodes.QR
{
  public sealed class QRDataNCorrection
  {
    public QRDataNCorrection(byte[] data, byte[] correction)
    {
      Data = data;
      Correction = correction;
    }
    public readonly byte[] Data;
    public readonly byte[] Correction;
  }

}
