namespace ash
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Hosts.ash.ProgramBody.Main(args);
    }
  }
}
