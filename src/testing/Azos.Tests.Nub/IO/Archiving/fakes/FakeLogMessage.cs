/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Data;
using Azos.Log;
using Azos.Serialization.Bix;

namespace Azos.Tests.Nub.IO.Archiving
{
  /// <summary>
  /// A fake wrapper class for Azos.Log Message concrete implementation 
  /// (containing only the sealed Message complex property). 
  /// This class differs from other FakeRow implementors only in the level of 
  /// complexity required to produce an adequately random set of logs data.
  /// </summary>
  [Bix("9FBF53FC-DAAA-4AF2-B74B-C43493153BF8")]
  public class FakeLogMessage : FakeRow
  {
    private Message m_Instance = new Message();

    public Message Instance
    {
      get
      {
        return m_Instance;
      }
      set
      {
        m_Instance = value;
      }
    }

    /// <summary>
    /// Populates a Log Message FakeRow from the Message instance property.
    /// </summary>
    public override FakeRow Populate(GDID parentGdid)
    {
      ID = parentGdid;
      Instance.Gdid = ID;

      // Since Log Messages are very dynamic in nature we want to use a random set of builders to create properties.
      Director.Construct(Director.GetRandomBuilder(), ref m_Instance);

      return this;
    }
  }

  #region Director and Builder Logic

  /// <summary>
  /// The static Director class that orchestrates the construction of 
  /// the Message properties using the provided IMessageBuilder.
  /// </summary>
  internal static class Director
  {
    internal static void Construct(IMessageBuilder builder, ref Message message)
    {
      message.Guid = Guid.NewGuid();
      message.RelatedTo = Guid.NewGuid();
      message.App = builder.GetApp(message.Gdid);
      message.Channel = builder.GetChannel(message.Gdid);
      message.Type = builder.GetType(message.Gdid);
      message.Source = builder.GetSource(message.Gdid);
      message.UTCTimeStamp = DateTime.Now.AddHours(Ambient.Random.NextScaledRandomInteger(1, 12)).AddMinutes(Ambient.Random.NextScaledRandomInteger(1, 60));
      message.Host = builder.GetHost(message.Gdid);
      message.From = builder.GetFrom(message.Gdid);
      message.Topic = builder.GetTopic(message.Gdid);
      message.Text = builder.GetText(message.Gdid);
      message.Parameters = builder.GetParameters(message.Gdid);
      message.Exception = builder.GetException(message.Gdid);
      message.ArchiveDimensions = builder.GetArchiveDimensions(message.Gdid);
    }

    internal static Random Rng = new Random();

    internal static int GetRandomInt(int lbound, int ubound)
    {
      return Rng.Next(lbound, ubound);
    }

    internal static IMessageBuilder GetRandomBuilder()
    {
      var choice = GetRandomInt(1, 1000);

      if (choice < 100) return new FlowInfoBuilder();
      if (choice < 200) return new XmlReqBuilder();
      if (choice < 300) return new JsonRequestBuilder();
      if (choice < 400) return new JsonResponseBuilder();
      if (choice < 500) return new XmlRespBuilder();
      if (choice < 775) return new DZeroReqBuilder();
      if (choice < 950) return new DZeroRespBuilder();
      if (choice < 985) return new ExceptionBuilder();
      if (choice < 1001) return new DeleteFilesBuilder();
      return new FlowInfoBuilder();
    }
  }

  /// <summary>
  /// Interface for Builder methods used by the Director
  /// </summary>
  interface IMessageBuilder
  {
    Atom GetApp(GDID parentGdid);
    Atom GetChannel(GDID parentGdid);
    MessageType GetType(GDID parentGdid);
    int GetSource(GDID parentGdid);
    string GetHost(GDID parentGdid);
    string GetFrom(GDID parentGdid);
    string GetTopic(GDID parentGdid);
    string GetText(GDID parentGdid);
    string GetParameters(GDID parentGdid);
    Exception GetException(GDID parentGdid);
    string GetArchiveDimensions(GDID parentGdid);
  }

  /// <summary>
  /// A base abstract class that random builders should inherit from.
  /// </summary>
  public abstract class BuilderBase
  {
    public abstract Atom GetApp(GDID parentGdid);
    public virtual string GetArchiveDimensions(GDID parentGdid) => null;
    public abstract Atom GetChannel(GDID parentGdid);
    public virtual Exception GetException(GDID parentGdid) => null;
    public abstract string GetFrom(GDID parentGdid);

