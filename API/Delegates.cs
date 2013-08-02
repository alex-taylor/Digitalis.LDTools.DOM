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

namespace Digitalis.LDTools.DOM.API
{
    #region Usings

    using System.IO;

    #endregion Usings

    #region DocumentWriterCallback

    /// <summary>
    /// Represents a method to create a <see cref="T:System.IO.TextWriter"/> to which an <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> may be written.
    /// </summary>
    /// <param name="targetName">The <see cref="P:Digitalis.LDTools.DOM.API.IPage.TargetName"/> of the <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> to be written.</param>
    /// <remarks>
    /// <para>
    /// Used by <see cref="M:Digitalis.LDTools.DOM.API.IDocument.Export(Digitalis.LDTools.DOM.API.DocumentWriterCallback)"/> and
    /// <see cref="M:Digitalis.LDTools.DOM.API.IDocument.Publish(Digitalis.LDTools.DOM.API.DocumentWriterCallback)"/> to obtain
    /// <see cref="T:System.IO.TextWriter"/>s to output their files to.
    /// </para>
    /// </remarks>
    public delegate TextWriter DocumentWriterCallback(string targetName);

    #endregion DocumentWriterCallback
}
