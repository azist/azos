/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Security.CAPTCHA
{
   /// <summary>
   /// Defines a module that renders puzzle keypad image
   /// </summary>
   public interface IPuzzleKeypadRenderer : Azos.Apps.IModule
   {
     /// <summary>
     /// Renders default image of the keypad suitable for user entry (i.e. touch or mouse clicks)
     /// </summary>
     Graphics.Image RenderDefaultPuzzleKeypad(PuzzleKeypad keypad, Color? bgColor, bool showRects);
   }

  /// <summary>
  /// Provides methods for generation, storing, and interpretation of user actions with a keypad of random layout.
  /// This .ctor is supplied some code that user has to punch-in(touch/click) on a randomly laid-out keypad which is usually rendered as
  ///  an image. Use DecipherCoordinates() methods to convert user click/touch coordinates into characters
  /// </summary>
  [Serializable]
  public sealed class PuzzleKeypad
  {
    public const int DEFAULT_RENDER_OFFSET_X = 16;
    public const int DEFAULT_RENDER_OFFSET_Y = 16;


    public PuzzleKeypad(string code,
                        string extra = null,
                        int puzzleBoxWidth = 5,
                        int boxWidth = 35,
                        int boxHeight = 50,
                        double boxSizeVariance = 0.3d,
                        int minBoxWidth = 10,
                        int minBoxHeight = 16
                        )
    {
      if (code.IsNullOrWhiteSpace())
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + GetType().FullName + ".ctor(code==null|empty)");
      m_Code = code;

      if (puzzleBoxWidth < 1) puzzleBoxWidth = 1;
      if (minBoxWidth < 8) minBoxWidth = 8;
      if (minBoxHeight < 8) minBoxHeight = 8;

      if (boxWidth < minBoxWidth) boxWidth = minBoxWidth;
      if (boxHeight < minBoxHeight) boxHeight = minBoxHeight;

      boxSizeVariance = Math.Abs(boxSizeVariance);
      if (boxSizeVariance >= 0.5d) boxSizeVariance = 0.5d;

      makePuzzle(Code.Union(extra ?? string.Empty).Distinct().ToArray(),
                  puzzleBoxWidth,
                  boxWidth,
                  boxHeight,
                  boxSizeVariance,
                  minBoxWidth,
                  minBoxHeight);
    }

    private string m_Code;
    private List<CharBox> m_Boxes = new List<CharBox>();

    [Serializable]
    public struct CharBox
    {
      public char Char;
      public Rectangle Rect;
    }


    /// <summary>
    /// Returns the original secret code that this keypad was created for
    /// </summary>
    public string Code { get { return m_Code; } }

    /// <summary>
    /// Returns char boxes
    /// </summary>
    public IEnumerable<CharBox> Boxes { get { return m_Boxes; } }

    /// <summary>
    /// Returns the size of rectangle that covers all boxes
    /// </summary>
    public Size Size
    {
      get { return new Size(m_Boxes.Max(b => b.Rect.Right + 1), m_Boxes.Max(b => b.Rect.Bottom + 1)); }
    }


    /// <summary>
    /// Translates user action coordinates (i.e. screen touches or mouse clicks) into a string.
    /// The coordinates must be supplied as a JSON array of json objects that have '{x: [int], y: [int]}' structure
    /// </summary>
    public string DecipherCoordinates(JsonDataArray coords, int? offsetX = null, int? offsetY = null)
    {
      return DecipherCoordinates(coords.Where(o => o is JsonDataMap)
                                       .Cast<JsonDataMap>()
                                       .Select(jp => new Point(jp["x"].AsInt(), jp["y"].AsInt())),
                                 offsetX, offsetY
                                );
    }

    /// <summary>
    /// Translates user action coordinates (i.e. screen touches or mouse clicks) into a string.
    /// The coordinates are supplied as IEnumerable(Point)
    /// </summary>
    public string DecipherCoordinates(IEnumerable<Point> coords, int? offsetX = null, int? offsetY = null)
    {
      var result = new StringBuilder();

      var ox = offsetX.HasValue ? offsetX.Value : DEFAULT_RENDER_OFFSET_X;
      var oy = offsetY.HasValue ? offsetY.Value : DEFAULT_RENDER_OFFSET_Y;

      foreach (var pnt in coords)
      {
        var op = pnt;
        op.Offset(-ox, -oy);
        var box = m_Boxes.Reverse<CharBox>().FirstOrDefault(b => b.Rect.Contains(op));
        if (box.Char > 0)
          result.Append(box.Char);
      }

      return result.ToString();
    }


    /// <summary>
    /// Renders default image of the keypad suitable for user entry (i.e. touch or mouse clicks).
    /// This method requires IPuzzleKeypadRenderer module to be installed in application module root
    /// </summary>
    public Graphics.Image DefaultRender(IApplication app, Color? bgColor = null, bool showRects = false)
    {
      var mod = app.ModuleRoot.Get<IPuzzleKeypadRenderer>();
      return mod.RenderDefaultPuzzleKeypad(this, bgColor, showRects);
    }



    private void makePuzzle(char[] alphabet,
                            int puzzleBoxWidth,
                            int boxWidth,
                            int boxHeight,
                            double boxSizeVariance,
                            int minBoxWidth,
                            int minBoxHeight)
    {
      alphabet = toss(alphabet);

      double wvar = boxWidth * boxSizeVariance;
      double hvar = boxHeight * boxSizeVariance;
      double wvar2 = wvar / 2d;
      double hvar2 = hvar / 2d;

      var x = 1 + (int)(wvar2 * rnd.NextRandomDouble);
      int ybase = (int)hvar + 1;

      var puzzleWidth = 0;
      var maxHeight = 0;
      foreach (var ch in alphabet)
      {
        int w = boxWidth + (int)(-wvar2 + (wvar * rnd.NextRandomDouble));
        int h = boxHeight + (int)(-hvar2 + (hvar * rnd.NextRandomDouble));

        if (w < minBoxWidth) w = minBoxWidth;
        if (h < minBoxHeight) h = minBoxHeight;

        int y = ybase + (int)(-hvar2 + (hvar * rnd.NextRandomDouble));

        var cbox = new CharBox();
        cbox.Char = ch;
        cbox.Rect = new Rectangle(x, y, w, h);
        m_Boxes.Add(cbox);

        if (cbox.Rect.Bottom - ybase > maxHeight)
        {
          maxHeight = cbox.Rect.Bottom - ybase;
        }

        puzzleWidth++;
        if (puzzleWidth == puzzleBoxWidth)
        {
          puzzleWidth = 0;
          ybase += maxHeight + 2;
          x = 1 + (int)(wvar2 * rnd.NextRandomDouble);
          continue;
        }

        x += w + 2 + (int)(wvar2 * rnd.NextRandomDouble);
      }

    }

    private char[] toss(char[] arr)
    {
      var result = new char[arr.Length];
      for (var i = 0; i < arr.Length; i++)
      {
        var c = arr[i];
        int idx;
        do
          idx = rnd.NextScaledRandomInteger(0, result.Length * 2);
        while (idx > result.Length - 1 || result[idx] > 0);
        result[idx] = c;
      }
      return result;
    }

    private static Platform.RandomGenerator rnd => Platform.RandomGenerator.Instance;

  }

}
