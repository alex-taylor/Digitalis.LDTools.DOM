#region License

//
// Events.cs
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
    using System.Collections.Generic;

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    #region ReferenceCacheChanged

    /// <summary>
    /// Represents a method which is invoked when an <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/> is added to or removed from the
    ///     <see cref="T:Digitalis.LDTools.DOM.LDReference"/> cache.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event-args instance containing the event data.</param>
    public delegate void ReferenceCacheChangedEventHandler(object sender, ReferenceCacheChangedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="E:Digitalis.LDTools.DOM.LDReference.CacheEntryAdded"/> and <see cref="E:Digitalis.LDTools.DOM.LDReference.CacheEntryRemoved"/> events.
    /// </summary>
    public class ReferenceCacheChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> which has been added or removed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> may not be modified, as it is <see cref="T:Digitalis.LDTools.DOM.API.IDocument.IsFrozen">frozen</see>. It
        /// should not be cached by the event-handler since it may be evicted from the <see cref="T:Digitalis.LDTools.DOM.LDReference"/> cache at any time.
        /// </para>
        /// </remarks>
        public LDDocument Document { get; private set; }

        /// <summary>
        /// Gets the key used to identify the <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> which has been added or removed.
        /// </summary>
        public string Key { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceCacheChangedEventArgs"/> class with the specified values.
        /// </summary>
        /// <param name="document">The <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> which has been added or removed.</param>
        /// <param name="key">The key used to identify the <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> which has been added or removed.</param>
        public ReferenceCacheChangedEventArgs(LDDocument document, string key)
        {
            Document = document;
            Key      = key;
        }

        #endregion Constructor
    }

    #endregion ReferenceCacheChanged
}
