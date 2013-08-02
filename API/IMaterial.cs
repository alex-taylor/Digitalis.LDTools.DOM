#region License

//
// IMaterial.cs
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

    using System;
    using System.Text;

    #endregion Usings

    /// <summary>
    /// Represents the material for an <see cref="T:Digitalis.LDTools.DOM.API.IColour"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a hint to a renderer as to how the <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> should reflect light.
    /// </para>
    /// </remarks>
    public interface IMaterial
    {
        #region Cloning

        /// <summary>
        /// Clones the <see cref="IMaterial"/>.
        /// </summary>
        /// <returns>A copy of the <see cref="IMaterial"/>.</returns>
        IMaterial Clone();

        /// <summary>
        /// Determines whether the <see cref="IMaterial"/> has the same values as another.
        /// </summary>
        /// <param name="material">The <see cref="IMaterial"/> to compare against.</param>
        /// <returns><c>true</c> if the specified <see cref="IMaterial"/> has the same values as this; otherwise, <c>false</c>.</returns>
        bool IsEquivalentTo(IMaterial material);

        #endregion Cloning

        #region Code-generation

        /// <summary>
        /// Returns the <see cref="IMaterial"/> as LDraw code.
        /// </summary>
        /// <param name="sb">A <see cref="T:System.Text.StringBuilder"/> to which the LDraw code will be appended.</param>
        /// <returns>A reference to <paramref name="sb"/> after the append operation has completed.</returns>
        StringBuilder ToCode(StringBuilder sb);

        #endregion Code-generation

        #region Freezing

        /// <summary>
        /// Gets a value indicating whether the <see cref="IMaterial"/> is frozen.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An <see cref="IMaterial"/> is frozen if the <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> to which it is
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.Material">attached</see> is <see cref="P:Digitalis.LDTools.DOM.API.IDOMObject.IsFrozen">frozen</see>. If the
        /// <see cref="IMaterial"/> is not attached to an <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> then it is not frozen.
        /// </para>
        /// <para>
        /// An <see cref="IMaterial"/> which is frozen is implicitly <see cref="IsLocked">locked</see>.
        /// </para>
        /// </remarks>
        bool IsFrozen { get; }

        #endregion Freezing

        #region Locking

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IMaterial"/> may be modified.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="IMaterial"/> is <see cref="IsFrozen">frozen</see>.</exception>
        /// <remarks>
        /// <para>
        /// An <see cref="IMaterial"/> is locked if the <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> to which it is
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.Material">attached</see> is <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>. If the
        /// <see cref="IMaterial"/> is not attached to an <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> then it is not locked.
        /// </para>
        /// </remarks>
        bool IsLocked { get; }

        #endregion Locking

        #region Properties

        /// <summary>
        /// Occurs when one of the <see cref="IMaterial"/>'s properties changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event may be subscribed to as an alternative to subscribing to each individual event provided by the <see cref="IMaterial"/>. Each event published by the
        /// <see cref="IMaterial"/> will also be published here, and also via the associated <see cref="Colour"/>'s <see cref="E:Digitalis.LDTools.DOM.API.IDOMObject.Changed"/>
        /// event.
        /// </para>
        /// </remarks>
        event EventHandler Changed;

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> with which the <see cref="IMaterial"/> is currently associated.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ObjectFrozenException">The property is set and the <see cref="IMaterial"/> is <see cref="IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The property is set and the <see cref="IMaterial"/> is <see cref="IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">The property is set and the supplied value was <c>null</c>.</exception>
        /// <exception cref="T:System.InvalidOperationException">The property is set and the <see cref="IMaterial"/> is already associated with an
        ///     <see cref="T:Digitalis.LDTools.DOM.API.IColour"/>.</exception>
        /// <remarks>
        /// <para>
        /// An <see cref="IMaterial"/> can be associated with an <see cref="T:Digitalis.LDTools.DOM.API.IColour"/> by setting either this property or
        /// <see cref="P:Digitalis.LDTools.DOM.API.IColour.Material"/>.
        /// </para>
        /// </remarks>
        IColour Colour { get; set; }

        #endregion Properties

        #region Self-description

        /// <summary>
        /// Gets a description of the <see cref="IMaterial"/> suitable for display to the user.
        /// </summary>
        string Description { get; }

        #endregion Self-description
    }
}
