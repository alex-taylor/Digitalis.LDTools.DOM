#region License

//
// LDColour.cs
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
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.Globalization;
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
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IColour"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [Export(typeof(IMetaCommand))]
    [MetaCommandPattern(@"^0\s+!COLOUR\s+\S+\s+CODE\s+\d+\s+VALUE\s+")]
    [MetaCommandPattern(@"^0\s+COLO(U)?R\s+\d+\s+\S+(\s+\d+){9}\s*$")]
    [DefaultIcon(typeof(Resources), "ColourIcon")]
    [TypeName(typeof(Resources), "ColourDefinition")]
    [ElementFlags(ElementFlags.HasEditor)]
    [ElementCategory(typeof(Resources), "ElementCategory_MetaCommand")]
    public sealed class LDColour : MetaCommand, IColour
    {
        #region Inner types

        // for sorting the list of Materials
        private class MaterialsComparer : Comparer<Type>
        {
            public override int Compare(Type x, Type y)
            {
                if (x == y)
                    return 0;

                IMaterial first = Activator.CreateInstance(x) as IMaterial;
                IMaterial second = Activator.CreateInstance(y) as IMaterial;

                if (first is PlasticMaterial || second is UnknownMaterial)
                    return -1;

                if (first is UnknownMaterial || second is PlasticMaterial)
                    return 1;

                return first.Description.CompareTo(second.Description);
            }
        }

        // Rule:   'Name' may only contain [A-Za-z0-9_] and the first character must be [A-Z]
        // Type:   Error
        // Source: http://www.ldraw.org/article/299.html
        private class InvalidNameProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_InvalidName; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_Colour_InvalidName; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public InvalidNameProblem(LDColour colour)
            {
                Element = colour;
            }
        }

        // Rule:   'EdgeCode' must be either a DirectColours value or the Code of an entry in SystemPalette or the local-palette
        // Type:   Warning
        // Source: http://www.ldraw.org/article/299.html
        private class InvalidEdgeProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_InvalidEdge; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_Colour_InvalidEdge; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public InvalidEdgeProblem(LDColour colour)
            {
                Element = colour;
            }
        }

        #endregion Inner types

        #region Analytics

        // Regex for checking the validity of the Name property
        private static readonly Regex NameRegex = new Regex("^[A-Za-z][A-Za-z0-9_]*$");

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsNameInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_InvalidName = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsEdgeInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_InvalidEdge = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether <see cref="Name"/> is correctly formatted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> is not a <i>Direct Colours</i> value, <see cref="P:Digitalis.LDTools.DOM.API.IColour.Name"/>
        /// should contain only upper- and lower-case letters A-Z, digits 0-9 and the underscore ('_') character, and should start with a letter.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_InvalidName"/>
        public bool IsNameInvalid { get { return !IsDirectColour(Code) && !NameRegex.IsMatch(Name); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="EdgeCode"/> is valid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.EdgeCode"/> should be either a <i>Direct Colours</i> value or else the
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> of either a value in <see cref="P:Digitalis.LDTools.DOM.Palette.SystemPalette"/> or another
        /// <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> in the <see cref="P:Digitalis.LDTools.DOM.API.IPage"/>.
        /// </para>
        /// </remarks>
        public bool IsEdgeInvalid
        {
            get
            {
                uint colourValue = EdgeCode;

                if (IsDirectColour(colourValue))
                    return false;

                IColour c;
                IPage page                   = Page;
                IStep step                   = Step;
                IElement element             = this;
                IElementCollection container = Parent;
                int stepIdx;

                if (null != page && null != step)
                    stepIdx = page.IndexOf(step);
                else stepIdx = -1;

                if (null == container)
                    container = step;

                // try the document-tree first
                while (null != container)
                {
                    if (container.ContainsColourElements)
                    {
                        int index = container.IndexOf(element);

                        for (int i = index - 1; i >= 0; i--)
                        {
                            c = container[i] as IColour;

                            if (null != c && c.Code == colourValue)
                                return false;
                        }
                    }

                    if (null != container.Parent)
                    {
                        element = container as IElement;
                        container = container.Parent;

                        if (null == container)
                            container = element.Step;
                    }
                    else
                    {
                        if (stepIdx <= 0)
                            break;

                        container = page[--stepIdx];
                        element = container[container.Count - 1];
                    }
                }

                // try the system-palette
                return (null == Palette.SystemPalette[colourValue]);
            }
        }

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            return base.HasProblems(mode) || IsNameInvalid || IsEdgeInvalid;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="Problem_InvalidName"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///     <term><see cref="Problem_InvalidEdge"/></term>
        ///     <description><see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/></description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (IsNameInvalid)
                problems.Add(new InvalidNameProblem(this));

            if (IsEdgeInvalid)
                problems.Add(new InvalidEdgeProblem(this));

            return problems;
        }

        #endregion Analytics

        #region Cloning

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IColour colour = (IColour)obj;

            colour.Name      = Name;
            colour.Code      = Code;
            colour.EdgeCode  = EdgeCode;
            colour.Value     = Value;
            colour.Luminance = Luminance;
            colour.Material  = Material.Clone();
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
                Color value = Value;

                sb.AppendFormat("0 !COLOUR {0} CODE {1} VALUE #{2:X2}{3:X2}{4:X2} EDGE ", Name, Code, value.R, value.G, value.B);

                if (IsDirectColour(EdgeCode))
                {
                    Color edge = EdgeValue;

                    sb.AppendFormat("#{0:X2}{1:X2}{2:X2}", edge.R, edge.G, edge.B);
                }
                else
                {
                    sb.Append(EdgeCode);
                }

                if (0xFF != value.A)
                    sb.AppendFormat(" ALPHA {0}", value.A);

                if (0 != Luminance)
                    sb.AppendFormat(" LUMINANCE {0}", Luminance);

                sb = Material.ToCode(sb);
                sb.Append(LineTerminator);
            }

            return sb;
        }

        #endregion Code-generation

        #region Constructor

        private static List<Type> Materials;

        static LDColour()
        {
            Materials materials = new Materials();

            Materials = new List<Type>(materials.AvailableMaterials);

            // sort the list so that Plastic is at the start (it's the most common) and Unknown is at the end (it's the catch-all)
            Materials.Sort(new MaterialsComparer());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDColour"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The colour's <see cref="P:Digitalis.LDTools.DOM.API.IColour.Name"/> is set to <c>"&lt;unknown&gt;"</c>, <see cref="P:Digitalis.LDTools.DOM.API.IColour.Code"/> is
        /// set to <c>0</c>, <see cref="P:Digitalis.LDTools.DOM.API.IColour.EdgeCode"/> is set to <see cref="F:Digitalis.LDTools.DOM.Palette.EdgeColour"/>,
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.Value"/> is set to <see cref="F:System.Drawing.Color.Black"/>, <see cref="P:Digitalis.LDTools.DOM.API.IColour.Luminance"/>
        /// is set to <c>0</c> and <see cref="P:Digitalis.LDTools.DOM.API.IColour.Material"/> is set to <see cref="T:Digitalis.LDTools.DOM.PlasticMaterial"/>.
        /// </para>
        /// </remarks>
        public LDColour()
            : this(Resources.Unknown, Palette.MainColour, Color.Black, Palette.EdgeColour, 0, new PlasticMaterial())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDColour"/> class with the specified values.
        /// </summary>
        /// <param name="directColourValue">A <i>Direct Colours</i> value.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="directColourValue"/> was not a <i>Direct Colours</i> value.</exception>
        /// <remarks>
        /// <para>
        /// <i>Direct Colours</i> values are as defined in <see href="http://www.ldraw.org/article/218.html#colours">the LDraw.org File Format specification</see>,
        /// with the addition of support for transparent and 12-bit RGB values as defined by <see href="http://ldlite.sourceforge.net/">LDLite</see>.
        /// </para>
        /// <para>
        /// Dithered <i>Direct Colours</i> values will be converted to 24-bit RGB. <see cref="P:Digitalis.LDTools.DOM.API.IColour.EdgeCode"/> will be set to <c>0x2000000</c> (black).
        /// </para>
        /// </remarks>
        public LDColour(uint directColourValue)
            : base()
        {
            if (!IsDirectColour(directColourValue))
                throw new ArgumentException("directColourValue must be a Direct Colours value; was '" + directColourValue + "'");

            SetEventHandlers();

            // convert 12-bit values to 24-bit
            Color c           = (Color)ConvertDirectColourToRGB(directColourValue);
            directColourValue = ConvertRGBToDirectColour(c);

            Code      = directColourValue;
            EdgeCode  = ConvertRGBToDirectColour(Color.Black);
            Value     = c;
            Luminance = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDColour"/> class with the specified values.
        /// </summary>
        /// <param name="name">The name for the <see cref="LDColour"/>. This should be in English.</param>
        /// <param name="code">The code for the <see cref="LDColour"/>.</param>
        /// <param name="value">The main colour for the <see cref="LDColour"/>.</param>
        /// <param name="edgeValue">The edge colour for the <see cref="LDColour"/>.</param>
        /// <param name="luminance">The luminance value for the <see cref="LDColour"/>.</param>
        /// <param name="material">The material for the <see cref="LDColour"/>.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="code"/> was a <i>Direct Colours</i> value.</exception>
        public LDColour(string name, uint code, Color value, Color edgeValue, byte luminance, Material material)
            : this(name, code, value, ConvertRGBToDirectColour(edgeValue), luminance, material)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDColour"/> class with the specified values.
        /// </summary>
        /// <param name="name">The name for the <see cref="LDColour"/>. This should be in English.</param>
        /// <param name="code">The code for the <see cref="LDColour"/>.</param>
        /// <param name="value">The main colour for the <see cref="LDColour"/>.</param>
        /// <param name="edgeCode">The code of the edge colour for the <see cref="LDColour"/>.</param>
        /// <param name="luminance">The luminance value for the <see cref="LDColour"/>.</param>
        /// <param name="material">The material for the <see cref="LDColour"/>.</param>
        /// <exception cref="T:System.ArgumentException"><paramref name="code"/> was a <i>Direct Colours</i> value.</exception>
        public LDColour(string name, uint code, Color value, uint edgeCode, byte luminance, Material material)
            : base()
        {
            if (IsDirectColour(code))
                throw new ArgumentException("code may not be a Direct Colours value; was '" + value + "'");

            SetEventHandlers();

            Name      = name;
            Code      = code;
            EdgeCode  = edgeCode;
            Value     = value;
            Luminance = luminance;
            Material  = material;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDColour"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw or <see href="http://ldlite.sourceforge.net/">LDLite</see> code representing this colour.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw or <see href="http://ldlite.sourceforge.net/">LDLite</see> colour code.</exception>
        /// <remarks>
        /// <para>
        /// The official LDraw <i>!COLOUR</i> and the unofficial <see href="http://ldlite.sourceforge.net/">LDLite</see> <i>COLOUR</i>/<i>COLOR</i>
        /// meta-commands are both supported.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// LDColour colour = new LDColour("0 !COLOUR Black CODE 0 VALUE #212121 EDGE #595959");      // LDraw format
        /// LDColour colour = new LDColour("0 COLOR 0 Black 0 33 33 33 255 33 33 33 255");            // LDLite format
        /// ]]>
        /// </code>
        /// </example>
        public LDColour(string code)
            : base()
        {
            string[] fields = code.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if ("0" != fields[0])
                throw new FormatException("LDraw colour code must start with '0'");

            SetEventHandlers();

            if ("!COLOUR" == fields[1])
            {
                if (fields.Length < 9)
                    throw new FormatException("LDraw colour code must have at least 9 fields");

                if ("CODE" != fields[3].ToUpper())
                    throw new FormatException("Field 3 must be 'CODE'");

                if ("VALUE" != fields[5].ToUpper())
                    throw new FormatException("Field 5 must be 'VALUE'");

                if ("EDGE" != fields[7].ToUpper())
                    throw new FormatException("Field 7 must be 'EDGE'");

                Name = fields[2];
                Code = uint.Parse(fields[4]);

                int alpha = 0xFF;

                for (int n = 9; n < fields.Length; n += 2)
                {
                    switch (fields[n])
                    {
                        case "ALPHA":
                            alpha = int.Parse(fields[n + 1]);
                            break;

                        case "LUMINANCE":
                            Luminance = byte.Parse(fields[n + 1]);
                            break;

                        default:
                            // anything we don't recognise is assumed to be a Material definition and terminates the parse
                            Material = GetMaterial(code.Substring(code.IndexOf(fields[n])));
                            n = fields.Length;
                            break;
                    }
                }

                // main colour
                if ('#' == fields[6][0])
                    Value = Color.FromArgb(alpha << 24 | int.Parse(fields[6].Substring(1), NumberStyles.HexNumber));
                else if (fields[6].StartsWith("0x") || fields[6].StartsWith("0X"))
                    Value = Color.FromArgb(alpha << 24 | int.Parse(fields[6].Substring(2), NumberStyles.HexNumber));
                else
                    throw new FormatException("Field 6 must be a number");

                // edge colour
                if ('#' == fields[8][0])
                    EdgeCode = DirectColourOpaque | uint.Parse(fields[8].Substring(1), NumberStyles.HexNumber);
                else if (fields[8].StartsWith("0x") || fields[8].StartsWith("0X"))
                    EdgeCode = DirectColourOpaque | uint.Parse(fields[8].Substring(2), NumberStyles.HexNumber);
                else
                    EdgeCode = uint.Parse(fields[8]);
            }
            else if ("COLOUR" == fields[1] || "COLOR" == fields[1])
            {
                if (fields.Length < 13)
                    throw new FormatException("LDLite colour code must have 13 fields");

                Code = uint.Parse(fields[2]);
                Name = fields[3];
                EdgeCode = uint.Parse(fields[4]);

                // we blend the two colours rather than dithering them
                Value = Color.FromArgb((IntHelper.ParseInt(fields[8]) + IntHelper.ParseInt(fields[12])) / 2,
                                         (IntHelper.ParseInt(fields[5]) + IntHelper.ParseInt(fields[9])) / 2,
                                         (IntHelper.ParseInt(fields[6]) + IntHelper.ParseInt(fields[10])) / 2,
                                         (IntHelper.ParseInt(fields[7]) + IntHelper.ParseInt(fields[11])) / 2);

                Luminance = 0;
            }
            else
            {
                throw new FormatException("Unrecognised code");
            }
        }

        private Material GetMaterial(string code)
        {
            foreach (Type t in Materials)
            {
                MetaCommandPatternAttribute pattern = Attribute.GetCustomAttribute(t, typeof(MetaCommandPatternAttribute)) as MetaCommandPatternAttribute;

                if (null != pattern)
                {
                    Regex regex = new Regex(pattern.Pattern);

                    if (regex.IsMatch(code))
                        return Activator.CreateInstance(t, code) as Material;
                }
            }

            return new UnknownMaterial(code);
        }

        private void SetEventHandlers()
        {
            _name.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != NameChanged)
                    NameChanged(this, e);

                OnChanged(this, "NameChanged", e);
            };

            _code.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (IsDirectColour(Code))
                    Value = (Color)ConvertDirectColourToRGB(Code);

                if (null != CodeChanged)
                    CodeChanged(this, e);

                OnChanged(this, "CodeChanged", e);
            };

            _edgeCode.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (IsDirectColour(EdgeCode))
                    _directEdgeColour = (Color)ConvertDirectColourToRGB(EdgeCode);

                if (null != EdgeCodeChanged)
                    EdgeCodeChanged(this, e);

                OnChanged(this, "EdgeCodeChanged", e);
            };

            _value.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Color> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != ValueChanged)
                    ValueChanged(this, e);

                OnChanged(this, "ValueChanged", e);
            };

            _luminance.ValueChanged += delegate(object sender, PropertyChangedEventArgs<byte> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != LuminanceChanged)
                    LuminanceChanged(this, e);

                OnChanged(this, "LuminanceChanged", e);
            };

            _material.ValueChanged += delegate(object sender, PropertyChangedEventArgs<IMaterial> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                e.OldValue.Colour = null;
                e.OldValue.Changed -= OnMaterialChanged;
                e.NewValue.Colour   = this;
                e.NewValue.Changed += OnMaterialChanged;

                if (null != MaterialChanged)
                    MaterialChanged(this, e);

                OnChanged(this, "MaterialChanged", e);
            };
        }

        private void OnMaterialChanged(object sender, EventArgs e)
        {
            OnChanged(this, "MaterialChanged", e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && null != Material)
                Material.Changed -= OnMaterialChanged;
        }

        #endregion Constructor

        #region Direct Colours flags

        // a Direct Colours value consists of 8 bits for the flags followed by 24 bits for the RGB data
        private const uint DirectColourFlagMask = 0xFF000000;

        /// <summary>
        /// The alpha-value used for transparent <i>Direct Colours</i> values.
        /// </summary>
        public const int DirectColourAlpha = 127;

        /// <summary>
        /// The flag indicating that the colour-value is an opaque <i>Direct Colours</i> value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An opaque <i>Direct Colours</i> colour-value is a 32-bit unsigned integer in big-endian format. Bits 31-24
        /// have the fixed value 0x02, bits 23-16 specify the Red component, bits 15-8 specify the Green component and bits
        /// 7-0 specify the Blue component. Alpha is 255.
        /// </para>
        /// </remarks>
        public const uint DirectColourOpaque = 0x2000000;

        /// <summary>
        /// The flag indicating that the colour-value is a transparent <i>Direct Colours</i> value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A transparent <i>Direct Colours</i> colour-value is a 32-bit unsigned integer in big-endian format.  Bits 31-24
        /// have the fixed value 0x03, bits 23-16 specify the Red component, bits 15-8 specify the Green component and bits
        /// 7-0 specify the Blue component. Alpha is <see cref="DirectColourAlpha"/>.
        /// <note>
        /// Transparent <i>Direct Colours</i> values are not permitted in official files.
        /// </note>
        /// </para>
        /// </remarks>
        public const uint DirectColourTransparent = 0x3000000;

        /// <summary>
        /// The flag indicating that the colour-value is an opaque dithered <i>Direct Colours</i> value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A dithered opaque <i>Direct Colours</i> colour-value is a 32-bit unsigned integer in big-endian format.  Bits 31-24
        /// have the fixed value 0x04, bits 23-20 specify the first Red component, bits 19-16 specify the first Green component,
        /// bits 15-12 specify the first Blue component, bits 11-8 specify the second Red component, bits 7-4 specify the second
        /// Green component and bits 3-0 specify the second Blue component. The final colour value is a 50-50 blend of the two
        /// 12-bit RGB values with an alpha-value of 255.
        /// </para>
        /// <note>
        /// Dithered <i>Direct Colours</i> values are not permitted in official files.
        /// </note>
        /// </remarks>
        /// <seealso href="http://ldlite.sourceforge.net/"/>
        public const uint DirectColourOpaqueDithered = 0x4000000;

        /// <summary>
        /// The flag indicating that the colour-value is a transparent dithered <i>Direct Colours</i> value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A dithered transparent <i>Direct Colours</i> colour-value is a 32-bit unsigned integer in big-endian format.  Bits 31-24
        /// have the fixed value 0x05, bits 23-20 specify the Red component, bits 19-16 specify the Green component, bits 15-12 specify
        /// the Blue component, and bits 11-0 are unused.  The final colour value is the 12-bit RGB value promoted to 24-bit and with
        /// an alpha-value of <see cref="DirectColourAlpha"/>.
        /// </para>
        /// <note>
        /// Dithered <i>Direct Colours</i> values are not permitted in official files.
        /// </note>
        /// </remarks>
        /// <seealso href="http://ldlite.sourceforge.net/"/>
        public const uint DirectColourTransparentDithered = 0x5000000;

        /// <summary>
        /// The flag indicating that the colour-value is a transparent dithered <i>Direct Colours</i> value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A dithered transparent <i>Direct Colours</i> colour-value is a 32-bit unsigned integer in big-endian format. Bits 31-24
        /// have the fixed value 0x06, bits 23-16 are unused, bits 15-12 specify the Red component, bits 11-8 specify the Green component
        /// and bits 7-0 specify the Blue component. The final colour value is the 12-bit RGB value promoted to 24-bit and with an alpha-value
        /// of <see cref="DirectColourAlpha"/>.
        /// </para>
        /// <note>
        /// Dithered <i>Direct Colours</i> values are not permitted in official files.
        /// </note>
        /// </remarks>
        /// <seealso href="http://ldlite.sourceforge.net/"/>
        public const uint DirectColourTransparentDitheredAlt = 0x6000000;

        #endregion Direct Colours flags

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDColourEditor", typeof(LDColour));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDColour"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        /// <see cref="LDColour"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
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
        public string Name
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsDirectColour(Code))
                    return ColourValueToCode(Code);

                return _name.Value;
            }
            set
            {
                if (Name == value)
                    return;

                if (IsDirectColour(Code))
                    return;

                _name.Value = value;
            }
        }
        private UndoableProperty<string> _name = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> NameChanged;

        /// <inheritdoc />
        public uint Code
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _code.Value;
            }
            set
            {
                if (Code == value)
                    return;

                if (IsDirectColour(value))
                {
                    // convert 12-bit values to 24-bit
                    Color c = (Color)ConvertDirectColourToRGB(value);
                    value   = ConvertRGBToDirectColour(c);
                }

                _code.Value = value;
            }
        }
        private UndoableProperty<uint> _code = new UndoableProperty<uint>(Palette.MainColour);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<uint> CodeChanged;

        /// <inheritdoc />
        public uint EdgeCode
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _edgeCode.Value;
            }
            set
            {
                if (IsTransparentDirectColour(value))
                    throw new ArgumentException("value may not be a transparent Direct Colours value; was '" + value + "'");

                if (EdgeCode == value)
                    return;

                if (IsDirectColour(value))
                {
                    // convert 12-bit values to 24-bit
                    Color c = (Color)ConvertDirectColourToRGB(value);
                    value   = ConvertRGBToDirectColour(c);
                }

                _edgeCode.Value = value;
            }
        }
        private UndoableProperty<uint> _edgeCode = new UndoableProperty<uint>(Palette.EdgeColour);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<uint> EdgeCodeChanged;

        /// <inheritdoc />
        public Color Value
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _value.Value;
            }
            set
            {
                if (Value != value)
                    _value.Value = value;
            }
        }
        private UndoableProperty<Color> _value = new UndoableProperty<Color>(Color.Black);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<Color> ValueChanged;

        /// <inheritdoc />
        public Color EdgeValue
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                uint edgeCode = EdgeCode;

                if (IsDirectColour(edgeCode))
                    return _directEdgeColour;

                IColour c;
                IPage page                   = Page;
                IStep step                   = Step;
                IElement element             = this;
                IElementCollection container = Parent;
                int stepIdx;

                if (null != page && null != step)
                    stepIdx = page.IndexOf(step);
                else
                    stepIdx = -1;

                // try the document-tree first
                while (null != container)
                {
                    if (container.ContainsColourElements)
                    {
                        int index = container.IndexOf(element);

                        for (int i = index - 1; i >= 0; i--)
                        {
                            c = container[i] as IColour;

                            if (null != c && c.Code == edgeCode)
                                return (Palette.EdgeColour == edgeCode) ? c.EdgeValue : c.Value;
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

                // try the system-palette
                c = Palette.SystemPalette[edgeCode];

                if (null != c)
                    return (Palette.EdgeColour == edgeCode) ? c.EdgeValue : c.Value;

                // fallback: just return EdgeColour
                return Palette.SystemPalette[Palette.EdgeColour].EdgeValue;
            }
        }
        private Color _directEdgeColour;

        /// <inheritdoc />
        public byte Luminance
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _luminance.Value;
            }
            set
            {
                if (Luminance != value)
                    _luminance.Value = value;
            }
        }
        private UndoableProperty<byte> _luminance = new UndoableProperty<byte>(0);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<byte> LuminanceChanged;

        /// <inheritdoc />
        public IMaterial Material
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _material.Value;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException();

                if (value == Material)
                    return;

                if (null != value.Colour)
                    throw new InvalidOperationException("The Material is already attached to a Colour");

                _material.Value = value;
            }
        }
        private UndoableProperty<IMaterial> _material = new UndoableProperty<IMaterial>(new PlasticMaterial());

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IMaterial> MaterialChanged;

        /// <inheritdoc />
        public bool IsTransparent
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return Value.A < 0xFF;
            }
        }

        /// <inheritdoc />
        public bool IsSystemPaletteColour { get; internal set; }

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        public override Image Icon { get { return Resources.ColourIcon; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.ColourDefinition; } }

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
        /// <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> is a state-element: the colour-values it defines is available for use by
        /// any <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/>s which follow it in the containing <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>.
        /// </para>
        /// </remarks>
        public override bool IsStateElement { get { return true; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> is not a top-level element.
        /// </para>
        /// </remarks>
        public override bool IsTopLevelElement { get { return false; } }

        #endregion Self-description

        #region Utility methods

        /// <summary>
        /// Tests whether a colour-value is a <i>Direct Colours</i> value.
        /// </summary>
        /// <param name="colourValue">The colour-value to check.</param>
        /// <returns><c>true</c> if <paramref name="colourValue"/> is a <i>Direct Colours</i> value; <c>false</c> otherwise.</returns>
        public static bool IsDirectColour(uint colourValue)
        {
            return (IsOpaqueDirectColour(colourValue) || IsTransparentDirectColour(colourValue));
        }

        /// <summary>
        /// Tests whether a colour-value is an opaque <i>Direct Colours</i> value.
        /// </summary>
        /// <param name="colourValue">The colour-value to check.</param>
        /// <returns><c>true</c> if <paramref name="colourValue"/> is an opaque or dithered opaque <i>Direct Colours</i> value; <c>false</c> otherwise.</returns>
        public static bool IsOpaqueDirectColour(uint colourValue)
        {
            colourValue &= DirectColourFlagMask;

            return (DirectColourOpaque == colourValue || DirectColourOpaqueDithered == colourValue);
        }

        /// <summary>
        /// Tests whether a colour-value is a transparent <i>Direct Colours</i> value.
        /// </summary>
        /// <param name="colourValue">The colour-value to check.</param>
        /// <returns><c>true</c> if <paramref name="colourValue"/> is a transparent or dithered transparent <i>Direct Colours</i> value; <c>false</c> otherwise.</returns>
        public static bool IsTransparentDirectColour(uint colourValue)
        {
            colourValue &= DirectColourFlagMask;

            return (DirectColourTransparent == colourValue || DirectColourTransparentDithered == colourValue || DirectColourTransparentDitheredAlt == colourValue);
        }

        /// <summary>
        /// Returns a <see cref="T:System.Drawing.Color"/> which represents a <i>Direct Colours</i> value.
        /// </summary>
        /// <param name="directColourValue">The colour-value to convert.</param>
        /// <returns>A <see cref="T:System.Drawing.Color"/>, or <c>null</c> if the colour-value was not a <i>Direct Colours</i> value.</returns>
        /// <remarks>
        /// <para>
        /// Transparent colours are returned with an alpha-value of <see cref="DirectColourAlpha"/>.
        /// </para>
        /// </remarks>
        public static Color? ConvertDirectColourToRGB(uint directColourValue)
        {
            Color? c = null;

            switch (directColourValue & DirectColourFlagMask)
            {
                case DirectColourOpaque:
                    c = Color.FromArgb((int)(directColourValue | 0xFF000000));
                    break;

                case DirectColourTransparent:
                    c = Color.FromArgb((int)((directColourValue & ~DirectColourFlagMask) | (DirectColourAlpha << 24)));
                    break;

                case DirectColourOpaqueDithered:
                    c = Color.FromArgb(0xFF,
                                       (int)(((directColourValue & 0x00F00000) >> 20) + ((directColourValue & 0x00000F00) >> 8)) * 17 / 2,
                                       (int)(((directColourValue & 0x000F0000) >> 16) + ((directColourValue & 0x000000F0) >> 4)) * 17 / 2,
                                       (int)(((directColourValue & 0x0000F000) >> 12) + ((directColourValue & 0x0000000F))) * 17 / 2);
                    break;

                case DirectColourTransparentDithered:
                    c = Color.FromArgb(DirectColourAlpha,
                                       (int)((directColourValue & 0x00F00000) >> 20) * 17,
                                       (int)((directColourValue & 0x000F0000) >> 16) * 17,
                                       (int)((directColourValue & 0x0000F000) >> 12) * 17);
                    break;

                case DirectColourTransparentDitheredAlt:
                    c = Color.FromArgb(DirectColourAlpha,
                                       (int)((directColourValue & 0x00000F00) >> 8) * 17,
                                       (int)((directColourValue & 0x000000F0) >> 4) * 17,
                                       (int)((directColourValue & 0x0000000F)) * 17);
                    break;
            }

            return c;
        }

        /// <summary>
        /// Returns a <i>Direct Colours</i> value which represents a <see cref="T:System.Drawing.Color"/>.
        /// </summary>
        /// <param name="color">The Color struct to read from.</param>
        /// <returns>A <i>Direct Colours</i> value that represents <paramref name="color"/>.</returns>
        /// <remarks>
        /// <para>
        /// If the alpha-value of <paramref name="color"/> is less than 255, a transparent <i>Direct Colours</i> value is created; otherwise an opaque value is returned.
        /// </para>
        /// </remarks>
        public static uint ConvertRGBToDirectColour(Color color)
        {
            if (color.A < 0xFF)
                return DirectColourTransparent | ((uint)color.R << 16) | ((uint)color.G << 8) | (uint)color.B;

            return DirectColourOpaque | ((uint)color.R << 16) | ((uint)color.G << 8) | (uint)color.B;
        }

        /// <summary>
        /// Returns a colour-value as LDraw code.
        /// </summary>
        /// <param name="colourValue">The colour-value.</param>
        /// <returns>LDraw code representing <paramref name="colourValue"/>.</returns>
        /// <remarks>
        /// <para>
        /// <i>Direct Colours</i> values are returned as a hexadecimal-format string prefixed with '#'. All other values are returned as base-10.
        /// </para>
        /// <note>
        /// Note to implementors: subclasses of <see cref="T:Digitalis.LDTools.DOM.API.IGraphic"/> which need to output LDraw code containing standard colour-values should
        /// use this function to generate standards-compliant code.
        /// </note>
        /// <seealso href="http://www.ldraw.org/article/512.html#colours">LDraw.org File Format Restrictions for the Official Library - Colours</seealso>
        /// </remarks>
        public static string ColourValueToCode(uint colourValue)
        {
            if (IsDirectColour(colourValue))
                return String.Format("#{0:X7}", colourValue);

            return colourValue.ToString();
        }

        #endregion Utility methods
    }
}
