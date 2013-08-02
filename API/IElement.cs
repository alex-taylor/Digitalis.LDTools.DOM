#region License

//
// IElement.cs
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
    /// Represents a descendant of an <see cref="IElementCollection"/>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Disposal</h3>
    /// <see cref="System.IDisposable.Dispose">Disposing</see> an <b>IElement</b> will automatically remove it from its
    /// <see cref="Parent"/>. If <see cref="Parent"/> is <see cref="IDOMObject.IsImmutable">immutable</see>,
    /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see> or
    /// <see cref="IPageElement.IsLocked">locked</see>, <see cref="System.IDisposable.Dispose">Dispose()</see> will throw the
    /// appropriate exception and the disposal will not take place.
    ///
    /// <h3>Self-description</h3>
    /// <b>IElement</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IsStateElement"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IsTopLevelElement"/></term><description>Implementation-specific</description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IElement : IPageElement
    {
        #region Document-tree

        /// <summary>
        /// Gets or sets the <see cref="IElementCollection"/> the <see cref="IElement"/> is a member of.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and either the <see cref="IElement"/> or the
        ///     <see cref="IElementCollection"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IElementCollection"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see> or
        ///     <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IElementCollection"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and the
        ///     <see cref="IElement"/>&#160;<see cref="IDOMObjectCollection{T}.CanReplace">cannot be added</see> to the
        ///     <see cref="IElementCollection"/> for some other reason.</exception>
        /// <remarks>
        /// If the <see cref="IElement"/> is a member of an <see cref="IStep"/> then this returns the same value as
        /// <see cref="IPageElement.Step"/>.
        /// <p/>
        /// Setting this property is equivalent to calling <see cref="System.Collections.Generic.ICollection{T}.Add">Add()</see>
        /// or <see cref="System.Collections.Generic.ICollection{T}.Remove">Remove()</see> on the supplied
        /// <see cref="IElementCollection"/> and passing in the <see cref="IElement"/>.
        /// <p/>
        /// It is possible to set this property if the <see cref="IElement"/> is <see cref="IPageElement.IsLocked">locked</see>
        /// or <see cref="IDOMObject.IsImmutable">immutable</see>, but not if it is <see cref="IDOMObject.IsFrozen">frozen</see>.
        /// <p/>
        /// If the <see cref="IElement"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this property
        /// will only be preserved if the <see cref="IElementCollection"/> is included in the same operation.
        /// <p/>
        /// <see cref="System.IDisposable.Dispose">Disposing</see> an <see cref="IElement"/> will automatically remove it from
        /// its <b>Parent</b>. If <b>Parent</b> is <see cref="IDOMObject.IsImmutable">immutable</see>,
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see> or
        /// <see cref="IPageElement.IsLocked">locked</see>, <see cref="System.IDisposable.Dispose">Dispose()</see> will throw
        /// the appropriate exception and the disposal will not take place.
        /// <p/>
        /// Raises the <see cref="ParentChanged"/> and <see cref="IDocumentElement.PathToDocumentChanged"/> events when its
        /// value changes.
        /// </remarks>
        IElementCollection Parent { get; set; }

        /// <summary>
        /// Occurs when <see cref="Parent"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IElementCollection> ParentChanged;

        #endregion Document-tree

        #region Self-description

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElement"/> is a state-element.
        /// </summary>
        /// <remarks>
        /// An <see cref="IElement"/> is a state-element if its values or presence affect subsequent <see cref="IElement"/>s in
        /// its containing <see cref="IPage"/> - for example, an <see cref="IColour"/> element that defines a palette-entry
        /// which may be used by <see cref="IGraphic"/>s later in the page.
        /// <p/>
        /// As this value is a constant, it is safe to get when the <see cref="IElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// </remarks>
        bool IsStateElement { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElement"/> is a top-level element.
        /// </summary>
        /// <remarks>
        /// Top-level <see cref="IElement"/>s can only be added to <see cref="IElementCollection"/>s which
        /// <see cref="IElementCollection.AllowsTopLevelElements">permit them</see>.
        /// <p/>
        /// As this value is a constant, it is safe to get when the <see cref="IElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: if this property is to return <c>true</c> then the class must be decorated with the
        /// <see cref="ElementFlagsAttribute"/> with the flag <see cref="ElementFlags.TopLevelElement"/> set.
        /// </note>
        /// </remarks>
        bool IsTopLevelElement { get; }

        #endregion Self-description
    }
}
