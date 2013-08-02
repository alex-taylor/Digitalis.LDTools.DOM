#region License

//
// IDocumentElement.cs
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
    using System.Collections.Generic;
    using System.Drawing;

    using Digitalis.LDTools.DOM.API.Analytics;

    #endregion Usings

    /// <summary>
    /// Represents a descendant of an <see cref="IDocument"/>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Self-description</h3>
    /// <b>IDocumentElement</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="ExtendedDescription"/></term><description>Implementation-specific</description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IDocumentElement : IDOMObject
    {
        #region Analytics

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDocumentElement"/> has detected any problems with its values.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocumentElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="mode">The mode to run the checks in. See <see cref="Analyse">Analyse()</see> for details.</param>
        /// <returns>A value indicating whether problems were detected.</returns>
        /// <remarks>
        /// <para>
        /// Details of any problems detected may be obtained by calling <see cref="Analyse">Analyse()</see>.
        /// </para>
        /// </remarks>
        bool HasProblems(CodeStandards mode);

        /// <summary>
        /// Checks the <see cref="IDocumentElement"/> for problems.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocumentElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <param name="mode">The mode to run the checks in.</param>
        /// <returns>A collection of <see cref="Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s describing any
        ///     problems detected with the <see cref="IDocumentElement"/>.</returns>
        /// <remarks>
        /// The values for <paramref name="mode"/> are hierarchical: <see cref="CodeStandards.OfficialModelRepository"/> is a
        /// superset of <see cref="CodeStandards.PartsLibrary"/>, which is a superset of <see cref="CodeStandards.Full"/>.
        /// <see cref="CodeStandards.Full"/> is thus the most lax and <see cref="CodeStandards.OfficialModelRepository"/> the
        /// most strict. In general, the following will be reported for each mode:
        /// <p/>
        /// <list type="table">
        ///   <item>
        ///     <term><see cref="CodeStandards.Full"/></term>
        ///     <description>
        ///       Anything that contravenes the
        ///       <see href="http://www.ldraw.org/article/218.html">LDraw.org File Format specification</see>.
        ///       Deprecated syntax will be reported as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>s, all
        ///       others as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>s.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CodeStandards.PartsLibrary"/></term>
        ///     <description>
        ///       As for <see cref="CodeStandards.Full"/>, plus anything that contravenes the
        ///       <see href="http://www.ldraw.org/article/512.html">LDraw.org File Format Restrictions for the Official Library</see>.
        ///       Deprecated syntax will be reported as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>s, all
        ///       others as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>s.
        ///       <p/>
        ///       Some issues reported as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>s when in
        ///       <see cref="CodeStandards.Full"/> mode may have their severity increased to
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="CodeStandards.OfficialModelRepository"/></term>
        ///     <description>
        ///       As for <see cref="CodeStandards.PartsLibrary"/>, plus anything that contravenes the
        ///       <see href="http://www.ldraw.org/article/593.html">Official Model Repository (OMR) Specification</see>.
        ///       Deprecated syntax will be reported as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>s, all
        ///       others as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>s.
        ///       <p/>
        ///       Some issues reported as <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/>s when in
        ///       <see cref="CodeStandards.Full"/> or <see cref="CodeStandards.PartsLibrary"/> mode may have their severity
        ///       increased to <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/>.
        ///     </description>
        ///   </item>
        /// </list>
        /// <p/>
        /// This method may be expensive to call, depending on what processing the <see cref="IDocumentElement"/> has to carry
        /// out to build the collection. To find out whether there are any problems without incurring the possible overheads of
        /// <b>Analyse()</b>, see <see cref="HasProblems"/>.
        /// <p/>
        /// If <see cref="HasProblems"/> is <c>false</c>, this method returns an empty
        /// <see cref="System.Collections.Generic.ICollection{T}"/>.
        /// </remarks>
        ICollection<IProblemDescriptor> Analyse(CodeStandards mode);

        #endregion Analytics

        #region Document-tree

        /// <summary>
        /// Gets the <see cref="IDocument"/> the <see cref="IDocumentElement"/> is a descendant of.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocumentElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the <see cref="IDocumentElement"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this
        /// property will only be preserved if the <see cref="IDocument"/> is included in the same operation.
        /// <p/>
        /// Raises the <see cref="PathToDocumentChanged"/> event when its value changes.
        /// </remarks>
        IDocument Document { get; }

        /// <summary>
        /// Occurs when the path from the <see cref="IDocumentElement"/> to its containing <see cref="Document"/> changes.
        /// </summary>
        /// <remarks>
        /// This event is raised when the <see cref="IDocumentElement"/> is added to or removed from a collection; or when its
        /// containing collection is added to or removed from its container; and so on.
        /// <p/>
        /// Note that this event does not participate in the <see cref="IDOMObject.Changed"/> propagation-mechanism: it is
        /// raised only on the <see cref="IDocumentElement"/> to which it relates.
        /// </remarks>
        event EventHandler PathToDocumentChanged;

        #endregion Document-tree

        #region Editor

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDocumentElement"/> provides an <see cref="IElementEditor"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocumentElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <remarks>
        /// If the <see cref="IDocumentElement"/> is <see cref="IDOMObject.IsFrozen">frozen</see> or
        /// <see cref="IDOMObject.IsImmutable">immutable</see> this always returns <c>false</c>; otherwise it is
        /// implementation-dependent.
        /// <p/>
        /// Implementations of <see cref="IDocumentElement"/> which have user-editable properties should provide an
        /// <see cref="IElementEditor"/> which allows the user to edit those properties, and implement this property to return
        /// <c>true</c>.
        /// <p/>
        /// Implementations which return <c>true</c> here must be decorated with an <see cref="ElementFlagsAttribute"/> with
        /// the <see cref="ElementFlags.HasEditor"/> flag set.
        /// </remarks>
        /// <seealso cref="GetEditor"/>
        bool HasEditor { get; }

        /// <summary>
        /// Creates an <see cref="IElementEditor"/> for the <see cref="IDocumentElement"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The <see cref="IDocumentElement"/> is
        ///     <see cref="IDOMObject.IsDisposed">disposed</see>.</exception>
        /// <returns>If <see cref="HasEditor"/> is <c>false</c>, returns <c>null</c>. Otherwise, returns an
        /// <see cref="IElementEditor"/>.</returns>
        /// <remarks>
        /// Implementations of <see cref="IDocumentElement"/> which have user-editable properties should provide an
        /// <see cref="IElementEditor"/> which allows the user to edit those properties, and implement this property to return
        /// an instance of it.
        /// </remarks>
        /// <seealso cref="HasEditor"/>
        IElementEditor GetEditor();

        #endregion Editor

        #region Self-description

        /// <summary>
        /// Gets an icon which represents the <see cref="IDocumentElement"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="IDocumentElement"/> supports it, this returns a 16x16 96dpi <see cref="T:System.Drawing.Image"/>
        /// which can be used to represent objects of the type; otherwise it returns <c>null</c>.
        /// <p/>
        /// As this value is normally a constant, it is safe to get when the <see cref="IDocumentElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: this should be supported if possible.
        /// </note>
        /// </remarks>
        Image Icon { get; }

        /// <summary>
        /// Gets the type-name of the <see cref="IDocumentElement"/> in a form suitable for display to the user.
        /// </summary>
        /// <remarks>
        /// As this value is normally a constant, it is safe to get when the <see cref="IDocumentElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: if possible, this string should be a noun and localized. For example: 'Part', 'Line',
        /// 'Colour Definition'.
        /// </note>
        /// </remarks>
        string TypeName { get; }

        /// <summary>
        /// Gets a description of the <see cref="IDocumentElement"/>.
        /// </summary>
        /// <remarks>
        /// This typically returns a summary of the element. <see cref="IDocumentElement"/>s which are adequately described by
        /// their <see cref="TypeName"/> may return <c>null</c> here.
        /// <p/>
        /// As this value is normally a constant, it is safe to get when the <see cref="IDocumentElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: if possible, this string should be localized.
        /// </note>
        /// </remarks>
        string Description { get; }

        /// <summary>
        /// Gets extended information for the <see cref="IDocumentElement"/>.
        /// </summary>
        /// <remarks>
        /// This typically returns additional information about the element, such as details of its usage.
        /// <see cref="IDocumentElement"/>s which do not have an extended description should return <c>null</c> here.
        /// <p/>
        /// As this value is normally a constant, it is safe to get when the <see cref="IDocumentElement"/> is
        /// <see cref="IDOMObject.IsDisposed">disposed</see>.
        /// <p/>
        /// <note>
        /// Note to implementors: if possible, this string should be localized.
        /// </note>
        /// </remarks>
        string ExtendedDescription { get; }

        #endregion Self-description
    }
}
