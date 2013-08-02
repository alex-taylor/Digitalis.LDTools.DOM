#region License

//
// IGroup.cs
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

    using System.Collections.Generic;

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents an MLCad <i>GROUP</i> element as defined in
    ///     <see href="http://www.lm-software.com/mlcad/Specification_V2.0.pdf">the MLCad specifications</see>.
    /// </summary>
    /// <remarks>
    ///
    /// An <b>IGroup</b> is a means of associating one or more <see cref="IGroupable"/>s in an <see cref="IPage"/> for ease of
    /// manipulation or editing. The elements remain members of their containing page, and are attached to the group either by
    /// setting their <see cref="IGroupable.Group"/> property or by calling the members of
    /// <see cref="System.Collections.Generic.ICollection{T}"/>. An <see cref="IGroupable"/> may only be added to an
    /// <b>IGroup</b> if <see cref="IGroupable.IsGroupable"/> returns <c>true</c>; if this is not the case then attempting to
    /// add the <see cref="IGroupable"/> will cause an <see cref="System.InvalidOperationException"/> to be thrown.
    ///
    /// <h3>Cloning and Serialization</h3>
    /// As an <b>IGroup</b> does not contain the <see cref="IGroupable"/>s but is merely an accessor over them,
    /// <see cref="IDOMObject.Clone">cloning</see> or serializing it does not copy the <see cref="IGroupable"/>s.
    ///
    /// <h3>Code-generation</h3>
    /// If the <b>IGroup</b> is not empty and <i>codeFormat</i> is <see cref="CodeStandards.Full"/> or
    /// <see cref="CodeStandards.OfficialModelRepository"/>, <see cref="IDOMObject.ToCode">ToCode()</see> will append the
    /// <i>GROUP</i> meta-command.
    ///
    /// <h3>Disposal</h3>
    /// <see cref="System.IDisposable.Dispose">Disposing</see> an <b>IGroup</b> will automatically clear the
    /// <see cref="IGroupable.Group"/> property of any <see cref="IGroupable"/>s associated with it. The
    /// <see cref="IGroupable"/>s will not be disposed.
    ///
    /// <h3>Self-description</h3>
    /// <b>IGroup</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Group"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>true</c></description></item>
    ///     <item><term><see cref="IElementCollection.AllowsTopLevelElements"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/></term><description><c>false</c></description></item>
    /// </list>
    /// <p/>
    /// As an <b>IGroup</b> does not contain the <see cref="IGroupable"/>s but is merely an accessor over them, it is never
    /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>:
    /// <see cref="IDOMObject.IsFrozen">freezing</see>, <see cref="IDOMObject.IsImmutable">immutability</see> and
    /// <see cref="IPageElement.IsLocked">locking</see> only affect the behaviour of the <see cref="IGroup"/>'s properties and
    /// not its <see cref="System.Collections.Generic.ICollection{T}">collection API</see>.
    ///
    /// </remarks>
    public interface IGroup : IElement, IGeometric, ICollection<IGroupable>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the <see cref="IGroup"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IGroup"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IGroup"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IGroup"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IGroup"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The supplied string was <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="DuplicateNameException">An <see cref="IGroup"/> with the same name already exists in the
        ///     <see cref="IPage"/> that the <see cref="IGroup"/> is a member of.</exception>
        /// <remarks>
        /// <b>Name</b> is case-sensitive and may not be empty, whitespace or <c>null</c>.
        /// <p/>
        /// Raises the <see cref="NameChanged"/> event when its value changes.
        /// <p/>
        /// Defaults to <c>"Untitled"</c>.
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Occurs when <see cref="Name"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> NameChanged;

        #endregion Properties
    }
}
