namespace ash
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Hosts.ash.ProgramBody.Main(args);
    }
  }
}
