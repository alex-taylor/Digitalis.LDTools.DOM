#region License

//
// DOMObject.cs
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

namespace Digitalis.LDTools.DOM
{
    #region Usings

    using System;
    using System.Runtime.Serialization;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IDOMObject"/>.
    /// </summary>
    /// <remarks>
    /// This is the base type of all classes which make up the document-tree.
    /// </remarks>
    [Serializable]
    public abstract class DOMObject : IDOMObject
    {
        #region Change-notification

        /// <inheritdoc />
        [field:NonSerialized]
        public event ObjectChangedEventHandler Changed;

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="DOMObject"/> is
        ///     <see cref="IsDisposed">disposed</see>.</exception>
        /// <param name="source">The original source of the change-specific event.</param>
        /// <param name="operation">The name of the change-specific event - for example, <c>"NameChanged"</c> or
        ///     <c>"ElementsAdded"</c>.</param>
        /// <param name="parameters">The event-args instance published by the change-specific event.</param>
        /// <remarks>
        /// Provides a standard <see cref="System.EventHandler"/> implementation which raises the <see cref="Changed"/> event.
        /// <p/>
        /// <note>
        /// Note to implementors: this must be called each time a property-specific or action-specific event is published,
        /// passing the name of the event and its <b>EventArgs</b> as parameters.
        /// <p/>
        /// <example>
        /// To implement a standard <see cref="Digitalis.UndoSystem.UndoableProperty{T}"/> on a subclass of
        /// <see cref="DOMObject"/>:
        /// <code lang="csharp">
        /// <![CDATA[
        /// public class MyElement : DOMObject
        /// {
        ///     public event PropertyChangedEventHandler<int> MyPropertyChanged;
        ///
        ///     public int MyProperty
        ///     {
        ///         get
        ///         {
        ///             if (IsDisposed)
        ///                 throw new ObjectDisposedException(null);
        ///
        ///             return myProperty.Value;
        ///         }
        ///         set
        ///         {
        ///             if (value != MyProperty)
        ///                 myProperty.Value = value;
        ///         }
        ///     }
        ///
        ///     private UndoableProperty<int> myProperty = new UndoableProperty<int>();
        ///
        ///     public MyElement() : base()
        ///     {
        ///         myProperty.ValueChanged += delegate(object sender, PropertyChangedEventArgs<int> e)
        ///         {
        ///             if (IsDisposed)
        ///                 throw new ObjectDisposedException(null);
        ///
        ///             if (IsFrozen)
        ///                 throw new ObjectFrozenException();
        ///
        ///             // first send the property-specific event
        ///             if (MyPropertyChanged != null)
        ///                 MyPropertyChanged(this, e);
        ///
        ///             // then send the generic event, passing the name of the property-specific event
        ///             // and its EventArgs
        ///             OnChanged(this, "MyPropertyChanged", e);
        ///         };
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// </note>
        /// </remarks>
        protected virtual void OnChanged(IDOMObject source, string operation, EventArgs parameters)
        {
            OnChanged(new ObjectChangedEventArgs(source, operation, parameters));
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="DOMObject"/> is
        ///     <see cref="IsDisposed"/>disposed.</exception>
        /// <param name="e">The event-args for the event.</param>
        protected virtual void OnChanged(ObjectChangedEventArgs e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != Changed)
                Changed(this, e);
        }

        // used by the unit-tests
        internal int ChangedSubscribers
        {
            get
            {
                if (null == Changed)
                    return 0;

                return Changed.GetInvocationList().Length;
            }
        }

        #endregion Change-notification

        #region Cloning and Serialization

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses must override <see cref="InitializeObject"/> if they have any setup to do on the
        /// newly-created <see cref="DOMObject"/>.
        /// </note>
        /// </remarks>
        public IDOMObject Clone()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            IDOMObject el = (IDOMObject)Activator.CreateInstance(this.GetType());
            InitializeObject(el);
            return el;
        }

        /// <summary>
        /// Copies the values of the <see cref="DOMObject"/> to another.
        /// </summary>
        /// <remarks>
        /// This is used by <see cref="Clone"/> to initialize a newly-created object before returning it. Subclasses should
        /// override this to copy their values to the new object.
        /// <p/>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void InitializeObject(IDOMObject obj)
        {
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        /// <inheritdoc />
        public abstract StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding);

        #endregion Code-generation

        #region Constants

        /// <summary>
        /// Gets the set of characters accepted as whitespace by the
        ///     <see href="http://www.ldraw.org/article/218.html">LDraw.org File Format specification</see>.
        /// </summary>
        /// <remarks>
        /// This may be used by subclasses when reading or writing LDraw code.
        /// </remarks>
        protected static readonly char[] WhitespaceChars = new char[] { ' ', '\t' };

        /// <summary>
        /// Gets the line-terminator string defined by the
        ///     <see href="http://www.ldraw.org/article/218.html">LDraw.org File Format specification</see>.
        /// </summary>
        /// <remarks>
        /// This may be used by subclasses when reading or writing LDraw code.
        /// </remarks>
        protected static readonly string LineTerminator = "\r\n";

        #endregion Constants

        #region Constructor

        /// <inheritdoc />
        ~DOMObject()
        {
            Dispose(false);
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        public virtual bool IsDisposing { get { return _isDisposing; } private set { _isDisposing = value; } }
        private bool _isDisposing;

        /// <inheritdoc />
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DOMObject"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     managed resources.</param>
        /// <remarks>
        /// This method is called by the public <see cref="Dispose()"/> method and the <see cref="System.Object.Finalize"/>
        /// method. <see cref="Dispose()"/> invokes this method with <paramref name="disposing"/> set to <c>true</c>.
        /// <see cref="System.Object.Finalize"/> invokes it with <paramref name="disposing"/> set to <c>false</c>.
        /// <p/>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                IsDisposing = true;
                Dispose(true);
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
            finally
            {
                IsDisposing = false;
            }
        }

        #endregion Disposal

        #region Freezing

        /// <inheritdoc />
        public virtual bool IsFrozen
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsDisposing)
                    return false;

                return _isFrozen;
            }
            private set
            {
                _isFrozen = value;
            }
        }
        private bool _isFrozen;

        /// <inheritdoc />
        public void Freeze()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (!IsFrozen)
            {
                OnFreezing(EventArgs.Empty);
                IsFrozen = true;
                OnFrozen(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called just prior to freezing the <see cref="DOMObject"/>.
        /// </summary>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnFreezing(EventArgs e)
        {
        }

        /// <summary>
        /// Called just after freezing the <see cref="DOMObject"/>.
        /// </summary>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected virtual void OnFrozen(EventArgs e)
        {
        }

        #endregion Freezing

        #region Self-description

        /// <inheritdoc />
        public abstract DOMObjectType ObjectType { get; }

        /// <inheritdoc />
        public abstract bool IsImmutable { get; }

        #endregion Self-description
    }
}
