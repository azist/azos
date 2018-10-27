/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Apps;
using Azos.IO;
using Azos.Conf;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Platform;


namespace Azos.Tools.Phash
{
    public static class ProgramBody
    {
        public static void Main(string[] args)
        {
          try
          {

           using(var app = new ServiceBaseApplication(args, null))
            run(app.CommandArgs);

           System.Environment.ExitCode = 0;
          }
          catch(Exception error)
          {
           ConsoleUtils.Error(error.ToMessageWithType());
           System.Environment.ExitCode = -1;
          }
        }

        private static void run(IConfigSectionNode args)
        {
          var pretty = args["pp", "pretty"].Exists;
          var noEntropy = args["ne", "noentropy"].Exists;
          var scoreThreshold = args["st", "score"].AttrByIndex(0).ValueAsInt(80);
          if (scoreThreshold<20) scoreThreshold = 20;
          if (scoreThreshold>100) scoreThreshold = 100;
          var strength = args["lvl","level"].AttrByIndex(0).ValueAsEnum<PasswordStrengthLevel>(PasswordStrengthLevel.Default);

          ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Welcome.txt") );

          if (args["?", "h", "help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
             return;
          }

          ConsoleUtils.Info("Score Threshold: {0}%".Args(scoreThreshold));
          ConsoleUtils.Info("Stength level: {0}".Args(strength));

          if (!noEntropy)
          {
             var count = App.Random.NextScaledRandomInteger(47, 94);
             ConsoleUtils.Info("Acquiring entropy from user...");
             Console.WriteLine();
             ConsoleUtils.WriteMarkupContent(
@"<push>
<f color=magenta>Please make <f color=white>{0}<f color=magenta> random keystrokes
Do not hit the same key and try to space key presses in time:<pop>
".Args(count));

             var pnow = Stopwatch.GetTimestamp();

             Console.WriteLine();
             for(var i=0; i<count;i++)
             {
               var k = Console.ReadKey(true).KeyChar;
               if (k<0x20) continue;
               var now = Stopwatch.GetTimestamp();
               var elapsed = (int)(39621 * (k - 0x19) * (now - pnow));
               pnow = now;
               App.Random.FeedExternalEntropySample(elapsed);
               Console.Write("\r{0}  {1} characters to go ...", elapsed, count-i-1);
             }
             ConsoleUtils.Info("OK. Entropy key entered");
             Console.WriteLine("-----------------------");
             System.Threading.Thread.Sleep(3000);
             while( Console.KeyAvailable) Console.ReadKey(true);
          }

          SecureBuffer password = null;

          while(true)
          {
             Console.WriteLine("Please type-in your password and press <enter>:");
             password = ConsoleUtils.ReadPasswordToSecureBuffer('*');
             var score = App.SecurityManager.PasswordManager.CalculateStrenghtPercent(PasswordFamily.Text, password);
             var pass = score >= scoreThreshold;
             Console.WriteLine();
             var t = "Password score: {0}% is {1} strong".Args(score, pass ? "sufficiently" : "insufficiently");
             if (pass)
             {
               ConsoleUtils.Info(t);
               break;
             }

             ConsoleUtils.Error(t);
             Console.WriteLine();
          }

          Console.WriteLine();

          while(true)
          {
            Console.WriteLine("Please re-type your password and press <enter>:");
            using(var p2 = ConsoleUtils.ReadPasswordToSecureBuffer('*'))
              if (password.Content.MemBufferEquals(p2.Content)) break;
            ConsoleUtils.Error("Passwords do not match");
          }

          Console.WriteLine();
          Console.WriteLine();

          var hashed = App.SecurityManager.PasswordManager.ComputeHash(
                                    Azos.Security.PasswordFamily.Text,
                                    password,
                                    strength);

          password.Dispose();

          var toPrint = JSONWriter.Write(hashed, pretty ? JSONWritingOptions.PrettyPrintASCII : JSONWritingOptions.CompactASCII);

          Console.WriteLine("Hashed Password:");
          Console.WriteLine();

          Console.WriteLine( toPrint );
        }
    }
}
