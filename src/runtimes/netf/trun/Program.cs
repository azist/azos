namespace trun
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Trun.ProgramBody.Main(args);
    }
  }
}
