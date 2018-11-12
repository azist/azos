namespace aws
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      Azos.Sky.Hosts.aws.ProgramBody.Main(args);
    }
  }
}
