/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class ConstraintsTests
  {
    [Run]
    public void NonNull()
    {
      object x = 1;

      Aver.AreEqual(1, (int)(x.NonNull()));

      x = null;
      Aver.Throws<AzosException>( () => x.NonNull());
    }

    [Run]
    public void NonNull_2()
    {
      object x = null;

      try
      {
        x.NonNull();
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(  error.Message.Contains("'<unknown>' may not be null") );
      }

      try
      {
        x.NonNull(nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'x' may not be null"));
      }
    }

    [Run]
    public void NonBlank()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlank());

      x = null;
      Aver.Throws<AzosException>(() => x.NonBlank());
    }


    [Run]
    public void NonBlank_2()
    {
      string x = null;

      try
      {
        x.NonBlank();
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'<unknown>' may not be blank"));
      }

      try
      {
        x.NonBlank(nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'x' may not be blank"));
      }
    }

    [Run]
    public void NonBlankMax()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlankMax(3));

      Aver.Throws<AzosException>(() => x.NonBlankMax(2));
      x = null;
      Aver.Throws<AzosException>(() => x.NonBlankMax(3));
    }

    [Run]
    public void NonBlankMax_2()
    {
      string x = "abcdef";

      try
      {
        x.NonBlankMax(2);
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
      }
    }

    [Run]
    public void NonBlankMin()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlankMin(3));

      Aver.Throws<AzosException>(() => x.NonBlankMin(4));
      x = null;
      Aver.Throws<AzosException>(() => x.NonBlankMin(4));
    }

    [Run]
    public void NonBlankMin_2()
    {
      string x = "abcdef";

      try
      {
        x.NonBlankMin(22);
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
      }
    }

    [Run]
    public void NonBlankMinMax()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlankMinMax(1,3));

      Aver.Throws<AzosException>(() => x.NonBlankMinMax(4,64));
      Aver.Throws<AzosException>(() => x.NonBlankMinMax(1, 2));
      x = null;
      Aver.Throws<AzosException>(() => x.NonBlankMinMax(1,4));
    }

    [Run]
    public void NonBlankMinMax_2()
    {
      string x = "abcdef";

      try
      {
        x.NonBlankMinMax(48, 64);
        Aver.Fail("ShouldNever be here");
      }
      catch (Exception error) when (!(error is AvermentException))
      {
        Console.WriteLine(error.ToMessageWithType());
      }
    }

  }
}