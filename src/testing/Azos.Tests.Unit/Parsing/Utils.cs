/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/



using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Azos.Scripting;

using Azos.Text;


namespace Azos.Tests.Unit.Parsing
{
    [Runnable(TRUN.BASE)]
    public class Utils
    {
        [Run]
        public void FieldNameToDescription()
        {
          Aver.AreEqual("First Name", "FIRST_NAME".ParseFieldNameToDescription(true));
          Aver.AreEqual("first name", "FIRST_NAME".ParseFieldNameToDescription(false));

          Aver.AreEqual("First Name 2", "FIRST-NAME_2".ParseFieldNameToDescription(true));
          Aver.AreEqual("first name 2", "FIRST-NAME_2".ParseFieldNameToDescription(false));

          Aver.AreEqual("First Name 013 S", "FirstName013S".ParseFieldNameToDescription(true));
          Aver.AreEqual("first name 013 s", "FirstName013S".ParseFieldNameToDescription(false));

          Aver.AreEqual("turn off", "TurnOFF".ParseFieldNameToDescription(false));
          Aver.AreEqual("Turn Off", "TurnOFF".ParseFieldNameToDescription(true));

          Aver.AreEqual("first name", "__FIRST__Name".ParseFieldNameToDescription(false));
          Aver.AreEqual("First Name", "__FIRST__Name__".ParseFieldNameToDescription(true));

          Aver.AreEqual("field 0 1", "field_0_1".ParseFieldNameToDescription(false));
          Aver.AreEqual("Field 0 1", "field_0_1".ParseFieldNameToDescription(true));
        }


        [Run]
        public void MatchPattern1()
        {
          Aver.IsTrue( "some address".MatchPattern("s?me?addres?") );
          Aver.IsTrue( "same-addresZ".MatchPattern("s?me?addres?") );

          Aver.IsFalse( "sone address".MatchPattern("s?me?addres?") );
          Aver.IsFalse( "sane-oddresZ".MatchPattern("s?me?addres?") );
        }

        [Run]
        public void MatchPattern2()
        {
          Aver.IsTrue ( "some address".MatchPattern("s?me?addres?", senseCase: true) );
          Aver.IsFalse( "same-addreZs".MatchPattern("s?me?addres?", senseCase: true) );

          Aver.IsFalse ( "sone address".MatchPattern("s?me?addres?", senseCase: true) );
          Aver.IsFalse ( "saMe-addrezs".MatchPattern("s?me?addres?", senseCase: true) );
        }


        [Run]
        public void MatchPattern3()
        {
          Aver.IsTrue ( "some address".MatchPattern("some*") );
          Aver.IsFalse( "sone address ".MatchPattern("some*") );
        }
        [Run]
        public void MatchPattern4()
        {
          Aver.IsTrue( "some address".MatchPattern("s?me*") );
          Aver.IsFalse( "sone address".MatchPattern("s?me*") );
        }

        [Run]
        public void MatchPattern5()
        {
          Aver.IsTrue( "some address".MatchPattern("s?me*addre??") );
          Aver.IsFalse( "some adzress".MatchPattern("s?me*addre??") );
        }

        [Run]
        public void MatchPattern6()
        {
          Aver.IsTrue( "same Address".MatchPattern("s?me*addre??") );
        }

        [Run]
        public void MatchPattern7()
        {
          Aver.IsTrue( "same AddreZZ".MatchPattern("s?me*addre??") );
          Aver.IsFalse( "same AddreZZ?".MatchPattern("s?me*addre??") );
          Aver.IsFalse( "same AddreZ"  .MatchPattern("s?me*addre??") );
        }

        [Run]
        public void MatchPattern8()
        {
          Aver.IsTrue( "same AddreZZ".MatchPattern("*") );
        }

        [Run]
        public void MatchPattern9()
        {
          Aver.IsFalse( "same AddreZZ".MatchPattern("") );
        }

        [Run]
        public void MatchPattern10()
        {
          Aver.IsFalse( "same AddreZZ".MatchPattern("?") );
        }

        [Run]
        public void MatchPattern11()
        {
          Aver.IsTrue( "same AddreZZ".MatchPattern("????????????") );
          Aver.IsFalse( "same Addre".MatchPattern("????????????") );
        }

        [Run]
        public void MatchPattern12()
        {
          Aver.IsTrue ( "same AddreZZ".MatchPattern("same*") );
          Aver.IsFalse( "some AddreZZ".MatchPattern("same*") );
        }

