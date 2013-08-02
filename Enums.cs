#region License

//
// Enums.cs
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

    #endregion Usings

    #region CatalogType

    /// <summary>
    /// Specifies the type of an <see cref="T:Digitalis.LDTools.DOM.LDTranslationCatalog"/>.
    /// </summary>
    public enum CatalogType
    {
        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.LDTranslationCatalog"/> contains translations for <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/>.
        /// </summary>
        Titles,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.LDTranslationCatalog"/> contains translations for <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/>.
        /// </summary>
        Categories,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.LDTranslationCatalog"/> contains translations for <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/>.
        /// </summary>
        Keywords,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.LDTranslationCatalog"/> contains translations for <see cref="P:Digitalis.LDTools.DOM.API.IColour.Name"/>.
        /// </summary>
        Colours
    }

    #endregion CatalogType

    #region ParseFlags

    /// <summary>
    /// Specifies flags to be passed to the
    ///     <see cref="M:Digitalis.LDTools.DOM.LDDocument(TextReader, string, ParserProgressCallback, ParseFlags, out bool)">LDDocument file-parsing constructor</see>.
    /// </summary>
    [Flags]
    public enum ParseFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The parser should replace any references to <i>~Moved to</i> parts with the replacement part.
        /// </summary>
        FollowRedirects = 1 << 0,

        /// <summary>
        /// The parser should replace any references to <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/> and
        ///     <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Alias"/> parts with the original part.
        /// </summary>
        FollowAliases = 1 << 1
    }

    #endregion ParseFlags

    #region TargetRedirectType

    /// <summary>
    /// Specifies the type of redirection when <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to a "~Moved to" or Alias part.
    /// </summary>
    public enum TargetRedirectType
    {
        /// <summary>
        /// No redirection is present.
        /// </summary>
        NoRedirect,

        /// <summary>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to a "~Moved to" part.
        /// </summary>
        MovedTo,

        /// <summary>
        /// <see cref="P:Digitalis.LDTools.DOM.API.IReference.TargetName"/> refers to an <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/> or
        ///     <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Alias"/> part.
        /// </summary>
        Alias
    }

    #endregion TargetRedirectType
}
