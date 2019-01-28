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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only
    /// access to JSON data over JSON text (RFC 4627).
    /// </summary>

    public sealed class JsonTextReader : JsonReaderBase
    {
        static readonly char[] _numNonDigitChars = { '.', 'e', 'E', '+', '-'};

        BufferedCharReader _reader;
        Stack<Continuation> _stack;
        int _endLineNumber;
        int _endLinePosition;
        int _endCharCount;

        delegate JsonToken Continuation();

        Continuation _methodParse;
        Continuation _methodParseArrayFirst;
        Continuation _methodParseArrayNext;
        Continuation _methodParseObjectMember;
        Continuation _methodParseObjectMemberValue;
        Continuation _methodParseNextMember;

        public JsonTextReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = new BufferedCharReader(reader);
            Push(ParseMethod);
        }

        public int LineNumber   => _reader?.LineNumber   ?? _endLineNumber;
        public int LinePosition => _reader?.LinePosition ?? _endLinePosition;
        public int CharCount    => _reader?.CharCount    ?? _endCharCount;

        /// <summary>
        /// Reads the next token and returns it.
        /// </summary>

        protected override JsonToken ReadTokenImpl()
        {
            if (_stack == null)
            {
                return JsonToken.EOF();
            }
            else if (_stack.Count == 0)
            {
                _stack = null;
                _endLineNumber = _reader.LineNumber;
                _endLinePosition = _reader.LinePosition;
                _endCharCount = _reader.CharCount;
                _reader = null;
                return JsonToken.EOF();
            }
            else
            {
                return Pop()();
            }
        }

        /// <summary>
        /// Parses the next token from the input and returns it.
        /// </summary>
        JsonToken Parse()
        {
            var ch = NextClean();

            //
            // String
            //

            if (ch == '"' || ch == '\'')
            {
                return Yield(JsonToken.String(NextString(ch)));
            }

            //
            // Object
            //

            if (ch == '{')
            {
                _reader.Back();
                return ParseObject();
            }

            //
            // Array
            //

            if (ch == '[')
            {
                _reader.Back();
                return ParseArray();
            }

            //
            // Handle unquoted text. This could be the values true, false, or
            // null, or it can be a number. An implementation (such as this one)
            // is allowed to also accept non-standard forms.
            //
            // Accumulate characters until we reach the end of the text or a
            // formatting character.
            //

            var sb = new StringBuilder();
            var b = ch;

            while (ch >= ' ' && ",:]}/\\\"[{;=#".IndexOf(ch) < 0)
            {
                sb.Append(ch);
                ch = _reader.Next();
            }

            _reader.Back();

            var s = sb.ToString().Trim();

            if (s.Length == 0)
                throw SyntaxError("Missing value.");


            //
            // Boolean
            //

            if (s == JsonBoolean.TrueText || s == JsonBoolean.FalseText)
                return Yield(JsonToken.Boolean(s == JsonBoolean.TrueText));

            //
            // Null
            //

            if (s == JsonNull.Text)
                return Yield(JsonToken.Null());

            //
            // Number
            //
            // Try converting it. We support the 0- and 0x- conventions.
            // If a number cannot be produced, then the value will just
            // be a string. Note that the 0-, 0x-, plus, and implied
            // string conventions are non-standard, but a JSON text parser
            // is free to accept non-JSON text forms as long as it accepts
            // all correct JSON text forms.
            //

            if ((b >= '0' && b <= '9') || b == '.' || b == '-' || b == '+')
            {
                if (b == '0' && s.Length > 1 && s.IndexOfAny(_numNonDigitChars) < 0)
                {
                    if (s.Length > 2 && (s[1] == 'x' || s[1] == 'X'))
                    {
                        var parsed = TryParseHex(s);
                        if (!ReferenceEquals(parsed, s))
                            return Yield(JsonToken.Number(parsed));
                    }
                    else
                    {
                        var parsed = TryParseOctal(s);
                        if (!ReferenceEquals(parsed, s))
                            return Yield(JsonToken.Number(parsed));
                    }
                }
                else
                {
                    if (!JsonNumber.IsValid(s))
                        throw SyntaxError(string.Format("The text '{0}' has the incorrect syntax for a number.", s));

                    return Yield(JsonToken.Number(s));
                }
            }

            //
            // Treat as String in all other cases, e.g. when unquoted.
            //

            return Yield(JsonToken.String(s));
        }

        Continuation ParseMethod => _methodParse ?? (_methodParse = Parse);

        /// <summary>
        /// Parses expecting an array in the source.
        /// </summary>
        JsonToken ParseArray()
        {
            if (NextClean() != '[')
                throw SyntaxError("An array must start with '['.");

            return Yield(JsonToken.Array(), ParseArrayFirstMethod);
        }

        /// <summary>
        /// Parses the first element of an array or the end of the array if
        /// it is empty.
        /// </summary>
        JsonToken ParseArrayFirst()
        {
            if (NextClean() == ']')
                return Yield(JsonToken.EndArray());

            _reader.Back();

            Push(ParseArrayNextMethod);
            return Parse();
        }

        Continuation ParseArrayFirstMethod =>
            _methodParseArrayFirst ?? (_methodParseArrayFirst = ParseArrayFirst);

        /// <summary>
        /// Parses the next element in the array.
        /// </summary>
        JsonToken ParseArrayNext()
        {
            switch (NextClean())
            {
                case ';':
                case ',':
                {
                    if (NextClean() == ']')
                        return Yield(JsonToken.EndArray());
                    else
                        _reader.Back();

                    break;
                }

                case ']':
                {
                    return Yield(JsonToken.EndArray());
                }

                default:
                    throw SyntaxError("Expected a ',' or ']'.");
            }

            Push(ParseArrayNextMethod);
            return Parse();
        }

        Continuation ParseArrayNextMethod =>
            _methodParseArrayNext ?? (_methodParseArrayNext = ParseArrayNext);

        /// <summary>
        /// Parses expecting an object in the source.
        /// </summary>
        JsonToken ParseObject()
        {
            if (NextClean() != '{')
                throw SyntaxError("An object must begin with '{'.");

            return Yield(JsonToken.Object(), ParseObjectMemberMethod);
        }

        /// <summary>
        /// Parses the first member name of the object or the end of the array
        /// in case of an empty object.
        /// </summary>
        JsonToken ParseObjectMember()
        {
            var ch = NextClean();

            if (ch == '}')
                return Yield(JsonToken.EndObject());

            if (ch == BufferedCharReader.EOF)
                throw SyntaxError("An object must end with '}'.");

            _reader.Back();
            var name = Parse().Text;
            return Yield(JsonToken.Member(name), ParseObjectMemberValueMethod);
        }

        Continuation ParseObjectMemberMethod =>
            _methodParseObjectMember ?? (_methodParseObjectMember = ParseObjectMember);

        JsonToken ParseObjectMemberValue()
        {
            var ch = NextClean();

            if (ch == '=')
            {
                if (_reader.Next() != '>')
                    _reader.Back();
            }
            else if (ch != ':')
                throw SyntaxError("Expected a ':' after a key.");

            Push(ParseNextMemberMethod);
            return Parse();
        }

        Continuation ParseObjectMemberValueMethod =>
            _methodParseObjectMemberValue ?? (_methodParseObjectMemberValue = ParseObjectMemberValue);

        JsonToken ParseNextMember()
        {
            switch (NextClean())
            {
                case ';':
                case ',':
                    {
                        if (NextClean() == '}')
                            return Yield(JsonToken.EndObject());
                        break;
                    }

                case '}':
                    return Yield(JsonToken.EndObject());

                default:
                    throw SyntaxError("Expected a ',' or '}'.");
            }

            _reader.Back();
            var name = Parse().Text;
            return Yield(JsonToken.Member(name), ParseObjectMemberValueMethod);
        }

        Continuation ParseNextMemberMethod =>
            _methodParseNextMember ?? (_methodParseNextMember = ParseNextMember);

        /// <summary>
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token and its text.
        /// </summary>
        JsonToken Yield(JsonToken token)
        {
            return Yield(token, null);
        }

        /// <summary>
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token, its text and the next
        /// continuation point into the reader.
        /// </summary>
        /// <remarks>
        /// By itself, this method cannot affect the stack such tha control
        /// is returned back to the reader's user. This must be done by
        /// Yield's caller by way of explicit return.
        /// </remarks>
        JsonToken Yield(JsonToken token, Continuation continuation)
        {
            if (continuation != null)
                Push(continuation);

            return token;
        }

        /// <summary>
        /// Get the next char in the string, skipping whitespace
        /// and comments (slashslash and slashstar).
        /// </summary>
        /// <returns>A character, or 0 if there are no more characters.</returns>
        char NextClean()
        {
            Debug.Assert(_reader != null);

            while (true)
            {
                var ch = _reader.Next();

                if (ch == '/')
                {
                    switch (_reader.Next())
                    {
                        case '/':
                        {
                            do
                            {
                                ch = _reader.Next();
                            } while (ch != '\n' && ch != '\r' && ch != BufferedCharReader.EOF);
                            break;
                        }
                        case '*':
                        {
                            while (true)
                            {
                                ch = _reader.Next();

                                if (ch == BufferedCharReader.EOF)
                                    throw SyntaxError("Unclosed comment.");

                                if (ch == '*')
                                {
                                    if (_reader.Next() == '/')
                                        break;

                                    _reader.Back();
                                }
                            }
                            break;
                        }
                        default:
                        {
                            _reader.Back();
                            return '/';
                        }
                    }
                }
                else if (ch == '#')
                {
                    do
                    {
                        ch = _reader.Next();
                    }
                    while (ch != '\n' && ch != '\r' && ch != BufferedCharReader.EOF);
                }
                else if (ch == BufferedCharReader.EOF || ch > ' ')
                {
                    return ch;
                }
            }
        }

        string NextString(char quote)
        {
            try
            {
                return JsonString.Dequote(_reader, quote);
            }
            catch (FormatException e)
            {
                throw SyntaxError(e.Message, e);
            }
        }

        void Push(Continuation continuation)
        {
            Debug.Assert(continuation != null);

            if (_stack == null)
                _stack = new Stack<Continuation>(6);

            _stack.Push(continuation);
        }

        Continuation Pop()
        {
            Debug.Assert(_stack != null);
            return _stack.Pop();
        }

        JsonException SyntaxError(string message)
        {
            return SyntaxError(message, null);
        }

        JsonException SyntaxError(string message, Exception inner)
        {
            if (LineNumber > 0)
            {
                message = string.Format(
                    "{0} See line {1}, position {2}.",
                    message, LineNumber.ToString("N0"), LinePosition.ToString("N0"));
            }

            return new JsonException(message, inner);
        }

        static string TryParseOctal(string s)
        {
            Debug.Assert(s != null);
            Debug.Assert(s[0] == '0');
            Debug.Assert(s.Length > 1);

            long num = 0;

            try
            {
                for (var i = 1; i < s.Length; i++)
                {
                    var ch = s[i];

                    if (ch < '0' || ch > '8')
                        return s;

                    num = checked(num * 8) | ((uint) ch - 0x30);
                }
            }
            catch (OverflowException)
            {
                return s;
            }

            return num.ToString(CultureInfo.InvariantCulture);
        }

        static string TryParseHex(string s)
        {
            Debug.Assert(s != null);
            Debug.Assert(s.Length > 1);

            try
            {
                var num = long.Parse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return num.ToString(CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                return s;
            }
            catch (FormatException)
            {
                return s;
            }
        }
    }
}
