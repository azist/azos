
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Serialization.Slim;

namespace Azos.Glue.Native
{
    /// <summary>
    /// Constants common to Native/Socket-based family of technologies
    /// </summary>
    internal class Consts
    {
        public const string SLIM_FORMAT = "slim";

        /// <summary>
        /// Size of the packet delimiting field
        /// </summary>
        public const int PACKET_DELIMITER_LENGTH    = sizeof(int);

        public const int DEFAULT_MAX_MSG_SIZE       = 512 * 1024;
        public const int DEFAULT_RCV_BUFFER_SIZE    = 16 * 1024;
        public const int DEFAULT_SND_BUFFER_SIZE    = 16 * 1024;


        public const int MAX_MSG_SIZE_LOW_BOUND     = 0xff;

        public const int DEFAULT_SERIALIZER_STREAM_CAPACITY = 32 * 1024;

        public const string CONFIG_MAX_MSG_SIZE_ATTR = "max-msg-size";

        public const string CONFIG_RCV_BUF_SIZE_ATTR = "rcv-buf-size";
        public const string CONFIG_SND_BUF_SIZE_ATTR = "snd-buf-size";

        public const string CONFIG_RCV_TIMEOUT_ATTR = "rcv-timeout";
        public const string CONFIG_SND_TIMEOUT_ATTR = "snd-timeout";

    }
}
