#region License

//
// CanInsertHelper.cs
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
    using System.IO;

    #endregion Usings

    /// <summary>
    /// Provides extension-methods for the <b>CanInsert()</b> APIs on <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>, <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>
    ///     and <see cref="T:Digitalis.LDTools.DOM.API.IElementCollection"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These methods provide the implementation for the various <b>CanInsert()</b> APIs. They have been implemented as extension-methods to allow other implementations
    /// of the <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>, <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> and <see cref="T:Digitalis.LDTools.DOM.API.IStep"/>
    /// interfaces to make use of them.
    /// </para>
    /// </remarks>
    public static class CanInsertHelper
    {
        #region CanReplacePage

        /// <summary>
        /// Provides an implementation of <see cref="M:Digitalis.LDTools.DOM.API.IDocument.CanInsert"/>
        /// </summary>
        /// <param name="document">The <see cref="T:Digitalis.LDTools.DOM.API.IDocument"/>.</param>
        /// <param name="pageToInsert">The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> to check.</param>
        /// <param name="pageToReplace">The <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> to replace, or <c>null</c> to check for an insertion.</param>
        /// <param name="flags">Flags to control the behaviour of the check.</param>
        /// <returns>A value indicating whether <paramref name="pageToInsert"/> may be added to <paramref name="document"/>.</returns>
        public static InsertCheckResult CanReplacePage(this IDocument document, IPage pageToInsert, IPage pageToReplace, InsertCheckFlags flags)
        {
            if (null == pageToInsert || pageToInsert.IsDisposed || document.IsFrozen || pageToInsert.IsFrozen)
                return InsertCheckResult.NotSupported;

            if (0 == (InsertCheckFlags.IgnoreCurrentCollection & flags) && null != pageToInsert.Document)
                return InsertCheckResult.AlreadyMember;

            if (null != pageToReplace && !document.Contains(pageToReplace))
                return InsertCheckResult.NotSupported;

            foreach (IPage page in document)
            {
                if (page != pageToInsert && page != pageToReplace && page.TargetName.Equals(pageToInsert.TargetName, StringComparison.OrdinalIgnoreCase))
                    return InsertCheckResult.DuplicateName;
            }

            return CheckCollectionForInsert(document, pageToInsert.Elements, pageToInsert.TargetName, pageToReplace);
        }

        private static InsertCheckResult CheckCollectionForInsert(IDocument document, IDOMObjectCollection<IElement> collection, string pageName, IPage pageToReplace)
        {
            IElementCollection childCollection;
            IReference reference;
            InsertCheckResult result;
            IPage target;

            foreach (IElement element in collection)
            {
                childCollection = element as IElementCollection;

                if (null != childCollection)
                {
                    result = CheckCollectionForInsert(document, childCollection, pageName, pageToReplace);

                    if (InsertCheckResult.CanInsert != result)
                        return result;
                }
                else
                {
                    reference = element as IReference;

                    if (null != reference)
                    {
                        target = document[reference.TargetName];

                        if (null != target)
                        {
                            if (ContainsReferenceTo(target.Elements, pageName, pageToReplace))
                                return InsertCheckResult.CircularReference;
                        }
                    }
                }
            }

            return InsertCheckResult.CanInsert;
        }

        private static bool ContainsReferenceTo(IDOMObjectCollection<IElement> collection, string targetName, IPage pageToReplace)
        {
            IElementCollection childCollection;
            IReference reference;
            IPage target;

            foreach (IElement element in collection)
            {
                childCollection = element as IElementCollection;

                if (null != childCollection)
                {
                    if (ContainsReferenceTo(childCollection, targetName, pageToReplace))
                        return true;
                }
                else
                {
                    reference = element as IReference;

                    if (null != reference)
                    {
                        if (targetName.Equals(reference.TargetName, StringComparison.OrdinalIgnoreCase))
                            return true;

                        try
                        {
                            target = reference.Target;

                            if (null != target && target != pageToReplace && ContainsReferenceTo(target.Elements, targetName, pageToReplace))
                                return true;
                        }
                        catch (CircularReferenceException)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion CanReplacePage

        #region CanReplaceStep

        /// <summary>
        /// Provides an implementation of <see cref="M:Digitalis.LDTools.DOM.API.IPage.CanReplace"/>
        /// </summary>
        /// <param name="page">The <see cref="T:Digitalis.LDTools.DOM.IPage"/>.</param>
        /// <param name="stepToInsert">The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> to check.</param>
        /// <param name="stepToReplace">The <see cref="T:Digitalis.LDTools.DOM.API.IStep"/> to replace, or <c>null</c> to check for an insertion.</param>
        /// <param name="flags">Flags to control the behaviour of the check.</param>
        /// <returns>A value indicating whether <paramref name="stepToInsert"/> may be added to <paramref name="page"/>.</returns>
        public static InsertCheckResult CanReplaceStep(this IPage page, IStep stepToInsert, IStep stepToReplace, InsertCheckFlags flags)
        {
            InsertCheckResult result;

            if (null == stepToInsert || stepToInsert.IsDisposed || page.IsFrozen || stepToInsert.IsFrozen)
                return InsertCheckResult.NotSupported;

            if (0 == (InsertCheckFlags.IgnoreCurrentCollection & flags) && null != stepToInsert.Page)
                return InsertCheckResult.AlreadyMember;

            if (null != stepToReplace && !page.Contains(stepToReplace))
                return InsertCheckResult.NotSupported;

            foreach (IStep step in page)
            {
                if (step != stepToReplace)
                {
                    foreach (IElement element in stepToInsert)
                    {
                        result = step.CanInsert(element, flags | InsertCheckFlags.IgnoreCurrentCollection);

                        if (InsertCheckResult.CanInsert != result)
                            return result;
                    }
                }
            }

            return InsertCheckResult.CanInsert;
        }

        #endregion CanReplaceStep

        #region CanReplaceElement

        /// <summary>
        /// Provides an implementation of <see cref="M:Digitalis.LDTools.DOM.API.IElementCollection.CanReplace"/>
        /// </summary>
        /// <param name="collection">The <see cref="T:Digitalis.LDTools.DOM.IElementCollection"/>.</param>
        /// <param name="elementToInsert">The <see cref="T:Digitalis.LDTools.DOM.API.IElement"/> to check.</param>
        /// <param name="elementToReplace">The <see cref="T:Digitalis.LDTools.DOM.API.IElement"/> to replace, or <c>null</c> to check for an insertion.</param>
        /// <param name="flags">Flags to control the behaviour of the check.</param>
        /// <returns>A value indicating whether <paramref name="elementToInsert"/> may be added to <paramref name="collection"/>.</returns>
        public static InsertCheckResult CanReplaceElement(this IElementCollection collection, IElement elementToInsert, IElement elementToReplace, InsertCheckFlags flags)
        {
            if (null == elementToInsert || elementToInsert.IsDisposed || (0 == (InsertCheckFlags.IgnoreIsLocked & flags) && collection.IsLocked) || elementToInsert.IsFrozen)
                return InsertCheckResult.NotSupported;

            if (elementToInsert.IsTopLevelElement && !collection.AllowsTopLevelElements)
                return InsertCheckResult.TopLevelElementNotAllowed;

            if (collection is IElement && (collection as IElement) == elementToInsert)
                return InsertCheckResult.NotSupported;

            if (0 == (InsertCheckFlags.IgnoreCurrentCollection & flags) && null != elementToInsert.Parent)
                return InsertCheckResult.AlreadyMember;

            if (null != elementToReplace && !collection.Contains(elementToReplace))
                return InsertCheckResult.NotSupported;

            IColour colour = elementToInsert as IColour;

            if (null != colour && LDColour.IsDirectColour(colour.Code))
                return InsertCheckResult.NotSupported;

            IGroup group = elementToInsert as IGroup;

            if (null != group && !CheckGroupNameIsUnique(group, collection.Page, group.Name))
                return InsertCheckResult.DuplicateName;

            if (null == collection.Page)
                return InsertCheckResult.CanInsert;

            return CanInsertElement(collection, elementToInsert, collection.Page.TargetName.ToLower());
        }

        private static bool CheckGroupNameIsUnique(IGroup elementToInsert, IPage page, string name)
        {
            if (null == page)
                return true;

            IGroup group;

            foreach (IElement element in page.Elements)
            {
                if (element != elementToInsert)
                {
                    group = element as IGroup;

                    if (null != group && group.Name == name)
                        return false;
                }
            }

            return true;
        }

        // check the element (or its contents if it's a collection) to ensure it would not create a circular reference back to the containing page
        private static InsertCheckResult CanInsertElement(IElementCollection collection, IElement element, string containingPageName)
        {
            IReference reference = element as IReference;

            if (null != reference)
                return CanInsertReference(collection, reference, containingPageName);

            IElementCollection subCollection = element as IElementCollection;

            if (null != subCollection)
            {
                // check that none of the collection's descendants will create a circular reference either
                InsertCheckResult result;

                foreach (IElement child in subCollection)
                {
                    result = CanInsertElement(collection, child, containingPageName);

                    if (InsertCheckResult.CanInsert != result)
                        return result;
                }
            }

            return InsertCheckResult.CanInsert;
        }

        // as we process each IReference, add it to a cache so we can detect multi-link circular references in the tree
        [ThreadStatic]
        private static Dictionary<string, string> CircularReferences;

        // check an IReference to ensure it would not create a circular reference back to the containing page
        private static InsertCheckResult CanInsertReference(IElementCollection collection, IReference reference, string containingPageName)
        {
            string targetName = reference.TargetName.ToLower();

            // check that the reference doesn't point at a page which points back to the containing page
            IPage target = null;

            try
            {
                if (null == CircularReferences)
                    CircularReferences = new Dictionary<string, string>();

                if (CircularReferences.ContainsKey(containingPageName) || targetName == containingPageName)
                    return InsertCheckResult.CircularReference;

                // TODO: find out whether the CircularReferences dictionary is still needed
                //CircularReferences.Add(containingPageName, String.Empty);

                // check local targets first
                IDocument doc = collection.Document;

                if (null != doc)
                {
                    if (Path.IsPathRooted(targetName) && targetName == doc.Filepath.ToLower())
                        return InsertCheckResult.CircularReference;

                    target = doc[targetName];
                }

                if (null == target)
                    target = reference.Target;

                if (TargetStatus.CircularReference == reference.TargetStatus)
                    return InsertCheckResult.CircularReference;
            }
            catch (CircularReferenceException)
            {
                return InsertCheckResult.CircularReference;
            }
            finally
            {
                if (null != CircularReferences)
                {
                    CircularReferences.Remove(containingPageName);

                    if (0 == CircularReferences.Count)
                        CircularReferences = null;
                }
            }

            // in order to speed things up, we assume that library-files will not contain circular references
            if (null != target && !target.Document.IsLibraryPart)
                return CanInsertReferenceTarget(collection, target.Elements, containingPageName);

            return InsertCheckResult.CanInsert;
        }

        // check the supplied collection for circular references
        private static InsertCheckResult CanInsertReferenceTarget(IElementCollection collection, IDOMObjectCollection<IElement> targetElements, string containingPageName)
        {
            IReference reference;
            IElementCollection subCollection;
            InsertCheckResult result;

            foreach (IElement el in targetElements)
            {
                reference = el as IReference;

                if (null != reference)
                {
                    result = CanInsertReference(collection, reference, containingPageName);

                    if (InsertCheckResult.CanInsert != result)
                        return result;
                }

                subCollection = el as IElementCollection;

                if (null != subCollection)
                {
                    result = CanInsertReferenceTarget(collection, subCollection, containingPageName);

                    if (InsertCheckResult.CanInsert != result)
                        return result;
                }
            }

            return InsertCheckResult.CanInsert;
        }

        #endregion CanReplaceElement
    }
}
