#region License

//
// Attributes.cs
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
    using System.ComponentModel;
    using System.Drawing;
    using System.Resources;

    #endregion Usings

    #region ElementFlags

    /// <summary>
    /// Specifies values used by <see cref="T:Digitalis.LDTools.DOM.API.ElementFlagsAttribute"/>.
    /// </summary>
    [Flags]
    public enum ElementFlags
    {
        /// <summary>
        /// Specifies that the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> has an <see cref="M:Digitalis.LDTools.DOM.API.IDocumentEditor.GetEditor">editor control</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this flag merely indicates that the type supports an editor control. The availability of an editor control for a specific instance
        /// is indicated by the <see cref="P:Digitalis.LDTools.DOM.API.IDocumentElement.HasEditor"/> property.
        /// </para>
        /// <note>
        /// This flag is only relevant to implementations of <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </note>
        /// </remarks>
        HasEditor = 1 << 0,

        /// <summary>
        /// Specifies that the <see cref="T:Digitalis.LDTools.DOM.API.IElement"/> is a top-level element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Top-level <see cref="T:Digitalis.LDTools.DOM.API.IElement"/>s can only be added to <see cref="T:Digitalis.LDTools.DOM.API.IElementCollection"/>s which
        /// <see cref="P:Digitalis.LDTools.DOM.API.IElementCollection.AllowsTopLevelElements">allow top-level elements</see>s; they may not be added
        /// to other types of <see cref="T:Digitalis.LDTools.DOM.API.IElementCollection"/>.
        /// </para>
        /// <note>
        /// This flag is only relevant to implementations of <see cref="T:Digitalis.LDTools.DOM.API.IElement"/>.
        /// </note>
        /// </remarks>
        TopLevelElement = 1 << 1,

        /// <summary>
        /// Specifies that the <see cref="T:Digitalis.LDTools.DOM.API.IDOMObject"/> is
        ///     <see cref="IDOMObject.IsImmutable">immutable</see>.
        /// </summary>
        Immutable = 1 << 2
    }

    #endregion ElementFlags

    #region MetaCommandPatternAttribute

    /// <summary>
    /// Specifies a regular-expression which can be used to identify a line of LDraw code supported by an <see cref="T:Digitalis.LDTools.DOM.API.IMetaCommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute must be specified at least once on the type, and may be specified as many times as required to cover all the options supported by the type.
    /// It must be a string suitable for passing to <see cref="M:System.Text.RegularExpressions.Regex(String)"/>, and may match all or part of a single line of LDraw
    /// code. It is recommended that the pattern be as inclusive as possible to reduce the risk of an ambiguous match.
    /// </para>
    /// <para>
    /// If the class supports more than one meta-command, or if a single pattern would be too complex to write, the attribute may be specified as many times as required
    /// to cover all cases.
    /// </para>
    /// <para>
    /// This attribute is only relevant to implementations of <see cref="T:Digitalis.LDTools.DOM.API.IMetacommand"/>, and is required to appear at least once.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple=true)]
    public class MetaCommandPatternAttribute : Attribute
    {
        /// <summary>
        /// Gets the pattern.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaCommandPatternAttribute"/> class with the specified values.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        public MetaCommandPatternAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }

    #endregion MetaCommandPatternAttribute

    #region ElementFlagsAttribute

    /// <summary>
    /// Specifies the flags supported by the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is optional and may appear no more than once.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class ElementFlagsAttribute : Attribute
    {
        /// <summary>
        /// Gets the flags.
        /// </summary>
        public ElementFlags Flags { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementFlagsAttribute"/> class with the specified values.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public ElementFlagsAttribute(ElementFlags flags)
        {
            Flags = flags;
        }
    }

    #endregion ElementFlagsAttribute

    #region DefaultIconAttribute

    /// <summary>
    /// Specifies the default icon which represents objects of this <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is required and may appear only once.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultIconAttribute : Attribute
    {
        /// <summary>
        /// Returns the <see cref="T:System.Drawing.Image"/> represented by the <see cref="DefaultIconAttribute"/>.
        /// </summary>
        public Image Icon { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultIconAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the resources class in the assembly which contains the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.</param>
        /// <param name="iconName">Name of the icon resource.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="iconName"/> must refer to an image resource of 16x16 pixels in the specified resources class.
        /// </para>
        /// </remarks>
        public DefaultIconAttribute(Type type, string iconName)
        {
            ResourceManager rm = new ResourceManager(type);
            Icon = rm.GetObject(iconName) as Image;
        }
    }

    #endregion DefaultIconAttribute

    #region TypeNameAttribute

    /// <summary>
    /// Specifies the default label which represents objects of this <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is required and may appear only once.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeNameAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNameAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the resources class in the assembly which contains the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.</param>
        /// <param name="resourceKey">Name of the string resource.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="resourceKey"/> must refer to a string resource in the specified resources class.
        /// The resulting label should be localized if possible.
        /// </para>
        /// </remarks>
        public TypeNameAttribute(Type type, string resourceKey)
            : base(new ResourceManager(type).GetString(resourceKey))
        {
        }
    }

    #endregion TypeNameAttribute

    #region ElementCategoryAttribute

    /// <summary>
    /// Specifies the category for objects of this <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value returned by this attribute may be used by applications to 'group' classes together for presentation to the user. It is a free-form string.
    /// </para>
    /// <para>
    /// This attribute is optional. If used, it may only appear once.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ElementCategoryAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementCategoryAttribute"/> class.
        /// </summary>
        /// <param name="type">Type of the resources class in the assembly which contains the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.</param>
        /// <param name="resourceKey">Name of the string resource.</param>
        /// <remarks>
        /// <para>
        /// <paramref name="resourceKey"/> must refer to a string resource in the specified resources class.
        /// The resulting label should be localized if possible.
        /// </para>
        /// </remarks>
        public ElementCategoryAttribute(Type type, string resourceKey)
            : base(new ResourceManager(type).GetString(resourceKey))
        {
        }
    }

    #endregion ElementCategoryAttribute
}
