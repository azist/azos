using Azos.Data;
using Azos.Instrumentation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Provides a static data contract which encapsulates data for IPC with polymorphic Datum-derivatives
  /// </summary>
  public sealed class DatumFrame
  {
    public readonly GDID Gdid;
    public readonly Guid Type;
    public readonly string Source; // #!ad {a:1, b:2, h:red}
    public readonly long Count;
    public readonly DateTime StartUtc;
    public readonly DateTime EndUtc;

    public readonly double ScalarValue;

    public readonly Atom ContentType;
    public readonly byte[] Content; //whatever opaque value serialized per m_ContentType specifier
  }
}
