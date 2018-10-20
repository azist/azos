namespace toy
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      BusinessLogic.Toy.ProgramBody.Main(args);
    }
  }
}
