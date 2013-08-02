#region License

//
// LDWrite.cs
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
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IWrite"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [Export(typeof(IMetaCommand))]
    [MetaCommandPattern(@"^0\s+(WRITE|PRINT)(\s+.*)?\s*$")]
    [DefaultIcon(typeof(Resources), "WriteIcon")]
    [TypeName(typeof(Resources), "Write")]
    [ElementFlags(ElementFlags.HasEditor)]
    [ElementCategory(typeof(Resources), "ElementCategory_MetaCommand")]
    public sealed class LDWrite : MetaCommand, IWrite
    {
        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            ((IWrite)obj).Text = Text;
            base.InitializeObject(obj);
        }

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        /// <param name="sb">A <see cref="T:System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <param name="codeFormat">The format required for the returned code.</param>
        /// <param name="overrideColour">Not used.</param>
        /// <param name="transform">Not used.</param>
        /// <param name="winding">Not used.</param>
        protected override StringBuilder GenerateCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (CodeStandards.Full == codeFormat || CodeStandards.OfficialModelRepository == codeFormat)
            {
                if (String.IsNullOrWhiteSpace(Text))
                    sb.AppendFormat("0 WRITE{0}", LineTerminator);
                else
                    sb.AppendFormat("0 WRITE {0}{1}", Text, LineTerminator);
            }

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="LDWrite"/> class with default values.
        /// </summary>
        public LDWrite()
        {
            _text.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != TextChanged)
                    TextChanged(this, e);

                OnChanged(this, "TextChanged", e);
            };
        }

        /// <summary>
        /// Initializes an instance of the <see cref="LDWrite"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this <i>WRITE</i> or <i>PRINT</i> meta-command.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw <i>WRITE</i> or <i>PRINT</i> meta-command code.</exception>
        /// <example>
        /// <code>
        /// LDWrite write = new LDWrite("0 WRITE my message here");
        /// </code>
        /// </example>
        public LDWrite(string code)
            : this()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 2)
                throw new FormatException("LDraw WRITE/PRINT code must have at least 2 fields");

            if ("0" != fields[0])
                throw new FormatException("LDraw WRITE/PRINT code must start with '0'");

            if ("WRITE" != fields[1] && "PRINT" != fields[1])
                throw new FormatException("Unrecognised code");

            if (fields.Length > 2)
                Text = code.Substring(code.IndexOf(fields[2]));
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDWriteEditor", typeof(LDWrite));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDWrite"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDWrite"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        public string Text
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _text.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (null != Text && null != value && Text.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;

                if (null != value)
                {
                    value = value.Replace("\r", "").Replace("\n", "").TrimEnd();

                    if (0 == value.Length)
                        value = null;
                }

                if (value != Text)
                    _text.Value = value;
            }
        }
        private UndoableProperty<string> _text = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> TextChanged;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.WriteIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Write; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="P:Digitalis.LDTools.DOM.API.IWrite.Text"/>.
        /// </para>
        /// </remarks>
        public override string Description { get { return Text; } }

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
        /// <see cref="T:Digitalis.LDTools.DOM.API.IWrite"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IWrite"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
