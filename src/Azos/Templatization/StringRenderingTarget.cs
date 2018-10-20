
using System.Text;

namespace Azos.Templatization
{
    /// <summary>
    /// Renders templates into string
    /// </summary>
    public class StringRenderingTarget : IRenderingTarget
    {
        private StringBuilder m_Buffer = new StringBuilder();

        public StringRenderingTarget()
        {

        }

        public StringRenderingTarget(bool encodeHtml)
        {
          EncodeHtml = encodeHtml;
        }

        public readonly bool EncodeHtml;


        /// <summary>
        /// Returns what has been written
        /// </summary>
        public override string ToString()
        {
          return Value;
        }

        /// <summary>
        /// Returns what has been written
        /// </summary>
        public string Value
        {
          get { return m_Buffer.ToString();}
        }

        public void Write(object value)
        {
            if (value!=null)
             m_Buffer.Append(value.ToString());
        }

        public void Write(string value)
        {
          if (value!=null)
             m_Buffer.Append(value);
        }

        public void WriteLine(object value)
        {
            if (value!=null)
             m_Buffer.AppendLine(value.ToString());
        }

        public void WriteLine(string value)
        {
          if (value!=null)
             m_Buffer.AppendLine(value);
        }



        public void Flush()
        {
        }

        public object Encode(object value)
        {
            if (!EncodeHtml) return value;

            if (value==null) return null;

            return System.Net.WebUtility.HtmlEncode(value.ToString());
        }
    }
}
