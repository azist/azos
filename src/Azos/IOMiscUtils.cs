/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using NFX.PAL;

namespace NFX
{
    /// <summary>
    /// Misc utils for I/O
    /// </summary>
    public static class IOMiscUtils
    {
        public const int IO_WAIT_GRANULARITY_MS = 200;
        public const int IO_WAIT_MIN_TIMEOUT = 100;
        public const int IO_WAIT_DEFAULT_TIMEOUT = 2000;


    

        /// <summary>
        /// Fetch the content of a given URL.
        /// </summary>
        /// <returns>
        /// Return fetched URL as a string, or null string when resulting status code is not HttpStatusCode.OK.
        /// </returns>
        public static KeyValuePair<HttpStatusCode,string> GetURL(string urlAddress, int timeout = 5000)
        {
            var req     = (HttpWebRequest)WebRequest.Create(urlAddress);
            req.Timeout = timeout;
            var res     = (HttpWebResponse)req.GetResponse();
            try
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var strm = res.GetResponseStream();
                    var rds  = res.CharacterSet == null
                             ? new StreamReader(strm)
                             : new StreamReader(strm, Encoding.GetEncoding(res.CharacterSet));
                    var s = rds.ReadToEnd();
                    rds.Close();
                    return new KeyValuePair<HttpStatusCode, string>(res.StatusCode, s);
                }
            }
            finally
            {
                res.Close();
            }
            return new KeyValuePair<HttpStatusCode, string>(res.StatusCode, null);
        }

        

        


       

        




       


       
        public static byte[] ToNetworkByteOrder(this Guid guid)
        {
          var result = guid.ToByteArray();

          var t = result[3];
          result[3] = result[0];
          result[0] = t;

          t = result[2];
          result[2] = result[1];
          result[1] = t;

          t = result[5];
          result[5] = result[4];
          result[4] = t;

          t = result[7];
          result[7] = result[6];
          result[6] = t;

          return result;
        }

        public static Guid GuidFromNetworkByteOrder(this byte[] buf, int offset=0)
        {
          var a = ReadBEInt32(buf, ref offset);
          var b = ReadBEShort(buf, ref offset);
          var c = ReadBEShort(buf, ref offset);

          return new Guid(a, b, c,
                          buf[offset++],
                          buf[offset++],
                          buf[offset++],
                          buf[offset++],
                          buf[offset++],
                          buf[offset++],
                          buf[offset++],
                          buf[offset]);
        }
    }
}
