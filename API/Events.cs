#region License

//
// Events.cs
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

    #endregion Usings

    #region DocumentTreeChanged

    /// <summary>
    /// Represents a method which is invoked when an <see cref="IDocument"/> or one of its descendants changes in some way.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event-args instance containing the event data.</param>
    public delegate void DocumentTreeChangedEventHandler(IDocument sender, DocumentTreeChangedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="IDocument.DocumentTreeChanged"/> event.
    /// </summary>
    public class DocumentTreeChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the number of changes in <see cref="Events"/>.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the changes which have occurred.
        /// </summary>
        public IEnumerable<ObjectChangedEventArgs> Events { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTreeChangedEventArgs"/> class with the specified values.
        /// </summary>
        /// <param name="count">The number of changes.</param>
        /// <param name="events">The changes which have occurred.</param>
        public DocumentTreeChangedEventArgs(int count, IEnumerable<ObjectChangedEventArgs> events)
        {
            Count  = count;
            Events = events;
        }

        #endregion Constructor
    }

    #endregion DocumentTreeChanged

    #region ObjectChanged

    /// <summary>
    /// Represents a method which is invoked when an <see cref="IDOMObject"/> changes in some way.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event-args instance containing the event data.</param>
    public delegate void ObjectChangedEventHandler(IDOMObject sender, ObjectChangedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="IDOMObject.Changed"/> event.
    /// </summary>
    public class ObjectChangedEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="IDOMObject"/> on which the change-specific event occurred.
        /// </summary>
        public IDOMObject Source { get; private set; }

        /// <summary>
        /// Gets a string specifying the change that occurred.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The name of the change-specific event - for example, <c>"NameChanged"</c> or <c>"ElementsAdded"</c>.
        /// </para>
        /// </remarks>
        public string Operation { get; private set; }

        /// <summary>
        /// Gets the original <b>EventArgs</b> for the change.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event-args instance containing the event data from the change-specific event.
        /// </para>
        /// </remarks>
        public EventArgs Parameters { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectChangedEventArgs"/> class with the specified values.
        /// </summary>
        /// <param name="source">The <see cref="IDOMObject"/> on which the change-specific event occurred.</param>
        /// <param name="operation">The name of the change-specific event - for example, <c>"NameChanged"</c> or <c>"ElementsAdded"</c>.</param>
        /// <param name="parameters">The event-args instance containing the event data from the change-specific event.</param>
        public ObjectChangedEventArgs(IDOMObject source, string operation, EventArgs parameters)
        {
            Source     = source;
            Operation  = operation;
            Parameters = parameters;
        }

        #endregion Constructor
    }

    #endregion ObjectChanged
}
