#region License

//
// LDBFCFlag.cs
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
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.Reflection;
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IBFCFlag"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [Export(typeof(IMetaCommand))]
    [MetaCommandPattern(@"^0\s+BFC\s+(CLIP|CLIP\s+(CW|CCW)|NOCLIP|CW|CCW|(CW|CCW)\s+CLIP)\s*$")]
    [DefaultIcon(typeof(Resources), "BFCFlagIcon")]
    [TypeName(typeof(Resources), "BFCFlag")]
    [ElementFlags(ElementFlags.HasEditor)]
    [ElementCategory(typeof(Resources), "ElementCategory_MetaCommand")]
    public sealed class LDBFCFlag : MetaCommand, IBFCFlag
    {
        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            ((IBFCFlag)obj).Flag = Flag;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        /// <param name="sb">A <see cref="T:System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <param name="codeFormat">Not used.</param>
        /// <param name="overrideColour">Not used.</param>
        /// <param name="transform">Not used.</param>
        /// <param name="winding">Not used.</param>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            sb.Append("0 BFC ");

            switch (Flag)
            {
                case BFCFlag.SetWindingModeClockwise:
                    sb.AppendFormat("CW{0}", LineTerminator);
                    break;

                case BFCFlag.SetWindingModeCounterClockwise:
                    sb.AppendFormat("CCW{0}", LineTerminator);
                    break;

                case BFCFlag.EnableBackFaceCulling:
                    sb.AppendFormat("CLIP{0}", LineTerminator);
                    break;

                case BFCFlag.DisableBackFaceCulling:
                    sb.AppendFormat("NOCLIP{0}", LineTerminator);
                    break;

                case BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise:
                    sb.AppendFormat("CLIP CW{0}", LineTerminator);
                    break;

                case BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise:
                    sb.AppendFormat("CLIP CCW{0}", LineTerminator);
                    break;
            }

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDBFCFlag"/> class with default values.
        /// </summary>
        public LDBFCFlag()
        {
            _flag.ValueChanged += delegate(object sender, PropertyChangedEventArgs<BFCFlag> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (!Enum.IsDefined(typeof(BFCFlag), e.NewValue))
                    throw new ArgumentOutOfRangeException();

                if (null != FlagChanged)
                    FlagChanged(this, e);

                OnChanged(this, "FlagChanged", e);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDBFCFlag"/> class with the specified values.
        /// </summary>
        /// <param name="flag">The back-face culling flag.</param>
        public LDBFCFlag(BFCFlag flag)
            : this()
        {
            Flag = flag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDBFCFlag"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this BFC flag.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw BFC code.</exception>
        /// <example>
        /// <code>
        /// LDBFCFlag bfcFlag = new LDBFCFlag("0 BFC CLIP CCW");
        /// </code>
        /// </example>
        public LDBFCFlag(string code)
            : this()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 3)
                throw new FormatException("LDraw BFCFlag code must have at least three fields");

            if ("0" != fields[0])
                throw new FormatException("LDraw BFCFlag code must start with '0'");

            if ("BFC" != fields[1])
                throw new FormatException("Field 1 must be 'BFC'");

            switch (fields[2])
            {
                case "CLIP":
                    Flag = BFCFlag.EnableBackFaceCulling;
                    break;

                case "NOCLIP":
                    Flag = BFCFlag.DisableBackFaceCulling;
                    return;

                case "CW":
                    Flag = BFCFlag.SetWindingModeClockwise;
                    break;

                case "CCW":
                    Flag = BFCFlag.SetWindingModeCounterClockwise;
                    break;

                default:
                    throw new FormatException("Unrecognised code '" + code + "'");
            }

            if (fields.Length > 3 && BFCFlag.DisableBackFaceCulling != Flag)
            {
                switch (fields[3])
                {
                    case "CW":
                        Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise;
                        break;

                    case "CCW":
                        Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise;
                        break;

                    case "CLIP":
                        if (BFCFlag.SetWindingModeClockwise == Flag)
                            Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise;
                        else if (BFCFlag.SetWindingModeCounterClockwise == Flag)
                            Flag = BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise;
                        break;

                    default:
                        throw new FormatException("Unrecognised code '" + code + "'");
                }
            }
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDBFCFlagEditor", typeof(LDBFCFlag));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDBFCFlag"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDBFCFlag"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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

        #region Properties

        /// <inheritdoc />
        public BFCFlag Flag
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _flag.Value;
            }
            set
            {
                if (Flag != value)
                    _flag.Value = value;
            }
        }
        private UndoableProperty<BFCFlag> _flag = new UndoableProperty<BFCFlag>(BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<BFCFlag> FlagChanged;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.BFCFlagIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.BFCFlag; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns a string describing <see cref="P:Digitalis.LDTools.DOM.API.IBFCFlag.Flag"/>.
        /// </para>
        /// </remarks>
        public override string Description { get { return Resources.ResourceManager.GetString(Flag.ToString()); } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IBFCFlag"/> is a state-element: the values it defines affect how any <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/>s
        /// which follow it in the containing <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> are rendered.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return true; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IBFCFlag"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
