namespace ntc
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Ntc.ProgramBody.Main(args);
    }
  }
}
