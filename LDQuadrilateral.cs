#region License

//
// LDQuadrilateral.cs
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
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [DefaultIcon(typeof(Resources), "QuadrilateralIcon")]
    [TypeName(typeof(Resources), "Quadrilateral")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDQuadrilateral : Graphic, IQuadrilateral
    {
        #region Inner types

        private enum BowtieSwap
        {
            None,
            FirstTwo,
            MiddleTwo
        };

        // Rule:   Vertices may not be co-linear
        // Type:   Error
        // Source: http://www.ldraw.org/article/218.html#lt4, http://www.ldraw.org/article/512.html#colinear
        private class ColinearProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_VerticesColinear; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_ColinearQuad; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public ColinearProblem(LDQuadrilateral element)
            {
                Element = element;

                if (null != element.Parent)
                {
                    // TODO: if the colinear quad is a triangle then the Fix is to replace it with a new LDTriangle
                    Fixes = new IFixDescriptor[] { new DeleteQuad(element, Fix_VerticesColinear_DeleteQuadrilateral) };
                }
            }
        }

        // Rule:   Quads may not be concave
        // Type:   Error
        // Source: http://www.ldraw.org/article/218.html#lt4
        private class ConcaveProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_Concave; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_Concave; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public ConcaveProblem(LDQuadrilateral element)
            {
                Element = element;

                // TODO: Fix is to split into two triangles
            }
        }

        // Rule:   Quads must be wound correctly
        // Type:   Error
        // Source: http://www.ldraw.org/article/218.html#lt4
        private class BowtieProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_Bowtie; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_Bowtie; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public BowtieProblem(LDQuadrilateral element)
            {
                Element = element;
                Fixes   = new IFixDescriptor[] { new Fix(element) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_Bowtie; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_FixedBowtie; } }
                public bool IsIntraElement { get { return true; } }

                private LDQuadrilateral _element;

                public Fix(LDQuadrilateral element)
                {
                    _element = element;
                }

                public bool Apply()
                {
                    _element.RepairBowtie();
                    return true;
                }
            }
        }

        // Rule:   Quads may not be non-planar
        // Type:   Error (warp is greater than 3 degrees); Warning (warp is between 1 and 3 degrees); Information (warp is less than 1 degree)
        // Source: http://www.ldraw.org/article/512.html#coplanar
        private class WarpProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_Warped; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public WarpProblem(LDQuadrilateral element, double warp)
            {
                Element = element;

                // convert radians to degrees
                warp        = Math.Round(180.0 / Math.PI * warp, 2);
                Description = String.Format(Resources.Analytics_Warp, warp);

                if (warp > 3.0)
                    Severity = Severity.Error;
                else if (warp >= 1.0)
                    Severity = Severity.Warning;
                else
                    Severity = Severity.Information;

                // TODO: Fix is to split into two triangles and a condline; there are two ways to do this, so two fixes are provided
            }
        }

        private class DeleteQuad : IFixDescriptor
        {
            public Guid Guid { get; private set; }
            public string Instruction { get { return Resources.Analytics_DeleteElement; } }
            public string Action { get { return Resources.Analytics_ElementDeleted; } }
            public bool IsIntraElement { get { return false; } }

            private LDQuadrilateral _quad;

            public DeleteQuad(LDQuadrilateral quad, Guid guid)
            {
                _quad = quad;
                Guid  = guid;
            }

            public bool Apply()
            {
                if (null != _quad.Parent)
                    return _quad.Parent.Remove(_quad);

                return false;
            }
        }

        private class ChangeColour : IFixDescriptor
        {
            public Guid Guid { get { return Fix_ColourInvalid_SetToMainColour; } }
            public string Instruction { get; private set; }
            public string Action { get { return Resources.Analytics_FixedInvalidColour; } }
            public bool IsIntraElement { get { return true; } }

            private LDQuadrilateral _quad;

            public ChangeColour(LDQuadrilateral quad)
            {
                _quad       = quad;
                Instruction = String.Format(Resources.Analytics_FixInvalidColour, Palette.SystemPalette[Palette.MainColour].Name);
            }

            public bool Apply()
            {
                _quad.ColourValue = Palette.MainColour;
                return true;
            }
        }

        #endregion Inner types

        #region Analytics

        private BowtieSwap _bowtieSwap = BowtieSwap.None;
        private bool       _validated  = false;

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     the <see cref="IsBowtie"/> condition.
        /// </summary>
        public static readonly Guid Problem_Bowtie = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     the <see cref="IsWarped"/> condition.
        /// </summary>
        public static readonly Guid Problem_Warped = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     the <see cref="IsConcave"/> condition.
        /// </summary>
        public static readonly Guid Problem_Concave = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     the <see cref="IsColinear"/> condition.
        /// </summary>
        public static readonly Guid Problem_VerticesColinear = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsBowtie"/> condition.
        /// </summary>
        public static readonly Guid Fix_Bowtie = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsColinear"/> condition.
        /// </summary>
        public static readonly Guid Fix_VerticesColinear_DeleteQuadrilateral = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Fix_CoordinatesColocated_DeleteQuadrilateral = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColourInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_ColourInvalid_SetToMainColour = Guid.NewGuid();

        /// <inheritdoc />
        public override bool IsColourInvalid { get { return (Palette.EdgeColour == ColourValue || base.IsColourInvalid); } }

        /// <summary>
        /// Gets a value indicating whether the <see cref="LDQuadrilateral"/> is warped.
        /// </summary>
        public bool IsWarped { get { Validate(); return _isWarped; } private set { _isWarped = value; } }
        private bool _isWarped;

        /// <summary>
        /// Gets the degree of warp, in radians.
        /// </summary>
        public double Warp { get { Validate(); return _warp; } private set { _warp = value; } }
        private double _warp;

        /// <inheritdoc />
        public bool IsBowtie
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                Validate();
                return _isBowtie;
            }
            private set
            {
                _isBowtie = value;
            }
        }
        private bool _isBowtie;

        /// <summary>
        /// Gets a value indicating whether the <see cref="LDQuadrilateral"/> has colinear vertices.
        /// </summary>
        public bool IsColinear { get { Validate(); return _isColinear; } private set { _isColinear = value; } }
        private bool _isColinear;

        /// <summary>
        /// Gets a value indicating whether the <see cref="LDQuadrilateral"/> is concave.
        /// </summary>
        public bool IsConcave { get { Validate(); return _isConcave; } private set { _isConcave = value; } }
        private bool _isConcave;

        /// <summary>
        /// Gets the indices of any <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.Coordinates"/> of the <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/>
        ///     which are <see cref="IsColinear">colinear</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned values are indices into <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.Coordinates"/>, if converted to an array.
        /// </para>
        /// </remarks>
        public uint[] ColinearVertices { get { Validate(); return _colinearVertices.Clone() as uint[]; } private set { _colinearVertices = value; } }
        private uint[] _colinearVertices = new uint[0];

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            return base.HasProblems(mode) || IsBowtie || IsColinear || IsConcave || IsWarped;
        }

        /// <inheritdoc />
        public override bool IsDuplicateOf(IGraphic graphic)
        {
            IQuadrilateral dupe = graphic as IQuadrilateral;

            if (null == dupe)
                return false;

            return base.IsDuplicateOf(graphic);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/> of the <see cref="T:Digitalis.LDTools.DOM.Analytics.IProblemDescriptor"/>s returned varies by
        /// <paramref name="mode"/>:
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="P:Digitalis.LDTools.DOM.Graphic.Problem_CoordinatesColocated"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_VerticesColinear"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_Bowtie"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_Warped"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if the degree of warp is greater than 3°;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> if the degree of warp is greater than or equal to 1°
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Information"/> otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_Concave"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="P:Digitalis.LDTools.DOM.Graphic.Problem_ColourInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            Validate();

            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            // Rule:   Vertices should have different values
            // Type:   Error
            // Source: http://www.ldraw.org/article/512.html#colinear
            // these two are mutually exclusive - colocation always looks like colinearity - but we want to distingush between them
            if (IsColocated)
            {
                // TODO: if the quad has degenerated to a triangle rather than a line or a point then the Fix is to replace it with an LDTriangle
                problems.Add(new ColocationProblem(this,
                                                   Graphic.Problem_CoordinatesColocated,
                                                   Severity.Error,
                                                   Resources.Analytics_Colocation,
                                                   (null == Parent) ? null : new IFixDescriptor[] { new DeleteQuad(this, Fix_CoordinatesColocated_DeleteQuadrilateral) }));
            }
            else if (IsColinear)
            {
                problems.Add(new ColinearProblem(this));
            }

            if (IsBowtie)
                problems.Add(new BowtieProblem(this));

            if (IsConcave)
                problems.Add(new ConcaveProblem(this));

            if (IsWarped)
                problems.Add(new WarpProblem(this, Warp));

            // Rule:   Quads should not normally use EdgeColour
            // Type:   Warning if mode is 'Full', Error otherwise
            // Source: http://www.ldraw.org/article/218.html#lt4, http://www.ldraw.org/article/512.html#colours
            if (Palette.EdgeColour == ColourValue)
            {
                problems.Add(new InvalidColourProblem(this,
                                                      Graphic.Problem_ColourInvalid,
                                                      (CodeStandards.Full == mode) ? Severity.Warning : Severity.Error,
                                                      String.Format(Resources.Analytics_InvalidColour_Polygon, LDTranslationCatalog.GetColourName(Palette.SystemPalette[Palette.EdgeColour].Name)),
                                                      new IFixDescriptor[] { new ChangeColour(this) }));
            }

            return problems;
        }

        private void Validate()
        {
            if (_validated)
                return;

            _validated       = true;
            ColinearVertices = new uint[0];
            IsWarped         = false;
            IsBowtie         = false;
            IsConcave        = false;
            IsConcave        = false;

            Vector3d[] vertices = CoordinatesArray;

            IsBowtie = NeedsSwap(vertices, 0, 1, 2, 3);

            if (IsBowtie)
            {
                if (!NeedsSwap(vertices, 0, 1, 3, 2))
                {
                    _bowtieSwap = BowtieSwap.FirstTwo;
                    Normal = -Normal;
                }
                else
                {
                    _bowtieSwap = BowtieSwap.MiddleTwo;
                }
            }

            if (!IsColinear)
            {
                Vector3d n = Normal;
                n.Normalize();
                Normal = n;
            }
        }

        private bool NeedsSwap(Vector3d[] vertices, int A, int B, int C, int D)
        {
            Vector3d t1, t2, n1, n2, n3, n4;

            t1 = vertices[A] - vertices[D];
            t2 = vertices[A] - vertices[B];
            Vector3d.Cross(ref t1, ref t2, out n1);
            double l1 = n1.Length;

            if (0.0 == l1)
            {
                IsColinear       = true;
                ColinearVertices = new uint[] { (uint)A, (uint)B, (uint)D };
                return false;
            }

            n1 /= l1;

            t1 = vertices[B] - vertices[A];
            t2 = vertices[B] - vertices[C];
            Vector3d.Cross(ref t1, ref t2, out n2);
            double l2 = n2.Length;

            if (0.0 == l2)
            {
                IsColinear       = true;
                ColinearVertices = new uint[] { (uint)A, (uint)B, (uint)C };
                return false;
            }

            n2 /= l2;

            t1 = vertices[C] - vertices[B];
            t2 = vertices[C] - vertices[D];
            Vector3d.Cross(ref t1, ref t2, out n3);
            double l3 = n3.Length;

            if (0.0 == l3)
            {
                IsColinear       = true;
                ColinearVertices = new uint[] { (uint)B, (uint)C, (uint)D };
                return false;
            }

            n3 /= l3;

            t1 = vertices[D] - vertices[C];
            t2 = vertices[D] - vertices[A];
            Vector3d.Cross(ref t1, ref t2, out n4);
            double l4 = n4.Length;

            if (0.0 == l4)
            {
                IsColinear       = true;
                ColinearVertices = new uint[] { (uint)A, (uint)C, (uint)D };
                return false;
            }

            n4 /= l4;

            IsColinear = false;

            Vector3d tmp;
            Vector3d.Cross(ref n1, ref n3, out tmp);
            double angle1 = Math.Asin(tmp.Length);

            Vector3d.Cross(ref n2, ref n4, out tmp);
            double angle2 = Math.Asin(tmp.Length);

            Warp = Math.Round(Math.Max(angle1, angle2), 2);
            IsWarped = (0.0 != Warp);

            // this is safe for bowtied quads
            Normal = -n3;

            // sides
            double s1 = Vector3d.Dot(n1, n2);
            double s2 = Vector3d.Dot(n2, n3);
            double s3 = Vector3d.Dot(n3, n4);
            double s4 = Vector3d.Dot(n4, n1);

            // diagonals
            double d1 = Vector3d.Dot(n1, n3);
            double d2 = Vector3d.Dot(n2, n4);

            IsConcave = ((d1 < 0.0) ^ (d2 < 0.0));

            return (((s1 <= 0.0) || (s2 <= 0.0) || (s3 <= 0.0) || (s4 <= 0.0)) && !IsConcave);
        }

        #endregion Analytics

        #region Colour

        /// <inheritdoc />
        public override uint OverrideableColourValue { get { return Palette.MainColour; } }

        /// <inheritdoc />
        public override bool ColourValueEnabled { get { return true; } }

        #endregion Colour

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// If <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> and the <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/>
        /// is either <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsVisible">hidden</see> or <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsGhosted">ghosted</see>, no
        /// code is appended.
        /// </para>
        /// </remarks>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            sb = base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            if (CodeStandards.PartsLibrary == codeFormat && (!IsVisible || IsGhosted))
                return sb;

            // use ColourValue if it's set to a fixed colour
            if (Palette.MainColour != ColourValue)
                overrideColour = ColourValue;
            else if (Palette.EdgeColour == overrideColour)
                overrideColour = Palette.MainColour;

            // in 'PartsLibrary' mode we need to convert local-palette colours to Direct Colours
            if (CodeStandards.PartsLibrary == codeFormat && !LDColour.IsDirectColour(overrideColour))
            {
                IColour c = GetColour(overrideColour);

                // only local-palette colours will have a Parent
                if (null != c.Parent)
                    overrideColour = LDColour.ConvertRGBToDirectColour(c.Value);
            }

            Vector3d[] verticesIn = new Vector3d[CoordinatesCount];

            if (WindingDirection.Reversed == winding)
            {
                verticesIn[0] = Vertex4;
                verticesIn[1] = Vertex3;
                verticesIn[2] = Vertex2;
                verticesIn[3] = Vertex1;
            }
            else
            {
                verticesIn[0] = Vertex1;
                verticesIn[1] = Vertex2;
                verticesIn[2] = Vertex3;
                verticesIn[3] = Vertex4;
            }

            Vector3d[] verticesOut = new Vector3d[CoordinatesCount];

            for (int n = 0; n < verticesIn.Length; n++)
            {
                Vector3d.TransformVector(ref verticesIn[n], ref transform, out verticesOut[n]);
            }

            IPage page = Page;
            uint  ndp;

            if (null != page && (PageType.Primitive == page.PageType || PageType.HiresPrimitive == page.PageType))
                ndp = Configuration.DecimalPlacesPrimitives;
            else
                ndp = Configuration.DecimalPlacesCoordinates;

            string fmt = Configuration.Formatters[ndp];

            sb.AppendFormat("4 {0}", LDColour.ColourValueToCode(overrideColour));

            foreach (Vector3d v in verticesOut)
            {
                sb.AppendFormat(" {0} {1} {2}",
                                v.X.ToString(fmt, CultureInfo.InvariantCulture),
                                v.Y.ToString(fmt, CultureInfo.InvariantCulture),
                                v.Z.ToString(fmt, CultureInfo.InvariantCulture));
            }

            sb.Append(LineTerminator);
            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDQuadrilateral"/> class with default values.
        /// </summary>
        /// <remarks>
        /// The quadrilateral's <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> is set to <see cref="F:Digitalis.LDTools.DOM.Palette.MainColour"/> and its
        /// vertices are set to <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        public LDQuadrilateral()
            : this(Palette.MainColour, new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, Vector3d.Zero })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDQuadrilateral"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex1">first vertex</see> of the quadrilateral.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex2">second vertex</see> of the quadrilateral.</param>
        /// <param name="vertex3">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex3">third vertex</see> of the quadrilateral.</param>
        /// <param name="vertex4">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex4">fourth vertex</see> of the quadrilateral.</param>
        public LDQuadrilateral(uint colour, Vector3d vertex1, Vector3d vertex2, Vector3d vertex3, Vector3d vertex4)
            : this(colour, new Vector3d[] { vertex1, vertex2, vertex3, vertex4 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDQuadrilateral"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex1">first vertex</see> of the quadrilateral.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex2">second vertex</see> of the quadrilateral.</param>
        /// <param name="vertex3">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex3">third vertex</see> of the quadrilateral.</param>
        /// <param name="vertex4">The <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex4">fourth vertex</see> of the quadrilateral.</param>
        public LDQuadrilateral(uint colour, ref Vector3d vertex1, ref Vector3d vertex2, ref Vector3d vertex3, ref Vector3d vertex4)
            : this(colour, new Vector3d[] { vertex1, vertex2, vertex3, vertex4 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDQuadrilateral"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertices">The vertices of the quadrilateral.</param>
        public LDQuadrilateral(uint colour, IEnumerable<Vector3d> vertices)
            : base(colour, vertices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDQuadrilateral"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this triangle.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw quadrilateral code.</exception>
        /// <example>
        /// <code>
        /// LDQuadrilateral quad = new LDQuadrilateral("4 1 -5 0 0 5 0 0 5 5 0 -5 5 0");
        /// </code>
        /// </example>
        public LDQuadrilateral(string code)
            : base()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 14)
                throw new FormatException("LDraw quadrilateral code must have 14 fields");

            if ("4" != fields[0])
                throw new FormatException("LDraw quadrilateral code must start with '4'");

            SetColourValue(fields[1]);

            Vector3d[] vertices = new Vector3d[CoordinatesCount];

            for (int n = 0, i = 2; n < vertices.Length; n++, i += 3)
            {
                vertices[n] = new Vector3d(double.Parse(fields[i], CultureInfo.InvariantCulture),
                                           double.Parse(fields[i + 1], CultureInfo.InvariantCulture),
                                           double.Parse(fields[i + 2], CultureInfo.InvariantCulture));
            }

            Coordinates = vertices;
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDQuadrilateralEditor", typeof(LDQuadrilateral));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDQuadrilateral"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return null != EditorFactory;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDQuadrilateral"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != EditorFactory)
                return EditorFactory.Invoke(new object[] { this }) as IElementEditor;

            return null;
        }

        #endregion Editor

        #region Geometry

        /// <inheritdoc />
        public void RepairBowtie()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsBowtie)
            {
                switch (_bowtieSwap)
                {
                    case BowtieSwap.FirstTwo:
                        Coordinates = new Vector3d[] { Vertex2, Vertex1, Vertex3, Vertex4 };
                        break;

                    case BowtieSwap.MiddleTwo:
                        Coordinates = new Vector3d[] { Vertex1, Vertex3, Vertex2, Vertex4 };
                        break;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The origin of an <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/> is <see cref="P:Digitalis.LDTools.DOM.API.IQuadrilateral.Vertex1"/>.
        /// </para>
        /// </remarks>
        public override Vector3d Origin { get { return Vertex1; } }

        /// <inheritdoc />
        public override uint CoordinatesCount { get { return 4; } }

        /// <inheritdoc />
        public override void ReverseWinding()
        {
            Coordinates = new Vector3d[] { Vertex4, Vertex3, Vertex2, Vertex1 };
        }

        /// <inheritdoc />
        protected override void OnCoordinatesChanged(PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
        {
            _validated = false;
            base.OnCoordinatesChanged(e);
        }

        #endregion Geometry

        #region Properties

        /// <inheritdoc />
        public Vector3d Vertex1 { get { return CoordinatesArray[0]; } set { Coordinates = new Vector3d[] { value, Vertex2, Vertex3, Vertex4 }; } }

        /// <inheritdoc />
        public Vector3d Vertex2 { get { return CoordinatesArray[1]; } set { Coordinates = new Vector3d[] { Vertex1, value, Vertex3, Vertex4 }; } }

        /// <inheritdoc />
        public Vector3d Vertex3 { get { return CoordinatesArray[2]; } set { Coordinates = new Vector3d[] { Vertex1, Vertex2, value, Vertex4 }; } }

        /// <inheritdoc />
        public Vector3d Vertex4 { get { return CoordinatesArray[3]; } set { Coordinates = new Vector3d[] { Vertex1, Vertex2, Vertex3, value }; } }

        /// <inheritdoc />
        public Vector3d Normal
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                Validate();
                return _normal;
            }
            private set
            {
                _normal = value;
            }
        }
        private Vector3d _normal = new Vector3d();

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Quadrilateral; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.QuadrilateralIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Quadrilateral; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string Description { get { return String.Empty; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IQuadrilateral"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
