#region License

//
// LDDocument.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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

namespace Digitalis.LDTools.DOM
{
    #region Usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;
    using System.Runtime.Serialization;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>LDDocument</b> provides both single and <see href="http://www.ldraw.org/article/47.html">Multi-Part Document</see> functionality.
    /// An <b>LDDocument</b> consists of zero or more <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>s, and may be created either empty or by loading from
    /// an LDraw file.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class LDDocument : DOMObject, IDocument
    {
        #region Inner types

        private class ActionBegin : IAction
        {
            private LDDocument _doc;

            public ActionBegin(LDDocument doc)
            {
                _doc = doc;
            }

            public void Apply()
            {
                if (null != _doc.UpdateBegun)
                    _doc.UpdateBegun(this, EventArgs.Empty);
            }

            public void Revert()
            {
                if (null != _doc.UpdateEnded)
                    _doc.UpdateEnded(this, EventArgs.Empty);
            }
        }

        private class ActionEnd : IAction
        {
            private LDDocument _doc;

            public ActionEnd(LDDocument doc)
            {
                _doc = doc;
            }

            public void Apply()
            {
                if (null != _doc.UpdateEnded)
                    _doc.UpdateEnded(this, EventArgs.Empty);
            }

            public void Revert()
            {
                if (null != _doc.UpdateBegun)
                    _doc.UpdateBegun(this, EventArgs.Empty);
            }
        }

        #endregion Inner types

        #region Internals

        // for use by LDReference's cacheing mechanism
        [NonSerialized] internal long RefCount;
        [NonSerialized] internal string CacheKey;

        // characters not permitted for the 'Filepath' property
        private static readonly char[] InvalidNameChars = Path.GetInvalidPathChars();

        // matches 'x-series' library names
        private static readonly Regex XSeriesName = new Regex(@"^(s\\)?x[0-9]+.*\.dat$", RegexOptions.IgnoreCase);

        #endregion Internals

        #region Cloning and Serialization

        private List<IPage> _serializedContents;

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            _serializedContents = new List<IPage>(this);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext sc)
        {
            Initialize();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext sc)
        {
            foreach (IPage page in _serializedContents)
            {
                AddUnchecked(page);
            }

            _serializedContents.Clear();
            _serializedContents = null;
        }

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IDocument doc = (IDocument)obj;

            doc.Filepath = Filepath;

            foreach (IPage page in this)
            {
                doc.Add((IPage)page.Clone());
            }

            base.InitializeObject(obj);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (0 == Count)
                throw new InvalidOperationException("Document does not contain any pages");

            if (1 == Count || CodeStandards.PartsLibrary == codeFormat)
                return this[0].ToCode(sb, codeFormat, overrideColour, ref transform, winding);

            bool first = true;

            foreach (IPage page in this)
            {
                // put an empty line between pages to make the file easier to read
                if (!first)
                    sb.Append(LineTerminator);

                first = false;

                sb.AppendFormat("0 FILE {0}{1}", page.TargetName, LineTerminator);
                sb = page.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
            }

