namespace JsonOutputDemo
{
    #region Imports

    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using Jayrock.Json;
    using Jayrock.Json.Conversion;

    #endregion

    /// <summary>
    /// This program demonstrates using JsonTextWriter to emit JSON text in
    /// a forward-only, stream-oriented fashion. It also demonstrates
    /// using JsonConvert to format CLR types into JSON text.
    /// </summary>
    static class Program
    {
        const string newsSourceUrl = "http://feeds.bbci.co.uk/news/rss.xml";

        delegate void Demo();

        static void Main()
        {
            var demos = new Demo[]
            {
                WriteContinents,
                WriteContact,
                WriteRssToJson,
                ExportRssToJson,
            };

            foreach (var demo in demos)
            {
                var title = demo.Method.Name;
                var length = title.Length + 20;
                Console.WriteLine(new string('=', length));
                Console.WriteLine("Demo: " + title);
                Console.WriteLine(new string('-', length));
                demo();
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        static void WriteContinents()
        {
            using (var writer = CreateJsonWriter(Console.Out))
            {
                string[] continents = {
                    "Europe", "Asia", "Australia", "Antarctica", "North America", "South America", "Africa"
                };

                writer.WriteStartArray();
                Array.ForEach(continents, writer.WriteString);
                writer.WriteEndArray();
            }
        }

        static void WriteContact()
        {
            using (var writer = CreateJsonWriter(Console.Out))
            {
                writer.WriteStartObject();              //  {
                writer.WriteMember("Name");             //      "Name" :
                writer.WriteString("John Doe");         //          "John Doe",
                writer.WriteMember("PermissionToCall"); //      "PermissionToCall" :
                writer.WriteBoolean(true);              //          true,
                writer.WriteMember("PhoneNumbers");     //      "PhoneNumbers" :
                writer.WriteStartArray();               //          [
                WritePhoneNumber(writer,                //            { "Location": "Home",
                    "Home", "555-555-1234");            //              "Number": "555-555-1234" },
                WritePhoneNumber(writer,                //            { "Location": "Work",
                    "Work", "555-555-9999 Ext. 123");   //              "Number": "555-555-9999 Ext. 123" }
                writer.WriteEndArray();                 //          ]
                writer.WriteEndObject();                //  }
            }
        }

        static void WritePhoneNumber(JsonWriter writer, string location, string number)
        {
            writer.WriteStartObject();      //  {
            writer.WriteMember("Location"); //      "Location" :
            writer.WriteString(location);   //          "...",
            writer.WriteMember("Number");   //      "Number" :
            writer.WriteString(number);     //          "..."
            writer.WriteEndObject();        //  }
        }

        static void WriteRssToJson()
        {
            using (var writer = CreateJsonWriter(Console.Out))
                WriteRssToJson(GetNews(), writer);
        }

        //
        // NOTE: For sake of brevity, the following WriteRss* methods do not
        // write out all the members of the RichSiteSummary and related types.
        //

        static void WriteRssToJson(RichSiteSummary rss, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteMember("version");
            writer.WriteString(rss.version);
            writer.WriteMember("channel");
            WriteRssToJson(rss.channel, writer);
            writer.WriteEndObject();
        }

        static void WriteRssToJson(Channel channel, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteMember("title");
            writer.WriteString(channel.title);
            writer.WriteMember("link");
            writer.WriteString(channel.link);
            writer.WriteMember("items");
            writer.WriteStartArray();
            foreach (var item in channel.item)
                WriteRssToJson(item, writer);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        static void WriteRssToJson(Item item, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteMember("title");
            writer.WriteString(item.title);
            writer.WriteMember("description");
            writer.WriteString(item.description);
            writer.WriteMember("link");
            writer.WriteString(item.link);
            writer.WriteEndObject();
        }

        static void ExportRssToJson()
        {
            using (var writer = CreateJsonWriter(Console.Out))
                JsonConvert.Export(GetNews(), writer);
        }

        static RichSiteSummary news;

        static RichSiteSummary GetNews()
        {
            if (news == null)
            {
                var serializer = new XmlSerializer(typeof(RichSiteSummary));

                using (var reader = XmlReader.Create(newsSourceUrl))
                    news = (RichSiteSummary) serializer.Deserialize(reader);
            }

            return news;
        }

        static JsonWriter CreateJsonWriter(TextWriter writer)
        {
            return new JsonTextWriter(writer) { PrettyPrint = true };
        }
    }
}
