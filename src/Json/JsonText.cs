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
    using System.Text;

    #endregion

    /// <summary>
    /// Facade for working with JsonReader and JsonWriter implementations
    /// that work with JSON text.
    /// </summary>

    public static class JsonText
    {
        static Func<TextReader, JsonReader> _currentReaderFactory;
        static Func<TextWriter, JsonWriter> _currentWriterFactory;

        static JsonText()
        {
            _currentReaderFactory = DefaultReaderFactory = reader => new JsonTextReader(reader);
            _currentWriterFactory = DefaultWriterFactory = writer => new JsonTextWriter(writer);
        }

        public static Func<TextReader, JsonReader> DefaultReaderFactory { get; }
        public static Func<TextWriter, JsonWriter> DefaultWriterFactory { get; }

        public static Func<TextReader, JsonReader> CurrentReaderFactory
        {
            get => _currentReaderFactory;
            set => _currentReaderFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static Func<TextWriter, JsonWriter> CurrentWriterFactory
        {
            get => _currentWriterFactory;
            set => _currentWriterFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static JsonReader CreateReader(TextReader reader)
        {
            return CurrentReaderFactory(reader);
        }

        public static JsonReader CreateReader(string source)
        {
            return CreateReader(new StringReader(source));
        }

        public static JsonWriter CreateWriter(TextWriter writer)
        {
            return CurrentWriterFactory(writer);
        }

        public static JsonWriter CreateWriter(StringBuilder sb)
        {
            return CreateWriter(new StringWriter(sb));
        }
    }
}
