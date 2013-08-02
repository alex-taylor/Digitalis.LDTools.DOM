#region License
//
// IntHelper.cs
//
// Copyright (C) 2009-2013 Alex Taylor.  All Rights Reserved.
//
// This file is part of Digitalis.LDTools.DOM.dll
//
// Digitalis.LDTools.DOM.dll is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Digitalis.LDTools.DOM.dll is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Digitalis.LDTools.DOM.dll.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion License

namespace Digitalis.LDTools.DOM
{
    using System.Globalization;

    internal class IntHelper
    {
        internal static int ParseInt(string s)
        {
            if ('#' == s[0])
                return int.Parse(s.Substring(1), NumberStyles.HexNumber);
            else if (s.StartsWith("0x") || s.StartsWith("0X"))
                return int.Parse(s.Substring(2), NumberStyles.HexNumber);
            else
                return int.Parse(s);
        }
    }
}
