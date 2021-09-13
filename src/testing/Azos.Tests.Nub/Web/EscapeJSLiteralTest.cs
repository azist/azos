/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

namespace Azos.Tests.Nub.Web
{
  [Runnable]
  public class EscapeJSLiteralTest
  {
    [Run]
    public void Empty()
    {
      Aver.AreEqual(null, WebUtils.EscapeJSLiteral(null));
      Aver.AreEqual("", WebUtils.EscapeJSLiteral(""));
      Aver.AreEqual("   ", WebUtils.EscapeJSLiteral("   "));
    }

    [Run]
    public void Quotes()
    {
      Aver.AreEqual(@"Mc\x27Cloud", WebUtils.EscapeJSLiteral("Mc'Cloud"));
      Aver.AreEqual(@"Mc\x22Cloud", WebUtils.EscapeJSLiteral("Mc\"Cloud"));
      Aver.AreEqual(@"Mc\x22\x27Cloud", WebUtils.EscapeJSLiteral("Mc\"'Cloud"));

    }

    [Run]
    public void Script()
    {
      Aver.AreEqual(@"not \x3C\x2Fscript\x3E the end", WebUtils.EscapeJSLiteral("not </script> the end"));
    }

    [Run]
    public void RN()
    {
      Aver.AreEqual(@"not \x0D \x0A the end", WebUtils.EscapeJSLiteral("not \r \n the end"));
    }

    [Run]
    public void Various()
    {
      Aver.AreEqual(@"not\x27s \x22\x26amp;\x22 the\x5Cs\x2F end", WebUtils.EscapeJSLiteral(@"not's ""&amp;"" the\s/ end"));
    }
  }
}
