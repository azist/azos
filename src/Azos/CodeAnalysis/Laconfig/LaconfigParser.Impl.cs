/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.Laconfig
{
    public sealed partial class LaconfigParser : Parser<LaconfigLexer>
    {
            private class abortException: Exception {}


            private IEnumerator<LaconfigToken> tokens;
            private LaconfigToken token;

            private void abort() { throw new abortException(); }

            private void errorAndAbort(LaconfigMsgCode code)
            {
               EmitMessage(MessageType.Error, (int)code, Lexer.SourceCodeReference,token: token);
               throw new abortException();
            }

            private void fetch()
            {
                if (!tokens.MoveNext())
                   errorAndAbort(LaconfigMsgCode.ePrematureEOF);

                token = tokens.Current;
            }

            private void fetchPrimary()
            {
                do fetch();
                while(!token.IsPrimary);
            }

            private void fetchPrimaryOrEOF()
            {
                do fetch();
                while(!token.IsPrimary && token.Type!=LaconfigTokenType.tEOF);
            }


            private void doRoot(Configuration config)
            {
               if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                 errorAndAbort(LaconfigMsgCode.eSectionNameExpected);

               config.Create(token.Text);
               fetchPrimary();
               if (token.Type==LaconfigTokenType.tEQ)
               {
                    fetchPrimary();
                    if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                          errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);
                    config.Root.Value = token.Text;
                    fetchPrimary();
               }

               populateSection(config.Root);

               if (token.Type!=LaconfigTokenType.tEOF)
                 errorAndAbort(LaconfigMsgCode.eContentPastRootSection);
            }


            private void populateSection(ConfigSectionNode section)
            {
                if (token.Type!=LaconfigTokenType.tBraceOpen)
                      errorAndAbort(LaconfigMsgCode.eSectionOpenBraceExpected);

                fetchPrimary();//skip {  section started

                while(true)
                {
                    if (token.Type==LaconfigTokenType.tBraceClose)
                    {
                      fetchPrimaryOrEOF();//skip }  section ended
                      return;
                    }

                    if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                      errorAndAbort(LaconfigMsgCode.eSectionOrAttributeNameExpected);

                    var name = token.Text;
                    fetchPrimary();

                    if (token.Type==LaconfigTokenType.tBraceOpen)//section w/o value
                    {
                       var subsection = section.AddChildNode(name, null);
                       populateSection(subsection);
                    }else if (token.Type==LaconfigTokenType.tEQ)//section with value or attribute
                    {
                       fetchPrimary();
                       if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                          errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);

                       var value = token.Text;
                       fetchPrimary();//skip value

                       if (token.Type==LaconfigTokenType.tBraceOpen)//section with value
                       {
                         var subsection = section.AddChildNode(name, value);
                         populateSection(subsection);
                       }
                       else
                        section.AddAttributeNode(name, value);

                    }else
                       errorAndAbort(LaconfigMsgCode.eSyntaxError);
               }
            }

    }
}
