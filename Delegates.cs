#region License

//
// Delegates.cs
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
    #region ParserProgressCallback

    /// <summary>
    /// Represents a method which may be notified of the progress of an <see cref="T:Digitalis.LDTools.DOM.LDDocument"/> parse operation.
    /// </summary>
    /// <param name="name">The name of the LDraw element currently being parsed.</param>
    /// <param name="progress">The percentage of the operation which has completed.</param>
    /// <returns><c>true</c> to continue the operation; <c>false</c> to cancel it.</returns>
    /// <remarks>
    /// <para>
    /// The delegate may choose to cancel the operation at any time by returning <c>false</c>
    /// </para>
    /// </remarks>
    public delegate bool ParserProgressCallback(string name, int progress);

    #endregion ParserProgressCallback
}
