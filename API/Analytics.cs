#region License

//
// Analytics.cs
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

namespace Digitalis.LDTools.DOM.API.Analytics
{
    #region Usings

    using System;
    using System.Collections.Generic;

    #endregion Usings

    #region Severity

    /// <summary>
    /// Specifies identifiers to describe the severity of an <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// The issue the <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/> describes is for information purposes only. This is
        /// something that is permitted by the LDraw specification but which should be flagged up to the user.
        /// </summary>
        Information,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/> describes a warning. This is usually something permitted but
        /// discouraged by the LDraw specification.
        /// </summary>
        Warning,

        /// <summary>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/> describes an error. Normally this means a violation of the
        /// LDraw specification.
        /// </summary>
        Error
    };

    #endregion Severity

    #region IProblemDescriptor

    /// <summary>
    /// Describes an issue and optionally provides a means to fix it.
    /// </summary>
    public interface IProblemDescriptor
    {
        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="IProblemDescriptor"/>s that describe a specific type of problem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value returned can be used to discover the type of problem the <see cref="IProblemDescriptor"/> refers to. Implementations of
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> will declare a publically-accessible <see cref="T:System.Guid"/> for each problem-type
        /// they recognise, and this value will be used in each <see cref="IProblemDescriptor"/> created for that problem-type by that
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </para>
        /// <para>
        /// The value is only valid for the current session, so it should not be saved and reloaded.
        /// </para>
        /// </remarks>
        Guid Guid { get; }

        /// <summary>
        /// Gets the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> to which this problem applies.
        /// </summary>
        IDocumentElement Element { get; }

        /// <summary>
        /// Gets a value indicating the severity of the problem.
        /// </summary>
        Severity Severity { get; }

        /// <summary>
        /// Gets a description of the problem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This should be in a form suitable for display to the user and should be kept short and to the point. For example,
        /// "The Quadrilateral is bow-tied".
        /// </para>
        /// </remarks>
        string Description { get; }

        /// <summary>
        /// Gets a collection of fixes for the problem.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If no fixes are available, or if the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> is immutable, returns <c>null</c>.
        /// </para>
        /// </remarks>
        IEnumerable<IFixDescriptor> Fixes { get; }
    }

    #endregion IProblemDescriptor

    #region IFixDescriptor

    /// <summary>
    /// Describes a fix for an <see cref="IProblemDescriptor"/> and provides the means to execute it.
    /// </summary>
    public interface IFixDescriptor
    {
        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="IFixDescriptor"/>s that describe a specific type of fix.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value returned can be used to discover the type of fix the <see cref="IFixDescriptor"/> refers to. Implementations of
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> will declare a publically-accessible <see cref="T:System.Guid"/> for each fix-type
        /// they offer, and this value will be used in each <see cref="IFixDescriptor"/> created for that fix-type by that
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>.
        /// </para>
        /// <para>
        /// The value is only valid for the current session, so it should not be saved and reloaded.
        /// </para>
        /// </remarks>
        Guid Guid { get; }

        /// <summary>
        /// Gets a description of the action to be performed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This should be in a form suitable for display to the user and should be kept short and to the point; "Fix this"
        /// is perfectly adequate for the majority of cases. It should be written as an imperative.
        /// </para>
        /// </remarks>
        string Instruction { get; }

        /// <summary>
        /// Gets a description of the action taken if the fix was successful.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned string should be in a form suitable for display to the user and should be kept short and to the point.
        /// It should be specific and written in the present tense: "Fix bow-tied quad" rather than "Fixed problem".
        /// </para>
        /// </remarks>
        string Action { get; }

        /// <summary>
        /// Returns a value indicating whether the fix is intra-element or inter-element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An intra-element fix modifies only the <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>; inter-element fixes need to make changes to other
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/>s, such as the containing collection - for example, a fix which needs to delete the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> is an inter-element fix, as it needs to modify the collection rather than the
        /// <see cref="T:Digitalis.LDTools.DOM.API.IDocumentElement"/> which generated the <see cref="IFixDescriptor"/>.
        /// </para>
        /// </remarks>
        bool IsIntraElement { get; }

        /// <summary>
        /// Attempts to apply the fix.
        /// </summary>
        /// <returns><c>true</c> if the fix succeeded; <c>false</c> otherwise.</returns>
        bool Apply();
    }

    #endregion IFixDescriptor
}
