#region License

//
// IStep.cs
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

    using Digitalis.UndoSystem;

    using OpenTK;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw <i>STEP</i> meta-command as defined in <see href="http://www.ldraw.org/article/218/#step">the LDraw.org File Format specification</see>, or
    /// an MLCad <i>ROTSTEP</i> meta-command as defined in <see href="http://www.lm-software.com/mlcad/Specification_V2.0.pdf">the MLCad specifications</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h2>Code-generation</h2>
    /// <see cref="IDOMObject.ToCode">ToCode()</see> generates code as defined by IElementCollection. If <i>codeFormat</i> is <see cref="CodeStandards.Full"/> or
    /// <see cref="CodeStandards.OfficialModelRepository"/> then either the <i>STEP</i> or <i>ROTSTEP</i> meta-command will be appended. Certain combinations of
    /// values for <see cref="Mode"/>, <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/> will be optimised:
    /// <list type="table">
    ///     <listheader>
    ///         <term><see cref="Mode"/></term><term><see cref="X"/></term><term><see cref="Y"/></term><term><see cref="Z"/></term><term>Output</term>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="StepMode.Additive"/></term><term><c>0.0</c></term><term><c>0.0</c></term><term><c>0.0</c></term><term><i>0 STEP</i></term>
    ///     </item>
    ///     <item>
    ///         <term><see cref="StepMode.Relative"/></term><term><c>0.0</c></term><term><c>0.0</c></term><term><c>0.0</c></term><term><i>0 ROTSTEP END</i></term>
    ///     </item>
    /// </list>
    /// If the IStep is <see cref="IPageElement.IsLocked">locked</see> <see cref="IPageElement.IsLocalLock">explicitly</see> the <i>'0 !DIGITALIS_LDTOOLS_DOM LOCKNEXT'</i>
    /// meta-command will be appended immediately before the <i>STEP</i> or <i>ROTSTEP</i>.
    /// <p/>
    /// If the IStep is the last member of an <see cref="IPage"/> and would normally output <i>0 STEP</i> then this will be suppressed as the meta-command would have no
    /// effect in this position.
    ///
    /// <h2>Disposal</h2>
    /// <see cref="M:System.IDisposable.Dispose">Disposing</see> of an IStep will automatically remove it from its <see cref="Page"/> and dispose of its descendants. If
    /// the document-tree is <see cref="IDOMObject.IsFrozen"/>, or <see cref="Page"/> is <see cref="IDOMObject.IsImmutable">immutable</see> or
    /// <see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>, <see cref="M:System.IDisposable.Dispose"/> will throw the appropriate exception
    /// and the disposal will not take place.
    ///
    /// <h2>Document-tree</h2>
    /// <see cref="IPageElement.Step"/> always returns <c>null</c>.
    ///
    /// <h2>Self-description</h2>
    /// IStep returns the following values:
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.Step"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElementCollection.AllowsTopLevelElements"/></term><description><c>true</c></description></item>
    ///     <item>
    ///         <term><see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly"/></term>
    ///         <description><c>true</c> if <see cref="IDOMObject.IsImmutable"/> is <c>true</c>; otherwise implementation-specific</description>
    ///     </item>
    /// </list>
    ///
    /// </remarks>
    public interface IStep : IElementCollection
    {
        #region Document-tree

        /// <summary>
        /// Gets or sets the <see cref="IPage"/> the <see cref="IStep"/> is a member of.
        /// </summary>
        /// <exception cref="ObjectFrozenException">The property is set and either the <see cref="IStep"/> or the <see cref="IPage"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.NotSupported">The property is set and the <see cref="IPage"/> is <see cref="IDOMObject.IsImmutable">immutable</see> or
        ///     <see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IPage"/> is <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.InvalidOperationException">The property is set and the <see cref="IStep"/> <see cref="IDOMObjectCollection{T}.CanReplace">cannot be added</see>
        ///     to the <see cref="IPage"/> for some other reason.</exception>
        /// <remarks>
        /// Setting this property is equivalent to calling <see cref="M:System.Collections.Generic.ICollection{T}.Add"/> or
        /// <see cref="M:System.Collections.Generic.ICollection{T}.Remove"/> on the supplied <see cref="IPage"/> and passing in the <see cref="IStep"/>.
        /// <p/>
        /// It is possible to set this property if the <see cref="IStep"/> is <see cref="IPageElement.IsLocked">locked</see> or <see cref="IDOMObject.IsImmutable">immutable</see>,
        /// but not if it is <see cref="IDOMObject.IsFrozen">frozen</see>.
        /// <p/>
        /// If the <see cref="IStep"/> is <see cref="IDOMObject.Clone">cloned</see> or serialized, the value of this property will only be preserved if the <see cref="IPage"/> is
        /// included in the same operation.
        /// <p/>
        /// <see cref="M:System.IDisposable.Dispose">Disposing</see> of an <see cref="IStep"/> will automatically remove it from its Page. If the page is
        /// <see cref="IDOMObject.IsImmutable">immutable</see> or <see cref="P:System.Collections.Generic.ICollection{T}.IsReadOnly">read-only</see>,
        /// <see cref="M:System.IDisposable.Dispose"/> will throw the appropriate exception and the disposal will not take place.
        /// <p/>
        /// Raises the <see cref="PageChanged"/> and <see cref="IDocumentElement.PathToDocumentChanged"/> events when its value changes.
        /// <p/>
        /// Default value is <c>null</c>.
        /// </remarks>
        new IPage Page { get; set; }

        /// <summary>
        /// Occurs when <see cref="Page"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<IPage> PageChanged;

        #endregion Document-tree

        #region Properties

        /// <summary>
        /// Gets or sets the operating-mode.
        /// </summary>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IStep"/> is <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property is set and the supplied value is not a valid <see cref="StepMode"/>.</exception>
        /// <remarks>
        /// <para>
        /// Raises the <see cref="ModeChanged"/> event when its value changes.
        /// </para>
        /// <para>
        /// Default value is <see cref="StepMode.Additive"/>.
        /// </para>
        /// </remarks>
        StepMode Mode { get; set; }

        /// <summary>
        /// Occurs when <see cref="Mode"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<StepMode> ModeChanged;

        /// <summary>
        /// Gets or sets the angle of rotation around the X-axis.
        /// </summary>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IStep"/> is <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property is set and the supplied value was less than <c>-360.0</c> or greater than <c>360.0</c>.</exception>
        /// <remarks>
        /// Angles are specified in degrees and are in the range <c>-360.0</c> to <c>360.0</c>.
        /// <p/>
        /// Raises the <see cref="XChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>0.0</c>.
        /// </remarks>
        double X { get; set; }

        /// <summary>
        /// Occurs when <see cref="X"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<double> XChanged;

        /// <summary>
        /// Gets or sets the angle of rotation around the Y-axis.
        /// </summary>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IStep"/> is <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property is set and the supplied value was less than <c>-360.0</c> or greater than <c>360.0</c>.</exception>
        /// <remarks>
        /// Angles are specified in degrees and are in the range <c>-360.0</c> to <c>360.0</c>.
        /// <p/>
        /// Raises the <see cref="YChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>0.0</c>.
        /// </remarks>
        double Y { get; set; }

        /// <summary>
        /// Occurs when <see cref="Y"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<double> YChanged;

        /// <summary>
        /// Gets or sets the angle of rotation around the Z-axis.
        /// </summary>
        /// <exception cref="ObjectFrozenException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsFrozen">frozen</see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="IStep"/> is <see cref="IDOMObject.IsImmutable">immutable</see>.</exception>
        /// <exception cref="ElementLockedException">The property is set and the <see cref="IStep"/> is <see cref="IPageElement.IsLocked">locked</see>.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The property is set and the supplied value was less than <c>-360.0</c> or greater than <c>360.0</c>.</exception>
        /// <remarks>
        /// Angles are specified in degrees and are in the range <c>-360.0</c> to <c>360.0</c>.
        /// <p/>
        /// Raises the <see cref="ZChanged"/> event when its value changes.
        /// <p/>
        /// Default value is <c>0.0</c>.
        /// </remarks>
        double Z { get; set; }

        /// <summary>
        /// Occurs when <see cref="Z"/> changes.
        /// </summary>
        event PropertyChangedEventHandler<double> ZChanged;

        /// <summary>
        /// Gets the view-transform that the <see cref="IStep"/> represents.
        /// </summary>
        /// <remarks>
        /// This is transform represented by <see cref="Mode"/>, <see cref="X"/>, <see cref="Y"/> and <see cref="Z"/>.
        /// </remarks>
        Matrix4d StepTransform { get; }

        #endregion Properties

        #region View-transform

        /// <summary>
        /// Gets the cumulative view-transform represented by the <see cref="IStep"/> and, if present, its predecessors in the containing <see cref="IPage"/>.
        /// </summary>
        /// <param name="initialTransform">The initial display-transform.</param>
        /// <param name="viewTransform">The cumulative view-transform.</param>
        /// <param name="isAbsolute">When calling GetViewTransform() this must be set to <c>true</c> if <paramref name="initialTransform"/> ended
        ///     with an <see cref="StepMode.Absolute"/> component and <c>false</c> otherwise. On return it will be set to <c>true</c>
        ///     if <paramref name="viewTransform"/> ends with an <see cref="StepMode.Absolute"/> component; and <c>false</c> otherwise.</param>
        /// <remarks>
        /// <para>
        /// Starting with <paramref name="initialTransform"/>, the <see cref="StepTransform"/> of each successive <see cref="IStep"/> in <see cref="Page"/> is applied
        /// and the result returned.  If <see cref="Page"/> is <c>null</c>, just the <see cref="StepTransform"/> of this <see cref="IStep"/> is applied.
        /// </para>
        /// <para>
        /// If <paramref name="viewTransform"/> ended with an <see cref="StepMode.Absolute"/> component then <paramref name="isAbsolute"/> will
        /// be <c>true</c> and renderers should disable any camera positioning they would otherwise apply to the display.
        /// </para>
        /// </remarks>
        void GetViewTransform(ref Matrix4d initialTransform, out Matrix4d viewTransform, ref bool isAbsolute);

        #endregion View-transform
    }
}