        [Run]
        public void MatchPattern13()
        {
          Aver.IsTrue( "same AddreZZ".MatchPattern("*addre??") );
          Aver.IsTrue( "good address".MatchPattern("*addre??") );
          Aver.IsTrue( "new address-2".MatchPattern("*addre????") );

          Aver.IsFalse( "same ApdreZZ".MatchPattern("*addre??") );
          Aver.IsFalse( "good adress".MatchPattern("*addre??") );
          Aver.IsFalse( "good adres".MatchPattern("*addre??") );
          Aver.IsFalse( "new accress-2".MatchPattern("*addre????") );
        }

        [Run]
        public void MatchPattern14()
        {
          Aver.IsTrue( "same Address".MatchPattern("*address") );
          Aver.IsFalse( "same Address ok".MatchPattern("*address") );
        }

        [Run]
        public void MatchPattern15_1()
        {
          Aver.IsTrue ( "some same crazy address address Address".MatchPattern("*address") );
          Aver.IsFalse( "some same crazy address address!".MatchPattern("*address") );
        }

        [Run]
        public void MatchPattern15_2()
        {
          Aver.IsFalse( "some same crazy address address Address".MatchPattern("*address", senseCase: true) );
        }

        [Run]
        public void MatchPattern16_1()
        {
          Aver.IsTrue ("some crazy address".MatchPattern("*crazy*"));
          Aver.IsFalse("some crizy address".MatchPattern("*crazy*"));
          Aver.IsFalse("some craizy address".MatchPattern("*crazy*"));
        }

        [Run]
        public void MatchPattern16_2()
        {
          Aver.IsTrue  ("some crazy address".MatchPattern("*cr?zy*"));
          Aver.IsTrue  ("some crizy address".MatchPattern("*cr?zy*"));
          Aver.IsFalse ("some criizy address".MatchPattern("*cr?zy*"));
          Aver.IsFalse ("some krazy address".MatchPattern("*cr?zy*"));
        }


        [Run]
        public void MatchPattern16_3()
        {
          Aver.IsFalse("some crazy address".MatchPattern("*cr*zy"));
        }


        [Run]
        public void MatchPattern17()
        {
          Aver.IsTrue( "127.0.0.1".MatchPattern("127.0.*") );
        }

        [Run]
        public void MatchPattern18()
        {
          Aver.IsTrue( "https://some-site.com/?q=aaaa".MatchPattern("https://some-site.com*") );
        }

        [Run]
        public void MatchPattern19()
        {
          Aver.IsTrue( "140.70.81.139".MatchPattern("140.70.81.139") );
        }

        [Run]
        public void MatchPattern20()//--
        {
          Aver.IsTrue( "140.70.81.139" .MatchPattern("140.70.*.139") );
          Aver.IsTrue( "140.70.1.139"  .MatchPattern("140.70.*.139") );
          Aver.IsTrue( "140.70.17.139" .MatchPattern("140.70.*.139") );
          Aver.IsTrue( "140.70.123.139".MatchPattern("140.70.*.139") );

          Aver.IsFalse( "141.70.81.139" .MatchPattern("140.70.*.139") );
          Aver.IsFalse( "140.71.1.139"  .MatchPattern("140.70.*.139") );
          Aver.IsFalse( "140.70.17.13"  .MatchPattern("140.70.*.139") );
          Aver.IsFalse( "140.70.123.137".MatchPattern("140.70.*.139") );
        }


        [Run]
        public void MatchPattern21()
        {
          Aver.IsTrue( "140.70.81.139" .MatchPattern("*.70.81.139") );
          Aver.IsFalse( "140.70.99.139" .MatchPattern("*.70.81.139") );
        }

        [Run]
        public void MatchPattern22()
        {
          Aver.IsTrue( "140.70.81.139" .MatchPattern("140.70.81.*") );
          Aver.IsFalse( "140.70.99.139" .MatchPattern("140.70.81.*") );
        }

        [Run]
        public void MatchPattern23()
        {
          Aver.IsTrue( "140.70.81.139" .MatchPattern("140.*.81.*") );
          Aver.IsTrue( "140.80.81.139" .MatchPattern("140.*.81.*") );
          Aver.IsTrue( "140.    80       .81.139" .MatchPattern("140.*.81.*") );
          Aver.IsTrue( "140. 80 .81.99999" .MatchPattern("140.*.81.*") );

          Aver.IsTrue( "1.70.81.1" .MatchPattern("*.70.81.*") );
          Aver.IsFalse( "1.70.82.1" .MatchPattern("*.70.81.*") );
        }

