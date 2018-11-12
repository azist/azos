namespace amm
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Tools.amm.ProgramBody.Main(args);
    }
  }
}
