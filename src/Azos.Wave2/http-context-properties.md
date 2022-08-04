# HTTP context Properties

## Request

See `Path` vs `PathBase`. We should use `Path` in here
https://andrewlock.net/understanding-pathbase-in-aspnetcore/



```
GET http://localhost:8080/dima/app/dodik?a=2&b=true

On server: 

Request.Protocol: HTTP/1.1
Request.Scheme: http
Request.Method: GET
Request.HasFormContentType: False
Request.ContentType:
Request.ContentLength:
Request.Host.ToString: localhost:8080
Request.Host.ToUriComponent: localhost:8080
Request.Path.ToString: /dima/app/dodik
Request.Path.ToUriComponent: /dima/app/dodik
Request.PathBase.ToString:
Request.PathBase.ToUriComponent:
Request.QueryString.ToString: ?a=2&b=true
Request.QueryString.ToUriComponent: ?a=2&b=true
Query: a = 2
Query: b = true
Request.Headers.ToString: Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestHeaders
Header: Accept = */*
Header: Host = localhost:8080
Header: User-Agent = insomnia/2022.4.2
Header: Cookie = WV.CV=SID~SURUEJ-MNEQGLTI-UBOHOAIM-GVSIAWAFEXETGLMDIDOFTVOABEIZOBUZ
Request.Cookies.ToString: Microsoft.AspNetCore.Http.RequestCookieCollection
Cookie: WV.CV = SID~SURUEJ-MNEQGLTI-UBOHOAIM-GVSIAWAFEXETGLMDIDOFTVOABEIZOBUZ
```


## Response

### Send chunked
Microsoft sends chunked transfer if you **do not spesify content size**:
```
Response headers raw:
  Content-Type: text/event-stream
  Date: Thu, 04 Aug 2022 18:53:31 GMT
  Transfer-Encoding: chunked
```

However, the monet we add `ctx.Response.ContentLength = x;`, we now get this in response:
```
Response headers raw:
  Content-Length: 1000
  Content-Type: text/event-stream
  Date: Thu, 04 Aug 2022 18:56:32 GMT 
```