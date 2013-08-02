#region License

//
// IOptionalLine.cs
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

namespace Digitalis.LDTools.DOM.API
{
    #region Usings

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw optional-line as defined in
    ///     <see href="http://www.ldraw.org/article/218.html#lt5">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Analytics</h3>
    /// <see cref="IGraphic.IsDuplicateOf">IsDuplicateOf()</see> will return <c>true</c> if <i>graphic</i> is an instance of
    /// <b>IOptionalLine</b> and has the same values for <see cref="IGraphic.ColourValue"/> and
    /// <see cref="IGraphic.Coordinates"/>. The ordering of <see cref="Vertex1"/> / <see cref="Vertex2"/> and of
    /// <see cref="Control1"/> / <see cref="Control2"/> is not important.
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> will generate a single LDraw <i>optional-line</i> statement whose values
    /// are determined by <see cref="IGraphic.ColourValue"/>, <see cref="Vertex1"/>, <see cref="Vertex2"/>,
    /// <see cref="Control1"/> and <see cref="Control2"/>. If <see cref="IPageElement.IsLocalLock"/> is <c>true</c> the
    /// statement will be prefixed with the <i>!DIGITALIS_LDTOOLS_DOM LOCKNEXT</i> meta-command.
    /// <p/>
    /// If <see cref="IGraphic.IsVisible"/> is <c>false</c> or <see cref="IGraphic.IsGhosted"/> is <c>true</c> and
    /// <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> then no code will be generated; otherwise the
    /// <i>optional-line</i> statement will be prefixed with the <i>MLCAD HIDE</i> and <i>GHOST</i> meta-commands as required.
    /// <p/>
    /// If <see cref="IGraphic.ColourValue"/> is not equal to <see cref="IGraphic.OverrideableColourValue"/> then the generated
    /// code will output <see cref="IGraphic.ColourValue"/>; otherwise it will output according to the following rules:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Value of <i>overrideColour</i></term><description>Output value</description></listheader>
    ///     <item>
    ///         <term><see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/></term>
    ///         <description><see cref="IGraphic.OverrideableColourValue"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="Digitalis.LDTools.DOM.Palette.MainColour"/></term>
    ///         <description><see cref="Digitalis.LDTools.DOM.Palette.MainColour"/></description>
    ///     </item>
    ///     <item>
    ///         <term>A <i>Direct Colours</i> value</term>
    ///         <description>
    ///             The specified value in upper-case hexadecimal format, prefixed with an <c>#</c>
    ///             <p/>
    ///             For example, <i>#2FF0000</i>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term>
    ///             The <see cref="IColour.Code"/> of an entry in the <see cref="Digitalis.LDTools.DOM.Palette.SystemPalette"/>
    ///         </term>
    ///         <description>The specified value in base-10 format</description>
    ///     </item>
    ///     <item>
    ///         <term>The <see cref="IColour.Code"/> of an <see cref="IColour"/> in the containing <see cref="IPage"/></term>
    ///         <description>
    ///             If <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/>, the <see cref="IColour"/>'s
    ///             <see cref="IColour.Value"/> converted to <i>Direct Colours</i> format; otherwise the specified value in
    ///             base-10 format
    ///         </description>
    ///     </item>
    /// </list>
    ///
    /// <h3>Geometry</h3>
    /// The <see cref="IGeometric.Origin"/> of an <b>IOptionalLine</b> is <see cref="Vertex1"/>.
    /// <p/>
    /// <see cref="IGeometric.ReverseWinding">ReverseWinding()</see> swaps the values of <see cref="Vertex1"/> and
    /// <see cref="Vertex2"/>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IOptionalLine</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.OptionalLine"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IGraphic.CoordinatesCount"/></term><description>4</description></item>
    ///     <item><term><see cref="IGraphic.OverrideableColourValue"/></term><description><see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/></description></item>
    ///     <item><term><see cref="IGraphic.ColourValueEnabled"/></term><description><c>true</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IOptionalLine : IGraphic
    {
        #region Properties

        /// <summary>
        /// Gets or sets the first vertex of the <see cref="IOptionalLine"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the first member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// </remarks>
        Vector3d Vertex1 { get; set; }

        /// <summary>
        /// Gets or sets the second vertex of the <see cref="IOptionalLine"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the second member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// </remarks>
        Vector3d Vertex2 { get; set; }

        /// <summary>
        /// Gets or sets the first control-point of the <see cref="IOptionalLine"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the third member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// </remarks>
        Vector3d Control1 { get; set; }

        /// <summary>
        /// Gets or sets the second control-point of the <see cref="IOptionalLine"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IOptionalLine"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the fouth member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// </remarks>
        Vector3d Control2 { get; set; }

        #endregion Properties
    }
}
