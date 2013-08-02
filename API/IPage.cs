#region License

//
// IPage.cs
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
    /// Represents a section of an LDraw document as defined in
    ///     <see href="http://www.ldraw.org/article/218.html">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// An <b>IPage</b> is the basic component of an <see cref="IDocument"/>, which normally contains at least one page. Pages
    /// are collections of <see cref="IStep"/>s, which in turn contain <see cref="IElement"/>s which represent the page's
    /// geometry, meta-commands and so on.
    /// <p/>
    /// The fields of an LDraw file's <see href="http://www.ldraw.org/article/398.html">header</see> are exposed as a set of
    /// properties on the <b>IPage</b>.
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> iterates over the contents of the <b>IPage</b> and generates code for each
    /// in turn, passing in the parameters it was given.
    /// <p/>
    /// If <see cref="PageType"/> is <see cref="API.PageType.Model"/>, calling <see cref="IDOMObject.ToCode">ToCode()</see> with
    /// a <i>codeFormat</i> of <see cref="CodeStandards.PartsLibrary"/> will cause an <see cref="System.ArgumentException"/> to
    /// be thrown.
    /// <p/>
    /// If <see cref="PageType"/> is anything other than <see cref="API.PageType.Model"/>, calling
    /// <see cref="IDOMObject.ToCode">ToCode()</see> with a <i>codeFormat</i> of
    /// <see cref="CodeStandards.OfficialModelRepository"/> will have the same effect as calling it with a <i>codeFormat</i> of
    /// <see cref="CodeStandards.PartsLibrary"/>.
    ///
    /// <h3>Collection-management</h3>
    /// In addition to the restrictions imposed by
    /// <see cref="IDOMObjectCollection{T}.CanReplace">IDOMObjectCollection&lt;T&gt;.CanReplace()</see>, an <see cref="IStep"/>
    /// may only be added to an <b>IPage</b> if:
    /// <p/>
    /// <list type="bullet">
    ///     <item><term>The <b>IPage</b> is not <see cref="IDOMObject.IsFrozen">frozen</see></term></item>
    ///     <item><term>The <b>IPage</b> is not <see cref="IDOMObject.IsImmutable">immutable</see></term></item>
    ///     <item>
    ///         <term>
    ///             The <b>IPage</b> is not
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </term>
    ///     </item>
    ///     <item><term>The insert does not violate any of the restrictions of <see cref="IStep"/></term></item>
    /// </list>
    /// <p/>
    /// The members of <see cref="System.Collections.Generic.IList{T}"/> will throw exceptions for the following conditions,
    /// checked for in this order:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="System.ObjectDisposedException"/></term>
    ///         <description>
    ///             The <b>IPage</b> is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IStep"/> to be inserted is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ObjectFrozenException"/></term>
    ///         <description>
    ///             The <b>IPage</b> is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IStep"/> to be inserted is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.NotSupportedException"/></term>
    ///         <description>
    ///             The <b>IPage</b> is <see cref="IDOMObject.IsImmutable">immutable</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <b>IPage</b> is
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.ArgumentNullException"/></term>
    ///         <description>An attempt was made to insert a <c>null</c> value into the <b>IPage</b></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.InvalidOperationException"/></term>
    ///         <description>
    ///             The insert is prohibited by any of the other restrictions imposed by
    ///             <see cref="IDOMObjectCollection{T}.CanReplace">CanReplace()</see>
    ///         </description>
    ///     </item>
    /// </list>
    /// <p/>
    /// Implementations of <b>IPage</b> may add further restrictions.
    /// <p/>
    /// When adding or removing <see cref="IElement"/>s, the <b>IPage</b> will raise one of
    /// <see cref="IDOMObjectCollection{T}.ItemsAdded"/>, <see cref="IDOMObjectCollection{T}.ItemsRemoved"/>,
    /// <see cref="IDOMObjectCollection{T}.ItemsReplaced"/> or <see cref="IDOMObjectCollection{T}.CollectionCleared"/> and each
    /// <see cref="IStep"/> will raise <see cref="IStep.PageChanged"/>. These events will also be raised via the
    /// <b>IPage</b>'s <see cref="IDOMObject.Changed"/> event, but are not guaranteed to appear in any particular
    /// order.
    ///
    /// <h3>Disposal</h3>
    /// <see cref="System.IDisposable.Dispose">Disposing</see> an <b>IPage</b> will automatically remove it from its
    /// <see cref="Document"/> and dispose of its descendants. If the document-tree is <see cref="IDOMObject.IsFrozen"/>, or
    /// <see cref="Document"/> is <see cref="IDOMObject.IsImmutable">immutable</see> or
    /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>,
    /// <see cref="System.IDisposable.Dispose"/> will throw the appropriate exception and the disposal will not take place.
    ///
    /// <h3>Geometry</h3>
    /// The <see cref="IGeometric.Origin"/> of an <b>IPage</b> is <see cref="OpenTK.Vector3d.Zero"/>, and its
    /// <see cref="IGeometric.WindingMode"/> is the current value of <see cref="BFC"/>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IPage</b> returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Page"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item>
    ///         <term><see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly"/></term>
    ///         <description><c>true</c> if <see cref="IDOMObject.IsImmutable"/> is <c>true</c>; otherwise implementation-specific</description>
    ///     </item>
    /// </list>
    ///
    /// </remarks>
    public interface IPage : IGeometric, IDOMObjectCollection<IStep>
    {
        #region Collection-management

        /// <summary>
        /// Gets a value indicating whether the <see cref="IPage"/> contains <see cref="IPageElement.IsLocked">locked</see>
        ///     descendants.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        bool HasLockedDescendants { get; }

        #endregion Collection-management

        #region Document-tree

        /// <summary>
        /// Gets or sets the <see cref="IDocument"/> the <see cref="IPage"/> is a child of.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and either the <see cref="IPage"/> or the
        ///     <see cref="IDocument"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see> or
        ///     <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and the
        ///     <see cref="IPage"/>&#160;<see cref="IDOMObjectCollection{T}.CanReplace">cannot be added</see> to the
        ///     <see cref="IDocument"/> for some other reason.</exception>
        /// <remarks>
        /// Setting this property is equivalent to calling <see cref="System.Collections.Generic.ICollection{T}.Add">Add()</see>
        /// or <see cref="System.Collections.Generic.ICollection{T}.Remove">Remove()</see> on the supplied
        /// <see cref="IDocument"/> and passing in the <see cref="IPage"/>.
        /// <p/>
        /// It is possible to set this property if the <see cref="IPage"/> is <see cref="IPageElement.IsLocked">locked</see> or
        /// <see cref="IDOMObject.IsImmutable">immutable</see>, but not if it is <see cref="IDOMObject.IsFrozen">frozen</see>.
        /// <p/>
        /// If the <see cref="IPage"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this property
        /// will only be preserved if the <see cref="IDocument"/> is included in the same operation.
        /// <p/>
        /// <see cref="System.IDisposable.Dispose">Disposing</see> an <see cref="IPage"/> will automatically remove it from its
        /// <b>Document</b>. If <b>Document</b> is <see cref="IDOMObject.IsImmutable">immutable</see>,
        /// <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see> or
        /// <see cref="IPageElement.IsLocked">locked</see>, <see cref="System.IDisposable.Dispose">Dispose()</see> will throw
        /// the appropriate exception and the disposal will not take place.
        /// <p/>
        /// Raises the <see cref="DocumentChanged"/> and <see cref="IDocumentElement.PathToDocumentChanged"/> events when its
        /// value changes.
        /// </remarks>
        new IDocument Document { get; set; }

        /// <summary>
        /// Occurs when <see cref="Document"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IDocument> DocumentChanged;

        #endregion Document-tree

        #region Properties

        /// <summary>
        /// Gets the <see cref="IElement"/>s in the <see cref="IPage"/>'s <see cref="IStep"/>s.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// This is provided as a convenience: rather than having to iterate over each <see cref="IStep"/> in the
        /// <see cref="IPage"/> and then over its <see cref="IElement"/>s, the <b>Elements</b> accessor represents the
        /// <see cref="IPage"/> as a flat list of <see cref="IElement"/>s in the same order that they appear in the
        /// <see cref="IStep"/>s.
        /// </remarks>
        IDOMObjectCollection<IElement> Elements { get; }

        /// <summary>
        /// Gets the target-name of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// The returned string is <see cref="Name"/> with the appropriate extension (<c>".ldr"</c> for
        /// <see cref="API.PageType.Model"/>s, <c>".dat"</c> for everything else) appended. If <see cref="PageType"/> is
        /// <see cref="API.PageType.Subpart"/> the returned string is prefixed with <c>'s\'</c>; if <see cref="PageType"/> is
        /// <see cref="API.PageType.HiresPrimitive"/> the returned string is prefixed with <c>'48\'</c>.
        /// <p/>
        /// Raises the <see cref="TargetNameChanged"/> event when its value changes.
        /// </remarks>
        string TargetName { get; }

        /// <summary>
        /// Occurs when <see cref="TargetName"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> TargetNameChanged;

        /// <summary>
        /// Gets or sets the name of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The property is set and the supplied value was <c>null</c> or empty.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the supplied value contained
        ///     <see cref="System.IO.Path.GetInvalidFileNameChars">invalid characters</see> or was greater than 255 characters
        ///     in length.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and the supplied value would result in a
        ///     circular dependency.</exception>
        /// <exception cref="DuplicateNameException">The property is set and an <see cref="IPage"/> with the same
        ///     <see cref="TargetName"/> already exists in <see cref="Document"/>.</exception>
        /// <remarks>
        /// This is the name of the file that the <see cref="IPage"/> represents. If the <see cref="IPage"/> is a member of a
        /// multi-page <see cref="IDocument"/>, this is the name of the file that it would be written to if
        /// <see cref="IDocument.Publish(DocumentWriterCallback)">published</see>.
        /// <p/>
        /// When setting, if the supplied value includes the <c>'.mpd'</c>, <c>'.ldr'</c> or <c>'.dat'</c> extension or the
        /// <c>'s\'</c> or <c>'48\'</c> prefix, these will be removed. They are added to <see cref="TargetName"/> automatically
        /// based on the <see cref="PageType"/> of the <see cref="IPage"/>.
        /// <p/>
        /// Names should not exceed 21 characters in length for values of <see cref="PageType"/> other than
        /// <see cref="API.PageType.Model"/>, and must not exceed 255 characters in length in any event. If
        /// <see cref="PageType"/> is not <see cref="API.PageType.Model"/> then Name should follow the restrictions in
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// <p/>
        /// If a fully- or partially-qualified filesystem path is set, it will be trimmed down to the filename component.
        /// Strings will have trailing and leading whitespace removed.
        /// <p/>
        /// Setting this will cause <see cref="TargetName"/> to update.
        /// <p/>
        /// Raises the <see cref="NameChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>"Untitled"</c>.
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Occurs when <see cref="Name"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> NameChanged;

        /// <summary>
        /// Gets or sets the title of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Strings will have trailing and leading whitespace removed. Setting the property to <c>null</c> or an all-whitespace
        /// string is equivalent to setting it to <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="TitleChanged"/> event when its value changes.
        /// <p/>
        /// Default value is dependent on the initial value of <see cref="PageType"/> when the <see cref="IPage"/> is
        /// constructed:
        /// <p/>
        /// <list type="table">
        ///     <item><see cref="API.PageType.Model"/></item><item><c>"New Model"</c></item>
        ///     <item><see cref="API.PageType.Part"/></item><item><c>"New Part"</c></item>
        ///     <item><see cref="API.PageType.Part_Alias"/></item><item><c>"New Alias"</c></item>
        ///     <item><see cref="API.PageType.Part_Physical_Colour"/></item><item><c>"New Physical-Colour Shortcut"</c></item>
        ///     <item><see cref="API.PageType.Shortcut"/></item><item><c>"New Shortcut"</c></item>
        ///     <item><see cref="API.PageType.Shortcut_Alias"/></item><item><c>"New Shortcut Alias"</c></item>
        ///     <item><see cref="API.PageType.Shortcut_Physical_Colour"/></item><item><c>"New Physical-Colour Shortcut"</c></item>
        ///     <item><see cref="API.PageType.Subpart"/></item><item><c>"New Subpart"</c></item>
        ///     <item><see cref="API.PageType.Primitive"/></item><item><c>"New Primitive"</c></item>
        ///     <item><see cref="API.PageType.HiresPrimitive"/></item><item><c>"New Hires Primitive"</c></item>
        /// </list>
        /// </remarks>
        string Title { get; set; }

        /// <summary>
        /// Occurs when <see cref="Title"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> TitleChanged;

        /// <summary>
        /// Gets or sets the Theme of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and <see cref="PageType"/> is not
        ///     <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>.</exception>
        /// <remarks>
        /// Strings will have trailing and leading whitespace removed. Setting the property to <c>null</c> or an all-whitespace
        /// string is equivalent to setting it to <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="ThemeChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="System.String.Empty"/>.
        /// </remarks>
        string Theme { get; set; }

        /// <summary>
        /// Occurs when <see cref="Theme"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> ThemeChanged;

        /// <summary>
        /// Gets or sets the author of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Strings will have trailing and leading whitespace removed. Setting the property to <c>null</c> or an all-whitespace
        /// string is equivalent to setting it to <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="AuthorChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Configuration.Author"/>, or <see cref="System.String.Empty"/> if this is not set.
        /// </remarks>
        string Author { get; set; }

        /// <summary>
        /// Occurs when <see cref="Author"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> AuthorChanged;

        /// <summary>
        /// Gets or sets the LDraw.org username of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Strings will have trailing and leading whitespace removed. Setting the property to <c>null</c> or an all-whitespace
        /// string is equivalent to setting it to <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="UserChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Configuration.Username"/>, or <see cref="System.String.Empty"/> if this is not set.
        /// </remarks>
        string User { get; set; }

        /// <summary>
        /// Occurs when <see cref="User"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> UserChanged;

        /// <summary>
        /// Gets or sets the type of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="DuplicateNameException">The property is set and an <see cref="IPage"/> with a matching
        ///     <see cref="TargetName"/> already exists in <see cref="Document"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="API.PageType"/>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and the supplied value would result in
        ///     <see cref="TargetName"/> changing such that it would create a circular dependency.</exception>
        /// <remarks>
        /// Setting this will cause <see cref="TargetName"/> to update.
        /// <p/>
        /// Raises the <see cref="PageTypeChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="API.PageType.Model"/>.
        /// </remarks>
        PageType PageType { get; set; }

        /// <summary>
        /// Occurs when <see cref="PageType"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<PageType> PageTypeChanged;

        /// <summary>
        /// Gets or sets the license of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="API.License"/>.</exception>
        /// <remarks>
        /// Raises the <see cref="LicenseChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="API.License.CCAL2"/> if either <see cref="Author"/> or <see cref="User"/> has a value on
        /// construction; or <see cref="API.License.None"/> otherwise.
        /// </remarks>
        License License { get; set; }

        /// <summary>
        /// Occurs when <see cref="License"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<License> LicenseChanged;

        /// <summary>
        /// Gets or sets the category of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and <see cref="PageType"/> is one of
        ///     <see cref="API.PageType.Primitive"/>, <see cref="API.PageType.HiresPrimitive"/> or
        ///     <see cref="API.PageType.Model"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="API.Category"/>.</exception>
        /// <remarks>
        /// If no explicit value has been set, the property will attempt to derive a value from the <see cref="Title"/> and/or
        /// <see cref="Name"/> of the <see cref="IPage"/> on each <c>get</c> operation.
        /// <p/>
        /// Raises the <see cref="CategoryChanged"/> when its value changes.
        /// <p/>
        /// Default value is <see cref="API.Category.Unknown"/>.
        /// </remarks>
        Category Category { get; set; }

        /// <summary>
        /// Occurs when <see cref="Category"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<Category> CategoryChanged;

        /// <summary>
        /// Gets or sets the default culling-mode of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="CullingMode"/>.</exception>
        /// <remarks>
        /// This is the initial culling-mode. It may be overridden by <see cref="IBFCFlag"/> elements in the page.
        /// <p/>
        /// Raises the <see cref="BFCChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="CullingMode.NotSet"/>.
        /// </remarks>
        CullingMode BFC { get; set; }

        /// <summary>
        /// Occurs when <see cref="BFC"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<CullingMode> BFCChanged;

        /// <summary>
        /// Gets or sets the default colour of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and <see cref="PageType"/> is one of
        ///     <see cref="API.PageType.Primitive"/>, <see cref="API.PageType.HiresPrimitive"/> or
        ///     <see cref="API.PageType.Model"/>, or the supplied value is a <i>Direct Colours</i> value.</exception>
        /// <remarks>
        /// Raises the <see cref="DefaultColourChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Digitalis.LDTools.DOM.Palette.MainColour"/>.
        /// </remarks>
        uint DefaultColour { get; set; }

        /// <summary>
        /// Occurs when <see cref="DefaultColour"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<uint> DefaultColourChanged;

        /// <summary>
        /// Gets or sets the 'update' information of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Raises the <see cref="UpdateChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>null</c>.
        /// </remarks>
        LDUpdate? Update { get; set; }

        /// <summary>
        /// Occurs when <see cref="Update"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<LDUpdate?> UpdateChanged;

        /// <summary>
        /// Gets or sets the help-text of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Newlines (in '\r\n' format) are permitted. Setting the property to <c>null</c> or an all-whitespace string is
        /// equivalent to setting it to <see cref="System.String.Empty"/>.
        /// <p/>
        /// Raises the <see cref="HelpChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="System.String.Empty"/>.
        /// </remarks>
        string Help { get; set; }

        /// <summary>
        /// Occurs when <see cref="Help"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> HelpChanged;

        /// <summary>
        /// Gets or sets the keywords of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The property is set and <see cref="PageType"/> is either
        ///     <see cref="API.PageType.Primitive"/> or <see cref="API.PageType.HiresPrimitive"/>.</exception>
        /// <remarks>
        /// When setting, the supplied set will be copied and any empty entries skipped. Each entry will have trailing and
        /// leading whitespace will be removed, and whitespace between words will be collapsed to a single space (' ') character.
        /// When getting, a copy of the stored value is returned.
        /// <p/>
        /// Raises the <see cref="KeywordsChanged"/> event when its value changes.
        /// <p/>
        /// Default value is an empty set.
        /// </remarks>
        IEnumerable<string> Keywords { get; set; }

        /// <summary>
        /// Occurs when <see cref="Keywords"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IEnumerable<string>> KeywordsChanged;

        /// <summary>
        /// Gets or sets the history of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The property is set and the
        ///     <see cref="Digitalis.LDTools.DOM.LDHistory.Description"/> of one of the
        ///     <see cref="Digitalis.LDTools.DOM.LDHistory"/>s is <c>null</c>, empty or whitespace.</exception>
        /// <remarks>
        /// When setting, the supplied set will be copied and its entries sorted by date, oldest first. When getting, a copy of
        /// the stored value is returned.
        /// <p/>
        /// Raises the <see cref="HistoryChanged"/> event when its value changes.
        /// <p/>
        /// Default value is a single <see cref="Digitalis.LDTools.DOM.LDHistory"/> with the current date, <see cref="Author"/>
        /// and <see cref="User"/>; or if neither <see cref="Author"/> or <see cref="User"/> has a value, the empty set.
        /// </remarks>
        IEnumerable<LDHistory> History { get; set; }

        /// <summary>
        /// Occurs when <see cref="History"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IEnumerable<LDHistory>> HistoryChanged;

        /// <summary>
        /// Gets or sets the rotation-point of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid
        ///     <see cref="Digitalis.LDTools.DOM.MLCadRotationConfig.Type"/> or an index into <see cref="RotationConfig"/>.</exception>
        /// <remarks>
        /// Value is either a member of <see cref="Digitalis.LDTools.DOM.MLCadRotationConfig.Type"/> or the 1-based index of the
        /// <see cref="RotationConfig"/> to use.
        /// <p/>
        /// Raises the <see cref="RotationPointChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Digitalis.LDTools.DOM.MLCadRotationConfig.Type.PartOrigin"/>.
        /// </remarks>
        MLCadRotationConfig.Type RotationPoint { get; set; }

        /// <summary>
        /// Occurs when the value of <see cref="RotationPoint"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<MLCadRotationConfig.Type> RotationPointChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="RotationPoint"/> should be displayed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Raises the <see cref="RotationPointVisibleChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>false</c>.
        /// </remarks>
        bool RotationPointVisible { get; set; }

        /// <summary>
        /// Occurs when <see cref="RotationPointVisible"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<bool> RotationPointVisibleChanged;

        /// <summary>
        /// Gets or sets the rotation-config of the <see cref="IPage"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The property is set and the
        ///     <see cref="Digitalis.LDTools.DOM.MLCadRotationConfig.Name"/> of one of the
        ///     <see cref="Digitalis.LDTools.DOM.MLCadRotationConfig"/>s is <c>null</c>, empty or whitespace.</exception>
        /// <remarks>
        /// When setting, the supplied set will be copied. When getting, a copy of the stored value is returned.
        /// <p/>
        /// The enumeration may contain any number of elements. If a new value is set which has fewer elements than the current
        /// value of <see cref="RotationPoint"/>, <see cref="RotationPoint"/> will be set to the last element in the enumeration.
        /// <p/>
        /// Raises the <see cref="RotationConfigChanged"/> event when its value changes.
        /// <p/>
        /// Default value is an empty set.
        /// </remarks>
        IEnumerable<MLCadRotationConfig> RotationConfig { get; set; }

        /// <summary>
        /// Occurs when <see cref="RotationConfig"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IEnumerable<MLCadRotationConfig>> RotationConfigChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IPage"/> should be inlined when
        ///     <see cref="IDocument.Publish(DocumentWriterCallback)">publishing</see> its containing <see cref="Document"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IPage"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <remarks>
        /// Default behaviour when <see cref="IDocument.Publish(DocumentWriterCallback)">publishing</see> <see cref="Document"/>
        /// is to export each <see cref="IPage"/> either as part of the .mpd file (if publishing a
        /// <see cref="API.PageType.Model"/>) or as a separate file (if publishing anything else). Setting this property to
        /// <c>true</c> instructs <see cref="IDocument.Publish(DocumentWriterCallback)">Publish()</see> to replace any
        /// <see cref="IReference"/>s whose <see cref="IReference.TargetName"/> refers to the <see cref="IPage"/> with the
        /// <see cref="IElement"/>s that make up the <see cref="IPage"/>, transformed by the reference's
        /// <see cref="IReference.Matrix"/> and <see cref="IGraphic.ColourValue"/> as required.
        /// <p/>
        /// <see cref="IDocument.Publish(DocumentWriterCallback)">Publish()</see> only checks the value of this property if the
        /// <see cref="IPage"/> is a member of the <see cref="IDocument"/> being published; if the <see cref="IReference"/>
        /// refers to an <see cref="IPage"/> in another <see cref="IDocument"/>, it will not be inlined.
        /// <p/>
        /// Raises the <see cref="InlineOnPublishChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>false</c>.
        /// </remarks>
        bool InlineOnPublish { get; set; }

        /// <summary>
        /// Occurs when <see cref="InlineOnPublish"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<bool> InlineOnPublishChanged;

        #endregion Properties
    }
}
