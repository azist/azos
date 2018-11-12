namespace agdida
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Hosts.agdida.ProgramBody.Main(args);
    }
  }
}
