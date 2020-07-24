using Azos.Apps;
using Azos.Client;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Provides client for consuming ILogChronicle and  IInstrumentationChronicle remote services
  /// </summary>
  public sealed class ChronicleWebClientLogic : ModuleBase, ILogChronicleLogic, IInstrumentationChronicleClientLogic
  {
    public ChronicleWebClientLogic(IApplication application) : base(application) { }
    public ChronicleWebClientLogic(IModule parent) : base(parent) { }


    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => "chronicle";

    public Task WriteAsync(LogBatch data)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

    public DatumFrame Map(Datum datum)
    {
      throw new NotImplementedException();
    }

    public Datum TryMaterialize(DatumFrame frame)
    {
      throw new NotImplementedException();
    }

    public Type MapInstrumentType(Guid id)
    {
      throw new NotImplementedException();
    }

    public Guid MapInstrumentType(Type tInstrument)
    {
      throw new NotImplementedException();
    }

    public Task WriteAsync(InstrumentationBatch data)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<DatumFrame>> GetAsync(InstrumentationChronicleFilter filter)
    {
      throw new NotImplementedException();
    }


    /*
    public async Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
    {
       var response = await m_Server.Call("REMOTE ADDRESS",
                                           nameof(ILogChronicleLogic),
                                           0,
                                           (http, ct) => http.Client.PostAndGetJsonMapAsync("", filter));

       var result = response.UnwrapPayloadArray()
               .OfType<JsonDataMap>()
               .Select(imap => JsonReader.ToDoc<Message>(imap));

       return result;
    }

    public async Task<IEnumerable<Datum>> GetAsync(InstrumentationChronicleFilter filter)
    {
      var response = await m_Server.Call("REMOTE ADDRESS",
                                           nameof(IInstrumentationChronicleLogic),
                                           0,
                                           (http, ct) => http.Client.PostAndGetJsonMapAsync("", filter));

      var result = response.UnwrapPayloadArray()
              .OfType<JsonDataMap>()
              .Select(imap => JsonReader.ToDoc<Datum>(imap));

      return result;
    }
    */

  }
}
