#region Copyright (c) 2006 Atif Aziz. All rights reserved.
//
// The MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files
// (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject
// to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

namespace TidyJson
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    #endregion

    sealed class ProgramOptions
    {
        public EventHandler Help;
        public JsonPalette Palette;

        public string[] Parse(string[] args)
        {
            Debug.Assert(args != null);

            var inputs = new Queue<string>(args);
            var anonymous = new Queue<string>(args.Length);

            char? altNamedToken = null;

            if (Path.DirectorySeparatorChar != '/')
                altNamedToken = '/';

            while (inputs.Count > 0)
            {
                var arg = DequeueSafely(inputs);

                if (arg.Length > 1 && (arg[0] == '-' || (altNamedToken.HasValue && arg[0] == altNamedToken.Value)))
                {
                    var parts = arg.Split(new[] { '=', ':' }, 2);
                    var name = parts[0].TrimStart(arg[0]);

                    if (name.Length == 0)
                        break;

                    var value = parts.Length > 1 ? parts[1] : string.Empty;

                    switch (name)
                    {
                        case "p":
                        case "palette":
                        {
                            if (value.Length == 0)
                                continue;

                            if (value[0] != '{')
                                value = "{" + value + "}";

                            Palette.ImportJson(value);
                            break;
                        }

                        case "m":
                        case "mono":
                        {
                            if (Convert.ToBoolean(Mask.EmptyString(value, bool.TrueString)))
                                Palette = new JsonPalette(ConsoleBrush.Current);
                            break;
                        }

                        case "?":
                        case "help":
                        {
                            if (Help != null)
                                Help(this, EventArgs.Empty);
                            break;
                        }

                        default:
                        {
                            throw new ApplicationException(string.Format("Unknown option '{0}'.", arg));
                        }
                    }
                }
                else
                {
                    anonymous.Enqueue(arg);
                }
            }

            args = anonymous.ToArray();
            return args;
        }

        public static void ShowUsage()
        {
            ShowUsage(null);
        }

        public static void ShowUsage(TextWriter output)
        {
            output = output ?? Console.Out;

            output.WriteLine(
                @"Usage: [OPTION]... FILE

where OPTION is one or more of:

--help              print this help
--mono=BOOL         no syntax coloring
--palette=SCHEME    set the color palette

To set the color palette, use the following syntax for the scheme:

    [ BRUSH = COLOR [ , BRUSH = COLOR ] ]

where BRUSH may be:

    Null
    Boolean
    Number
    String
    Object
    Array

and COLOR may be:

    {0}
",
                string.Join(Environment.NewLine + new string(' ', 4),
                    Enum.GetNames(typeof(ConsoleColor))));
        }

        static T DequeueSafely<T>(Queue<T> queue)
        {
            return queue.Count > 0 ? queue.Dequeue() : default(T);
        }
    }
}
