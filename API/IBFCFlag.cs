#region License

//
// IBFCFlag.cs
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

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents a back-face culling (BFC) meta-command as defined in
    ///     <see href="http://www.ldraw.org/article/415.html">the LDraw.org Language Extension for Back Face Culling</see>.
    /// </summary>
    /// <remarks>
    ///
    /// The <i>CLIP</i>, <i>NOCLIP</i>, <i>CW</i> and <i>CCW</i> options are supported by this type. For the <i>INVERTNEXT</i>
    /// option, please see <see cref="IReference.Invert">IReference.Invert</see>. For the <i>CERTIFY</i> and <i>NOCERTIFY</i>
    /// options, please see <see cref="IPage.BFC">IPage.BFC</see>.
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> will generate a single LDraw <i>BFC</i> statement whose value is determined
    /// by <see cref="Flag"/>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IBFCFlag</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.MetaCommand"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>true</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IBFCFlag : IMetaCommand
    {
        #region Properties

        /// <summary>
        /// Gets or sets the back-face culling flag.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IBFCFlag"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IBFCFlag"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IBFCFlag"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IBFCFlag"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="BFCFlag"/>.</exception>
        /// <remarks>
        /// Raises the <see cref="FlagChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise"/>.
        /// </remarks>
        BFCFlag Flag { get; set; }

        /// <summary>
        /// Occurs when <see cref="Flag"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<BFCFlag> FlagChanged;

        #endregion Properties
    }
}
