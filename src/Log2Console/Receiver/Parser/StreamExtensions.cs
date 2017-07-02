using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log2Console.Receiver.Parser
{
    public static class StreamExtensions
    {
        public static string GenerateStringFromStream(this Stream stream)
        {
            return stream.GenerateStringFromStream(Encoding.UTF8);
        }

        public static string GenerateStringFromStream(this Stream stream, Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;
            stream.Position = 0;
            using (StreamReader sr = new StreamReader(stream, encoding, true, 1024, true))
            {
                return sr.ReadToEnd();
            }
        }

        public static Stream GenerateStreamFromString(this string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
