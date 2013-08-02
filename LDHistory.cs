#region License

//
// LDHistory.cs
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
    using System.Text;
    using System.Text.RegularExpressions;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw.org !HISTORY entry.
    /// </summary>
    [Serializable]
    public struct LDHistory : IEquatable<LDHistory>
    {
        #region Properties

        /// <summary>
        /// Specifies the year.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to zero if the year is not known.
        /// </para>
        /// <para>
        /// Default value is zero.
        /// </para>
        /// </remarks>
        public int Year;

        /// <summary>
        /// Specifies the month.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to zero if the month is not known.
        /// </para>
        /// <para>
        /// Default value is zero.
        /// </para>
        /// </remarks>
        public int Month;

        /// <summary>
        /// Specifies the day.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set to zero if the day is not known.
        /// </para>
        /// <para>
        /// Default value is zero.
        /// </para>
        /// </remarks>
        public int Day;

        /// <summary>
        /// Specifies the name of the user who created the entry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This may be either the user's real name or their LDraw.org username.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsRealName"/>.
        public string Name;

        /// <summary>
        /// Specifies whether <see cref="Name"/> is the user's real name or their LDraw.org username.
        /// </summary>
        public bool IsRealName;

        /// <summary>
        /// Specifies details of the change.
        /// </summary>
        public string Description;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDHistory"/> class with default values.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// <para>
        /// The new <see cref="LDHistory"/> object will have <see cref="Name"/> set to either <see cref="P:Digitalis.LDTools.DOM.Configuration.Username"/> or
        /// <see cref="P:Digitalis.LDTools.DOM.Configuration.Author"/>, depending on which is available. If neither is available <see cref="Name"/> will be set to <c>null</c>.
        /// </para>
        /// </remarks>
        public LDHistory(DateTime date, string description)
        {
            if (!String.IsNullOrEmpty(Configuration.Username))
            {
                Name       = Configuration.Username;
                IsRealName = false;
            }
            else if (!String.IsNullOrEmpty(Configuration.Author))
            {
                Name       = Configuration.Author;
                IsRealName = true;
            }
            else
            {
                Name       = null;
                IsRealName = false;
            }

            Year        = date.Year;
            Month       = date.Month;
            Day         = date.Day;
            Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDHistory"/> class with the specified values.
        /// </summary>
        /// <param name="code">Valid LDraw !HISTORY code.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw !HISTORY code.</exception>
        /// <example>
        /// <code>
        /// LDHistory history = new LDHistory("0 !HISTORY 2011-05-24 [username] description");
        /// </code>
        /// </example>
        public LDHistory(string code)
        {
            string[] words = code.Split(new char[]{ ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length < 3)
                throw new FormatException("LDraw History code must have at least 3 fields");

            int idx = 2;

            Year       = 0;
            Month      = 0;
            Day        = 0;
            Name       = null;
            IsRealName = false;

            if (Regex.IsMatch(words[idx], "^.+-.+-.+$"))
            {
                string[] date = words[idx++].Split(new char[]{ '-' }, StringSplitOptions.RemoveEmptyEntries);

                if (!int.TryParse(date[0], out Year))
                    Year = 0;

                if (!int.TryParse(date[1], out Month))
                    Month = 0;

                if (!int.TryParse(date[2], out Day))
                    Day = 0;

                // there are several known errors in the library, all of which we need to recognise and correct

                // first, a date with the year and day fields transposed
                if (Year < 100 && Day > 100)
                {
                    int tmp = Day;
                    Day     = Year;
                    Year    = tmp;
                }

                // second, a date with the month and day fields transposed
                if (Month > 12 && Day <= 12)
                {
                    int tmp = Month;
                    Month   = Day;
                    Day     = tmp;
                }

                // third, a miskeying of the month or day field such that the two digits were transposed
                if (Month > 12)
                {
                    int units = Month / 10;
                    Month = (Month - units * 2) * 10 + units;
                }

                if (Day > 31)
                {
                    int units = Day / 10;
                    Day = (Day - units * 2) * 10 + units;
                }
            }

            int nameStart = -1;

            if ('[' == words[idx][0])
            {
                nameStart = code.IndexOf('[') + 1;
            }
            else if ('{' == words[idx][0])
            {
                IsRealName = true;
                nameStart  = code.IndexOf('{') + 1;
            }

            if (-1 != nameStart)
            {
                int nameEnd = code.IndexOf('}');

                if (-1 == nameEnd)
                    nameEnd = code.IndexOf(']');

                if (-1 != nameEnd)
                {
                    int nameLength = nameEnd - nameStart;

                    Name       = code.Substring(nameStart, nameLength);
                    nameStart += nameLength + 1;
                }
            }
            else
            {
                nameStart = code.IndexOf(words[idx]);
            }

            Description = code.Substring(nameStart).Trim();
        }

        #endregion Constructor

        #region Code-generation

        /// <summary>
        /// Returns the <see cref="LDHistory"/> as LDraw code.
        /// </summary>
        public string ToCode()
        {
            return "0 !HISTORY " + ToString();
        }

        #endregion Code-generation

        #region API

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (0 != Year || 0 != Month || 0 != Day)
            {
                if (0 == Year)
                    sb.Append("????-");
                else
                    sb.Append(Year.ToString("0000") + "-");

                if (0 == Month)
                    sb.Append("??-");
                else
                    sb.Append(Month.ToString("00") + "-");

                if (0 == Day)
                    sb.Append("??");
                else
                    sb.Append(Day.ToString("00"));
            }

            if (!String.IsNullOrWhiteSpace(Name))
            {
                if (IsRealName)
                    sb.Append(" {" + Name + "}");
                else
                    sb.Append(" [" + Name + "]");
            }

            if (!String.IsNullOrWhiteSpace(Description))
                sb.Append(" " + Description);

            sb.Append("\r\n");

            return sb.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is LDHistory))
                return false;

            return Equals((LDHistory)other);
        }

        /// <inheritdoc />
        public bool Equals(LDHistory other)
        {
            return Year == other.Year && Month == other.Month && Day == other.Day && Name == other.Name && IsRealName == other.IsRealName && Description == other.Description;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = Year.GetHashCode() ^ Month.GetHashCode() ^ Day.GetHashCode() ^ IsRealName.GetHashCode();

            if (null != Name)
                hash ^= Name.GetHashCode();

            if (null != Description)
                hash ^= Description.GetHashCode();

            return hash;
        }

        #endregion API

        #region Operators

        /// <summary>
        /// Performs an equality test on two <see cref="LDHistory"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="LDHistory"/>.</param>
        /// <param name="b">The second <see cref="LDHistory"/>.</param>
        /// <returns><b>true</b> if the two <see cref="LDHistory"/>s are equal; <b>false</b> otherwise.</returns>
        public static bool operator == (LDHistory a, LDHistory b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Performs an inequality test on two <see cref="LDHistory"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="LDHistory"/>.</param>
        /// <param name="b">The second <see cref="LDHistory"/>.</param>
        /// <returns><b>true</b> if the two <see cref="LDHistory"/>s are not equal; <b>false</b> otherwise.</returns>
        public static bool operator != (LDHistory a, LDHistory b)
        {
            return !a.Equals(b);
        }

        #endregion Operators
    }
}
