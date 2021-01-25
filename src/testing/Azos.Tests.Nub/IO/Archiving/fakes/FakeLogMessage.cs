/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Log;

namespace Azos.Tests.Nub.IO.Archiving
{
  // The static FakeLogMessage class that orchestrates the construction of
  // the Message properties using the provided IMessageBuilder.This is used to emulate
  // logging data similar to the one generated in real complex business system
  public static class FakeLogMessage
  {
    public static Message BuildRandom()
    {
      var message = new Message();
      message.InitDefaultFields();

      var builder = GetRandomBuilder();

      message.Guid = Guid.NewGuid();
      message.RelatedTo = Guid.NewGuid();
      message.App = builder.GetApp();
      message.Channel = builder.GetChannel();
      message.Type = builder.GetLogType();
      message.Source = builder.GetSource();
      message.Host = builder.GetHost();
      message.From = builder.GetFrom();
      message.Topic = builder.GetTopic();
      message.Text = builder.GetText();
      message.Parameters = builder.GetParameters();
      message.Exception = builder.GetException();
      message.ArchiveDimensions = builder.GetArchiveDimensions();

      return message;
    }


    public static readonly BuilderBase[] BUILDERS =
    {
      new FlowInfoBuilder(),
      new XmlReqBuilder(),
      new JsonRequestBuilder(),
      new JsonResponseBuilder(),
      new XmlRespBuilder(),
      new DZeroReqBuilder(),
      new DZeroRespBuilder(),
      new ExceptionBuilder(),
      new DeleteFilesBuilder(),
      new FlowInfoBuilder()
    };


    public static BuilderBase GetRandomBuilder()
      => BUILDERS[(CoreConsts.ABS_HASH_MASK & Platform.RandomGenerator.Instance.NextRandomInteger) % BUILDERS.Length];


    public abstract class BuilderBase
    {
      public abstract Atom GetApp();
      public virtual string GetArchiveDimensions() => null;
      public abstract Atom GetChannel();
      public virtual Exception GetException() => null;
      public abstract string GetFrom();

      public virtual string GetHost()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 6);

        if (choice == 1) return "APPSRV-0001";
        if (choice == 2) return "APPSRV-0002";
        if (choice == 3) return "WEBSRV-0001";
        return "APPSRV-0003";
      }

      public virtual string GetParameters() => null;