        [Run]
        public void MatchPattern24()
        {
          Aver.IsTrue( "Alex Boris" .MatchPattern("*") );
          Aver.IsTrue( "Alex Boris" .MatchPattern("Alex*") );
          Aver.IsTrue( "Alex Boris" .MatchPattern("*Boris") );
          Aver.IsTrue( "Alex Boris" .MatchPattern("*lex Bo*") );
        }

        [Run]
        public void MatchPattern25()
        {
          Aver.IsTrue( "Alex Boris" .MatchPattern("*") );
          Aver.IsFalse( "Alex Boris" .MatchPattern("Axex*") );
          Aver.IsFalse( "Alex Boris" .MatchPattern("*Bosir") );
          Aver.IsFalse( "Alex Boris" .MatchPattern("*lxe Bo*") );
        }

        [Run]
        public void MatchPattern26()
        {
          Aver.IsTrue ( "Alex Boris" .MatchPattern("*") );
          Aver.IsFalse( "Alex Boris" .MatchPattern("alex*", senseCase: true) );
          Aver.IsTrue ( "Alex Boris" .MatchPattern("Alex*", senseCase: true) );

          Aver.IsFalse ( "Alex Boris" .MatchPattern("*boris", senseCase: true) );
          Aver.IsTrue ( "Alex Boris" .MatchPattern("*Boris", senseCase: true) );
        }

        [Run]
        public void MatchPattern27()
        {
          Aver.IsTrue ( "Honda buick honda monda donda ford buick ford ford" .MatchPattern("*ford") );
          Aver.IsFalse ( "Honda buick honda monda donda ford buick ford ford" .MatchPattern("*honda") );
          Aver.IsTrue  ( "Honda buick honda monda donda ford buick ford ford" .MatchPattern("*honda*") );
        }

        [Run]
        public void MatchPattern28()
        {
          Aver.IsTrue ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*ford") );
          Aver.IsFalse ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*ford", senseCase: true) );
          Aver.IsTrue  ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*fORd", senseCase: true) );
        }

        [Run]
        public void MatchPattern29()
        {
          Aver.IsTrue ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*buick*") );
          Aver.IsFalse ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*buick handa*") );
          Aver.IsTrue ( "Honda buick honda monda donda ford buick ford fORd" .MatchPattern("*buick h?nda*") );
        }

        [Run]
        public void MatchPattern30()
        {
          Aver.IsTrue ( "kikimora zhaba fly snake toad" .MatchPattern("*?ly*") );
          Aver.IsFalse ( "kikimora zhaba fly snake toad" .MatchPattern("*?ly") );
          Aver.IsTrue ( "kikimora zhaba fly snake toad" .MatchPattern("*?ly*toad") );
        }

        [Run]
        public void MatchPattern31()
        {
          Aver.IsTrue ( "We shall overcome" .MatchPattern("*****************") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("?????????????????") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("?*????????**?????") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("*?????????**????*") );

          Aver.IsFalse ( "We shall overcome" .MatchPattern("***x*************") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("?A???????????????") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("?*????-???**?????") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("*?????????**???? ") );
        }

        [Run]
        public void MatchPattern32()
        {
          Aver.IsTrue ( "We shall overcome" .MatchPattern("*********overcome") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("??????????v???o??") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("?e????????**??o??") );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("We*???????**????*") );

