namespace toy
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      TestBusinessLogic.Toy.ProgramBody.Main(args);
    }
  }
}
