#region License

//
// IDOMObject.cs
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
    using System.Text;

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents an object in a <see cref="Digitalis.LDTools.DOM.API">Digitalis.LDTools.DOM.API</see> document-tree.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Cloning and Serialization</h3>
    /// Non-abstract implementations of <b>IDOMObject</b> are required to provide a parameterless constructor in order for
    /// <see cref="Clone">Clone()</see> to function.
    /// <p/>
    /// Implementations must be serializable, as must any types they depend on to store values; typically these are the fields
    /// which are copied by <see cref="Clone">Clone()</see>.
    ///
    /// <h3>Disposal</h3>
    /// <b>IDOMObject</b> is an <see cref="System.IDisposable"/>, and as such instances of it should be disposed of when
    /// finished with. If an <b>IDOMObject</b> is a child of another, disposing of it will remove it from its parent.
    /// <p/>
    /// If the document-tree is <see cref="IsFrozen">frozen</see>, disposal may only take place at the root of the tree;
    /// calling <see cref="System.IDisposable.Dispose">Dispose()</see> on any other member of the tree will cause an
    /// <see cref="ObjectFrozenException"/> to be thrown.
    /// <p/>
    /// Once disposed, all members of the <b>IDOMObject</b> will throw <see cref="System.ObjectDisposedException"/> when
    /// accessed unless otherwise noted.
    ///
    /// <h3>Self-description</h3>
    /// <b>IDOMObject</b>s may be either mutable or immutable on creation. Mutable objects may become immutable during their
    /// lifetime by being <see cref="IDOMObject.IsFrozen">frozen</see>; immutable objects stay that way forever.
    /// <p/>
    /// A frozen object will throw an <see cref="ObjectFrozenException"/> when any attempt is made to modify it. An immutable
    /// object, unless also frozen, will throw a <see cref="System.NotSupportedException"/> when any attempt is made to modify
    /// it. Frozen objects which are a member of a document-tree may only be
    /// <see cref="System.IDisposable.Dispose">disposed</see> by disposing the entire tree.
    /// <p/>
    /// <b>IDOMObject</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="ObjectType"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IsImmutable"/></term><description>Implementation-specific</description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IDOMObject : IDisposable
    {
        #region Change-notification

        /// <summary>
        /// Occurs when the <see cref="IDOMObject"/> changes in some way.
        /// </summary>
        /// <remarks>
        /// Whenever a property-specific or action-specific event is raised by the <see cref="IDOMObject"/>, this event will
        /// also be sent, with its <see cref="ObjectChangedEventArgs.Operation"/> property set to the name of the specific event
        /// that occurred. This event may thus be subscribed to as an alternative to subscribing individually to each of the
        /// specific events.
        /// <p/>
        /// If the <see cref="IDOMObject"/> has child objects, their <b>Changed</b> events will be forwarded on through this
        /// event too.
        /// </remarks>
        event ObjectChangedEventHandler Changed;

        #endregion Change-notification

        #region Cloning

        /// <summary>
        /// Creates a copy of the <see cref="IDOMObject"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDOMObject"/> is
        ///     <see cref="IsDisposed">disposed</see>.</exception>
        /// <returns>A copy of the <see cref="IDOMObject"/>.</returns>
        /// <remarks>
        /// Unless otherwise noted, the copy will be identical to the original object.
        /// </remarks>
        IDOMObject Clone();

        #endregion Cloning

        #region Code-generation

        /// <summary>
        /// Returns the <see cref="IDOMObject"/> as LDraw code.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDOMObject"/> is
        ///     <see cref="IsDisposed">disposed</see>.</exception>
        /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <param name="codeFormat">The format required for the returned code.</param>
        /// <param name="overrideColour">The colour-value to be used to override <see cref="IGraphic.OverrideableColourValue"/>
        ///     in any <see cref="IGraphic"/>s in the returned code.</param>
        /// <param name="transform">The transform to be applied to any <see cref="IGeometric"/>s in the returned code.</param>
        /// <param name="winding">The winding direction to be used by any <see cref="IGeometric"/>s in the returned code.</param>
        /// <returns>
        /// A reference to <paramref name="sb"/> after the append operation has completed.
        /// </returns>
        StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding);

        #endregion Code-generation

        #region Disposal

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDOMObject"/> is currently being disposed of.
        /// </summary>
        /// <remarks>
        /// This returns <c>true</c> if either the <see cref="IDOMObject"/> or its parent (if any) is currently being disposed
        /// of by a call to <see cref="System.IDisposable.Dispose">Dispose()</see>, and <c>false</c> at any other time. Subtypes
        /// of <see cref="IDOMObject"/> should use this value to determine how much, if any, of their resources need to be
        /// disposed of.
        /// <p/>
        /// When <see cref="System.IDisposable.Dispose">Dispose()</see> is invoked, the <see cref="IDOMObject"/> should take the
        /// following actions:
        /// <p/>
        /// <list type="bullet">
        ///     <item><term>set <b>IsDisposing</b> to <c>true</c></term></item>
        ///     <item>
        ///         <term>
        ///             if it has a parent object in the document-tree whose IsDisposing property is <c>false</c>, it should
        ///             remove itself from the parent
        ///         </term>
        ///     </item>
        ///     <item><term>clean up any locally-held resources</term></item>
        ///     <item><term>dispose of any children</term></item>
        ///     <item><term>set <see cref="IsDisposed"/> to <c>true</c></term></item>
        ///     <item><term>set <b>IsDisposing</b> to <c>false</c></term></item>
        /// </list>
        /// <p/>
        /// Whilst it is not an error for an <see cref="IDOMObject"/> to explicitly remove itself from a parent object which is
        /// in the process of being disposed, it is usually unnecessary and may result in performance problems so should be
        /// avoided.
        /// </remarks>
        bool IsDisposing { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDOMObject"/> has been disposed of.
        /// </summary>
        /// <remarks>
        /// Following a successful call to <see cref="System.IDisposable.Dispose">Dispose()</see>, this property will return
        /// <c>true</c>. Any attempt to modify the <see cref="IDOMObject"/> or access its properties (other than this one) after
        /// this point will result in an <see cref="System.ObjectDisposedException"/> being thrown.
        /// <p/>
        /// It is safe to call <see cref="System.IDisposable.Dispose">Dispose()</see> if this property is <c>true</c>; doing so
        /// will have no effect.
        /// <p/>
        /// <note>
        /// Note to implementors: this must be supported. Each property or method which modifies the <see cref="IDOMObject"/>
        /// must check the value of <b>IsDisposed</b> before proceeding, and throw an
        /// <see cref="System.ObjectDisposedException"/> if it is <c>true</c>.
        /// </note>
        /// </remarks>
        bool IsDisposed { get; }

        #endregion Disposal

        #region Freezing

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDOMObject"/> is frozen.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDOMObject"/> is
        ///     <see cref="IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// An <see cref="IDOMObject"/> that is frozen may not be modified in any way, including adding it to or removing it
        /// from a collection, and attempting to do so will cause an <see cref="ObjectFrozenException"/> to be thrown. Freezing
        /// is permanent: a frozen <see cref="IDOMObject"/> can not be unfrozen.
        /// <p/>
        /// An <see cref="IDOMObject"/> is frozen implicitly if its immediate ancestor in the document-tree is frozen.
        /// <p/>
        /// The value of this property is not preserved when the <see cref="IDOMObject"/> is <see cref="Clone">cloned</see>.
        /// <p/>
        /// For convenience, this property will return <c>false</c> if <see cref="IsDisposing"/> is <c>true</c> in order to
        /// simplify implementing <see cref="System.IDisposable.Dispose">Dispose()</see>.
        /// <p/>
        /// Default value is <c>false</c>.
        /// <p/>
        /// <note>
        /// Note to implementors: this must be supported. Each property or method which modifies the <see cref="IDOMObject"/>
        /// must check the value of <b>IsFrozen</b> before proceeding, and throw an <see cref="ObjectFrozenException"/> if it is
        /// <c>true</c>. If the <see cref="IDOMObject"/> is <see cref="IsDisposed">disposed</see> then this must be checked for
        /// first and the appropriate exception thrown, as this takes precedence over <b>IsFrozen</b>. If overriding this
        /// property, you must <c>OR</c> your value with that returned by the superclass.
        /// </note>
        /// </remarks>
        bool IsFrozen { get; }

        /// <summary>
        /// Freezes the <see cref="IDOMObject"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDOMObject"/> is
        ///     <see cref="IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// <p/>
        /// If the <see cref="IDOMObject"/> has no immediate ancestor in the document-tree, it will be frozen explicitly;
        /// otherwise the tree is traversed to its root and the top-most object is frozen. As an <see cref="IDOMObject"/> is
        /// frozen implicitly when its immediate ancestor is frozen, calling this method on any member of a document-tree will
        /// have the effect of freezing the entire tree.
        /// <p/>
        /// An <see cref="IDOMObject"/> that is <see cref="IsFrozen">frozen</see> may not be modified in any way, including
        /// adding it to or removing it from a collection, and attempting to do so will cause an
        /// <see cref="ObjectFrozenException"/> to be thrown. Freezing is permanent: a frozen <see cref="IDOMObject"/> can not
        /// be unfrozen.
        /// <p/>
        /// An <see cref="IsImmutable">immutable</see>&#160;<see cref="IDOMObject"/> may be frozen.
        /// <p/>
        /// Calling <b>Freeze()</b> on an already-frozen <see cref="IDOMObject"/> will have no effect.
        /// </remarks>
        void Freeze();

        #endregion Freezing

        #region Self-description

        /// <summary>
        /// Gets the type of the <see cref="IDOMObject"/>.
        /// </summary>
        /// <remarks>
        /// As this value is a constant, it is safe to get when the <see cref="IDOMObject"/> is
        /// <see cref="IsDisposed">disposed</see>.
        /// </remarks>
        DOMObjectType ObjectType { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDOMObject"/> is immutable.
        /// </summary>
        /// <remarks>
        /// An <see cref="IDOMObject"/> that is immutable does not allow modification of its properties or, if it is a
        /// collection of other objects, addition or removal of collection-members. The <see cref="IDOMObject"/> may itself be
        /// added to or removed from a collection, however. It may also be <see cref="Freeze">frozen</see>.
        /// <p/>
        /// Any attempt to modify an immutable <see cref="IDOMObject"/> will cause a
        /// <see cref="System.NotSupportedException"/> to be thrown.
        /// <p/>
        /// The value of this property is set when the <see cref="IDOMObject"/> is created and does not change.
        /// <p/>
        /// As this value is a constant, it is safe to get when the <see cref="IDOMObject"/> is
        /// <see cref="IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: if this property is to return <c>true</c>, each property-setter or method from the API which
        /// would ordinarily modify the <see cref="IDOMObject"/> must throw a <see cref="System.NotSupportedException"/> when
        /// invoked.
        /// <p/>
        /// Implementations which return <c>true</c> here must be decorated with an <see cref="ElementFlagsAttribute"/> with
        /// the <see cref="ElementFlags.Immutable"/> flag set.
        /// </note>
        /// </remarks>
        bool IsImmutable { get; }

        #endregion Self-description
    }
}