      public virtual int GetSource() => 0;
      public abstract string GetText();
      public abstract string GetTopic();
      public abstract MessageType GetLogType();
    }


    internal class DeleteFilesBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_GOV;

      public override Atom GetChannel() => Atom.ZERO;

      public override string GetFrom() => "DeleteFilesJob@134.DoFire";

      public override string GetText() => "Scanned 26 files, 0 dirs; Deleted 0 files, 0 dirs";

      public override string GetTopic() => "Time";

      public override MessageType GetLogType() => MessageType.Info;
    }

    internal class XmlReqBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.ReadRequestMessage";

      public override string GetText() => "Raw xml req";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceC;

      public override string GetArchiveDimensions()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

        if (choice == 1) return @"{ ""chn"" : ""XXX"", ""clr"" : ""10.0.52.32:53322"" }";
        if (choice == 2) return @"{ ""chn"" : ""YYY"", ""clr"" : ""10.1.28.43:53323"" }";
        if (choice == 3) return @"{ ""chn"" : ""ZZZ"", ""clr"" : ""10.0.88.56:53380"" }";
        if (choice < 7) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.33.44:53322"" }";
        if (choice < 9) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.55.66:53322"" }";
        return @"{ ""chn"" : ""ABCD"", ""clr"" : ""10.2.33.68:53399"" }";
      }

      public override string GetParameters()
      {
        return FakeLogConstants.FAKE_LOG_XML_REQ;
      }
    }

    internal class FlowInfoBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.CheckAsync";

      public override string GetText() => "Flow info";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceA;

      public override string GetParameters()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 4);

        if (choice == 1) return FakeLogConstants.FAKE_LOG_FLOW_1;
        if (choice == 2) return FakeLogConstants.FAKE_LOG_FLOW_2;
        if (choice == 3) return FakeLogConstants.FAKE_LOG_FLOW_3;
        return FakeLogConstants.FAKE_LOG_FLOW_3;
      }

    }

    internal class JsonRequestBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.CheckAsync";

      public override string GetText() => "Got Fake request";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceB;

      public override string GetParameters() => FakeLogConstants.FAKE_LOG_JSON_REQ;
    }

    internal class JsonResponseBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.CheckAsync";

      public override string GetText() => "Fake response";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceD;

      public override string GetParameters() => FakeLogConstants.FAKE_LOG_JSON_RESP;
    }

    internal class XmlRespBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.WriteResponseMessage";

      public override string GetText() => "Raw xml resp";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceC;

      public override string GetArchiveDimensions()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

        if (choice == 1) return @"{ ""chn"" : ""XXX"", ""clr"" : ""10.0.52.32:53322"" }";
        if (choice == 2) return @"{ ""chn"" : ""YYY"", ""clr"" : ""10.1.28.43:53323"" }";
        if (choice == 3) return @"{ ""chn"" : ""ZZZ"", ""clr"" : ""10.0.88.56:53380"" }";
        if (choice < 7) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.33.44:53322"" }";
        if (choice < 9) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.55.66:53322"" }";
        return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.77.88:53322"" }";
      }

      public override string GetParameters()
      {
        return FakeLogConstants.FAKE_LOG_XML_RESP;
      }
    }

    internal class ExceptionBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.safeGetOneAsync";

      public override string GetText() => "Error leaked: [Fake.Clients.ClientException] API Call eventually failed; 0 endpoints tried; See .InnerException aggregate";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.Error;

      public override Exception GetException()
      {
        return new ArgumentOutOfRangeException("Something bad happened!", new ArgumentNullException("I think we forgot something!"));
      }
    }

    internal class DZeroReqBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.DZeroAsync";

      public override string GetText() => $"[{Ambient.Random.NextScaledRandomInteger(0, 5)}] DZ req";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceA;

      public override string GetArchiveDimensions()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

        if (choice == 1) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.1.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFA"", ""pcn"" : ""ABCD"" }";
        if (choice == 2) return @"{ ""bin"" : ""009999"", ""chn"" : ""FAKE"", ""clr"" : ""15.2.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/02"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFB"", ""pcn"" : ""ABCD"" }";
        if (choice == 3) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.3.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/03"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFC"", ""pcn"" : ""ABCD"" }";
        if (choice < 7) return @"{ ""bin"" : ""009993"", ""chn"" : ""none"", ""clr"" : ""15.4.35.68:53399"", ""dctx"" : ""none"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFD"", ""pcn"" : ""DCBA"" }";
        if (choice < 9) return @"{ ""bin"" : ""009994"", ""chn"" : ""SUM"", ""clr"" : ""20.5.35.68:53399"", ""dctx"" : ""sum"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFE"", ""pcn"" : ""XYZ"" }";
        return @"{ ""bin"" : ""009994"", ""chn"" : ""RED"", ""clr"" : ""30.0.0.69:53399"", ""dctx"" : ""red"", ""mbr"" : ""mbr@pbm::REDTEST2018/16"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFF"", ""pcn"" : ""XYZ"" }";

      }

      public override string GetParameters()
      {
        return FakeLogConstants.FAKE_LOG_DZERO_REQ;
      }
    }

    internal class DZeroRespBuilder : BuilderBase
    {
      public override Atom GetApp() => FakeLogConstants.FAKE_APP_TZT;

      public override Atom GetChannel() => FakeLogConstants.FAKE_CHANNEL_OPLOG;

      public override string GetFrom() => "FakeLogic.DZeroAsync";

      public override string GetText() => $"[{Ambient.Random.NextScaledRandomInteger(0, 5)}] DZ rsp";

      public override string GetTopic() => "blogic";

      public override MessageType GetLogType() => MessageType.TraceA;

      public override string GetArchiveDimensions()
      {
        var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

        if (choice == 1) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.1.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFA"", ""pcn"" : ""ABCD"" }";
        if (choice == 2) return @"{ ""bin"" : ""009999"", ""chn"" : ""FAKE"", ""clr"" : ""15.2.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/02"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFB"", ""pcn"" : ""ABCD"" }";
        if (choice == 3) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.3.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/03"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFC"", ""pcn"" : ""ABCD"" }";
        if (choice < 7) return @"{ ""bin"" : ""009993"", ""chn"" : ""none"", ""clr"" : ""15.4.35.68:53399"", ""dctx"" : ""none"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFD"", ""pcn"" : ""DCBA"" }";
        if (choice < 9) return @"{ ""bin"" : ""009994"", ""chn"" : ""SUM"", ""clr"" : ""20.5.35.68:53399"", ""dctx"" : ""sum"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFE"", ""pcn"" : ""XYZ"" }";
        return @"{ ""bin"" : ""009994"", ""chn"" : ""RED"", ""clr"" : ""30.0.0.69:53399"", ""dctx"" : ""red"", ""mbr"" : ""mbr@pbm::REDTEST2018/16"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFF"", ""pcn"" : ""XYZ"" }";
      }

      public override string GetParameters()
      {
        return FakeLogConstants.FAKE_LOG_DZERO_RSP;
      }
    }

  }//LogMessageFaker
}
