#region License

//
// Matrix3d.cs
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

//
// This class was adapted from Matrix4d.cs, part of The Open Toolkit library.
//

//
// Copyright (c) 2006 - 2008 The Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

#endregion License

namespace Digitalis.LDTools.DOM.Geom
{
    #region Usings

    using System;
    using System.Runtime.InteropServices;

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents a 3x3 Matrix with double-precision components.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix3d : IEquatable<Matrix3d>
    {
        #region Fields

        /// <summary>
        /// Top row of the matrix.
        /// </summary>
        public Vector3d Row0;

        /// <summary>
        /// Second row of the matrix.
        /// </summary>
        public Vector3d Row1;

        /// <summary>
        /// Third row of the matrix.
        /// </summary>
        public Vector3d Row2;

        /// <summary>
        /// The identity matrix.
        /// </summary>
        public static Matrix3d Identity = new Matrix3d(Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructs a new instance of the <see cref="Matrix3d"/> struct with the specified values.
        /// </summary>
        /// <param name="row0">Top row of the matrix.</param>
        /// <param name="row1">Second row of the matrix.</param>
        /// <param name="row2">Third row of the matrix.</param>
        public Matrix3d(Vector3d row0, Vector3d row1, Vector3d row2)
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="Matrix3d"/> struct with the specified values.
        /// </summary>
        /// <param name="m00">First item of the first row.</param>
        /// <param name="m01">Second item of the first row.</param>
        /// <param name="m02">Third item of the first row.</param>
        /// <param name="m10">First item of the second row.</param>
        /// <param name="m11">Second item of the second row.</param>
        /// <param name="m12">Third item of the second row.</param>
        /// <param name="m20">First item of the third row.</param>
        /// <param name="m21">Second item of the third row.</param>
        /// <param name="m22">Third item of the third row.</param>
        public Matrix3d(double m00, double m01, double m02,
                        double m10, double m11, double m12,
                        double m20, double m21, double m22)
        {
            Row0 = new Vector3d(m00, m01, m02);
            Row1 = new Vector3d(m10, m11, m12);
            Row2 = new Vector3d(m20, m21, m22);
        }

        #endregion Constructors

        #region Indexers

        /// <summary>
        /// Gets or sets the value at a specified row and column.
        /// </summary>
        public double this[int rowIndex, int columnIndex]
        {
            get
            {
                switch (rowIndex)
                {
                    case 0:
                        switch (columnIndex)
                        {
                            case 0: return Row0.X;
                            case 1: return Row0.Y;
                            case 2: return Row0.Z;
                        }
                        break;

                    case 1:
                        switch (columnIndex)
                        {
                            case 0: return Row1.X;
                            case 1: return Row1.Y;
                            case 2: return Row1.Z;
                        }
                        break;

                    case 2:
                        switch (columnIndex)
                        {
                            case 0: return Row2.X;
                            case 1: return Row2.Y;
                            case 2: return Row2.Z;
                        }
                        break;
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (rowIndex)
                {
                    case 0:
                        switch (columnIndex)
                        {
                            case 0: Row0.X = value; return;
                            case 1: Row0.Y = value; return;
                            case 2: Row0.Z = value; return;
                        }
                        break;

                    case 1:
                        switch (columnIndex)
                        {
                            case 0: Row1.X = value; return;
                            case 1: Row1.Y = value; return;
                            case 2: Row1.Z = value; return;
                        }
                        break;

                    case 2:
                        switch (columnIndex)
                        {
                            case 0: Row2.X = value; return;
                            case 1: Row2.Y = value; return;
                            case 2: Row2.Z = value; return;
                        }
                        break;
                }

                throw new IndexOutOfRangeException();
            }
        }

        #endregion Indexers

        #region Properties

        /// <summary>
        /// Gets the determinant of this matrix.
        /// </summary>
        public double Determinant
        {
            get
            {
                return (Row0.X * Row1.Y * Row2.Z) +
                       (Row0.Y * Row1.Z * Row2.X) +
                       (Row0.Z * Row1.X * Row2.Y) -
                       (Row0.X * Row1.Z * Row2.Y) -
                       (Row0.Y * Row1.X * Row2.Z) -
                       (Row0.Z * Row1.Y * Row2.X);
            }
        }

        /// <summary>
        /// Gets the first column of the matrix.
        /// </summary>
        public Vector3d Column0
        {
            get { return new Vector3d(Row0.X, Row1.X, Row2.X); }
            set { Row0.X = value.X; Row1.X = value.Y; Row2.X = value.Z; }
        }

        /// <summary>
        /// Gets the second column of the matrix.
        /// </summary>
        public Vector3d Column1
        {
            get { return new Vector3d(Row0.Y, Row1.Y, Row2.Y); }
            set { Row0.Y = value.X; Row1.Y = value.Y; Row2.Y = value.Z; }
        }

        /// <summary>
        /// Gets the third column of the matrix.
        /// </summary>
        public Vector3d Column2
        {
            get { return new Vector3d(Row0.Z, Row1.Z, Row2.Z); }
            set { Row0.Z = value.X; Row1.Z = value.Y; Row2.Z = value.Z; }
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11 { get { return Row0.X; } set { Row0.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12 { get { return Row0.Y; } set { Row0.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 1, column 3 of the matrix.
        /// </summary>
        public double M13 { get { return Row0.Z; } set { Row0.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21 { get { return Row1.X; } set { Row1.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22 { get { return Row1.Y; } set { Row1.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 2, column 3 of the matrix.
        /// </summary>
        public double M23 { get { return Row1.Z; } set { Row1.Z = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 1 of the matrix.
        /// </summary>
        public double M31 { get { return Row2.X; } set { Row2.X = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 2 of the matrix.
        /// </summary>
        public double M32 { get { return Row2.Y; } set { Row2.Y = value; } }

        /// <summary>
        /// Gets or sets the value at row 3, column 3 of the matrix.
        /// </summary>
        public double M33 { get { return Row2.Z; } set { Row2.Z = value; } }

        #endregion Properties

        #region Instance Methods

        /// <summary>
        /// Converts the matrix into its inverse.
        /// </summary>
        public void Invert()
        {
            this = Matrix3d.Invert(this);
        }

        /// <summary>
        /// Converts the matrix into its transpose.
        /// </summary>
        public void Transpose()
        {
            this = Matrix3d.Transpose(this);
        }

        #endregion Instance Methods

        #region Static Methods

        /// <summary>
        /// Builds a scaling matrix.
        /// </summary>
        /// <param name="scale">Scale factor for x, y and z axes.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix3d Scale(double scale)
        {
            return Scale(scale, scale, scale);
        }

        /// <summary>
        /// Builds a scaling matrix.
        /// </summary>
        /// <param name="scale">Scale factors for x, y and z axes.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix3d Scale(Vector3d scale)
        {
            return Scale(scale.X, scale.Y, scale.Z);
        }

        /// <summary>
        /// Builds a scaling matrix.
        /// </summary>
        /// <param name="x">Scale factor for x-axis.</param>
        /// <param name="y">Scale factor for y-axis.</param>
        /// <param name="z">Scale factor for z-axis.</param>
        /// <returns>A scaling matrix.</returns>
        public static Matrix3d Scale(double x, double y, double z)
        {
            return new Matrix3d(Vector3d.UnitX * x, Vector3d.UnitY * y, Vector3d.UnitZ * z);
        }

        /// <summary>
        /// Builds a rotation matrix.
        /// </summary>
        /// <param name="angle">Angle in radians to rotate counter-clockwise.</param>
        /// <returns>A rotation matrix.</returns>
        public static Matrix3d Rotate(double angle)
        {
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            return new Matrix3d(new Vector3d(cos, -sin, 0.0),
                                new Vector3d(sin,  cos, 0.0),
                                Vector3d.UnitZ);
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new matrix that is the result of the multiplication.</returns>
        public static Matrix3d Mult(Matrix3d left, Matrix3d right)
        {
            Matrix3d result;
            Mult(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new matrix that is the result of the multiplication.</param>
        public static void Mult(ref Matrix3d left, ref Matrix3d right, out Matrix3d result)
        {
            result = new Matrix3d();
            result.M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31;
            result.M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32;
            result.M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33;
            result.M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31;
            result.M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32;
            result.M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33;
            result.M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31;
            result.M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32;
            result.M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33;
        }

        /// <summary>
        /// Calculates the inverse of a matrix.
        /// </summary>
        /// <param name="mat">The matrix to invert.</param>
        /// <returns>The inverse of the matrix if it has one, or the input if it is singular.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the matrix is singular.</exception>
        public static Matrix3d Invert(Matrix3d mat)
        {
            int[] colIdx   = { 0, 0, 0 };
            int[] rowIdx   = { 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1 };

            // convert the matrix to an array for easy looping
            double[,] inverse = { { mat.Row0.X, mat.Row0.Y, mat.Row0.Z },
                                  { mat.Row1.X, mat.Row1.Y, mat.Row1.Z },
                                  { mat.Row2.X, mat.Row2.Y, mat.Row2.Z } };
            int icol = 0;
            int irow = 0;

            for (int i = 0; i < 3; i++)
            {
                // Find the largest pivot value
                double maxPivot = 0.0;

                for (int j = 0; j < 3; j++)
                {
                    if (0 != pivotIdx[j])
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (-1 == pivotIdx[k])
                            {
                                double absVal = System.Math.Abs(inverse[j, k]);

                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow     = j;
                                    icol     = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return mat;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        double f = inverse[irow, k];

                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                double pivot = inverse[icol, icol];

                // check for singular matrix
                if (0.0 == pivot)
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

                // Scale row so it has a unit diagonal
                double oneOverPivot = 1.0 / pivot;

                inverse[icol, icol] = 1.0;

                for (int k = 0; k < 3; k++)
                {
                    inverse[icol, k] *= oneOverPivot;
                }

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 3; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        double f = inverse[j, icol];

                        inverse[j, icol] = 0.0;

                        for (int k = 0; k < 3; k++)
                        {
                            inverse[j, k] -= inverse[icol, k] * f;
                        }
                    }
                }
            }

            for (int j = 2; j >= 0; j--)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];

                for (int k = 0; k < 3; k++)
                {
                    double f = inverse[k, ir];

                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            mat.Row0 = new Vector3d(inverse[0, 0], inverse[0, 1], inverse[0, 2]);
            mat.Row1 = new Vector3d(inverse[1, 0], inverse[1, 1], inverse[1, 2]);
            mat.Row2 = new Vector3d(inverse[2, 0], inverse[2, 1], inverse[2, 2]);
            return mat;
        }

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        public static Matrix3d Transpose(Matrix3d mat)
        {
            return new Matrix3d(mat.Column0, mat.Column1, mat.Column2);
        }


        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <param name="result">The result of the calculation</param>
        public static void Transpose(ref Matrix3d mat, out Matrix3d result)
        {
            result.Row0 = mat.Column0;
            result.Row1 = mat.Column1;
            result.Row2 = mat.Column2;
        }

        #endregion Static Methods

        #region Operators

        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix3d which holds the result of the multiplication</returns>
        public static Matrix3d operator *(Matrix3d left, Matrix3d right)
        {
            return Matrix3d.Mult(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Matrix3d left, Matrix3d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Matrix3d left, Matrix3d right)
        {
            return !left.Equals(right);
        }

        #endregion Operators

        #region Overrides

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current Matrix3d.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}\n{1}\n{2}\n{3}", Row0, Row1, Row2);
        }

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A <see cref="T:System.Int32"/> containing the unique hashcode for this instance.</returns>
        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode() ^ Row2.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix3d))
                return false;

            return this.Equals((Matrix3d)obj);
        }

        #endregion Overrides

        #region IEquatable<Matrix3d> Members

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(Matrix3d other)
        {
            return (Row0 == other.Row0 && Row1 == other.Row1 && Row2 == other.Row2);
        }

        #endregion IEquatable<Matrix3d> Members
    }
}
