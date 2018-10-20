namespace getdatetime
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Getdatetime.ProgramBody.Main(args);
    }
  }
}
