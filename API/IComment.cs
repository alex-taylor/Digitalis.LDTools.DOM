#region License

//
// IComment.cs
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
using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw comment as defined in <see href="http://www.ldraw.org/article/218.html#lt0">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Self-description</h3>
    /// <b>IComment</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Comment"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IComment : IGroupable
    {
        #region Properties

        /// <summary>
        /// Gets or sets the content of the <see cref="IComment"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IComment"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IComment"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IComment"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IComment"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <see cref="System.String.Empty"/> and <c>null</c> are permitted. Carriage-returns and line-feeds will be stripped
        /// out, and trailing whitespace will be removed. Setting the property to <c>null</c> or an all-whitespace string is
        /// equivalent to passing <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="TextChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="System.String.Empty"/>.
        /// </remarks>
        string Text { get; set; }

        /// <summary>
        /// Occurs when <see cref="Text"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> TextChanged;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IComment"/> represents the 'empty comment'.
        /// </summary>
        /// <remarks>
        /// An <see cref="IComment"/> is regarded as being 'empty' if <see cref="Text"/> is the
        /// <see cref="System.String.Empty">empty string</see> or consists solely of the characters <c>'//'</c> (excluding any
        /// leading whitespace).
        /// </remarks>
        bool IsEmpty { get; }

        #endregion Properties
    }
}
