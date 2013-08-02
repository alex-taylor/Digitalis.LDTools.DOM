#region License

//
// WindingModeHelper.cs
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
    using System.Collections.Generic;

    #endregion Usings

    /// <summary>
    /// Provides an extension-method for the <see cref="M:Digitalis.LDTools.DOM.API.IGeometric.WindingMode"/> API.
    /// </summary>
    public static class WindingModeHelper
    {
        /// <summary>
        /// Provides an implementation of <see cref="M:Digitalis.LDTools.DOM.API.IGeometric.WindingMode"/>
        /// </summary>
        /// <param name="geometric">The <see cref="T:Digitalis.LDTools.DOM.API.IGeometric"/>.</param>
        /// <param name="page">The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> of which the <see cref="T:Digitalis.LDTools.DOM.API.IGeometric"/> is a descendant, or <c>null</c>.</param>
        /// <param name="parent">The <see cref="T:Digitalis.LDTools.DOM.API.IElementCollection"/> of which the <see cref="T:Digitalis.LDTools.DOM.API.IGeometric"/> is a child, or <c>null</c>.</param>
        /// <returns>The <see cref="T:Digitalis.LDTools.DOM.API.CullingMode"/> which currently applies to <paramref name="geometric"/>.</returns>
        public static CullingMode GetWindingMode(this IGeometric geometric, IPage page, IElementCollection parent)
        {
            if (null == page)
                return CullingMode.NotSet;

            if (CullingMode.Disabled == page.BFC)
                return CullingMode.Disabled;

            // the page is either Certified or NotSet, so we need to walk the document-tree to find the last BFCFlag prior to us
            IBFCFlag    bfcFlag;
            CullingMode mode    = page.BFC;
            bool        enabled = (CullingMode.CertifiedClockwise == mode || CullingMode.CertifiedCounterClockwise == mode);

            if (null != parent)
            {
                List<IElementCollection> branches = new List<IElementCollection>();

                while (null != parent)
                {
                    if (parent.ContainsBFCFlagElements)
                        branches.Insert(0, parent);

                    parent = parent.Parent;
                }

                for (int idx = 0; idx < branches.Count; idx++)
                {
                    IElementCollection collection = branches[idx];
                    IElementCollection next       = (idx < branches.Count - 1) ? branches[idx + 1] : null;

                    foreach (IElement el in collection)
                    {
                        if (el == next)
                            break;

                        if (el == geometric)
                            return (enabled) ? mode : CullingMode.Disabled;

                        bfcFlag = el as IBFCFlag;

                        if (null != bfcFlag)
                        {
                            switch (bfcFlag.Flag)
                            {
                                case BFCFlag.SetWindingModeClockwise:
                                    mode = CullingMode.CertifiedClockwise;
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeClockwise:
                                    mode = CullingMode.CertifiedClockwise;
                                    enabled = true;
                                    break;

                                case BFCFlag.SetWindingModeCounterClockwise:
                                    mode = CullingMode.CertifiedCounterClockwise;
                                    break;

                                case BFCFlag.EnableBackFaceCullingAndSetWindingModeCounterClockwise:
                                    mode = CullingMode.CertifiedCounterClockwise;
                                    enabled = true;
                                    break;

                                case BFCFlag.EnableBackFaceCulling:
                                    enabled = true;
                                    break;

                                case BFCFlag.DisableBackFaceCulling:
                                    enabled = false;
                                    break;
                            }
                        }
                    }
                }
            }

            // didn't find an explicit flag, so go with the page's setting
            return page.BFC;
        }
    }
}
