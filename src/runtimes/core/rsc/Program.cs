namespace rsc
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Tools.Rsc.ProgramBody.Main(args);
    }
  }
}
