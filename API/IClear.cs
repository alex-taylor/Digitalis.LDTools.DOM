#region License

//
// IClear.cs
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
    /// Represents an LDraw <i>CLEAR</i> meta-command as defined in
    ///     <see href="http://www.ldraw.org/article/218/#clear">the LDraw.org File Format specification</see>.
    /// </summary>
    /// <remarks>
    ///
    /// <h3>Code-generation</h3>
    /// If <i>codeFormat</i> is either <see cref="CodeStandards.Full"/> or <see cref="CodeStandards.OfficialModelRepository"/>,
    /// <see cref="IDOMObject.ToCode">ToCode()</see> will append the <i>CLEAR</i> meta-command to <i>sb</i>.
    ///
    /// <h3>Self-description</h3>
    /// <b>IClear</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.MetaCommand"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description><c>false</c></description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description><c>false</c></description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IClear : IMetaCommand
    {
    }
}
