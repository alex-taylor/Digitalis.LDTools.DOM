#region License

//
// LDComment.cs
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
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IComment"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [DefaultIcon(typeof(Resources), "CommentIcon")]
    [TypeName(typeof(Resources), "Comment")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDComment : Groupable, IComment
    {
        #region Inner types

        // Rule:   'Text' should start with '//'
        // Type:   Warning if mode is 'Full' or if mode is 'PartsLibrary' and page-type is 'Model'; Error otherwise
        // Source: http://www.ldraw.org/article/218.html#lt0, http://www.ldraw.org/article/512.html#metaheader
        private class MissingSlashesProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_MissingSlashes; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get; private set; }
            public string Description { get { return Resources.Analytics_Comment_MissingSlashes; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public MissingSlashesProblem(LDComment comment, Severity severity)
            {
                Element  = comment;
                Severity = severity;
                Fixes    = new IFixDescriptor[] { new Fix(comment) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_AddSlashes; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_Comment_AddSlashes; } }
                public bool IsIntraElement { get { return true; } }

                private LDComment _comment;
                private string _text;

                public Fix(LDComment comment)
                {
                    _comment = comment;
                    _text    = "// " + comment.Text;
                }

                public bool  Apply()
                {
                    _comment.Text = _text;
                    return true;
                }
            }
        }

        #endregion Inner types

        #region Analytics

        private static readonly Regex Regex_ValidFormat = new Regex(@"^\s*(//|!?[\p{Lu}_]{2,}\b)");
        private static readonly Regex Regex_Metacommand = new Regex(@"^\s*!?[\p{Lu}_]{2,}\b");

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsMissingSlashes"/> condition.
        /// </summary>
        public static readonly Guid Problem_MissingSlashes = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsMissingSlashes"/> condition.
        /// </summary>
        public static readonly Guid Fix_AddSlashes = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IComment.Text"/> is missing its leading slashes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It is recommended that comments start with two slashes (<c>'//'</c>) in order to clearly distinguish them from meta-commands.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_MissingSlashes"/>
        /// <seealso cref="Fix_AddSlashes"/>
        public bool IsMissingSlashes { get { return (null != Text && !Regex_ValidFormat.IsMatch(Text)); } }

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            return base.HasProblems(mode) || IsMissingSlashes;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/> of the <see cref="T:Digitalis.LDTools.DOM.Analytics.IProblemDescriptor"/>s returned varies by
        /// <paramref name="mode"/>:
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="Problem_MissingSlashes"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> otherwise
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (IsMissingSlashes)
            {
                Severity severity;

                if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
                    severity = Severity.Error;
                else
                    severity = Severity.Warning;

                problems.Add(new MissingSlashesProblem(this, severity));
            }

            return problems;
        }

        #endregion Analytics

        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            ((IComment)obj).Text = Text;
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
            if (String.IsNullOrWhiteSpace(Text))
                sb.AppendFormat("0{0}", LineTerminator);
            else
                sb.AppendFormat("0 {0}{1}", Text, LineTerminator);

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDComment"/> class with default values.
        /// </summary>
        public LDComment()
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
        /// Initializes a new instance of the <see cref="LDComment"/> class with the specified content.
        /// </summary>
        /// <param name="code">The LDraw code representing this comment.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw comment code.</exception>
        /// <example>
        /// <code>
        /// LDComment comment = new LDComment("0 some comment");
        /// </code>
        /// </example>
        public LDComment(string code)
            : this()
        {
            if ('0' != code[0])
                throw new FormatException("LDraw comment code must start with '0'");

            if (code.Length > 2)
                Text = code.Substring(2);
        }

        #endregion Constructor

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDCommentEditor", typeof(LDComment));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDComment"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDComment"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
                if (null != Text && null != value && Text.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return;

                if (null != value)
                {
                    value = value.Replace("\r", "").Replace("\n", "").TrimEnd();

                    if (0 == value.Length)
                        value = null;
                }

                if (null != value && Regex_Metacommand.IsMatch(value))
                    value = value.Trim();

                if (value != Text)
                    _text.Value = value;
            }
        }
        private UndoableProperty<string> _text = new UndoableProperty<string>();

        /// <inheritdoc />
        [field:NonSerialized]
        public event PropertyChangedEventHandler<string> TextChanged;

        /// <inheritdoc />
        public bool IsEmpty { get { return (String.IsNullOrWhiteSpace(Text) || Text == "//"); } }

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Comment; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.CommentIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Comment; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="P:Digitalis.LDTools.DOM.API.IComment.Text"/>.
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
        /// <see cref="T:Digitalis.LDTools.DOM.API.IComment"/> is not a state-element.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IComment"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description
    }
}
