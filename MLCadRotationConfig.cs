#region License

//
// MLCadRotationConfig.cs
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
    using System.Globalization;

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents an MLCad rotation-configuration.
    /// </summary>
    [Serializable]
    public struct MLCadRotationConfig : IEquatable<MLCadRotationConfig>
    {
        #region Inner types

        /// <summary>
        /// The rotation mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.APi.IPage"/> holds an array of <see cref="MLCadRotationConfig"/>, each of which represents a user-defined rotation-configuration.
        /// The active configuration is selected by setting <see cref="P:Digitalis.LDTools.DOM.API.IPage.RotationPoint"/> to the index of the chosen configuration.  In addition,
        /// three pre-defined configurations exist, which are selected by setting <see cref="P:Digitalis.LDTools.DOM.API.IPage.RotationPoint"/> to one of these values.
        /// </para>
        /// </remarks>
        public enum Type
        {
            /// <summary>
            /// Rotation takes place around the origin of the selected part.
            /// </summary>
            PartOrigin = 0,

            /// <summary>
            /// Rotation takes place around the centre of the selected part's <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.BoundingBox"/>.
            /// </summary>
            PartCentre = -1,

            /// <summary>
            /// Rotation takes place around the first user-defined rotation-configuration of the selected part, or the part's origin if no user-defined configurations exist.
            /// </summary>
            PartRotationPoint = -2,

            /// <summary>
            /// Rotation takes place around the origin of the <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>.
            /// </summary>
            WorldOrigin = -3
        };

        #endregion Inner types

        #region Properties

        /// <summary>
        /// The point around which rotation takes place, relative to the origin of the <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>.
        /// </summary>
        public Vector3d Point;

        /// <summary>
        /// Gets or sets a value which indicates whether the centre of rotation may be changed on-screen by the user.
        /// </summary>
        public bool AllowChange;

        /// <summary>
        /// Gets or sets the name of the <see cref="MLCadRotationConfig"/>.
        /// </summary>
        public string Name;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCadRotationConfig"/> class with the specified values.
        /// </summary>
        /// <param name="point">The point around which rotation takes place.</param>
        /// <param name="allowChange">Whether the centre of rotation may be changed on-screen by the user.</param>
        /// <param name="name">The name for the <see cref="MLCadRotationConfig"/>.</param>
        public MLCadRotationConfig(Vector3d point, bool allowChange, string name)
        {
            Point       = point;
            AllowChange = allowChange;
            Name        = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MLCadRotationConfig"/> class with the specified values.
        /// </summary>
        /// <param name="code">Valid MLCad <i>ROTATION CENTER</i> code.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid MLCad <i>ROTATION CENTER</i> code.</exception>
        /// <example>
        /// <code>
        /// MLCadRotationConfig cfg = new MLCadRotationConfig("0 ROTATION CENTER 0 0 0 1 \"Point Name\"");
        /// </code>
        /// </example>
        public MLCadRotationConfig(string code)
        {
            string[] fields = code.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 8)
                throw new FormatException("MLCadRotationConfig code must have at least 8 fields");

            Point       = new Vector3d(double.Parse(fields[3], CultureInfo.InvariantCulture), double.Parse(fields[4], CultureInfo.InvariantCulture), double.Parse(fields[5], CultureInfo.InvariantCulture));
            AllowChange = (1 == int.Parse(fields[6]));
            Name        = code.Substring(code.IndexOf(fields[7])).Trim(new char[] { ' ', '"' });
        }

        #endregion Constructor

        #region API

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (!(other is MLCadRotationConfig))
                return false;

            return Equals((MLCadRotationConfig)other);
        }

        /// <inheritdoc />
        public bool Equals(MLCadRotationConfig other)
        {
            return (Point == other.Point && AllowChange == other.AllowChange && Name == other.Name);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = Point.GetHashCode() ^ AllowChange.GetHashCode();

            if (null != Name)
                hash ^= Name.GetHashCode();

            return hash;
        }

        #endregion API

        #region Operators

        /// <summary>
        /// Performs an equality test on two <see cref="MLCadRotationConfig"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="MLCadRotationConfig"/>.</param>
        /// <param name="b">The second <see cref="MLCadRotationConfig"/>.</param>
        /// <returns><c>true</c> if the two <see cref="MLCadRotationConfig"/>s are equal; <c>false</c> otherwise.</returns>
        public static bool operator == (MLCadRotationConfig a, MLCadRotationConfig b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Performs an inequality test on two <see cref="MLCadRotationConfig"/>s.
        /// </summary>
        /// <param name="a">The first <see cref="MLCadRotationConfig"/>.</param>
        /// <param name="b">The second <see cref="MLCadRotationConfig"/>.</param>
        /// <returns><c>true</c> if the two <see cref="MLCadRotationConfig"/>s are not equal; <c>false</c> otherwise.</returns>
        public static bool operator != (MLCadRotationConfig a, MLCadRotationConfig b)
        {
            return !a.Equals(b);
        }

        #endregion Operators
    }
}
