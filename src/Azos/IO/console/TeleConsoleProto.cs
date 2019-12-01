using Azos.Data;
using Azos.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azos.IO.Console
{

  public struct TeleConsoleMsg : IJsonWritable, IJsonReadable
  {
    public enum CmdType { Write, WriteLine, Beep, Clear, Reset }

    public static TeleConsoleMsg Beep(string name) => new TeleConsoleMsg(name, CmdType.Beep, null, null, null);
    public static TeleConsoleMsg Clear(string name) => new TeleConsoleMsg(name, CmdType.Clear, null, null, null);
    public static TeleConsoleMsg Reset(string name) => new TeleConsoleMsg(name, CmdType.Reset, null, null, null);
    public static TeleConsoleMsg Write(string name, string text, ConsoleColor bg, ConsoleColor fg) => new TeleConsoleMsg(name, CmdType.Write, text, bg, fg);
    public static TeleConsoleMsg WriteLine(string name, string text, ConsoleColor bg, ConsoleColor fg) => new TeleConsoleMsg(name, CmdType.WriteLine, text, bg, fg);


    private TeleConsoleMsg(string name, CmdType  cmd, string text, ConsoleColor? bg, ConsoleColor? fg)
    {
      Name = name;
      Cmd = cmd;
      Text = text;
      Background = bg;
      Foreground = fg;
    }

    public string Name { get; private set; }
    public CmdType Cmd {  get; private set; }
    public string Text { get; private set; }
    public ConsoleColor? Background { get; private set; }
    public ConsoleColor? Foreground { get; private set; }


    /// <summary> Returns an approximate size in relative units (e.g. `chars`) used to estimate memory requirements</summary>
    public int Size => Text != null ? Text.Length : 0;

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      wri.Write("{\"n\": "); JsonWriter.EncodeString(wri, Name, options); wri.Write(",");
      wri.Write("\"c\": \""); wri.Write(Cmd.ToString()); wri.Write("\",");
      wri.Write("\"t\": "); JsonWriter.EncodeString(wri, Text, options);

      if (Background.HasValue)
      {
        wri.Write(",\"bg\": ");
        wri.Write(Background.Value.ToString());
      }

      if (Foreground.HasValue)
      {
        wri.Write(",\"fg\": ");
        wri.Write(Foreground.Value.ToString());
      }
      wri.Write("}");
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.NameBinding? nameBinding)
    {
      if (data is JsonDataMap map)
      {
        Name = map["n"].AsString();
        Cmd  = map["c"].AsEnum<CmdType>(CmdType.Write);
        Text = map["t"].AsString();

        Background = map["bg"].AsNullableEnum<ConsoleColor>();
        Foreground = map["fg"].AsNullableEnum<ConsoleColor>();

        return (true, this);
      }
      return (false, null);
    }
  }

  public sealed class TeleConsoleMsgBatch : TypedDoc
  {
    [Field] public Atom App{ get; set; }
    [Field] public DateTime TimestampUtc { get; set; }

    #warning TODO USE JSON string so it does not get re-serialized by server (senseless operation) the payload must be JSOned withtout CRLF for SSE
    [Field] public List<TeleConsoleMsg> Data{ get; set; } //<---  this is parsed, should we not use STRING in pre-serialized JSON
  }


}
