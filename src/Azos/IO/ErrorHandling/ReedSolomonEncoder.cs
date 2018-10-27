/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;

namespace Azos.IO.ErrorHandling
{
  public class ReedSolomonEncoder
  {
    #region .ctor

    public ReedSolomonEncoder(GaloisField field)
    {
      m_Field = field;
      m_PolySet.Add( new GaloisPolynomial(field, new int[] { 1 }) );
    }

    #endregion

    #region Pvt/Prot/Int Fields

      private GaloisField m_Field;
      private List<GaloisPolynomial> m_PolySet = new List<GaloisPolynomial>();

    #endregion

    #region Public

      public void Encode(int[] src, int errorCorrectionLength)
      {
        if (errorCorrectionLength <= 0)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Encode(errorCorrectionLength)");

         int dataLength = src.Length - errorCorrectionLength;
         if (dataLength <= 0)
           throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Encode: No data bytes provided");

         GaloisPolynomial generationPoly = getPolySet(errorCorrectionLength);
         int[] infoCoefficients = new int[dataLength];
         Array.Copy(src, 0, infoCoefficients, 0, dataLength);

         GaloisPolynomial infoPoly = new GaloisPolynomial(m_Field, infoCoefficients);
         infoPoly = infoPoly.Multiply(errorCorrectionLength, 1);

         GaloisPolynomial remainder, quotient;
         infoPoly.Divide(generationPoly, out quotient, out remainder);
         int[] coefficients = remainder.Coefficients;
         int numZeroCoefficients = errorCorrectionLength - coefficients.Length;
         for (var i = 0; i < numZeroCoefficients; i++)
            src[dataLength + i] = 0;

         Array.Copy(coefficients, 0, src, dataLength + numZeroCoefficients, coefficients.Length);
      }

    #endregion

    #region .pvt. impl.

      private GaloisPolynomial getPolySet(int degree)
      {
        if (degree >= m_PolySet.Count)
        {
          GaloisPolynomial lastPoly = m_PolySet.Last();
          for (int i = m_PolySet.Count; i <= degree; i++)
          {
            GaloisPolynomial newPoly = lastPoly.Multiply( new GaloisPolynomial(m_Field, new int[] {1, m_Field.Exp(i - 1 + m_Field.GeneratorBase)}));
            m_PolySet.Add(newPoly);
            lastPoly = newPoly;
          }
        }
        return m_PolySet[degree];
      }

    #endregion

  }//class

}
