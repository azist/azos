namespace phash
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Phash.ProgramBody.Main(args);
    }
  }
}
