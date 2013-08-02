#region License

//
// ITexmapGeometry.cs
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
    /// <summary>
    /// Represents one of the sub-collections of an <see cref="ITexmap"/>.
    /// </summary>
    /// <remarks>
    ///
    /// <h2>Code-generation</h2>
    /// If the <b>ITexmapGeometry</b> is
    /// <see cref="IPageElement.IsLocked">locked</see>&#160;<see cref="IPageElement.IsLocalLock">explicitly</see>, the
    /// <i>'0 !DIGITALIS_LDTOOLS_DOM LOCKGEOM'</i> meta-command is output. Then, <see cref="IDOMObject.ToCode">ToCode()</see>
    /// iterates over the contents of the <b>ITexmapGeometry</b> and generates code for each in turn, passing in the parameters
    /// it was given. If <see cref="GeometryType"/> is <see cref="TexmapGeometryType.Texture"/> then each line of code generated
    /// for the contents is prefixed with <i>'0 !: '</i>.
    ///
    /// <h2>Collection-management</h2>
    /// Most <see cref="IElement"/>s which are not <see cref="IElement.IsTopLevelElement">top-level</see> can be added to an
    /// <b>ITexmapGeometry</b>, with the exception of <see cref="ITexmap"/>.
    ///
    /// <h2>Disposal</h2>
    /// An <b>ITexmapGeometry</b> which is attached to an <see cref="ITexmap"/> may not be
    /// <see cref="System.IDisposable.Dispose">disposed</see> directly; attempting to do so will cause
    /// <see cref="System.InvalidOperationException"/> to be thrown. Disposing the <see cref="ITexmap"/> will automatically
    /// dispose its descendants.
    ///
    /// <h2>Geometry</h2>
    /// The <see cref="IGeometric.Origin"/> of an <b>ITexmapCollection</b> is the centre of its
    /// <see cref="IGeometric.BoundingBox"/>. Its <see cref="IGeometric.WindingMode"/> is the
    /// <see cref="IGeometric.WindingMode"/> of its <see cref="Texmap"/>.
    ///
    /// <h2>Self-description</h2>
    /// <b>ITexmapGeometry</b> returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Collection"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElementCollection.AllowsTopLevelElements"/></term><description><c>false</c></description></item>
    ///     <item>
    ///         <term><see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/></term>
    ///         <description><c>true</c> if <see cref="IDOMObject.IsImmutable"/> is <c>true</c>; otherwise implementation-specific</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GeometryType"/></term>
    ///         <description>One of the <see cref="TexmapGeometryType"/> values, depending on which of the <see cref="ITexmap"/>
    ///         geometry-collections the <b>ITexmapGeometry</b> represents</description>
    ///     </item>
    /// </list>
    ///
    /// </remarks>
    public interface ITexmapGeometry : IElementCollection
    {
        #region Document-tree

        /// <summary>
        /// Gets the <see cref="ITexmap"/> the <see cref="ITexmapGeometry"/> belongs to.
        /// </summary>
        /// <remarks>
        /// This simply returns <see cref="IElementCollection.Parent"/>.
        /// </remarks>
        ITexmap Texmap { get; }

        #endregion Document-tree

        #region Self-description

        /// <summary>
        /// Gets the type of <see cref="ITexmap"/> geometry-collection the <see cref="ITexmapGeometry"/> represents.
        /// </summary>
        TexmapGeometryType GeometryType { get; }

        #endregion Self-description
    }
}
