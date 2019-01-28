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

namespace TidyJson.Properties
{
    using System.Collections.Generic;

    internal sealed partial class Settings
    {
        public static readonly Settings Default = new Settings();

        public const string WhitePalette = "{nil=DarkGray,str=Blue,num=DarkGreen,bit=DarkMagenta,obj=Red,arr=Red,mem=DarkCyan}";
        public const string BlackPalette = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";

        static readonly Dictionary<string, string> Config = new Dictionary<string, string>
        {
            [nameof(WhitePalette)] = WhitePalette,
            [nameof(BlackPalette)] = BlackPalette
        };

        public string this[string name] => Config[name];
    }
}
