#region License

//
// IDOMObjectCollection.cs
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
    using System.Collections.Generic;

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents a collection of <see cref="IDOMObject"/>s that can be individually accessed by index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <remarks>
    ///
    /// <h3>Collection-management</h3>
    /// <b>IDOMObjectCollection&lt;T&gt;</b> extends <see cref="System.Collections.Generic.IList{T}"/> and adds some restrictions
    /// on what can be added to it. The members of <see cref="System.Collections.Generic.IList{T}"/> will throw exceptions for
    /// the following conditions, checked for in this order:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="System.ObjectDisposedException"/></term>
    ///         <description>
    ///             The <see cref="IDOMObject"/> to be inserted is <see cref="System.IDisposable.Dispose">disposed</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ObjectFrozenException"/></term>
    ///         <description>
    ///             The <see cref="IDOMObject"/> to be inserted is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.NotSupportedException"/></term>
    ///         <description>
    ///             The <b>IDOMObjectCollection&lt;T&gt;</b> is
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.ArgumentNullException"/></term>
    ///         <description>An attempt was made to insert a <c>null</c> value into the <b>IDOMObjectCollection&lt;T&gt;</b></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.InvalidOperationException"/></term>
    ///         <description>
    ///             The insert is prohibited by any of the other restrictions imposed by
    ///             <see cref="CanReplace">CanReplace()</see>
    ///         </description>
    ///     </item>
    /// </list>
    /// <p/>
    /// Implementations and subtypes of <b>IDOMObjectCollection&lt;T&gt;</b> may add further restrictions and exceptions.
    ///
    /// </remarks>
    public interface IDOMObjectCollection<T> : IList<T> where T : class, IDOMObject
    {
        #region Collection-management

        /// <summary>
        /// Occurs when <see cref="IDOMObject"/>s are added to the <see cref="IDOMObjectCollection{T}"/>.
        /// </summary>
        event UndoableListChangedEventHandler<T> ItemsAdded;

        /// <summary>
        /// Occurs when <see cref="IDOMObject"/>s are removed from the <see cref="IDOMObjectCollection{T}"/>.
        /// </summary>
        event UndoableListChangedEventHandler<T> ItemsRemoved;

        /// <summary>
        /// Occurs when <see cref="IDOMObject"/>s in the <see cref="IDOMObjectCollection{T}"/> are
        ///     <see cref="System.Collections.Generic.IList{T}.this">replaced</see>.
        /// </summary>
        event UndoableListReplacedEventHandler<T> ItemsReplaced;

        /// <summary>
        /// Occurs when the <see cref="IDOMObjectCollection{T}"/> is cleared.
        /// </summary>
        event UndoableListChangedEventHandler<T> CollectionCleared;

        /// <summary>
        /// Checks whether an <see cref="IDOMObject"/> can be added to the <see cref="IDOMObjectCollection{T}"/>.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="flags">Flags to control the behaviour of the check.</param>
        /// <returns>A value indicating whether <paramref name="obj"/> may be added to the collection.</returns>
        /// <remarks>
        /// Equivalent to calling <see cref="CanReplace">CanReplace()</see> with <i>objToReplace</i> set to <c>null</c>.
        /// </remarks>
        InsertCheckResult CanInsert(T obj, InsertCheckFlags flags);

        /// <summary>
        /// Checks whether an <see cref="IDOMObject"/> can be added to the <see cref="IDOMObjectCollection{T}"/> by
        ///     <see cref="System.Collections.Generic.IList{T}.this">replacing</see> an existing member.
        /// </summary>
        /// <param name="objToInsert">The <see cref="IDOMObject"/> to check.</param>
        /// <param name="objToReplace">The <see cref="IDOMObject"/> to replace, or <c>null</c> to check for an insertion.</param>
        /// <param name="flags">Flags to control the behaviour of the check.</param>
        /// <returns>
        /// A value indicating whether <paramref name="objToInsert"/> may replace <paramref name="objToReplace"/>.
        /// </returns>
        /// <remarks>
        /// An <see cref="IDOMObject"/> may only be added to an <see cref="IDOMObjectCollection{T}"/> if:
        /// <p/>
        /// <list type="bullet">
        ///     <item>
        ///         <term>
        ///             The <see cref="IDOMObjectCollection{T}"/> is not
        ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
        ///         </term>
        ///     </item>
        ///     <item><term>The <see cref="IDOMObject"/> is not <see cref="IDOMObject.IsDisposed">disposed</see></term></item>
        ///     <item><term>The <see cref="IDOMObject"/> is not <see cref="IDOMObject.IsFrozen">frozen</see></term></item>
        ///     <item>
        ///         <term>
        ///             The <see cref="IDOMObject"/> is not already a member of this or another
        ///             <see cref="IDOMObjectCollection{T}"/>
        ///         </term>
        ///     </item>
        /// </list>
        /// <p/>
        /// If not <c>null</c>, <paramref name="objToReplace"/> is excluded when carrying out the above checks.
        /// <p/>
        /// Specifying <see cref="InsertCheckFlags.IgnoreCurrentCollection"/> in <paramref name="flags"/> allows you to determine
        /// whether an <see cref="IDOMObject"/> which is currently a child of an <see cref="IDOMObjectCollection{T}"/> may be
        /// added to this <see cref="IDOMObjectCollection{T}"/> without having to first remove it from its present location.
        /// <p/>
        /// Note that implementations and subtypes of <see cref="IDOMObjectCollection{T}"/> may add further restrictions.
        /// </remarks>
        InsertCheckResult CanReplace(T objToInsert, T objToReplace, InsertCheckFlags flags);

        #endregion Collection-management
    }
}
