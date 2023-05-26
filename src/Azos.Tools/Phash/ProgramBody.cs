/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Apps;
using Azos.IO.Console;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Platform;
using Azos.Conf;
using System.Linq;

namespace Azos.Tools.Phash
{
  /// <summary>
  /// Program entry point for PASSWORD HASH generator tool
  /// </summary>
  [Platform.ProcessActivation.ProgramBody("phash,pwd", Description = "Security password hash generator tool")]
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {

        using(var app = new AzosApplication(args, null))
          run(app);

        System.Environment.ExitCode = 0;
      }
      catch(Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        System.Environment.ExitCode = -1;
      }
    }

    private static void run(IApplication app)
    {
      var args = app.CommandArgs;

      if (args["list"].Exists)
      {
        Console.WriteLine(" #   Type  (name)");
        Console.WriteLine("--------------------------------");
        app.SecurityManager.PasswordManager.Algorithms.ForEach( (alg, i) =>
          Console.WriteLine("[{0}]  {1}(`{2}`)", i, alg.GetType().DisplayNameWithExpandedGenericArgs(), alg.Name)
        );
        Console.WriteLine();
        return;
      }


      var pretty = args["pp", "pretty"].Exists;
      var noEntropy = args["ne", "noentropy"].Exists;
      var scoreThreshold = args["st", "score"].AttrByIndex(0).ValueAsInt(80);
      if (scoreThreshold < 20) scoreThreshold = 20;
      if (scoreThreshold > 100) scoreThreshold = 100;
      var strength = args["lvl", "level"].AttrByIndex(0).ValueAsEnum<PasswordStrengthLevel>(PasswordStrengthLevel.Default);
      var algname = args["alg", "algo", "algorithm"].AttrByIndex(0).Value;

      ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

      if (args["?", "h", "help"].Exists)
      {
        ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
        return;
      }

      if (!noEntropy) getEntropy();

      var keySwitch = args["k", "key"];
      if (keySwitch.Exists)
       doKey(app,keySwitch);
      else
       doPassword(app, pretty, scoreThreshold, strength, algname);
    }

    private static void doKey(IApplication app, IConfigSectionNode keySwitch)
    {
      var bitLength = keySwitch.AttrByIndex(0).ValueAsInt(256);

      if (bitLength < 8 || bitLength % 8 != 0)
      {
        ConsoleUtils.Error("Invalid key bit length: {0}; would use 256 bits instead".Args(bitLength));
        bitLength = 256;
      }

      var count = bitLength / 8;
      ConsoleUtils.Info("Key of {0} bits/{1} bytes is generated".Args(bitLength, count));

      var key = app.SecurityManager.Cryptography.GenerateRandomBytes(count);
      Console.WriteLine("As BASE64: \"base64:{0}\"".Args(key.ToWebSafeBase64()));
      Console.WriteLine("As HEX: \"{0}\"".Args(string.Join(",", key.Select(b => b.ToString("X2")))));
    }

    private static void doPassword(IApplication app, bool pretty, int scoreThreshold, PasswordStrengthLevel strength, string algname)
    {

      ConsoleUtils.Info("Score Threshold: {0}%".Args(scoreThreshold));
      ConsoleUtils.Info("Strength level: {0}".Args(strength));

      SecureBuffer password = null;

      while (true)
      {
        Console.WriteLine("Please type-in your password and press <enter>:");
        password = ConsoleUtils.ReadPasswordToSecureBuffer('*');
        var score = app.SecurityManager.PasswordManager.CalculateStrenghtPercent(PasswordFamily.Text, password);
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

      while (true)
      {
        Console.WriteLine("Please re-type your password and press <enter>:");
        using (var p2 = ConsoleUtils.ReadPasswordToSecureBuffer('*'))
          if (password.Content.MemBufferEquals(p2.Content)) break;
        ConsoleUtils.Error("Passwords do not match");
      }

      Console.WriteLine();
      Console.WriteLine();

      HashedPassword hashed = null;

      if (algname.IsNotNullOrWhiteSpace())
      {
        var alg = app.SecurityManager.PasswordManager.Algorithms[algname];
        if (alg != null)
          hashed = alg.ComputeHash(PasswordFamily.Text, password);
        else
          ConsoleUtils.Error("Specified algorithm not found. Using default...");
      }

      if (hashed == null)
      {
        hashed = app.SecurityManager.PasswordManager.ComputeHash(
                                  PasswordFamily.Text,
                                  password,
                                  strength);
      }

      password.Dispose();

      var toPrint = JsonWriter.Write(hashed, pretty ? JsonWritingOptions.PrettyPrintASCII : JsonWritingOptions.CompactASCII);

      Console.WriteLine("Hashed Password:");
      Console.WriteLine();

      Console.WriteLine(toPrint);
    }

    private static void getEntropy()
    {
      var count = Ambient.Random.NextScaledRandomInteger(47, 94);
      ConsoleUtils.Info("Acquiring entropy from user...");
      Console.WriteLine();
      ConsoleUtils.WriteMarkupContent(
@"<push>
<f color=magenta>Please make <f color=white>{0}<f color=magenta> random keystrokes
Do not hit the same key and try to space key presses in time:<pop>
".Args(count));

      var pnow = Stopwatch.GetTimestamp();

      Console.WriteLine();
      for (var i = 0; i < count; i++)
      {
        var k = Console.ReadKey(true).KeyChar;
        if (k < 0x20) continue;
        var now = Stopwatch.GetTimestamp();
        var elapsed = (int)(39621 * (k - 0x19) * (now - pnow));
        pnow = now;
        Ambient.Random.FeedExternalEntropySample(elapsed);
        Console.Write("\r{0}  {1} characters to go ...", elapsed, count - i - 1);
      }
      ConsoleUtils.Info("OK. Entropy key entered");
      Console.WriteLine("-----------------------");
      System.Threading.Thread.Sleep(3000);
      while (Console.KeyAvailable) Console.ReadKey(true);
    }
  }
}
