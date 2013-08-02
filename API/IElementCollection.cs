#region License

//
// IElementCollection.cs
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
    /// <summary>
    /// Represents a collection of <see cref="IElement"/>s.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Code-generation</h3>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> iterates over the contents of the <b>IElementCollection</b> and generates
    /// code for each in turn, passing in the parameters it was given. <see cref="ILine"/>s and <see cref="IOptionalLine"/>s
    /// with a <see cref="IGraphic.ColourValue"/> of <see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/> will be treated as
    /// if the <b>IElementCollection</b> was an <see cref="IReference"/>: that is, if <i>overrideColour</i> is set to anything
    /// other than <see cref="Digitalis.LDTools.DOM.Palette.MainColour"/> or
    /// <see cref="Digitalis.LDTools.DOM.Palette.EdgeColour"/> the <see cref="ILine"/> / <see cref="IOptionalLine"/> code will
    /// use the complement of the specified colour. If this is not an index into the current palette then it will be converted
    /// to a <i>Direct Colours</i> value. If <i>overrideColour</i> is itself a <i>Direct Colours</i> value then the complement
    /// is defined as <c>#2000000</c>.
    /// <p/>
    /// If <i>codeFormat</i> is <see cref="CodeStandards.PartsLibrary"/> the resulting LDraw code will be optimised by
    /// eliminating or modifying <see cref="IBFCFlag"/> elements which would either have no effect or else simply change the
    /// winding-direction.
    /// <p/>
    /// Code-generation for the <see cref="IPageElement.IsLocked"/> property is implementation-specific.
    ///
    /// <h3>Collection-management</h3>
    /// In addition to the restrictions imposed by
    /// <see cref="IDOMObjectCollection{T}.CanReplace">IDOMObjectCollection&lt;T&gt;.CanReplace()</see>, an
    /// <see cref="IElement"/> may only be added to an <b>IElementCollection</b> if:
    /// <p/>
    /// <list type="bullet">
    ///     <item><term>The <b>IElementCollection</b> is not <see cref="IDOMObject.IsFrozen">frozen</see></term></item>
    ///     <item><term>The <b>IElementCollection</b> is not <see cref="IDOMObject.IsImmutable">immutable</see></term></item>
    ///     <item>
    ///         <term>
    ///             The <b>IElementCollection</b> is not
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </term>
    ///     </item>
    ///     <item>
    ///         <term>
    ///             It would not create a circular reference; for example, an <see cref="IReference"/> whose
    ///             <see cref="IReference.Target"/> is the <see cref="IPage"/> to which it is being added
    ///         </term>
    ///     </item>
    ///     <item>
    ///         <term>
    ///             It is not a <see cref="IElement.IsTopLevelElement">top-level element</see>, unless the
    ///             <b>IElementCollection</b>&#160;<see cref="AllowsTopLevelElements">allows</see> these
    ///         </term>
    ///     </item>
    ///     <item><term>It is not a <i>Direct Colours</i>&#160;<see cref="IColour"/></term></item>
    /// </list>
    /// <p/>
    /// Additionally, an <see cref="IGroup"/> may only be added if the containing <see cref="IPage"/> or <see cref="IStep"/>, if
    /// any, does not already contain an <see cref="IGroup"/> with the same <see cref="IGroup.Name"/>.
    /// <p/>
    /// Specifying <see cref="InsertCheckFlags.IgnoreIsLocked"/> in the <i>flags</i> parameter of
    /// <see cref="IDOMObjectCollection{T}.CanReplace">CanReplace()</see> allows you to determine whether an
    /// <see cref="IElement"/> may be added to a <see cref="IPageElement.IsLocked">locked</see>&#160;<b>IElementCollection</b>
    /// without having to first unlock.
    /// <p/>
    /// The members of <see cref="System.Collections.Generic.IList{T}"/> will throw exceptions for the following conditions,
    /// checked for in this order:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="System.ObjectDisposedException"/></term>
    ///         <description>
    ///             The <b>IElementCollection</b> is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IElement"/> to be inserted is <see cref="IDOMObject.IsDisposed">disposed</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ObjectFrozenException"/></term>
    ///         <description>
    ///             The <b>IElementCollection</b> is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <see cref="IElement"/> to be inserted is <see cref="IDOMObject.IsFrozen">frozen</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.NotSupportedException"/></term>
    ///         <description>
    ///             The <b>IElementCollection</b> is <see cref="IDOMObject.IsImmutable">immutable</see>
    ///             <p/>
    ///             <i>- or -</i>
    ///             <p/>
    ///             The <b>IElementCollection</b> is
    ///             <see cref="System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ElementLockedException"/></term>
    ///         <description>The <b>IElementCollection</b> is <see cref="IPageElement.IsLocked">locked</see></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.ArgumentNullException"/></term>
    ///         <description>An attempt was made to insert a <c>null</c> value into the <b>IElementCollection</b></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="System.InvalidOperationException"/></term>
    ///         <description>
    ///             The insert is prohibited by any of the other restrictions imposed by
    ///             <see cref="IDOMObjectCollection{T}.CanReplace">CanReplace()</see>
    ///         </description>
    ///     </item>
    /// </list>
    /// <p/>
    /// Implementations and subtypes of <b>IElementCollection</b> may add further restrictions.
    /// <p/>
    /// When adding or removing <see cref="IElement"/>s, the <b>IElementCollection</b> will raise one of
    /// <see cref="IDOMObjectCollection{T}.ItemsAdded"/>, <see cref="IDOMObjectCollection{T}.ItemsRemoved"/>,
    /// <see cref="IDOMObjectCollection{T}.ItemsReplaced"/> or <see cref="IDOMObjectCollection{T}.CollectionCleared"/> and each
    /// <see cref="IElement"/> will raise <see cref="IElement.ParentChanged"/>. These events will also be raised via the
    /// <b>IElementCollection</b>'s <see cref="IDOMObject.Changed"/> event, but are not guaranteed to appear in any particular
    /// order.
    ///
    /// <h3>Disposal</h3>
    /// <see cref="System.IDisposable.Dispose">Disposing</see> an <b>IElementCollection</b> will also dispose its descendants.
    ///
    /// <h3>Geometry</h3>
    /// <see cref="IGeometric.BoundingBox"/> is the union of the bounding-boxes of the <see cref="IGeometric"/> members of the
    /// <b>IElementCollection</b>.
    /// <p/>
    /// <see cref="IGeometric.Origin"/> is the centre of <see cref="IGeometric.BoundingBox"/>.
    /// <p/>
    /// <see cref="IGeometric.Transform">IGeometric.Transform()</see> and <see cref="IGeometric.ReverseWinding"/> act on each
    /// <see cref="IGeometric"/> member of the <b>IElementCollection</b> in turn.
    ///
    /// <h3>Self-description</h3>
    /// <b>IElementCollection</b> returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElementCollection.AllowsTopLevelElements"/></term><description>Implementation-specific</description></item>
    ///     <item>
    ///         <term><see cref="System.Collections.Generic.ICollection{T}.IsReadOnly"/></term>
    ///         <description><c>true</c> if <see cref="IDOMObject.IsImmutable"/> is <c>true</c>; otherwise implementation-specific</description>
    ///     </item>
    /// </list>
    ///
    /// </remarks>
    public interface IElementCollection : IPageElement, IGeometric, IDOMObjectCollection<IElement>
    {
        #region Collection-management

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElementCollection"/> contains <see cref="IColour"/> elements.
        /// </summary>
        bool ContainsColourElements { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElementCollection"/> contains <see cref="IBFCFlag"/> elements.
        /// </summary>
        bool ContainsBFCFlagElements { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElementCollection"/> contains
        ///     <see cref="IPageElement.IsLocked">locked</see> descendants.
        /// </summary>
        /// <remarks>
        /// The check is recursive: if the <see cref="IElementCollection"/> contains other <see cref="IElementCollection"/>s,
        /// their <see cref="IElement"/>s will be checked, and so on.
        /// <p/>
        /// If this property returns <c>true</c> then <see cref="IGeometric.Transform">Transform()</see> and
        /// <see cref="IGeometric.ReverseWinding">ReverseWinding()</see> will throw an <see cref="ElementLockedException"/> when
        /// called.
        /// </remarks>
        bool HasLockedDescendants { get; }

        #endregion Collection-management

        #region Document-tree

        /// <summary>
        /// Gets the <see cref="IElementCollection"/> the <see cref="IElementCollection"/> is a child of.
        /// </summary>
        /// <remarks>
        /// If the <see cref="IElementCollection"/> is a child of an <see cref="IStep"/> then this returns the same value as
        /// <see cref="IPageElement.Step"/>.
        /// <p/>
        /// If the <see cref="IElementCollection"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of
        /// this property will only be preserved if the parent collection is included in the same operation.
        /// <p/>
        /// Raises the <see cref="IDocumentElement.PathToDocumentChanged"/> event when its value changes.
        /// </remarks>
        IElementCollection Parent { get; }

        #endregion Document-tree

        #region Self-description

        /// <summary>
        /// Gets a value indicating whether the <see cref="IElementCollection"/> allows
        ///     <see cref="IElement.IsTopLevelElement">top-level</see> elements to be added to it.
        /// </summary>
        bool AllowsTopLevelElements { get; }

        #endregion Self-description
    }
}
