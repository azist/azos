/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Azos.Text;
using Azos.Security;


namespace Azos.IO.Console
{
  /// <summary>
  /// Provides various console-helper utilities
  /// </summary>
  public static class ConsoleUtils
  {
    /// <summary>
    /// Reads password from console displaying substitute characters instead of real ones
    /// </summary>
    public static string ReadPassword(char substitute)
    {
      string buff = string.Empty;

      while (true)
      {
        char c = System.Console.ReadKey(true).KeyChar;
        if (Char.IsControl(c)) return buff;
        buff += c;

        if (substitute != (char)0)
          System.Console.Write(substitute);

      }
    }

    /// <summary>
    /// Reads password from console displaying substitute characters instead of real ones
    /// into a sealed SecureBuffer
    /// </summary>
    public static SecureBuffer ReadPasswordToSecureBuffer(char substitute)
    {
      var result = new SecureBuffer();

      while (true)
      {
        char c = System.Console.ReadKey(true).KeyChar;
        if (Char.IsControl(c))
        {
          result.Seal();
          return result;
        }

        var buf = Encoding.UTF8.GetBytes(new char[]{ c });

        for(var i=0; i< buf.Length; i++)
          result.Push(buf[i]);

        Array.Clear(buf, 0, buf.Length);

        if (substitute != (char)0)
          System.Console.Write(substitute);

      }
    }


    /// <summary>
    /// Outputs colored text from content supplied in an HTML-like grammar:
    /// </summary>
    public static void WriteMarkupContent(string content)
    {
      WriteMarkupContent(content, '<', '>');
    }


    private enum direction { Left, Center, Right };


    public static string WriteMarkupContentAsHTML(
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
      using( var str = new System.IO.StringWriter())
      {
        WriteMarkupContentAsHTML(str, content, encoding, open, close, mkpPRE, mkpCssForeColorPrefix, mkpCssBackColorPrefix, defaultForeColor, defaultBackColor);
        return str.ToString();
      }
    }

    public static void WriteMarkupContentAsHTML(System.IO.Stream output,
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
      using(var wri = new System.IO.StreamWriter(output, encoding ?? Encoding.UTF8, 1024, true))
	    {
        WriteMarkupContentAsHTML(wri, content, encoding, open, close, mkpPRE, mkpCssForeColorPrefix, mkpCssBackColorPrefix, defaultForeColor, defaultBackColor);
      }
    }


