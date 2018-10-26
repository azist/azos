namespace inventory
{
    class Program
    {
        static void Main(string[] args)
        {
          new NFX.PAL.NetCore20.NetCore20Runtime();
          NFX.Tools.Inventory.ProgramBody.Main(args);
        }
    }
}
