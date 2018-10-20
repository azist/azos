namespace gluec
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      Azos.Tools.Gluec.ProgramBody.Main(args);
    }
  }
}
