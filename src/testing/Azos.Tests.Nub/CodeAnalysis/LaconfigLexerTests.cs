/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Azos.CodeAnalysis;
using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.Laconfig;
using LL=Azos.CodeAnalysis.Laconfig.LaconfigLexer;
using Azos.Scripting;

namespace Azos.Tests.Nub.CodeAnalysis
{
    [Runnable]
    public class LaconfigLexerTests
    {

        [Run]
        public void BOF_EOF()
        {
          var src = @"a";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tIdentifier, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="ePrematureEOF")]
        public void ePrematureEOF_Thrown()
        {
          var src = @"";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        public void ePrematureEOF_Logged()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new LL(new StringSource(src), msgs);
          lxr.AnalyzeAll();

          Aver.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)LaconfigMsgCode.ePrematureEOF));
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="ePrematureEOF")]
        public void ePrematureEOF_CouldLogButThrown()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new LL(new StringSource(src), msgs, throwErrors: true);
          lxr.AnalyzeAll();

          Aver.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)LaconfigMsgCode.ePrematureEOF));
        }


        [Run]
        public void TokenClassifications()
        {
          var src = @"a 'string' = 12 {} //comment";

          var tokens = new LL(new StringSource(src)).Tokens;

          Aver.IsTrue( tokens[0].IsBOF);
          Aver.IsTrue( tokens[0].IsNonLanguage);
          Aver.IsFalse( tokens[0].IsPrimary);
          Aver.IsTrue(TokenKind.BOF == tokens[0].Kind);

          Aver.IsTrue( LaconfigTokenType.tIdentifier == tokens[1].Type);
          Aver.IsFalse( tokens[1].IsNonLanguage);
          Aver.IsTrue( tokens[1].IsPrimary);
          Aver.IsTrue(TokenKind.Identifier == tokens[1].Kind);

          Aver.IsTrue( LaconfigTokenType.tStringLiteral == tokens[2].Type);
          Aver.IsFalse( tokens[2].IsNonLanguage);
          Aver.IsTrue( tokens[2].IsPrimary);
          Aver.IsTrue( tokens[2].IsTextualLiteral);
          Aver.IsTrue(TokenKind.Literal == tokens[2].Kind);

          Aver.IsTrue( LaconfigTokenType.tEQ == tokens[3].Type);
          Aver.IsFalse( tokens[3].IsNonLanguage);
          Aver.IsTrue( tokens[3].IsPrimary);
          Aver.IsTrue( tokens[3].IsOperator);
          Aver.IsTrue(TokenKind.Operator == tokens[3].Kind);


          Aver.IsTrue( LaconfigTokenType.tIdentifier == tokens[4].Type);
          Aver.IsFalse( tokens[4].IsNonLanguage);
          Aver.IsTrue( tokens[4].IsPrimary);
          Aver.IsTrue(TokenKind.Identifier == tokens[4].Kind);

          Aver.IsTrue( LaconfigTokenType.tBraceOpen == tokens[5].Type);
          Aver.IsFalse( tokens[5].IsNonLanguage);
          Aver.IsTrue( tokens[5].IsPrimary);
          Aver.IsTrue(TokenKind.Symbol == tokens[5].Kind);

          Aver.IsTrue( LaconfigTokenType.tBraceClose == tokens[6].Type);
          Aver.IsFalse( tokens[6].IsNonLanguage);
          Aver.IsTrue( tokens[6].IsPrimary);
          Aver.IsTrue(TokenKind.Symbol == tokens[6].Kind);

          Aver.IsTrue( LaconfigTokenType.tComment == tokens[7].Type);
          Aver.IsFalse( tokens[7].IsNonLanguage);
          Aver.IsFalse( tokens[7].IsPrimary);
          Aver.IsTrue( tokens[7].IsComment);
          Aver.IsTrue(TokenKind.Comment == tokens[7].Kind);

        }

        [Run]
        public void Comments1()
        {
          var src = @"{
           //'string'}
          }
          ";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        public void Comments2()
        {
          var src = @"{
           /*'string'}*/
          }
          ";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        public void Comments3()
        {
          var src = @"{/*
                     
          'string //'}//inner
          */
          }
          ";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tComment, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        public void Comments4()
        {
          var src = @"{
           //comment text
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual("comment text", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments5()
        {
          var src = @"{
          /* //comment text */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" //comment text ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments6()
        {
          var src = @"{
          /* //comment text "+"\n"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" //comment text \n ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments7()
        {
          var src = @"{
          /* //comment text "+"\r"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" //comment text \r ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments8()
        {
          var src = @"{
          /* //comment text "+"\r\n"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" //comment text \r\n ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments9()
        {
          var src = @"{
          /* //comment text "+"\n\r"+@" */
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" //comment text \n\r ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments10()
        {
          var src = @"{       
          |* /* //comment text "+"\n\r"+@" */ *|
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(" /* //comment text \n\r */ ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments11withStrings()
        {
          var src = @"{       
          $'|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new LL(new StringSource(src));

          Aver.IsTrue(LaconfigTokenType.tStringLiteral == lxr.ElementAt(2).Type);
          Aver.AreEqual("|* /* //comment text \n\r */ *|", lxr.ElementAt(2).Text);
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString")]
        public void Comments12withStrings()
        {
          //string is opened but line break
          var src = @"{       
          '|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new LL(new StringSource(src));

          lxr.AnalyzeAll();
        }


        [Run]
        public void Comments13withStrings()
        {
          var src = @"{       
          |*'aaaa /* //comment""text "+"\n\r"+@" */ *|
          }
          ";
          var lxr = new LL(new StringSource(src));

          Aver.IsTrue(LaconfigTokenType.tComment == lxr.ElementAt(2).Type);
          Aver.AreEqual("'aaaa /* //comment\"text \n\r */ ", lxr.ElementAt(2).Text);
        }



        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedComment")]
        public void eUnterminatedComment1()
        {
          var src = @"a: /*aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedComment")]
        public void eUnterminatedComment2()
        {
          var src = @"a: |*aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [Run]
        public void String1()
        {
          var src = @"{'string'}";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tStringLiteral, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        public void String2()
        {
          var src = @"{""string""}";

          var lxr = new LL(new StringSource(src));

          var expected = new LaconfigTokenType[]{LaconfigTokenType.tBOF, LaconfigTokenType.tBraceOpen, LaconfigTokenType.tStringLiteral, LaconfigTokenType.tBraceClose, LaconfigTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString")]
        public void eUnterminatedString1()
        {
          var src = @"a: 'aaaa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString")]
        public void eUnterminatedString2()
        {
          var src = @"a: ""aaaa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString")]
        public void eUnterminatedString3_Verbatim()
        {
          var src = @"a: $""aa

          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString")]
        public void eUnterminatedString4_Verbatim()
        {
          var src = @"a: $'aa
          
          aa";

          var lxr = new LL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [Run]
        public void String_Escapes1()
        {
          var src = @"{""str\""ing""}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(@"str""ing", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes2()
        {
          var src = @"{'str\'ing'}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(@"str'ing", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes2_1()
        {
          var src = @"a{ n='str\'ing\''}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(@"str'ing'", lxr.ElementAt(5).Text);
        }

        [Run]
        public void String_Escapes2_2()
        {
          var src = @"a{ n = 'string\''}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual(@"string'", lxr.ElementAt(5).Text);
        }

        [Run]
        public void String_Escapes3()
        {
          var src = @"{'str\n\rring'}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual("str\n\rring", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes4_Unicode()
        {
          var src = @"{'str\u8978ring'}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes5_Unicode()
        {
          var src = @"{""str\u8978ring""}";

          var lxr = new LL(new StringSource(src));

          Aver.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }


    }


}
