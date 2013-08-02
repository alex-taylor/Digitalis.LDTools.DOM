#region License

//
// IGeometric.cs
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

    using Digitalis.LDTools.DOM.Geom;

    #endregion Usings

    /// <summary>
    /// Represents an <see cref="IDocumentElement"/> which has geometric properties.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Self-description</h3>
    /// <b>IGeometric</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IGeometric : IDocumentElement
    {
        #region Geometry

        /// <summary>
        /// Gets the bounding-box of the <see cref="IGeometric"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        Box3d BoundingBox { get; }

        /// <summary>
        /// Gets the origin of the <see cref="IGeometric"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        Vector3d Origin { get; }

        /// <summary>
        /// Gets the current BFC winding-mode of the <see cref="IGeometric"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the <see cref="IGeometric"/> is a member of a document-tree, the value returned is the cumulative effect of
        /// applying all <see cref="IBFCFlag"/> elements found between the head of the tree and the <see cref="IGeometric"/>. If
        /// no <see cref="IBFCFlag"/>s are present, the <b>WindingMode</b> of the containing <see cref="IPage"/> is returned, or
        /// <see cref="CullingMode.NotSet"/> if the <see cref="IGeometric"/> is not a descendant of an <see cref="IPage"/>.
        /// </remarks>
        CullingMode WindingMode { get; }

        /// <summary>
        /// Applies a transform to the <see cref="IGeometric"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The <see cref="IGeometric"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <param name="transform">The transform to be applied.</param>
        /// <remarks>
        /// The transform is applied around the origin of the coordinate-system.
        /// </remarks>
        void Transform(ref Matrix4d transform);

        /// <summary>
        /// Reverses the winding of the <see cref="IGeometric"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The <see cref="IGeometric"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The <see cref="IGeometric"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        void ReverseWinding();

        #endregion Geometry
    }
}
