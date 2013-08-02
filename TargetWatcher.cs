#region License

//
// TargetWatcher.cs
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

namespace Digitalis.LDTools
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using Timer = System.Threading.Timer;

    #endregion Usings

    internal static class TargetWatcher
    {
        #region Inner types

        private class FileSystemEvent
        {
            public FileSystemEventArgs    Args;
            public FileSystemEventHandler Event;
            public Timer                  Timer;

            public FileSystemEvent(FileSystemEventHandler handler)
            {
                Timer  = new Timer(OnExpired, null, Timeout.Infinite, Timeout.Infinite);
                Event += handler;
            }

            private void OnExpired(object state)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);

                if (null != Event)
                    Event(this, Args);
            }
        }

        private class RenamedEvent
        {
            public RenamedEventArgs    Args;
            public RenamedEventHandler Event;
            public Timer               Timer;

            public RenamedEvent(RenamedEventHandler handler)
            {
                Timer  = new Timer(OnExpired, null, Timeout.Infinite, Timeout.Infinite);
                Event += handler;
            }

            private void OnExpired(object state)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);

                if (null != Event)
                    Event(this, Args);
            }
        }

        #endregion Inner types

        #region Internals

        private const int TimerPeriod = 100;     // milliseconds

        private static Dictionary<string, FileSystemWatcher> _watchers    = new Dictionary<string, FileSystemWatcher>();
        private static Dictionary<string, FileSystemEvent>   _fileChanged = new Dictionary<string, FileSystemEvent>();
        private static Dictionary<string, FileSystemEvent>   _fileCreated = new Dictionary<string, FileSystemEvent>();
        private static Dictionary<string, FileSystemEvent>   _fileDeleted = new Dictionary<string, FileSystemEvent>();
        private static Dictionary<string, RenamedEvent>      _fileRenamed = new Dictionary<string, RenamedEvent>();

        #endregion Internals

        #region API

        public static void WatchFile(string path, FileSystemEventHandler onChanged, FileSystemEventHandler onDeleted, RenamedEventHandler onRenamed)
        {
            lock (_watchers)
            {
                string folder = Path.GetDirectoryName(path);
                string key    = folder.ToLower();

                if (!_watchers.ContainsKey(key))
                {
                    FileSystemWatcher w = new FileSystemWatcher(folder, "*.*");

                    w.Changed += OnChanged;
                    w.Created += OnCreated;
                    w.Deleted += OnDeleted;
                    w.Renamed += OnRenamed;

                    w.NotifyFilter        = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    w.EnableRaisingEvents = true;

                    _watchers.Add(key, w);
                }

                key = path.ToLower();

                if (null != onChanged)
                {
                    if (_fileChanged.ContainsKey(key))
                        _fileChanged[key].Event += onChanged;
                    else
                        _fileChanged.Add(key, new FileSystemEvent(onChanged));
                }

                if (null != onDeleted)
                {
                    if (_fileDeleted.ContainsKey(key))
                        _fileDeleted[key].Event += onDeleted;
                    else
                        _fileDeleted.Add(key, new FileSystemEvent(onDeleted));
                }

                if (null != onRenamed)
                {
                    if (_fileRenamed.ContainsKey(key))
                        _fileRenamed[key].Event += onRenamed;
                    else
                        _fileRenamed.Add(key, new RenamedEvent(onRenamed));
                }
            }
        }

        public static void WatchFolder(string path, FileSystemEventHandler onChanged, FileSystemEventHandler onCreated, FileSystemEventHandler onDeleted, RenamedEventHandler onRenamed)
        {
            lock (_watchers)
            {
                string key = path.ToLower();

                if (!_watchers.ContainsKey(key))
                {
                    FileSystemWatcher w = new FileSystemWatcher(path, String.Empty);

                    w.Changed += OnChanged;
                    w.Created += OnCreated;
                    w.Deleted += OnDeleted;
                    w.Renamed += OnRenamed;

                    w.NotifyFilter        = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    w.EnableRaisingEvents = true;

                    _watchers.Add(key, w);
                }

                if (null != onChanged)
                {
                    if (_fileChanged.ContainsKey(key))
                        _fileChanged[key].Event += onChanged;
                    else
                        _fileChanged.Add(key, new FileSystemEvent(onChanged));
                }

                if (null != onCreated)
                {
                    if (_fileChanged.ContainsKey(key))
                        _fileCreated[key].Event += onCreated;
                    else
                        _fileCreated.Add(key, new FileSystemEvent(onCreated));
                }

                if (null != onDeleted)
                {
                    if (_fileDeleted.ContainsKey(key))
                        _fileDeleted[key].Event += onDeleted;
                    else
                        _fileDeleted.Add(key, new FileSystemEvent(onDeleted));
                }

                if (null != onRenamed)
                {
                    if (_fileRenamed.ContainsKey(key))
                        _fileRenamed[key].Event += onRenamed;
                    else
                        _fileRenamed.Add(key, new RenamedEvent(onRenamed));
                }
            }
        }

        public static void RemoveWatch(string path, FileSystemEventHandler onChanged, FileSystemEventHandler onDeleted, RenamedEventHandler onRenamed)
        {
            RemoveWatch(path, onChanged, null, onDeleted, onRenamed);
        }

        public static void RemoveWatch(string path, FileSystemEventHandler onChanged, FileSystemEventHandler onCreated, FileSystemEventHandler onDeleted, RenamedEventHandler onRenamed)
        {
            lock (_watchers)
            {
                string key = path.ToLower();

                if (null != onChanged && _fileChanged.ContainsKey(key))
                {
                    _fileChanged[key].Event -= onChanged;

                    if (null == _fileChanged[key].Event)
                        _fileChanged.Remove(key);
                }

                if (null != onCreated && _fileCreated.ContainsKey(key))
                {
                    _fileCreated[key].Event -= onCreated;

                    if (null == _fileCreated[key].Event)
                        _fileCreated.Remove(key);
                }

                if (null != onDeleted && _fileDeleted.ContainsKey(key))
                {
                    _fileDeleted[key].Event -= onDeleted;

                    if (null == _fileDeleted[key].Event)
                        _fileDeleted.Remove(key);
                }

                if (null != onRenamed && _fileRenamed.ContainsKey(key))
                {
                    _fileRenamed[key].Event -= onRenamed;

                    if (null == _fileRenamed[key].Event)
                        _fileRenamed.Remove(key);
                }
            }
        }

        public static void OnChanged(object sender, FileSystemEventArgs e)
        {
            lock (_watchers)
            {
                FileSystemEvent handler;
                string          key     = e.FullPath.ToLower();

                if (_fileChanged.TryGetValue(key, out handler))
                {
                    handler.Args = e;
                    handler.Timer.Change(TimerPeriod, Timeout.Infinite);
                }
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            lock (_watchers)
            {
                FileSystemEvent handler;
                string          key     = e.FullPath.ToLower();

                if (_fileCreated.TryGetValue(key, out handler))
                {
                    handler.Args = e;
                    handler.Timer.Change(TimerPeriod, Timeout.Infinite);
                }
            }
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_watchers)
            {
                FileSystemEvent handler;
                string          key     = e.FullPath.ToLower();

                if (_fileDeleted.TryGetValue(key, out handler))
                {
                    handler.Args = e;
                    handler.Timer.Change(TimerPeriod, Timeout.Infinite);
                }
            }
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            lock (_watchers)
            {
                RenamedEvent handler;
                string       key     = e.OldFullPath.ToLower();

                if (_fileRenamed.TryGetValue(key, out handler))
                {
                    handler.Args = e;
                    handler.Timer.Change(TimerPeriod, Timeout.Infinite);
                }
            }
        }

        #endregion API
    }
}