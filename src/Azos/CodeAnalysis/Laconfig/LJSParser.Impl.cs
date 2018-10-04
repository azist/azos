
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.Laconfig
{
    public sealed partial class LJSParser
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
          while(!token.IsPrimary && !token.IsDirective);
      }

      private void fetchPrimaryOrEOF()
      {
          do fetch();
          while(!token.IsPrimary && !token.IsDirective && !token.IsEOF);
      }


      private void doRoot(LJSTree tree)
      {
          if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
            errorAndAbort(LaconfigMsgCode.eSectionNameExpected);


          var node = new LJSSectionNode();
          tree.Root = node;

          node.StartToken = token;
          node.Name = token.Text;

          fetchPrimary();
          if (token.Type==LaconfigTokenType.tEQ)
          {
              fetchPrimary();
              if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                    errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);
              node.TranspilerPragma = token.Text;
              fetchPrimary();
          }

          populateSection(node);

          if (token.Type!=LaconfigTokenType.tEOF)
            errorAndAbort(LaconfigMsgCode.eContentPastRootSection);
      }


      private void populateSection(LJSSectionNode section)
      {
          if (token.Type!=LaconfigTokenType.tBraceOpen)
            errorAndAbort(LaconfigMsgCode.eSectionOpenBraceExpected);

          var children = new List<LJSNode>();

          fetchPrimary();//skip {  section started

          LJSContentNode content = null;
          LJSScriptNode  script  = null;

          while(true)
          {
              if (token.Type==LaconfigTokenType.tBraceClose)
              {
                fetchPrimaryOrEOF();//skip }  section ended
                break;
              }

              if (token.Type==LaconfigTokenType.tDirective)
              {
               content = null;
               if (script==null)
                {
                  script = new LJSScriptNode();//script
                  script.Parent = section;
                  script.Name = "SCRIPT";
                  script.Script = token.Text;
                  script.StartToken = token;
                  children.Add(script);
                }
                else
                  script.Script += ("\n"+ token.Text); //add to existing script

                fetchPrimary();
                continue;
              }


              if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                errorAndAbort(LaconfigMsgCode.eSectionOrAttributeNameExpected);

              var startToken = token;
              fetchPrimary(); //fetch next

              if (token.Type==LaconfigTokenType.tBraceOpen)//section w/o value
              {
                  content = null;
                  script = null;
                  var subsection = new LJSSectionNode();
                  subsection.Parent = section;
                  subsection.Name = startToken.Text;
                  subsection.StartToken = startToken;
                  populateSection(subsection);
                  children.Add(subsection);
              }
              else if (token.Type==LaconfigTokenType.tEQ)//section with value or attribute
              {
                  fetchPrimary();
                  if (token.Type!=LaconfigTokenType.tIdentifier && token.Type!=LaconfigTokenType.tStringLiteral)
                    errorAndAbort(LaconfigMsgCode.eSectionOrAttributeValueExpected);

                  var value = token.Text;
                  fetchPrimary();//skip value

                  if (token.Type==LaconfigTokenType.tBraceOpen)//section with pragma
                  {
                    content = null;
                    script = null;
                    var subsection = new LJSSectionNode();
                    subsection.Parent = section;
                    subsection.Name = startToken.Text;
                    subsection.TranspilerPragma = value;
                    subsection.StartToken = startToken;
                    populateSection(subsection);
                    children.Add(subsection);
                  }
                  else
                  {
                    content = null;
                    script = null;
                    var attr = new LJSAttributeNode();//attribute
                    attr.Parent = section;
                    attr.Name = startToken.Text;
                    attr.Value = value;
                    attr.StartToken = startToken;
                    children.Add(attr);
                  }

              }
              else
              {
                script = null;
                if (content==null)
                {
                  content = new LJSContentNode();//content
                  content.Parent = section;
                  content.Name = "CONTENT";
                  content.Content = startToken.Text;
                  content.StartToken = startToken;
                  children.Add(content);
                }
                else
                 content.Content += (" "+ startToken.Text); //add to existing content
              }
          }//while

          section.Children = children.ToArray();
      }

    }
}