    public static void WriteMarkupContentAsHTML(System.IO.TextWriter output,
                                                string content,
                                                Encoding encoding = null,  // UTF8 if null
                                                char open = '<', char close = '>',
                                                string mkpPRE = "pre",
                                                string mkpCssForeColorPrefix = "conForeColor_",
                                                string mkpCssBackColorPrefix = "conBackColor_",
                                                ConsoleColor defaultForeColor = ConsoleColor.White,
                                                ConsoleColor defaultBackColor = ConsoleColor.Black)
    {
       // [mkpPRE]<span class='conColor_red'>This string will be red</span>[/mkpPRE]
        if (mkpPRE.IsNotNullOrWhiteSpace())
        {
          output.Write('<');
          output.Write(mkpPRE);
          output.Write('>');
        }

        TokenParser parser = new TokenParser(content, open, close);
        Stack<ConsoleColor> stack = new Stack<ConsoleColor>();

        bool collapsespaces = false;
        ConsoleColor foreColor = defaultForeColor, backColor = defaultBackColor;
        bool isSpanOpen = false;

        foreach (TokenParser.Token tok in parser)
        {
          if (tok.IsSimpleText)
          {
            if (collapsespaces)
              output.Write(WebUtility.HtmlEncode(tok.Content.Trim()));
            else
              output.Write(WebUtility.HtmlEncode(tok.Content));
            continue;
          }

          string name = tok.Name.ToUpperInvariant().Trim();


          if (name == "LITERAL")
          {
            collapsespaces = false;
            continue;
          }

          if (name == "HTML")
          {
            collapsespaces = true;
            continue;
          }

          if (name == "BR")
          {
            output.WriteLine();
            continue;
          }

          if ((name == "SP") || (name == "SPACE"))
          {
            string txt = " ";
            int cnt = 1;

            try { cnt = int.Parse(tok["COUNT"]); }
            catch { }

            while (txt.Length < cnt) txt += " ";

            output.Write(WebUtility.HtmlEncode(txt));
            continue;
          }

          if (name == "PUSH")
          {
            stack.Push(foreColor);
            stack.Push(backColor);
            continue;
          }

          if (name == "POP")
          {
            if (stack.Count > 1)
            {
              backColor = stack.Pop();
              foreColor = stack.Pop();
              if (isSpanOpen)
              {
                isSpanOpen = false;
                output.Write("</span>");
                if (stack.Count > 1)
                {
                  isSpanOpen = true;
                  output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
                }
              }
            }
            continue;
          }

          if (name == "RESET")
          {
            foreColor = defaultForeColor;
            if (isSpanOpen)
            {
              isSpanOpen = false;
              output.Write("</span>");
            }

            continue;
          }


          if ((name == "F") || (name == "FORE") || (name == "FOREGROUND"))
          {
            try
            {
              ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
              foreColor = clr;

              if (isSpanOpen) output.Write("</span>");
              output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
              isSpanOpen = true;
            }
            catch { }
            continue;
          }

          if ((name == "B") || (name == "BACK") || (name == "BACKGROUND"))
          {
            try
            {
              ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
              backColor = clr;

              if (isSpanOpen) output.Write("</span>");
              output.Write("<span class='{0}{1} {2}{3}'>".Args(mkpCssForeColorPrefix, foreColor, mkpCssBackColorPrefix, backColor));
              isSpanOpen = true;
            }
            catch { }
            continue;
          }

          if ((name == "J") || (name == "JUST") || (name == "JUSTIFY"))
          {
            try
            {
              int width = int.Parse(tok["WIDTH"]);
              direction dir = (direction)Enum.Parse(typeof(direction), tok["DIR"], true);
              string txt = tok["TEXT"];


              switch (dir)
              {
                case direction.Right:
                  {
                    while (txt.Length < width) txt = " " + txt;
                    break;
                  }
                case direction.Center:
                  {
                    while (txt.Length < width) txt = " " + txt + " ";
                    if (txt.Length > width) txt = txt.Substring(0, txt.Length - 1);
                    break;
                  }
                default:
                  {
                    while (txt.Length < width) txt = txt + " ";
                    break;
                  }
              }

              output.Write(WebUtility.HtmlEncode(txt));
            }
            catch { }
            continue;
          }
        }

        if (mkpPRE.IsNotNullOrWhiteSpace())
        {
          output.Write('<');
          output.Write('/');
          output.Write(mkpPRE);
          output.Write('>');
        }
    }


    /// <summary>
    /// Outputs colored text from content supplied in an HTML-like grammar
    /// </summary>
    public static void WriteMarkupContent(string content, char open, char close) => WriteMarkupContent(LocalConsolePort.Default.DefaultConsole, content, open, close);


    /// <summary>
    /// Outputs colored text from content supplied in an HTML-like grammar
    /// </summary>
    public static void WriteMarkupContent(IConsoleOut cout, string content, char open, char close)
    {
      cout.NonNull(nameof(cout));

      TokenParser parser = new TokenParser(content, open, close);
      Stack<ConsoleColor> stack = new Stack<ConsoleColor>();

      bool collapsespaces = false;


      foreach (TokenParser.Token tok in parser)
      {
        if (tok.IsSimpleText)
        {
          if (collapsespaces)
            cout.Write(tok.Content.Trim());
          else
            cout.Write(tok.Content);
          continue;
        }

        string name = tok.Name.ToUpperInvariant().Trim();


        if (name == "LITERAL")
        {
          collapsespaces = false;
          continue;
        }

        if (name == "HTML")
        {
          collapsespaces = true;
          continue;
        }



        if (name == "BR")
        {
          cout.WriteLine();
          continue;
        }

        if ((name == "SP") || (name == "SPACE"))
        {
          string txt = " ";
          int cnt = 1;

          try { cnt = int.Parse(tok["COUNT"]); }
          catch { }

          while (txt.Length < cnt) txt += " ";

          cout.Write(txt);
          continue;
        }

        if (name == "PUSH")
        {
          stack.Push(cout.ForegroundColor);
          stack.Push(cout.BackgroundColor);
          continue;
        }

        if (name == "POP")
        {
          if (stack.Count > 1)
          {
            cout.BackgroundColor = stack.Pop();
            cout.ForegroundColor = stack.Pop();
          }
          continue;
        }

        if (name == "RESET")
        {
          cout.ResetColor();
          continue;
        }


        if ((name == "F") || (name == "FORE") || (name == "FOREGROUND"))
        {
          try
          {
            ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
            cout.ForegroundColor = clr;
          }
          catch { }
          continue;
        }

        if ((name == "B") || (name == "BACK") || (name == "BACKGROUND"))
        {
          try
          {
            ConsoleColor clr = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tok["COLOR"], true);
            cout.BackgroundColor = clr;
          }
          catch { }
          continue;
        }


        if ((name == "J") || (name == "JUST") || (name == "JUSTIFY"))
        {
          try
          {
            int width = int.Parse(tok["WIDTH"]);
            direction dir = (direction)Enum.Parse(typeof(direction), tok["DIR"], true);
            string txt = tok["TEXT"];


            switch (dir)
            {
              case direction.Right:
                {
                  while (txt.Length < width) txt = " " + txt;
                  break;
                }
              case direction.Center:
                {
                  while (txt.Length < width) txt = " " + txt + " ";
                  if (txt.Length > width) txt = txt.Substring(0, txt.Length - 1);
                  break;
                }
              default:
                {
                  while (txt.Length < width) txt = txt + " ";
                  break;
                }
            }

            cout.Write(txt);
          }
          catch { }
          continue;
        }

      }

    }

