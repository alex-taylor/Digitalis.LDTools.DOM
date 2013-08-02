#region License

//
// Box3d.cs
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

namespace Digitalis.LDTools.DOM.Geom
{
    #region Usings

    using System;

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents a three-dimensional axis-aligned cuboid with double-precision components.
    /// </summary>
    [Serializable]
    public struct Box3d : IEquatable<Box3d>
    {
        #region Fields

        private Vector3d _p1;
        private Vector3d _p2;
        private bool     _valid;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the first vertex of the <see cref="Box3d"/>.
        /// </summary>
        public Vector3d Point1 { get { return _p1; } set { _p1 = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the second vertex of the <see cref="Box3d"/>.
        /// </summary>
        public Vector3d Point2 { get { return _p2; } set { _p2 = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the x-component of the first vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double X1 { get { return _p1.X; } set { _p1.X = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the y-component of the first vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double Y1 { get { return _p1.Y; } set { _p1.Y = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the z-component of the first vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double Z1 { get { return _p1.Z; } set { _p1.Z = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the x-component of the second vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double X2 { get { return _p2.X; } set { _p2.X = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the y-component of the second vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double Y2 { get { return _p2.Y; } set { _p2.Y = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the z-component of the second vertex of the <see cref="Box3d"/>.
        /// </summary>
        public double Z2 { get { return _p2.Z; } set { _p2.Z = value; _valid = true; } }

        /// <summary>
        /// Gets or sets the x-size of the <see cref="Box3d"/>.
        /// </summary>
        public double SizeX { get { return _p2.X - _p1.X; } set { _p2.X = value - _p1.X; _valid = true; } }

        /// <summary>
        /// Gets or sets the y-size of the <see cref="Box3d"/>.
        /// </summary>
        public double SizeY { get { return _p2.Y - _p1.Y; } set { _p2.Y = value - _p1.Y; _valid = true; } }

        /// <summary>
        /// Gets or sets the z-size of the <see cref="Box3d"/>.
        /// </summary>
        public double SizeZ { get { return _p2.Z - _p1.Z; } set { _p2.Z = value - _p1.Z; _valid = true; } }

        /// <summary>
        /// Gets the distance from <see cref="Point1"/> to <see cref="Point2"/>.
        /// </summary>
        public double Length { get { return (Point2 - Point1).Length; } }

        /// <summary>
        /// Gets or sets the centre of the <see cref="Box3d"/>.
        /// </summary>
        public Vector3d Centre
        {
            get { return new Vector3d(X1 + SizeX / 2, Y1 + SizeY / 2, Z1 + SizeZ / 2); }
            set { Vector3d diff = value - Centre; Point1 += diff; Point2 += diff; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Box3d"/> struct with the specified values.
        /// </summary>
        /// <param name="x1">The x-component of the first vertex.</param>
        /// <param name="y1">The y-component of the first vertex.</param>
        /// <param name="z1">The z-component of the first vertex.</param>
        /// <param name="x2">The x-component of the second vertex.</param>
        /// <param name="y2">The y-component of the second vertex.</param>
        /// <param name="z2">The z-component of the second vertex.</param>
        public Box3d(double x1, double y1, double z1, double x2, double y2, double z2)
            : this(new Vector3d(x1, y1, z1), new Vector3d(x2, y2, z2))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Box3d"/> struct with the specified values.
        /// </summary>
        /// <param name="p1">The first vertex.</param>
        /// <param name="p2">The second vertex.</param>
        public Box3d(Vector3d p1, Vector3d p2)
        {
            _p1.X = p1.X < p2.X ? p1.X : p2.X;
            _p1.Y = p1.Y < p2.Y ? p1.Y : p2.Y;
            _p1.Z = p1.Z < p2.Z ? p1.Z : p2.Z;

            _p2.X = p1.X > p2.X ? p1.X : p2.X;
            _p2.Y = p1.Y > p2.Y ? p1.Y : p2.Y;
            _p2.Z = p1.Z > p2.Z ? p1.Z : p2.Z;

            _valid = true;
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Returns a value indicating whether the <see cref="Box3d"/> contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns><c>true</c> if the <see cref="Box3d"/> contains the specified point; otherwise, <c>false</c>.</returns>
        public bool Contains(Vector3d point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Returns a value indicating whether the <see cref="Box3d"/> contains the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns><c>true</c> if the <see cref="Box3d"/> contains the specified point; otherwise, <c>false</c>.</returns>
        public bool Contains(ref Vector3d point)
        {
            if (point.X < _p1.X || point.Y < _p1.Y || point.Z < _p1.Z || point.X > _p2.X || point.Y > _p2.Y || point.Z > _p2.Z)
                return false;

            return true;
        }

        /// <summary>
        /// Adds a point to the <see cref="Box3d"/>.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void Union(Vector3d point)
        {
            Union(ref point);
        }

        /// <summary>
        /// Adds a point to the <see cref="Box3d"/>.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void Union(ref Vector3d point)
        {
            if (!_valid)
            {
                _p1 = _p2 = point;
                _valid    = true;
                return;
            }

            if (point.X < _p1.X)
                _p1.X = point.X;

            if (point.Y < _p1.Y)
                _p1.Y = point.Y;

            if (point.Z < _p1.Z)
                _p1.Z = point.Z;

            if (point.X > _p2.X)
                _p2.X = point.X;

            if (point.Y > _p2.Y)
                _p2.Y = point.Y;

            if (point.Z > _p2.Z)
                _p2.Z = point.Z;
        }

        /// <summary>
        /// Adds two <see cref="Box3d"/>s together.
        /// </summary>
        /// <param name="box">The box to be addded.</param>
        public void Union(Box3d box)
        {
            Union(ref box);
        }

        /// <summary>
        /// Adds two <see cref="Box3d"/>s together.
        /// </summary>
        /// <param name="box">The box to be addded.</param>
        public void Union(ref Box3d box)
        {
            Union(ref box._p1);
            Union(ref box._p2);
        }

        /// <summary>
        /// Transforms a <see cref="Box3d"/>.
        /// </summary>
        /// <param name="box">The box to transform.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="result">The transformed box.</param>
        public static void Transform(ref Box3d box, ref Matrix4d transform, out Box3d result)
        {
            if (Matrix4d.Identity == transform)
            {
                result = box;
                return;
            }

            Vector3d[] vertices = new Vector3d[8];
            Vector3d[] results  = new Vector3d[8];

            vertices[0] = new Vector3d(box.X1, box.Y1, box.Z1);
            vertices[1] = new Vector3d(box.X2, box.Y1, box.Z1);
            vertices[2] = new Vector3d(box.X2, box.Y2, box.Z1);
            vertices[3] = new Vector3d(box.X1, box.Y2, box.Z1);
            vertices[4] = new Vector3d(box.X1, box.Y1, box.Z2);
            vertices[5] = new Vector3d(box.X2, box.Y1, box.Z2);
            vertices[6] = new Vector3d(box.X2, box.Y2, box.Z2);
            vertices[7] = new Vector3d(box.X1, box.Y2, box.Z2);

            for (int n = 0; n < vertices.Length; n++)
            {
                Vector3d.TransformPosition(ref vertices[n], ref transform, out results[n]);
            }

            result = new Box3d(results[0], results[0]);

            for (int n = 1; n < results.Length; n++)
            {
                result.Union(ref results[n]);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Point1.ToString() + " " + Point2.ToString();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Point1.GetHashCode() ^ Point2.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is Box3d))
                return false;

            return this.Equals((Box3d)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Box3d other)
        {
            return Point1 == other.Point1 && Point2 == other.Point2;
        }

        #endregion Methods

        #region Operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Box3d left, Box3d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Box3d left, Box3d right)
        {
            return !left.Equals(right);
        }

        #endregion Operators
     }
}
