#region License

//
// IMetaCommand.cs
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
    /// Represents an LDraw meta-command.
    /// </summary>
    /// <remarks>
    ///
    /// This is the base-type for all meta-commands which do not generate graphical elements (for these, see
    /// <see cref="ICompositeElement"/>) and which do not appear in the
    /// <see href="http://www.ldraw.org/article/398.html">header section</see> of an LDraw file (for these, see the properties
    /// of <see cref="IPage"/>).
    /// <p/>
    /// Implementations of <b>IMetaCommand</b> must be decorated with a number of <see cref="System.Attribute"/>s:
    /// <p/>
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="System.ComponentModel.Composition.ExportAttribute"/></term>
    ///         <description>
    ///             Required for the plugins-system to discover the class. Must have the value <c>typeof(IMetaCommand)</c>, and
    ///             may appear only once.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TypeNameAttribute"/></term>
    ///         <description>
    ///             Specifies the type-name of the <b>IMetaCommand</b>. This is usually set to the same value as
    ///             <see cref="IDocumentElement.TypeName"/>. May appear only once.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DefaultIconAttribute"/></term>
    ///         <description>
    ///             Specifies the icon used to represent instances of the <b>IMetaCommand</b>. This is usually set to the same
    ///             value as <see cref="IDocumentElement.Icon"/>. May appear only once.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="MetaCommandPatternAttribute"/></term>
    ///         <description>
    ///             Specifies a regular-expression which will match the LDraw code of the meta-command. Must appear at least
    ///             once, and must specify the <b>full</b> text of the meta-command - for example, <c>"^0\s+CLEAR\s*$"</c>.
    ///         </description>
    ///     </item>
    /// </list>
    /// <p/>
    /// Like all pluggable <see cref="IElement"/>s, implementations of <b>IMetaCommand</b> must provide a constructor which
    /// takes a single parameter of type <see cref="System.String"/> and which is interpreted as the LDraw code required to
    /// instantiate the meta-command.
    /// <p/>
    /// <example>
    /// <code lang="csharp">
    /// <![CDATA[
    /// [Export(typeof(IMetaCommand)]
    /// [DefaultIcon(typeof(Resources), "ClearIcon")]
    /// [TypeName(typeof(Resources), "ClearTypeName")]
    /// [MetaCommandPattern(@"^0\s+CLEAR\s*$")]
    /// public class MyMetaCommand : IMetaCommand
    /// {
    ///     public MyMetaCommand()
    ///     {
    ///         // default ctor, required for IDOMObject.Clone() to function
    ///     }
    ///
    ///     public MyMetaCommand(string ldrawCode)
    ///     {
    ///         // LDraw-code ctor, required for all pluggable elements
    ///     }
    ///
    ///     ...
    /// }
    /// ]]>
    /// </code>
    /// </example>
    ///
    /// <h3>Self-description</h3>
    /// <b>IMetaCommand</b> returns the following values:
    /// <p/>
    /// <list type="table">
    ///     <listheader><term>Property</term><description>Value</description></listheader>
    ///     <item><term><see cref="IDOMObject.ObjectType"/></term><description><see cref="DOMObjectType.MetaCommand"/></description></item>
    ///     <item><term><see cref="IDOMObject.IsImmutable"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Icon"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.TypeName"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.Description"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IDocumentElement.ExtendedDescription"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsStateElement"/></term><description>Implementation-specific</description></item>
    ///     <item><term><see cref="IElement.IsTopLevelElement"/></term><description>Implementation-specific</description></item>
    /// </list>
    ///
    /// </remarks>
    public interface IMetaCommand : IGroupable
    {
    }
}
