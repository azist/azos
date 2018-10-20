namespace rsc
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Rsc.ProgramBody.Main(args);
    }
  }
}
