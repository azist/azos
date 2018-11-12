namespace ascon
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Sky.Tools.ascon.ProgramBody.Main(args);
    }
  }
}
