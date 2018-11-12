namespace aph
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Hosts.aph.ProgramBody.Main(args);
    }
  }
}
