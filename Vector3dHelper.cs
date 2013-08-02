#region License

//
// Vector3dHelper.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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

namespace Digitalis.LDTools.Utils
{
    #region Usings

    using System;
    using System.Globalization;

    using OpenTK;

    using Configuration = Digitalis.LDTools.DOM.Configuration;

    #endregion Usings

    /// <summary>
    /// Contains extension methods for <see cref="T:OpenTK.Vector3d"/>.
    /// </summary>
    public static class Vector3dHelper
    {
        private static string   ListSeparator  = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        private static string[] ListSeparators = new string[] { ListSeparator, " ", "\t" };
        private static char[]   TrimChars      = new char[] { ' ', '(', ')' };

        /// <summary>
        /// Returns a System.String that represents the current <see cref="T:OpenTK.Vector3d"/>, trimmed to a specified number of decimal-places.
        /// </summary>
        /// <param name="v">A <see cref="T:OpenTK.Vector3d"/></param>
        /// <param name="numDecimalPlaces">The maximum number of decimal-places to display, from 0..15.</param>
        /// <returns>A formatted string.</returns>
        public static string ToString(this Vector3d v, uint numDecimalPlaces)
        {
            string formatter = Configuration.Formatters[numDecimalPlaces];

            return String.Format("({0}{3} {1}{3} {2})", v.X.ToString(formatter), v.Y.ToString(formatter), v.Z.ToString(formatter), ListSeparator);
        }

        /// <summary>
        /// Converts the string representation of a <see cref="T:OpenTK.Vector3d"/> to a <see cref="T:OpenTK.Vector3d"/> struct.
        /// </summary>
        /// <param name="s">A string that contains a <see cref="T:OpenTK.Vector3d"/> to convert.</param>
        /// <returns>A <see cref="T:OpenTK.Vector3d"/> that is equivalent to the value specified in <paramref name="s"/>.</returns>
        /// <exception cref="T:System.FormatException"><paramref name="s"/> was not in a recognised format.</exception>
        /// <remarks>
        /// <para>
        /// <paramref name="s"/> must contain three numeric values, separated by either whitespace or the current culture's
        /// <see cref="P:System.Globalization.TextInfo.ListSeparator">ListSeparator</see> and optionally enclosed in parentheses.
        /// </para>
        /// </remarks>
        public static Vector3d Parse(string s)
        {
            string[] groups = s.Trim(TrimChars).Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (3 != groups.Length)
                throw new FormatException("Input must contain 3 values");

            Vector3d v = new Vector3d();

            v.X = double.Parse(groups[0]);
            v.Y = double.Parse(groups[1]);
            v.Z = double.Parse(groups[2]);

            return v;
        }
    }
}
