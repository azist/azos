/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
