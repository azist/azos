/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace TestBusinessLogic.Server
{
    /// <summary>
    /// Provides implementation for IJokeCalculator used for testing
    /// </summary>
    [Azos.Glue.ThreadSafe]
    public class JokeCalculatorServer : IJokeCalculator
    {
        private int m_Value; //state is retained between calls


        public void Init(int value)
        {
           m_Value = value;
        }

        public int Add(int value)
        {
          m_Value += value;
          return m_Value;
        }

        public int Sub(int value)
        {
          m_Value -= value;
          return m_Value;
        }

        public int Done()
        {
          return m_Value;
        }
    }
}
