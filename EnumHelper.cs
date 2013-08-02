#region License

//
// EnumHelper.cs
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

namespace Digitalis.LDTools.DOM
{
    #region Usings

    using System;
    using System.Collections.Generic;

    #endregion Usings

    internal static class EnumHelper
    {
        private struct HashEntry
        {
            public string[] Names;
            public Array    Values;
        }

        // for efficiency, we store the name->value mapping for each Enum in here
        private static Dictionary<Type, HashEntry> _enums = new Dictionary<Type, HashEntry>();

        /// <summary>
        /// Returns the value from an Enum which corresponds to the supplied string.
        /// </summary>
        /// <typeparam name="TEnum">The type of the Enum.</typeparam>
        /// <param name="value">The string representation of the Enum value.</param>
        /// <param name="ignoreCase">If set to <c>true</c> the match is case-insensitive.</param>
        /// <param name="result">The Enum value, if found.</param>
        /// <returns><c>true</c> if a match was found in the Enum; <c>false</c> otherwise.</returns>
        public static bool ToValue<TEnum>(string value, bool ignoreCase, out TEnum result)
        {
            HashEntry entry;

            lock (_enums)
            {
                // see if we've already stored a set of results for this Enum and add one if not
                if (!_enums.TryGetValue(typeof(TEnum), out entry))
                {
                    entry        = new HashEntry();
                    entry.Names  = Enum.GetNames(typeof(TEnum));
                    entry.Values = Enum.GetValues(typeof(TEnum));
                    _enums.Add(typeof(TEnum), entry);
                }
            }

            if (ignoreCase)
            {
                for (int i = 0; i < entry.Names.Length; i++)
                {
                    if (0 == string.Compare(entry.Names[i], value, StringComparison.OrdinalIgnoreCase))
                    {
                        result = (TEnum)entry.Values.GetValue(i);
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < entry.Names.Length; i++)
                {
                    if (entry.Names[i].Equals(value))
                    {
                        result = (TEnum)entry.Values.GetValue(i);
                        return true;
                    }
                }
            }

            // have to set something even if the lookup failed
            result = (TEnum)(object)0;
            return false;
        }
    }
}