/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Scripting;

using Azos.CodeAnalysis;
using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.JSON;
using JL=Azos.CodeAnalysis.JSON.JsonLexer;
using System.Reflection;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Nub.CodeAnalysis
{
    [Runnable]
    public class JSONLexerTests
    {

        [Run]
        public void BOF_EOF()
        {
          var src = @"a";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tIdentifier, JsonTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="ePrematureEOF")]
        public void ePrematureEOF_Thrown()
        {
          var src = @"";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        public void ePrematureEOF_Logged()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new JL(new StringSource(src), msgs);
          lxr.AnalyzeAll();

          Aver.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)JsonMsgCode.ePrematureEOF));
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="ePrematureEOF", MsgMatch=MatchType.Contains)]
        public void ePrematureEOF_CouldLogButThrown()
        {
          var src = @"";

          var msgs = new MessageList();

          var lxr = new JL(new StringSource(src), msgs, throwErrors: true);
          lxr.AnalyzeAll();

          Aver.IsNotNull(msgs.FirstOrDefault(m => m.Type == MessageType.Error && m.Code == (int)JsonMsgCode.ePrematureEOF));
        }


        [Run]
        public void TokenClassifications()
        {
          var src = @"a 'string' : 12 //comment";

          var tokens = new JL(new StringSource(src)).Tokens;

          Aver.IsTrue( tokens[0].IsBOF);
          Aver.IsTrue( tokens[0].IsNonLanguage);
          Aver.IsFalse( tokens[0].IsPrimary);
          Aver.IsTrue(TokenKind.BOF == tokens[0].Kind);

          Aver.IsTrue( JsonTokenType.tIdentifier == tokens[1].Type);
          Aver.IsFalse( tokens[1].IsNonLanguage);
          Aver.IsTrue( tokens[1].IsPrimary);
          Aver.IsTrue(TokenKind.Identifier == tokens[1].Kind);

          Aver.IsTrue( JsonTokenType.tStringLiteral == tokens[2].Type);
          Aver.IsFalse( tokens[2].IsNonLanguage);
          Aver.IsTrue( tokens[2].IsPrimary);
          Aver.IsTrue( tokens[2].IsTextualLiteral);
          Aver.IsTrue(TokenKind.Literal == tokens[2].Kind);

          Aver.IsTrue( JsonTokenType.tColon == tokens[3].Type);
          Aver.IsFalse( tokens[3].IsNonLanguage);
          Aver.IsTrue( tokens[3].IsPrimary);
          Aver.IsTrue( tokens[3].IsOperator);
          Aver.IsTrue(TokenKind.Operator == tokens[3].Kind);


          Aver.IsTrue( JsonTokenType.tIntLiteral == tokens[4].Type);
          Aver.IsFalse( tokens[4].IsNonLanguage);
          Aver.IsTrue( tokens[4].IsPrimary);
          Aver.IsTrue( tokens[4].IsNumericLiteral);
          Aver.IsTrue(TokenKind.Literal == tokens[4].Kind);

          Aver.IsTrue( JsonTokenType.tComment == tokens[5].Type);
          Aver.IsFalse( tokens[5].IsNonLanguage);
          Aver.IsFalse( tokens[5].IsPrimary);
          Aver.IsTrue( tokens[5].IsComment);
          Aver.IsTrue(TokenKind.Comment == tokens[5].Kind);

        }


        [Run]
        public void BasicTokensWithIdentifierAndIntLiteral()
        {
          var src = @"{a: 2}";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]
          {
           JsonTokenType.tBOF, JsonTokenType.tBraceOpen,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tIntLiteral,
           JsonTokenType.tBraceClose, JsonTokenType.tEOF};

          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

         [Run]
        public void BasicTokensWithIdentifierAndDoubleLiteral()
        {
          var src = @"{a: 2.2}";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]
          {
           JsonTokenType.tBOF, JsonTokenType.tBraceOpen,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tDoubleLiteral,
           JsonTokenType.tBraceClose, JsonTokenType.tEOF};

          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        public void BasicTokens2()
        {
          var src = @"{a: 2, b: true, c: false, d: null, e: ['a','b','c']}";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]
          {
           JsonTokenType.tBOF, JsonTokenType.tBraceOpen,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tIntLiteral, JsonTokenType.tComma,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tTrue, JsonTokenType.tComma,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tFalse, JsonTokenType.tComma,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tNull, JsonTokenType.tComma,
           JsonTokenType.tIdentifier, JsonTokenType.tColon, JsonTokenType.tSqBracketOpen, JsonTokenType.tStringLiteral, JsonTokenType.tComma,
                                                                                          JsonTokenType.tStringLiteral, JsonTokenType.tComma,
                                                                                          JsonTokenType.tStringLiteral,
                                                            JsonTokenType.tSqBracketClose,
           JsonTokenType.tBraceClose, JsonTokenType.tEOF};

           //lxr.AnalyzeAll();
           //Console.Write(lxr.ToString());
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        public void IntLiterals()
        {
           Aver.AreObjectsEqual(12,  new JL(new StringSource(@"12")).Tokens.First(t=>t.IsPrimary).Value);
           Aver.AreObjectsEqual(2,   new JL(new StringSource(@"0b10")).Tokens.First(t=>t.IsPrimary).Value);
           Aver.AreObjectsEqual(16,  new JL(new StringSource(@"0x10")).Tokens.First(t=>t.IsPrimary).Value);
           Aver.AreObjectsEqual(8,   new JL(new StringSource(@"0o10")).Tokens.First(t=>t.IsPrimary).Value);
        }

        [Run]
        public void DoubleLiterals()
        {
           Aver.AreObjectsEqual(12.7,  new JL(new StringSource(@"12.7")).Tokens.First(t=>t.IsPrimary).Value);
           Aver.AreObjectsEqual(4e+8,  new JL(new StringSource(@"4e+8")).Tokens.First(t=>t.IsPrimary).Value);
        }



        [Run]
        public void Comments1()
        {
          var src = @"{
           //'string'}
          }
          ";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tBraceOpen, JsonTokenType.tComment, JsonTokenType.tBraceClose, JsonTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        public void Comments2()
        {
          var src = @"{
           /*'string'}*/
          }
          ";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tBraceOpen, JsonTokenType.tComment, JsonTokenType.tBraceClose, JsonTokenType.tEOF};
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

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tBraceOpen, JsonTokenType.tComment, JsonTokenType.tBraceClose, JsonTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        public void Comments4()
        {
          var src = @"{
           //comment text
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual("comment text", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments5()
        {
          var src = @"{
          /* //comment text */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" //comment text ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments6()
        {
          var src = @"{
          /* //comment text "+"\n"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" //comment text \n ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments7()
        {
          var src = @"{
          /* //comment text "+"\r"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" //comment text \r ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments8()
        {
          var src = @"{
          /* //comment text "+"\r\n"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" //comment text \r\n ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments9()
        {
          var src = @"{
          /* //comment text "+"\n\r"+@" */
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" //comment text \n\r ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments10()
        {
          var src = @"{       
          |* /* //comment text "+"\n\r"+@" */ *|
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(" /* //comment text \n\r */ ", lxr.ElementAt(2).Text);
        }

        [Run]
        public void Comments11withStrings()
        {
          var src = @"{       
          $'|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new JL(new StringSource(src));

          Aver.IsTrue(JsonTokenType.tStringLiteral == lxr.ElementAt(2).Type);
          Aver.AreEqual("|* /* //comment text \n\r */ *|", lxr.ElementAt(2).Text);
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString", MsgMatch=MatchType.Contains)]
        public void Comments12withStrings()
        {
          //string is opened but line break
          var src = @"{       
          '|* /* //comment text "+"\n\r"+@" */ *|'
          }
          ";

          var lxr = new JL(new StringSource(src));

          lxr.AnalyzeAll();
        }


        [Run]
        public void Comments13withStrings()
        {
          var src = @"{       
          |*'aaaa /* //comment""text "+"\n\r"+@" */ *|
          }
          ";
          var lxr = new JL(new StringSource(src));

          Aver.IsTrue(JsonTokenType.tComment == lxr.ElementAt(2).Type);
          Aver.AreEqual("'aaaa /* //comment\"text \n\r */ ", lxr.ElementAt(2).Text);
        }



        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedComment", MsgMatch=MatchType.Contains)]
        public void eUnterminatedComment1()
        {
          var src = @"a: /*aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedComment", MsgMatch=MatchType.Contains)]
        public void eUnterminatedComment2()
        {
          var src = @"a: |*aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [Run]
        public void String1()
        {
          var src = @"{'string'}";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tBraceOpen, JsonTokenType.tStringLiteral, JsonTokenType.tBraceClose, JsonTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }

        [Run]
        public void String2()
        {
          var src = @"{""string""}";

          var lxr = new JL(new StringSource(src));

          var expected = new JsonTokenType[]{JsonTokenType.tBOF, JsonTokenType.tBraceOpen, JsonTokenType.tStringLiteral, JsonTokenType.tBraceClose, JsonTokenType.tEOF};
          Aver.IsTrue( lxr.Select(t => t.Type).SequenceEqual(expected) );
        }


        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString", MsgMatch=MatchType.Contains)]
        public void eUnterminatedString1()
        {
          var src = @"a: 'aaaa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString", MsgMatch=MatchType.Contains)]
        public void eUnterminatedString2()
        {
          var src = @"a: ""aaaa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString", MsgMatch=MatchType.Contains)]
        public void eUnterminatedString3_Verbatim()
        {
          var src = @"a: $""aa

          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }

        [Run]
        [Aver.Throws(typeof(CodeProcessorException), Message="eUnterminatedString", MsgMatch=MatchType.Contains)]
        public void eUnterminatedString4_Verbatim()
        {
          var src = @"a: $'aa
          
          aa";

          var lxr = new JL(new StringSource(src));
          lxr.AnalyzeAll();
        }


        [Run]
        public void String_Escapes1()
        {
          var src = @"{""str\""ing""}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(@"str""ing", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes2()
        {
          var src = @"{'str\'ing'}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(@"str'ing", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes2_1()
        {
          var src = @"{a: -2, 'str\'ing\'': 2}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(@"str'ing'", lxr.ElementAt(7).Text);
        }

        [Run]
        public void String_Escapes2_2()
        {
          var src = @"{a: -2, 'string\'': 2}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual(@"string'", lxr.ElementAt(7).Text);
        }

        [Run]
        public void String_Escapes3()
        {
          var src = @"{'str\n\rring'}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual("str\n\rring", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes4_Unicode()
        {
          var src = @"{'str\u8978ring'}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes5_Unicode()
        {
          var src = @"{""str\u8978ring""}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual("str\u8978ring", lxr.ElementAt(2).Text);
        }

        [Run]
        public void String_Escapes6_ForwardSlash()
        {
          var src = @"{'male\/female'}";

          var lxr = new JL(new StringSource(src));

          Aver.AreEqual("male/female", lxr.ElementAt(2).Text);
        }


        [Run]
        public void PlusInt()
        {
          var src = @"{+12}";

          var lxr = new JL(new StringSource(src));

          Aver.IsTrue(JsonTokenType.tPlus == lxr.ElementAt(2).Type);
          Aver.AreObjectsEqual(12, lxr.ElementAt(3).Value);
        }

        [Run]
        public void MinusInt()
        {
          var src = @"{-12}";

          var lxr = new JL(new StringSource(src));

          Aver.IsTrue(JsonTokenType.tMinus == lxr.ElementAt(2).Type);
          Aver.AreObjectsEqual(12, lxr.ElementAt(3).Value);
        }



        [Run]
        public void PatternSearch()
        {
          var src = @"{a: 2, b: 'Znatoki', c: false, d: null, e: ['a','b','c']}";

          var lxr = new JL(new StringSource(src));


          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tColon),
                 (s,t) => FSMI.TakeAndComplete
             );

          Aver.AreEqual( "Znatoki", bvalue.Text );
        }

        [Run]
        public void PatternSearch2()
        {
          var src = @"{a: 2, b: 'Znatoki', c: false}";

          var lxr = new JL(new StringSource(src));


          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tComma)
             );

          Aver.AreEqual( "Znatoki", bvalue.Text );
        }

        [Run]
        public void PatternSearch3()
        {
          var src = @"{a: 2, b: 'Znatoki'}";

          var lxr = new JL(new StringSource(src));


          var bvalue = lxr.LazyFSM(
                 (s,t) => t.LoopUntilAny("b"),
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tComma)
             );

          Aver.IsNull(bvalue);
        }

        [Run]
        public void PatternSearch4_LoopUntilMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilMatch(
                                            (ss, tk) => tk.LoopUntilAny(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => s.Skip(3),
                 (s,t) => t.LoopUntilAny(JsonTokenType.tStringLiteral),
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tColon),
                 (s,t) => FSMI.Take,
                 (s,t) => t.IsAnyOrAbort(JsonTokenType.tBraceClose)
             );

          Aver.IsNotNull( capture );

          Aver.IsTrue( JsonTokenType.tIntLiteral == capture.Type );
          Aver.AreObjectsEqual( 127, capture.Value );
        }

        [Run]
        public void PatternSearch5_Skip()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q:2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.Skip(1),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tIdentifier == capture.Type );
          Aver.AreEqual( "a", capture.Text );

          capture = lxr.LazyFSM(
                 (s,t) => s.Skip(9),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tIdentifier == capture.Type );
          Aver.AreEqual( "q", capture.Text );
        }

        [Run]
        public void PatternSearch6_LoopUntilMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tStringLiteral == capture.Type );
          Aver.AreObjectsEqual( "Name", capture.Value );
        }


        [Run]
        public void PatternSearch7_LoopUntilAfterMatch()
        {
          var src = @"{a: 'Budilnik', 'Name': 'Znatoki', q: 2, z: 148, 'hero': 0x7f}";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tComma == capture.Type );
        }

        [Run]
        public void PatternSearch8_LoopUntilAfterMatch()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tNull == capture.Type );
        }

        [Run]
        public void PatternSearch8_LoopUntilAfterMatch_AnyToken()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(false,
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tNull == capture.Type );
        }

        [Run]
        public void PatternSearch9_LoopUntilAfterMatch()
        {
          var src = @"'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.LoopUntilAny(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tNull == capture.Type );
        }


        [Run]
        public void PatternSearch10_LoopUntilAfterMatch()
        {
          var src = @"1,2,3,4,5,6,7,8,9 : 'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));


          var capture = lxr.LazyFSM(
                 (s,t) => s.LoopUntilAfterMatch(
                                            (ss, tk) => tk.LoopUntilAny(JsonTokenType.tStringLiteral),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                 (s,t) => FSMI.Take
             );

          Aver.IsNotNull( capture );
          Aver.IsTrue( JsonTokenType.tNull == capture.Type );
        }


        [JsonPersonMatch]
        [Run]
        public void JSONPatternMatchAttribute1()
        {
          var src = @"1,2,3,4,5,6,7,8,9 : 'Name': 'Znatoki' null 'ok'";

          var lxr = new JL(new StringSource(src));

          var match = JsonPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);


          Aver.IsFalse( match );
        }

        [JsonPersonMatch]
        [Run]
        public void JSONPatternMatchAttribute2()
        {
          var src = @"{ code: 1121982, 'FirstName': 'Alex', DOB: null}";

          var lxr = new JL(new StringSource(src));

          var match = JsonPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);


          Aver.IsTrue( match );
        }

        [JsonPersonMatch]
        [Run]
        public void JSONPatternMatchAttribute3()
        {
          var src = @"{ code: 1121982, color: red, 'first_name': 'Alex', DOB: null}";

          var lxr = new JL(new StringSource(src));

          var match = JsonPatternMatchAttribute.Check(MethodBase.GetCurrentMethod(), lxr);


          Aver.IsTrue( match );
        }

    }


    public class JsonPersonMatchAttribute : JsonPatternMatchAttribute
    {
        public override bool Match(Azos.CodeAnalysis.JSON.JsonLexer content)
        {
          return content.LazyFSM(
                  (s,t) => s.LoopUntilMatch(
                                            (ss, tk) => tk.LoopUntilAny("First-Name","FirstName","first_name"),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tColon),
                                            (ss, tk) => tk.IsAnyOrAbort(JsonTokenType.tStringLiteral),
                                            (ss, tk) => FSMI.TakeAndComplete
                                          ),
                  (s,t) => FSMI.Take
              ) != null;
        }
    }


}
