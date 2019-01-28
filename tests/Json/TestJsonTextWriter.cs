#region Copyright (c) 2005 Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
#endregion

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.IO;
    using NUnit.Framework;
    using Jayrock.Json.Conversion;

    #endregion

    [ TestFixture ]
    public class TestJsonTextWriter
    {
        [ Test ]
        public void Blank()
        {
            var writer = new JsonTextWriter(new StringWriter());
            Assert.AreEqual(string.Empty, writer.ToString());
        }

        [ Test ]
        public void WriteString()
        {
            WriteString("[\"Hello\"]", "Hello");
            WriteString("[\"Hello World\"]", "Hello World");
            WriteString("[\"And before he parted, he said, \\\"Goodbye, people!\\\"\"]", "And before he parted, he said, \"Goodbye, people!\"");
            WriteString("[\"Hello\\tWorld\"]", "Hello\tWorld");
            WriteString("[\"Hello\\u0000World\"]", "Hello" + ((char) 0) + "World");
        }

        static void WriteString(string expected, string value)
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteString(value);
            Assert.AreEqual(expected, writer.ToString());
        }

        [ Test ]
        public void WriteNumber()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteNumber(123);
            Assert.AreEqual("[123]", writer.ToString());
        }

        [ Test ]
        public void WriteNull()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteNull();
            Assert.AreEqual("[null]", writer.ToString());
        }

        [ Test ]
        public void WriteTrueBoolean()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteBoolean(true);
            Assert.AreEqual("[true]", writer.ToString());
        }

        [ Test ]
        public void WriteFalseBoolean()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteBoolean(false);
            Assert.AreEqual("[false]", writer.ToString());
        }

        [ Test ]
        public void WriteEmptyArray()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteStringArray();
            Assert.AreEqual("[]", writer.ToString());
        }

        [ Test ]
        public void WriteArray()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteStringArray(new object[] { 123, "Hello \"Old\" World", true });
            Assert.AreEqual("[\"123\",\"Hello \\\"Old\\\" World\",\"True\"]", writer.ToString());
        }

        [ Test ]
        public void WriteEmptyObject()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteStartObject();
            writer.WriteEndObject();
            Assert.AreEqual("{}", writer.ToString());
        }

        [ Test ]
        public void WriteObject()
        {
            var writer = new JsonTextWriter(new StringWriter());
            writer.WriteStartObject();
            writer.WriteMember("Name");
            writer.WriteString("John Doe");
            writer.WriteMember("Salary");
            writer.WriteNumber(123456789);
            writer.WriteEndObject();
            Assert.AreEqual("{\"Name\":\"John Doe\",\"Salary\":123456789}", writer.ToString());
        }

        [ Test ]
        public void WriteNullValue()
        {
            Assert.AreEqual("[null]", JsonConvert.ExportToString(JsonNull.Value));
        }

        [ Test ]
        public void WriteValue()
        {
            Assert.AreEqual("[123]", WriteValue((byte) 123), "Byte");
            Assert.AreEqual("[\"123\"]", WriteValue((sbyte) 123), "Short byte");
            Assert.AreEqual("[123]", WriteValue((short) 123), "Short integer");
            Assert.AreEqual("[123]", WriteValue(123), "Integer");
            Assert.AreEqual("[123]", WriteValue(123L), "Long integer");
            Assert.AreEqual("[123]", WriteValue(123m), "Decimal");
        }

        [ Test ]
        public void WriteObjectArray()
        {
            var o = new JsonObject();
            o.Put("one", 1);
            o.Put("two", 2);
            o.Put("three", 3);
            Assert.AreEqual("[{\"one\":1,\"two\":2,\"three\":3},{\"one\":1,\"two\":2,\"three\":3},{\"one\":1,\"two\":2,\"three\":3}]", WriteValue(new object[] { o, o, o }));
        }

        [ Test ]
        public void WriteNestedArrays()
        {
            var inner = new[] { 1, 2, 3 };
            var outer = new[] { inner, inner, inner };
            Assert.AreEqual("[[1,2,3],[1,2,3],[1,2,3]]", WriteValue(outer));
        }

        [ Test ]
        public void WriteFromReader()
        {
            var reader = new JsonTextReader(new StringReader(@"
                { 'menu': {
                    'id': 'file',
                    'value': 'File:',
                    'popup': {
                      'menuitem': [
                        {'value': 'New', 'onclick': 'CreateNewDoc()'},
                        {'value': 'Open', 'onclick': 'OpenDoc()'},
                        {'value': 'Close', 'onclick': 'CloseDoc()'}
                      ]
                    }
                  }
                }"));

            var writer = new JsonTextWriter();
            writer.WriteFromReader(reader);
            Assert.AreEqual("{\"menu\":{\"id\":\"file\",\"value\":\"File:\",\"popup\":{\"menuitem\":[{\"value\":\"New\",\"onclick\":\"CreateNewDoc()\"},{\"value\":\"Open\",\"onclick\":\"OpenDoc()\"},{\"value\":\"Close\",\"onclick\":\"CloseDoc()\"}]}}}", writer.ToString());
        }

        [ Test ]
        public void PrettyPrinting()
        {
            var writer = new JsonTextWriter() { PrettyPrint = true };
            writer.WriteFromReader(new JsonTextReader(new StringReader("{'menu':{'id':'file','value':'File:','popup':{'menuitem':[{'value':'New','onclick':'CreateNewDoc()'},{'value':'Open','onclick':'OpenDoc()'},{'value':'Close','onclick':'CloseDoc()'}]}}}")));
            Assert.AreEqual(RewriteLines(string.Empty
                + "{ \n"
                + "    \"menu\": { \n"
                + "        \"id\": \"file\",\n"
                + "        \"value\": \"File:\",\n"
                + "        \"popup\": { \n"
                + "            \"menuitem\": [ { \n"
                + "                \"value\": \"New\",\n"
                + "                \"onclick\": \"CreateNewDoc()\"\n"
                + "            }, { \n"
                + "                \"value\": \"Open\",\n"
                + "                \"onclick\": \"OpenDoc()\"\n"
                + "            }, { \n"
                + "                \"value\": \"Close\",\n"
                + "                \"onclick\": \"CloseDoc()\"\n"
                + "            } ]\n"
                + "        }\n"
                + "    }\n"
                + "}\n"), writer.ToString() + Environment.NewLine);
        }

        static string WriteValue(object value)
        {
            var writer = new JsonTextWriter(new StringWriter());
            JsonConvert.Export(value, writer);
            return writer.ToString();
        }

        static string RewriteLines(string s)
        {
            var reader = new StringReader(s);
            var writer = new StringWriter();

            var line = reader.ReadLine();
            while (line != null)
            {
                writer.WriteLine(line);
                line = reader.ReadLine();
            }

            return writer.ToString();
        }
    }
}
