#region License

//
// LibraryManager.cs
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

namespace Digitalis.LDTools.Library
{
    #region Usings

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    /// <summary>
    /// Represents a method which may be notified of progress by the <see cref="M:Digitalis.LDTools.DOM.LibraryManager.Load"/> method.
    /// </summary>
    /// <param name="progress">The percentage completed.</param>
    /// <param name="status">A status message.</param>
    /// <param name="filename">The path of the file being loaded.</param>
    /// <returns><c>false</c> to cancel the load; <c>true</c> to continue it.</returns>
    /// <remarks>
    /// <para>
    /// During the load, <paramref name="status"/> will be set to the <see cref="P:Digitalis.LDTools.DOM.API.IPage..Title"/> of the part being loaded and <paramref name="filename"/>
    /// will be set to its filename. Once the load has completed the cache may need to be updated, in which case the status message will change to 'Cacheing...'
    /// and <paramref name="filename"/> will be set to <see cref="F:System.String.Empty"/>.
    /// </para>
    /// <para>
    /// The delegate may choose to cancel the load at any time by returning <c>false</c>.
    /// </para>
    /// </remarks>
    public delegate bool LoadProgressCallback(int progress, string status, string filename);

    /// <summary>
    /// Represents a method which is invoked when the <see cref="E:Digitalis.LDTools.DOM.LibraryManager.Changed"/> event occurs.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event-args instance containing the event data.</param>
    public delegate void LibraryChangedEventHandler(object sender, LibraryChangedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="E:Digitalis.LDTools.DOM.LibraryManager.Changed"/> event.
    /// </summary>
    public class LibraryChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets details of the items added to the library.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each member of the enumeration is the <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> of an item that has been added to the library. If no items were added,
        /// this property returns <c>null</c>.
        /// </para>
        /// </remarks>
        public IEnumerable<string> Added { get; private set; }

        /// <summary>
        /// Gets details of the items removed from the library.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each member of the enumeration is the <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> of an item that has been removed from the library. If no items were
        /// removed, this property returns <c>null</c>.
        /// </para>
        /// </remarks>
        public IEnumerable<string> Removed { get; private set; }

        /// <summary>
        /// Gets details of the items modified in the library.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each member of the enumeration is the <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> of an item that has been modified in the library. If no items were
        /// modified, this property returns <c>null</c>.
        /// </para>
        /// </remarks>
        public IEnumerable<string> Modified { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryChangedEventArgs"/> class with the specified values.
        /// </summary>
        /// <param name="added">The added items.</param>
        /// <param name="removed">The removed items.</param>
        /// <param name="modified">The modified items.</param>
        public LibraryChangedEventArgs(IEnumerable<string> added, IEnumerable<string> removed, IEnumerable<string> modified)
            : base()
        {
            Added    = added;
            Removed  = removed;
            Modified = modified;
        }

        #endregion Constructor
    }

    /// <summary>
    /// Represents the LDraw library installed on this computer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>LibraryManager</b> provides two functions: an event which reports when items are added to or removed from the installed library,
    /// or are modified or renamed; and a cache to speed up access to the items in the library.
    /// </para>
    /// <para>
    /// On startup, <b>LibraryManager</b> installs a <see cref="T:System.IO.FileSystemWatcher"/> to monitor for changes to any component of the
    /// <see cref="P:Digitalis.LDTools.DOM.API.Configuration.FullSearchPath"/>. It detects files and folders being added, removed, modified or renamed and raises the
    /// <see cref="Changed"/> event when this occurs.
    /// </para>
    /// <para>
    /// The cache indexes the contents of the installed library and returns a collection of <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s. The cache may be initalised by
    /// calling <see cref="Load"/>, and if successful is made available via <see cref="Cache"/>. When <see cref="Changed"/> occurs, the cache will
    /// update itself. This cache is also written automatically to global Isolated Storage to speed up indexing the next time it is loaded.
    /// </para>
    /// <para>
    /// If the cache is loaded, <see cref="T:Digitalis.LDTools.DOM.LDReference"/> will make use of it when resolving its <see cref="P:Digitalis.LDTools.DOM.API.IReference.Target"/>.
    /// The cache also provides the <see cref="InstalledVersion"/> of the LDraw library, and lists of the <see cref="Digitalis.LDTools.DOM.API.Category">categories</see> found in it.
    /// </para>
    /// <para>
    /// Finally, whilst the DOM itself does not use the <c>parts.lst</c> file, older LDraw programs may still rely on it. <b>LibraryManager</b> provides APIs to
    /// write this file from the cached data, and can optionally update it automatically when the cache updates.
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class LibraryManager : IEnumerable<IndexCard>
    {
        #region Internals

        // the cache itself, indexed by TargetName
        private Dictionary<string, IndexCard> _index = new Dictionary<string, IndexCard>();

        // path to the parts.lst file
        private static string PartsLst { get { return Path.Combine(Configuration.LDrawBase, "parts.lst"); } }

        // path to the file-cache
        private const string LibraryFile = "library.bin";

        // interlocking for thread-safety
        private static object Lock = new object();

        // recognised file-extensions
        private static readonly string[] FileExtensions = new string[] { ".ldr", ".mpd", ".dat" };

        // the current version of the cache object; used to determine whether the file-cache needs refreshing
        private int _version = Hash();

        private static FileSystemWatcher Watcher;
        private static SynchronizationContext SyncContext;

        static LibraryManager()
        {
            SyncContext = SynchronizationContext.Current;

            Configuration.LDrawBaseChanged += delegate(object sender, EventArgs e)
            {
                Unload();
                Watch();
            };

            Watch();
        }

        // calculate our version
        private static int Hash()
        {
            int hash = 0;
            int i    = 0;

            foreach (string name in Enum.GetNames(typeof(Category)))
            {
                hash ^= (name + i++).GetHashCode();
            }

            foreach (string name in Enum.GetNames(typeof(PageType)))
            {
                hash ^= (name + i++).GetHashCode();
            }

            return hash;
        }

        private void UpdateStatistics()
        {
            IEnumerable<LDUpdate> test = (from n in this where null != n.Update orderby ((LDUpdate)n.Update).Year descending, ((LDUpdate)n.Update).Release descending select (LDUpdate)n.Update);

            InstalledVersion = (from n in this where null != n.Update orderby ((LDUpdate)n.Update).Year descending, ((LDUpdate)n.Update).Release descending select (LDUpdate)n.Update).First();

            _partsCategories.AddRange((from n in this where n.IsPart select n.Category).Distinct());
            _subpartsCategories.AddRange((from n in this where n.IsSubpart select n.Category).Distinct());
            _primitivesCategories.AddRange((from n in this where n.IsPrimitive select n.Category).Distinct());
        }

        private static void Watch()
        {
            lock (Lock)
            {
                if (null != Watcher)
                {
                    Watcher.Dispose();
                    Watcher = null;
                }
            }

            Watch(Configuration.LDrawBase);
        }

        private static bool IsWatchedPath(string folder)
        {
            foreach (string path in Configuration.FullSearchPath)
            {
                if (path.Equals(folder, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string GetTargetName(string filePath)
        {
            filePath = filePath.ToLower();

            string folder = Path.GetDirectoryName(Path.GetDirectoryName(filePath));

            if ("s" == folder)
                return Path.Combine("s", Path.GetFileName(filePath));

            if ("48" == folder)
                return Path.Combine("48", Path.GetFileName(filePath));

            return Path.GetFileName(filePath);
        }

        private static void Watch(string folder)
        {
            if (!Directory.Exists(folder))
                return;

            FileSystemWatcher watcher     = new FileSystemWatcher(folder);
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter          = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Error += delegate(object sender, ErrorEventArgs e)
            {
                // if the watcher dies, create a new one to replace it
                Watch(folder);
            };

            watcher.Created += delegate(object sender, FileSystemEventArgs e)
            {
                if (!File.Exists(e.FullPath) || WatcherChangeTypes.Created != e.ChangeType)
                    return;

                IEnumerable<string> added   = null;
                IEnumerable<string> changed = null;

                if (IsWatchedPath(Path.GetDirectoryName(e.FullPath)))
                {
                    lock (Lock)
                    {
                        if (null != Cache)
                        {
                            try
                            {
                                IndexCard newCard = new IndexCard(new FileInfo(e.FullPath));
                                IndexCard oldCard = Cache[newCard.TargetName];

                                if (null != oldCard)
                                {
                                    // the existing card takes precedence
                                    if (oldCard.Rank < newCard.Rank)
                                        return;

                                    // the new card will replace it, so this is a change rather than an add
                                    Cache.Remove(oldCard);
                                    changed = new string[] { newCard.TargetName };
                                }
                                else
                                {
                                    added = new string[] { newCard.TargetName };
                                }

                                Cache.Add(newCard);
                            }
                            catch
                            {
                                // not a valid LDraw file, so ignore
                                return;
                            }
                        }
                        else if (!FileExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
                        {
                            // presumed not a valid LDraw file, so ignore
                            return;
                        }
                        else
                        {
                            added = new string[] { GetTargetName(e.FullPath) };
                        }
                    }

                    if (null != Changed)
                    {
                        LibraryChangedEventArgs eventArgs = new LibraryChangedEventArgs(added, null, changed);

                        if (null != SyncContext)
                            SyncContext.Post(delegate(object state) { Changed(null, eventArgs); }, null);
                        else
                            Changed(null, eventArgs);
                    }
                }
            };

            watcher.Deleted += delegate(object sender, FileSystemEventArgs e)
            {
                if (WatcherChangeTypes.Deleted != e.ChangeType)
                    return;

                if (IsWatchedPath(Path.GetDirectoryName(e.FullPath)))
                {
                    string[] changed = null;
                    string[] deleted = null;

                    lock (Lock)
                    {
                        if (null != Cache)
                        {
                            string targetName = GetTargetName(e.FullPath);
                            IndexCard card = Cache[targetName];

                            // the file wasn't a member of the cache, so we're not interested
                            if (null == card || !e.FullPath.Equals(card.Filepath, StringComparison.OrdinalIgnoreCase))
                                return;

                            Cache.Remove(card);

                            // now we need to see if there's another file available elsewhere in the path to replace the deleted one
                            card = LoadIndexCard(targetName);

                            // if so then we should report this as a change rather than a delete
                            if (null != card)
                                changed = new string[] { GetTargetName(card.Filepath) };
                            else
                                deleted = new string[] { targetName };
                        }
                        else
                        {
                            // presumed not a valid LDraw file, so ignore
                            if (!FileExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
                                return;

                            deleted = new string[] { GetTargetName(e.FullPath) };
                        }
                    }

                    if (null != Changed)
                    {
                        LibraryChangedEventArgs eventArgs = new LibraryChangedEventArgs(null, deleted, changed);

                        if (null != SyncContext)
                            SyncContext.Post(delegate(object state) { Changed(null, eventArgs); }, null);
                        else
                            Changed(null, eventArgs);
                    }
                }
            };

            watcher.Changed += delegate(object sender, FileSystemEventArgs e)
            {
                if (!File.Exists(e.FullPath) || WatcherChangeTypes.Changed != e.ChangeType)
                    return;

                if (IsWatchedPath(Path.GetDirectoryName(e.FullPath)))
                {
                    lock (Lock)
                    {
                        if (null != Cache)
                        {
                            try
                            {
                                IndexCard newCard = new IndexCard(new FileInfo(e.FullPath));
                                IndexCard oldCard = Cache[newCard.TargetName];

                                // the existing card takes precedence
                                if (null != oldCard && (oldCard.Rank < newCard.Rank || oldCard.Modified == newCard.Modified))
                                    return;

                                Cache.Add(newCard);
                            }
                            catch
                            {
                                // not a valid LDraw file, so ignore
                                return;
                            }
                        }
                        else if (!FileExtensions.Contains(Path.GetExtension(e.FullPath).ToLower()))
                        {
                            // presumed not a valid LDraw file, so ignore
                            return;
                        }
                    }

                    if (null != Changed)
                    {
                        LibraryChangedEventArgs eventArgs = new LibraryChangedEventArgs(null, null, new string[] { GetTargetName(e.FullPath) });

                        if (null != SyncContext)
                            SyncContext.Post(delegate(object state) { Changed(null, eventArgs); }, null);
                        else
                            Changed(null, eventArgs);
                    }
                }
            };

            watcher.Renamed += delegate(object sender, RenamedEventArgs e)
            {
                if (WatcherChangeTypes.Renamed != e.ChangeType)
                    return;

                IEnumerable<string> added   = null;
                IEnumerable<string> deleted = null;
                IEnumerable<string> changed = null;
                string newPath;
                string oldPath;

                if (File.Exists(e.FullPath))
                {
                    // file rename
                    newPath = Path.GetDirectoryName(e.FullPath);
                    oldPath = Path.GetDirectoryName(e.OldFullPath);

                    lock (Lock)
                    {
                        if (null != Cache)
                        {
                            string targetName;

                            if (IsWatchedPath(oldPath))
                            {
                                targetName = GetTargetName(e.OldFullPath);

                                IndexCard card = Cache[targetName];

                                if (null != card && e.OldFullPath.Equals(card.Filepath, StringComparison.OrdinalIgnoreCase))
                                {
                                    Cache.Remove(card);

                                    // now we need to see if there's another file available elsewhere in the path to replace the deleted one
                                    card = LoadIndexCard(targetName);

                                    // if so then we should report this as a change rather than a delete
                                    if (null != card)
                                        changed = new string[] { GetTargetName(card.Filepath) };
                                    else
                                        deleted = new string[] { targetName };
                                }
                            }

                            if (IsWatchedPath(newPath))
                            {
                                try
                                {
                                    IndexCard newCard = new IndexCard(new FileInfo(e.FullPath));
                                    IndexCard oldCard = Cache[newCard.TargetName];

                                    if (null != oldCard)
                                    {
                                        // the new file supercedes the existing one
                                        if (newCard.Rank < oldCard.Rank)
                                        {
                                            Cache.Remove(oldCard);
                                            Cache.Add(newCard);
                                            changed = new string[] { newCard.TargetName };
                                        }
                                    }
                                    else
                                    {
                                        Cache.Add(newCard);
                                        added = new string[] { newCard.TargetName };
                                    }
                                }
                                catch
                                {
                                    // not a valid LDraw file, so ignore
                                }
                            }
                        }
                    }

                    if (null != Changed && (null != added || null != deleted || null != changed))
                    {
                        LibraryChangedEventArgs eventArgs = new LibraryChangedEventArgs(added, deleted, changed);

                        if (null != SyncContext)
                            SyncContext.Post(delegate(object state) { Changed(null, eventArgs); }, null);
                        else
                            Changed(null, eventArgs);
                    }
                }
                else
                {
                    newPath = e.FullPath;
                    oldPath = e.OldFullPath;

                    // a folder-rename is treated as either an 'add' or a 'remove'
                    if (IsWatchedPath(oldPath))
                    {
                        lock (Lock)
                        {
                            if (null != Cache)
                            {
                                oldPath += @"\";

                                List<IndexCard> cards = new List<IndexCard>(from n in Cache where n.Filepath.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase) select n);

                                Cache.Remove(cards);
                                deleted = from n in cards select n.TargetName;
                            }
                            else if (null != Changed)
                            {
                                deleted = Directory.GetFiles(oldPath, "*.*", SearchOption.AllDirectories);
                            }
                        }
                    }

                    if (IsWatchedPath(newPath))
                        added = AddPath(newPath);

                    if (null != Changed && (null != added || null != deleted))
                    {
                        LibraryChangedEventArgs eventArgs = new LibraryChangedEventArgs(added, deleted, null);

                        if (null != SyncContext)
                            SyncContext.Post(delegate(object state) { Changed(null, eventArgs); }, null);
                        else
                            Changed(null, eventArgs);
                    }
                }
            };

            watcher.EnableRaisingEvents = true;

            lock (Lock)
            {
                Watcher = watcher;
            }
        }

        private static IndexCard LoadIndexCard(string filename)
        {
            string filepath;

            foreach (string path in Configuration.PrimarySearchPath)
            {
                filepath = Path.Combine(path, filename);

                if (File.Exists(filepath))
                {
                    try
                    {
                        IndexCard card = new IndexCard(new FileInfo(filepath));

                        if (null != card)
                        {
                            Cache.Add(card);
                            return card;
                        }
                    }
                    catch
                    {
                        // not a valid LDraw file, so ignore
                    }
                }
            }

            return null;
        }

        private static IEnumerable<string> AddPath(string newPath)
        {
            if (!Directory.Exists(newPath))
                return null;

            List<string> paths    = new List<string>();
            List<IndexCard> cards = new List<IndexCard>();
            DirectoryInfo dirInfo = new DirectoryInfo(newPath);

            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                if (null != Cache)
                {
                    try
                    {
                        IndexCard card = new IndexCard(fileInfo);
                        cards.Add(card);
                    }
                    catch
                    {
                        // ignore non-LDraw files
                        continue;
                    }
                }

                paths.Add(GetTargetName(fileInfo.FullName));
            }

            if (null != Cache && cards.Count > 0)
                Cache.Add(cards);

            if (paths.Count > 0)
                return paths;

            return null;
        }

        private void Save()
        {
            BinaryFormatter     formatter = new BinaryFormatter();
            IsolatedStorageFile isoStore  = IsolatedStorageFile.GetMachineStoreForAssembly();

            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(LibraryFile, FileMode.Create, isoStore))
            {
                using (BufferedStream writer = new BufferedStream(stream))
                {
                    try
                    {
                        formatter.Serialize(writer, this);
                        _updated = true;

                        if (AutoUpdatePartsLst)
                            WritePartsLst();
                    }
                    catch
                    {
                        if (Debugger.IsAttached)
                            Debugger.Break();
                    }
                }
            }
        }

        private void Add(IndexCard card)
        {
            string key = card.TargetName.ToLower();
            _index[key] = card;
            UpdateStatistics();
            Save();
        }

        private void Add(IEnumerable<IndexCard> cards)
        {
            foreach (IndexCard card in cards)
            {
                string key = card.TargetName.ToLower();
                _index[key] = card;
            }

            UpdateStatistics();
            Save();
        }

        private void Remove(IndexCard card)
        {
            string key = card.TargetName.ToLower();

            if (_index.ContainsKey(key))
            {
                _index.Remove(key);
                UpdateStatistics();
                Save();
            }
        }

        private void Remove(IEnumerable<IndexCard> cards)
        {
            foreach (IndexCard card in cards)
            {
                string key = card.TargetName.ToLower();

                if (_index.ContainsKey(key))
                    _index.Remove(key);
            }

            UpdateStatistics();
            Save();
        }

        #endregion Internals

        #region Properties

        /// <summary>
        /// Returns the library-cache following a successful call to <see cref="Load"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="Digitalis.LDTools.DOM.Configuration.LDrawBase"/> changes, the cache will be <see cref="Unload">unloaded</see> and must be
        /// <see cref="Load">reloaded</see> by the application.
        /// </para>
        /// </remarks>
        public static LibraryManager Cache { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parts.lst file should be updated when the contents of the <see cref="LibraryManager"/> change.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Whilst the <see cref="Cache"/> is capable of detecting changes to the LDraw library installed on the computer and updating itself
        /// automatically, older LDraw programs may rely on the <c>parts.lst</c> file. If this property is set to <c>true</c>, <see cref="Cache"/>
        /// will update <c>parts.lst</c> when it detects changes to the LDraw library.
        /// </para>
        /// <para>
        /// Default value is <c>false</c>.
        /// </para>
        /// </remarks>
        /// <seealso cref="WritePartsLst"/>
        public bool AutoUpdatePartsLst
        {
            get { return _autoUpdatePartsLst; }
            set
            {
                _autoUpdatePartsLst = value;

                if (value && (_updated || !File.Exists(PartsLst)))
                {
                    WritePartsLst();
                    _updated = false;
                }
            }
        }
        [NonSerialized]
        private bool _autoUpdatePartsLst;
        [NonSerialized]
        private bool _updated;

        /// <summary>
        /// Gets the number of <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s in the <see cref="Cache"/>.
        /// </summary>
        public int Count { get { lock (Lock) { return _index.Count; } } }

        /// <summary>
        /// Returns the <see cref="T:Digitalis.LDTools.DOM.IndexCard"/> with the specified name, or <c>null</c> if no <see cref="T:Digitalis.LDTools.DOM.IndexCard"/> was found.
        /// </summary>
        /// <param name="name">The <see cref="P:Digitalis.LDTools.DOM.IndexCard.TargetName"/> of the <see cref="T:Digitalis.LDTools.DOM.IndexCard"/> to return. Names are
        ///     not case-sensitive.</param>
        public IndexCard this[string name]
        {
            get
            {
                lock (Lock)
                {
                    IndexCard card;

                    if (_index.TryGetValue(name.ToLower(), out card))
                        return card;

                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the installed version of the LDraw Parts Library.
        /// </summary>
        public LDUpdate InstalledVersion { get; private set; }

        /// <summary>
        /// Gets the set of <see cref="Digitalis.LDTools.DOM.API.Category">categories</see> used by the <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s which are
        ///     <see cref="P:Digitalis.LDTools.DOM.IndexCard.IsPart">parts</see>.
        /// </summary>
        public IEnumerable<Category> PartsCategories { get { return _partsCategories; } }
        [NonSerialized]
        private List<Category> _partsCategories = new List<Category>();

        /// <summary>
        /// Gets the set of <see cref="Digitalis.LDTools.DOM.API.Category">categories</see> used by the <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s which are
        ///     <see cref="P:Digitalis.LDTools.DOM.IndexCard.IsSubpart">subparts</see>.
        /// </summary>
        public IEnumerable<Category> SubpartsCategories { get { return _subpartsCategories; } }
        [NonSerialized]
        private List<Category> _subpartsCategories = new List<Category>();

        /// <summary>
        /// Gets the set of <see cref="Digitalis.LDTools.DOM.API.Category">categories</see> used by the <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s which are
        ///     <see cref="P:Digitalis.LDTools.DOM.IndexCard.IsPrimitive">primitives</see>.
        /// </summary>
        public IEnumerable<Category> PrimitivesCategories { get { return _primitivesCategories; } }
        [NonSerialized]
        private List<Category> _primitivesCategories = new List<Category>();

        #endregion Properties

        #region Constructor

        private LibraryManager(IEnumerable<string> paths, LoadProgressCallback callback, LibraryManager archive, out bool updated)
        {
            List<DirectoryInfo> dirs         = new List<DirectoryInfo>(paths.Count());
            IndexCard           card;
            string              path;
            string              folder;
            string              name;
            int                 total        = 0;
            int                 count        = 0;
            int                 progress;
            int                 lastProgress = 0;

            foreach (string p in paths)
            {
                DirectoryInfo di = new DirectoryInfo(p);

                if (null == di || !di.Exists)
                    continue;

                count = di.EnumerateFiles().Count();

                if (count > 0)
                {
                    dirs.Add(di);
                    total += count;
                }
            }

            if (null == archive)
            {
                updated = true;

                foreach (DirectoryInfo di in dirs)
                {
                    folder = di.Name;
                    path   = di.FullName;

                    foreach (FileSystemInfo fi in di.EnumerateFiles())
                    {
                        count += 100;

                        try
                        {
                            progress = count / total;

                            if (progress > lastProgress)
                            {
                                if (progress > 100)
                                    progress = 100;

                                if (!callback(progress, path, fi.Name))
                                    throw new OperationCanceledException();

                                lastProgress = progress;
                            }

                            if ("S" == folder || "s" == folder || "48" == folder)
                                name = Path.Combine(folder, fi.Name);
                            else
                                name = fi.Name;

                            name = name.ToLower();

                            if (!_index.ContainsKey(name))
                            {
                                card = new IndexCard(folder, fi.Name, fi.FullName, fi.LastWriteTime);
                                _index.Add(name, card);
                            }
                        }
                        catch (FormatException)
                        {
                            // ignore non-LDraw files
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, IndexCard> archiveIndex = archive._index;

                updated = false;

                foreach (DirectoryInfo di in dirs)
                {
                    folder = di.Name;
                    path   = di.FullName;

                    foreach (FileSystemInfo fi in di.EnumerateFiles())
                    {
                        count += 100;

                        try
                        {
                            progress = count / total;

                            if (progress > lastProgress)
                            {
                                if (progress > 100)
                                    progress = 100;

                                if (!callback(progress, path, fi.Name))
                                    throw new OperationCanceledException();

                                lastProgress = progress;
                            }

                            if ("S" == folder || "s" == folder || "48" == folder)
                                name = Path.Combine(folder, fi.Name);
                            else
                                name = fi.Name;

                            name = name.ToLower();

                            if (!_index.ContainsKey(name))
                            {
                                if (archiveIndex.ContainsKey(name))
                                {
                                    card = archiveIndex[name];

                                    if (fi.LastWriteTime.ToFileTimeUtc() != card.Modified || !fi.FullName.Equals(card.Filepath, StringComparison.OrdinalIgnoreCase))
                                    {
                                        card    = new IndexCard(folder, fi.Name, fi.FullName, fi.LastWriteTime);
                                        updated = true;
                                    }
                                }
                                else
                                {
                                    card    = new IndexCard(folder, fi.Name, fi.FullName, fi.LastWriteTime);
                                    updated = true;
                                }

                                _index.Add(name, card);
                            }
                        }
                        catch (FormatException)
                        {
                            // ignore non-LDraw files
                        }
                    }
                }
            }

            UpdateStatistics();
        }

        #endregion Constructor

        #region API

        /// <summary>
        /// Occurs when the <see cref="Cache"/> is <see cref="Load">loaded</see>.
        /// </summary>
        public static event EventHandler CacheLoaded;

        /// <summary>
        /// Occurs when the <see cref="Cache"/> is <see cref="Unload">unloaded</see>.
        /// </summary>
        public static event EventHandler CacheUnloaded;

        /// <summary>
        /// Occurs when an item is added to or removed from the library, or modified or renamed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is available whether the <see cref="Cache"/> is <see cref="Load">loaded</see> or not. Note that the availability and reliability of
        /// the event is dependent on the type of the drive(s) being monitored - networked drives in particular may not generate events.
        /// </para>
        /// <para>
        /// If <see cref="F:System.Threading.SynchronizationContext.Current"/> is not <c>null</c> for the thread which executes the <see cref="LibraryManager"/>'s
        /// static constructor, the event will be sent via its <see cref="M:System.Threading.SynchronizationContext.Post"/> method; otherwise it will be sent on
        /// the thread currently being used to handle <see cref="T:System.IO.FileSystemWatcher"/> events.
        /// </para>
        /// </remarks>
        public static event LibraryChangedEventHandler Changed;

        /// <summary>
        /// Loads the library-cache from the list of paths returned by <see cref="Digitalis.LDTools.DOM.Configuration.FullSearchPath"/>.
        /// </summary>
        /// <param name="callback">A delegate to be notified of the progress of the load.</param>
        /// <param name="autoUpdatePartsLst">If <c>true</c>, <c>parts.lst</c> will be updated if required.</param>
        /// <exception cref="LDrawLibraryAlreadyLoadedException">The <see cref="LibraryManager"/> has already been loaded.</exception>
        /// <exception cref="T:System.OperationCanceledException">The delegate cancelled the load.</exception>
        /// <remarks>
        /// <para>
        /// Raises the <see cref="CacheLoaded"/> event on successful completion.
        /// </para>
        /// </remarks>
        public static void Load(LoadProgressCallback callback, bool autoUpdatePartsLst)
        {
            lock (Lock)
            {
                if (null != Cache)
                    throw new LDrawLibraryAlreadyLoadedException();

                LibraryManager      oldInstance = null;
                BinaryFormatter     formatter   = new BinaryFormatter();
                IsolatedStorageFile isoStore    = IsolatedStorageFile.GetMachineStoreForAssembly();
                bool                updated;

                formatter.TypeFormat = FormatterTypeStyle.XsdString;

                if (0 != isoStore.GetFileNames(LibraryFile).Length)
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(LibraryFile, FileMode.Open, isoStore))
                    {
                        using (BufferedStream reader = new BufferedStream(stream))
                        {
                            try
                            {
                                oldInstance = formatter.Deserialize(reader) as LibraryManager;

                                if (null != oldInstance && oldInstance._version != Hash())
                                    oldInstance = null;
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                Cache = new LibraryManager(Configuration.FullSearchPath, callback, oldInstance, out updated);
                Cache.AutoUpdatePartsLst = autoUpdatePartsLst;

                if (updated)
                {
                    // there have been some changes, so re-save the cache-file
                    callback(100, Resources.Cacheing, String.Empty);
                    Cache.Save();
                }
            }

            if (null != CacheLoaded)
                CacheLoaded(null, EventArgs.Empty);
        }


        /// <summary>
        /// Unloads the <see cref="Cache"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Raises the <see cref="CacheUnloaded"/> event on successful completion.
        /// </para>
        /// </remarks>
        public static void Unload()
        {
            lock (Lock)
            {
                if (null == Cache)
                    return;

                Cache = null;
            }

            if (null != CacheUnloaded)
                CacheUnloaded(null, EventArgs.Empty);
        }

        /// <summary>
        /// Writes the <c>parts.lst</c> file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All <see cref="T:Digitalis.LDTools.DOM.IndexCard"/>s with a <see cref="P:Digitalis.LDTools.DOM.IndexCard.Type"/> of <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Physical_Colour"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Physical_Colour"/> and which are not marked as <see cref="IndexCard.IsRedirect"/>
        /// or <see cref="IndexCard.IsObsolete"/> will be written out.
        /// </para>
        /// </remarks>
        /// <seealso cref="AutoUpdatePartsLst"/>
        public void WritePartsLst()
        {
            lock (Lock)
            {
                using (TextWriter writer = File.CreateText(PartsLst))
                {
                    foreach (IndexCard card in this.OrderBy(n => n.Title, StringComparer.Ordinal))
                    {
                        if (card.IsObsolete || card.IsRedirect)
                            continue;

                        switch (card.Type)
                        {
                            case PageType.Part:
                            case PageType.Part_Physical_Colour:
                            case PageType.Shortcut:
                            case PageType.Shortcut_Physical_Colour:
                                writer.Write(String.Format("{0,-25}  {1}\r\n", card.Name, card.Title));
                                break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<IndexCard> GetEnumerator()
        {
            lock (Lock)
            {
                foreach (KeyValuePair<string, IndexCard> entry in _index)
                {
                    yield return entry.Value;
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion API
    }
}
