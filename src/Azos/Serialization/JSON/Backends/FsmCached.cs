/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Azos.Serialization.JSON.Backends
{
  /// <summary>
  /// cached fsm state bag to reduce allocations
  /// </summary>
  internal sealed class FsmCached
  {
    private const int SNIPPET_BUFFER_SZ_1 = 64;
    private const int SNIPPET_BUFFER_SZ_2 = 128;

    private static FsmCached s_Cache1;
    private static FsmCached s_Cache2;
    private static FsmCached s_Cache3;
    private static FsmCached s_Cache4;


    public static FsmCached Get()
    {
      var instance =
        Interlocked.Exchange(ref s_Cache1, null) ??
        Interlocked.Exchange(ref s_Cache2, null) ??
        Interlocked.Exchange(ref s_Cache3, null) ??
        Interlocked.Exchange(ref s_Cache4, null);

      if (instance == null) instance = new FsmCached();

      instance.ctor();

      return instance;
    }

    public static void Release(FsmCached instance)
    {
      instance.reset();

      Thread.MemoryBarrier();

      if (null == Interlocked.CompareExchange(ref s_Cache1, instance, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Cache2, instance, null)) return;
      if (null == Interlocked.CompareExchange(ref s_Cache3, instance, null)) return;
      Interlocked.CompareExchange(ref s_Cache4, instance, null);
    }


    private void ctor()
    {
      if (m_Builder == null) m_Builder = new StringBuilder(0xff);

      m_ErrorLevel = JsonReader.ErrorSourceDisclosureLevel;

      m_CallStackIdx = 0;
      m_SnippetIdx = 0;

      if (m_ErrorLevel > 0)
      {
        if (m_CallStack == null) m_CallStack = new List<string>(256);
      }
      else
      {
        m_CallStack = null;
      }

      if (m_ErrorLevel > 1)
      {

        var ssz = m_ErrorLevel > 2 ? SNIPPET_BUFFER_SZ_2 : SNIPPET_BUFFER_SZ_1;

        if (m_SnippetBuffer == null || m_SnippetBuffer.Length != ssz)
        {
          m_SnippetBuffer = new char[ssz];
        }
      }
      else
      {
        m_SnippetBuffer = null;
      }
    }


    private StringBuilder m_Builder;
    private int m_ErrorLevel;

    private List<string> m_CallStack;
    private int m_CallStackIdx;

    private char[] m_SnippetBuffer;
    private int m_SnippetIdx;


    private void reset()
    {
      m_Builder.Clear();
      if (m_SnippetBuffer != null) m_CallStack.Clear();
      if (m_SnippetBuffer != null) Array.Fill(m_SnippetBuffer, (char)0);
    }

    internal void StackPush(string what)
    {
      if (m_CallStack == null) return;
      if (m_CallStackIdx < m_CallStack.Count)
      {
        m_CallStack[m_CallStackIdx++] = what;
      }
      else
      {
        m_CallStack.Add(what);
        m_CallStackIdx = m_CallStack.Count;
      }
    }

    internal void StackPop()
    {
      if (m_CallStack == null) return;
      m_CallStackIdx--;
      Aver.IsTrue(m_CallStackIdx >= 0, "stack mismatch");
    }

    //use circular buffer to avoid dynamic string allocations
    internal void AddChar(char c)
    {
      if (m_SnippetBuffer == null) return;
      if (m_SnippetIdx == m_SnippetBuffer.Length) m_SnippetIdx = 0;//circular buffer
      m_SnippetBuffer[m_SnippetIdx++] = c;
    }

    //Reads circular buffer (used for efficiency) into a regular string for error reporting
    //this is ONLY called when error occurs, the SB allocation is ok
    internal string GetSnippetString()
    {
      if (m_SnippetBuffer == null) return null;
      var sb = new StringBuilder();

      var idx = m_SnippetIdx;
      for(var cnt =0; cnt < m_SnippetBuffer.Length; cnt++)
      {
        if (idx == m_SnippetBuffer.Length) idx = 0;//circular
        sb.Append(m_SnippetBuffer[idx]);
      }

      return sb.ToString();
    }

  }
}
