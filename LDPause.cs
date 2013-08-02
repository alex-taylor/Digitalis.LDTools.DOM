#region License

//
// LDPause.cs
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
    using System.Text;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IPause"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [Export(typeof(IMetaCommand))]
    [MetaCommandPattern(@"^0\s+PAUSE\s*$")]
    [DefaultIcon(typeof(Resources), "PauseIcon")]
    [TypeName(typeof(Resources), "Pause")]
    [ElementCategory(typeof(Resources), "ElementCategory_MetaCommand")]
    public sealed class LDPause : MetaCommand, IPause
    {
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
                sb.AppendFormat("0 PAUSE{0}", LineTerminator);

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes an instance of the <see cref="LDPause"/> class with default values.
        /// </summary>
        public LDPause()
        {
        }

        /// <summary>
        /// Initializes an instance of the <see cref="LDPause"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this <i>PAUSE</i> meta-command.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw <i>PAUSE</i> meta-command code.</exception>
        /// <example>
        /// <code>
        /// LDPause pause = new LDPause("0 CLEAR");
        /// </code>
        /// </example>
        public LDPause(string code)
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 2)
                throw new FormatException("LDraw PAUSE code must have 2 fields");

            if ("0" != fields[0])
                throw new FormatException("LDraw PAUSE code must start with '0'");

            if ("PAUSE" != fields[1])
                throw new FormatException("Unrecognised code");
        }

        #endregion

        #region Editor

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDPause"/> elements do not have editable parameters, so do not provide an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/>.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return false;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDPause"/> elements do not have editable parameters, so do not provide an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/>.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            return null;
        }

        #endregion Editor

        #region Self-description

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.PauseIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Pause; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public override string Description { get { return String.Empty; } }

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
        /// <see cref="T:Digitalis.LDTools.DOM.API.IPause"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IPause"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
