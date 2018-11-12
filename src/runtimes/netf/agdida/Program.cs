namespace agdida
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Hosts.agdida.ProgramBody.Main(args);
    }
  }
}
