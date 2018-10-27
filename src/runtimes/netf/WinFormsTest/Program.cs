/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Windows.Forms;

using Azos.Apps;

namespace WinFormsTest
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();

      //This line initializes  Azos Application Model dependency injection services container
      //Separate class was needed because Application  class is sealed
      using (new ServiceBaseApplication(args, null))
      {
        Application.EnableVisualStyles();
       // Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new MenuForm());
   //  Application.Run( new ChartForm());
   //    Application.Run( new ChartFormDemo());

      // Application.Run(new Form2());
      //    Application.Run(new MongoDBForm());
//          Application.Run(new LogForm());

       //  Application.Run(new SerializerForm());
       //   Application.Run(new SerializerForm2());

//        Application.Run(new WinFormsTest.Glue.JokeCalculatorClientForm());
         //Application.Run(new SerializerForm());
//         Application.Run(new SerializerForm2());

  //      Application.Run(new GlueForm());
//       Application.Run(new ELinkForm());
   //     Application.Run(new CacheTest());

       // Application.Run(new QRTestForm());

      //  Application.Run(new BlankForm());
      // Application.Run(new WaveServerForm());
     //     Application.Run(new WaveForm());

     // Application.Run(new FIDForm());

     //   Application.Run(new WinFormsTest.ConsoleUtils.ConsoleUtilsFrm());
      }

    //   Application.Run(new BlankForm());
    }

  }
}
