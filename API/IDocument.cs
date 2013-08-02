#region License

//
// IDocument.cs
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
    using System.IO;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw document as defined in
    ///     <see href="http://www.ldraw.org/article/218.html">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> iterates over the contents of the <b>IDocument</b> and generates code for
    /// each in turn, passing in the parameters it was given. If the <b>IDocument</b> contains more than one <see cref="IPage"/>
    /// and <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> then only the first page will be output; otherwise
    /// each <see cref="IPage"/>'s code will be prefixed with the <i>0 FILE</i> meta-command.
    /// <p/>
    /// If the <b>IDocument</b> contains no <see cref="IPage"/>s then <see cref="IDOMObject.ToCode">ToCode()</see> will throw
    /// <see cref="System.InvalidOperationException"/>.
    ///
    /// <h2>Collection-management</h2>
    /// In addition to the restrictions imposed by
    /// <see cref="IDOMObjectCollection{T}.CanReplace">IDOMObjectCollection&lt;T&gt;.CanReplace()</see>, an
    /// <see cref="IPage"/> may only be added to an <b>IDocument</b> if:
    /// <p/>
    /// <list type="bullet">
    ///     <item><term>The <b>IDocument</b> is not <see cref="IDOMObject.IsFrozen">frozen</see></term></item>
    ///     <item><term>The <b>IDocument</b> is not <see cref="IDOMObject.IsImmutable">immutable</see></term></item>
    ///     <item>
    ///         <term>
    ///             The <b>IDocument</b> is not
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </term>
    ///     </item>
    ///     <item>
    ///         <term>
    ///             The <b>IDocument</b> does not already contain an <see cref="IPage"/> with a matching
    ///             <see cref="IPage.TargetName"/>
    ///         </term>
    ///     </item>
    ///     <item><term>The insert does not violate any of the restrictions of <see cref="IPage"/></term></item>
    /// </list>
    /// <p/>
    /// The members of <see cref="System.Collections.Generic.IList{T}"/> will throw exceptions for the following conditions,
    /// checked for in this order:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="System.ObjectDisposedException"/></term>
    ///         <description>
    ///             The <b>IDocument</b> is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IPage"/> to be inserted is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ObjectFrozenException"/></term>
    ///         <description>
    ///             The <b>IDocument</b> is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IPage"/> to be inserted is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.NotSupportedException"/></term>
    ///         <description>
    ///             The <b>IDocument</b> is <see cref="IDOMObject.IsImmutable">immutable</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <b>IDocument</b> is
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.ArgumentNullException"/></term>
    ///         <description>An attempt was made to insert a <c>null</c> value into the <b>IDocument</b></description>
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
    /// Implementations of <b>IDocument</b> may add further restrictions.
    /// <p/>
    /// When adding or removing <see cref="IPage"/>s, the <b>IDocument</b> will raise one of
    /// <see cref="IDOMObjectCollection{T}.ItemsAdded"/>, <see cref="IDOMObjectCollection{T}.ItemsRemoved"/>,
    /// <see cref="IDOMObjectCollection{T}.ItemsReplaced"/> or <see cref="IDOMObjectCollection{T}.CollectionCleared"/> and each
    /// <see cref="IPage"/> will raise <see cref="IPage.DocumentChanged"/>. These events will also be raised via the
    /// <b>IDocument</b>'s <see cref="IDOMObject.Changed"/> and <see cref="DocumentTreeChanged"/> events, but are not guaranteed
    /// to appear in any particular order.
    ///
    /// <h3>Disposal</h3>
    /// <see cref="System.IDisposable.Dispose">Disposing</see> an <b>IDocument</b> will also dispose its descendants.
    ///
    /// <h3>Self-description</h3>
    /// <b>IDocument</b> returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Document"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item>
    ///         <term><see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly"/></term>
    ///         <description><c>true</c> if <see cref="IDOMObject.IsImmutable"/> is <c>true</c>; otherwise implementation-specific</description>
    ///     </item>
    /// </list>
    ///
    /// </remarks>
    public interface IDocument : IDOMObject, IDOMObjectCollection<IPage>
    {
        #region Change-notification

        /// <summary>
        /// Occurs when the <see cref="IDocument"/> or one of its descendants changes in some way.
        /// </summary>
        /// <remarks>
        /// This event is provided as a convenience: rather than having to subscribe individually to the
        /// <see cref="IDOMObject.Changed"/> event of each descendant of the <see cref="IDocument"/>, subscribing to this event
        /// will catch all events raised by the <see cref="IDocument"/> and each of its descendants in a single location.
        /// <p/>
        /// If the event occurs between a call to <see cref="BeginUpdate"/> and <see cref="EndUpdate"/>, it will be buffered and
        /// raised on the last call to <see cref="EndUpdate"/>.
        /// </remarks>
        event DocumentTreeChangedEventHandler DocumentTreeChanged;

        /// <summary>
        /// Occurs when <see cref="BeginUpdate"/> is invoked.
        /// </summary>
        event EventHandler UpdateBegun;

        /// <summary>
        /// Occurs when <see cref="Update"/> is invoked.
        /// </summary>
        event EventHandler UpdateInProgress;

        /// <summary>
        /// Occurs when <see cref="EndUpdate"/> is invoked.
        /// </summary>
        event EventHandler UpdateEnded;

        /// <summary>
        /// Raises the <see cref="UpdateBegun"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <remarks>
        /// If <see cref="DocumentTreeChanged"/> events are required, this method should be called before modifying any part of
        /// an <see cref="IDocument"/> or its contents. When done, <see cref="EndUpdate"/> should be called.
        /// <p/>
        /// Calls to this method nest, so each call must be matched by exactly one call to <see cref="EndUpdate"/>.
        /// </remarks>
        void BeginUpdate();

        /// <summary>
        /// Raises the <see cref="UpdateInProgress"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <remarks>
        /// This may be called at any time between calls to <see cref="BeginUpdate"/> and <see cref="EndUpdate"/> in order to
        /// signal listeners that the <see cref="IDocument"/> has changed in some way, but that the updates are not yet complete.
        /// For example, a renderer may choose to listen for this event in order to refresh its display as the contents of the
        /// <see cref="IDocument"/> change during a lengthy edit operation.
        /// <p/>
        /// Calling this method outside a <see cref="BeginUpdate"/> / <see cref="EndUpdate"/> pairing will have no effect.
        /// </remarks>
        void Update();

        /// <summary>
        /// Raises the <see cref="UpdateEnded"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.InvalidOperationException">A call to this method was not preceded by a call to
        ///     <see cref="BeginUpdate"/>.</exception>
        /// <remarks>
        /// Calls to this method nest, so each call to <see cref="BeginUpdate"/> must be matched by exactly one call to this method.
        /// </remarks>
        void EndUpdate();

        #endregion Change-notification

        #region Collection-management

        /// <summary>
        /// Gets the <see cref="IPage"/> with the specified <see cref="IPage.TargetName"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// Names are not case-sensitive. If no <see cref="IPage"/> has the specified name, <c>null</c> is returned.
        /// </remarks>
        IPage this[string targetName] { get; }

        #endregion Collection-management

        #region Import and Export

        /// <summary>
        /// Adds the <see cref="IPage"/>s of one <see cref="IDocument"/> to another.
        /// </summary>
        /// <param name="import">The <see cref="IDocument"/> to take the <see cref="IPage"/>s from.</param>
        /// <exception cref="System.ObjectDisposedException">Either the <see cref="IDocument"/> or <paramref name="import"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">Either the <see cref="IDocument"/> or <paramref name="import"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">Either the <see cref="IDocument"/> or <paramref name="import"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see> or
        ///     <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>.</exception>
        /// <exception cref="DuplicateNameException">An <see cref="IPage"/> in <paramref name="import"/> has the same
        ///     <see cref="IPage.TargetName"/> as an <see cref="IPage"/> already present in the <see cref="IDocument"/>.</exception>
        /// <remarks>
        /// The donor <see cref="IDocument"/> will be left empty.
        /// </remarks>
        void Import(IDocument import);

        /// <summary>
        /// Writes the <see cref="IDocument"/> back to its underlying file.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.IO.FileNotFoundException"><see cref="Filepath"/> does not refer to a writeable file.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// The <see cref="IDocument"/> will be written to the file specified by <see cref="Filepath"/> in
        /// <see cref="CodeStandards.Full"/> format. <see cref="Filepath"/> must be a fully-qualified file-path; if a file
        /// already exists at that location it will be overwritten unless it is read-only.
        /// </remarks>
        void Save();

        /// <summary>
        /// Writes the <see cref="IDocument"/> to a <see cref="System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="System.IO.TextWriter"/>.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// The <see cref="IDocument"/> will be written to <paramref name="textWriter"/> in <see cref="CodeStandards.Full"/>
        /// format.
        /// </remarks>
        void Save(TextWriter textWriter);

        /// <summary>
        /// Publishes the <see cref="IDocument"/> to a file or files suitable for uploading to either the LDraw.org Parts Tracker
        ///     or the Official Model Repository.
        /// </summary>
        /// <param name="writer">A <see cref="DocumentWriterCallback"/> which will create the files on demand.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s, or all pages are marked as <see cref="IPage.InlineOnPublish"/>.</exception>
        /// <remarks>
        /// If <see cref="DocumentType"/> is determined to be <see cref="PageType.Model"/>, a single
        /// <see cref="System.IO.TextWriter"/> will be obtained from <paramref name="writer"/> and each <see cref="IPage"/>
        /// <i>not</i> marked as <see cref="IPage.InlineOnPublish"/> will be written to it in the same order they appear in the
        /// <see cref="IDocument"/>. <see cref="IPage"/>s marked as <see cref="IPage.InlineOnPublish"/> will be inlined where
        /// they are referenced; any which are not referenced will be omitted. The <see cref="IPage.Name"/> of the first
        /// <see cref="IPage"/> to be written out will be used as the <i>targetName</i> parameter for <paramref name="writer"/>,
        /// with an extension of <c>.ldr</c> for a single-page document and <c>.mpd</c> for a multi-page document. All
        /// <see cref="IPage"/>s will be written out in <see cref="CodeStandards.OfficialModelRepository"/> format.
        /// <p/>
        /// For all other <see cref="DocumentType"/>s, a separate <see cref="System.IO.TextWriter"/> will be obtained for each
        /// <see cref="IPage"/> <i>not</i> marked as <see cref="IPage.InlineOnPublish"/>, using the <see cref="IPage"/>'s
        /// <see cref="IPage.TargetName"/> as the <i>targetName</i> for <paramref name="writer"/>. <see cref="IPage"/>s marked
        /// as <see cref="IPage.InlineOnPublish"/> will be inlined where they are referenced; any which are not referenced will
        /// not be written. All <see cref="IPage"/>s will be written out in <see cref="CodeStandards.PartsLibrary"/> format.
        /// </remarks>
        void Publish(DocumentWriterCallback writer);

        /// <summary>
        /// Publishes the <see cref="IDocument"/> to a file or files suitable for uploading to either the LDraw.org Parts Tracker
        ///     or the Official Model Repository.
        /// </summary>
        /// <param name="folderPath">The folder path to save the files to.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// Each file <see cref="Publish(DocumentWriterCallback)">published</see> will be written to the specified folder, or to
        /// a subfolder in the case of <see cref="PageType.Subpart"/>s and <see cref="PageType.HiresPrimitive"/>s; if the
        /// corresponding subfolder does not yet exist it will be created.
        /// </remarks>
        /// <seealso cref="Publish(DocumentWriterCallback)"/>
        void Publish(string folderPath);

        /// <summary>
        /// Exports the <see cref="IPage"/>s of the <see cref="IDocument"/> to individual files.
        /// </summary>
        /// <param name="writer">A <see cref="DocumentWriterCallback"/> which will create the files on demand.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// Each <see cref="IPage"/> in the <see cref="IDocument"/> will be written out to a separate
        /// <see cref="System.IO.TextWriter"/> obtained from <paramref name="writer"/> using <see cref="IPage.TargetName"/> as
        /// the <i>targetName</i> parameter. All <see cref="IPage"/>s will be written out in <see cref="CodeStandards.Full"/>
        /// format.
        /// </remarks>
        void Export(DocumentWriterCallback writer);

        /// <summary>
        /// Exports the <see cref="IPage"/>s of the <see cref="IDocument"/> to individual files.
        /// </summary>
        /// <param name="folderPath">The folder path to save the files to.</param>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// Each file <see cref="Export(DocumentWriterCallback)">exported</see> will be written to the specified folder, or to a
        /// subfolder in the case of <see cref="PageType.Subpart"/>s and <see cref="PageType.HiresPrimitive"/>s; if the
        /// corresponding subfolder does not yet exist it will be created.
        /// </remarks>
        /// <seealso cref="Export(DocumentWriterCallback)"/>
        void Export(string folderPath);

        #endregion Import and Export

        #region Properties

        /// <summary>
        /// Gets or sets the path of the file the <see cref="IDocument"/> represents.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="T:System.ArgumentException">The property is set and the supplied value contained characters
        ///     <see cref="M:System.IO.Path.GetInvalidPathChars()">not permitted</see> by the filesystem.</exception>
        /// <remarks>
        /// This property does not support <see cref="Digitalis.UndoSystem.UndoStack"/>, and it can be set if the
        /// <see cref="IDocument"/> is <see cref="IDOMObject.IsFrozen">frozen</see> or
        /// <see cref="IDOMObject.IsImmutable">immutable</see>.
        /// <p/>
        /// Raises the <see cref="FilepathChanged"/> event when its value changes. Note that this event is <b>not</b> included
        /// in the <see cref="IDOMObject.Changed"/> and <see cref="DocumentTreeChanged"/> output.
        /// <p/>
        /// Defaults to the string <c>"Untitled"</c>.
        /// </remarks>
        string Filepath { get; set; }

        /// <summary>
        /// Occurs when <see cref="Filepath"/> changes.
        /// </summary>
        event EventHandler FilepathChanged;

        /// <summary>
        /// Returns a value indicating whether the <see cref="IDocument"/> is a member of the LDraw Parts Library.
        /// </summary>
        /// <remarks>
        /// An <see cref="IDocument"/> is a member of the LDraw Parts Library if:
        /// <p/>
        /// <list type="bullet">
        ///     <item><term>It is not a <see cref="PageType.Model"/>; <i>and</i></term></item>
        ///     <item>
        ///         <term>
        ///             The <see cref="Filepath">file</see> it represents is present in the
        ///             <see cref="Digitalis.LDTools.DOM.Configuration.FullSearchPath"/>
        ///         </term>
        ///     </item>
        /// </list>
        /// </remarks>
        bool IsLibraryPart { get; }

        /// <summary>
        /// Gets the current status of the <see cref="Filepath">file</see> the <see cref="IDocument"/> represents.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the file is not <see cref="IsLibraryPart">a member of the LDraw Parts Library</see> this returns
        /// <see cref="DocumentStatus.Private"/>; otherwise it returns a value indicating the current status of the file within
        /// the library, if known.
        /// </remarks>
        DocumentStatus Status { get; }

        /// <summary>
        /// Gets the <see cref="PageType"/> that best describes the <see cref="IDocument"/>
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocument"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="System.InvalidOperationException">The <see cref="IDocument"/> does not contain any
        ///     <see cref="IPage"/>s.</exception>
        /// <remarks>
        /// The type is derived from the types of the <see cref="IDocument"/>'s <see cref="IPage"/>s. If the
        /// <see cref="IDocument"/> contains a single page then the <see cref="IPage.PageType"/> of that page is returned.
        /// Otherwise, an <see cref="IDocument"/> is regarded as being a <see cref="PageType.Model"/> if at least one of its
        /// pages is; otherwise it is a <see cref="PageType.Part"/>. As such, this value should be regarded as a hint rather
        /// than absolute fact.
        /// </remarks>
        PageType DocumentType { get; }

        #endregion Properties
    }
}
