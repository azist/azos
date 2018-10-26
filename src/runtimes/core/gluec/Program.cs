namespace gluec
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Tools.Gluec.ProgramBody.Main(args);
    }
  }
}
