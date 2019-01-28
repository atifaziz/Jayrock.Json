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
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Jayrock.Json;

    #endregion

    static class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var defaultBrush = ConsoleBrush.Current;

                var options = new ProgramOptions();
                options.Help += delegate { Help(); Environment.Exit(0); };
                options.Palette = JsonPalette.Auto(defaultBrush);
                args = options.Parse(args);

                var path = args.Length > 0 ? args[0] : "-";

                try
                {
                    try
                    {
                        PrettyColorPrint(path, Console.Out, options.Palette);
                    }
                    finally
                    {
                        //
                        // The location of this finally clause is significant
                        // and should not be merged with the outer catch
                        // block. The default brush needs to be restored
                        // in case an error message is about to be printed
                        // and the standard output and error point to the
                        // same console device.
                        //

                        defaultBrush.Apply();
                    }
                }
                catch (JsonException e)
                {
                    //
                    // In case of JsonException, we don't display the
                    // base exception since the root cause would not provide
                    // line and position information and which JsonException
                    // does. For example, "Unterminated string" has the
                    // root case of FormatException, but which bubble as
                    // JsonException with line and position about where the
                    // error was found in the source.
                    //

                    Console.Error.WriteLine(e.Message);
                    Trace.WriteLine(e.ToString());
                    return 2;
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.GetBaseException().Message);
                Trace.WriteLine(e.ToString());
                return 1;
            }
        }

        static void PrettyColorPrint(string path, TextWriter output, JsonPalette palette)
        {
            Debug.Assert(output != null);

            using (var input = path.Equals("-") ? Console.In : new StreamReader(path))
            using (var reader = new JsonTextReader(input))
            using (var writer = new JsonTextWriter(output))
            {
                writer.PrettyPrint = true;
                var colorWriter = new JsonColorWriter(writer, palette);
                colorWriter.WriteFromReader(reader);
                output.WriteLine();
            }
        }

        #region Help (Logo, Usage and Disclaimer)

        static void Help()
        {
            WriteLogo();
            ProgramOptions.ShowUsage();
            Console.WriteLine(@"

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.");
        }

        static void WriteLogo()
        {
            var assembly = typeof(Program).Assembly;

            Console.WriteLine("{0}, v{1}",
                GetCustomAttribute<AssemblyTitleAttribute>().Title,
                assembly.GetName().Version);
            Console.WriteLine(GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
            Console.WriteLine("Written by Atif Aziz -- http://www.raboof.com/");
            Console.WriteLine(GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
            Console.WriteLine();

            T GetCustomAttribute<T>() where T : Attribute =>
                assembly.GetCustomAttribute<T>()
                ?? throw new ObjectNotFoundException(string.Format("The attribute {0} was not found.", typeof(T).FullName));
        }

        #endregion
    }
}
