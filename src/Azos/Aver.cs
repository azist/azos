/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Azos
{
  /// <summary>
  /// Provides basic averments for test construction. May call Aver.Fail(msg) manually
  /// </summary>
  public static class Aver
  {
    /// <summary>
    /// Aver that method throws an exception of type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ThrowsAttribute : Attribute
    {
      public enum MatchType{Contains = 0, Exact}

      public ThrowsAttribute(){}
      public ThrowsAttribute(Type expected){ ExceptionType = expected; }
      public ThrowsAttribute(Type expected, string msg){ ExceptionType = expected; Message = msg; }

      public Type ExceptionType { get; set; }
      public bool ExactType     { get; set; }
      public string Message     { get; set; }
      public MatchType MsgMatch { get; set; }


      /// <summary>
      /// Checks to see if the method has Throws attribute specified and if it does whether the error matches the required condition.
      /// Returns true if the error clause is specified and matches, false if the Throws is not specified, throws on mismatch
      /// </summary>
      public static bool CheckMethodError(MethodInfo method, Exception error, string from = null)
      {
        if (method==null) return false;
        var attr = method.GetCustomAttribute<ThrowsAttribute>(false);
        if (attr==null) return false;

        //Throws is specified, error must not be null
        if (error==null)
          throw new AvermentException(StringConsts.AVER_THROWS_NOT_THROWN_ERROR.Args(
                                        method.Name,
                                        nameof(ThrowsAttribute)), from, error);

        if (
            attr.ExceptionType!=null &&
            (
             (attr.ExactType && error.GetType() != attr.ExceptionType) ||
             (!attr.ExactType && !attr.ExceptionType.IsAssignableFrom(error.GetType()))
            )
           )
          throw new AvermentException(StringConsts.AVER_THROWS_TYPE_MISMATCH_ERROR.Args(
                                        method.Name,
                                        attr.ExceptionType.DisplayNameWithExpandedGenericArgs(),
                                        error.GetType().DisplayNameWithExpandedGenericArgs()), from, error);

        if (attr.Message.IsNotNullOrWhiteSpace())
        {
          var m = false;
          var em = error.Message;
          if (em.IsNotNullOrWhiteSpace())
          {
            if (attr.MsgMatch==MatchType.Exact)
             m = attr.Message.EqualsOrdSenseCase(em);
            else
             m = em.IndexOf(attr.Message, StringComparison.Ordinal) >= 0;
          }

          if (!m)
           throw new AvermentException(StringConsts.AVER_THROWS_MSG_MISMATCH_ERROR.Args(
                                        method.Name,
                                        attr.MsgMatch,
                                        attr.Message,
                                        error.Message), from, error);
        }

        return true;
      }

    }

    /// <summary>
    /// Fails averment by throwing AvermentException
    /// </summary>
    public static void Fail(string message, string from = null)
    {
      throw new AvermentException(message, from);
    }

    public static void Pass(string message = null, string from = null)
    {
      //todo: Need to tell runner context that everything is OK?
    }

    #region AreEqual/AreNotEqual

      /// <summary>
      /// Test for equality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static bool AreObjectEqualTest(object expect, object got)
      {
        if (expect==null && got==null) return true;
        if (expect==null) return false;

        return expect.Equals(got);
      }

      /// <summary>
      /// Test for equality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static void AreObjectsEqual(object expect, object got, string from = null)
      {
        if (!AreObjectEqualTest(expect, got)) Fail("AreObjectsEqual({0}, {1})".args(expect, got), from);
      }

      /// <summary>
      /// Test for inequality via object.Equals()only disregarding all other possible equality comparers like IEquatable etc...
      /// </summary>
      public static void AreObjectsNotEqual(object expect, object got, string from = null)
      {
        if (AreObjectEqualTest(expect, got)) Fail("AreObjectsNotEqual({0}, {1})".args(expect, got), from);
      }


      public static void AreEqual(string expect, string got, string from = null)
      {
        AreEqual(expect, got, StringComparison.InvariantCulture, from);
      }

      public static void AreEqual(string expect, string got, StringComparison comparison, string from = null)
      {
        if (!string.Equals(expect, got, comparison)) Fail("AreEqual({0}, {1}, {2})".args(expect, got, comparison), from);
      }

      public static void AreNotEqual(string expect, string got, string from = null)
      {
        AreNotEqual(expect, got, StringComparison.InvariantCulture, from);
      }

      public static void AreNotEqual(string expect, string got, StringComparison comparison, string from = null)
      {
        if (string.Equals(expect, got, comparison)) Fail("AreNotEqual({0}, {1}, {2})".args(expect, got, comparison), from);
      }

      public static void AreEqual   <T>(T expect, T got, string from = null) where T : IEquatable<T>
      { if (!expect.Equals(got)) Fail("AreEqual({0}, {1})"   .args(expect, got), from); }

      public static void AreNotEqual<T>(T expect, T got, string from = null) where T : IEquatable<T>
      { if (expect.Equals(got))  Fail("AreNotEqual({0}, {1})".args(expect, got), from); }



      public static bool AreEqualTest<T>(Nullable<T> expect, Nullable<T> got) where T : struct, IEquatable<T>
      {
        if (!expect.HasValue && !got.HasValue) return true;
        if (!expect.HasValue || !got.HasValue) return false;

        return expect.Value.Equals(got.Value);
      }

      public static void AreEqual<T>(Nullable<T> expect, Nullable<T> got, string from = null) where T : struct, IEquatable<T>
      { if (!AreEqualTest(expect, got)) Fail("AreEqual({0}, {1})"   .args(expect, got), from); }

      public static void AreNotEqual<T>(Nullable<T> expect, Nullable<T> got, string from = null) where T : struct, IEquatable<T>
      { if (AreEqualTest(expect, got)) Fail("AreNotEqual({0}, {1})"   .args(expect, got), from); }




      public static bool AreWithinTest(decimal expect, decimal got, decimal delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (decimal expect, decimal got, decimal delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (decimal expect, decimal got, decimal delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }



      public static bool AreWithinTest(float expect, float got, float delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (float expect, float got, float delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (float expect, float got, float delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }



      public static bool AreWithinTest(double expect, double got, double delta) { return Math.Abs(expect - got) <= delta;  }

      public static void AreWithin    (double expect, double got, double delta, string from = null)
      { if (!AreWithinTest(expect, got, delta)) Fail("AreWithin({0}, {1}, {2})".args(expect, got, delta), from); }

      public static void AreNotWithin (double expect, double got, double delta, string from = null)
      { if (AreWithinTest(expect, got, delta)) Fail("AreNotWithin({0}, {1}, {2})".args(expect, got, delta), from); }


    #endregion

    #region IsTrue/False

      public static void IsTrue(bool condition, string from = null)
      {
        if (!condition) Fail("IsTrue({0})".args(condition), from);
      }

      public static void IsFalse(bool condition, string from = null)
      {
        if (condition) Fail("IsFalse({0})".args(condition), from);
      }
    #endregion

    #region AreSameRef/IsNull
      public static bool AreSameRefTest(object expect, object got)
      {
        if (expect==null && got==null) return true;
        if (expect==null || got==null) return false;

        return object.ReferenceEquals(expect, got);
      }

      public static void AreSameRef(object expect, object got, string from = null)
      {
        if (!AreSameRefTest(expect, got)) Fail("AreSameRef({0}, {1})".args(expect, got), from);
      }

      public static void AreNotSameRef(object expect, object got, string from = null)
      {
        if (AreSameRefTest(expect, got)) Fail("AreNotSameRef({0}, {1})".args(expect, got), from);
      }

      public static void IsNull(object reference, string from = null)
      {
        if (reference!=null) Fail("IsNull({0})".args(reference), from);
      }

      public static void IsNotNull(object reference, string from = null)
      {
        if (reference==null) Fail("IsNotNull({0})".args(reference), from);
      }

      public static void IsNull<T>(Nullable<T> got, string from = null) where T : struct
      {
        if (got.HasValue) Fail("IsNull({0})".args(got), from);
      }

      public static void IsNotNull<T>(Nullable<T> got, string from = null) where T : struct
      {
        if (!got.HasValue) Fail("IsNotNull({0})".args(got), from);
      }

    #endregion

    #region Throws
      public static bool ThrowsTest<TException>(Action a) where TException : Exception
      {
        try
        {
          a();
          return false;
        }
        catch(Exception error)
        {
          return error is TException;
        }
      }

      public static void Throws<TException>(Action action, string from = null) where TException : Exception
      {
        if (!ThrowsTest<TException>(action)) Fail("Throws<{0}>".args(typeof(TException).FullNameWithExpandedGenericArgs(false)), from);
      }

    #endregion

    #region Arrays

      /// <summary>
      /// Tests Arrays for equality via Array.SequenceEqual()
      /// </summary>
      public static bool AreArraysEquivalentTest(Array expect, Array got, IEqualityComparer<object> comparer = null)
      {
        if (expect==null && got==null) return true;
        if (expect==null || got==null) return false;

        if (expect.Length!=got.Length) return false;

        if (comparer==null)
          return expect.Cast<object>().SequenceEqual(got.Cast<object>());

        return expect.Cast<object>().SequenceEqual(got.Cast<object>(), comparer);
      }

      /// <summary>
      /// Tests Arrays for equality via Array.SequenceEqual()
      /// </summary>
      public static void AreArraysEquivalent(Array expect, Array got, string from = null, IEqualityComparer<object> comparer = null)
      {
        if (!AreArraysEquivalentTest(expect, got, comparer)) Fail("AreArraysEquivalent({0}, {1})".args(expect, got), from);
      }

      /// <summary>
      /// Tests arrays of T : IEquatable(T) for equality - element by element
      /// </summary>
      public static bool AreArraysEquivalentTest<T>(T[] expect, T[] got, out int differenceIdx, IEqualityComparer<T> comparer = null) where T : IEquatable<T>
      {
        differenceIdx = -1;
        if (expect==null && got==null) return true;
        if (expect==null || got==null) return false;

        if (expect.Length!=got.Length) return false;

        if (comparer==null)
          comparer = EqualityComparer<T>.Default;

        for(var i=0; i<expect.Length; i++ )
        {
          var eq = comparer.Equals( expect[i], got[i] );
          if (!eq)
          {
            differenceIdx = i;
            return false;
          }
        }
        return true;
      }

      /// <summary>
      /// Tests arrays of T : IEquatable(T) for equality - element by element
      /// </summary>
      public static void AreArraysEquivalent<T>(T[] expect, T[] got, string from = null, IEqualityComparer<T> comparer = null) where T : IEquatable<T>
      {
        if (!AreArraysEquivalentTest<T>(expect, got, out int differenceIdx, comparer))
          Fail("AreArraysEquivalent<{0}>({1}, {2}) at index {3}".Args(typeof(T).FullName, expect, got, differenceIdx), from);
      }

      /// <summary>
      /// Tests arrays of T : IEquatable(T) for equality - element by element
      /// </summary>
      public static bool AreArraysEquivalentTest<T>(Nullable<T>[] expect, Nullable<T>[] got, out int differenceIdx, IEqualityComparer<T> comparer = null) where T : struct, IEquatable<T>
      {
        differenceIdx = -1;
        if (expect==null && got==null) return true;
        if (expect==null || got==null) return false;

        if (expect.Length!=got.Length) return false;

        if (comparer==null)
          comparer = EqualityComparer<T>.Default;

        for(var i=0; i<expect.Length; i++ )
        {
          var e1 = expect[i];
          var e2 = got[i];

          if (!e1.HasValue && !e2.HasValue) continue;
          if (!e1.HasValue || !e2.HasValue)
          {
            differenceIdx = i;
            return false;
          }

          var eq = comparer.Equals( e1.Value, e2.Value );
          if (!eq)
          {
            differenceIdx = i;
            return false;
          }
        }
        return true;
      }

      /// <summary>
      /// Tests arrays of T : IEquatable(T) for equality - element by element
      /// </summary>
      public static void AreArraysEquivalent<T>(Nullable<T>[] expect, Nullable<T>[] got, string from = null, IEqualityComparer<T> comparer = null) where T : struct, IEquatable<T>
      {
        if (!AreArraysEquivalentTest<T>(expect, got, out int differenceIdx, comparer))
          Fail("AreArraysEquivalent<{0}>({1}, {2}) at index {3}".Args(typeof(T).FullName, expect, got, differenceIdx), from);
      }


      /// <summary>
      /// Tests arrays for equivalance using element-by-element equality test based on object.Equals()
      /// </summary>
      public static bool AreArrayObjectsEquivalentTest<T>(T[] expect, T[] got, out int differenceIdx) where T : class
      {
        differenceIdx = -1;
        if (expect==null && got==null) return true;
        if (expect==null || got==null) return false;

        if (expect.Length!=got.Length) return false;

        for(var i=0; i<expect.Length; i++ )
        {
          var e1 = expect[i];
          var e2 = got[i];

          if (e1==null && e2==null) continue;
          if (e1==null || e2==null)
          {
            differenceIdx = i;
            return false;
          }

          var eq = e1.Equals(e2);
          if (!eq)
          {
            differenceIdx = i;
            return false;
          }
        }
        return true;
      }

      /// <summary>
      /// Tests arrays for equivalance using element-by-element equality test based on object.Equals()
      /// </summary>
      public static void AreArrayObjectsEquivalent<T>(T[] expect, T[] got, string from = null) where T : class
      {
        if (!AreArrayObjectsEquivalentTest<T>(expect, got, out int differenceIdx))
          Fail("AreArrayObjectsEquivalent<{0}>({1}, {2}) at index {3}".Args(typeof(T).FullName, expect, got, differenceIdx), from);
      }

    #endregion

    #region .pvt
    private static string args(this string pat, params object[] args)
      {
        for(var i=0; i<args.Length; i++)
          args[i] = argToStr(args[i]);

        return pat.Args(args);
      }

      private static string argToStr(object arg)
      {
         const int MAX_LEN = 96;

         if (arg==null) return StringConsts.NULL_STRING;

         var sarg = arg as string;
         if (sarg!=null)
           return "(string)\"{0}\" of {1} chars".Args(sarg.TakeFirstChars(MAX_LEN, "..."), sarg.Length);

         var barg = arg as byte[];
         if (barg!=null)
           return "(byte[])\"{0}\" of {1} bytes".Args(barg.ToDumpString(DumpFormat.Hex, maxLen: MAX_LEN), barg.Length);


         var tp = arg.GetType();

         if (tp.IsEnum) return "{0}.{1}".Args(tp.Name, arg);
         if (tp.IsPrimitive) return "({0}){1}".Args(tp.Name, arg);
         if (arg is Type) return "({0}){1}".Args(tp.Name, ((Type)arg).FullNameWithExpandedGenericArgs());

         return "({0})`{1}`".Args(arg.GetType().FullNameWithExpandedGenericArgs(false), arg == null ? "<null>" : arg.ToString().TakeFirstChars(64));
      }
    #endregion

  }
}
