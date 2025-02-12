/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Serialization.Bix;
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
    public int Size => Text!=null ? Text.Length : 0;//This is slower: Text?.Length ?? 0;

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      wri.Write("{\"n\":"); JsonWriter.EncodeString(wri, Name, options); wri.Write(",");
      wri.Write("\"c\":\""); wri.Write(Cmd.ToString()); wri.Write("\"");

      if (Text!=null)
      {
        wri.Write(",\"t\":"); JsonWriter.EncodeString(wri, Text, options);
      }

      if (Background.HasValue)
      {
        wri.Write(",\"bg\":");
        wri.Write(Background.Value.ToString());
      }

      if (Foreground.HasValue)
      {
        wri.Write(",\"fg\":");
        wri.Write(Foreground.Value.ToString());
      }

      wri.Write("}");
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
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

  /// <summary>
  /// Describes a batch of TeleConsoleMsg sent to the remote port server
  /// </summary>
  [Bix("54E28C9F-402B-4FFF-9436-87A5D98AF716")]
  public sealed class TeleConsoleMsgBatch : TypedDoc
  {
    private static readonly JsonWritingOptions JSON_OPTIONS = new JsonWritingOptions{ MemberLineBreak = false, ObjectLineBreak = false };

    public TeleConsoleMsgBatch() { }
    /// <summary>
    /// Sets the DataJsonLine from the enumerable serialized in the compact JSON format without line breaks
    /// which is useful for efficient transmission and use in cases like SSE(server sent event data: protocol)
    /// </summary>
    public TeleConsoleMsgBatch(Atom app, DateTime utcNow, IEnumerable<TeleConsoleMsg> data)
    {
      Host = Azos.Platform.Computer.HostName;
      App = app;
      TimestampUtcMs = utcNow.ToMillisecondsSinceUnixEpochStart();
      DataJsonLine = JsonWriter.Write(data.NonNull(nameof(data)), JSON_OPTIONS);
    }


    [Field(description: "Origin host name")]
    public string Host { get; private set; }

    [Field(description: "Origin application id")]
    public Atom App { get; private set; }

    [Field(description: "Origin UTC time stamp")]
    public long TimestampUtcMs { get; private set; }

    /// <summary>
    /// The data is marshaled as a pre-serialized terse json
    /// so servers/relays do not need to recode the data object.
    /// The data may not have newline characters (they have to be escaped)
    /// </summary>
    [Field(description: "Json data payload")]
    public string DataJsonLine { get; private set; }
  }

}
