#region License

//
// IReference.cs
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

    using System;

    using OpenTK;

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw sub-file reference as defined in
    ///     <see href="http://www.ldraw.org/article/218.html#lt1">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// Initially, an <b>IReference</b> just holds the <see cref="TargetName"/> of the sub-file to which it refers. This may
    /// eventually be resolved to an actual <see cref="IPage"/>, either in the same <see cref="IDocument"/> as the
    /// <b>IReference</b> or in a separate file entirely.
    ///
    /// <h3>Analytics</h3>
    /// <see cref="IGraphic.IsDuplicateOf">IsDuplicateOf()</see> will return <c>true</c> if <i>graphic</i> is an instance of
    /// <b>IReference</b> and has the same values for <see cref="IGraphic.ColourValue"/>,  <see cref="Matrix"/> and
    /// <see cref="TargetName"/>.
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> will generate a single LDraw <i>sub-file reference</i> statement whose
    /// values are determined by <see cref="IGraphic.ColourValue"/>, <see cref="Matrix"/> and <see cref="TargetName"/>. If
    /// <see cref="Invert"/> is <c>true</c> the statement will be prefixed with the <i>BFC INVERTNEXT</i> meta-command, and if
    /// <see cref="IPageElement.IsLocalLock"/> is <c>true</c> the entire statement including <i>BFC INVERTNEXT</i> if present
    /// will be prefixed with the <i>!DIGITALIS_LDTOOLS_DOM LOCKNEXT</i> meta-command.
    /// <p/>
    /// The <i>winding</i> parameter is not used by <b>IReference</b>s and will have no effect.
    /// <p/>
    /// If <see cref="IGraphic.IsVisible"/> is <c>false</c> or <see cref="IGraphic.IsGhosted"/> is <c>true</c> and
    /// <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> then no code will be generated; otherwise the
    /// <i>quadrilateral</i> statement will be prefixed with the <i>MLCAD HIDE</i> and <i>GHOST</i> meta-commands as required.
    /// <p/>
    /// If <see cref="IGraphic.ColourValue"/> is not equal to <see cref="IGraphic.OverrideableColourValue"/> then the generated
    /// code will output <see cref="IGraphic.ColourValue"/>; otherwise it will output according to the following rules:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Value of <i>overrideColour</i></term><description>Output value</description></listheader>
    ///     <item>
    ///         <term><see cref="Digitalis.LDTools.DOM.Palette.MainColour"/></term>
    ///         <description><see cref="IGraphic.OverrideableColourValue"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/></term>
    ///         <description><see cref="IGraphic.OverrideableColourValue"/></description>
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
    /// <p/>
    /// If <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> and the <see cref="Target"/>'s
    /// <see cref="IPage.InlineOnPublish"/> property is <c>true</c> then instead of the <i>sub-file reference</i> statement, the
    /// members of <see cref="Target"/> will be output as LDraw code. <see cref="IDOMObject.ToCode">ToCode()</see> is called on
    /// each member in turn with the following parameters:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><i>overrideColour</i></term>
    ///         <term>
    ///             If the <b>IReference</b>'s <see cref="IGraphic.ColourValue"/> is not equal to its
    ///             <see cref="IGraphic.OverrideableColourValue"/> then it is passed; otherwise the original
    ///             <i>overrideColour</i> value is passed
    ///         </term>
    ///     </item>
    ///     <item>
    ///         <term><i>transform</i></term>
    ///         <term>The original transform passed to the <b>IReference</b> multiplied by <see cref="Matrix"/></term>
    ///     </item>
    ///     <item>
    ///         <term><i>winding</i></term>
    ///         <term>
    ///             If the original value passed to the <b>IReference</b> is <see cref="WindingDirection.None"/> then this is
    ///             passed down; otherwise <see cref="WindingDirection.Normal"/> or <see cref="WindingDirection.Reversed"/> is
    ///             passed as required to conform the <see cref="Target"/>'s winding-direction to that of the <b>IReference</b>
    ///         </term>
    ///     </item>
    /// </list>
    /// <p/>
    /// If the <b>IReference</b> is BFC-enabled and the <see cref="Target"/> is not, then <i>BFC NOCLIP</i> / <i>BFC CLIP</i>
    /// meta-commands will be placed around the inlined code. If the <see cref="Target"/> is BFC-enabled and the
    /// <b>IReference</b> is not, the <see cref="Target"/>'s <i>BFC</i> meta-commands will be omitted.
    ///
    /// <h3>Geometry</h3>
    /// The <see cref="IGeometric.BoundingBox"/> of an <b>IReference</b> is the bounding-box of its <see cref="Target"/>, which
    /// will be resolved if necessary. If <see cref="Target"/> cannot be resolved then an empty
    /// <see cref="Digitalis.LDTools.DOM.Geom.Box3d"/> is returned.
    ///
    /// The <see cref="IGeometric.Origin"/> of an <b>IReference</b> is the <c>(x, y, z)</c> component of <see cref="Matrix"/>.
    /// <p/>
    /// <see cref="IGeometric.ReverseWinding">ReverseWinding()</see> toggles the value of <see cref="Invert"/>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IReference</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Reference"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IGraphic.CoordinatesCount"/></term><description>0</description></item>
    ///     <item><term><see cref="IGraphic.OverrideableColourValue"/></term><description><see cref="Digitalis.LDTools.DOM.Palette.MainColour"/></description></item>
    ///     <item><term><see cref="IGraphic.ColourValueEnabled"/></term><description><c>true</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IReference : IGraphic
    {
        #region Properties

        /// <summary>
        /// Gets or sets the matrix of the <see cref="IReference"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the fourth column of the supplied value is
        ///     not <see cref="OpenTK.Vector4d.UnitW"/>.</exception>
        /// <remarks>
        /// The matrix is defined in the following order, with reference to
        /// <see href="http://www.ldraw.org/article/218.html#lt1">the LDraw specifications</see>:
        /// <code>
        ///     / a d g 0 \
        ///     | b e h 0 |
        ///     | c f i 0 |
        ///     \ x y z 1 /
        /// </code>
        /// <p/>
        /// Raises the <see cref="MatrixChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="OpenTK.Matrix4d.Identity"/>.
        /// </remarks>
        Matrix4d Matrix { get; set; }

        /// <summary>
        /// Occurs when <see cref="Matrix"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<Matrix4d> MatrixChanged;

        /// <summary>
        /// Gets or sets the BFC-invert flag of the <see cref="IReference"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// If the <see cref="Target"/> has been resolved and is 2D then setting this property will instead scale
        /// <see cref="Matrix"/> by -1 in the x-, y- or z-direction as appropriate.
        /// <p/>
        /// Raises the <see cref="InvertChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>false</c>.
        /// </remarks>
        bool Invert { get; set; }

        /// <summary>
        /// Occurs when <see cref="Invert"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<bool> InvertChanged;

        #endregion Properties

        #region Target-management

        /// <summary>
        /// Gets or sets the name of the sub-file referred to by the <see cref="IReference"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IReference"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The supplied string was <c>null</c>, empty or whitespace.</exception>
        /// <remarks>
        /// The referenced sub-file may be one of:
        /// <p/>
        /// <list type="bullet">
        ///     <item><term>A file referred to by an absolute file-path</term></item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> in the <see cref="IDocument"/> the <see cref="IReference"/> is a descendant of
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>An <see cref="IPage"/> in the <see cref="IDocument"/> specified by <see cref="TargetContext"/></term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             A file referred to by a file-path relative to the folder containing the <see cref="IDocument"/> the
        ///             <see cref="IReference"/> is a descendant of
        ///         </term>
        ///     </item>
        ///     <item><term>A file in the LDraw Library</term></item>
        /// </list>
        /// <p/>
        /// When resolving <see cref="Target"/>, the possible locations for the sub-file are searched in the order specified
        /// above.
        /// <p/>
        /// The <see cref="IReference"/> will react to changes in its containing <see cref="IDocument"/>'s structure which would
        /// affect the resolution of <see cref="Target"/>:
        /// <p/>
        /// <list type="table">
        ///     <listheader><term>Event</term><term>Action taken</term></listheader>
        ///     <item>
        ///         <term>
        ///             The <see cref="IReference"/>'s <see cref="IDocumentElement.PathToDocumentChanged"/> event is raised
        ///         </term>
        ///         <term>
        ///             If <see cref="Target"/> is or could be resolved to an <see cref="IPage"/> in the containing
        ///             <see cref="IDocument"/> then:
        ///             <p/>
        ///             <see cref="Target"/> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> whose <see cref="IPage.TargetName"/> matches the <see cref="IReference"/>'s is
        ///             added to or removed from the <see cref="IDocument"/>
        ///         </term>
        ///         <term>
        ///             If <see cref="Target"/> is or could be resolved to an <see cref="IPage"/> in the containing
        ///             <see cref="IDocument"/> then:
        ///             <p/>
        ///             <see cref="Target"/> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> in the <see cref="IDocument"/> whose <see cref="IPage.TargetName"/> matches the
        ///             <see cref="IReference"/>'s is renamed
        ///         </term>
        ///         <term>
        ///             <b>TargetName</b> is updated to match the new name
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> in the <see cref="IDocument"/> has its <see cref="IPage.TargetName"/> changed to
        ///             match the <see cref="IReference"/>'s
        ///         </term>
        ///         <term>
        ///             <see cref="Target"/> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        /// </list>
        /// <p/>
        /// These events are checked for regardless of whether <see cref="Target"/> is currently resolved. If the event requires
        /// the value of <b>TargetName</b> to change, this will occur even if the <see cref="IReference"/> is
        /// <see cref="IDOMObject.IsImmutable">immutable</see> or <see cref="IPageElement.IsLocked">locked</see>.
        /// <p/>
        /// Changing the value of this property will set <see cref="Target"/> to <c>null</c> and <see cref="TargetStatus"/> to
        /// <see cref="API.TargetStatus.Unresolved"/>.
        /// <p/>
        /// Raises the <see cref="TargetNameChanged"/> event when its value changes.
        /// <p/>
        /// Default value is the string <c>"Undefined"</c>.
        /// </remarks>
        string TargetName { get; set; }

        /// <summary>
        /// Occurs when <see cref="TargetName"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> TargetNameChanged;

        /// <summary>
        /// Gets or sets an optional <see cref="IDocument"/> to use when resolving <see cref="Target"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If set, the <see cref="IReference"/> will check the supplied <see cref="IDocument"/> when trying to resolve
        /// <see cref="Target"/>. It will be checked after checking the <see cref="IReference"/>'s
        /// <see cref="IDocumentElement.Document">containing document</see>, if any.
        /// <p/>
        /// The purpose of this is to allow an <see cref="IReference"/> which is not currently a descendant of a specific
        /// <see cref="IDocument"/> to resolve targets from that document in order to perform a temporary or 'preview' render
        /// prior to adding the <see cref="IReference"/> to the <see cref="IDocument"/>; it is not normally used.
        /// <p/>
        /// The value of this property is not preserved when the <see cref="IReference"/> is
        /// <see cref="IDOMObject.Clone">cloned</see> or serialized. It may be set if the <see cref="IReference"/> is
        /// <see cref="IDOMObject.IsFrozen">frozen</see>, <see cref="IDOMObject.IsImmutable">immutable</see> or
        /// <see cref="IPageElement.IsLocked">locked</see>.
        /// <p/>
        /// Default value is <c>null</c>.
        /// </remarks>
        IDocument TargetContext { get; set; }

        /// <summary>
        /// Gets the status of <see cref="Target"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// The value returned indicates whether <see cref="Target"/> is currently resolved, or else the reason why it is not.
        /// As resolving <see cref="Target"/> may be an expensive operation, checking the value returned is an efficient means
        /// of determining whether <see cref="Target"/> is currently available. The possible return values are:
        /// <p/>
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="API.TargetStatus.Unresolved"/></term>
        ///         <term>
        ///             No attempt has been made to resolve <see cref="Target"/> since the last time <see cref="TargetName"/>
        ///             was set
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="API.TargetStatus.Missing"/></term>
        ///         <term>
        ///             The last attempt to resolve <see cref="Target"/> could not find a file matching <see cref="TargetName"/>
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="API.TargetStatus.CircularReference"/></term>
        ///         <term>
        ///             The last attempt to resolve <see cref="Target"/> found a file that would create a circular-dependency if
        ///             loaded
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="API.TargetStatus.Unloadable"/></term>
        ///         <term>
        ///             The last attempt to resolve <see cref="Target"/> found a file that could not be loaded because it was
        ///             not a valid LDraw file
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="API.TargetStatus.Resolved"/></term>
        ///         <term>The last attempt to resolve <see cref="Target"/> was successful</term>
        ///     </item>
        /// </list>
        /// <p/>
        /// This status is updated each time <see cref="Target"/> performs a resolve. It is reset to
        /// <see cref="API.TargetStatus.Unresolved"/> when <see cref="TargetName"/> is changed or
        /// <see cref="ClearTarget">ClearTarget()</see> is called, or when an <see cref="Target">event occurs that requires
        /// Target to be cleared</see>.
        /// <p/>
        /// The value of this property is not preserved when the <see cref="IReference"/> is
        /// <see cref="IDOMObject.Clone">cloned</see> or serialized.
        /// </remarks>
        TargetStatus TargetStatus { get; }

        /// <summary>
        /// Gets the <see cref="IPage"/> referenced by <see cref="TargetName"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ParserException">One of the <see cref="ParserException"/> subtypes if there was a problem with the
        ///     <b>Target</b>.</exception>
        /// <remarks>
        /// On the first access to this property after a change to <see cref="TargetName"/>, the <see cref="IReference"/> will
        /// attempt to resolve its reference to an <see cref="IPage"/>.
        /// <p/>
        /// Unless <see cref="TargetName"/> is an absolute file-path, the <see cref="IReference"/>'s containing
        /// <see cref="IDocument"/> is checked first, followed by <see cref="TargetContext"/> if set, then the folder
        /// containing the <see cref="IDocument"/>'s underlying file, and finally the members of the
        /// <see cref="Digitalis.LDTools.DOM.Configuration.PrimarySearchPath"/>.
        /// <p/>
        /// Note that this can be an expensive operation, so only use this property if you actually need the <see cref="IPage"/>.
        /// Use <see cref="TargetStatus"/> if you simply want to know whether the target has been resolved yet.
        /// <p/>
        /// If <see cref="TargetName"/> cannot be resolved, <c>null</c> is returned and <see cref="TargetStatus"/> is set to a
        /// value indicating the reason for the failure.
        /// <p/>
        /// The <see cref="IReference"/> will react to changes in its containing <see cref="IDocument"/>'s structure which would
        /// affect the resolution of <b>Target</b>:
        /// <p/>
        /// <list type="table">
        ///     <listheader><term>Event</term><term>Action taken</term></listheader>
        ///     <item>
        ///         <term>
        ///             The <see cref="IReference"/>'s <see cref="IDocumentElement.PathToDocumentChanged"/> event is raised
        ///         </term>
        ///         <term>
        ///             If <b>Target</b> is or could be resolved to an <see cref="IPage"/> in the containing
        ///             <see cref="IDocument"/> then:
        ///             <p/>
        ///             <b>Target</b> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> whose <see cref="IPage.TargetName"/> matches the <see cref="IReference"/>'s is
        ///             added to or removed from the <see cref="IDocument"/>
        ///         </term>
        ///         <term>
        ///             If <b>Target</b> is or could be resolved to an <see cref="IPage"/> in the containing
        ///             <see cref="IDocument"/> then:
        ///             <p/>
        ///             <b>Target</b> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> in the <see cref="IDocument"/> whose <see cref="IPage.TargetName"/> matches the
        ///             <see cref="IReference"/>'s is renamed
        ///         </term>
        ///         <term>
        ///             <see cref="TargetName"/> is updated to match the new name
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///             An <see cref="IPage"/> in the <see cref="IDocument"/> has its <see cref="IPage.TargetName"/> changed to
        ///             match the <see cref="IReference"/>'s
        ///         </term>
        ///         <term>
        ///             <b>Target</b> is set to <c>null</c>
        ///             <p/>
        ///             <see cref="TargetStatus"/> is set to <see cref="API.TargetStatus.Unresolved"/>
        ///             <p/>
        ///             <see cref="TargetChanged"/> is raised
        ///         </term>
        ///     </item>
        /// </list>
        /// <p/>
        /// These events are checked for regardless of whether <b>Target</b> is currently resolved. If the event requires
        /// the value of <see cref="TargetName"/> to change, this will occur even if the <see cref="IReference"/> is
        /// <see cref="IDOMObject.IsImmutable">immutable</see> or <see cref="IPageElement.IsLocked">locked</see>.
        /// <p/>
        /// The value of this property is not preserved when the <see cref="IReference"/> is
        /// <see cref="IDOMObject.Clone">cloned</see> or serialized.
        /// <p/>
        /// Raises the <see cref="TargetChanged"/> event when its value changes.
        /// </remarks>
        IPage Target { get; }

        /// <summary>
        /// Occurs when <see cref="Target"/> changes, or when <see cref="Target"/> itself issues a
        ///     <see cref="IDOMObject.Changed"/> event.
        /// </summary>
        /// <remarks>
        /// If the event occurred as a result of a <see cref="IDOMObject.Changed"/> event from <see cref="Target"/>, the
        /// <see cref="ObjectChangedEventArgs"/> from that event will be passed back.
        /// </remarks>
        event EventHandler TargetChanged;

        /// <summary>
        /// Clears <see cref="Target"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IReference"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// <see cref="Target"/> will be set to <c>null</c> and <see cref="TargetStatus"/> will be set to
        /// <see cref="API.TargetStatus.Unresolved"/>.
        /// </remarks>
        void ClearTarget();

        #endregion Target-management
    }
}
