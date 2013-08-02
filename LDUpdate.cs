#region License

//
// LDUpdate.cs
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

    #endregion Usings

    /// <summary>
    /// Represents the 'update' information for an LDraw library file.
    /// </summary>
    [Serializable]
    public struct LDUpdate : IEquatable<LDUpdate>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the year this file was released.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For files released as part of the original LDraw Parts Library, this will be zero; otherwise, values of less than 100
        /// will be assumed to be in the 1901..1999 range, and adjusted accordingly.
        /// </para>
        /// </remarks>
        public uint Year { get { return _year; } set { if (0 != value && value < 100) value += 1900; _year = value; if (0 == value) Release = 0; } }
        private uint _year;

        /// <summary>
        /// Gets or sets the number of the release this file was included in.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For files released as part of the original LDraw Parts Library, this will be zero.
        /// </para>
        /// </remarks>
        public uint Release { get { return _release; } set { _release = value; if (0 == value) Year = 0; } }
        private uint _release;

        /// <summary>
        /// Gets a value indicating whether the <see cref="LDUpdate"/> represents a file from the original LDraw Parts Library.
        /// </summary>
        public bool IsOriginal { get { return (0 == Year || 0 == Release); } }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDUpdate"/> struct with the specified values.
        /// </summary>
        /// <param name="year">The year of release.</param>
        /// <param name="release">The release number.</param>
        /// <remarks>
        /// <para>
        /// For files released as part of the original LDraw Parts Library, pass zero for either or both values; otherwise, values for <paramref name="year"/>
        /// of less than 100 will be assumed to be in the 1901..1999 range, and adjusted accordingly.
        /// </para>
        /// </remarks>
        public LDUpdate(uint year, uint release)
        {
            if (0 == year || 0 == release)
            {
                year    = 0;
                release = 0;
            }
            else
            {
                if (year < 100)
                    year += 1900;

                _year = year;
            }

            _year    = year;
            _release = release;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDUpdate"/> struct with the specified values.
        /// </summary>
        /// <param name="code">Valid LDraw 'update' code.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'update' code.</exception>
        /// <example>
        /// <code>
        /// LDUpdate update = new LDUpdate("UPDATE 2012-01");
        /// </code>
        /// </example>
        public LDUpdate(string code)
        {
            code = code.Trim();

            if (code.StartsWith("ORIGINAL"))
            {
                _year    = 0;
                _release = 0;
            }
            else if (code.StartsWith("UPDATE"))
            {
                string[] fields = code.Split(new char[] { ' ', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length < 3)
                    throw new FormatException("LDraw Update code must have at least three fields");

                _year    = uint.Parse(fields[1]);
                _release = uint.Parse(fields[2]);
            }
            else if (code.EndsWith("UPDATE"))
            {
                string[] fields = code.Split(new char[] { ' ', '\t', '-' }, StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length < 3)
                    throw new FormatException("LDraw Update code must have at least three fields");

                _year    = uint.Parse(fields[0]);
                _release = uint.Parse(fields[1]);
            }
            else
            {
                throw new FormatException("Invalid code format");
            }

            if (0 != _year && _year < 100)
                _year += 1900;
        }

        #endregion Constructor

        #region Code-generation

        /// <summary>
        /// Returns the <see cref="LDUpdate"/> as LDraw code.
        /// </summary>
        /// <returns>LDraw 'update' code</returns>
        public string ToCode()
        {
            if (0 == Year || 0 == Release)
                return "ORIGINAL";

            return String.Format("UPDATE {0}-{1:00}", Year, Release);
        }

        #endregion Code-generation

        #region API

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is LDUpdate))
                return false;

            return Equals((LDUpdate)other);
        }

        /// <inheritdoc />
        public bool Equals(LDUpdate other)
        {
            return Year == other.Year && Release == other.Release;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Year.GetHashCode() ^ Release.GetHashCode();
        }

        #endregion API

        #region Operators

        /// <summary>
        /// Performs an equality test on two <see cref="LDUpdate"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="LDUpdate"/>.</param>
        /// <param name="b">The second <see cref="LDUpdate"/>.</param>
        /// <returns><c>true</c> if the two <see cref="LDUpdate"/>s are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(LDUpdate a, LDUpdate b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Performs an inequality test on two <see cref="LDUpdate"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="LDUpdate"/>.</param>
        /// <param name="b">The second <see cref="LDUpdate"/>.</param>
        /// <returns><c>true</c> if the two <see cref="LDUpdate"/>s are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(LDUpdate a, LDUpdate b)
        {
            return !a.Equals(b);
        }

        #endregion Operators
    }
}
