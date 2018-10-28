
/*
 * Based on zXing / Apache 2.0; See NOTICE and CHANGES for attribution
 */

namespace Azos.Media.TagCodes.QR
{
  public sealed class QRCorrectionLevel
  {
    public static readonly QRCorrectionLevel L = new QRCorrectionLevel("L", 0, 0x01); // up to 7%
    public static readonly QRCorrectionLevel M = new QRCorrectionLevel("M", 1, 0x00); // up to 15%
    public static readonly QRCorrectionLevel Q = new QRCorrectionLevel("Q", 2, 0x03); // up to 25%
    public static readonly QRCorrectionLevel H = new QRCorrectionLevel("H", 3, 0x02); // up to 30%

    public static readonly QRCorrectionLevel[] LEVELS = new [] { L, M, Q, H};


    private QRCorrectionLevel (string name, int ordinal, int markerBits)
    {
      Name = name;
      Ordinal = ordinal;
      MarkerBits = markerBits;
    }

    public readonly string Name;
    public readonly int Ordinal;
    public readonly int MarkerBits;


    public override string ToString() => $"QRECL({Name})";
  }

}
