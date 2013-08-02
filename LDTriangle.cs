#region License

//
// LDTriangle.cs
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
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [DefaultIcon(typeof(Resources), "TriangleIcon")]
    [TypeName(typeof(Resources), "Triangle")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDTriangle : Graphic, ITriangle
    {
        #region Inner types

        // Rule:   Vertices should not be co-linear
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#colinear
        private class ColinearProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_VerticesColinear; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_ColinearTriangle; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public ColinearProblem(LDTriangle element)
            {
                Element = element;

                if (null != element.Parent)
                    Fixes = new IFixDescriptor[] { new DeleteTriangle(element, Fix_VerticesColinear_DeleteTriangle) };
            }
        }

        private class DeleteTriangle : IFixDescriptor
        {
            public Guid Guid { get; private set; }
            public string Instruction { get { return Resources.Analytics_DeleteElement; } }
            public string Action { get { return Resources.Analytics_ElementDeleted; } }
            public bool IsIntraElement { get { return false; } }

            private LDTriangle _triangle;

            public DeleteTriangle(LDTriangle triangle, Guid guid)
            {
                _triangle = triangle;
                Guid      = guid;
            }

            public bool Apply()
            {
                if (null != _triangle.Parent)
                    return _triangle.Parent.Remove(_triangle);

                return false;
            }
        }

        private class ChangeColour : IFixDescriptor
        {
            public Guid Guid { get { return Fix_ColourInvalid_SetToMainColour; } }
            public string Instruction { get; private set; }
            public string Action { get { return Resources.Analytics_FixedInvalidColour; } }
            public bool IsIntraElement { get { return true; } }

            private LDTriangle _triangle;

            public ChangeColour(LDTriangle triangle)
            {
                _triangle   = triangle;
                Instruction = String.Format(Resources.Analytics_FixInvalidColour, Palette.SystemPalette[Palette.MainColour].Name);
            }

            public bool Apply()
            {
                _triangle.ColourValue = Palette.MainColour;
                return true;
            }
        }

        #endregion Inner types

        #region Analytics

        private bool _validated = false;

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     the <see cref="IsColinear"/> condition.
        /// </summary>
        public static readonly Guid Problem_VerticesColinear = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="IsColinear"/> condition.
        /// </summary>
        public static readonly Guid Fix_VerticesColinear_DeleteTriangle = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Fix_CoordinatesColocated_DeleteTriangle = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe
        ///     a fix for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColourInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_ColourInvalid_SetToMainColour = Guid.NewGuid();

        /// <inheritdoc />
        public override bool IsColourInvalid { get { return (Palette.EdgeColour == ColourValue || base.IsColourInvalid); } }

        /// <inheritdoc />
        public override bool IsDuplicateOf(IGraphic graphic)
        {
            ITriangle dupe = graphic as ITriangle;

            if (null == dupe)
                return false;

            return base.IsDuplicateOf(graphic);
        }

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            return base.HasProblems(mode) || IsColinear;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/> of the <see cref="T:Digitalis.LDTools.DOM.Analytics.IProblemDescriptor"/>s returned varies by
        /// <paramref name="mode"/>:
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="P:Digitalis.LDTools.DOM.API.Graphic.Problem_CoordinatesColocated"/></term>
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
        ///     <term><see cref="P:Digitalis.LDTools.DOM.API.Graphic.Problem_ColourInvalid"/></term>
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
                problems.Add(new ColocationProblem(this,
                                                   Graphic.Problem_CoordinatesColocated,
                                                   Severity.Error,
                                                   Resources.Analytics_Colocation,
                                                   (null == Parent) ? null : new IFixDescriptor[] { new DeleteTriangle(this, Fix_CoordinatesColocated_DeleteTriangle) }));
            }
            else if (IsColinear)
            {
                problems.Add(new ColinearProblem(this));
            }

            // Rule:   Triangles should not normally use EdgeColour
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="LDTriangle"/> has colinear vertices.
        /// </summary>
        public bool IsColinear { get { Validate(); return _isColinear; } private set { _isColinear = value; } }
        private bool _isColinear;

        private void Validate()
        {
            if (_validated)
                return;

            _validated = true;

            // colocation always looks like colinearity so there's no need to check further
            if (IsColocated)
            {
                IsColinear = true;
                return;
            }

            Vector3d[] vertices = CoordinatesArray;
            Vector3d   t1       = vertices[1] - vertices[2];
            Vector3d   t2       = vertices[1] - vertices[0];
            Vector3d   n;

            Vector3d.Cross(ref t1, ref t2, out n);

            IsColinear = (0.0 == n.LengthSquared);

            if (!IsColinear)
            {
                n.Normalize();
                Normal = n;
            }
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
        /// If <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> and the <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/>
        /// is either <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsVisible">hidden</see> or <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsGhosted">ghosted</see>,
        /// no code is appended.
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
                verticesIn[0] = Vertex3;
                verticesIn[1] = Vertex2;
                verticesIn[2] = Vertex1;
            }
            else
            {
                verticesIn[0] = Vertex1;
                verticesIn[1] = Vertex2;
                verticesIn[2] = Vertex3;
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

            sb.AppendFormat("3 {0}", LDColour.ColourValueToCode(overrideColour));

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
        /// Initializes a new instance of the <see cref="LDTriangle"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The triangle's <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> is set to <see cref="F:Digitalis.LDTools.DOM.Palette.MainColour"/> and its
        /// vertices are set to <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </para>
        /// </remarks>
        public LDTriangle()
            : this(Palette.MainColour, new Vector3d[] { Vector3d.Zero, Vector3d.Zero, Vector3d.Zero })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTriangle"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex1">first vertex</see> of the triangle.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex2">second vertex</see> of the triangle.</param>
        /// <param name="vertex3">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex3">third vertex</see> of the triangle.</param>
        public LDTriangle(uint colour, Vector3d vertex1, Vector3d vertex2, Vector3d vertex3)
            : this(colour, new Vector3d[] { vertex1, vertex2, vertex3 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTriangle"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex1">first vertex</see> of the triangle.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex2">second vertex</see> of the triangle.</param>
        /// <param name="vertex3">The <see cref="P:Digitalis.LDTools.DOM.API.Vertex3">third vertex</see> of the triangle.</param>
        public LDTriangle(uint colour, ref Vector3d vertex1, ref Vector3d vertex2, ref Vector3d vertex3)
            : this(colour, new Vector3d[] { vertex1, vertex2, vertex3 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTriangle"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertices">The vertices of the triangle.</param>
        public LDTriangle(uint colour, IEnumerable<Vector3d> vertices)
            : base(colour, vertices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTriangle"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this triangle.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw triangle code.</exception>
        /// <example>
        /// <code>
        /// LDTriangle triangle = new LDTriangle("3 4 -5 0 0 5 0 0 0 5 0");
        /// </code>
        /// </example>
        public LDTriangle(string code)
            : base()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 11)
                throw new FormatException("LDraw triangle code must have 11 fields");

            if ("3" != fields[0])
                throw new FormatException("LDraw triangle code must start with '3'");

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
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDTriangleEditor", typeof(LDTriangle));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDTriangle"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (null != EditorFactory);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDTriangle"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <remarks>
        /// <para>
        /// The origin of an <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/> is <see cref="P:Digitalis.LDTools.DOM.API.ITriangle.Vertex1"/>.
        /// </para>
        /// </remarks>
        public override Vector3d Origin { get { return Vertex1; } }

        /// <inheritdoc />
        public override uint CoordinatesCount { get { return 3; } }

        /// <inheritdoc />
        public override void ReverseWinding()
        {
            Coordinates = new Vector3d[] { Vertex3, Vertex2, Vertex1 };
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
        public Vector3d Vertex1 { get { return CoordinatesArray[0]; } set { Coordinates = new Vector3d[] { value, Vertex2, Vertex3 }; } }

        /// <inheritdoc />
        public Vector3d Vertex2 { get { return CoordinatesArray[1]; } set { Coordinates = new Vector3d[] { Vertex1, value, Vertex3 }; } }

        /// <inheritdoc />
        public Vector3d Vertex3 { get { return CoordinatesArray[2]; } set { Coordinates = new Vector3d[] { Vertex1, Vertex2, value }; } }

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
        public override DOMObjectType ObjectType { get { return DOMObjectType.Triangle; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.TriangleIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Triangle; } }

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
        /// <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.ITriangle"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
