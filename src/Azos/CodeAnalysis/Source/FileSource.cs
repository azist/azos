
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Azos.CodeAnalysis.Source
{

      /// <summary>
      /// Represents source code stored in a file
      /// </summary>
      public class FileSource : StreamReader, ISourceText
      {


            /// <summary>
            /// Constructs file source infering source language from file extension
            /// </summary>
            public FileSource(string fileName)
              : base(fileName)
            {
              m_Language = Language.TryFindLanguageByFileExtension(Path.GetExtension(fileName));
              m_Name = fileName;
            }

            /// <summary>
            /// Constructs file source with specified language ignoring file extension
            /// </summary>
            public FileSource(Language language, string fileName)
              : base(fileName)
            {
              m_Language = language;
              m_Name = fileName;
            }


            private Language m_Language;
            private string m_Name;


            public void Reset()
            {
              BaseStream.Position = 0;
              DiscardBufferedData();
            }


            /// <summary>
            /// Returns source's file name
            /// </summary>
            public string Name
            {
              get { return m_Name ?? string.Empty; }
            }

            public bool EOF
            {
              get
              {
                return EndOfStream;
              }
            }

            public char ReadChar()
            {

              return (char)Read();
            }

            public char PeekChar()
            {
              return (char)Peek();
            }

            public Language Language
            {
              get { return m_Language ?? UnspecifiedLanguage.Instance; }
            }

      }

        /// <summary>
        /// Represents a list of file names
        /// </summary>
        public class FileNameList : List<string>
        {
            /// <summary>
            /// Checks that all files exist
            /// </summary>
            public void CheckAllNames()
            {
                foreach(string fn in this)
                    if (!File.Exists(fn))
                     throw new CodeAnalysisException(StringConsts.FILE_NOT_FOUND_ERROR + (fn ?? CoreConsts.UNKNOWN));
            }


            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                foreach (string fn in this)
                    sb.AppendLine(fn.ToString());

                return sb.ToString();
            }

        }


}
