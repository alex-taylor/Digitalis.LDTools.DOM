#region License

//
// LDStep.cs
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
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IStep"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [TypeName(typeof(Resources), "Step")]
    [DefaultIcon(typeof(Resources), "StepIcon")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDStep : ElementCollection, IStep
    {
        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IStep step = (IStep)obj;

            step.Mode = Mode;
            step.X    = X;
            step.Y    = Y;
            step.Z    = Z;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint colour, ref Matrix4d transform, WindingDirection winding)
        {
            sb = base.ToCode(sb, codeFormat, colour, ref transform, winding);

            IPage page = Page;

            if (CodeStandards.Full == codeFormat || (CodeStandards.OfficialModelRepository == codeFormat && (null == Page || PageType.Model == Page.PageType)))
            {
                if (IsLocalLock)
                    sb.AppendFormat("0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT{0}", LineTerminator);

                // optimisations
                if (0.0 == X && 0.0 == Y && 0.0 == Z)
                {
                    if (StepMode.Additive == Mode)
                    {
                        // if this is the last step in the page, suppress the output
                        if (null == page || page[page.Count - 1] != this)
                            sb.AppendFormat("0 STEP{0}", LineTerminator);
                    }
                    else if (StepMode.Relative == Mode)
                    {
                        sb.AppendFormat("0 ROTSTEP END{0}", LineTerminator);
                    }
                }
                else
                {
                    switch (Mode)
                    {
                        case StepMode.Absolute:
                            sb.AppendFormat("0 ROTSTEP {0} {1} {2} ABS{3}", X, Y, Z, LineTerminator);
                            break;

                        case StepMode.Additive:
                            sb.AppendFormat("0 ROTSTEP {0} {1} {2} ADD{3}", X, Y, Z, LineTerminator);
                            break;

                        case StepMode.Relative:
                            sb.AppendFormat("0 ROTSTEP {0} {1} {2} REL{3}", X, Y, Z, LineTerminator);
                            break;

                        case StepMode.Reset:
                            sb.AppendFormat("0 ROTSTEP END{0}", LineTerminator);
                            break;
                    }
                }
            }

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="LDStep"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IStep.Mode"/> will be set to <see cref="Digitalis.LDTools.DOM.API.StepMode.Relative"/> and
        /// <see cref="P:Digitalis.LDTools.DOM.API.IStep.X"/>, <see cref="P:Digitalis.LDTools.DOM.API.IStep.Y"/> and <see cref="P:Digitalis.LDTools.DOM.API.IStep.Z"/>
        /// will be set to <c>0.0</c>.
        /// </para>
        /// </remarks>
        public LDStep()
        {
            _mode.ValueChanged += delegate(object sender, PropertyChangedEventArgs<StepMode> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != ModeChanged)
                    ModeChanged(this, e);

                OnChanged(this, "ModeChanged", e);
            };

            _x.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != XChanged)
                    XChanged(this, e);

                OnChanged(this, "XChanged", e);
            };

            _y.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != YChanged)
                    YChanged(this, e);

                OnChanged(this, "YChanged", e);
            };

            _z.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != ZChanged)
                    ZChanged(this, e);

                OnChanged(this, "ZChanged", e);
            };
        }

        /// <summary>
        /// Initializes an instance of the <see cref="LDStep"/> class with the specified values.
        /// </summary>
        /// <param name="mode">The operation-mode.</param>
        /// <param name="x">The angle of rotation around the X-axis, in degrees.</param>
        /// <param name="y">The angle of rotation around the Y-axis, in degrees.</param>
        /// <param name="z">The angle of rotation around the Z-axis, in degrees.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">One of the supplied angles was less than <c>-360.0</c> or greater than <c>360.0</c>.</exception>
        public LDStep(StepMode mode, double x, double y, double z)
            : this()
        {
            Mode = mode;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="LDStep"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this <i>STEP</i> or <i>ROTSTEP</i> meta-command.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw <i>STEP</i> or <i>ROTSTEP</i> meta-command code.</exception>
        /// <example>
        /// <code>
        /// LDStep step = new LDStep("0 STEP");
        /// LDStep rotStep = new LDStep("0 ROTSTEP 45.0 0 0 REL");
        /// </code>
        /// </example>
        public LDStep(string code)
            : this()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 2)
                throw new FormatException("LDraw STEP/ROTSTEP code must have at least 2 fields");

            if ("0" != fields[0])
                throw new FormatException("LDraw STEP/ROTSTEP code must start with '0'");

            if ("STEP" == fields[1])
            {
                Mode = StepMode.Additive;
                X = 0.0;
                Y = 0.0;
                Z = 0.0;
            }
            else if ("ROTSTEP" == fields[1])
            {
                if (fields.Length < 3)
                    throw new FormatException("LDraw ROTSTEP code must have at least 3 fields");

                if (fields.Length < 5)
                {
                    if ("END" != fields[2])
                        throw new FormatException("Unrecognised code '" + code + "'");

                    Mode = StepMode.Reset;
                }
                else
                {
                    if (fields.Length >= 6)
                    {
                        switch (fields[5])
                        {
                            case "REL":
                                Mode = StepMode.Relative;
                                break;

                            case "ABS":
                                Mode = StepMode.Absolute;
                                break;

                            case "ADD":
                                Mode = StepMode.Additive;
                                break;

                            default:
                                throw new FormatException("Unrecognised code '" + code + "'");
                        }
                    }
                    else
                    {
                        Mode = StepMode.Relative;
                    }

                    double x = double.Parse(fields[2], CultureInfo.InvariantCulture);
                    double y = double.Parse(fields[3], CultureInfo.InvariantCulture);
                    double z = double.Parse(fields[4], CultureInfo.InvariantCulture);

                    X = Math.IEEERemainder(x, 360.0);
                    Y = Math.IEEERemainder(y, 360.0);
                    Z = Math.IEEERemainder(z, 360.0);
                }
            }
            else
            {
                throw new FormatException("Unrecognised code '" + code + "'");
            }
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && null != Page && Page.IsFrozen)
                throw new ObjectFrozenException();

            base.Dispose(disposing);

            if (disposing && !IsFrozen && null != Page && !Page.IsDisposing)
                ((IStep)this).Page = null;
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        public override IElementCollection Parent { get { return null; } }

        /// <inheritdoc />
        public override IStep Step { get { return null; } }

        /// <inheritdoc />
        public override IPage Page { get { return _page; } }

        /// <inheritdoc />
        IPage IStep.Page
        {
            get { return _page; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != Page && null != value)
                    throw new InvalidOperationException("Cannot set Page: " + InsertCheckResult.AlreadyMember);

                if (value == Page)
                    return;

                if (null == value)
                {
                    // removing
                    if (Page.Contains(this))
                    {
                        // being set directly
                        Page.Remove(this);
                    }
                    else
                    {
                        // being set by a call to IPage.Remove()
                        PropertyChangedEventArgs<IPage> args = new PropertyChangedEventArgs<IPage>(Page, null);

                        _page = null;
                        OnPageChanged(args);
                    }
                }
                else
                {
                    // adding
                    if (value.Contains(this))
                    {
                        // being set by a call to IPage.Insert()
                        PropertyChangedEventArgs<IPage> args = new PropertyChangedEventArgs<IPage>(Page, value);

                        _page = value;
                        OnPageChanged(args);
                    }
                    else
                    {
                        // being set directly; this will do the CanInsert() checks
                        value.Add(this);
                    }
                }
            }
        }
        [NonSerialized]
        private IPage _page;

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IPage> PageChanged;

        private void OnPageChanged(PropertyChangedEventArgs<IPage> e)
        {
            if (null != e.OldValue)
                e.OldValue.PathToDocumentChanged -= OnPathToDocumentChanged;

            if (null != e.NewValue)
                e.NewValue.PathToDocumentChanged += OnPathToDocumentChanged;

            if (!IsDisposing)
            {
                if (null != PageChanged)
                    PageChanged(this, e);

                OnChanged(this, "PageChanged", e);
                OnPathToDocumentChanged(this, EventArgs.Empty);
            }
        }

        private void OnPathToDocumentChanged(object sender, EventArgs e)
        {
            OnPathToDocumentChanged(e);
        }

        #endregion Document-tree

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDStepEditor", typeof(LDStep));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDStep"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (null != EditorFactory);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDStep"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != EditorFactory)
                return EditorFactory.Invoke(new object[] { this }) as IElementEditor;

            return null;
        }

        #endregion Editor

        #region Geometry

        /// <inheritdoc />
        public override CullingMode WindingMode
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                IPage page = Page;

                if (null == page)
                    return CullingMode.NotSet;

                if (CullingMode.Disabled == page.BFC)
                    return CullingMode.Disabled;

                // the page is either Certified or NotSet, so we need to walk the document-tree to find the last BFCFlag prior to us
                IBFCFlag    bfcFlag;
                CullingMode mode    = Page.BFC;
                bool        isSet   = (CullingMode.NotSet != mode);
                bool        enabled = (CullingMode.CertifiedClockwise == mode || CullingMode.CertifiedCounterClockwise == mode);

                foreach (IStep step in page)
                {
                    if (step == this)
                    {
                        if (isSet)
                            return (enabled) ? mode : CullingMode.Disabled;

                        return CullingMode.NotSet;
                    }

                    foreach (IElement element in step)
                    {
                        bfcFlag = element as IBFCFlag;

                        if (null != bfcFlag)
                        {
                            switch (bfcFlag.Flag)
                            {
                                case BFCFlag.SetWindingModeClockwise:
                                    mode = CullingMode.CertifiedClockwise;
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise:
                                    mode    = CullingMode.CertifiedClockwise;
                                    enabled = true;
                                    break;

                                case BFCFlag.SetWindingModeCounterClockwise:
                                    mode = CullingMode.CertifiedCounterClockwise;
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise:
                                    mode    = CullingMode.CertifiedCounterClockwise;
                                    enabled = true;
                                    break;

                                case BFCFlag.EnableBackFaceCulling:
                                    enabled = true;
                                    break;

                                case BFCFlag.DisableBackFaceCulling:
                                    enabled = false;
                                    break;
                            }
                        }
                    }
                }

                // didn't find an explicit flag, so go with the page's setting
                return page.BFC;
            }
        }

        #endregion Geometry

        #region Properties

        /// <inheritdoc />
        public StepMode Mode
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _mode.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (!Enum.IsDefined(typeof(StepMode), value))
                    throw new ArgumentOutOfRangeException();

                if (_mode.Value != value)
                    _mode.Value = value;
            }
        }
        private UndoableProperty<StepMode> _mode = new UndoableProperty<StepMode>(StepMode.Additive);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<StepMode> ModeChanged;

        /// <inheritdoc />
        public double X
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _x.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (value < -360.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException();

                if (_x.Value != value)
                    _x.Value = value;
            }
        }
        private UndoableProperty<double> _x = new UndoableProperty<double>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<double> XChanged;

        /// <inheritdoc />
        public double Y
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _y.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (value < -360.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException();

                if (_y.Value != value)
                    _y.Value = value;
            }
        }
        private UndoableProperty<double> _y = new UndoableProperty<double>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<double> YChanged;

        /// <inheritdoc />
        public double Z
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _z.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (value < -360.0 || value > 360.0)
                    throw new ArgumentOutOfRangeException();

                if (_z.Value != value)
                    _z.Value = value;
            }
        }
        private UndoableProperty<double> _z = new UndoableProperty<double>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<double> ZChanged;

        /// <inheritdoc />
        public Matrix4d StepTransform
        {
            get
            {
                Matrix4d matrix = Matrix4d.Identity;

                if (StepMode.Reset != Mode)
                {
                    // MLCad's implementation of this has an error: it does its calculations in OpenGL coordinate-space, not LDraw,
                    // so we need to invert the Y and Z angles in order to generate the correct transform
                    if (0.0 != Z)
                        matrix *= Matrix4d.RotateZ(Math.PI * -Z / 180.0);

                    if (0.0 != Y)
                        matrix *= Matrix4d.RotateY(Math.PI * -Y / 180.0);

                    if (0.0 != X)
                        matrix *= Matrix4d.RotateX(Math.PI * X / 180.0);
                }

                return matrix;
            }
        }

        #endregion

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Step; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.StepIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Step; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns a description of the effect the <see cref="LDStep"/> will have on the view-transform.
        /// </para>
        /// </remarks>
        public override string Description
        {
            get
            {
                switch (Mode)
                {
                    case StepMode.Absolute:
                        return String.Format(Resources.Step_Absolute, X, Y, Z);

                    case StepMode.Additive:
                        if (0.0 == X && 0.0 == Y && 0.0 == Z)
                            return String.Empty;

                        return String.Format(Resources.Step_Additive, X, Y, Z);

                    case StepMode.Relative:
                        if (0.0 == X && 0.0 == Y && 0.0 == Z)
                            return Resources.Step_Reset;

                        return String.Format(Resources.Step_Relative, X, Y, Z);

                    case StepMode.Reset:
                        return Resources.Step_Reset;
                }

                return String.Empty;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        /// <inheritdoc />
        public override bool AllowsTopLevelElements { get { return true; } }

        /// <inheritdoc />
        public override bool IsReadOnly { get { return false; } }

        #endregion Self-description

        #region ViewTransform

        /// <inheritdoc />
        public void GetViewTransform(ref Matrix4d initialTransform, out Matrix4d viewTransform, ref bool isAbsolute)
        {
            IEnumerable<IStep> steps;
            IPage page = Page;

            if (null != page)
                steps = page;
            else
                steps = new IStep[] { this };

            Matrix4d matrix                 = initialTransform;
            bool initialTransformIsAbsolute = isAbsolute;

            foreach (IStep step in steps)
            {
                switch (step.Mode)
                {
                    case StepMode.Absolute:
                        matrix     = step.StepTransform;
                        isAbsolute = true;
                        break;

                    case StepMode.Relative:
                        matrix     = step.StepTransform;
                        isAbsolute = false;
                        break;

                    case StepMode.Additive:
                        // not clear why it's this way round, but this is what MLCad does and we need to be compatible
                        matrix = step.StepTransform * matrix;
                        break;

                    case StepMode.Reset:
                        matrix     = Matrix4d.Identity;
                        isAbsolute = false;
                        break;
                }

                if (step == this)
                    break;
            }

            viewTransform = matrix;
        }

        #endregion
    }
}
