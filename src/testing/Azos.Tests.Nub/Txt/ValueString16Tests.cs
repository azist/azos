using Azos.Scripting;
using Azos.Text;
using System;
using System.Diagnostics;

namespace Azos.Tests.Nub.Txt
{
  [Runnable]
  public class ValueString16Tests
  {
    private string[] a1;
    private ValueString16[] a2;

    [Run]
    public void Basic1()
    {
      ValueString16 vs = new ValueString16("abcd");//"abcdef");
      var vs2 = new ValueString16("abcd");//"abcdef");

      var a = new ValueString16[100];
       //sizeof(int[10])
      //sizeof(ValueString16).See();
      //sizeof(a).See();
      System.Runtime.InteropServices.Marshal.SizeOf<ValueString16>().See();
     // System.Runtime.InteropServices.Marshal.SizeOf(a).See();

      vs.StringValue.See();
      vs2.StringValue.See();

      Aver.AreEqual(vs, vs2);

      var sw = Stopwatch.StartNew();

      a1 = new string[16 * 1024*1024];
      for(var i=0; i< a1.Length; i++)
       a1[i] = new string(' ', 16);

      var e1 = sw.ElapsedMilliseconds;
      sw.Restart();

      a2 = new ValueString16[16 * 1024 * 1024];
      for (var i = 0; i < a2.Length; i++)
        a2[i] = new ValueString16("1234567890123456");

      var e2 = sw.ElapsedMilliseconds;

      "{0}  {1}".SeeArgs(e1, e2);



      a2.Length.See();

    }
  }
}