          Aver.IsFalse ( "We shall overcome" .MatchPattern("*********ofercome") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("??????????A???o??") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("?e--??????**??o??") );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("They*?????**????*") );
        }

        [Run]
        public void MatchPattern33()
        {
          Aver.IsTrue ( "We shall overcome" .MatchPattern("*********overCOME", senseCase: false) );
          Aver.IsFalse ( "We shall overcome" .MatchPattern("*********overCOME", senseCase: true) );

          Aver.IsTrue ( "We shall overcome" .MatchPattern("@@@@@@@@@overcome", '@') );
          Aver.IsTrue ( "We shall overcome" .MatchPattern("@erco$$", '@', '$') );
        }

        [Run]
        public void MatchPattern34()
        {
          Aver.IsTrue ( ((string)null).MatchPattern(null) );
          Aver.IsTrue ( "".MatchPattern("") );

          Aver.IsTrue ( ((string)null).MatchPattern("") );
          Aver.IsTrue ( "".MatchPattern(null) );

          Aver.IsFalse( " a ".MatchPattern(null));
          Aver.IsFalse ( ((string)null).MatchPattern(" a ") );
        }


        [Run]
        public void CapturePatternMatch_1()
        {
          Aver.AreEqual("aaa", "controller/aaa/user".CapturePatternMatch("controller/*/user"));

          Aver.AreEqual("aaa", "aaa".CapturePatternMatch("*"));
          Aver.AreEqual("aaa", "/aaa".CapturePatternMatch("/*"));

          Aver.AreEqual("", "/aaa".CapturePatternMatch("x/*"));
        }

        [Run]
        public void CapturePatternMatch_2()
        {
          Aver.AreEqual("controller/aaa", "controller/aaa/user".CapturePatternMatch("*/user"));

          Aver.AreEqual("", "aaa".CapturePatternMatch("/*"));
          Aver.AreEqual("", "aaa/".CapturePatternMatch("/*"));
          Aver.AreEqual("", "aaa/234234".CapturePatternMatch("/*"));
          Aver.AreEqual("aaa/9", "/aaa/9".CapturePatternMatch("/*"));
        }

        [Run]
        public void CheckScreenName()
        {
          Aver.IsFalse( DataEntryUtils.CheckScreenName("10o") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("1.0o") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName(".aa") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("2d-2222") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("DIMA-aaaaa..") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("дима 123") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName(".дима 123") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("1дима-123") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("-дима") );
          Aver.IsFalse( DataEntryUtils.CheckScreenName("дима.") );


          Aver.IsTrue(  DataEntryUtils.CheckScreenName("dima-qwerty") );
          Aver.IsTrue(  DataEntryUtils.CheckScreenName("d2-2222") );
          Aver.IsTrue( DataEntryUtils.CheckScreenName("дима123") );
          Aver.IsTrue( DataEntryUtils.CheckScreenName("дима-123") );
          Aver.IsTrue( DataEntryUtils.CheckScreenName("дима.123") );
        }

        [Run]
        public void NormalizePhone1()
        {
          var n = DataEntryUtils.NormalizeUSPhone("5552224415");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415", n);
        }

        [Run]
        public void NormalizePhone2()
        {
          var n = DataEntryUtils.NormalizeUSPhone("2224415");
          Console.WriteLine(n);
          Aver.AreEqual("(???) 222-4415", n);
        }

        [Run]
        public void NormalizePhone3()
        {
          var n = DataEntryUtils.NormalizeUSPhone("   +38 067 2148899   ");
          Console.WriteLine(n);
          Aver.AreEqual("+38 067 2148899", n);
        }

        [Run]
        public void NormalizePhone4()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415", n);
        }

        [Run]
        public void NormalizePhone5()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415 EXT 2014");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x2014", n);
        }

        [Run]
        public void NormalizePhone6()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415.2014");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x2014", n);
        }

        [Run]
        public void NormalizePhone7()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415EXT.2014");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x2014", n);
        }

        [Run]
        public void NormalizePhone8()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415 X 2014");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x2014", n);
        }

        [Run]
        public void NormalizePhone9()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555.222.4415");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415", n);
        }

        [Run]
        public void NormalizePhone10()
        {
          var n = DataEntryUtils.NormalizeUSPhone("555-222-4415");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415", n);
        }

        [Run]
        public void NormalizePhone11()
        {
          var n = DataEntryUtils.NormalizeUSPhone("5552224415ext123");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x123", n);
        }

        [Run]
        public void NormalizePhone12()
        {
          var n = DataEntryUtils.NormalizeUSPhone("5552224415ext.123");
          Console.WriteLine(n);
          Aver.AreEqual("(555) 222-4415x123", n);
        }

        [Run]
        public void CheckEmail()
        {
          string[] validEMails = {
            "user@example.com",
            "user.777@example.com",
            "u.ser@example.com",
            "user@com",
            "user@e.xample",
            "юзер@мояпочта.ры",
            "#!$%&'*+-/=?^_`{}|~@example.com",
            "-user-@example.com",
            "us-_-er@example.com",
            "user#01@example.com",
            "user@7super.puper08.example.com",
            "user@example--com",
            "user@example.s43",
            "user@example.museum",
            "alex.jack.soybean@example.of.my.domain.com.me",
            "boris_zhaba@yahoo.com",
            "boris-zhaba@yahoo.com",
            "boris.zhaba@yahoo.com",
            "boris___zhaba@yahoo.com",
            "sunny2346273864263@yahoo.com"
          };

          Console.WriteLine("==== Valid emails ====");
          foreach (var email in validEMails)
          {
            Console.WriteLine(email);
            Aver.IsTrue(DataEntryUtils.CheckEMail(email));
          }

          Console.WriteLine("==== Invlaid emails ====");
          string[] invalidEMails = {
            "  ",
            "@",
            "user@ ",
            "user@",
            " @example.com",
            "@example.com",
            ".@.",
            "dima@zaza@yahoo.com",
            "dima zaza@yahoo.com",
            "user",
            "user2example.com",
            "user.@example.com",
            ".user@example.com",
            "user@example.com.",
            "user@.example.com",
            "us..er@example.com",
            "user@example..com",
            "user @example.com",
            "user@example.com ",
            "user@ example.com",
            "us er@example.com",
            "user@example com",
            "user@example .com",
            "user@example.-com",
            "user@example-.com",
            "user@-example.com",
            "user@examplecom-",
            "user@e-.xample.com",
            "user@e.-xample.com",
            "us@er@example.com",
            "user#example.com",
            "user@example/com",
            @"us\er@example.com",
            @"user@exa\mple.com",
            "us(er@example.com",
            "user(comment)@example.com",
            "user@exa(mple.com",
            "us)er@example.com",
            "user@exa)mple.com",
            "us,er@example.com",
            "user@exa,mple.com",
            "us:er@example.com",
            "user@exa:mple.com",
            "us;er@example.com",
            "user@exa;mple.com",
            "us<er@example.com",
            "user@exa<mple.com",
            "us>er@example.com",
            "user@exa>mple.com",
            "us[er@example.com",
            "user@exa[mple.com",
            "us]er@example.com",
            "user@exa]mple.com",
            "user@exam-_ple.com"
          };

          foreach (var email in invalidEMails)
          {
            Console.WriteLine(email);
            Aver.IsFalse(DataEntryUtils.CheckEMail(email));
          }

        }

        [Run]
        public void CheckPhone()
        {
          string[] good = {
            "(800) 234-2345x234",
            "(800) 234-2345",
            "800 2345678",
            "800 234-4522",
            "800.2345678",
            "800.234.4522",
            "800-234-2345",
            "800-234-2345x234",
            "8882344511",
            "(888)2344511",
            "(888)234-4511",
            "(888)234.4511",
            "(888) 2344511",
            "(888) 234 4511",
            "(900) 4megood",
            "9004megood",
            "+28937498723987498237",
            "+8293 823098 82394",
            "+3423-3423-234-34" ,
            "+3423-3423-234x456",
            "+1 900 4ME-GOOD"
          };
          string[] bad =
          {
            "800",
            "(800)",
            "(8888)234-4511",
            " (888)234-4511",
            "(888)234-4511 ",
            "(8-88)234-4511",
            "+1423423 +23423",
            ")800 23456777(",
            "800)1234567",
            "(216) 234(2345)",
            "345#aaaaa",
            "7567:242333",
            "+800242--3333",
            "+800242..3333",
            "+800242-.3333",
            "#800242.-3333",
            "+800242.-3333",
            "+(80 0)242.-3333",
            "(800).2423333",
            "(800)-2423333",
            "(800)2423333.",
            ".(800)2423333",
            "-(800)2423333",
            "((800))2423333",
            "(800-)2423333",
            "(.800)2423333",
            "+(800)242-3333",
            "(800)242. 3333",
            "(800)242 - 3333",
            "(800)242        COOL",
            "(800)242 - 33 - 33"
          };
          Console.WriteLine("Good numbers:");
          for (int i = 0; i < good.Length; i++)
          {
            Console.WriteLine(good[i]);
            Aver.IsTrue(DataEntryUtils.CheckTelephone(good[i]));
          }

          Console.WriteLine("Bad numbers:");
          for (int i = 0; i < bad.Length; i++)
          {
              Console.WriteLine(bad[i]);
              Aver.IsFalse(DataEntryUtils.CheckTelephone(bad[i]));
          }
        }
    }
  }


