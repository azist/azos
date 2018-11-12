namespace azgov
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Hosts.azgov.ProgramBody.Main(args);
    }
  }
}
