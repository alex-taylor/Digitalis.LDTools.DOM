#region License

//
// Palette.cs
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

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    /// <summary>
    /// Represents the LDraw system palette as defined in <see href="http://www.ldraw.org/library/official/ldconfig.ldr">ldconfig.ldr"</see>.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The palette is loaded on the first access to <see cref="SystemPalette"/>. Once loaded, it will update automatically if either <c>ldconfig.ldr</c>
    /// or <see cref="Digitalis.LDTools.DOM.Configuration.LDConfigPath"/> is changed.
    /// </para>
    /// </remarks>
    public sealed class Palette : IEnumerable<IColour>, IDisposable
    {
        #region Internals

        // LDraw code for the two permanent colours, in case the ldconfig.ldr file isn't available
        private const string MainCode = "0 !COLOUR Main_Color CODE 16 VALUE #7F7F7F EDGE #333333";
        private const string EdgeCode = "0 !COLOUR Edge_Color CODE 24 VALUE #7F7F7F EDGE #333333";

        private Dictionary<uint, IColour> _palette;
        private FileSystemWatcher _watcher;

        private void Watch(string path)
        {
            if (null != _watcher)
                _watcher.Dispose();

            _watcher              = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            _watcher.NotifyFilter = NotifyFilters.LastWrite;

            _watcher.Changed += delegate(object sender, FileSystemEventArgs e)
            {
                try
                {
                    LoadPalette(path);
                    Watch(path);

                    if (null != ContentsChanged)
                        ContentsChanged(this, e);
                }
                catch
                {
                }
            };

            _watcher.Error += delegate(object sender, ErrorEventArgs e)
            {
                // if the watcher dies, create a new one to replace it
                Watch(path);
            };

            _watcher.EnableRaisingEvents = true;
        }

        private void LoadPalette(string path)
        {
            string line;
            uint maxCode = 0;
            Dictionary<uint, IColour> palette = new Dictionary<uint, IColour>();

            using (TextReader stream = File.OpenText(path))
            {
                while (null != (line = stream.ReadLine()))
                {
                    if (line.Contains("!COLOUR"))
                    {
                        LDColour c = new LDColour(line);
                        c.IsSystemPaletteColour = true;
                        c.Freeze();
                        palette.Add(c.Code, c);

                        if (c.Code > maxCode)
                            maxCode = c.Code;
                    }
                }

                if (!palette.ContainsKey(MainColour))
                    palette.Add(MainColour, new LDColour(MainCode));

                if (!palette.ContainsKey(EdgeColour))
                    palette.Add(EdgeColour, new LDColour(EdgeCode));
            }

            _palette = palette;
            MaxCode  = maxCode;
        }

        #endregion Internals

        #region Properties

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> for the LDraw Main_Colour.
        /// </summary>
        public const uint MainColour = 16;

        /// <summary>
        /// The <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> for the LDraw Edge_Colour.
        /// </summary>
        public const uint EdgeColour = 24;

        /// <summary>
        /// Gets the system palette.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The palette is loaded from the <c>ldconfig.ldr</c> file found in <see cref="Digitalis.LDTools.DOM.Configuration.LDrawBase"/>.
        /// If this file is missing or unreadable, a minimal palette containing only <see cref="MainColour"/> and <see cref="EdgeColour"/> will be returned.
        /// </para>
        /// </remarks>
        public static Palette SystemPalette
        {
            get
            {
                if (null == _defaultPalette)
                {
                    try
                    {
                        _defaultPalette = new Palette(Configuration.LDConfigPath);
                    }
                    catch
                    {
                        _defaultPalette = new Palette();
                    }
                }

                return _defaultPalette;
            }
        }
        private static Palette _defaultPalette;

        /// <summary>
        /// Gets the number of entries in the <see cref="Palette"/>.
        /// </summary>
        public int Count { get { return _palette.Count; } }

        /// <summary>
        /// Gets the highest <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> in the <see cref="Palette"/>.
        /// </summary>
        public uint MaxCode { get; private set; }

        /// <summary>
        /// Occurs when the contents of the <see cref="Palette"/> have been updated due to a change in the <c>ldconfig.ldr</c> file or
        /// <see cref="Digitalis.LDTools.DOM.Configuration.LDConfigPath"/>.
        /// </summary>
        public event EventHandler ContentsChanged;

        #endregion Properties

        #region Constructor

        private Palette()
        {
            _palette = new Dictionary<uint, IColour>();
            _palette.Add(MainColour, new LDColour(MainCode));
            _palette.Add(EdgeColour, new LDColour(EdgeCode));
            MaxCode = EdgeColour;
        }

        private Palette(string path)
        {
            LoadPalette(path);
            Watch(path);

            if (path == Configuration.LDConfigPath)
            {
                Configuration.LDrawBaseChanged += delegate(object sender, EventArgs e)
                {
                    LoadPalette(Configuration.LDConfigPath);
                    Watch(Configuration.LDConfigPath);

                    if (null != ContentsChanged)
                        ContentsChanged(this, e);
                };
            }
        }

        /// <inheritdoc />
        ~Palette()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (null != _watcher)
                _watcher.Dispose();
        }

        #endregion Constructor

        #region API

        /// <summary>
        /// Initializes a new instance of the <see cref="Palette"/> class for a specified <see cref="P:Digitalis.LDTools.DOM.API.IElement"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <remarks>
        /// <para>
        /// The returned <see cref="Palette"/> consists of the <see cref="SystemPalette"/> modified with any <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> elements
        /// encountered between <paramref name="element"/> and the top of its containing document-tree. It therefore represents the full range of non-<i>Direct Colours</i>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IGraphic.ColourValue"/>s that may be used with <paramref name="element"/>.
        /// </para>
        /// </remarks>
        public static Palette PaletteForElement(IElement element)
        {
            List<IColour> localColours   = null;
            IElementCollection container = element.Parent;
            IPage page                   = element.Page;
            IStep step                   = element.Step;
            int stepIdx;

            if (null != page && null != step)
                stepIdx = page.IndexOf(step);
            else
                stepIdx = -1;

            if (null == container)
                container = step;

            while (null != container)
            {
                if (container.ContainsColourElements)
                {
                    IColour localColour;

                    if (null == localColours)
                        localColours = new List<IColour>();

                    int idx = container.IndexOf(element);

                    for (int i = idx - 1; i >= 0; i--)
                    {
                        localColour = container[i] as IColour;

                        if (null != localColour)
                        {
                            localColour = localColour.Clone() as IColour;
                            localColour.Freeze();
                            localColours.Insert(0, localColour);
                        }
                    }
                }

                if (null != container.Parent)
                {
                    element   = container as IElement;
                    container = container.Parent;
                }
                else
                {
                    if (stepIdx <= 0)
                        break;

                    container = page[--stepIdx];
                    element   = container[container.Count - 1];
                }
            }

            if (null == localColours)
                return SystemPalette;

            Palette palette = new Palette();

            foreach (IColour colour in SystemPalette)
            {
                palette[colour.Code] = colour;
            }

            palette.MaxCode = SystemPalette.MaxCode;

            foreach (IColour colour in localColours)
            {
                palette[colour.Code] = colour;

                if (colour.Code > palette.MaxCode)
                    palette.MaxCode = colour.Code;
            }

            return palette;
        }

        /// <summary>
        /// Gets the <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> with the specified <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/>.
        /// </summary>
        /// <param name="code">The code of the <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> to get.</param>
        /// <returns>The requested <see cref="T:Digitalis.LDTools.DOM.API.IColour"/>, or <c>null</c> if it does not exist.</returns>
        public IColour this[uint code] { get { if (!_palette.ContainsKey(code)) return null; return _palette[code]; } private set { _palette[code] = value; } }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Palette"/>.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<IColour> GetEnumerator()
        {
            foreach (KeyValuePair<uint, IColour> key in _palette)
                yield return key.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion API
    }
}
