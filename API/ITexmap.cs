#region License

//
// ITexmap.cs
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

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw <i>!TEXMAP</i> meta-command as defined in
    ///     <see href="http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html">the Language Extension for Texture Mapping</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h2>Analytics</h2>
    /// <see cref="IGraphic.IsDuplicateOf">IsDuplicateOf()</see> is not supported by <b>ITexmap</b> and always returns
    /// <c>false</c>.
    ///
    /// <h2>Attributes</h2>
    /// <b>ITexmap</b> implements <see cref="IGraphic.IsVisible"/> and <see cref="IGraphic.IsGhosted"/> as 'meta' properties:
    /// setting them will apply the specified value to each <see cref="IGraphic"/> member of <see cref="TextureGeometry"/>,
    /// <see cref="SharedGeometry"/> and <see cref="FallbackGeometry"/> rather than to the <b>ITexmap</b> itself. When getting
    /// the property values, <see cref="IGraphic.IsVisible"/> will return <c>false</c> if all <see cref="IGraphic"/>s in each of
    /// the three <see cref="ITexmapGeometry"/> collections are currently not visible, and <c>true</c> otherwise;
    /// <see cref="IGraphic.IsGhosted"/> will return <c>true</c> if all <see cref="IGraphic"/>s in each collection are currently
    /// ghosted, and <c>false</c> otherwise. If the <b>ITexmap</b> does not contain any <see cref="IGraphic"/>s then
    /// <see cref="IGraphic.IsVisible"/> will return <c>true</c> and <see cref="IGraphic.IsGhosted"/> will return <c>false</c>.
    /// <p/>
    /// The <see cref="IGraphic.IsVisibleChanged"/> and <see cref="IGraphic.IsGhostedChanged"/> events will function as normal
    /// and will be raised whenever the values of <see cref="IGraphic.IsVisible"/> and <see cref="IGraphic.IsGhosted"/> change
    /// regardless of whether the change was caused by the property being set on the <b>ITexmap</b> or by one of the
    /// <see cref="IGraphic"/>s it contains having its property changed directly.
    ///
    /// <h2>Code-generation</h2>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> generates code as defined in
    /// <see href="http://www.ldraw.org/documentation/ldraw-org-file-format-standards/language-extension-for-texture-mapping.html">the Language Extension for Texture Mapping</see>.
    /// <p/>
    /// If none of <see cref="TextureGeometry"/>, <see cref="SharedGeometry"/> or <see cref="FallbackGeometry"/> have content,
    /// or if <see cref="IGraphic.IsVisible"/> is <c>false</c> or <see cref="IGraphic.IsGhosted"/> is <c>true</c>, and
    /// <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> then no code will be generated.
    ///
    /// <h2>Collection-management</h2>
    /// In order to simplify renderers which do not support texturing, <b>ITexmap</b> implements <see cref="IElementCollection"/>
    /// as a read-only accessor over the contents of its <see cref="SharedGeometry"/> and <see cref="FallbackGeometry"/>
    /// <see cref="ITexmapGeometry"/> collections. <see cref="TextureGeometry"/> is specifically excluded from this behaviour;
    /// thus a renderer may simply treat an <b>ITexmap</b> as an <see cref="IElementCollection"/> and draw the non-textured
    /// version of its graphics without needing to understand the semantics of <i>!TEXMAP</i>.
    /// <p/>
    /// As a <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see> implementation of
    /// <see cref="IElementCollection"/>, all the mutator APIs will throw <see cref="System.NotSupportedException"/> when
    /// invoked, and the collection-change events are inactive. <see cref="IDOMObjectCollection{T}.CanInsert">CanInsert()</see>
    /// and <see cref="IDOMObjectCollection{T}.CanReplace">CanReplace()</see> will always return
    /// <see cref="InsertCheckResult.NotSupported"/>.
    /// <p/>
    /// When accessing the members of the <b>ITexmap</b>, the contents of <see cref="SharedGeometry"/> will be returned first,
    /// followed by those of <see cref="FallbackGeometry"/>.
    ///
    /// <h2>Self-description</h2>
    /// ITexmap returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Texmap"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElementCollection.AllowsTopLevelElements"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/></term><description><c>true</c></description></item>
    ///     <item><term><see cref="IGraphic.ColourValueEnabled"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IGraphic.OverrideableColourValue"/></term><description><i>undefined</i></description></item>
    ///     <item><term><see cref="IGraphic.CoordinatesCount"/></term><description><c>3</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface ITexmap : IGraphic, IElementCollection
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="ITexmapGeometry"/> representing the primary textured geometry of the <see cref="ITexmap"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// When texture-rendering is enabled, the members of this collection will be rendered with <see cref="Texture"/> and
        /// <see cref="Glossmap"/> applied. When texture-rendering is disabled or otherwise unavailable, the members of this
        /// collection will not be rendered.
        /// </remarks>
        ITexmapGeometry TextureGeometry { get; }

        /// <summary>
        /// Gets the <see cref="ITexmapGeometry"/> representing the shared geometry of the <see cref="ITexmap"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// When texture-rendering is enabled, the members of this collection will be rendered with <see cref="Texture"/> and
        /// <see cref="Glossmap"/> applied. When texture-rendering is disabled or otherwise unavailable, the members of this
        /// collection will be rendered normally.
        /// <p/>
        /// For convenience, the members of this collection may also be accessed via the <see cref="IElementCollection"/> API
        /// provided by the <see cref="ITexmap"/> itself. This is a read-only API, and includes the members of
        /// <see cref="FallbackGeometry"/> as well.
        /// </remarks>
        ITexmapGeometry SharedGeometry { get; }

        /// <summary>
        /// Gets an <see cref="ITexmapGeometry"/> representing the primary geometry which should be used when texture-rendering
        ///     is disabled or unavailable.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// When texture-rendering is enabled, the members of this collection will not be rendered. When texture-rendering is
        /// disabled or unavailable, the members of this collection will be rendered in place of <see cref="TextureGeometry"/>.
        /// <p/>
        /// For convenience, the members of this collection may also be accessed via the <see cref="IElementCollection"/> API
        /// provided by the <see cref="ITexmap"/> itself. This is a read-only API, and includes the members of
        /// <see cref="SharedGeometry"/> as well.
        /// </remarks>
        ITexmapGeometry FallbackGeometry { get; }

        /// <summary>
        /// Gets or sets the projection to use when applying <see cref="Texture"/> to <see cref="TextureGeometry"/> and
        ///     <see cref="SharedGeometry"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="TexmapProjection"/>.</exception>
        /// <remarks>
        /// Each projection requires a set of parameters to be supplied via <see cref="Point1"/>, <see cref="Point2"/> and
        /// <see cref="Point3"/>, and optionally the <see cref="HorizontalExtent"/> and <see cref="VerticalExtent"/> properties.
        /// <p/>
        /// Raises the <see cref="ProjectionChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="TexmapProjection.Planar"/>.
        /// </remarks>
        TexmapProjection Projection { get; set; }

        /// <summary>
        /// Occurs when <see cref="Projection"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<TexmapProjection> ProjectionChanged;

        /// <summary>
        /// Gets or sets the first projection control-point of the <see cref="ITexmap"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the first member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        Vector3d Point1 { get; set; }

        /// <summary>
        /// Gets or sets the second projection control-point of the <see cref="ITexmap"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the second member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        Vector3d Point2 { get; set; }

        /// <summary>
        /// Gets or sets the third projection control-point of the <see cref="ITexmap"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is equivalent to getting or setting the third member of <see cref="IGraphic.Coordinates"/>.
        /// <p/>
        /// Raises the <see cref="IGraphic.CoordinatesChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        Vector3d Point3 { get; set; }

        /// <summary>
        /// Gets or sets the horizontal extent of the <see cref="TexmapProjection.Cylindrical"/> and
        ///     <see cref="TexmapProjection.Spherical"/> <see cref="Projection"/>s.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value was out of range.</exception>
        /// <remarks>
        /// <p/>
        /// Valid range is <c>0&lt;n&lt;=360</c>.
        /// <p/>
        /// Raises the <see cref="HorizontalExtentChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>360.0</c>.
        /// </remarks>
        double HorizontalExtent { get; set; }

        /// <summary>
        /// Occurs when <see cref="HorizontalExtent"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<double> HorizontalExtentChanged;

        /// <summary>
        /// Gets or sets the horizontal extent of the <see cref="TexmapProjection.Spherical"/> <see cref="Projection"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value was out of range.</exception>
        /// <remarks>
        /// Valid range is <c>0&lt;n&lt;=360</c>.
        /// <p/>
        /// Raises the <see cref="VerticalExtentChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>360.0</c>.
        /// </remarks>
        double VerticalExtent { get; set; }

        /// <summary>
        /// Occurs when <see cref="VerticalExtent"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<double> VerticalExtentChanged;

        /// <summary>
        /// Gets or sets the filename of the texture image.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The supplied string was <c>null</c>, empty or whitespace.</exception>
        /// <remarks>
        /// The supplied filename may be a fully-qualified filepath, or may refer to a texture-image file in the containing
        /// folder of the <see cref="ITexmap"/>'s <see cref="IDocument"/> or in the
        /// <see cref="Digitalis.LDTools.DOM.Configuration.FullSearchPath"/>; if found in one of these locations,
        /// <see cref="TexturePath"/> will return the fully-qualified filepath.
        /// <p/>
        /// Raises the <see cref="TextureChanged"/> event when its value changes.
        /// <p/>
        /// Default value is the string <c>"Undefined"</c>.
        /// </remarks>
        string Texture { get; set; }

        /// <summary>
        /// Occurs when <see cref="Texture"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> TextureChanged;

        /// <summary>
        /// Gets the fully-qualified filepath of <see cref="Texture"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// <para>
        /// If <see cref="Texture"/> has not been set to the filepath of a suitable image file, this will return <c>null</c>;
        /// otherwise it returns the path of the texture-image file to be used.
        /// </para>
        /// </remarks>
        string TexturePath { get; }

        /// <summary>
        /// Gets or sets the filepath of the glossmap image.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="ITexmap"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is an optional texture image, whose alpha-channel is used to add specular highlighting to the main
        /// <see cref="Texture"/>.
        /// <p/>
        /// The supplied filename may be a fully-qualified filepath, or may refer to a texture-image file in the containing
        /// folder of the <see cref="ITexmap"/>'s <see cref="IDocument"/> or in the
        /// <see cref="Digitalis.LDTools.DOM.Configuration.FullSearchPath"/>; if found in one of these locations,
        /// <see cref="GlossmapPath"/> will return the fully-qualified filepath.
        /// <p/>
        /// Raises the <see cref="GlossmapChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>null</c>.
        /// </remarks>
        string Glossmap { get; set; }

        /// <summary>
        /// Occurs when <see cref="Glossmap"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> GlossmapChanged;

        /// <summary>
        /// Gets the fully-qualified filepath of <see cref="Texture"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="ITexmap"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If <see cref="Glossmap"/> has not been set to the filepath of a suitable image file, this will return <c>null</c>;
        /// otherwise it returns the path of the texture-image file to be used.
        /// </remarks>
        string GlossmapPath { get; }

        #endregion Properties
    }
}
