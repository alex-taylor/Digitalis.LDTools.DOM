#region License

//
// MetaCommand.cs
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

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    /// <summary>
    /// Abstract implementation of <see cref="Digitalis.LDTools.DOM.API.IMetaCommand"/>.
    /// </summary>
    [Serializable]
    public abstract class MetaCommand : Groupable, IMetaCommand
    {
        #region Self-description

        /// <inheritdoc />
        public sealed override DOMObjectType ObjectType { get { return DOMObjectType.MetaCommand; } }

        #endregion Self-description
    }
}
