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

