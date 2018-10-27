/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.IO.ErrorHandling;
using Azos.Scripting;

namespace Azos.Tests.Unit.IO.ErrorHandling
{
  [Runnable]
  public class GaloisPolynomialTest
  {
    [Run]
    public void Divide()
    {
      //System.Diagnostics.Debugger.Launch();

      GaloisField field = new GaloisField(285, 256, 0);

      GaloisPolynomial divident = new GaloisPolynomial(field, new int[] {32, 49, 205, 69, 42, 20, 0, 236, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

      GaloisPolynomial divider = new GaloisPolynomial(field, new int[] {1, 119, 66, 83, 120, 119, 22, 197, 83, 249, 41, 143, 134, 85, 53, 125, 99, 79});

      GaloisPolynomial quotient, remainder;
      divident.Divide(divider, out quotient, out remainder);

      int[] expectedQuotientCoefficients = new int[] { 32, 119, 212, 254, 109, 212, 30, 95, 117};
      int[] expectedRemainderCoefficients = new int[] { 3, 130, 179, 194, 0, 55, 211, 110, 79, 98, 72, 170, 96, 211, 137, 213};

      Aver.AreEqual(expectedQuotientCoefficients.Length, quotient.Coefficients.Length);
      Aver.AreEqual(expectedRemainderCoefficients.Length, remainder.Coefficients.Length);

      for (int i = 0; i < quotient.Coefficients.Length; i++)
        Aver.AreEqual(expectedQuotientCoefficients[i], quotient.Coefficients[i]);

      for (int i = 0; i < remainder.Coefficients.Length; i++)
        Aver.AreEqual(expectedRemainderCoefficients[i], remainder.Coefficients[i]);
    }
  }
}
