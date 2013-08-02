#region License

//
// LDOptionalLine.cs
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

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [DefaultIcon(typeof(Resources), "OptionalLineIcon")]
    [TypeName(typeof(Resources), "OptionalLine")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDOptionalLine : Graphic, IOptionalLine
    {
        #region Inner types

        private class DeleteOptionalLine : IFixDescriptor
        {
            public Guid Guid { get { return Fix_CoordinatesColocated_DeleteLine; } }
            public string Instruction { get { return Resources.Analytics_DeleteElement; } }
            public string Action { get { return Resources.Analytics_ElementDeleted; } }
            public bool IsIntraElement { get { return false; } }
            private LDOptionalLine _line;

            public DeleteOptionalLine(LDOptionalLine line)
            {
                _line = line;
            }

            public bool Apply()
            {
                if (null != _line.Parent)
                    return _line.Parent.Remove(_line);

                return false;
            }
        }

        private class ChangeColour : IFixDescriptor
        {
            public Guid Guid { get { return Fix_ColourInvalid_SetToEdgeColour; } }
            public string Instruction { get; private set; }
            public string Action { get { return Resources.Analytics_FixedInvalidColour; } }
            public bool IsIntraElement { get { return true; } }

            private LDOptionalLine _line;

            public ChangeColour(LDOptionalLine line)
            {
                _line       = line;
                Instruction = String.Format(Resources.Analytics_FixInvalidColour, Palette.SystemPalette[Palette.EdgeColour].Name);
            }

            public bool Apply()
            {
                _line.ColourValue = Palette.EdgeColour;
                return true;
            }
        }

        #endregion Inner types

        #region Analytics

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Problem_ControlPointsColocated = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Problem_VerticesAndControlPointsColocated = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe a fix
        ///     for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Fix_CoordinatesColocated_DeleteLine = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe a fix
        ///     for the <see cref="P:Digitalis.LDTools.DOM.Graphic.IsColourInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_ColourInvalid_SetToEdgeColour = Guid.NewGuid();

        /// <inheritdoc />
        public override bool IsColourInvalid { get { return (Palette.MainColour == ColourValue || base.IsColourInvalid); } }

        /// <inheritdoc />
        public override bool IsDuplicateOf(IGraphic graphic)
        {
            if (graphic == this)
                return false;

            IOptionalLine dupe = graphic as IOptionalLine;

            if (null == dupe)
                return false;

            // only the vertices are checked - identical control-points are allowed
            return ((Vertex1 == dupe.Vertex1 && Vertex2 == dupe.Vertex2) || (Vertex1 == dupe.Vertex2 && Vertex2 == dupe.Vertex1)) && ColourValue == graphic.ColourValue;
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
        ///     <term><see cref="P:Digitalis.LDTools.DOM.API.Graphic.Problem_ColourInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Information"/> otherwise
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            // Rule:   Vertices should have different values
            // Type:   Error
            // Source: None
            if (Vertex1 == Vertex2)
            {
                problems.Add(new ColocationProblem(this,
                                                   Graphic.Problem_CoordinatesColocated,
                                                   Severity.Error,
                                                   Resources.Analytics_Colocation_Line,
                                                   (null == Parent) ? null : new IFixDescriptor[] { new DeleteOptionalLine(this) }));
            }

            // Rule:   Control-points should have different values
            // Type:   Error
            // Source: None
            // TODO: could we try and locate the 'correct' values for the points?
            if (Control1 == Control2)
            {
                problems.Add(new ColocationProblem(this,
                                                   Problem_ControlPointsColocated,
                                                   Severity.Error,
                                                   Resources.Analytics_Colocation_Control,
                                                   null));
            }

            // Rule:   Vertices and control-points should have different values
            // Type:   Error
            // Source: None
            // TODO: could we try and locate the 'correct' values for the control-points?
            if (Vertex1 == Control1 || Vertex1 == Control2 || Vertex2 == Control1 || Vertex2 == Control2)
            {
                problems.Add(new ColocationProblem(this,
                                                   Problem_VerticesAndControlPointsColocated,
                                                   Severity.Error,
                                                   Resources.Analytics_Colocation_Mixed,
                                                   null));
            }

            // Rule:   Lines should not normally use MainColour
            // Type:   Informational if mode is 'Full', Warning otherwise
            // Source: http://www.ldraw.org/article/512.html#colours
            if (Palette.MainColour == ColourValue)
            {
                problems.Add(new InvalidColourProblem(this,
                                                      Graphic.Problem_ColourInvalid,
                                                      (CodeStandards.Full == mode) ? Severity.Information : Severity.Warning,
                                                      String.Format(Resources.Analytics_InvalidColour_Line, LDTranslationCatalog.GetColourName(Palette.SystemPalette[Palette.MainColour].Name)),
                                                      new IFixDescriptor[] { new ChangeColour(this) }));
            }

            return problems;
        }

        #endregion Analytics

        #region Code-generation

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// If <paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> and the <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/>
        /// is either <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsVisible">hidden</see> or <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.IsGhosted">ghosted</see>, no code
        /// is appended.
        /// </para>
        /// </remarks>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            sb = base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            if (CodeStandards.PartsLibrary == codeFormat && (!IsVisible || IsGhosted))
                return sb;

            // use ColourValue if it's set to a fixed colour
            if (Palette.EdgeColour != ColourValue)
                overrideColour = ColourValue;

            // in 'PartsLibrary' mode we need to convert local-palette colours to Direct Colours
            if (CodeStandards.PartsLibrary == codeFormat && !LDColour.IsDirectColour(overrideColour))
            {
                IColour c = GetColour(overrideColour);

                // only local-palette colours will have a Parent
                if (null != c.Parent)
                    overrideColour = LDColour.ConvertRGBToDirectColour(c.Value);
            }

            Vector3d[] vertices = CoordinatesArray;
            Vector3d   vertex1;
            Vector3d   vertex2;
            Vector3d   control1;
            Vector3d   control2;

            if (WindingDirection.Reversed == winding)
            {
                Vector3d.Transform(ref vertices[1], ref transform, out vertex1);
                Vector3d.Transform(ref vertices[0], ref transform, out vertex2);
                Vector3d.Transform(ref vertices[3], ref transform, out control1);
                Vector3d.Transform(ref vertices[2], ref transform, out control2);
            }
            else
            {
                Vector3d.Transform(ref vertices[0], ref transform, out vertex1);
                Vector3d.Transform(ref vertices[1], ref transform, out vertex2);
                Vector3d.Transform(ref vertices[2], ref transform, out control1);
                Vector3d.Transform(ref vertices[3], ref transform, out control2);
            }

            IPage page = Page;
            uint  ndp;

            if (null != page && (PageType.Primitive == page.PageType || PageType.HiresPrimitive == page.PageType))
                ndp = Configuration.DecimalPlacesPrimitives;
            else
                ndp = Configuration.DecimalPlacesCoordinates;

            string fmt = Configuration.Formatters[ndp];

            sb.AppendFormat("5 {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}{13}",
                            LDColour.ColourValueToCode(overrideColour),
                            vertex1.X.ToString(fmt, CultureInfo.InvariantCulture),
                            vertex1.Y.ToString(fmt, CultureInfo.InvariantCulture),
                            vertex1.Z.ToString(fmt, CultureInfo.InvariantCulture),
                            vertex2.X.ToString(fmt, CultureInfo.InvariantCulture),
                            vertex2.Y.ToString(fmt, CultureInfo.InvariantCulture),
                            vertex2.Z.ToString(fmt, CultureInfo.InvariantCulture),
                            control1.X.ToString(fmt, CultureInfo.InvariantCulture),
                            control1.Y.ToString(fmt, CultureInfo.InvariantCulture),
                            control1.Z.ToString(fmt, CultureInfo.InvariantCulture),
                            control2.X.ToString(fmt, CultureInfo.InvariantCulture),
                            control2.Y.ToString(fmt, CultureInfo.InvariantCulture),
                            control2.Z.ToString(fmt, CultureInfo.InvariantCulture),
                            LineTerminator);

            return sb;
        }

        #endregion Code-generation

        #region Colour

        /// <inheritdoc />
        public override uint OverrideableColourValue { get { return Palette.EdgeColour; } }

        /// <inheritdoc />
        public override bool ColourValueEnabled { get { return true; } }

        #endregion Colour

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDOptionalLine"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The line's <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> is set to <see cref="F:Digitalis.LDTools.DOM.Palette.EdgeColour"/> and its vertices
        /// and control-points are set to <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </para>
        /// </remarks>
        public LDOptionalLine()
            : this(Palette.EdgeColour, Vector3d.Zero, Vector3d.Zero, Vector3d.Zero, Vector3d.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDOptionalLine"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex1">first vertex</see> of the line.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex2">second vertex</see> of the line.</param>
        /// <param name="control1">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control1">first control-point</see> of the line.</param>
        /// <param name="control2">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control2">second control-point</see> of the line.</param>
        public LDOptionalLine(uint colour, Vector3d vertex1, Vector3d vertex2, Vector3d control1, Vector3d control2)
            : this(colour, new Vector3d[] { vertex1, vertex2, control1, control2 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDOptionalLine"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertex1">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex1">first vertex</see> of the line.</param>
        /// <param name="vertex2">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex2">second vertex</see> of the line.</param>
        /// <param name="control1">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control1">first control-point</see> of the line.</param>
        /// <param name="control2">The <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control2">second control-point</see> of the line.</param>
        public LDOptionalLine(uint colour, ref Vector3d vertex1, ref Vector3d vertex2, ref Vector3d control1, ref Vector3d control2)
            : this(colour, new Vector3d[] { vertex1, vertex2, control1, control2 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDOptionalLine"/> class with the specified values.
        /// </summary>
        /// <param name="colour">The <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/> to set.</param>
        /// <param name="vertices">The vertices and control-points of the line.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="vertices"/> must be in the order <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex1"/>,
        /// <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex2"/>, <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control1"/>,
        /// <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Control2"/>.
        /// </para>
        /// </remarks>
        public LDOptionalLine(uint colour, IEnumerable<Vector3d> vertices)
            : base(colour, vertices)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDOptionalLine"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this optional-line.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw optional-line code.</exception>
        /// <example>
        /// <code>
        /// LDOptionalLine line = new LDOptionalLine("5 4 0 0 0 10 10 10 0 5 0 0 -5 0");
        /// </code>
        /// </example>
        public LDOptionalLine(string code)
            : base()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 14)
                throw new FormatException("LDraw optional-line code must have 14 fields");

            if ("5" != fields[0])
                throw new FormatException("LDraw optional-line code must start with '5'");

            SetColourValue(fields[1]);

            Coordinates = new Vector3d[] { new Vector3d(double.Parse(fields[2],  CultureInfo.InvariantCulture), double.Parse(fields[3],  CultureInfo.InvariantCulture), double.Parse(fields[4],  CultureInfo.InvariantCulture)),
                                           new Vector3d(double.Parse(fields[5],  CultureInfo.InvariantCulture), double.Parse(fields[6],  CultureInfo.InvariantCulture), double.Parse(fields[7],  CultureInfo.InvariantCulture)),
                                           new Vector3d(double.Parse(fields[8],  CultureInfo.InvariantCulture), double.Parse(fields[9],  CultureInfo.InvariantCulture), double.Parse(fields[10], CultureInfo.InvariantCulture)),
                                           new Vector3d(double.Parse(fields[11], CultureInfo.InvariantCulture), double.Parse(fields[12], CultureInfo.InvariantCulture), double.Parse(fields[13], CultureInfo.InvariantCulture)) };
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDOptionalLineEditor", typeof(LDOptionalLine));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDOptionalLine"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDOptionalLine"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// The origin of an <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/> is <see cref="P:Digitalis.LDTools.DOM.API.IOptionalLine.Vertex1"/>.
        /// </para>
        /// </remarks>
        public override Vector3d Origin { get { return Vertex1; } }

        /// <inheritdoc />
        public override uint CoordinatesCount { get { return 4; } }

        /// <inheritdoc />
        public override void ReverseWinding()
        {
            Coordinates = new Vector3d[] { Vertex2, Vertex1, Control1, Control2 };
        }

        #endregion Geometry

        #region Properties

        /// <inheritdoc />
        public Vector3d Vertex1 { get { return CoordinatesArray[0]; } set { Coordinates = new Vector3d[] { value, Vertex2, Control1, Control2 }; } }

        /// <inheritdoc />
        public Vector3d Vertex2 { get { return CoordinatesArray[1]; } set { Coordinates = new Vector3d[] { Vertex1, value, Control1, Control2 }; } }

        /// <inheritdoc />
        public Vector3d Control1 { get { return CoordinatesArray[2]; } set { Coordinates = new Vector3d[] { Vertex1, Vertex2, value, Control2 }; } }

        /// <inheritdoc />
        public Vector3d Control2 { get { return CoordinatesArray[3]; } set { Coordinates = new Vector3d[] { Vertex1, Vertex2, Control1, value }; } }

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.OptionalLine; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.OptionalLineIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.OptionalLine; } }

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
        /// <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IOptionalLine"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
