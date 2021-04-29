/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class WebUtilsTests
  {
    [Run]
    public void Http_CookieTime()
    {
      var d = new DateTime(1980, 07, 12, 14, 45, 32, DateTimeKind.Utc);

      Aver.AreEqual("Sat, 12-Jul-1980 14:45:32 GMT", WebUtils.DateTimeToHTTPCookieDateTime(d));
    }

    [Run]
    public void Http_LastModifiedTime()
    {
      var d = new DateTime(1980, 07, 12, 14, 45, 32, DateTimeKind.Utc);

      Aver.AreEqual("Sat, 12 Jul 1980 14:45:32 GMT", WebUtils.DateTimeToHTTPLastModifiedHeaderDateTime(d));
    }

    [Run]
    public void URI_Join()
    {
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs("static","site","content"));
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs(" static","  site  "," content"));
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs(" static"," \\ site  "," // content"));
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs(" static/","//site  "," // content"));
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs(" static/","/","/site","// content"));
      Aver.AreEqual("/static/site/content", WebUtils.JoinPathSegs("/static/","/","/site","// content"));
      Aver.AreEqual("/static/site/content", WebUtils.JoinPathSegs("      /static/","site","\\content"));
      Aver.AreEqual("/static/site/content", WebUtils.JoinPathSegs(" ", null, "      /static/","site","\\content"));
      Aver.AreEqual("static/site/content",  WebUtils.JoinPathSegs("static", null, "site","", "", "\\content"));
    }

    [Run]
    public void ComposeURLQueryString_Empty()
    {
      Dictionary<string, object> pars = null;
      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual(string.Empty, result);

      pars = new Dictionary<string, object>();
      result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual(string.Empty, result);
    }

    [Run]
    public void ComposeURLQueryString_NullOrEmptyQueryParts()
    {
      var pars = new Dictionary<string, object>
      {
        { "name", null }
      };
      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("name", result);

      pars = new Dictionary<string, object>
      {
        { "name", string.Empty },
      };
      result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("name=", result);

      pars = new Dictionary<string, object>
      {
        { string.Empty, "ABBA" }
      };
      result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual(string.Empty, result);

      pars = new Dictionary<string, object>
      {
        { "name1", null },
        { "name2", string.Empty },
        { string.Empty, "ABBA" }
      };
      result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("name1&name2=", result);

      pars = new Dictionary<string, object>
      {
        { "name1", string.Empty },
        { "name2", null },
        { string.Empty, "ABBA" },
        { "name3", "John" }
      };
      result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("name1=&name2&name3=John", result);
    }

    [Run]
    public void ComposeURLQueryString_SpecSymbols()
    {
      var pars = new Dictionary<string, object> { { "name", "Petrov" }, { "age", 19 }, { "spec", @" -y~!@#$%^&*()_?><|';:\/=+" } };

      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("name=Petrov&age=19&spec=%20-y%7E%21%40%23%24%25%5E%26%2A%28%29_%3F%3E%3C%7C%27%3B%3A%5C%2F%3D%2B", result);
    }

    [Run]
    public void ComposeURLQueryString_Types()
    {
      var pars = new Dictionary<string, object>
      {
        { "int", -257 },
        { "bool", true },
        { "double", 1.9D },
        { "string", "data&data" },
        { "dec", 23.45M },
        { "float", -12.34F }
      };

      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("int=-257&bool=True&double=1.9&string=data%26data&dec=23.45&float=-12.34", result);
    }

    [Run]
    public void ComposeURLQueryString_UTF8()
    {
      var pars = new Dictionary<string, object>
      {
        { "eng", "Hello!" },
        { "jap", "こんにちは" },
        { "chi", "久有归天愿"},
        { "chi2", "你好" },
        { "fra", "Allô" },
        { "привет", "rus" },
        { "नमस्कार", "hind" }
      };

      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("eng=Hello%21&jap=%E3%81%93%E3%82%93%E3%81%AB%E3%81%A1%E3%81%AF&chi=%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF&chi2=%E4%BD%A0%E5%A5%BD&fra=All%C3%B4&%D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82=rus&%E0%A4%A8%E0%A4%AE%E0%A4%B8%E0%A5%8D%E0%A4%95%E0%A4%BE%E0%A4%B0=hind", result);
    }

    [Run]
    public void ComposeURLQueryString_Mixed()
    {
      var pars = new Dictionary<string, object>
      {
        { "eng", "Hello!" },
        { "jap", null },
        { "chi", "久有归天愿"},
        { "chi2", 12 },
        { "", -123456 },
        { "привет", string.Empty },
        { "नमस्कार", null }
      };

      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("eng=Hello%21&jap&chi=%E4%B9%85%E6%9C%89%E5%BD%92%E5%A4%A9%E6%84%BF&chi2=12&%D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82=&%E0%A4%A8%E0%A4%AE%E0%A4%B8%E0%A5%8D%E0%A4%95%E0%A4%BE%E0%A4%B0", result);
    }

    [Run]
    public void ComposeURLQueryString_PlusAndSpaces()
    {
      var pars = new Dictionary<string, object>
      {
        { "eng", "Hello Lenin!" },
        { "rus", "Привет Ленин!" }
      };

      var result = WebUtils.ComposeURLQueryString(pars);
      Aver.AreEqual("eng=Hello%20Lenin%21&rus=%D0%9F%D1%80%D0%B8%D0%B2%D0%B5%D1%82%20%D0%9B%D0%B5%D0%BD%D0%B8%D0%BD%21", result);
    }

    [Run]
    public void EscapeURIStringWithPlus()
    {
      var goodURL = "https://shippo-delivery-east.s3.amazonaws.com/fff5ec643c2c44539e5a26940d29e917.pdf?Signature=UUd8Pyuki6EDp8RJ/JtEIcSm524=&Expires=1505468405&AWSAccessKeyId=AKIAJGLCC5MYLLWIG42A";
      var badURL  = "https://shippo-delivery-east.s3.amazonaws.com/6dcf1e56f4fe49b892716393de92dd7e.pdf?Signature=/4iTy32xguuMX7Eba+5qc8TFCbs=&Expires=1505468476&AWSAccessKeyId=AKIAJGLCC5MYLLWIG42A";

      var escapedGoodURL = goodURL.EscapeURIStringWithPlus();
      var escapedBadURL = badURL.EscapeURIStringWithPlus();

      Aver.AreEqual(goodURL, escapedGoodURL);
      Aver.AreEqual("https://shippo-delivery-east.s3.amazonaws.com/6dcf1e56f4fe49b892716393de92dd7e.pdf?Signature=/4iTy32xguuMX7Eba%2B5qc8TFCbs=&Expires=1505468476&AWSAccessKeyId=AKIAJGLCC5MYLLWIG42A", escapedBadURL);
    }

    private void checkNextDayOfWeek(DateTime now, DayOfWeek dayOfWeek)
    {
      var day = now.RoundToNextWeekDay(dayOfWeek);
      var dt = (day - now.Date);
      Aver.IsTrue(day.DayOfWeek == dayOfWeek && (TimeSpan.FromDays(0) < dt) && (dt <= TimeSpan.FromDays(7)));
    }

    [Run]
    public void IsValidJSIdentifier()
    {
      Aver.IsFalse( ((string)null).IsValidJSIdentifier() );
      Aver.IsFalse( "".IsValidJSIdentifier() );
      Aver.IsFalse( "1".IsValidJSIdentifier() );
      Aver.IsFalse( "1aaa".IsValidJSIdentifier() );

      Aver.IsTrue( "a1".IsValidJSIdentifier() );

      Aver.IsTrue( "a".IsValidJSIdentifier() );
      Aver.IsTrue( "b".IsValidJSIdentifier() );

      Aver.IsTrue( "_b7878".IsValidJSIdentifier() );
      Aver.IsTrue( "_7878".IsValidJSIdentifier() );
      Aver.IsTrue( "$1".IsValidJSIdentifier() );
      Aver.IsTrue( "$$1".IsValidJSIdentifier() );

      Aver.IsFalse( "let".IsValidJSIdentifier() );
      Aver.IsFalse( "var".IsValidJSIdentifier() );
      Aver.IsFalse( "class".IsValidJSIdentifier() );
      Aver.IsFalse( "return".IsValidJSIdentifier() );
      Aver.IsFalse( "function".IsValidJSIdentifier() );
      Aver.IsFalse( "volatile".IsValidJSIdentifier() );

      Aver.IsTrue( "_let".IsValidJSIdentifier() );
    }

  }
}