            return sb;
        }

        #endregion Code-generation

        #region Collection-management

        [NonSerialized]
        private UndoableList<IPage> _pages;

        [NonSerialized]
        private bool _loading;

        private void Initialize()
        {
            _pages              = new UndoableList<IPage>();
            _documentTreeEvents = new List<ObjectChangedEventArgs>();

            _pages.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
            {
                if (!_loading)
                {
                    if (IsFrozen)
                        throw new ObjectFrozenException();

                    foreach (IPage page in e.Items)
                    {
                        if (null == page)
                            throw new ArgumentNullException();

                        if (page.IsFrozen)
                            throw new ObjectFrozenException("page is frozen");

                        InsertCheckResult result = CanInsert(page, InsertCheckFlags.None);

                        if (InsertCheckResult.CanInsert != result)
                            throw new InvalidOperationException("Cannot insert this page: " + result);
                    }
                }

                foreach (IPage page in e.Items)
                {
                    page.Document = this;
                    page.Changed += OnPageChanged;
                }

                if (!_loading)
                {
                    if (null != ItemsAdded)
                        ItemsAdded(this, e);

                    FireDocumentTreeEvent(this, "ItemsAdded", e);
                }
            };

            _pages.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IPage page in e.Items)
                {
                    page.Document = null;
                    page.Changed -= OnPageChanged;
                }

                if (null != ItemsRemoved)
                    ItemsRemoved(this, e);

                FireDocumentTreeEvent(this, "ItemsRemoved", e);
            };

            _pages.ItemsReplaced += delegate(object sender, UndoableListReplacedEventArgs<IPage> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IPage page in e.ItemsAdded.Items)
                {
                    if (null == page)
                        throw new ArgumentNullException();

                    if (page.IsFrozen)
                        throw new ObjectFrozenException("page is frozen");

                    InsertCheckResult result = CanInsert(page, InsertCheckFlags.None);

                    if (InsertCheckResult.CanInsert != result)
                        throw new InvalidOperationException("Cannot insert this page: " + result);
                }

                foreach (IPage page in e.ItemsRemoved.Items)
                {
                    page.Document = null;
                    page.Changed -= OnPageChanged;
                }

                foreach (IPage page in e.ItemsAdded.Items)
                {
                    page.Document = this;
                    page.Changed += OnPageChanged;
                }

                if (null != ItemsReplaced)
                    ItemsReplaced(this, e);

                FireDocumentTreeEvent(this, "ItemsReplaced", e);
            };

            _pages.ListCleared += delegate(object sender, UndoableListChangedEventArgs<IPage> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IPage page in e.Items)
                {
                    page.Document = null;
                    page.Changed -= OnPageChanged;
                }

                if (null != CollectionCleared)
                    CollectionCleared(this, e);

                FireDocumentTreeEvent(this, "CollectionCleared", e);
            };
        }

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IPage> ItemsAdded;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IPage> ItemsRemoved;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListReplacedEventHandler<IPage> ItemsReplaced;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IPage> CollectionCleared;

        /// <inheritdoc />
        public InsertCheckResult CanInsert(IPage page, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return this.CanReplacePage(page, null, flags);
        }

        /// <inheritdoc />
        public InsertCheckResult CanReplace(IPage pageToInsert, IPage pageToReplace, InsertCheckFlags flags)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return this.CanReplacePage(pageToInsert, pageToReplace, flags);
        }

        /// <inheritdoc />
        public IPage this[string targetName] { get { return (from n in this where n.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase) select n).Take(1).FirstOrDefault(); } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return IsFrozen; } }

        /// <inheritdoc />
        public int Count { get { return _pages.Count; } }

        /// <inheritdoc />
        public int IndexOf(IPage page)
        {
            return _pages.IndexOf(page);
        }

        /// <inheritdoc />
        public bool Contains(IPage page)
        {
            return _pages.Contains(page);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">The operation is <c>set</c> and the supplied <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>
        ///     <see cref="M:Digitalis.LDTools.DOM.API.IDocument.CanInsertPage">cannot be inserted</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The operation is <c>set</c> and the <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public IPage this[int index] { get { return _pages[index]; } set { _pages[index] = value; } }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException"><paramref name="page"/>
        ///     <see cref="M:Digitalis.LDTools.DOM.API.IDocument.CanInsertPage">cannot be inserted</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public void Add(IPage page)
        {
            Insert(Count, page);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException"><paramref name="page"/>
        ///     <see cref="M:Digitalis.LDTools.DOM.API.IDocument.CanInsertPage">cannot be inserted</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public void Insert(int index, IPage page)
        {
            _pages.Insert(index, page);
        }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public bool Remove(IPage page)
        {
            return _pages.Remove(page);
        }

        /// <inheritdoc />
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>.</exception>
        public void Clear()
        {
            _pages.Clear();
        }

        /// <inheritdoc />
        public void CopyTo(IPage[] array, int arrayIndex)
        {
            _pages.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<IPage> GetEnumerator()
        {
            return _pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void AddUnchecked(IPage page)
        {
            _loading = true;
            Add(page);
            _loading = false;
        }

        #endregion Collection-management

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDDocument"/> class with default values.
        /// </summary>
        public LDDocument()
        {
            Initialize();
        }

        // to allow the basic constructor to function
        private static bool _dummy;

        /// <summary>
        /// Initializes a new instance of the <see cref="LDDocument"/> class from an LDraw file.
        /// </summary>
        /// <param name="filePath">The path of the LDraw file to create the <see cref="LDDocument"/> from.</param>
        /// <param name="flags">Flags to control the behaviour of the parser.</param>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="T:System.IO.IOException">A problem occurred reading the file.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> was <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="filePath"/> was whitespace or did not end with one of <c>.ldr</c>, <c>.dat</c> or <c>.mpd</c>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.DuplicatePageException">The stream contained two <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>s with identical
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>s.</exception>
        /// <exception cref="T:System.FormatException">The stream was not recognised as LDraw data.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.CircularReferenceException">The stream contained a circular reference.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.SyntaxException">The stream contained a syntax-error.</exception>
        /// <remarks>
        /// <para>
        /// Equivalent to calling <see cref="LDDocument(TextReader, string, ParserProgressCallback, ParseFlags, out bool)"/> with a <see cref="T:System.IO.TextReader"/>
        /// constructed from <paramref name="filePath"/> and <i>callback</i> set to <c>null</c>.
        /// </para>
        /// </remarks>
        public LDDocument(string filePath, ParseFlags flags)
            : this(filePath, flags, out _dummy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDDocument"/> class from an LDraw file.
        /// </summary>
        /// <param name="filePath">The path of the LDraw file to create the <see cref="LDDocument"/> from.</param>
        /// <param name="flags">Flags to control the behaviour of the parser.</param>
        /// <param name="documentModified">Returns a value indicating whether the document was modified during loading.</param>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="T:System.IO.IOException">A problem occurred reading the file.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> was <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="filePath"/> was whitespace or did not end with one of <c>.ldr</c>, <c>.dat</c> or <c>.mpd</c>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.DuplicatePageException">The stream contained two <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>s with identical
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>s.</exception>
        /// <exception cref="T:System.FormatException">The stream was not recognised as LDraw data.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.CircularReferenceException">The stream contained a circular reference.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.SyntaxException">The stream contained a syntax-error.</exception>
        /// <remarks>
        /// <para>
        /// Equivalent to calling <see cref="LDDocument(TextReader, string, ParserProgressCallback, ParseFlags, out bool)"/> with a <see cref="T:System.IO.TextReader"/>
        /// constructed from <paramref name="filePath"/> and <i>callback</i> set to <c>null</c>.
        /// </para>
        /// </remarks>
        public LDDocument(string filePath, ParseFlags flags, out bool documentModified)
            : this(filePath, null, flags, out documentModified)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDDocument"/> class from an LDraw file.
        /// </summary>
        /// <param name="filePath">The path of the LDraw file to create the <see cref="LDDocument"/> from.</param>
        /// <param name="callback">A delegate to be notified of the progress of the load, or <c>null</c> if no notifications are required.</param>
        /// <param name="flags">Flags to control the behaviour of the parser.</param>
        /// <param name="documentModified">Returns a value indicating whether the document was modified during loading.</param>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="T:System.IO.IOException">A problem occurred reading the file.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> was <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="filePath"/> was whitespace or did not end with one of <c>.ldr</c>, <c>.dat</c> or <c>.mpd</c>.</exception>
        /// <exception cref="T:System.OperationCanceledException">The delegate cancelled the load.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.DuplicatePageException">The stream contained two <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>s with identical
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>s.</exception>
        /// <exception cref="T:System.FormatException">The stream was not recognised as LDraw data.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.CircularReferenceException">The stream contained a circular reference.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.SyntaxException">The stream contained a syntax-error.</exception>
        /// <remarks>
        /// <para>
        /// Equivalent to calling <see cref="LDDocument(TextReader, string, ParserProgressCallback, ParseFlags, out bool)"/> with a <see cref="T:System.IO.TextReader"/>
        /// constructed from <paramref name="filePath"/>.
        /// </para>
        /// </remarks>
        public LDDocument(string filePath, ParserProgressCallback callback, ParseFlags flags, out bool documentModified)
            : this()
        {
            using (TextReader stream = File.OpenText(filePath))
            {
                Load(stream, filePath, callback, flags, out documentModified);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDDocument"/> class from a stream.
        /// </summary>
        /// <param name="stream">The stream to read data from.</param>
        /// <param name="filePath">The path of the LDraw file that <see cref="LDDocument"/> will represent if <see cref="Save()">saved</see>. May not be <c>null</c> or empty.</param>
        /// <param name="callback">A delegate to be notified of the progress of the load, or <c>null</c> if no notifications are required.</param>
        /// <param name="flags">Flags to control the behaviour of the parser.</param>
        /// <param name="documentModified">Returns a value indicating whether the document was modified during loading.</param>
        /// <exception cref="T:System.IO.IOException">A problem occurred reading the stream.</exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="filePath"/> was <c>null</c>.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="filePath"/> was whitespace or did not end with one of <c>.ldr</c>, <c>.dat</c> or <c>.mpd</c>.</exception>
        /// <exception cref="T:System.OperationCanceledException">The delegate cancelled the load.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.DuplicatePageException">The stream contained two <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>s with identical
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>s.</exception>
        /// <exception cref="T:System.FormatException">The stream was not recognised as LDraw data.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.CircularReferenceException">The stream contained a circular reference.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.SyntaxException">The stream contained a syntax-error.</exception>
        /// <remarks>
        /// <para>
        /// The data loaded from <paramref name="stream"/> will be sanitised:
        /// <list type="bullet">
        ///   <item><term>
        ///     trailing <see cref="P:Digitalis.LDTools.DOM.API.IComment.IsEmpty">empty comments</see> are removed from each <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>
        ///   </term></item>
        ///   <item><term>
        ///     blocks of <see cref="P:Digitalis.LDTools.DOM.API.IComment.IsEmpty">empty comments</see> are collapsed down to a single empty comment
        ///   </term></item>
        ///   <item><term>
        ///     invalid duplicated lines in the <see href="http://www.ldraw.org/article/398.html">page header</see> are converted into comments
        ///   </term></item>
        ///   <item><term>
        ///     if the <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> or <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of a page conflicts with the
        ///     page's <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>, it will be fixed
        ///   </term></item>
        /// </list>
        /// </para>
        /// <para>
        /// If <paramref name="callback"/> is not <c>null</c> then it will be called periodically as the stream is parsed. Two types of callback message are sent:
        /// <list type="bullet">
        ///   <item><term>
        ///     if <i>progress</i> is <c>-1</c> then <i>name</i> is the <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> of the first
        ///     <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> in the document, or <see cref="P:Digitalis.LDTools.DOM.API.IDocument.Filepath"/> if the page has no title
        ///   </term></item>
        ///   <item><term>
        ///     if <i>progress</i> is <c>0..100</c> then <i>name</i> is a description of the <see cref="T:Digitalis.LDTools.DOM.API.IPageElement"/> currently being loaded
        ///   </term></item>
        /// </list>
        /// </para>
        /// <para>
        /// The parser may need to modify the document during loading, either in order to follow the rules specified by <paramref name="flags"/>
        /// or to correct errors in the document's structure. If this happens, <paramref name="documentModified"/> will be set to <c>true</c>
        /// before returning. The underlying file referenced by <paramref name="filePath"/> will be unchanged.
        /// </para>
        /// <para>
        /// The following occurrences will cause a modification:
        /// <list type="bullet">
        ///   <item><term>
        ///     <paramref name="flags"/> specifies <see cref="Digitalis.LDTools.DOM.ParseFlags.FollowRedirects"/> and an <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>
        ///     is encountered which refers to a <i>~Moved to</i> file; the reference's <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> will be changed to the
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> of the new file
        ///   </term></item>
        ///   <item><term>
        ///     <paramref name="flags"/> specifies <see cref="Digitalis.LDTools.DOM.ParseFlags.FollowAliases"/> and an <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>
        ///     is encountered which refers to an <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/> or <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Alias"/>;
        ///     the reference's <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> will be changed to the <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/>
        ///     of the aliased file
        ///   </term></item>
        ///   <item><term>
        ///     an <see cref="T:Digitalis.LDTools.DOM.API.IReference"/> is encountered whose <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> begins with an
        ///     <i>x</i> but for which the referenced file has been renamed from <i>xNNNNN</i> to <i>NNNNN</i>, where <i>NNNNN</i> is the 'base' name of the part the file
        ///     represents; the reference's <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> will be changed to the new name
        ///   </term></item>
        ///   <item><term>
        ///     an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> is loaded whose <i>Name:</i> and <i>!LDRAW_ORG</i> fields are invalid or inconsistent; the page's
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> will be changed to a correct value, and any <see cref="T:Digitalis.LDTools.DOM.API.IReference"/>s
        ///     in the document will be updated with this value
        ///   </term></item>
        /// </list>
        /// </para>
        /// </remarks>
        public LDDocument(TextReader stream, string filePath, ParserProgressCallback callback, ParseFlags flags, out bool documentModified)
            : this()
        {
            Load(stream, filePath, callback, flags, out documentModified);
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Filepath = null;

                foreach (IPage page in this)
                {
                    page.Dispose();
                }
            }
        }

        #endregion Disposal

        #region Document-tree

        [NonSerialized]
        private int _updateInProgress = 0;

        [NonSerialized]
        private List<ObjectChangedEventArgs> _documentTreeEvents;

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler UpdateBegun;

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler UpdateInProgress;

        /// <inheritdoc />
        [field: NonSerialized]
        public event EventHandler UpdateEnded;

        /// <inheritdoc />
        [field: NonSerialized]
        public event DocumentTreeChangedEventHandler DocumentTreeChanged;

        /// <inheritdoc />
        public void BeginUpdate()
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (0 == _updateInProgress++)
                UndoStack.AddAction(new ActionBegin(this));
        }

        /// <inheritdoc />
        public void Update()
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (0 != _updateInProgress && null != UpdateInProgress)
                UpdateInProgress(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void EndUpdate()
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (0 == _updateInProgress)
                throw new InvalidOperationException("EndUpdate() called without BeginUpdate()");

            if (0 == --_updateInProgress)
            {
                UndoStack.AddAction(new ActionEnd(this));

                if (null != DocumentTreeChanged && 0 != _documentTreeEvents.Count)
                    DocumentTreeChanged(this, new DocumentTreeChangedEventArgs(_documentTreeEvents.Count, _documentTreeEvents));

                _documentTreeEvents.Clear();
            }
        }

        private void OnPageChanged(IDOMObject sender, ObjectChangedEventArgs e)
        {
            FireDocumentTreeEvent(sender, e.Operation, e);
        }

        private void FireDocumentTreeEvent(IDOMObject element, string operation, EventArgs parameters)
        {
            _documentTreeEvents.Add(new ObjectChangedEventArgs(element, operation, parameters));

            if (0 == _updateInProgress)
            {
                if (null != DocumentTreeChanged)
                    DocumentTreeChanged(this, new DocumentTreeChangedEventArgs(1, _documentTreeEvents));

                _documentTreeEvents.Clear();
            }

            OnChanged(element, operation, parameters);
        }

        #endregion Document-tree

        #region File-change handling

        private DateTime _lastWriteTime = DateTime.Now;

        /// <summary>
        /// Occurs when the file the <see cref="IDocument"/> represents is changed by an external source.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event will not occur if the change was caused by a call to <see cref="Save()"/>.
        /// </para>
        /// </remarks>
        [field: NonSerialized]
        public event FileSystemEventHandler FileChanged;

        /// <summary>
        /// Occurs when the file the <see cref="IDocument"/> represents is deleted.
        /// </summary>
        [field: NonSerialized]
        public event FileSystemEventHandler FileDeleted;

        /// <summary>
        /// Occurs when the file the <see cref="IDocument"/> represents is renamed by an external source.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event will not occur if <see cref="Filepath"/> already matches the name to which the file has been moved.
        /// </para>
        /// </remarks>
        [field: NonSerialized]
        public event RenamedEventHandler FileRenamed;

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (null != FileChanged)
            {
                // we don't send an event if the change was performed by us...
                // do it this way because file-writes and the OnFileChanged event are asynchronous
                if (!String.IsNullOrWhiteSpace(Filepath) && _lastWriteTime < File.GetLastWriteTime(Filepath))
                    FileChanged(this, e);

                FireDocumentTreeEvent(this, "FileChanged", e);
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Filepath) && null != FileDeleted)
                FileDeleted(this, e);

            FireDocumentTreeEvent(this, "FileDeleted", e);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Filepath) && null != FileRenamed && !Filepath.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
                FileRenamed(this, e);

            FireDocumentTreeEvent(this, "FileRenamed", e);
        }

        #endregion File-change handling

        #region Import and Export

        /// <inheritdoc />
        public void Import(IDocument import)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (IsFrozen)
                throw new ObjectFrozenException();

            foreach (IPage page in import)
            {
                if (null != this[page.TargetName])
                    throw new DuplicateNameException(page.TargetName);
            }

            while (import.Count > 0)
            {
                IPage page = import.First();
                import.Remove(page);
                Add(page);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="FileChanged"/> events will not be raised by this method.
        /// </para>
        /// </remarks>
        public void Save()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (0 == Count)
                throw new InvalidOperationException("Document does not contain any pages");

            if (String.IsNullOrWhiteSpace(Filepath) || !Path.IsPathRooted(Filepath) || (File.Exists(Filepath) && 0 != (FileAttributes.ReadOnly & File.GetAttributes(Filepath))))
                throw new FileNotFoundException();

            using (TextWriter stream = File.CreateText(Filepath))
            {
                Save(stream);
            }

            // save this off to ensure we don't fire a FileChanged event for our own activity
            _lastWriteTime = File.GetLastWriteTime(Filepath);

            // Force a change-event, as files on networked shares may not do so automatically.
            // There does not appear to be any issue with two events turning up if the file was on a local drive, but if this does arise then:
            //      DriveInfo di = new DriveInfo(FullName[0]);       -- note: check that FullName[0] is in [A-Za-z]
            //      if (DriveType.Network == di.Type || Filepath.StartsWith(@"\\"))
            TargetWatcher.OnChanged(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetDirectoryName(Filepath), Path.GetFileName(Filepath)));
        }

        /// <inheritdoc />
        public void Save(TextWriter textWriter)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            StringBuilder sb = ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal);
            textWriter.Write(sb.ToString());
        }

        /// <inheritdoc />
        public void Publish(DocumentWriterCallback writer)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (0 == Count)
                throw new InvalidOperationException("Document does not contain any pages");

            if (PageType.Model == DocumentType)
            {
                string path   = null;
                int numPages  = 0;
                int firstPage = 0;
                bool first    = true;

                foreach (IPage page in this)
                {
                    if (!page.InlineOnPublish)
                    {
                        numPages++;

                        if (null == path)
                        {
                            path      = page.Name;
                            firstPage = IndexOf(page);
                        }
                    }
                }

                if (0 == numPages)
                    throw new InvalidOperationException("Document does not contain any non-inlined pages");

                path += ((numPages > 1) ? ".mpd" : ".ldr");

                using (TextWriter textWriter = writer(path))
                {
                    if (1 == numPages)
                    {
                        textWriter.Write(this[firstPage].ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                    }
                    else
                    {
                        foreach (IPage page in this)
                        {
                            if (!page.InlineOnPublish)
                            {
                                // put an empty line between pages to make the file easier to read
                                if (!first)
                                    textWriter.Write(LineTerminator);

                                first = false;
                                textWriter.Write("0 FILE ");
                                textWriter.Write(page.TargetName);
                                textWriter.Write(LineTerminator);
                                textWriter.Write(page.ToCode(new StringBuilder(), CodeStandards.OfficialModelRepository, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                            }
                        }
                    }
                }
            }
            else
            {
                int i = 0;

                foreach (IPage page in this)
                {
                    if (!page.InlineOnPublish)
                    {
                        i++;

                        using (TextWriter textWriter = writer(page.TargetName))
                        {
                            textWriter.Write(page.ToCode(new StringBuilder(), CodeStandards.PartsLibrary, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                        }
                    }
                }

                if (0 == i)
                    throw new InvalidOperationException("Document does not contain any non-inlined pages");
            }
        }

        /// <inheritdoc />
        public void Publish(string folderPath)
        {
            Publish(delegate(string targetName) { return File.CreateText(Path.Combine(folderPath, targetName)); });
        }

        /// <inheritdoc />
        public void Export(DocumentWriterCallback writer)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (0 == Count)
                throw new InvalidOperationException("Document does not contain any pages");

            foreach (IPage page in this)
            {
                using (TextWriter textWriter = writer(page.TargetName))
                {
                    textWriter.Write(page.ToCode(new StringBuilder(), CodeStandards.Full, Palette.MainColour, ref Matrix4d.Identity, WindingDirection.Normal));
                }
            }
        }

        /// <inheritdoc />
        public void Export(string folderPath)
        {
            Publish(delegate(string targetName) { return File.CreateText(Path.Combine(folderPath, targetName)); });
        }

        #endregion Import and Export

        #region Parser

        private void Load(TextReader stream, string filePath, ParserProgressCallback callback, ParseFlags flags, out bool documentModified)
        {
            if (null == filePath)
                throw new ArgumentNullException("filePath was null");

            if (String.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath was empty");

            string extension = Path.GetExtension(filePath).ToLower();

            if (".dat" != extension && ".ldr" != extension && ".mpd" != extension)
                throw new ArgumentException("filePath must end with .dat, .ldr or .mpd");

            Filepath = filePath;

            // set up a dummy callback so we don't have to check for the presence of the delegate each time around the loop
            if (null == callback)
                callback = delegate(string name, int progress) { return true; };

            if (!callback(filePath, -1))
                throw new OperationCanceledException();

            Dictionary<string, IPage> renamedPages = new Dictionary<string, IPage>();
            FileInfo                  fileInfo     = new FileInfo(filePath);
            LDPage                    page;
            string                    pageLine     = null;
            string                    line         = stream.ReadLine();
            string                    pagePath     = fileInfo.Name;
            string[]                  fields;
            long                      fileSize     = (File.Exists(filePath)) ? fileInfo.Length : 1;
            long                      bytesRead    = 0;
            uint                      pageLineNum  = 0;
            uint                      lineNum      = 1;
            bool                      skipLines    = false;

            while (null != line)
            {
                line = line.Trim();

                if (String.IsNullOrWhiteSpace(line))
                {
                    skipLines = true;
                }
                else
                {
                    fields = line.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

                    if (fields.Length > 2 && "0" == fields[0] && "FILE" == fields[1])
                    {
                        pagePath    = line.Substring(line.IndexOf(fields[2]));
                        bytesRead  += line.Length + LineTerminator.Length;
                        pageLine    = line;
                        pageLineNum = lineNum;
                        skipLines   = false;
                        line        = stream.ReadLine();
                        lineNum++;

                        if (null != line)
                            line = line.Trim();
                    }
                    else if (2 == fields.Length && "0" == fields[0] && "NOFILE" == fields[1])
                    {
                        skipLines = true;
                    }
                }

                if (skipLines)
                {
                    bytesRead += line.Length + LineTerminator.Length;
                    line       = stream.ReadLine();
                    lineNum++;
                }
                else
                {
                    page = new LDPage();
                    page.Load(stream, filePath, pagePath, ref lineNum, ref line, fileSize, ref bytesRead, callback);

                    if (null != this[page.TargetName])
                        throw new DuplicatePageException("A page with the name '" + page.TargetName + "' already exists in the document", null, filePath, pageLine, pageLineNum);

                    if (!pagePath.Equals(page.TargetName, StringComparison.OrdinalIgnoreCase))
                        renamedPages.Add(pagePath.ToLower(), page);

                    // add directly in order to bypass the normal safety-checks; we'll be validating the entire document later so they're redundant right now
                    AddUnchecked(page);
                }
            }

            documentModified = Validate(renamedPages, fileInfo, callback, flags);
        }

        private bool Validate(Dictionary<string, IPage> renamedPages, FileInfo fileInfo, ParserProgressCallback callback, ParseFlags flags)
        {
            if (0 == Count)
                throw new FormatException("No LDraw data found");

            LDReference        reference;
            IElementCollection collection;
            string             targetName;
            int                progress;
            int                lastProgress     = 50;
            int                count            = 0;
            int                i                = 0;
            bool               followRedirects  = (0 != (ParseFlags.FollowRedirects & flags));
            bool               followAliases    = (0 != (ParseFlags.FollowAliases & flags));
            bool               documentModified = false;

            if (!callback(Resources.Verifying, lastProgress))
                throw new OperationCanceledException();

            foreach (IPage p in this)
            {
                count += p.Elements.Count;
            }

            foreach (IPage page in this)
            {
                foreach (IElement element in page.Elements)
                {
                    reference = element as LDReference;
                    progress  = (50 * ++i / count) + 50;

                    if (progress != lastProgress)
                    {
                        if (progress > 100)
                            progress = 100;

                        lastProgress = progress;
                        targetName   = (null != reference) ? reference.TargetName : element.TypeName;

                        if (!callback(targetName, progress))
                            throw new OperationCanceledException();
                    }

                    if (null != reference)
                    {
                        if (CheckReference(renamedPages, fileInfo, page, reference, followRedirects, followAliases))
                            documentModified = true;
                    }
                    else
                    {
                        collection = element as IElementCollection;

                        if (null != collection)
                        {
                            if (CheckElementCollection(renamedPages, fileInfo, page, page.Elements, followRedirects, followAliases))
                                documentModified = true;
                        }
                    }
                }
            }

            return documentModified;
        }

        private bool CheckElementCollection(Dictionary<string, IPage> renamedPages, FileInfo fileInfo, IPage page, IList<IElement> collection, bool followRedirects, bool followAliases)
        {
            IElementCollection childCollection;
            LDReference        r;
            bool               documentModified = false;

            foreach (IElement element in collection)
            {
                childCollection = element as IElementCollection;

                if (null != childCollection)
                {
                    if (CheckElementCollection(renamedPages, fileInfo, page, childCollection, followRedirects, followAliases))
                        documentModified = true;
                }
                else
                {
                    r = element as LDReference;

                    if (null != r)
                    {
                        if (CheckReference(renamedPages, fileInfo, page, r, followRedirects, followAliases))
                            documentModified = true;
                    }
                }
            }

            return documentModified;
        }

        private bool CheckReference(Dictionary<string, IPage> renamedPages, FileInfo fileInfo, IPage page, LDReference reference, bool followRedirects, bool followAliases)
        {
            IPage  target;
            string targetName       = reference.TargetName;
            bool   documentModified = false;

            // fix illegally-named pages in the document; in this case and this case only we may modify a locked reference
            if (renamedPages.TryGetValue(targetName.ToLower(), out target))
            {
                bool locked          = reference.IsLocked;
                reference.IsLocked   = false;
                reference.TargetName = target.TargetName;
                reference.IsLocked   = locked;
            }
            else
            {
                target = reference.Target;

                switch (reference.TargetStatus)
                {
                    case TargetStatus.Missing:
                        if (!reference.IsLocked)
                        {
                            // some unofficial parts have been renamed from 'xNNNN.dat' to 'NNNN.dat'; we always resolve these
                            if (null == target && XSeriesName.IsMatch(targetName))
                            {
                                reference.TargetName = targetName.Substring(1);
                                target               = reference.Target;

                                if (null != target)
                                    documentModified = true;
                                else
                                    reference.TargetName = targetName;
                            }
                        }
                        break;

                    case TargetStatus.Unloadable:
                    case TargetStatus.Unresolved:
                        reference.LDrawCode = null;
                        return false;

                    case TargetStatus.CircularReference:
                        throw new CircularReferenceException(String.Empty, null, fileInfo.FullName, reference.LDrawCode, reference.CodeLine);

                    case TargetStatus.Resolved:
                        if (!reference.IsLocked)
                        {
                            // follow redirects and aliases
                            if ((followRedirects && target.Title.StartsWith("~Moved to", StringComparison.OrdinalIgnoreCase)) ||
                                (followAliases && (PageType.Part_Alias == target.PageType || PageType.Shortcut_Alias == target.PageType)))
                            {
                                IEnumerable<IReference> redirects = from n in target.Elements where DOMObjectType.Reference == n.ObjectType select n as IReference;

                                if (1 == redirects.Count())
                                {
                                    IReference redirect = redirects.First();

                                    reference.TargetName = redirect.TargetName;
                                    reference.Matrix    *= redirect.Matrix;

                                    if (Palette.MainColour == reference.ColourValue)
                                        reference.ColourValue = redirect.ColourValue;

                                    target           = reference.Target;
                                    documentModified = true;
                                }
                            }
                        }
                        break;

                    default:
                        throw new SyntaxException(String.Empty, null, fileInfo.FullName, reference.LDrawCode, reference.CodeLine);
                }
            }

            if (!reference.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                documentModified = true;

            reference.LDrawCode = null;
            return documentModified;
        }

        #endregion Parser

        #region Properties

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// If the current value of this property is an absolute filepath, the <see cref="LDDocument"/> will monitor the file for changes made to it by other applications,
        /// and raise the <see cref="FileChanged"/>, <see cref="FileDeleted"/> and <see cref="FileRenamed"/> events as appropriate.
        /// </para>
        /// </remarks>
        public string Filepath
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _filepath;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (_filepath == value)
                    return;

                if (null != value && -1 != value.IndexOfAny(InvalidNameChars))
                    throw new ArgumentException("Name may not contain any of the characters " + new String(InvalidNameChars));

                if (!String.IsNullOrWhiteSpace(_filepath) && Path.IsPathRooted(_filepath))
                    TargetWatcher.RemoveWatch(_filepath, OnFileChanged, OnFileDeleted, OnFileRenamed);

                _filepath = value;

                if (!String.IsNullOrWhiteSpace(_filepath) && Path.IsPathRooted(_filepath) && File.Exists(_filepath))
                    TargetWatcher.WatchFile(_filepath, OnFileChanged, OnFileDeleted, OnFileRenamed);

                if (null != FilepathChanged)
                    FilepathChanged(this, EventArgs.Empty);

                FireDocumentTreeEvent(this, "Filepath", EventArgs.Empty);
            }
        }
        private string _filepath = Resources.Untitled;

        /// <inheritdoc />
        public event EventHandler FilepathChanged;

        /// <inheritdoc />
        public bool IsLibraryPart
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (0 != Count && PageType.Model != DocumentType && !String.IsNullOrWhiteSpace(Filepath) && Path.IsPathRooted(Filepath))
                {
                    string folder = Path.GetDirectoryName(Filepath);

                    foreach (string path in Configuration.FullSearchPath)
                    {
                        if (folder.Equals(path, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }

                return false;
            }
        }

        /// <inheritdoc />
        public DocumentStatus Status
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (!IsLibraryPart)
                    return DocumentStatus.Private;

                IPage page = this[0];

                if (null != page.Update)
                    return DocumentStatus.Released;

                foreach (LDHistory history in page.History)
                {
                    if (history.Description.StartsWith("Official Update"))
                        return DocumentStatus.Rework;
                }

                // TODO: connect to the Parts Tracker and check the votes
                return DocumentStatus.Unreleased;
            }
        }

        /// <inheritdoc />
        public PageType DocumentType
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (0 == Count)
                    throw new InvalidOperationException("Document does not contain any pages");

                if (1 == Count)
                    return this[0].PageType;

                foreach (IPage p in this)
                {
                    if (PageType.Model == p.PageType)
                        return PageType.Model;
                }

                return PageType.Part;
            }
        }

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Document; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        #endregion Self-description
    }
}
