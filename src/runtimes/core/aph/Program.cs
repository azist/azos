namespace aph
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Hosts.aph.ProgramBody.Main(args);
    }
  }
}
