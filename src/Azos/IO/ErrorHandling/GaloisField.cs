/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.IO.ErrorHandling
{
  public class GaloisField
  {
    #region CONSTS

      public static readonly GaloisField QRCODE_256 = new GaloisField(0x011D, 256, 0); // polynomial: x^8 + x^4 + x^3 + x^2 + 1

      private const int INIT_THRESHOLD = 0;

    #endregion

    #region .ctor

      public GaloisField(int primitive, int size, int generatorBase)
      {
        m_Primitive = primitive;
        m_Size = size;
        m_GeneratorBase = generatorBase;

        if (size <= INIT_THRESHOLD)
          initialize();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private int[] m_ExpTable;
      private int[] m_LogTable;
      private GaloisPolynomial m_Polynomial0;
      private GaloisPolynomial m_Polynomial1;
      private readonly int m_Size;
      private readonly int m_Primitive;
      private readonly int m_GeneratorBase;
      private bool m_Initialized;

    #endregion

    #region Properties

      public int GeneratorBase
      {
        get { return m_GeneratorBase;}
      }

      public int Size
      {
        get { return m_Size;}
      }

      public GaloisPolynomial Polynomial0
      {
        get
        {
          ensureInitialized();
          return m_Polynomial0;
        }
      }

      public GaloisPolynomial Polynomial1
      {
        get
        {
          ensureInitialized();
          return m_Polynomial1;
        }
      }

    #endregion

    #region Public

      public GaloisPolynomial GenerateMonomial( int degree, int coefficient)
      {
        ensureInitialized();

        if (coefficient == 0)
          return m_Polynomial0;

        int[] coefficients = new int[degree + 1];
        coefficients[0] = coefficient;

        GaloisPolynomial polynomial = new GaloisPolynomial(this, coefficients);
        return polynomial;
      }

      public int Exp(int a)
      {
        ensureInitialized();

        return m_ExpTable[a];
      }

      public int Log(int a)
      {
        if (a <= 0)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Log(a>0)");

        ensureInitialized();

        return m_LogTable[a];
      }

      public int Inverse(int a)
      {
        if (a <= 0)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Inverse(a>0)");

        ensureInitialized();

        return m_ExpTable[m_Size - m_LogTable[a] - 1];
      }

      public static int Add(int a, int b)
      {
         return a ^ b;
      }

      public int Multiply(int a, int b)
      {
        ensureInitialized();

        if (a == 0 || b == 0)
          return 0;

        int logSum = m_LogTable[a] + m_LogTable[b];
        int expIndex = logSum % (m_Size - 1);
        return m_ExpTable[expIndex];
      }

    #endregion

    #region .pvt. impl.

      private void initialize()
      {
        m_ExpTable = new int[m_Size];
        m_LogTable = new int[m_Size];

        int x = 1;
        for (int i = 0; i < m_Size; i++)
        {
          m_ExpTable[i] = x;
          x <<= 1;
          if (x >= m_Size)
          {
            x ^= m_Primitive;
            x &= m_Size - 1;
          }
        }

        for (int i = 0; i < m_Size-1; i++)
        {
          m_LogTable[m_ExpTable[i]] = i;
        }

        m_Polynomial0 = new GaloisPolynomial(this, new int[] { 0 });
        m_Polynomial1 = new GaloisPolynomial(this, new int[] { 1 });

        m_Initialized = true;
      }

      private void ensureInitialized()
      {
        if (!m_Initialized)
          initialize();
      }

    #endregion
  }
}
