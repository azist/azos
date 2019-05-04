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
  public class GuardTests
  {
    [Run]
    public void NonNull()
    {
      object x = 1;

      Aver.AreEqual(1, (int)(x.NonNull()));

      x = null;
      Aver.Throws<CallGuardException>( () => x.NonNull());
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
      catch(CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(  error.Message.Contains("'<unknown>' may not be null") );
      }

      try
      {
        x.NonNull(nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'x' may not be null"));
      }
    }


    [Run]
    public void IsOfType()
    {
      Type x = GetType();

      Aver.IsTrue( GetType() == x.IsOfType<GuardTests>() );
      Aver.IsTrue(GetType() == x.IsOfType<object>());

      x = typeof(AzosException);

      Aver.IsTrue(typeof(AzosException) == x.IsOfType<Exception>());
      Aver.IsTrue(typeof(AzosException) == x.IsOfType<object>());
      Aver.Throws<CallGuardException>(() => x.IsOfType<GuardTests>());

      x = null;
      Aver.Throws<CallGuardException>(() => x.IsOfType<object>());
      x = typeof(Exception);
      Aver.Throws<CallGuardException>(() => x.IsOfType<AzosException>());
    }

    [Run]
    public void IsOfType_2()
    {
      var x = typeof(GuardTests);

      try
      {
        x.IsOfType<Exception>();
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("must be of 'Exception' type"));
      }
    }

    [Run]
    public void NonBlank()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlank());

      x = null;
      Aver.Throws<CallGuardException>(() => x.NonBlank());
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
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'<unknown>' may not be null or blank"));
      }

      try
      {
        x.NonBlank(nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
        Aver.IsTrue(error.Message.Contains("'x' may not be null or blank"));
      }
    }

    [Run]
    public void NonBlankMax()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlankMax(3));

      Aver.Throws<CallGuardException>(() => x.NonBlankMax(2));
      x = null;
      Aver.Throws<CallGuardException>(() => x.NonBlankMax(3));
    }

    [Run]
    public void NonBlankMax_2()
    {
      string x = "abcdef";

      try
      {
        x.NonBlankMax(2, nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
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
        x.NonBlankMin(22, nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
      }
    }

    [Run]
    public void NonBlankMinMax()
    {
      string x = "abc";

      Aver.AreEqual("abc", x.NonBlankMinMax(1,3));

      Aver.Throws<CallGuardException>(() => x.NonBlankMinMax(4,64));
      Aver.Throws<CallGuardException>(() => x.NonBlankMinMax(1, 2));
      x = null;
      Aver.Throws<CallGuardException>(() => x.NonBlankMinMax(1,4));
    }

    [Run]
    public void NonBlankMinMax_2()
    {
      string x = "abcdef";

      try
      {
        x.NonBlankMinMax(48, 64, nameof(x));
        Aver.Fail("ShouldNever be here");
      }
      catch (CallGuardException error)
      {
        Console.WriteLine(error.ToMessageWithType());
      }
    }

  }
}