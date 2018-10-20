
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
