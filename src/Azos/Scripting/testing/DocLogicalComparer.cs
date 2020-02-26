using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Scripting
{
  /// <summary>
  /// Provides an extendable implementation of data document comparison/differ.
  /// Data documents are deep-compared for logical differences - field by field, visiting nested documents if necessary,
  /// so they can even have different schema as long as field sets are compatible.
  /// You can do A to B and/or B to A comparisons with or without AmorphousData.
  /// </summary>
  public class DocLogicalComparer
  {
    public enum Source { A, B };
    public enum DiffType { FieldNotFound, ValueDifference }

    public class Diff
    {
      public DiffType  Type      { get; internal set; }
      public string    FieldName { get; internal set; }
      public string    Text      { get; internal set; }
      public Result    DocResult { get; internal set; }
      public Exception Error     { get; internal set; }
    }

    public class Result
    {
      internal Result(DocLogicalComparer comparer, Doc a, Doc b)
      {
        Comparer = comparer;
        A = a;
        B = b;
      }

      private List<Diff> m_Diffs = new List<Diff>();

      public DocLogicalComparer Comparer { get; private set; }
      public Doc A { get; private set; }
      public Doc B { get; private set; }
      public (Doc master, Doc other) this[Source src] => src== Source.A ? (A, B) : (B, A);
      public bool AreSame => !AreDifferent;
      public bool AreDifferent => Differences.Any();
      public IEnumerable<Diff> Differences => m_Diffs;

      public void ReportDiff(DiffType tp, string fname, string text, Result docResult = null, Exception error = null)
      {
        var diff = new Diff
        {
          Type = tp,
          FieldName = fname,
          Text = text,
          DocResult = docResult,
          Error = error
        };
        m_Diffs.Add(diff);
      }
    }


    [Config]public bool LoopByA { get; set; } = true;
    [Config]public bool LoopByB { get; set; } = true;
    [Config]public bool LoopByAmorphous { get; set; } = true;
    [Config]public bool FindMissingInAmorphous { get; set; }

    public Result Compare(Doc docA, Doc  docB)
    {
      var result = new Result(this, docA.NonNull(nameof(docA)), docB.NonNull(nameof(docB)));
      DoCompare(result);
      return result;
    }

    /// <summary>
    /// Performs comparison of documents in result context
    /// </summary>
    protected virtual void DoCompare(Result ctx)
    {
      var i = 0;
      if (LoopByA) { i++; Loop(ctx, Source.A);}
      if (LoopByB) { i++; Loop(ctx, Source.B);}

      if (i==0) throw new AzosException("Nothing was done, neither {0} nor {1} were set".Args(nameof(LoopByA), nameof(LoopByB)));
    }

    protected virtual void Loop(Result ctx, Source src)
    {
      var (master, other) = ctx[src];

      DoLoopBySchema(ctx, src, master, other);

      if (LoopByAmorphous && master is IAmorphousData mad && mad.AmorphousDataEnabled)
      {
        DoLoopByAmorphous(ctx, src, mad, other);
      }
    }

    protected virtual void DoLoopBySchema(Result ctx, Source src, Doc master, Doc other)
    {
      foreach (var mfd in master.Schema)
      {
        object mval = master.GetFieldValue(mfd);
        object oval = null;

        var ofd = FindMatchingFieldDef(ctx, src, master, other, mfd);
        string ofn = null;
        if (ofd == null)
        {
          if (FindMissingInAmorphous && other is IAmorphousData oad && oad.AmorphousDataEnabled)
          {
            if (!oad.AmorphousData.TryGetValue(mfd.Name, out oval))
            {
              ctx.ReportDiff(DiffType.FieldNotFound, mfd.Name, "Field '{0}' of {1} not found in another and amorphous data tried".Args(mfd.Name, GetNameOf(src, true)));
              continue;
            }
            ofn = mfd.Name;
          }
          else
          {
            //log field disparity
            ctx.ReportDiff(DiffType.FieldNotFound, mfd.Name, "Field '{0}' of {1} not found in another".Args(mfd.Name, GetNameOf(src, true)));
            continue;
          }
        }
        else
        {
          ofn = ofd.Name;
          oval = other.GetFieldValue(ofd);
        }

        var eq = TestValueEqu(ctx, src, mfd.Name, ofn, mval, oval);
        if (!eq)
        {
          //difference is already reported by the method TestValueEqu
        }
      }
    }

    protected virtual void DoLoopByAmorphous(Result ctx, Source src, IAmorphousData master, Doc other)
    {
      foreach (var kvp in master.AmorphousData)
      {
        object mval = kvp.Value;
        object oval = null;

        var ofd = FindMatchingFieldDefFromAmorphousName(ctx, src, other, kvp.Key);
        string ofn  = null;
        if (ofd == null)
        {
          if (other is IAmorphousData oad && oad.AmorphousDataEnabled)
          {
            if (!oad.AmorphousData.TryGetValue(kvp.Key, out oval))
            {
              ctx.ReportDiff(DiffType.FieldNotFound, kvp.Key, "Field '{0}' of {1} not found in another and amorphous data tried".Args(kvp.Key, GetNameOf(src, true)));
              continue;
            }
            ofn = kvp.Key;
          }
          else
          {
            //log field disparity
            ctx.ReportDiff(DiffType.FieldNotFound, kvp.Key, "Field '{0}' of {1} not found in another".Args(kvp.Key, GetNameOf(src, true)));
            continue;
          }
        }
        else
        {
          oval = other.GetFieldValue(ofd);
          ofn = ofd.Name;
        }

        var eq = TestValueEqu(ctx, src, kvp.Key, ofn, mval, oval);
        if (!eq)
        {
          //difference is already reported by the method TestValueEqu
        }
      }
    }

    protected string GetNameOf(Source src, bool master) => master ? "master ({0})".Args(src) : "another ({0})".Args(src== Source.A ? "B" : "A");


    protected virtual Schema.FieldDef FindMatchingFieldDef(Result ctx, Source src, Doc master, Doc other, Schema.FieldDef mfd)
    {
      var result = other.Schema[mfd.Name]; //todo Consider using targeted names?
      return result;
    }

    protected virtual Schema.FieldDef FindMatchingFieldDefFromAmorphousName(Result ctx, Source src, Doc other, string fldName)
    {
      var result = other.Schema[fldName]; //todo Consider using targeted names?
      return result;
    }

    protected virtual bool TestValueEqu(Result ctx, Source src, string mname, string oname, object mval, object oval)
    {
      if (mval == null && oval == null) return true;//both are null

      if (mval == null || oval == null)
      {
        ctx.ReportDiff(DiffType.ValueDifference, mname, "Field '{0}' of {1} is null but the comparand is not".Args(mval==null ? mname : oname,
                                                                                                                   GetNameOf(src, mval==null)));
        return false;//one of them is null
      }

      var tm = mval.GetType();
      var to = oval.GetType();

      //both values are documents
      var mdoc = typeof(Doc).IsAssignableFrom(tm);
      var odoc = typeof(Doc).IsAssignableFrom(to);

      if (mdoc != odoc)//one is doc another is not
      {
        ctx.ReportDiff(DiffType.ValueDifference, mname, "Field '{0}' of {1} is Doc but the comparand is not".Args(mdoc ? mname : oname,
                                                                                                                  GetNameOf(src, mdoc)));
        return false;
      }

      if (mdoc)//both values are documents
      {
        var innerComparisonResult = Compare(mval as Doc, oval as Doc);
        if (innerComparisonResult.AreDifferent)
        {
          ctx.ReportDiff(DiffType.ValueDifference, mname, "Field '{0}' document values are different; see inner result".Args(mname), innerComparisonResult);
          return false;
        }

        return true;
      }

      //both values are enumerables
      var menum = tm != typeof(string) && typeof(IEnumerable).IsAssignableFrom(tm);
      var oenum = to != typeof(string) && typeof(IEnumerable).IsAssignableFrom(to);

      if (menum != oenum)
      {
        ctx.ReportDiff(DiffType.ValueDifference, mname, "Field '{0}' of {1} is enumerable but the comparand is not".Args(menum ? mname : oname,
                                                                                                                         GetNameOf(src, menum)));
        return false;//one is enumerable another is not
      }

      if (menum)
      {
        var mev = (mval as IEnumerable).Cast<object>();
        var oev = (oval as IEnumerable).Cast<object>();

        if (mev.Count() != oev.Count())
        {
          ctx.ReportDiff(DiffType.ValueDifference,
                         mname,
                         "Field '{0}' enumerable value item count mismatch: {1} has {2} but the comparand has {3} elements".Args(mname,
                                                                                                                                 GetNameOf(src, true),
                                                                                                                                 mev.Count(),
                                                                                                                                 oev.Count()));
          return false;
        }

        //we know that count is the same
        using(var me = mev.GetEnumerator())
          using(var oe = oev.GetEnumerator())
            for(var i = 0; me.MoveNext() && oe.MoveNext(); i++)
            {
              var equ = TestValueEqu(ctx, src, "{0}[{1}]".Args(mname,i), "{0}[{1}]".Args(oname, i), me.Current, oe.Current);

              if (!equ)
              {
                ctx.ReportDiff(DiffType.ValueDifference, mname, "Field '{0}' value sequences are different at index [{1}]".Args(mname, i));
                return false;
              }
            }

        return true;
      }

      bool result;
      try
      {
        result = mval.Equals(oval);
      }
      catch(Exception error)
      {
        ctx.ReportDiff(DiffType.ValueDifference,
                       mname,
                       "Field '{0}' equality test for value '{1}...' leaked: {2}".Args(mname,
                                                                                       mval.ToString().TakeFirstChars(32),
                                                                                       error.ToMessageWithType()),
                       error: error);
        return false;
      }

      if (!result)
      {
        ctx.ReportDiff(DiffType.ValueDifference,
                         mname,
                         "Field '{0}' value '{1}' in {2} does not equal value '{3}' in {4}".Args(mname,
                                                                                          mval.ToString().TakeFirstChars(32, ".."),
                                                                                          GetNameOf(src, true),
                                                                                          oval.ToString().TakeFirstChars(32, ".."),
                                                                                          GetNameOf(src, false)));
      }

      return result;

    }

  }
}
