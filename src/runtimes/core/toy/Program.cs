namespace toy
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      TestBusinessLogic.Toy.ProgramBody.Main(args);
    }
  }
}
