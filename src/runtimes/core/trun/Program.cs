namespace trun
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Tools.Trun.ProgramBody.Main(args);
    }
  }
}
