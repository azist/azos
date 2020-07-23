using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Log;
using Azos.Security;
using Azos.Sky.Security.Permissions.Chronicle;
using Azos.Wave.Mvc;

namespace Azos.Sky.Chronicle.Server.Web
{
  [NoCache]
  [ChroniclePermission]
  [ApiControllerDoc(
    BaseUri = "/chronicle/log",
    Connection = "default/keep alive",
    Title = "Log Chronicle",
    Authentication = "Token/Default",
    Description = "Provides REST API for uploading log batches and querying chronicles by posting filter requests",
    TypeSchemas = new[]{typeof(ChroniclePermission) }
  )]
  [Release(ReleaseType.Preview, 2020, 07, 05, "Initial Release", Description = "Preview release of API")]
  public class Log : ApiProtocolController
  {
    [ApiEndpointDoc(Title = "Filter",
                    Uri = "filter",
                    Description = "Queries log chronicle by applying a structured filter `{@LogChronicleFilter}`",
                    Methods = new []{"POST - post filter object for query execution"},
                    RequestHeaders = new[]{ API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[]{ API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of `{@LogChronicleFilter}`",
                    ResponseContent = "JSON filter result - enumerable of `{@Message}`",
                    TypeSchemas = new[]{typeof(Message)})]
    [ActionOnPost(Name = "filter"), AcceptsJson]
    public async Task<object> Filter(LogChronicleFilter filter) => await ApplyFilterAsync(filter);

    [ApiEndpointDoc(Title = "Batch",
                    Uri = "batch",
                    Description = "Uploads a batch of log messages to log chronicle using `{@LogBatch}`",
                    Methods = new[] { "POST - post `{@LogBatch}` for insertion into archive" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of `{@LogBatch}`",
                    ResponseContent = "Api Change Result")]
    [ActionOnPost(Name = "batch"), AcceptsJson, ChroniclePermission(AccessLevel.VIEW_CHANGE)]
    public async Task<object> PostDataBatch(LogBatch batch) => await SaveNewAsync(batch);
  }

  [NoCache]
  [ChroniclePermission]
  [ApiControllerDoc(
    BaseUri = "/chronicle/instrumentation",
    Connection = "default/keep alive",
    Title = "Instrumentation Chronicle",
    Authentication = "Token/Default",
    Description = "Provides REST API for uploading instrumentation batches and querying chronicles by posting filter requests",
    TypeSchemas = new[] { typeof(ChroniclePermission) }
  )]
  [Release(ReleaseType.Preview, 2020, 07, 05, "Initial Release", Description = "Preview release of API")]
  public class Instrumentation : ApiProtocolController
  {
    [ApiEndpointDoc(Title = "Filter",
                    Uri = "filter",
                    Description = "Queries instrumentation chronicle by applying a structured filter `{@InstrumentationChronicleFilter}`",
                    Methods = new[] { "POST - post filter object for query execution" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of `{@InstrumentationChronicleFilter}`",
                    ResponseContent = "JSON filter result - enumerable of `{@Datum}`",
                    TypeSchemas = new[] { typeof(Message) })]
    [ActionOnPost(Name = "filter"), AcceptsJson]
    public async Task<object> Filter(InstrumentationChronicleFilter filter) => await ApplyFilterAsync(filter);

    [ApiEndpointDoc(Title = "Batch",
                    Uri = "batch",
                    Description = "Uploads a batch of instrumentation messages to log chronicle using `{@InstrumentationBatch}`",
                    Methods = new[] { "POST - post `{@InstrumentationBatch}` for insertion into archive" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of `{@InstrumentationBatch}`",
                    ResponseContent = "Api Change Result")]
    [ActionOnPost(Name = "batch"), AcceptsJson, ChroniclePermission(AccessLevel.VIEW_CHANGE)]
    public async Task<object> PostDataBatch(InstrumentationBatch batch) => await SaveNewAsync(batch);
  }
}