    /// <summary>
    /// Shows message with colored error header
    /// </summary>
    public static void Error(string msg, int ln = 0) => Error(LocalConsolePort.Default.DefaultConsole, msg, ln);

    /// <summary>
    /// Shows message with colored error header
    /// </summary>
    public static void Error(IConsoleOut cout, string msg, int ln = 0)
    {
      var f = cout.NonNull(nameof(cout)).ForegroundColor;
      var b = cout.BackgroundColor;


      cout.ForegroundColor = ConsoleColor.Black;
      cout.BackgroundColor = ConsoleColor.Red;
        if (ln==0)
        cout.Write("ERROR:");
        else
        cout.Write("{0:D3}-ERROR:".Args(ln));
      cout.BackgroundColor = ConsoleColor.Black;
      cout.ForegroundColor = ConsoleColor.Red;
      cout.WriteLine(" "+msg);

      cout.ForegroundColor = f;
      cout.BackgroundColor = b;
    }


    /// <summary>
    /// Shows message with colored warning header
    /// </summary>
    public static void Warning(string msg, int ln = 0) => Warning(LocalConsolePort.Default.DefaultConsole, msg, ln);

    /// <summary>
    /// Shows message with colored warning header
    /// </summary>
    public static void Warning(IConsoleOut cout, string msg, int ln = 0)
    {
      var f = cout.NonNull(nameof(cout)).ForegroundColor;
      var b = cout.BackgroundColor;

      cout.ForegroundColor = ConsoleColor.Black;
      cout.BackgroundColor = ConsoleColor.Yellow;
        if (ln==0)
        cout.Write("WARNING:");
        else
        cout.Write("{0:D3}-WARNING:".Args(ln));
      cout.BackgroundColor = ConsoleColor.Black;
      cout.ForegroundColor = ConsoleColor.Yellow;
      cout.WriteLine(" "+msg);

      cout.ForegroundColor = f;
      cout.BackgroundColor = b;
    }


    /// <summary>
    /// Shows message with colored info header
    /// </summary>
    public static void Info(string msg, int ln = 0) => Info(LocalConsolePort.Default.DefaultConsole, msg, ln);

    /// <summary>
    /// Shows message with colored info header
    /// </summary>
    public static void Info(IConsoleOut cout, string msg, int ln = 0)
    {
      var f = cout.NonNull(nameof(cout)).ForegroundColor;
      var b = cout.BackgroundColor;

      cout.ForegroundColor = ConsoleColor.Black;
      cout.BackgroundColor = ConsoleColor.Green;
        if (ln==0)
        cout.Write("Info:");
        else
        cout.Write("{0:D3}-Info:".Args(ln));
      cout.BackgroundColor = ConsoleColor.Black;
      cout.ForegroundColor = ConsoleColor.Gray;
      cout.WriteLine(" "+msg);

      cout.ForegroundColor = f;
      cout.BackgroundColor = b;
    }

  }//class
}
