namespace ascon
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Tools.ascon.ProgramBody.Main(args);
    }
  }
}
