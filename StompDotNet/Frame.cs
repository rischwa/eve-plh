using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StompDotNet
{
    //TODO limits einbauen
    public class Frame
    {
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        public string Command { get; set; }
        public Dictionary<string, string> Headers
        {
            get { return _headers; }
        }

        public string Body { get; set; }

        public void WriteTo(Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.Write(Command);
            writer.Write('\n');

            foreach (var header in Headers)
            {
                WriteEscaped(writer, header.Key);
                writer.Write(':');
                WriteEscaped(writer, header.Value);
                writer.Write('\n');
            }

            writer.Write('\n');
            if (Body != null)
            {
                writer.Write(Body);
            }

            writer.Write('\0');
            writer.Flush();
        }

        private static void WriteEscaped(TextWriter writer, string value)
        {
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                        writer.Write(@"\\");
                        continue;
                    case ':': 
                        writer.Write(@"\c");
                        continue;
                    case '\n':
                        writer.Write(@"\n");
                        continue;
                    default:
                        writer.Write(c);
                        continue;
                }
            }
        }

        //TODO 0 byte kann von newlines gefollowed werden

        public static Frame ReadFrom(Stream stream)
        {
            var frame = new Frame();
            frame.InitFrom(stream);

            return frame;
        }

        private void InitFrom(Stream stream)
        {
            var b = GetStreamReader(stream);

            ReadCommand(b);
            
            char c;
            while ((c = b.ReadChar()) != '\n')
            {
                ReadHeader(b, c);
            }

            ReadBody(b);
        }

        //TODO extract, body muss in message gelesen werden
        public bool TryReadIntHeader( string headerKey, out int value)
        {
            string subscriptionString;
            value = -1;
            return Headers.TryGetValue(headerKey, out subscriptionString) &&
                   Int32.TryParse(subscriptionString, out value);
        }

        private void ReadBody(BinaryReader streamReader)
        {
            var builder = new StringBuilder();
            int length;
            if (TryReadIntHeader("content-length", out length))
            {
                
                if (length < 0)
                {
                    throw new StompProtocolException("Invalid content-length: " + length);
                }
                int actualReadCount =0;
                int accumulatedReadCount = 0;
                if (length != 0)
                {
                    var buffer = new byte[length];
                    while (accumulatedReadCount < length)
                    {
                        actualReadCount = streamReader.Read(buffer, accumulatedReadCount, length - accumulatedReadCount);
                        accumulatedReadCount += actualReadCount;
                    }

                    builder.Append(Encoding.UTF8.GetChars(buffer));//TODO body koennte direkt gesetzt werden
                }

                byte b;
                if ((b = streamReader.ReadByte()) != 0)
                {
                    throw new StompProtocolException(string.Format("Expected '\\0' at the end of the body after {3} (content-length {0}), but got [{1}]\n{2}", length, (int)b, builder.ToString().Replace('\0', '*'), actualReadCount));
                }
            }
            else
            {
                char c;
                while ((c = streamReader.ReadChar()) != '\0')
                {
                    builder.Append(c);
                }
            }

            Body = builder.ToString();
        }

        private static BinaryReader GetStreamReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return new BinaryReader(stream, Encoding.UTF8);
        }

        private void ReadHeader(BinaryReader b, char c)
        {
            var builder = new StringBuilder();
            builder.Append(c);
            var headerKey = ReadUntil(b, ':', builder);

            builder.Clear();
            var headerValue = ReadUntil(b, '\n', builder);
            //if there are multiple occurances of the same header, the first occurence counts
            if (!Headers.ContainsKey(headerKey))
            {
                Headers[headerKey] = headerValue;    
            }
        }

        private static String ReadUntil(BinaryReader b, char stop, StringBuilder stringBuilder)
        {
            char c;
            bool lastCharWasBackslash = false;
            while ((c = b.ReadChar()) != stop || lastCharWasBackslash)
            {
                if (lastCharWasBackslash)
                {
                    lastCharWasBackslash = false;
                    switch (c)
                    {
                        case '\\':
                            stringBuilder.Append('\\');
                            continue;
                        case 'n':
                            stringBuilder.Append('\n');
                            continue;
                        case 'c':
                            stringBuilder.Append(':');
                            continue;
                        default:
                            throw new StompProtocolException("Invalid escape sequence: \\" + c);
                    }
                }

                if (c == '\\')
                {
                    lastCharWasBackslash = true;
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString();
        }

        private void ReadCommand(BinaryReader streamReader)
        {
            char firstChar;
            //skip newlines after a previous message
            while ((firstChar = streamReader.ReadChar()) == '\n')
            {
            }

            var builder = new StringBuilder();
            builder.Append(firstChar);
            char c;
            while ((c = streamReader.ReadChar()) != '\n')
            {
                builder.Append(c);
            }

            Command = builder.ToString();

            return;

            //switch (firstChar)
            //{
            //    case 'C':
            //        //TODO hier duerfen fuer vers. 1.0 ':' nicht escaped auftauchen
            //        ReadRestOfCommand(streamReader, "CONNECTED");
            //        break;
            //    case 'M':
            //        ReadRestOfCommand(streamReader, "MESSAGE");
            //        break;
            //    case 'R':
            //        ReadRestOfCommand(streamReader, "RECEIPT");
            //        break;
            //    case 'E':
            //        ReadRestOfCommand(streamReader, "ERROR");
            //        break;
            //    default:
            //        throw new StompProtocolException("Expected one of [C]ONNECTED, [M]ESSAGE, [R]ECEIPT, [E]RROR, but got charcode " + (int)firstChar + " (" + firstChar + ")");
            //}
        }

        private void ReadRestOfCommand(TextReader streamReader, string expected)
        {
            for (int i = 1; i < expected.Length; ++i)
            {
                char c;
                if ((c = (char) streamReader.Read()) != expected[i])
                {
                    //TODO bessere meldung
                    throw new StompProtocolException(String.Format("Invalid command, expected {0}, but got {1}[{2}]", expected, expected.Substring(0, i), c));
                }
            }
            if (streamReader.Read() != '\n')
            {
                throw new StompProtocolException(String.Format("Missing '\\n' after <{0}>", expected));
            }
            //TODO enum?
            Command = expected;
        }
    }
}