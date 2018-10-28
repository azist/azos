/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
