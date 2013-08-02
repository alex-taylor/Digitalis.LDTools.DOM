#region License

//
// Materials.cs
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
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    #region Material

    /// <summary>
    /// Abstract implementation of <see cref="T:Digitalis.LDTools.DOM.API.IMaterial"/>
    /// </summary>
    [Serializable]
    public abstract class Material : IMaterial
    {
        #region Cloning

        /// <inheritdoc />
        public abstract IMaterial Clone();

        /// <inheritdoc />
        public abstract bool IsEquivalentTo(IMaterial material);

        #endregion Cloning

        #region Code-generation

        /// <inheritdoc />
        public abstract StringBuilder ToCode(StringBuilder sb);

        #endregion Code-generation

        #region Freezing

        /// <inheritdoc />
        [Browsable(false)]
        public bool IsFrozen
        {
            get
            {
                if (null != Colour)
                    return Colour.IsFrozen;

                return false;
            }
        }

        #endregion Freezing

        #region Locking

        /// <inheritdoc />
        [Browsable(false)]
        public bool IsLocked
        {
            get
            {
                if (null != Colour)
                    return Colour.IsLocked;

                return false;
            }
        }

        #endregion Locking

        #region Properties

        /// <inheritdoc />
        public event EventHandler Changed;

        /// <summary>
        /// Raises the <see cref="E:Digitalis.LDTools.DOM.API.IMaterial.Changed"/> event.
        /// </summary>
        /// <param name="e">The event-args instance containing the event data.</param>
        /// <remarks>
        /// <note>
        /// Note to implementors: subclasses which override this method must call their superclass before returning.
        /// </note>
        /// </remarks>
        protected void OnChanged(EventArgs e)
        {
            if (null != Changed)
                Changed(this, e);
        }

        /// <inheritdoc />
        public IColour Colour
        {
            get { return _colour; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (null != Colour && null != value)
                    throw new InvalidOperationException("The Material is already attached to a Colour");

                if (null != value)
                    value.Material = this;

                _colour = value;
            }
        }
        private IColour _colour;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        [Browsable(false)]
        public abstract string Description { get; }

        #endregion Self-description
    }

    #endregion Material

    #region Materials

    /// <summary>
    /// Provides an enumeration of the available <see cref="T:Digitalis.LDTools.DOM.API.IMaterial"/>s.
    /// </summary>
    public class Materials
    {
        /// <summary>
        /// Returns the available <see cref="T:Digitalis.LDTools.DOM.API.IMaterial"/>s.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IMaterial"/>s are not guaranteed to be enumerated in any particular order.
        /// </para>
        /// </remarks>
        public IEnumerable<Type> AvailableMaterials = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Materials"/> class.
        /// </summary>
        public Materials()
        {
            List<Type> materials = new List<Type>();
            Type material        = typeof(IMaterial);

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!t.IsAbstract && material.IsAssignableFrom(t) && !t.IsInterface)
                    materials.Add(t);
            }

            AvailableMaterials = materials;
        }
    }

    #endregion Materials

    #region UnknownMaterial

    /// <summary>
    /// Represents an unrecognised finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(".*")]
    public sealed class UnknownMaterial : Material
    {
        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Unknown; } }

        /// <summary>
        /// Gets or sets the specification for this material.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="UnknownMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="UnknownMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// This property may be used to store any string required to define the material. Its value will be output by <see cref="M:Digitalis.LDTools.DOM.API.IMaterial.ToCode"/>.
        /// </para>
        /// <para>
        /// Raises the <see cref="ParametersChanged"/> event when its value changes.
        /// </para>
        /// <para>
        /// Default value is <c>null</c>.
        /// </para>
        /// </remarks>
        public string Parameters
        {
            get { return _parameters.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (value == _parameters.Value)
                    return;

                _parameters.Value = value;
            }
        }
        private UndoableProperty<string> _parameters = new UndoableProperty<string>();

        /// <summary>
        /// Occurs when <see cref="Parameters"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<string> ParametersChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMaterial"/> class with default values.
        /// </summary>
        public UnknownMaterial()
        {
            _parameters.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (null != ParametersChanged)
                    ParametersChanged(this, e);

                OnChanged(e);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <remarks>
        /// <para>
        /// This class should be used when a <i>!COLOUR</i> meta-command specifies a material for which a more specific implementation is not currently available.
        /// It has a single free-form string property <see cref="Parameters"/> which should be used to store the portion of the <i>!COLOUR</i> line that contains
        /// the material specification.
        /// </para>
        /// </remarks>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 NEW_MATERIAL PARAM1 PARAM2</i> then <see cref="UnknownMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// UnknownMaterial material = new UnknownMaterial("NEW_MATERIAL PARAM1 PARAM2");
        /// </code>
        /// </example>
        public UnknownMaterial(string code)
            : this()
        {
            Parameters = code;
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            if (null != Parameters)
                sb.Append(Parameters);

            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new UnknownMaterial(Parameters);
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is UnknownMaterial))
                return false;

            return (Parameters == (material as UnknownMaterial).Parameters);
        }
    }

    #endregion UnknownMaterial

    #region PlasticMaterial

    /// <summary>
    /// Represents a plastic such as ABS, acrylic, nylon etc.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*$")]
    public sealed class PlasticMaterial : Material
    {
        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Plastic; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlasticMaterial"/> class with default values.
        /// </summary>
        public PlasticMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlasticMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not empty.</exception>
        /// <remarks>
        /// <para>
        /// This constructor is provided for consistency with the other materials, and should not normally be used. <paramref name="code"/> is
        /// expected to be either <c>null</c> or an empty/whitespace string.
        /// </para>
        /// </remarks>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1</i> then <see cref="PlasticMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// PlasticMaterial material = new PlasticMaterial("");
        /// </code>
        /// </example>
        public PlasticMaterial(string code)
        {
            if (!String.IsNullOrWhiteSpace(code))
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb) { return sb; }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new PlasticMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is PlasticMaterial))
                return false;

            return true;
        }
    }

    #endregion PlasticMaterial

    #region ChromeMaterial

    /// <summary>
    /// Represents a chromed finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*CHROME\s*$")]
    public sealed class ChromeMaterial : Material
    {
        private static readonly string Token = "CHROME";

        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Chrome; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromeMaterial"/> class with default values.
        /// </summary>
        public ChromeMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromeMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'CHROME' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 CHROME</i> then <see cref="ChromeMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// ChromeMaterial material = new ChromeMaterial("CHROME");
        /// </code>
        /// </example>
        public ChromeMaterial(string code)
        {
            if (code.Trim() != Token)
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.Append(Token);
            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new ChromeMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is ChromeMaterial))
                return false;

            return true;
        }
    }

    #endregion ChromeMaterial

    #region PearlescentMaterial

    /// <summary>
    /// Represents a pearlescent finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*PEARLESCENT\s*$")]
    public sealed class PearlescentMaterial : Material
    {
        private static readonly string Token = "PEARLESCENT";

        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Pearlescent; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PearlescentMaterial"/> class with default values.
        /// </summary>
        public PearlescentMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PearlescentMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'PEARLESCENT' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 PEARLESCENT</i> then <see cref="PearlescentMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// PearlescentMaterial material = new PearlescentMaterial("PEARLESCENT");
        /// </code>
        /// </example>
        public PearlescentMaterial(string code)
        {
            if (code.Trim() != Token)
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.Append(Token);
            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new PearlescentMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is PearlescentMaterial))
                return false;

            return true;
        }
    }

    #endregion PearlescentMaterial

    #region RubberMaterial

    /// <summary>
    /// Represents a rubber finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*RUBBER\s*$")]
    public sealed class RubberMaterial : Material
    {
        private static readonly string Token = "RUBBER";

        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Rubber; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="RubberMaterial"/> class with default values.
        /// </summary>
        public RubberMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RubberMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'RUBBER' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 RUBBER</i> then <see cref="RubberMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// RubberMaterial material = new RubberMaterial("RUBBER");
        /// </code>
        /// </example>
        public RubberMaterial(string code)
        {
            if (code.Trim() != Token)
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.Append(Token);
            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new RubberMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is RubberMaterial))
                return false;

            return true;
        }
    }

    #endregion RubberMaterial

    #region MatteMetallicMaterial

    /// <summary>
    /// Represents a matte metallic finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*MATTE_METALLIC\s*$")]
    public sealed class MatteMetallicMaterial : Material
    {
        private static readonly string Token = "MATTE_METALLIC";

        /// <inheritdoc />
        public override string Description { get { return Resources.Material_MatteMetallic; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatteMetallicMaterial"/> class with default values.
        /// </summary>
        public MatteMetallicMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatteMetallicMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'MATTE_METALLIC' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 MATTE_METALLIC</i> then <see cref="MatteMetallicMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// MatteMetallicMaterial material = new MatteMetallicMaterial("MATTE_METALLIC");
        /// </code>
        /// </example>
        public MatteMetallicMaterial(string code)
        {
            if (code.Trim() != Token)
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.Append(Token);
            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new MatteMetallicMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is MatteMetallicMaterial))
                return false;

            return true;
        }
    }

    #endregion MatteMetallicMaterial

    #region MetalMaterial

    /// <summary>
    /// Represents a metal finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*METAL\s*$")]
    public sealed class MetalMaterial : Material
    {
        private static readonly string Token = "METAL";

        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Metal; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetalMaterial"/> class with default values.
        /// </summary>
        public MetalMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetalMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'METAL' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 METAL</i> then <see cref="MetalMaterial"/> should
        /// be constructed as follows:
        /// <code>
        /// MetalMaterial material = new MetalMaterial("METAL");
        /// </code>
        /// </example>
        public MetalMaterial(string code)
        {
            if (code.Trim() != Token)
                throw new FormatException();
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.Append(Token);
            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            return new MetalMaterial();
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is MetalMaterial))
                return false;

            return true;
        }
    }

    #endregion MetalMaterial

    #region GlitterMaterial

    /// <summary>
    /// Represents a glitter finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*MATERIAL\s+GLITTER\s+VALUE\s+(#|0x)[0-9a-zA-Z]{6}(\s+ALPHA\s+\d+)?(\s+LUMINANCE\s+\d+)?\s+FRACTION\s+\d*\.\d+\s+VFRACTION\s+\d*\.\d+\s+(SIZE\s+\d+|MINSIZE\s+\d+\s+MAXSIZE\s+\d+)\s*$")]
    public sealed class GlitterMaterial : GrainedMaterial
    {
        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Glitter; } }

        /// <summary>
        /// Gets or sets the proportion of the volume that should be filled with glitter fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GlitterMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GlitterMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// This must be a value between <c>0.0</c> and <c>1.0</c>. Default value is <c>0.5</c>.
        /// </para>
        /// <para>
        /// Raises the <see cref="VFractionChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public double VFraction
        {
            get { return _vfraction.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (VFraction == value)
                    return;

                if (value <= 0.0 || value >= 1.0)
                    value = DefaultFraction;

                _vfraction.Value = value;
            }
        }
        private UndoableProperty<double> _vfraction = new UndoableProperty<double>(DefaultFraction);

        /// <summary>
        /// Occurs when <see cref="VFraction"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<double> VFractionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlitterMaterial"/> class with default values.
        /// </summary>
        public GlitterMaterial()
        {
            _vfraction.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (null != VFractionChanged)
                    VFractionChanged(this, e);

                OnChanged(e);
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlitterMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'GLITTER' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 MATERIAL GLITTER VALUE #123456 FRACTION 0.5 VFRACTION 0.5 SIZE 3</i> then
        /// <see cref="GlitterMaterial"/> should be constructed as follows:
        /// <code>
        /// GlitterMaterial material = new GlitterMaterial("MATERIAL GLITTER VALUE #123456 FRACTION 0.5 VFRACTION 0.5 SIZE 3");
        /// </code>
        /// </example>
        public GlitterMaterial(string code)
            : this()
        {
            string[] fields = code.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 10)
                throw new FormatException("MATERIAL GLITTER code should have at least 10 fields");

            if ("MATERIAL" != fields[0] || "GLITTER" != fields[1] || "VALUE" != fields[2])
                throw new FormatException("Code must start with 'MATERIAL GLITTER VALUE'");

            int rgb      = IntHelper.ParseInt(fields[3]);
            int alpha    = 0xFF;
            uint minSize = DefaultSize;
            uint maxSize = DefaultSize;

            for (int i = 4; i < fields.Length; i += 2)
            {
                switch (fields[i])
                {
                    case "ALPHA":
                        alpha = int.Parse(fields[i + 1]);

                        if (alpha < 0 || alpha > 0xFF)
                            throw new FormatException("ALPHA must be in the range 0..255");
                        break;

                    case "LUMINANCE":
                        int luminance = int.Parse(fields[i + 1]);

                        if (luminance < 0 || luminance > 255)
                            throw new FormatException("LUMINANCE must be in the range 0..255");

                        Luminance = (byte)luminance;
                        break;

                    case "FRACTION":
                        double fraction = double.Parse(fields[i + 1]);

                        if (fraction <= 0.0 || fraction >= 1.0)
                            throw new FormatException("FRACTION must be between 0 and 1");

                        Fraction = fraction;
                        break;

                    case "VFRACTION":
                        double vfraction = double.Parse(fields[i + 1]);

                        if (vfraction <= 0.0 || vfraction >= 1.0)
                            throw new FormatException("VFRACTION must be between 0 and 1");

                        VFraction = vfraction;
                        break;

                    case "SIZE":
                        uint size = uint.Parse(fields[i + 1]);

                        if (0 == size)
                            throw new FormatException("SIZE must be greater than zero");

                        minSize = maxSize = size;
                        break;

                    case "MINSIZE":
                        minSize = uint.Parse(fields[i + 1]);

                        if (0 == minSize)
                            throw new FormatException("MINSIZE must be greater than zero");
                        break;

                    case "MAXSIZE":
                        maxSize = uint.Parse(fields[i + 1]);

                        if (0 == maxSize)
                            throw new FormatException("MAXSIZE must be greater than zero");
                        break;

                    default:
                        throw new FormatException("Unrecognised value '" + fields[i] + "'");
                }
            }

            Value = Color.FromArgb((alpha << 24) | rgb);

            if (maxSize < minSize)
                throw new FormatException("MAXSIZE must be greater than or equal to MINSIZE");

            // need to set these the right way around to avoid exceptions from the property setters
            if (maxSize >= MinSize)
            {
                MaxSize = maxSize;
                MinSize = minSize;
            }
            else
            {
                MinSize = minSize;
                MaxSize = maxSize;
            }
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.AppendFormat("MATERIAL GLITTER VALUE #{0:X2}{1:X2}{2:X2}", Value.R, Value.G, Value.B);

            if (0xFF != Value.A)
                sb.Append(" ALPHA ").Append(Value.A);

            if (0 != Luminance)
                sb.Append(" LUMINANCE ").Append(Luminance);

            sb.Append(" FRACTION ").Append(Fraction).Append(" VFRACTION ").Append(VFraction);

            if (MinSize == MaxSize)
                sb.Append(" SIZE ").Append(MinSize);
            else
                sb.Append(" MINSIZE ").Append(MinSize).Append(" MAXSIZE ").Append(MaxSize);

            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            GlitterMaterial copy = new GlitterMaterial();
            copy.Value           = Value;
            copy.Fraction        = Fraction;
            copy.VFraction       = VFraction;
            copy.Luminance       = Luminance;

            uint maxSize = MaxSize;
            uint minSize = MinSize;

            // need to set these the right way around to avoid exceptions from the property setters
            if (maxSize >= copy.MinSize)
            {
                copy.MaxSize = maxSize;
                copy.MinSize = minSize;
            }
            else
            {
                copy.MinSize = minSize;
                copy.MaxSize = maxSize;
            }

            return copy;
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is GlitterMaterial))
                return false;

            if (VFraction != (material as GlitterMaterial).VFraction)
                return false;

            return base.IsEquivalentTo(material);
        }
    }

    #endregion GlitterMaterial

    #region SpeckleMaterial

    /// <summary>
    /// Represents a speckle finish.
    /// </summary>
    [Serializable]
    [MetaCommandPattern(@"^\s*MATERIAL\s+SPECKLE\s+VALUE\s+(#|0x)[0-9a-zA-Z]{6}(\s+ALPHA\s+\d+)?(\s+LUMINANCE\s+\d+)?\s+FRACTION\s+\d*\.\d+\s+(SIZE\s+\d+|MINSIZE\s+\d+\s+MAXSIZE\s+\d+)\s*$")]
    public sealed class SpeckleMaterial : GrainedMaterial
    {
        /// <inheritdoc />
        public override string Description { get { return Resources.Material_Speckle; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeckleMaterial"/> class with default values.
        /// </summary>
        public SpeckleMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeckleMaterial"/> class with the specified values.
        /// </summary>
        /// <param name="code">The LDraw code representing this material.</param>
        /// <exception cref="T:System.FormatException">The supplied string was not valid LDraw 'SPECKLE' material code.</exception>
        /// <example>
        /// If the <i>!COLOUR</i> meta-command is <i>!COLOUR name CODE 0 VALUE #123456 EDGE 1 MATERIAL SPECKLE VALUE #123456 FRACTION 0.5 SIZE 3</i> then
        /// <see cref="SpeckleMaterial"/> should be constructed as follows:
        /// <code>
        /// SpeckleMaterial material = new SpeckleMaterial("MATERIAL GLITTER VALUE #123456 FRACTION 0.5 SIZE 3");
        /// </code>
        /// </example>
        public SpeckleMaterial(string code)
        {
            string[] fields = code.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 8)
                throw new FormatException("MATERIAL SPECKLE code should have at least 8 fields");

            if ("MATERIAL" != fields[0] || "SPECKLE" != fields[1] || "VALUE" != fields[2])
                throw new FormatException("Code must start with 'MATERIAL SPECKLE VALUE'");

            int rgb      = IntHelper.ParseInt(fields[3]);
            int alpha    = 0xFF;
            uint minSize = DefaultSize;
            uint maxSize = DefaultSize;

            for (int i = 4; i < fields.Length; i += 2)
            {
                switch (fields[i])
                {
                    case "ALPHA":
                        alpha = int.Parse(fields[i + 1]);

                        if (alpha < 0 || alpha > 0xFF)
                            throw new FormatException("ALPHA must be in the range 0..255");
                        break;

                    case "LUMINANCE":
                        int luminance = int.Parse(fields[i + 1]);

                        if (luminance < 0 || luminance > 255)
                            throw new FormatException("LUMINANCE must be in the range 0..255");

                        Luminance = (byte)luminance;
                        break;

                    case "FRACTION":
                        double fraction = double.Parse(fields[i + 1]);

                        if (fraction <= 0.0 || fraction >= 1.0)
                            throw new FormatException("FRACTION must be between 0 and 1");

                        Fraction = fraction;
                        break;

                    case "SIZE":
                        uint size = uint.Parse(fields[i + 1]);

                        if (0 == size)
                            throw new FormatException("SIZE must be greater than zero");

                        maxSize = minSize = size;
                        break;

                    case "MINSIZE":
                        minSize = uint.Parse(fields[i + 1]);

                        if (0 == minSize)
                            throw new FormatException("MINSIZE must be greater than zero");
                        break;

                    case "MAXSIZE":
                        maxSize = uint.Parse(fields[i + 1]);

                        if (0 == maxSize)
                            throw new FormatException("MAXSIZE must be greater than zero");
                        break;

                    default:
                        throw new FormatException("Unrecognised value '" + fields[i] + "'");
                }
            }

            Value = Color.FromArgb((alpha << 24) | rgb);

            if (maxSize < minSize)
                throw new FormatException("MAXSIZE must be greater than or equal to MINSIZE");

            // need to set these the right way around to avoid exceptions from the property setters
            if (maxSize >= MinSize)
            {
                MaxSize = maxSize;
                MinSize = minSize;
            }
            else
            {
                MinSize = minSize;
                MaxSize = maxSize;
            }
        }

        /// <inheritdoc />
        public override StringBuilder ToCode(StringBuilder sb)
        {
            sb.AppendFormat("MATERIAL SPECKLE VALUE #{0:X2}{1:X2}{2:X2}", Value.R, Value.G, Value.B);

            if (0xFF != Value.A)
                sb.Append(" ALPHA ").Append(Value.A);

            if (0 != Luminance)
                sb.Append(" LUMINANCE ").Append(Luminance);

            sb.Append(" FRACTION ").Append(Fraction);

            if (MinSize == MaxSize)
                sb.Append(" SIZE ").Append(MinSize);
            else
                sb.Append(" MINSIZE ").Append(MinSize).Append(" MAXSIZE ").Append(MaxSize);

            return sb;
        }

        /// <inheritdoc />
        public override IMaterial Clone()
        {
            SpeckleMaterial copy = new SpeckleMaterial();
            copy.Value = Value;
            copy.Fraction = Fraction;
            copy.Luminance = Luminance;

            uint maxSize = MaxSize;
            uint minSize = MinSize;

            // need to set these the right way around to avoid exceptions from the property setters
            if (maxSize >= copy.MinSize)
            {
                copy.MaxSize = maxSize;
                copy.MinSize = minSize;
            }
            else
            {
                copy.MinSize = minSize;
                copy.MaxSize = maxSize;
            }

            return copy;
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is SpeckleMaterial))
                return false;

            return base.IsEquivalentTo(material);
        }
    }

    #endregion SpeckleMaterial

    #region GrainedMaterial

    /// <summary>
    /// Abstract base class for <see cref="T:Digitalis.LDTools.DOM.GlitterMaterial"/> and <see cref="T:Digitalis.LDTools.DOM.SpeckleMaterial"/>.
    /// </summary>
    [Serializable]
    public abstract class GrainedMaterial : Material
    {
        private class ValueConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    try
                    {
                        return Color.FromArgb(IntHelper.ParseInt(value as string));
                    }
                    catch
                    {
                        // ignore parser problems
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return String.Format("#{0:X8}", ((Color)value).ToArgb());

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        internal const double DefaultFraction = 0.5;
        internal const uint DefaultSize       = 3;

        /// <summary>
        /// Gets or sets the colour-value for the grain fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// Raises the <see cref="ValueChanged"/> event when its value changes.
        /// </para>
        /// <para>
        /// Default value is <see cref="F:System.Drawing.Color.Empty"/>.
        /// </para>
        /// </remarks>
        [Editor("Digitalis.GUI.Controls.RGBPickerUITypeEditor, Digitalis.GUI", typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ValueConverter))]
        [UIHint("TransparencyMode", "255")]
        public Color Value
        {
            get { return _value.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (Value == value)
                    return;

                _value.Value = value;
            }
        }
        private UndoableProperty<Color> _value = new UndoableProperty<Color>(Color.Empty);

        /// <summary>
        /// Occurs when <see cref="Value"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<Color> ValueChanged;

        /// <summary>
        /// Gets or sets the luminance value for the grain fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// Raises the <see cref="LuminanceChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public byte Luminance
        {
            get { return _luminance.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (Luminance == value)
                    return;

                _luminance.Value = value;
            }
        }
        private UndoableProperty<byte> _luminance = new UndoableProperty<byte>(0);

        /// <summary>
        /// Occurs when <see cref="Luminance"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<byte> LuminanceChanged;

        /// <summary>
        /// Gets or sets the proportion of the surface that should be covered with grain fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// This must be a value between <c>0.0</c> and <c>1.0</c>. Default value is <c>0.5</c>.
        /// </para>
        /// <para>
        /// Raises the <see cref="FractionChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public double Fraction
        {
            get { return _fraction.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (Fraction == value)
                    return;

                if (value <= 0.0 || value >= 1.0)
                    value = DefaultFraction;

                _fraction.Value = value;
            }
        }
        private UndoableProperty<double> _fraction = new UndoableProperty<double>(DefaultFraction);

        /// <summary>
        /// Occurs when <see cref="Fraction"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<double> FractionChanged;

        /// <summary>
        /// Gets or sets the minimum size of grain fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// The value must be greater than zero. Default value is <c>3</c>.
        /// </para>
        /// <para>
        /// Raises the <see cref="MinSizeChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public uint MinSize
        {
            get { return _minSize.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (MinSize == value)
                    return;

                if (0 == value)
                    value = 1;

                if (value > MaxSize)
                    value = MaxSize;

                _minSize.Value = value;
            }
        }
        private UndoableProperty<uint> _minSize = new UndoableProperty<uint>(DefaultSize);

        /// <summary>
        /// Occurs when <see cref="MinSize"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<uint> MinSizeChanged;

        /// <summary>
        /// Gets or sets the maximum size of grain fragments.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="GrainedMaterial"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IMaterial.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// <para>
        /// The value must be greater than zero. Default value is <c>3</c>.
        /// </para>
        /// <para>
        /// Raises the <see cref="MaxSizeChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public uint MaxSize
        {
            get { return _maxSize.Value; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (IsLocked)
                    throw new ElementLockedException();

                if (MaxSize == value)
                    return;

                if (value < MinSize)
                    value = MinSize;

                _maxSize.Value = value;
            }
        }
        private UndoableProperty<uint> _maxSize = new UndoableProperty<uint>(DefaultSize);

        /// <summary>
        /// Occurs when <see cref="MaxSize"/> changes.
        /// </summary>
        public event PropertyChangedEventHandler<uint> MaxSizeChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainedMaterial"/> class with default values.
        /// </summary>
        public GrainedMaterial()
        {
            _value.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Color> e)
            {
                if (null != ValueChanged)
                    ValueChanged(this, e);

                OnChanged(e);
            };

            _luminance.ValueChanged += delegate(object sender, PropertyChangedEventArgs<byte> e)
            {
                if (null != LuminanceChanged)
                    LuminanceChanged(this, e);

                OnChanged(e);
            };

            _fraction.ValueChanged += delegate(object sender, PropertyChangedEventArgs<double> e)
            {
                if (null != FractionChanged)
                    FractionChanged(this, e);

                OnChanged(e);
            };

            _minSize.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (null != MinSizeChanged)
                    MinSizeChanged(this, e);

                OnChanged(e);
            };

            _maxSize.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (null != MaxSizeChanged)
                    MaxSizeChanged(this, e);

                OnChanged(e);
            };
        }

        /// <inheritdoc />
        public override bool IsEquivalentTo(IMaterial material)
        {
            if (null == material || !(material is GrainedMaterial))
                return false;

            GrainedMaterial grained = material as GrainedMaterial;

            return (Value     == grained.Value &&
                    MinSize   == grained.MinSize &&
                    MaxSize   == grained.MaxSize &&
                    Fraction  == grained.Fraction &&
                    Luminance == grained.Luminance);
        }
    }

    #endregion GrainedMaterial
}