    public virtual string GetHost(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 6);

      if (choice == 1) return "APPSRV-0001";
      if (choice == 2) return "APPSRV-0002";
      if (choice == 3) return "WEBSRV-0001";
      return "APPSRV-0003";
    }

    public virtual string GetParameters(GDID parentGdid) => null;

    public virtual int GetSource(GDID parentGdid) => 0;
    public abstract string GetText(GDID parentGdid);
    public abstract string GetTopic(GDID parentGdid);
    public abstract MessageType GetType(GDID parentGdid);
  }

  #endregion

  #region Concrete Random Builders

  internal class DeleteFilesBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) =>
      parentGdid.ID % 3 == 0 ? Atom.Encode("esgov") : Atom.Encode("hub");

    public override Atom GetChannel(GDID parentGdid) => Atom.ZERO;

    public override string GetFrom(GDID parentGdid) => "DeleteFilesJob@134.DoFire";

    public override string GetText(GDID parentGdid) => "Scanned 26 files, 0 dirs; Deleted 0 files, 0 dirs";

    public override string GetTopic(GDID parentGdid) => "Time";

    public override MessageType GetType(GDID parentGdid) => MessageType.Info;
  }

  internal class XmlReqBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.ReadRequestMessage";

    public override string GetText(GDID parentGdid) => "Raw xml req";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceC;

    public override string GetArchiveDimensions(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

      if (choice == 1) return @"{ ""chn"" : ""XXX"", ""clr"" : ""10.0.52.32:53322"" }";
      if (choice == 2) return @"{ ""chn"" : ""YYY"", ""clr"" : ""10.1.28.43:53323"" }";
      if (choice == 3) return @"{ ""chn"" : ""ZZZ"", ""clr"" : ""10.0.88.56:53380"" }";
      if (choice < 7) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.33.44:53322"" }";
      if (choice < 9) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.55.66:53322"" }";
      return @"{ ""chn"" : ""ABCD"", ""clr"" : ""10.2.33.68:53399"" }";
    }

    public override string GetParameters(GDID parentGdid)
    {
      return FakeLogConstants.FAKE_LOG_XML_REQ;
    }
  }

  internal class FlowInfoBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.CheckAsync";

    public override string GetText(GDID parentGdid) => "Flow info";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceA;

    public override string GetParameters(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 4);

      if (choice == 1) return FakeLogConstants.FAKE_LOG_FLOW_1;
      if (choice == 2) return FakeLogConstants.FAKE_LOG_FLOW_2;
      if (choice == 3) return FakeLogConstants.FAKE_LOG_FLOW_3;
      return FakeLogConstants.FAKE_LOG_FLOW_3;
    }

  }

  internal class JsonRequestBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.CheckAsync";

    public override string GetText(GDID parentGdid) => "Got Fake request";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceB;

    public override string GetParameters(GDID parentGdid) => FakeLogConstants.FAKE_LOG_JSON_REQ;
  }

  internal class JsonResponseBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.CheckAsync";

    public override string GetText(GDID parentGdid) => "Fake response";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceD;

    public override string GetParameters(GDID parentGdid) => FakeLogConstants.FAKE_LOG_JSON_RESP;
  }

  internal class XmlRespBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.WriteResponseMessage";

    public override string GetText(GDID parentGdid) => "Raw xml resp";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceC;

    public override string GetArchiveDimensions(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

      if (choice == 1) return @"{ ""chn"" : ""XXX"", ""clr"" : ""10.0.52.32:53322"" }";
      if (choice == 2) return @"{ ""chn"" : ""YYY"", ""clr"" : ""10.1.28.43:53323"" }";
      if (choice == 3) return @"{ ""chn"" : ""ZZZ"", ""clr"" : ""10.0.88.56:53380"" }";
      if (choice < 7) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.33.44:53322"" }";
      if (choice < 9) return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.55.66:53322"" }";
      return @"{ ""chn"" : ""--any--"", ""clr"" : ""10.1.77.88:53322"" }";
    }

    public override string GetParameters(GDID parentGdid)
    {
      return FakeLogConstants.FAKE_LOG_XML_RESP;
    }
  }

  internal class ExceptionBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.safeGetOneAsync";

    public override string GetText(GDID parentGdid) => "Error leaked: [Fake.Clients.ClientException] API Call eventually failed; 0 endpoints tried; See .InnerException aggregate";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.Error;

    public override Exception GetException(GDID parentGdid)
    {
      return new ArgumentOutOfRangeException("Something bad happened!", new ArgumentNullException("I think we forgot something!"));
    }
  }

  internal class DZeroReqBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.DZeroAsync";

    public override string GetText(GDID parentGdid) => $"[{Ambient.Random.NextScaledRandomInteger(0, 5)}] DZ req";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceA;

    public override string GetArchiveDimensions(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

      if (choice == 1) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.1.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFA"", ""pcn"" : ""ABCD"" }";
      if (choice == 2) return @"{ ""bin"" : ""009999"", ""chn"" : ""FAKE"", ""clr"" : ""15.2.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/02"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFB"", ""pcn"" : ""ABCD"" }";
      if (choice == 3) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.3.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/03"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFC"", ""pcn"" : ""ABCD"" }";
      if (choice < 7) return @"{ ""bin"" : ""009993"", ""chn"" : ""none"", ""clr"" : ""15.4.35.68:53399"", ""dctx"" : ""none"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFD"", ""pcn"" : ""DCBA"" }";
      if (choice < 9) return @"{ ""bin"" : ""009994"", ""chn"" : ""SUM"", ""clr"" : ""20.5.35.68:53399"", ""dctx"" : ""sum"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFE"", ""pcn"" : ""XYZ"" }";
      return @"{ ""bin"" : ""009994"", ""chn"" : ""RED"", ""clr"" : ""30.0.0.69:53399"", ""dctx"" : ""red"", ""mbr"" : ""mbr@pbm::REDTEST2018/16"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFF"", ""pcn"" : ""XYZ"" }";

    }

    public override string GetParameters(GDID parentGdid)
    {
      return FakeLogConstants.FAKE_LOG_DZERO_REQ;
    }
  }

  internal class DZeroRespBuilder : BuilderBase, IMessageBuilder
  {
    public override Atom GetApp(GDID parentGdid) => Atom.Encode("fake");

    public override Atom GetChannel(GDID parentGdid) => Atom.Encode("oplog");

    public override string GetFrom(GDID parentGdid) => "FakeLogic.DZeroAsync";

    public override string GetText(GDID parentGdid) => $"[{Ambient.Random.NextScaledRandomInteger(0, 5)}] DZ rsp";

    public override string GetTopic(GDID parentGdid) => "blogic";

    public override MessageType GetType(GDID parentGdid) => MessageType.TraceA;

    public override string GetArchiveDimensions(GDID parentGdid)
    {
      var choice = Ambient.Random.NextScaledRandomInteger(1, 12);

      if (choice == 1) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.1.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFA"", ""pcn"" : ""ABCD"" }";
      if (choice == 2) return @"{ ""bin"" : ""009999"", ""chn"" : ""FAKE"", ""clr"" : ""15.2.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/02"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFB"", ""pcn"" : ""ABCD"" }";
      if (choice == 3) return @"{ ""bin"" : ""009999"", ""chn"" : ""--any--"", ""clr"" : ""15.3.35.68:53399"", ""dctx"" : ""fake"", ""mbr"" : ""mbr@pbm::JREMETEST2018/03"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFC"", ""pcn"" : ""ABCD"" }";
      if (choice < 7) return @"{ ""bin"" : ""009993"", ""chn"" : ""none"", ""clr"" : ""15.4.35.68:53399"", ""dctx"" : ""none"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFD"", ""pcn"" : ""DCBA"" }";
      if (choice < 9) return @"{ ""bin"" : ""009994"", ""chn"" : ""SUM"", ""clr"" : ""20.5.35.68:53399"", ""dctx"" : ""sum"", ""mbr"" : ""mbr@pbm::JREMETEST2018/01"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFE"", ""pcn"" : ""XYZ"" }";
      return @"{ ""bin"" : ""009994"", ""chn"" : ""RED"", ""clr"" : ""30.0.0.69:53399"", ""dctx"" : ""red"", ""mbr"" : ""mbr@pbm::REDTEST2018/16"", ""mid"" : ""8045D45130CD41139FCC36781DA6FEFF"", ""pcn"" : ""XYZ"" }";
    }

    public override string GetParameters(GDID parentGdid)
    {
      return FakeLogConstants.FAKE_LOG_DZERO_RSP;
    }
  } 

  #endregion
}
