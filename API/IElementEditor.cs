#region License

//
// IElementEditor.cs
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
    using System.Drawing;
    using System.Windows.Forms;

    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    /// <summary>
    /// Represents an editor control for an <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each implementation of <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> which has caller-settable properties should provide an implementation of
    /// <b>IElementEditor</b> that supplies a user-interface to edit the values of those properties. An instance of the implementation should be returned by
    /// <see cref="M:Digitalis.LDTools.DOM.API.T:Digitalis.LDTools.DOM.API.IDocumentElement.GetEditor"/>.
    /// </para>
    /// <para>
    /// The implementation is free to use whatever controls are appropriate to support the properties of the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// Changes made to the values of the controls must not be applied to the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> until <see cref="ApplyChanges"/>
    /// is called. If possible and appropriate, the implementation should support the <see cref="N:Digitalis.LDTools.DOM.API.Analytics"/> system.
    /// </para>
    /// </remarks>
    public interface IElementEditor
    {
        /// <summary>
        /// Occurs when the <see cref="IElementEditor"/>'s value(s) change.
        /// </summary>
        event EventHandler ValuesChanged;

        /// <summary>
        /// Returns the <see cref="T:System.Windows.Forms.UserControl"/> which provides the UI for the <see cref="IElementEditor"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The control must provide values for <see cref="P:System.Windows.Forms.Control.MinimumSize"/> and <see cref="P:System.Windows.Forms.Control.MaximumSize"/>.
        /// If the control is not resizable then <see cref="P:System.Windows.Forms.Control.MinimumSize"/> and <see cref="P:System.Windows.Forms.Control.MaximumSize"/>
        /// must be set to the same value.
        /// </para>
        /// <para>
        /// The control may manage its own size, and is free to change this at any time. Applications should handle the <see cref="E:System.Windows.Forms.Control.SizeChanged"/>
        /// event and update their layout as required.
        /// </para>
        /// </remarks>
        UserControl Control { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IElementEditor"/> should perform <see cref="N:Digitalis.LDTools.DOM.API.Analytics"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default value is <c>false</c>.
        /// </para>
        /// </remarks>
        bool AnalyticsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Digitalis.LDTools.DOM.API.CodeStandards">mode</see> to be used when performing <see cref="N:Digitalis.LDTools.DOM.API.Analytics"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default value is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.Full"/>.
        /// </para>
        /// </remarks>
        CodeStandards AnalyticsMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current value(s) in the <see cref="IElementEditor"/> can be applied to its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This must return <c>false</c> when the current value(s) would result in a structural problem if applied, and <c>true</c> otherwise.
        /// </para>
        /// </remarks>
        bool IsValid { get; }

        /// <summary>
        /// Gets a value indicating whether the current value(s) in the <see cref="IElementEditor"/> differ from those of its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the current value(s) in the <see cref="IElementEditor"/> are different to the current value(s) of its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>,
        /// this must return <c>true</c>. Note that the values of the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> may <see cref="ApplyChanges">change</see> during
        /// the lifetime of the <see cref="IElementEditor"/>, so they must be read each time this property is retrieved.
        /// </para>
        /// </remarks>
        bool HasChanges { get; }

        /// <summary>
        /// Applies the current value(s) in the <see cref="IElementEditor"/> to its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.</exception>
        void ApplyChanges();

        /// <summary>
        /// Gets a value indicating whether the current value(s) in the <see cref="IElementEditor"/> differ from those when it was created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the current value(s) in the <see cref="IElementEditor"/> are different to the initial value(s) of its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>,
        /// this must return <c>true</c>.
        /// </para>
        /// </remarks>
        bool CanReset { get; }

        /// <summary>
        /// Resets the <see cref="IElementEditor"/> to the initial value(s) of its <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </summary>
        /// <exception cref="T:Digitalis.LDTools.DOM.API.ElementLockedException">The <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> is
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPageElement.IsLocked">locked</see>.</exception>
        void Reset();

        /// <summary>
        /// Gets or sets the colour that should be used to highlight errors the <see cref="IElementEditor"/> wants to bring to the user's attention.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="IElementEditor"/> implements <see cref="N:Digitalis.LDTools.DOM.API.Analytics"/>, this colour may be used to highlight UI
        /// elements which are currently in an <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> state. Editors should prefer to use
        /// <see cref="T:Digitalis.LDTools.Controls.AnalyticsLabel"/>, as this provides a user-friendly description of the problems, but use of this colour may be
        /// appropriate where the <see cref="T:Digitalis.LDTools.Controls.AnalyticsLabel"/> covers multiple controls, or applies only to part of a multi-value control,
        /// and the editor wishes to highlight only the affected area.
        /// </para>
        /// <para>
        /// Default value is <see cref="F:System.Drawing.Color.Empty"/>.
        /// </para>
        /// </remarks>
        Color ErrorHighlight { get; set; }

        /// <summary>
        /// Gets or sets the colour that should be used to highlight warnings the <see cref="IElementEditor"/> wants to bring to the user's attention.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="IElementEditor"/> implements <see cref="N:Digitalis.LDTools.DOM.API.Analytics"/>this colour may be used to highlight UI
        /// elements which are currently in a <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> state. Editors should prefer to use
        /// <see cref="T:Digitalis.LDTools.Controls.AnalyticsLabel"/>, as this provides a user-friendly description of the problems, but use of this colour may be
        /// appropriate where the <see cref="T:Digitalis.LDTools.Controls.AnalyticsLabel"/> covers multiple controls, or applies only to part of a multi-value control,
        /// and the editor wishes to highlight only the affected area.
        /// </para>
        /// <para>
        /// Default value is <see cref="F:System.Drawing.Color.Empty"/>.
        /// </para>
        /// </remarks>
        Color WarningHighlight { get; set; }
    }
}
