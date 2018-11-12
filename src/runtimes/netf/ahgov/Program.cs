namespace ahgov
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Hosts.ahgov.ProgramBody.Main(args);
    }
  }
}
