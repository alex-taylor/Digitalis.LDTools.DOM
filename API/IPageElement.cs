#region License

//
// IPageElement.cs
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
    /// Represents a descendant of an <see cref="IPage"/>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Code-generation</h3>
    /// If <see cref="IsLocalLock"/> is <c>true</c> and <i>codeFormat</i> is either <see cref="CodeStandards.Full"/> or
    /// <see cref="CodeStandards.OfficialModelRepository"/>, <see cref="IDOMObject.ToCode">ToCode()</see> will prefix the LDraw
    /// code generated for the <b>IPageElement</b> with either the <i>!DIGITALIS_LDTOOLS_DOM LOCKGEOM</i> meta-command (for
    /// instances of <see cref="ITexmapGeometry"/>) or <i>!DIGITALIS_LDTOOLS_DOM LOCKNEXT</i> (for all other types) to <i>sb</i>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IPageElement</b> returns the following values:
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
    public interface IPageElement : IDocumentElement
    {
        #region Document-tree

        /// <summary>
        /// Gets the <see cref="IPage"/> the <see cref="IPageElement"/> is a descendant of.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the <see cref="IPageElement"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this
        /// property will only be preserved if the <see cref="IPage"/> is included in the same operation.
        /// <p/>
        /// Raises the <see cref="IDocumentElement.PathToDocumentChanged"/> event when its value changes.
        /// </remarks>
        IPage Page { get; }

        /// <summary>
        /// Gets the <see cref="IStep"/> the <see cref="IPageElement"/> is a descendant of.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the <see cref="IPageElement"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this
        /// property will only be preserved if the <see cref="IStep"/> is included in the same operation.
        /// <p/>
        /// Raises the <see cref="IDocumentElement.PathToDocumentChanged"/> event when its value changes.
        /// </remarks>
        IStep Step { get; }

        #endregion Document-tree

        #region Locking

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IPageElement"/> may be modified.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IPageElement"/> is locked
        ///     implicitly.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// An <see cref="IPageElement"/> may be locked explicitly by setting the value of this property to <c>true</c>, or
        /// implicitly by locking any of its ancestors in the document-tree. This property will return <c>true</c> in both
        /// cases: to determine which is the case, see <see cref="IsLocalLock"/>.
        /// <p/>
        /// If the <see cref="IPageElement"/> is locked explicitly, it is always possible to change the value of this property.
        /// If the <see cref="IPageElement"/> is locked implicitly, or is <see cref="IDOMObject.IsFrozen">frozen</see> either
        /// explicitly or implicitly, the value of this property may not be changed.
        /// <p/>
        /// An <see cref="IPageElement"/> that is locked does not allow modification of its properties or, if it is a collection
        /// of other elements, additions to or removals from the collection. The <see cref="IPageElement"/> may itself be added to
        /// or removed from a collection, however. It may also be <see cref="IDOMObject.Freeze">frozen</see>.
        /// <p/>
        /// Any attempt to modify a locked <see cref="IPageElement"/> will cause an <see cref="ElementLockedException"/> to be
        /// thrown.
        /// <p/>
        /// Raises the <see cref="IsLockedChanged"/> event when its value changes.
        /// <p/>
        /// For convenience, this property will return <c>false</c> if <see cref="IDOMObject.IsDisposing"/> is <c>true</c> in
        /// order to simplify implementing <see cref="System.IDisposable.Dispose">Dispose()</see>.
        /// <p/>
        /// Default value is <c>false</c>.
        /// <p/>
        /// <note>
        /// Note to implementors: this must be supported. Each property or method which modifies the <see cref="IPageElement"/>
        /// must check the value of <b>IsLocked</b> before proceeding, and throw an <see cref="ElementLockedException"/> if it
        /// is <c>true</c>. If the <see cref="IPageElement"/> is <see cref="IDOMObject.IsDisposed">disposed</see>,
        /// <see cref="IDOMObject.IsImmutable">immutable</see> or <see cref="IDOMObject.IsFrozen">frozen</see> then these must
        /// be checked for first (and in that order) and the appropriate exceptions thrown, as these take precedence over this
        /// property. If overriding this property, you must <c>OR</c> your value with that returned by the superclass.
        /// </note>
        /// </remarks>
        bool IsLocked { get; set; }

        /// <summary>
        /// Occurs when <see cref="IsLocked"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<bool> IsLockedChanged;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPageElement"/> is locked explicitly or implicitly.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPageElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// An <see cref="IPageElement"/> may be locked explicitly by setting <see cref="IsLocked"/> to <c>true</c>, or
        /// implicitly by locking any of its ancestors in the document-tree. This property will return <c>true</c> if the lock
        /// is explicit and <c>false</c> if it is explicit or if the <see cref="IPageElement"/> is not locked.
        /// </remarks>
        bool IsLocalLock { get; }

        #endregion Locking
    }
}
