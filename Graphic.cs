#region License

//
// Graphic.cs
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IGraphic"/>.
    /// </summary>
    [Serializable]
    public abstract class Graphic : Groupable, IGraphic
    {
        #region Inner types

        /// <summary>
        /// Descriptor class for a co-location problem.
        /// </summary>
        protected class ColocationProblem : IProblemDescriptor
        {
            /// <inheritdoc />
            public Guid Guid { get; private set; }

            /// <inheritdoc />
            public IDocumentElement Element { get; private set; }

            /// <inheritdoc />
            public Severity Severity { get; private set; }

            /// <inheritdoc />
            public string Description { get; private set; }

            /// <inheritdoc />
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ColocationProblem"/> class with the specified values.
            /// </summary>
            /// <param name="element">The element to which the problem applies.</param>
            /// <param name="guid">The GUID used to identify the problem.</param>
            /// <param name="severity">The severity of the problem.</param>
            /// <param name="description">A description of the problem.</param>
            /// <param name="fixes">A collection of fixes for the problem.</param>
            public ColocationProblem(IGraphic element, Guid guid, Severity severity, string description, IEnumerable<IFixDescriptor> fixes)
            {
                Element     = element;
                Guid        = guid;
                Severity    = severity;
                Description = description;
                Fixes       = fixes;
            }
        }

        /// <summary>
        /// Descriptor class for a colour-value problem.
        /// </summary>
        protected class InvalidColourProblem : IProblemDescriptor
        {
            /// <inheritdoc />
            public Guid Guid { get; private set; }

            /// <inheritdoc />
            public IDocumentElement Element { get; private set; }

            /// <inheritdoc />
            public Severity Severity { get; private set; }

            /// <inheritdoc />
            public string Description { get; private set; }

            /// <inheritdoc />
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidColourProblem"/> class with the specified values.
            /// </summary>
            /// <param name="element">The element to which the problem applies.</param>
            /// <param name="guid">The GUID used to identify the problem.</param>
            /// <param name="severity">The severity of the problem.</param>
            /// <param name="description">A description of the problem.</param>
            /// <param name="fixes">A collection of fixes for the problem.</param>
            public InvalidColourProblem(IGraphic element, Guid guid, Severity severity, string description, IEnumerable<IFixDescriptor> fixes)
            {
                Element     = element;
                Guid        = guid;
                Severity    = severity;
                Description = description;
                Fixes       = fixes;
            }
        }

        #endregion Inner types

        #region Analytics

        /// <summary>
        /// Gets the <see cref="System.Guid"/> used to identify
        ///     <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsColourInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_ColourInvalid = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="System.Guid"/> used to identify
        ///     <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsColocated"/> condition.
        /// </summary>
        public static readonly Guid Problem_CoordinatesColocated = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="System.Guid"/> used to identify
        ///     <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe
        ///     the <see cref="IsDuplicate"/> condition.
        /// </summary>
        public static readonly Guid Problem_GraphicDuplicate = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether the <see cref="Graphic"/> is a duplicate of another <see cref="Graphic"/> in the
        ///     same <see cref="Digitalis.LDTools.DOM.API.IPage"/>.
        /// </summary>
        /// <remarks>
        /// What is considered to be a duplicate is defined by the
        /// <see href="http://www.ldraw.org/article/512.html#duplicates">LDraw specification</see> and will vary according to
        /// the specific subtype of <see cref="Graphic"/>.
        /// </remarks>
        public bool IsDuplicate { get { return false; } }

        /// <summary>
        /// Gets the <see cref="Graphic"/>s in the containing <see cref="Digitalis.LDTools.DOM.API.IPage"/> which are duplicates
        ///     of this one.
        /// </summary>
        /// <remarks>
        /// What is considered to be a duplicate is defined by the
        /// <see href="http://www.ldraw.org/article/512.html#duplicates">LDraw specification</see> and will vary according to
        /// the specific subtype of <see cref="Graphic"/>.
        /// </remarks>
        public IGraphic[] Duplicates { get { return new IGraphic[0]; } }

        /// <inheritdoc />
        /// <remarks>
        /// <see cref="Graphic"/> compares the <see cref="ColourValue"/> if <see cref="ColourValueEnabled">enabled</see> and the
        /// <see cref="Coordinates"/> of <paramref name="graphic"/> for equality. The order of the <see cref="Coordinates"/> is
        /// unimportant.
        /// </remarks>
        public virtual bool IsDuplicateOf(IGraphic graphic)
        {
            if (graphic == this)
                return false;

            if (ColourValueEnabled && graphic.ColourValueEnabled && ColourValue != graphic.ColourValue)
                return false;

            if (CoordinatesCount != graphic.CoordinatesCount)
                return false;

            return CompareCoordinates(graphic);
        }

        private bool CompareCoordinates(IGraphic dupe)
        {
            List<Vector3d> dupeCoords = dupe.Coordinates.ToList<Vector3d>();

            foreach (Vector3d v in CoordinatesArray)
            {
                dupeCoords.Remove(v);
            }

            return (0 == dupeCoords.Count);
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="ColourValue"/> is set to a value that can be resolved to an
        ///     <see cref="Digitalis.LDTools.DOM.API.IColour"/> and is a valid colour for the type of <see cref="Graphic"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ColourValue"/> is regarded as valid if:
        /// <list type="bullet">
        ///   <item><term>it is an opaque <i>Direct Colours</i> value</term></item>
        ///   <item><term>it refers to an <see cref="Digitalis.LDTools.DOM.API.IColour"/> which precedes the
        ///     <see cref="Graphic"/> in the document-tree</term></item>
        ///   <item><term>it refers to an <see cref="Digitalis.LDTools.DOM.API.IColour"/> in the
        ///     <see cref="Palette.SystemPalette"/></term></item>
        ///   <item><term>it is a valid colour for the type of <see cref="Graphic"/></term></item>
        /// </list>
        /// <p/>
        /// Subclasses of <see cref="Graphic"/> may add further checks.
        /// </remarks>
        public virtual bool IsColourInvalid { get { return IsColourInvalidImpl(); } }

        private bool IsColourInvalidImpl()
        {
            if (!ColourValueEnabled)
                return false;

            uint colourValue = ColourValue;

            if (LDColour.IsOpaqueDirectColour(colourValue))
                return false;

            if (LDColour.IsTransparentDirectColour(colourValue))
                return true;

            IColour colour = GetColour(colourValue);

            return (colourValue != colour.Code);
        }

        /// <summary>
        /// Gets a value indicating whether any of the <see cref="Coordinates"/> of the <see cref="Graphic"/> are co-located.
        /// </summary>
        public bool IsColocated { get { return (0 != ColocatedCoordinates.Length); } }

        /// <summary>
        /// Gets the indices of any co-located <see cref="Coordinates"/> of the <see cref="Graphic"/>.
        /// </summary>
        /// <remarks>
        /// The returned values are indices into <see cref="Coordinates"/> if it is converted to an array.
        /// </remarks>
        public uint[] ColocatedCoordinates
        {
            get
            {
                if (!_validated)
                {
                    List<uint> indices = new List<uint>((int)CoordinatesCount);

                    for (uint idx = 0; idx < CoordinatesCount; idx++)
                    {
                        if (!CheckVertexIsUnique(idx))
                            indices.Add(idx);
                    }

                    _validated = true;
                    _colocated = indices.ToArray();
                }

                return _colocated.Clone() as uint[];
            }
        }
        private uint[] _colocated;
        private bool _validated;

        private bool CheckVertexIsUnique(uint idx)
        {
            Vector3d v = CoordinatesArray[idx];

            for (int i = 0; i < CoordinatesCount; i++)
            {
                if (i != idx)
                {
                    if (v == CoordinatesArray[i])
                        return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <see cref="Graphic"/> detects the <see cref="IsColocated"/> and <see cref="IsColourInvalid"/> conditions. Subclasses
        /// may add their own checks.
        /// </remarks>
        public override bool HasProblems(CodeStandards mode)
        {
            return IsColocated || IsColourInvalid;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Whilst <see cref="Graphic"/> detects the <see cref="IsColocated"/> condition, it does <b>not</b> return
        /// <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s for the condition. This is to allow its
        /// subclasses to supply individual <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s for the problem.
        /// <p/>
        /// The protected class <see cref="ColocationProblem"/> is provided for subclasses to use when <see cref="IsColocated"/>
        /// is <c>true</c>. This should normally have its
        /// <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor.Guid"/> set to
        /// <see cref="Problem_CoordinatesColocated"/>, but subclasses may supply their own if required.
        /// <p/>
        /// For the <see cref="IsColourInvalid"/> condition it detects <see cref="ColourValue"/>s that cannot be resolved and
        /// returns suitable <see cref="Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s for these, but does not handle
        /// resolvable values which are not permitted for the specific type of <see cref="Graphic"/>, which must be handled by
        /// the subclass.
        /// <p/>
        /// The protected class <see cref="InvalidColourProblem"/> is provided for subclasses to use when
        /// <see cref="IsColourInvalid"/> is <c>true</c>. This should normally have its
        /// <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor.Guid"/> set to
        /// <see cref="Problem_ColourInvalid"/>, but subclasses may supply their own if required.
        /// <p/>
        /// The <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity"/> of the
        /// <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s returned varies by <paramref name="mode"/>:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Problem</term>
        ///         <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity"/></description>
        ///     </listheader>
        ///     <item>
        ///         <term><see cref="Problem_ColourInvalid"/></term>
        ///         <description>
        ///             <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///             <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or
        ///             <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///             <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Information"/> otherwise
        ///         </description>
        ///     </item>
        /// </list>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (IsColourInvalidImpl())
            {
                String description;
                Severity severity;

                if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
                    severity = Severity.Error;
                else
                    severity = Severity.Information;

                if (LDColour.IsTransparentDirectColour(ColourValue))
                    description = Resources.Analytics_InvalidDirectColour;
                else
                    description = String.Format(Resources.Analytics_InvalidColour, ColourValue);

                problems.Add(new InvalidColourProblem(this, Problem_ColourInvalid, severity, description, null));
            }

            return problems;
        }

        #endregion Analytics

        #region Attributes

        /// <inheritdoc />
        public virtual bool IsVisible
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _isVisible.Value;
            }
            set
            {
                if (_isVisible.Value != value)
                    _isVisible.Value = value;
            }
        }
        private UndoableProperty<bool> _isVisible = new UndoableProperty<bool>(true);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<bool> IsVisibleChanged;

        /// <summary>
        /// Raises the <see cref="IsVisibleChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Graphic"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnIsVisibleChanged(PropertyChangedEventArgs<bool> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != IsVisibleChanged)
                IsVisibleChanged(this, e);

            OnChanged(this, "IsVisibleChanged", e);
        }

        /// <inheritdoc />
        public virtual bool IsGhosted
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _isGhosted.Value;
            }
            set
            {
                if (_isGhosted.Value != value)
                    _isGhosted.Value = value;
            }
        }
        private UndoableProperty<bool> _isGhosted = new UndoableProperty<bool>(false);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<bool> IsGhostedChanged;

        /// <summary>
        /// Raises the <see cref="IsGhostedChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Graphic"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnIsGhostedChanged(PropertyChangedEventArgs<bool> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != IsGhostedChanged)
                IsGhostedChanged(this, e);

            OnChanged(this, "IsGhostedChanged", e);
        }

        #endregion Attributes

        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IGraphic graphic = (IGraphic)obj;

            graphic.Coordinates = Coordinates;
            graphic.ColourValue = ColourValue;
            graphic.IsVisible   = IsVisible;
            graphic.IsGhosted   = IsGhosted;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        /// <param name="sb">A <see cref="System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <param name="codeFormat">The format required for the returned code.</param>
        /// <param name="overrideColour">Not used.</param>
        /// <param name="transform">Not used.</param>
        /// <param name="winding">Not used.</param>
        /// <remarks>
        /// If <see cref="IsGhosted"/> is <c>true</c> and <paramref name="codeFormat"/> is
        /// <see cref="Digitalis.LDTools.DOM.API.CodeStandards.Full"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> this will append the <i>GHOST</i>
        /// meta-command to <paramref name="sb"/>. Similarly if <see cref="IsVisible"/> is <c>false</c> the <i>MLCAD HIDE</i>
        /// meta-command will be appended.
        /// <p/>
        /// If the <see cref="Graphic"/>'s <see cref="Digitalis.LDTools.DOM.API.IDOMObject.ObjectType"/> is
        /// <see cref="Digitalis.LDTools.DOM.API.DOMObjectType.Texmap"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.DOMObjectType.CompositeElement"/> the meta-commands are not appended, as these
        /// two types do not implement <see cref="IsVisible"/> and <see cref="IsGhosted"/> directly.
        /// <p/>
        /// <note>
        /// Note to implementors: whilst <see cref="Graphic"/> implements <see cref="ColourValue"/> and
        /// <see cref="Coordinates"/>, it does <b>not</b> convert them to LDraw code. Subclasses must override this method, call
        /// their superclass and then append their own LDraw code to the returned string.
        /// </note>
        /// </remarks>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            base.GenerateCode(sb, codeFormat, overrideColour, ref transform, winding);

            // Texmap and CompositeElement do not implement these attributes directly
            if ((CodeStandards.Full == codeFormat || CodeStandards.OfficialModelRepository == codeFormat) && DOMObjectType.Texmap != ObjectType && DOMObjectType.CompositeElement != ObjectType)
            {
                if (IsGhosted)
                    sb.Append("0 GHOST ");

                if (!IsVisible)
                    sb.Append("0 MLCAD HIDE ");
            }

            return sb;
        }

        #endregion Code-generation

        #region Colour

        private IColour _directColour;

        /// <inheritdoc />
        public uint ColourValue
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _colourValue.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (ColourValueEnabled && _colourValue.Value != value)
                    _colourValue.Value = value;
            }
        }
        private UndoableProperty<uint> _colourValue = new UndoableProperty<uint>(Palette.MainColour);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<uint> ColourValueChanged;

        /// <summary>
        /// Raises the <see cref="ColourValueChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Graphic"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnColourValueChanged(PropertyChangedEventArgs<uint> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != ColourValueChanged)
                ColourValueChanged(this, e);
        }

        /// <summary>
        /// Sets <see cref="ColourValue"/> from the supplied LDraw code.
        /// </summary>
        /// <exception cref="System.FormatException">The supplied code was not valid LDraw colour code.</exception>
        /// <param name="code">The LDraw code representing the colour-value.</param>
        /// <remarks>
        /// This is provided largely for the benefit of subclass constructors which parse lines of LDraw code. It handles values
        /// in the following formats:
        /// <example>
        /// <code lang="csharp">
        /// <![CDATA[
        /// SetColourValue("1");            // set ColourValue to a Code, from either the system-palette or an IColour in the document-tree
        /// SetColourValue("#2FF0000");     // set ColourValue to an opaque Direct Colours value of fully-saturated red
        /// SetColourValue("0x2FF0000");    // the same as above but with the '0x' prefix
        /// SetColourValue("50266112");     // the same as above but in base-10
        /// ]]>
        /// </code>
        /// </example>
        /// </remarks>
        protected void SetColourValue(string code)
        {
            if (code.StartsWith("0x") || code.StartsWith("0X"))
                ColourValue = uint.Parse(code.Substring(2), NumberStyles.HexNumber);
            else if ('#' == code[0])
                ColourValue = uint.Parse(code.Substring(1), NumberStyles.HexNumber);
            else
                ColourValue = uint.Parse(code);
        }

        /// <inheritdoc />
        public IColour GetColour(uint overrideColour)
        {
            if (null != _directColour)
                return _directColour;

            uint colourValue = ColourValue;

            if (OverrideableColourValue == colourValue)
                colourValue = overrideColour;

            if (LDColour.IsDirectColour(colourValue))
                return new LDColour(colourValue);

            // try the document-tree first
            IStep step                = Step;
            IElementCollection parent = Parent;
            IPageElement element      = this;
            IColour colour;

            while (null != parent)
            {
                if (parent.ContainsColourElements)
                {
                    int idx = parent.IndexOf(element as IElement);

                    for (int i = idx; i >= 0; i--)
                    {
                        colour = parent[i] as IColour;

                        if (null != colour && colour.Code == colourValue)
                            return colour;
                    }
                }

                element = parent;
                parent  = parent.Parent;

                if (null == parent && null != Page)
                {
                    int idx = Page.IndexOf(element as IStep);

                    if (idx > 0)
                    {
                        parent  = Page[idx - 1];
                        element = parent.LastOrDefault();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // try the system-palette
            colour = Palette.SystemPalette[colourValue];

            if (null != colour)
                return colour;

            // fallback: just return MainColour
            return Palette.SystemPalette[Palette.MainColour];
        }

        #endregion Colour

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphic"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <see cref="ColourValue"/> is set to <see cref="OverrideableColourValue"/> and each member of
        /// <see cref="Coordinates"/> is set to <see cref="OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        protected Graphic()
            : this(Palette.MainColour)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphic"/> class with the specified values.
        /// </summary>
        /// <exception cref="System.ArgumentException">The number of values in <paramref name="coordinates"/> does not equal
        ///     <see cref="CoordinatesCount"/></exception>
        /// <param name="colourValue">The <see cref="ColourValue"/> of the element.</param>
        /// <param name="coordinates">The <see cref="Coordinates"/> of the element.</param>
        protected Graphic(uint colourValue, IEnumerable<Vector3d> coordinates)
            : this(colourValue)
        {
            Coordinates = coordinates;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphic"/> class with the specified values.
        /// </summary>
        /// <param name="colourValue">The <see cref="ColourValue"/> of the element.</param>
        /// <remarks>
        /// Each member of <see cref="Coordinates"/> is set to <see cref="OpenTK.Vector3d.Zero"/>.
        /// </remarks>
        protected Graphic(uint colourValue)
        {
            ColourValue        = colourValue;
            _coordinates.Value = new Vector3d[CoordinatesCount];

            _colourValue.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (LDColour.IsDirectColour(ColourValue))
                {
                    UndoStack undoStack = UndoStack.CurrentStack;

                    if (null != undoStack)
                    {
                        // if this is occurring inside an undoable operation, we need to exclude the constructor
                        // otherwise the 'set value' will be added to the command
                        undoStack.SuspendCommand();
                        _directColour = new LDColour(ColourValue);
                        undoStack.ResumeCommand();
                    }
                    else
                    {
                        _directColour = new LDColour(ColourValue);
                    }

                    _directColour.Freeze();
                }
                else
                {
                    _directColour = null;
                }

                OnColourValueChanged(e);
                OnChanged(this, "ColourValueChanged", e);
            };

            _isVisible.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                OnIsVisibleChanged(e);
            };

            _isGhosted.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                OnIsGhostedChanged(e);
            };

            _coordinates.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Vector3d[]> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                _boundsDirty = true;
                _validated   = false;

                // convert the event-args here as the underlying UndoableProperty uses a different type to the public Property
                PropertyChangedEventArgs<IEnumerable<Vector3d>> args = new PropertyChangedEventArgs<IEnumerable<Vector3d>>(e.OldValue.ToArray(), e.NewValue.ToArray());

                OnCoordinatesChanged(args);
            };
        }

        #endregion Constructor

        #region Coordinates

        /// <inheritdoc />
        public IEnumerable<Vector3d> Coordinates
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _coordinates.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                Vector3d[] array = (Vector3d[])value.ToArray().Clone();

                if (CoordinatesCount != array.Length)
                    throw new ArgumentException();

                for (int i = 0; i < CoordinatesCount; i++)
                {
                    if (array[i] != _coordinates.Value[i])
                    {
                        _coordinates.Value = array;
                        break;
                    }
                }
            }
        }
        private UndoableProperty<Vector3d[]> _coordinates = new UndoableProperty<Vector3d[]>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IEnumerable<Vector3d>> CoordinatesChanged;

        /// <summary>
        /// Raises the <see cref="CoordinatesChanged"/> event.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Graphic"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before exiting.
        /// </note>
        /// </remarks>
        protected virtual void OnCoordinatesChanged(PropertyChangedEventArgs<IEnumerable<Vector3d>> e)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != CoordinatesChanged)
                CoordinatesChanged(this, e);

            OnChanged(this, "CoordinatesChanged", e);
        }

        /// <summary>
        /// Returns <see cref="Coordinates"/> as an array.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="Graphic"/> is
        ///     <see cref="Digitalis.LDTools.DOM.API.IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// This is provided as a convenience for subclasses which want to be able to access the coordinates by indexing. The
        /// members of the array must not be modified.
        /// </remarks>
        protected Vector3d[] CoordinatesArray
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _coordinates.Value;
            }
        }

        #endregion Coordinates

        #region Geometry

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: this returns the bounding-box formed by the union of the <see cref="Coordinates"/>. Subclasses
        /// may override this to provide their own implementation.
        /// </note>
        /// </remarks>
        public virtual Box3d BoundingBox
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (_boundsDirty)
                {
                    _boundsDirty = false;

                    if (CoordinatesCount > 0)
                    {
                        Vector3d[] coordinates = CoordinatesArray;

                        _bounds = new Box3d(coordinates[0], coordinates[0]);

                        for (int idx = 1; idx < coordinates.Length; idx++)
                        {
                            _bounds.Union(coordinates[idx]);
                        }
                    }
                    else
                    {
                        _bounds = new Box3d();
                    }
                }

                return _bounds;
            }
        }
        private Box3d _bounds;
        private bool  _boundsDirty = true;

        /// <inheritdoc />
        public abstract Vector3d Origin { get; }

        /// <inheritdoc />
        public CullingMode WindingMode
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return this.GetWindingMode(Page, Parent);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <note>
        /// Note to implementors: <see cref="Graphic"/> will transform each member of <see cref="Coordinates"/>. Subclasses may
        /// override this to perform their own transformations.
        /// </note>
        /// </remarks>
        public virtual void Transform(ref Matrix4d transform)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            Vector3d[] input  = CoordinatesArray;
            Vector3d[] output = new Vector3d[input.Length];

            for (int idx = 0; idx < output.Length; idx++)
            {
                Vector3d.Transform(ref input[idx], ref transform, out output[idx]);
            }

            Coordinates = output;
        }

        /// <inheritdoc />
        public abstract void ReverseWinding();

        #endregion Geometry

        #region Self-description

        /// <inheritdoc />
        public abstract uint CoordinatesCount { get; }

        /// <inheritdoc />
        public abstract uint OverrideableColourValue { get; }

        /// <inheritdoc />
        public abstract bool ColourValueEnabled { get; }

        #endregion Self-description
    }
}
