#region License

//
// IColour.cs
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

    using System.Drawing;

    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw <i>!COLOUR</i> meta-command as defined in
    ///     <see href="http://www.ldraw.org/article/299.html">the LDraw.org Colour Definition Language Extension</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Code-generation</h3>
    /// If <i>codeFormat</i> is either <see cref="CodeStandards.Full"/> or <see cref="CodeStandards.OfficialModelRepository"/>,
    /// <see cref="IDOMObject.ToCode">ToCode()</see> will append the <i>!COLOUR</i> meta-command to <i>sb</i>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IColour</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.MetaCommand"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>true</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IColour : IMetaCommand
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentNullException">The property is set and the supplied string was <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the supplied string contained
        ///     <see href="http://www.ldraw.org/article/299.html#syntax">invalid characters</see>.</exception>
        /// <remarks>
        /// If <see cref="Code"/> is a <i>Direct Colours</i> value then setting this property will have no effect, and it will
        /// return <see cref="Code"/> converted to a hexadecimal string prefixed with a <c>#</c>.
        /// <p/>
        /// Raises the <see cref="NameChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>"&lt;unknown&gt;"</c>.
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Occurs when <see cref="Name"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<string> NameChanged;

        /// <summary>
        /// Gets or sets the code of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// This is either an LDraw colour-code or a <i>Direct Colours</i> value. If it is a <i>Direct Colours</i> value,
        /// <see cref="Value"/> will be updated as well. Dithered <i>Direct Colours</i> values will be converted to 24-bit.
        /// <p/>
        /// Raises the <see cref="CodeChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Digitalis.LDTools.DOM.Palette.MainColour"/>.
        /// </remarks>
        uint Code { get; set; }

        /// <summary>
        /// Occurs when <see cref="Code"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<uint> CodeChanged;

        /// <summary>
        /// Gets or sets the edge code of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="System.ArgumentException">The property is set and the supplied value was a transparent
        ///     <i>Direct Colours</i> value.</exception>
        /// <remarks>
        /// This is either an LDraw colour-code or an opaque <i>Direct Colours</i> value. Dithered <i>Direct Colours</i> values
        /// will be converted to 24-bit.
        /// <p/>
        /// Raises the <see cref="EdgeCodeChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/>.
        /// </remarks>
        uint EdgeCode { get; set; }

        /// <summary>
        /// Occurs when <see cref="EdgeCode"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<uint> EdgeCodeChanged;

        /// <summary>
        /// Gets or sets the main colour of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// Raises the <see cref="ValueChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <see cref="System.Drawing.Color.Black"/>.
        /// </remarks>
        Color Value { get; set; }

        /// <summary>
        /// Occurs when <see cref="Value"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<Color> ValueChanged;

        /// <summary>
        /// Gets the edge colour of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        Color EdgeValue { get; }

        /// <summary>
        /// Gets or sets the luminance of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// Raises the <see cref="LuminanceChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>0</c>.
        /// </remarks>
        byte Luminance { get; set; }

        /// <summary>
        /// Occurs when <see cref="Luminance"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<byte> LuminanceChanged;

        /// <summary>
        /// Gets or sets the material of the <see cref="IColour"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="System.NotSupportedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IColour"/> is
        ///     <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <remarks>
        /// Raises the <see cref="MaterialChanged"/> event when its value changes. In addition, if the <see cref="IMaterial"/>
        /// raises its <see cref="IMaterial.Changed"/> event, the <see cref="IColour"/> will raise <see cref="MaterialChanged"/>.
        /// <p/>
        /// Default value is an instance of <see cref="Digitalis.LDTools.DOM.PlasticMaterial"/>.
        /// </remarks>
        IMaterial Material { get; set; }

        /// <summary>
        /// Occurs when <see cref="Material"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IMaterial> MaterialChanged;

        /// <summary>
        /// Gets a value indicating whether the <see cref="IColour"/> is transparent.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        bool IsTransparent { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IColour"/> is a member of the
        ///     <see cref="Digitalis.LDTools.DOM.Palette.SystemPalette"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IColour"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        bool IsSystemPaletteColour { get; }

        #endregion Properties
    }
}
