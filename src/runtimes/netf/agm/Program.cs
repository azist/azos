namespace agm
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Tools.agm.ProgramBody.Main(args);
    }
  }
}
