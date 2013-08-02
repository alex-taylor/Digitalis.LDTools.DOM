#region License

//
// LDPage.cs
//
// Copyright (C) 2009-2012 Alex Taylor.  All Rights Reserved.
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using OpenTK;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.API.Analytics;
    using Digitalis.LDTools.DOM.Geom;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.UndoSystem;

    #endregion Usings

    /// <summary>
    /// Implements <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [TypeName(typeof(Resources), "Page")]
    [DefaultIcon(typeof(Resources), "ModelIcon")]
    [ElementFlags(ElementFlags.HasEditor)]
    public sealed class LDPage : DocumentElement, IPage
    {
        #region Inner types

        /// <summary>
        /// Specifies the problems found with <see cref="Name"/> during <see cref="IsNameFormatInvalid">analysis</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Other than the restrictions imposed by the <see cref="M:System.IO.Path.GetInvalidFileNameChars">filesystem</see>, <see cref="Name"/> is free-form. However, for certain
        /// <see cref="PageType"/>s of <see cref="LDPage"/> it should conform to a specific format and should not contain characters disallowed by
        /// <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </para>
        /// <para>
        /// The following formats are recognised by <see cref="LDPage"/>, and <see cref="IsNameFormatInvalid"/> will check for them:
        /// <list type="bullet">
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Part"/></term><description><i>[prefix]nnnnn[suffix][pattern-marker][shortcut-marker]</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/></term><description><i>nnnnn[suffix]</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Part_Physical_Colour"/></term><description><i>nnnnn</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/></term><description><i>[prefix]nnnnn[suffix][pattern-marker][shortcut-marker]</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Alias"/></term><description><i>nnnnn</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Physical_Colour"/></term><description><i>[prefix]nnnnn[suffix][pattern-marker][shortcut-marker]</i></description></item>
        ///   <item><term><see cref="Digitalis.LDTools.DOM.API.PageType.Subpart"/></term><description><i>[prefix]nnnnn[suffix][pattern-marker]subpart-marker</i></description></item>
        /// </list>
        /// where:
        /// <list type="bullet">
        ///   <item><term><i>prefix</i> is one of <i>u</i>, <i>x</i> or <i>s</i></term></item>
        ///   <item><term><i>nnnnn</i> is one or more digits</term></item>
        ///   <item><term><i>suffix</i> is any letter</term></item>
        ///   <item><term><i>pattern-marker</i> is a <i>p</i> followed by two or more letters or digits; the letters <i>i</i>, <i>l</i>, <i>o</i> and <i>p</i> may only be used if the first character after the <i>p</i> is a <i>t</i>, a <i>u</i> or a <i>v</i></term></item>
        ///   <item><term><i>shortcut-marker</i> is a <i>c</i> or a <i>d</i> followed by two or more digits</term></item>
        ///   <item><term><i>subpart-marker</i> is an <i>s</i> followed by two or more digits</term></item>
        /// </list>
        /// and <i>[...]</i> denotes that the token is optional. For all other <see cref="Digitalis.LDTools.DOM.API.PageType"/>s, <see cref="Name"/> is free-form.
        /// </para>
        /// </remarks>
        /// <seealso href="http://www.ldraw.org/library/tracker/ref/numberfaq/">FAQ for LDraw Part Numbers</seealso>
        /// <seealso href="http://www.ldraw.org/library/tracker/ref/patterns/">LDraw.org Parts Tracker Patterned Part Information</seealso>
        /// <seealso href="http://www.ldraw.org/article/562.html">Parts numbering scheme for parts with unknown numbers</seealso>
        [Flags]
        public enum NameFormatProblem
        {
            /// <summary>
            /// No problems were found.
            /// </summary>
            None = 0,

            /// <summary>
            /// <see cref="Name"/> could not be parsed as one of the recognised formats.
            /// </summary>
            Unrecognised = 1 << 0,

            /// <summary>
            /// <see cref="Name"/> contains characters not allowed by <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
            /// </summary>
            Invalid_Chars = 1 << 1,

            /// <summary>
            /// <see cref="Name"/> should contain a subpart-marker but does not.
            /// </summary>
            Missing_SubpartMarker = 1 << 2,

            /// <summary>
            /// <see cref="Name"/> should not contain a prefix but one was found.
            /// </summary>
            Unneeded_Prefix = 1 << 3,

            /// <summary>
            /// <see cref="Name"/> should not contain a suffix but one was found.
            /// </summary>
            Unneeded_Suffix = 1 << 4,

            /// <summary>
            /// <see cref="Name"/> should not contain a subpart-marker but one was found.
            /// </summary>
            Unneeded_SubpartMarker = 1 << 5,

            /// <summary>
            /// <see cref="Name"/> should not contain a shortcut-marker but one was found.
            /// </summary>
            Unneeded_ShortcutMarker = 1 << 6,

            /// <summary>
            /// <see cref="Name"/> should not contain a pattern-marker but one was found.
            /// </summary>
            Unneeded_PatternMarker = 1 << 7,

            /// <summary>
            /// <see cref="Name"/> contains an invalid prefix character.
            /// </summary>
            Invalid_Prefix = 1 << 8,

            /// <summary>
            /// <see cref="Name"/> contains an invalid suffix character.
            /// </summary>
            Invalid_Suffix = 1 << 9,

            /// <summary>
            /// <see cref="Name"/> contains an incorrectly-formatted subpart-marker.
            /// </summary>
            Invalid_SubpartMarker = 1 << 10,

            /// <summary>
            /// <see cref="Name"/> contains an incorrectly-formatted or positioned shortcut-marker.
            /// </summary>
            Invalid_ShortcutMarker = 1 << 11,

            /// <summary>
            /// <see cref="Name"/> contains an incorrectly-formatted or positioned pattern-marker.
            /// </summary>
            Invalid_PatternMarker = 1 << 12,

            /// <summary>
            /// <see cref="Name"/> contains more than one subpart-marker.
            /// </summary>
            Duplicate_SubpartMarker = 1 << 13,

            /// <summary>
            /// <see cref="Name"/> contains more than one shortcut-marker.
            /// </summary>
            Duplicate_ShortcutMarker = 1 << 14,

            /// <summary>
            /// <see cref="Name"/> contains more than one pattern-marker.
            /// </summary>
            Duplicate_PatternMarker = 1 << 15,

            /// <summary>
            /// The markers in <see cref="Name"/> appear in the wrong order.
            /// </summary>
            Invalid_MarkerSequence = 1 << 16
        }

        /// <summary>
        /// Specifies the problems found with <see cref="Title"/> during <see cref="IsTitleFormatInvalid">analysis</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Title"/> is generally a free-form string, but certain <see cref="PageType"/>s of <see cref="LDPage"/> should conform to a specific format:
        /// <list type="bullet">
        ///   <item>
        ///     <term><see cref="Digitalis.LDTools.DOM.API.PageType.Part_Physical_Colour"/></term>
        ///     <description>the <see cref="Title"/> should start with an underscore ('_') character and end with the colour-value enclosed in square brackets</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Physical_Colour"/></term>
        ///     <description>the <see cref="Title"/> should start with an underscore ('_') character and end with the colour-value enclosed in square brackets</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Digitalis.LDTools.DOM.API.PageType.Subpart"/></term>
        ///     <description>the <see cref="Title"/> should start with a tilde ('~') character</description>
        ///   </item>
        /// </list>
        /// Single-digit numbers in <see cref="Title"/>, unless they are the first character, should be separated from the previous token by two space characters;
        /// otherwise tokens in <see cref="Title"/> should be separated by a single space character.
        /// </para>
        /// </remarks>
        [Flags]
        public enum TitleFormatProblem
        {
            /// <summary>
            /// No problems were found.
            /// </summary>
            None = 0,

            /// <summary>
            /// <see cref="Title"/> should start with an underscore character ('_'), but does not.
            /// </summary>
            Missing_Underscore = 1 << 0,

            /// <summary>
            /// <see cref="Title"/> should start with a tilde character ('~'), but does not.
            /// </summary>
            Missing_Tilde = 1 << 1,

            /// <summary>
            /// <see cref="Title"/> should end with a colour-value marker, but does not.
            /// </summary>
            Missing_ColourValue = 1 << 2,

            /// <summary>
            /// <see cref="Title"/> should not start with an underscore character ('_'), but one was found.
            /// </summary>
            Unneeded_Underscore = 1 << 3,

            /// <summary>
            /// <see cref="Title"/> should not start with a tilde character ('~'), but one was found.
            /// </summary>
            Unneeded_Tilde = 1 << 4,

            /// <summary>
            /// <see cref="Title"/> should not end with a colour-value marker, but one was found.
            /// </summary>
            Unneeded_ColourValue = 1 << 5,

            /// <summary>
            /// <see cref="Title"/> should have leading spaces before single-digit numbers, but some are missing.
            /// </summary>
            Missing_SpaceBeforeNumber = 1 << 6,

            /// <summary>
            /// <see cref="Title"/> contains excess whitespace.
            /// </summary>
            Unneeded_Spaces = 1 << 7
        }

        // Rule:   'Name' should only contain [A-Za-z0-9_-] and should follow the standard naming format for patterns, subparts, shortcuts etc
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name, http://www.ldraw.org/library/tracker/ref/numberfaq/, http://www.ldraw.org/library/tracker/ref/patterns/
        private class NameFormatInvalidProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_NameFormatInvalid; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public NameFormatInvalidProblem(LDPage page, NameFormatProblem problem, char prefix, char suffix, string allMarkers, string patternMarker, string shortcutMarker, string subpartMarker)
            {
                Element = page;

                List<IFixDescriptor> fixes = new List<IFixDescriptor>();
                StringBuilder sb           = new StringBuilder();
                string separator           = ": ";

                sb.Append(Resources.Analytics_PageName_InvalidFormat);

                if (NameFormatProblem.Unrecognised == problem)
                {
                    sb.Append(separator);

                    switch (page.PageType)
                    {
                        case PageType.Part_Physical_Colour:
                        case PageType.Shortcut_Alias:
                            sb.Append(Resources.Analytics_PageName_Unrecognised_Simple);
                            break;

                        case PageType.Subpart:
                            sb.Append(Resources.Analytics_PageName_Unrecognised_Subpart);
                            break;

                        case PageType.Part_Alias:
                            sb.Append(Resources.Analytics_PageName_Unrecognised_Alias);
                            break;

                        default:
                            sb.Append(Resources.Analytics_PageName_Unrecognised_Part);
                            break;
                    }
                }
                else
                {
                    if (0 != (NameFormatProblem.Invalid_Chars & problem))
                    {
                        sb.Append(separator).Append(Resources.Analytics_PageName_InvalidChars);
                        separator = "; ";
                    }

                    if (0 != (NameFormatProblem.Unneeded_Prefix & problem))
                    {
                        sb.Append(separator).AppendFormat(Resources.Analytics_PageName_UnneededPrefix, prefix);
                        separator = "; ";
                        fixes.Add(new RemovePrefix(page));
                    }
                    else if (0 != (NameFormatProblem.Invalid_Prefix & problem))
                    {
                        sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidPrefix, prefix);
                        separator = "; ";
                        fixes.Add(new RemovePrefix(page));
                        fixes.Add(new ChangePrefix(page, 'u'));
                        fixes.Add(new ChangePrefix(page, 'x'));
                        fixes.Add(new ChangePrefix(page, 's'));
                    }

                    if (0 != (NameFormatProblem.Unneeded_Suffix & problem))
                    {
                        sb.Append(separator).AppendFormat(Resources.Analytics_PageName_UnneededSuffix, suffix);
                        separator = "; ";
                        fixes.Add(new RemoveSuffix(page));
                    }
                    if (0 != (NameFormatProblem.Invalid_Suffix & problem))
                    {
                        sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidSuffix, suffix);
                        separator = "; ";
                        fixes.Add(new RemoveSuffix(page));
                    }

                    if (0 != (NameFormatProblem.Invalid_MarkerSequence & problem))
                    {
                        string correctMarkers;

                        if (PageType.Subpart == page.PageType)
                            correctMarkers = patternMarker + subpartMarker;
                        else
                            correctMarkers = patternMarker + shortcutMarker;

                        sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidMarkerSequence, allMarkers, correctMarkers);
                        fixes.Add(new SwapMarkers(page, allMarkers, correctMarkers));
                    }
                    else
                    {
                        if (0 != (NameFormatProblem.Missing_SubpartMarker & problem))
                        {
                            sb.Append(separator).Append(Resources.Analytics_PageName_MissingSubpartMarker);
                            separator = "; ";
                        }
                        else if (0 != (NameFormatProblem.Unneeded_SubpartMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_UnneededSubpartMarker, subpartMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, subpartMarker));
                        }
                        else if (0 != (NameFormatProblem.Invalid_SubpartMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidSubpartMarker, subpartMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, subpartMarker));
                        }
                        else if (0 != (NameFormatProblem.Duplicate_SubpartMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_DuplicateMarkers, subpartMarker);
                            separator = "; ";
                        }

                        if (0 != (NameFormatProblem.Unneeded_ShortcutMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_UnneededShortcutMarker, shortcutMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, shortcutMarker));
                        }
                        else if (0 != (NameFormatProblem.Invalid_ShortcutMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidShortcutMarker, shortcutMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, shortcutMarker));
                        }
                        else if (0 != (NameFormatProblem.Duplicate_ShortcutMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_DuplicateMarkers, shortcutMarker);
                            separator = "; ";
                        }

                        if (0 != (NameFormatProblem.Unneeded_PatternMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_UnneededPatternMarker, patternMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, patternMarker));
                        }
                        else if (0 != (NameFormatProblem.Invalid_PatternMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_InvalidPatternMarker, patternMarker);
                            separator = "; ";
                            fixes.Add(new RemoveMarker(page, patternMarker));
                        }
                        else if (0 != (NameFormatProblem.Duplicate_PatternMarker & problem))
                        {
                            sb.Append(separator).AppendFormat(Resources.Analytics_PageName_DuplicateMarkers, patternMarker);
                        }
                    }
                }

                Description = sb.ToString();

                if (fixes.Count > 0)
                    Fixes = fixes;
            }

            private class ChangePrefix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_NameChangePrefix; } }
                public string Instruction { get; private set; }
                public string Action { get { return Resources.Analytics_PageName_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _name;

                public ChangePrefix(LDPage page, char prefix)
                {
                    _page       = page;
                    _name       = prefix + page.Name.Substring(1);
                    Instruction = String.Format(Resources.Analytics_PageName_ChangePrefix, prefix);
                }

                public bool Apply()
                {
                    _page.Name = _name;
                    return true;
                }
            }

            private class RemoveSuffix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_NameRemoveSuffix; } }
                public string Instruction { get { return Resources.Analytics_PageName_RemoveSuffix; } }
                public string Action { get { return Resources.Analytics_PageName_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _name;

                public RemoveSuffix(LDPage page)
                {
                    _page = page;
                    _name = page.Name.Substring(0, page.Name.Length - 1);
                }

                public bool Apply()
                {
                    _page.Name = _name;
                    return true;
                }
            }

            private class RemovePrefix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_NameRemovePrefix; } }
                public string Instruction { get { return Resources.Analytics_PageName_RemovePrefix; } }
                public string Action { get { return Resources.Analytics_PageName_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _name;

                public RemovePrefix(LDPage page)
                {
                    _page = page;
                    _name = page.Name.Substring(1);
                }

                public bool Apply()
                {
                    _page.Name = _name;
                    return true;
                }
            }

            private class RemoveMarker : IFixDescriptor
            {
                public Guid Guid { get { return Fix_NameRemoveMarker; } }
                public string Instruction { get; private set; }
                public string Action { get { return Resources.Analytics_PageName_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _name;

                public RemoveMarker(LDPage page, string marker)
                {
                    _page       = page;
                    _name       = page.Name.Replace(marker, "");
                    Instruction = String.Format(Resources.Analytics_PageName_RemoveMarker, marker);
                }

                public bool Apply()
                {
                    _page.Name = _name;
                    return true;
                }
            }

            private class SwapMarkers : IFixDescriptor
            {
                public Guid Guid { get { return Fix_NameSwapMarkers; } }
                public string Instruction { get { return Resources.Analytics_PageName_SwapMarkers; } }
                public string Action { get { return Resources.Analytics_PageName_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _name;

                public SwapMarkers(LDPage page, string oldMarkers, string newMarkers)
                {
                    _page = page;
                    _name = page.Name.Replace(oldMarkers, newMarkers);
                }

                public bool Apply()
                {
                    _page.Name = _name;
                    return true;
                }
            }
        }

        // Rule:   'Name' should be no more than 21 characters for non-Models
        // Type:   Error
        // Source: http://www.ldraw.org/article/512.html#file_name
        private class NameTooLongProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_NameTooLong; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageName_TooLong; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public NameTooLongProblem(LDPage page)
            {
                Element = page;
            }
        }

        // Rule:   'Title' should follow the standards on whitespace, prefix etc
        // Type:   Error
        // Source: http://wiki.ldraw.org/index.php?title=Use_of_leading_space_for_numbers, undocumented conventions established on the Parts Tracker
        private class TitleFormatInvalidProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_TitleFormatInvalid; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get; private set; }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public TitleFormatInvalidProblem(LDPage page, TitleFormatProblem problem)
            {
                Element = page;

                List<IFixDescriptor> fixes = new List<IFixDescriptor>();
                StringBuilder sb           = new StringBuilder();
                string separator           = ": ";

                sb.Append(Resources.Analytics_PageTitle_InvalidFormat);

                if (0 != (TitleFormatProblem.Missing_SpaceBeforeNumber & problem) || 0 != (TitleFormatProblem.Unneeded_Spaces & problem))
                {
                    if (0 != (TitleFormatProblem.Missing_SpaceBeforeNumber & problem))
                        sb.Append(separator).Append(Resources.Analytics_PageTitle_MissingSpaceBeforeNumber);

                    separator = "; ";

                    if (0 != (TitleFormatProblem.Unneeded_Spaces & problem))
                        sb.Append(separator).Append(Resources.Analytics_PageTitle_UnneededWhitespace);

                    fixes.Add(new TitleCorrectSpacing(page));
                }

                if (0 != (TitleFormatProblem.Missing_ColourValue & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_MissingColourValue);
                    separator = "; ";

                    foreach (IStep step in page)
                    {
                        IReference r = (from n in step where n is IReference select n).FirstOrDefault() as IReference;

                        if (null != r && !LDColour.IsDirectColour(r.ColourValue))
                        {
                            fixes.Add(new TitleAddColourValue(page, r.ColourValue));
                            break;
                        }
                    }
                }
                else if (0 != (TitleFormatProblem.Unneeded_ColourValue & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_UnneededColourValue);
                    separator = "; ";
                    fixes.Add(new TitleRemoveColourValue(page));
                }

                if (0 != (TitleFormatProblem.Missing_Underscore & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_MissingUnderscore);
                    separator = "; ";
                    fixes.Add(new TitleAddUnderscore(page));
                }
                else if (0 != (TitleFormatProblem.Unneeded_Underscore & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_UnneededUnderscore);
                    separator = "; ";
                    fixes.Add(new TitleRemoveUnderscore(page));
                }

                if (0 != (TitleFormatProblem.Missing_Tilde & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_MissingTilde);
                    separator = "; ";
                    fixes.Add(new TitleAddTilde(page));
                }
                else if (0 != (TitleFormatProblem.Unneeded_Tilde & problem))
                {
                    sb.Append(separator).Append(Resources.Analytics_PageTitle_UnneededTilde);
                    separator = "; ";
                    fixes.Add(new TitleRemoveTilde(page));
                }

                Description = sb.ToString();

                if (fixes.Count > 0)
                    Fixes = fixes;
            }

            private class TitleAddUnderscore : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleAddUnderscore; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_AddUnderscore; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleAddUnderscore(LDPage page)
                {
                    _page  = page;
                    _title = "_" + page.Title;
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleRemoveUnderscore : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleRemoveUnderscore; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_RemoveUnderscore; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleRemoveUnderscore(LDPage page)
                {
                    _page  = page;
                    _title = page.Title.Substring(1);
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleAddTilde : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleAddTilde; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_AddTilde; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleAddTilde(LDPage page)
                {
                    _page  = page;
                    _title = "~" + page.Title;
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleRemoveTilde : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleRemoveTilde; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_RemoveTilde; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleRemoveTilde(LDPage page)
                {
                    _page  = page;
                    _title = page.Title.Substring(1);
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleAddColourValue : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleAddColourValue; } }
                public string Instruction { get; private set; }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleAddColourValue(LDPage page, uint colourValue)
                {
                    _page        = page;
                    _title       = page.Title + " [" + colourValue + "]";
                    Instruction  = String.Format(Resources.Analytics_PageTitle_AddColourValue, colourValue);
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleRemoveColourValue : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleRemoveColourValue; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_RemoveColourValue; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleRemoveColourValue(LDPage page)
                {
                    _page  = page;
                    _title = Regex_Title_ColourValue.Replace(page.Title, "");
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }

            private class TitleCorrectSpacing : IFixDescriptor
            {
                public Guid Guid { get { return Fix_TitleCorrectSpacing; } }
                public string Instruction { get { return Resources.Analytics_PageTitle_CorrectWhitespace; } }
                public string Action { get { return Resources.Analytics_PageTitle_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private string _title;

                public TitleCorrectSpacing(LDPage page)
                {
                    _page  = page;
                    _title = Regex_Whitespace.Replace(page.Title, " ");
                    _title = Regex_Title_MissingWhitespace.Replace(_title, "$1$&");
                }

                public bool Apply()
                {
                    _page.Title = _title;
                    return true;
                }
            }
        }

        // Rule:   Members of 'Keywords' should be unique
        // Type:   Error
        // Source: Convention established on the Parts Tracker
        private class KeywordDuplicatesProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_KeywordDuplicates; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageKeyword_Duplicates; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public KeywordDuplicatesProblem(LDPage page, uint[] keywords)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new KeywordRemoveUnneededEntries(page, keywords) };
            }
        }

        // Rule:   Members of 'Keywords' should not appear in 'Title'
        // Type:   Error
        // Source: Convention established on the Parts Tracker
        private class KeywordInTitleProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_KeywordInTitle; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageKeyword_InTitle; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public KeywordInTitleProblem(LDPage page, uint[] keywords)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new KeywordRemoveUnneededEntries(page, keywords) };
            }
        }

        // Rule:   'BFC' should be set explicitly
        // Type:   Warning
        // Source: http://www.ldraw.org/article/398.html, http://www.ldraw.org/article/415.html
        private class BFCMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_BFCMissing; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Warning; } }
            public string Description { get { return Resources.Analytics_PageBFC_Missing; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public BFCMissingProblem(LDPage page)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new BFCSetCounterClockwise(page), new BFCSetClockwise(page), new BFCSetUncertified(page) };
            }

            private class BFCSetClockwise : IFixDescriptor
            {
                public Guid Guid { get { return Fix_BFCSetClockwise; } }
                public string Instruction { get { return Resources.Analytics_PageBFC_SetClockwise; } }
                public string Action { get { return Resources.Analytics_PageBFC_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public BFCSetClockwise(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.BFC = CullingMode.CertifiedClockwise;
                    return true;
                }
            }

            private class BFCSetCounterClockwise : IFixDescriptor
            {
                public Guid Guid { get { return Fix_BFCSetCounterClockwise; } }
                public string Instruction { get { return Resources.Analytics_PageBFC_SetCounterClockwise; } }
                public string Action { get { return Resources.Analytics_PageBFC_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public BFCSetCounterClockwise(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.BFC = CullingMode.CertifiedCounterClockwise;
                    return true;
                }
            }

            private class BFCSetUncertified : IFixDescriptor
            {
                public Guid Guid { get { return Fix_BFCSetUncertified; } }
                public string Instruction { get { return Resources.Analytics_PageBFC_SetUncertified; } }
                public string Action { get { return Resources.Analytics_PageBFC_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public BFCSetUncertified(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.BFC = CullingMode.Disabled;
                    return true;
                }
            }
        }

        // Rule:   'BFC' should be set to CertifiedCounterClockwise for primitives
        // Type:   Error
        // Source: http://www.ldraw.org/article/398.html, http://www.ldraw.org/article/415.html
        private class BFCInvalidProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_BFCInvalid; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageBFC_Invalid; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public BFCInvalidProblem(LDPage page)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new Fix(page) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_BFCSetCounterClockwise; } }

                public string Instruction { get { return Resources.Analytics_FixThis; } }

                public string Action { get { return Resources.Analytics_PageBFC_Changed; } }

                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public Fix(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.BFC = CullingMode.CertifiedCounterClockwise;
                    return true;
                }
            }
        }

        // Rule:   'License' should be set to CCAL2 for files destined for the Parts Tracker or Official Model Repository
        // Type:   Error
        // Source: http://www.ldraw.org/article/398.html, http://www.ldraw.org/article/593.html#file_headers
        private class LicenseInvalidProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_LicenseInvalid; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageLicense_Invalid; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public LicenseInvalidProblem(LDPage page)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new Fix(page) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_LicenseSetCCAL2; } }
                public string Instruction { get { return Resources.Analytics_FixThis; } }
                public string Action { get { return Resources.Analytics_PageLicense_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public Fix(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.License = License.CCAL2;
                    return true;
                }
            }
        }

        // Rule:   'Category' should return a value other than Unknown
        // Type:   Error
        // Source: http://www.ldraw.org/article/398.html
        private class CategoryMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_CategoryMissing; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageCategory_Missing; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public CategoryMissingProblem(LDPage page)
            {
                Element = page;
            }
        }

        // Rule:   'Category' should not override 'Title'
        // Type:   Error
        // Source: Convention established on the Parts Tracker
        private class CategoryMismatchProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_CategoryMismatch; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Warning; } }
            public string Description { get { return Resources.Analytics_PageCategory_Mismatch; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public CategoryMismatchProblem(LDPage page)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new Fix(page) };
            }

            private class Fix : IFixDescriptor
            {
                public Guid Guid { get { return Fix_CategoryClear; } }
                public string Instruction { get { return Resources.Analytics_PageCategory_Clear; } }
                public string Action { get { return Resources.Analytics_PageCategory_Cleared; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;

                public Fix(LDPage page)
                {
                    _page = page;
                }

                public bool Apply()
                {
                    _page.Category = Category.Unknown;
                    return true;
                }
            }
        }

        // Rule:   'Author' should have a value
        // Type:   Error
        // Source: http://www.ldraw.org/article/398.html
        private class AuthorMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_AuthorMissing; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageAuthor_Missing; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public AuthorMissingProblem(LDPage page)
            {
                Element = page;
            }
        }

        // Rule:   'User' should have a value
        // Type:   Error
        // Source: http://www.ldraw.org/article/398.html
        private class UserMissingProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_UserMissing; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Error; } }
            public string Description { get { return Resources.Analytics_PageUser_Missing; } }
            public IEnumerable<IFixDescriptor> Fixes { get { return null; } }

            public UserMissingProblem(LDPage page)
            {
                Element = page;
            }
        }

        // Rule:   'Origin' should be within 'BoundingBox' for non-Models
        // Type:   Warning
        // Source: Convention established on the Parts Tracker
        private class OriginOutsideBoundingBoxProblem : IProblemDescriptor
        {
            public Guid Guid { get { return Problem_OriginOutsideBoundingBox; } }
            public IDocumentElement Element { get; private set; }
            public Severity Severity { get { return Severity.Warning; } }
            public string Description { get { return Resources.Analytics_PageOrigin_OutsideBoundingBox; } }
            public IEnumerable<IFixDescriptor> Fixes { get; private set; }

            public OriginOutsideBoundingBoxProblem(LDPage page)
            {
                Element = page;
                Fixes   = new IFixDescriptor[] { new OriginCentreTop(page), new OriginCentreBottom(page), new OriginCentreBoundingBox(page) };
            }

            private class OriginCentreTop : IFixDescriptor
            {
                public Guid Guid { get { return Fix_OriginCentreTop; } }
                public string Instruction { get { return Resources.Analytics_PageOrigin_SetCentreTop; } }
                public string Action { get { return Resources.Analytics_PageOrigin_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private Matrix4d _transform;

                public OriginCentreTop(LDPage page)
                {
                    _page           = page;
                    Box3d bounds    = page.BoundingBox;
                    Vector3d centre = bounds.Centre;
                    centre.Y        = bounds.Y1;
                    _transform      = Matrix4d.CreateTranslation(-centre);
                }

                public bool Apply()
                {
                    _page.Transform(ref _transform);
                    return true;
                }
            }

            private class OriginCentreBottom : IFixDescriptor
            {
                public Guid Guid { get { return Fix_OriginCentreBottom; } }
                public string Instruction { get { return Resources.Analytics_PageOrigin_SetCentreBottom; } }
                public string Action { get { return Resources.Analytics_PageOrigin_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private Matrix4d _transform;

                public OriginCentreBottom(LDPage page)
                {
                    _page = page;
                    Box3d bounds    = page.BoundingBox;
                    Vector3d centre = bounds.Centre;
                    centre.Y        = bounds.Y1 + bounds.SizeY;
                    _transform      = Matrix4d.CreateTranslation(-centre);
                }

                public bool Apply()
                {
                    _page.Transform(ref _transform);
                    return true;
                }
            }

            private class OriginCentreBoundingBox : IFixDescriptor
            {
                public Guid Guid { get { return Fix_OriginCentreBoundingBox; } }
                public string Instruction { get { return Resources.Analytics_PageOrigin_SetCentreBoundingBox; } }
                public string Action { get { return Resources.Analytics_PageOrigin_Changed; } }
                public bool IsIntraElement { get { return true; } }

                private LDPage _page;
                private Matrix4d _transform;

                public OriginCentreBoundingBox(LDPage page)
                {
                    _page           = page;
                    Vector3d centre = page.BoundingBox.Centre;
                    _transform      = Matrix4d.CreateTranslation(-centre);
                }

                public bool Apply()
                {
                    _page.Transform(ref _transform);
                    return true;
                }
            }
        }

        // for KeywordDuplicatesProblem & KeywordInTitleProblem
        private class KeywordRemoveUnneededEntries : IFixDescriptor
        {
            public Guid Guid { get { return Fix_KeywordRemoveUnneededEntries; } }
            public string Instruction { get { return Resources.Analytics_FixThis; } }
            public string Action { get { return Resources.Analytics_PageKeyword_Removed; } }
            public bool IsIntraElement { get { return true; } }

            private LDPage _page;
            private List<string> _keywords = new List<string>();

            public KeywordRemoveUnneededEntries(LDPage page, uint[] indices)
            {
                _page = page;

                List<string> keywords = new List<string>(page.Keywords);

                for (int idx = 0; idx < keywords.Count; idx++)
                {
                    if (!indices.Contains((uint)idx))
                        _keywords.Add(keywords[idx]);
                }
            }

            public bool Apply()
            {
                _page.Keywords = _keywords;
                return true;
            }
        }

        #endregion Inner types

        #region Analytics

        // characters not permitted for the 'Name' property at all
        private static readonly char[] InvalidNameChars = Path.GetInvalidFileNameChars();

        // non-Models must have Names which conform to these
        private const int Max_NameLength                        = 21;

        private static readonly Regex Regex_Name                = new Regex("^([^0-9])?([0-9]+)([^0-9])??([cds]..|p[^p].+?)*$", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Name_InvalidChars   = new Regex("^[-A-Za-z0-9_]+$", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Name_ShortcutMarker = new Regex("^[cd][0-9]+$", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Name_SubpartMarker  = new Regex("^s[0-9]+$", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Name_PatternMarker  = new Regex("^p([abcdefghjkmnqrswxyz0-9][abcdefghjkmnqrstuvwxyz0-9]+|[tuv][a-z0-9]+)$", RegexOptions.IgnoreCase);
        private static readonly string Name_Prefixes            = "sux";
        private static readonly string Name_Alphabet            = "abcdefghijklmnopqrstuvwyz";

        // non-Models must have Titles which confirm to these
        private static readonly Regex Regex_Title_ColourValue       = new Regex(@"\s\[\d+\]$", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Title_MissingWhitespace = new Regex(@"\b(\s)\d(\D|$)", RegexOptions.IgnoreCase);
        private static readonly Regex Regex_Title_ExcessWhitespace  = new Regex(@"(\s\s+\D|\s\s\d\d)", RegexOptions.IgnoreCase);

        private static readonly Regex Regex_Whitespace = new Regex(@"\s+");

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_NameFormatInvalid = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsNameTooLong"/> condition.
        /// </summary>
        public static readonly Guid Problem_NameTooLong = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_TitleFormatInvalid = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="HasKeywordDuplicates"/> condition.
        /// </summary>
        public static readonly Guid Problem_KeywordDuplicates = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="HasKeywordInTitle"/> condition.
        /// </summary>
        public static readonly Guid Problem_KeywordInTitle = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsBFCMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_BFCMissing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsBFCInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_BFCInvalid = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsLicenseInvalid"/> condition.
        /// </summary>
        public static readonly Guid Problem_LicenseInvalid = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsCategoryMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_CategoryMissing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsCategoryMismatch"/> condition.
        /// </summary>
        public static readonly Guid Problem_CategoryMismatch = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsAuthorMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_AuthorMissing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsUserMissing"/> condition.
        /// </summary>
        public static readonly Guid Problem_UserMissing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IProblemDescriptor"/>s that describe the
        ///     <see cref="IsOriginOutsideBoundingBox"/> condition.
        /// </summary>
        public static readonly Guid Problem_OriginOutsideBoundingBox = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_NameChangePrefix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_NameRemovePrefix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_NameRemoveSuffix = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_NameRemoveMarker = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsNameFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_NameSwapMarkers = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleAddUnderscore = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleRemoveUnderscore = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleAddTilde = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleRemoveTilde = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleAddColourValue = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleRemoveColourValue = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsTitleFormatInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_TitleCorrectSpacing = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="HasKeywordDuplicates"/> and
        /// <see cref="HasKeywordInTitle"/> conditions.
        /// </summary>
        public static readonly Guid Fix_KeywordRemoveUnneededEntries = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsBFCMissing"/> and
        /// <see cref="IsBFCInvalid"/>conditions.
        /// </summary>
        public static readonly Guid Fix_BFCSetCounterClockwise = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsBFCMissing"/> condition.
        /// </summary>
        public static readonly Guid Fix_BFCSetClockwise = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsBFCMissing"/> condition.
        /// </summary>
        public static readonly Guid Fix_BFCSetUncertified = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsLicenseInvalid"/> condition.
        /// </summary>
        public static readonly Guid Fix_LicenseSetCCAL2 = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsCategoryMismatch"/> condition.
        /// </summary>
        public static readonly Guid Fix_CategoryClear = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsOriginOutsideBoundingBox"/> condition.
        /// </summary>
        public static readonly Guid Fix_OriginCentreTop = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsOriginOutsideBoundingBox"/> condition.
        /// </summary>
        public static readonly Guid Fix_OriginCentreBottom = Guid.NewGuid();

        /// <summary>
        /// Gets the <see cref="T:System.Guid"/> used to identify <see cref="T:Digitalis.LDTools.DOM.API.IFixDescriptor"/>s that describe a fix for
        ///     the <see cref="IsOriginOutsideBoundingBox"/> condition.
        /// </summary>
        public static readonly Guid Fix_OriginCentreBoundingBox = Guid.NewGuid();

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> is incorrectly formatted for the
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of the <see cref="LDPage"/>.
        /// </summary>
        /// <seealso cref="NameFormatProblem"/>
        /// <seealso cref="Problem_NameFormatInvalid"/>
        /// <seealso cref="Fix_NameChangePrefix"/>
        /// <seealso cref="Fix_NameRemovePrefix"/>
        /// <seealso cref="Fix_NameRemoveSuffix"/>
        /// <seealso cref="Fix_NameRemoveMarker"/>
        /// <seealso cref="Fix_NameSwapMarkers"/>
        public NameFormatProblem IsNameFormatInvalid { get { ValidateName(); return _isNameFormatInvalid; } }
        private NameFormatProblem _isNameFormatInvalid;
        private char _namePrefix;
        private char _nameSuffix;
        private string _nameSubpartMarker;
        private string _nameShortcutMarker;
        private string _namePatternMarker;
        private string _nameAllMarkers;
        private bool _isNameValidated;

        private void ValidateName()
        {
            if (_isNameValidated)
                return;

            _isNameValidated     = true;
            _isNameFormatInvalid = NameFormatProblem.None;

            string name = Name;

            if (PageType.Model != PageType)
            {
                // pages in a Model document can use anything they like
                if (null != Document && PageType.Model == Document.DocumentType)
                    return;

                // otherwise they're restricted to these: [-A-Za-z0-9_]
                if (!Regex_Name_InvalidChars.IsMatch(name))
                    _isNameFormatInvalid |= NameFormatProblem.Invalid_Chars;
            }

            // these three have no further restrictions on their naming
            if (PageType.Model != PageType && PageType.Primitive != PageType && PageType.HiresPrimitive != PageType)
            {
                Match match = Regex_Name.Match(name);

                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    Group prefix           = groups[1];
                    Group suffix           = groups[3];
                    Group markers          = groups[4];
                    int patternMarker      = -1;
                    int shortcutMarker     = -1;
                    int subpartMarker      = -1;

                    switch (PageType)
                    {
                        // [prefix]number[suffix][pattern-marker][shortcut-marker]
                        case PageType.Part:
                        case PageType.Shortcut:
                        case PageType.Shortcut_Physical_Colour:
                            if (prefix.Success && !Name_Prefixes.Contains(Char.ToLower(prefix.Value[0])))
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Invalid_Prefix;
                                _namePrefix           = prefix.Value[0];
                            }

                            if (suffix.Success && !Name_Alphabet.Contains(Char.ToLower(suffix.Value[0])))
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Invalid_Suffix;
                                _nameSuffix           = suffix.Value[0];
                            }

                            if (markers.Success)
                            {
                                _nameShortcutMarker = "";
                                _namePatternMarker  = "";
                                _nameAllMarkers     = "";

                                foreach (Capture capture in markers.Captures)
                                {
                                    switch (capture.Value[0])
                                    {
                                        case 's':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_SubpartMarker;
                                            _nameSubpartMarker    = capture.Value;
                                            break;

                                        case 'c':
                                        case 'd':
                                            _nameAllMarkers += capture.Value;

                                            if (-1 == shortcutMarker)
                                            {
                                                _nameShortcutMarker = capture.Value;

                                                if (!Regex_Name_ShortcutMarker.IsMatch(_nameShortcutMarker))
                                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_ShortcutMarker;

                                                shortcutMarker = capture.Index;
                                            }
                                            else
                                            {
                                                _isNameFormatInvalid &= ~NameFormatProblem.Invalid_ShortcutMarker;
                                                _isNameFormatInvalid |= NameFormatProblem.Duplicate_ShortcutMarker;
                                                _nameShortcutMarker  += capture.Value;
                                            }
                                            break;

                                        case 'p':
                                            _nameAllMarkers += capture.Value;

                                            if (-1 == patternMarker)
                                            {
                                                _namePatternMarker = capture.Value;

                                                if (!Regex_Name_PatternMarker.IsMatch(_namePatternMarker))
                                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_PatternMarker;

                                                patternMarker = capture.Index;
                                            }
                                            else
                                            {
                                                _isNameFormatInvalid &= ~NameFormatProblem.Invalid_PatternMarker;
                                                _isNameFormatInvalid |= NameFormatProblem.Duplicate_PatternMarker;
                                                _namePatternMarker   += capture.Value;
                                            }
                                            break;
                                    }
                                }

                                if (-1 != shortcutMarker &&
                                    shortcutMarker < patternMarker &&
                                    0 == (NameFormatProblem.Duplicate_ShortcutMarker & _isNameFormatInvalid) &&
                                    0 == (NameFormatProblem.Duplicate_PatternMarker & _isNameFormatInvalid))
                                {
                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_MarkerSequence;
                                }
                            }
                            break;

                        // number
                        case PageType.Part_Physical_Colour:
                        case PageType.Shortcut_Alias:
                            if (prefix.Success)
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Unneeded_Prefix;
                                _namePrefix           = prefix.Value[0];
                            }

                            if (suffix.Success)
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Unneeded_Suffix;
                                _nameSuffix           = suffix.Value[0];
                            }

                            if (markers.Success)
                            {
                                foreach (Capture capture in markers.Captures)
                                {
                                    switch (capture.Value[0])
                                    {
                                        case 's':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_SubpartMarker;
                                            _nameSubpartMarker    = capture.Value;
                                            break;

                                        case 'c':
                                        case 'd':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_ShortcutMarker;
                                            _nameShortcutMarker   = capture.Value;
                                            break;

                                        case 'p':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_PatternMarker;
                                            _namePatternMarker    = capture.Value;
                                            break;
                                    }
                                }
                            }
                            break;

                        // number[suffix]
                        case PageType.Part_Alias:
                            if (prefix.Success)
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Unneeded_Prefix;
                                _namePrefix           = prefix.Value[0];
                            }

                            if (suffix.Success && !Name_Alphabet.Contains(Char.ToLower(suffix.Value[0])))
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Invalid_Suffix;
                                _nameSuffix           = suffix.Value[0];
                            }

                            if (markers.Success)
                            {
                                foreach (Capture capture in markers.Captures)
                                {
                                    switch (capture.Value[0])
                                    {
                                        case 's':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_SubpartMarker;
                                            _nameSubpartMarker    = capture.Value;
                                            break;

                                        case 'c':
                                        case 'd':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_ShortcutMarker;
                                            _nameShortcutMarker   = capture.Value;
                                            break;

                                        case 'p':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_PatternMarker;
                                            _namePatternMarker    = capture.Value;
                                            break;
                                    }
                                }
                            }
                            break;

                        // [prefix]number[suffix][pattern-marker]subpart-marker
                        case PageType.Subpart:
                            if (prefix.Success && !Name_Prefixes.Contains(Char.ToLower(prefix.Value[0])))
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Invalid_Prefix;
                                _namePrefix           = prefix.Value[0];
                            }

                            if (suffix.Success && !Name_Alphabet.Contains(Char.ToLower(suffix.Value[0])))
                            {
                                _isNameFormatInvalid |= NameFormatProblem.Invalid_Suffix;
                                _nameSuffix           = suffix.Value[0];
                            }

                            _isNameFormatInvalid |= NameFormatProblem.Missing_SubpartMarker;

                            if (markers.Success)
                            {
                                _nameSubpartMarker = "";
                                _namePatternMarker = "";
                                _nameAllMarkers    = "";

                                foreach (Capture capture in markers.Captures)
                                {
                                    switch (capture.Value[0])
                                    {
                                        case 's':
                                            _nameAllMarkers += capture.Value;

                                            if (-1 == subpartMarker)
                                            {
                                                _nameSubpartMarker = capture.Value;
                                                _isNameFormatInvalid &= ~NameFormatProblem.Missing_SubpartMarker;

                                                if (!Regex_Name_SubpartMarker.IsMatch(_nameSubpartMarker))
                                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_SubpartMarker;

                                                subpartMarker = capture.Index;
                                            }
                                            else
                                            {
                                                _isNameFormatInvalid &= ~NameFormatProblem.Invalid_SubpartMarker;
                                                _isNameFormatInvalid |= NameFormatProblem.Duplicate_SubpartMarker;
                                                _nameSubpartMarker   += capture.Value;
                                            }
                                            break;

                                        case 'c':
                                        case 'd':
                                            _isNameFormatInvalid |= NameFormatProblem.Unneeded_ShortcutMarker;
                                            _nameShortcutMarker   = capture.Value;
                                            break;

                                        case 'p':
                                            _nameAllMarkers += capture.Value;

                                            if (-1 == patternMarker)
                                            {
                                                _namePatternMarker = capture.Value;

                                                if (!Regex_Name_PatternMarker.IsMatch(_namePatternMarker))
                                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_PatternMarker;

                                                patternMarker = capture.Index;
                                            }
                                            else
                                            {
                                                _isNameFormatInvalid &= ~NameFormatProblem.Invalid_PatternMarker;
                                                _isNameFormatInvalid |= NameFormatProblem.Duplicate_PatternMarker;
                                                _namePatternMarker   += capture.Value;
                                            }
                                            break;
                                    }
                                }

                                if (-1 != subpartMarker &&
                                    subpartMarker < patternMarker &&
                                    0 == (NameFormatProblem.Duplicate_SubpartMarker & _isNameFormatInvalid) &&
                                    0 == (NameFormatProblem.Duplicate_PatternMarker & _isNameFormatInvalid))
                                {
                                    _isNameFormatInvalid |= NameFormatProblem.Invalid_MarkerSequence;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    _isNameFormatInvalid |= NameFormatProblem.Unrecognised;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> exceeds the maximum number of characters permitted by
        ///     <see href="http://www.ldraw.org/article/512.html#file_name">the File Format Restrictions for the Official Library</see>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is anything other than
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> and <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> is more than 21 characters in length.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_NameTooLong"/>
        public bool IsNameTooLong { get { return (Name.Length > Max_NameLength && PageType.Model != PageType); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> is incorrectly formatted for the
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of the <see cref="LDPage"/>.
        /// </summary>
        /// <seealso cref="TitleFormatProblem"/>
        /// <seealso cref="Problem_TitleFormatInvalid"/>
        /// <seealso cref="Fix_TitleAddUnderscore"/>
        /// <seealso cref="Fix_TitleRemoveUnderscore"/>
        /// <seealso cref="Fix_TitleAddTilde"/>
        /// <seealso cref="Fix_TitleRemoveTilde"/>
        /// <seealso cref="Fix_TitleAddColourValue"/>
        /// <seealso cref="Fix_TitleRemoveColourValue"/>
        /// <seealso cref="Fix_TitleCorrectSpacing"/>
        public TitleFormatProblem IsTitleFormatInvalid { get { ValidateTitle(); return _isTitleFormatInvalid; } private set { _isTitleFormatInvalid = value; } }
        private TitleFormatProblem _isTitleFormatInvalid;
        private bool _isTitleValidated;

        private void ValidateTitle()
        {
            if (_isTitleValidated)
                return;

            _isTitleValidated     = true;
            _isTitleFormatInvalid = TitleFormatProblem.None;

            if (PageType.Model != PageType)
            {
                string title = Title;

                switch (PageType)
                {
                    case PageType.Part_Physical_Colour:
                    case PageType.Shortcut_Physical_Colour:
                        if ('_' != title[0])
                        {
                            _isTitleFormatInvalid |= TitleFormatProblem.Missing_Underscore;

                            if ('~' == title[0])
                                _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_Tilde;
                        }

                        if (!Regex_Title_ColourValue.IsMatch(title))
                            _isTitleFormatInvalid |= TitleFormatProblem.Missing_ColourValue;
                        break;

                    case PageType.Subpart:
                        if ('~' != title[0])
                        {
                            _isTitleFormatInvalid |= TitleFormatProblem.Missing_Tilde;

                            if ('_' == title[0])
                                _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_Underscore;
                        }

                        if (Regex_Title_ColourValue.IsMatch(title))
                            _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_ColourValue;
                        break;

                    default:
                        if ('_' == title[0])
                            _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_Underscore;

                        if (Regex_Title_ColourValue.IsMatch(title))
                            _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_ColourValue;
                        break;
                }

                if (Regex_Title_MissingWhitespace.IsMatch(title))
                    _isTitleFormatInvalid |= TitleFormatProblem.Missing_SpaceBeforeNumber;

                if (Regex_Title_ExcessWhitespace.IsMatch(title))
                    _isTitleFormatInvalid |= TitleFormatProblem.Unneeded_Spaces;
            }
        }

        /// <summary>
        /// Gets a value indicating whether one or more values in <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/> is a duplicate.
        /// </summary>
        /// <seealso cref="Problem_KeywordDuplicates"/>
        /// <seealso cref="Fix_KeywordRemoveUnneededEntries"/>
        public bool HasKeywordDuplicates { get { ValidateKeywords(); return (_keywordDuplicateIndices.Length > 0); } }
        private bool _isKeywordsValidated;
        private uint[] _keywordDuplicateIndices;

        /// <summary>
        /// Gets the indices of the values in <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/> which are duplicates.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return the indices of those keywords which should be removed in order to resolve the <see cref="HasKeywordDuplicates"/> and/or <see cref="HasKeywordInTitle"/>
        /// problems.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_KeywordDuplicates"/>
        /// <seealso cref="Fix_KeywordRemoveUnneededEntries"/>
        public uint[] KeywordDuplicates { get { ValidateKeywords(); return _keywordDuplicates.Clone() as uint[]; } }
        private uint[] _keywordDuplicates;

        /// <summary>
        /// Gets a value indicating whether one or more values in <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/> also appears in
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/>.
        /// </summary>
        /// <seealso cref="Problem_KeywordInTitle"/>
        /// <seealso cref="Fix_KeywordRemoveUnneededEntries"/>
        public bool HasKeywordInTitle { get { ValidateKeywords(); return (_keywordInTitleIndices.Length > 0); } }
        private uint[] _keywordInTitleIndices;

        private void ValidateKeywords()
        {
            if (_isKeywordsValidated)
                return;

            _isKeywordsValidated = true;

            if (AllowsKeywords(PageType))
            {
                List<string> keywords = new List<string>(Keywords);
                List<uint> inTitle    = new List<uint>();
                List<uint> dupes      = new List<uint>();
                uint n                = 0;
                uint m;

                // collapse all whitespace down to single ' ' chars
                string title = Regex_Whitespace.Replace(Title, " ");

                foreach (string keyword in keywords)
                {
                    // can't use Regex.IsMatch() because there's a bug and it will fail to handle any whitespace in the keyword
                    if (new Regex(@"\b" + Regex.Escape(keyword) + @"\b", RegexOptions.IgnoreCase).IsMatch(title))
                        inTitle.Add(n);

                    m = 0;

                    foreach (string k in keywords)
                    {
                        if (m > n && k.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                            dupes.Add(m);

                        m++;
                    }

                    n++;
                }

                _keywordInTitleIndices   = inTitle.ToArray();
                _keywordDuplicateIndices = dupes.ToArray();

                dupes.AddRange(inTitle);
                dupes.Sort();
                _keywordDuplicates = dupes.Distinct().ToArray();
            }
            else
            {
                _keywordInTitleIndices   = new uint[0];
                _keywordDuplicateIndices = new uint[0];
                _keywordDuplicates       = new uint[0];
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> is incorrectly set to <see cref="Digitalis.LDTools.DOM.API.CullingMode.NotSet"/>.
        /// </summary>
        /// <remarks>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is anything other than <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>
        /// and <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> is set to <see cref="Digitalis.LDTools.DOM.API.CullingMode.NotSet"/>.
        /// </remarks>
        /// <seealso cref="Problem_BFCMissing"/>
        /// <seealso cref="Fix_BFCSetCounterClockwise"/>
        /// <seealso cref="Fix_BFCSetClockwise"/>
        /// <seealso cref="Fix_BFCSetUncertified"/>
        public bool IsBFCMissing { get { return (PageType.Model != PageType && CullingMode.NotSet == BFC); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> is incorrectly set for the <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/>
        ///     of the <see cref="LDPage"/>.
        /// </summary>
        /// <remarks>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> and <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> is set to anything other than
        /// <see cref="Digitalis.LDTools.DOM.API.CullingMode.CertifiedCounterClockwise"/>.
        /// </remarks>
        /// <seealso cref="Problem_BFCInvalid"/>
        /// <seealso cref="Fix_BFCSetCounterClockwise"/>
        public bool IsBFCInvalid { get { return ((PageType.Primitive == PageType || PageType.HiresPrimitive == PageType) && CullingMode.CertifiedCounterClockwise != BFC); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.License"/> is incorrectly set.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.License"/> is not set to <see cref="Digitalis.LDTools.DOM.API.License.CCAL2"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_LicenseInvalid"/>
        /// <seealso cref="Fix_LicenseSetCCAL2"/>
        public bool IsLicenseInvalid { get { return (License.CCAL2 != License); } }

        /// <summary>
        /// Gets a value indicating whether a category can be determined for the <see cref="LDPage"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is anything other than <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>,
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/> or <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> and
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/> returns <see cref="Digitalis.LDTools.DOM.API.Category.Unknown"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_CategoryMissing"/>
        public bool IsCategoryMissing
        {
            get
            {
                if (PageType.Model == PageType || PageType.Primitive == PageType || PageType.HiresPrimitive == PageType)
                    return false;

                return (Category.Unknown == Category);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the value of <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/> will override the category implied by
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if the first word of <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> specifies a valid
        /// <see cref="Digitalis.LDTools.DOM.API.Category"/>, but <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/> has been set to a different value.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_CategoryMismatch"/>
        /// <seealso cref="Fix_CategoryClear"/>
        public bool IsCategoryMismatch
        {
            get
            {
                if (AllowsCategory(PageType))
                {
                    switch (Category)
                    {
                        // the multi-word categories always take precedence, since they cannot be specified via the Title
                        case Category.MinifigAccessory:
                        case Category.MinifigFootwear:
                        case Category.MinifigHeadwear:
                        case Category.MinifigHipwear:
                        case Category.MinifigNeckwear:
                        case Category.FigureAccessory:
                            return false;
                    }

                    Category category = Configuration.GetCategoryFromName(Title, Name, PageType);

                    if (Category.Unknown == category || Category.Primitive_Unknown == category)
                        return false;

                    return (category != Category);
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.Author"/> has a value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.Author"/> is absent or empty.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_AuthorMissing"/>
        public bool IsAuthorMissing { get { return String.IsNullOrWhiteSpace(Author); } }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IPage.User"/> has a value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IPage.User"/> is absent or empty and <see cref="P:Digitalis.LDTools.DOM.API.IPage.Update"/> is
        /// either <c>null</c> or else specifies that the file is not from the original LDraw Parts Library, as many of these original files pre-date the introduction of usernames.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_UserMissing"/>
        public bool IsUserMissing
        {
            get
            {
                // James Jessiman never had an LDraw.org username, so there's no point in flagging this as an error
                if (null != Author && "James Jessiman" == Author)
                    return false;

                return ((null == Update || !((LDUpdate)Update).IsOriginal) && String.IsNullOrWhiteSpace(User));
            }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.Origin"/> falls outside <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.BoundingBox"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return <c>true</c> if <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.Origin"/> falls outside <see cref="P:Digitalis.LDTools.DOM.API.IGeometric.BoundingBox"/>
        /// and <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is anything other than <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Problem_OriginOutsideBoundingBox"/>
        /// <seealso cref="Fix_OriginCentreTop"/>
        /// <seealso cref="Fix_OriginCentreBottom"/>
        /// <seealso cref="Fix_OriginCentreBoundingBox"/>
        public bool IsOriginOutsideBoundingBox { get { return (PageType.Model != PageType && !BoundingBox.Contains(Origin)); } }

        /// <inheritdoc />
        public override bool HasProblems(CodeStandards mode)
        {
            if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
            {
                return base.HasProblems(mode) || IsNameTooLong || (NameFormatProblem.None != IsNameFormatInvalid) || (TitleFormatProblem.None != IsTitleFormatInvalid) ||
                    HasKeywordDuplicates || HasKeywordInTitle || IsBFCMissing || IsBFCInvalid || IsLicenseInvalid || IsCategoryMissing || IsCategoryMismatch || IsAuthorMissing ||
                    IsUserMissing || IsOriginOutsideBoundingBox;
            }

            return base.HasProblems(mode);
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The <see cref="T:Digitalis.LDTools.DOM.API.Analytics.Severity"/> of the <see cref="T:Digitalis.LDTools.DOM.API.Analytics.IProblemDescriptor"/>s returned varies by
        /// <paramref name="mode"/>:
        /// <list type="table">
        ///   <listheader><term>Problem</term><description><see cref="T:Digitalis.LDTools.DOM.API.Analytics.Severity"/></description></listheader>
        ///   <item>
        ///     <term><see cref="Problem_NameFormatInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_NameTooLong"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_TitleFormatInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_KeywordDuplicates"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_KeywordInTitle"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_BFCMissing"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_BFCInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_LicenseInvalid"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_CategoryMissing"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_CategoryMismatch"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_AuthorMissing"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_UserMissing"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Error"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="Problem_OriginOutsideBoundingBox"/></term>
        ///     <description>
        ///       <see cref="Digitalis.LDTools.DOM.API.Analytics.Severity.Warning"/> if <paramref name="mode"/> is
        ///       <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> or <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/>;
        ///       ignored otherwise
        ///     </description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override ICollection<IProblemDescriptor> Analyse(CodeStandards mode)
        {
            ICollection<IProblemDescriptor> problems = base.Analyse(mode);

            if (CodeStandards.OfficialModelRepository == mode || CodeStandards.PartsLibrary == mode)
            {
                if (NameFormatProblem.None != IsNameFormatInvalid)
                    problems.Add(new NameFormatInvalidProblem(this, IsNameFormatInvalid, _namePrefix, _nameSuffix, _nameAllMarkers, _namePatternMarker, _nameShortcutMarker, _nameSubpartMarker));

                if (IsNameTooLong)
                    problems.Add(new NameTooLongProblem(this));

                if (TitleFormatProblem.None != IsTitleFormatInvalid)
                    problems.Add(new TitleFormatInvalidProblem(this, IsTitleFormatInvalid));

                if (HasKeywordDuplicates)
                    problems.Add(new KeywordDuplicatesProblem(this, _keywordDuplicateIndices));

                if (HasKeywordInTitle)
                    problems.Add(new KeywordInTitleProblem(this, _keywordInTitleIndices));

                if (IsBFCMissing)
                    problems.Add(new BFCMissingProblem(this));
                else if (IsBFCInvalid)
                    problems.Add(new BFCInvalidProblem(this));

                if (IsLicenseInvalid)
                    problems.Add(new LicenseInvalidProblem(this));

                if (IsCategoryMissing)
                    problems.Add(new CategoryMissingProblem(this));
                else if (IsCategoryMismatch)
                    problems.Add(new CategoryMismatchProblem(this));

                if (IsAuthorMissing)
                    problems.Add(new AuthorMissingProblem(this));

                if (IsUserMissing)
                    problems.Add(new UserMissingProblem(this));

                if (IsOriginOutsideBoundingBox)
                    problems.Add(new OriginOutsideBoundingBoxProblem(this));
            }

            return problems;
        }

        #endregion Analytics

        #region Cloning and Serialization

        private List<IStep> _serializedContents;

        [OnSerializing]
        private void OnSerializing(StreamingContext sc)
        {
            _serializedContents = new List<IStep>(this);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext sc)
        {
            Initialize();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext sc)
        {
            _skipPermissionChecks = true;

            foreach (IStep step in _serializedContents)
            {
                Add(step);
            }

            _skipPermissionChecks = false;

            _serializedContents.Clear();
            _serializedContents = null;
        }

        /// <inheritdoc />
        protected override void InitializeObject(IDOMObject obj)
        {
            IPage page = (IPage)obj;

            page.Name = Name;
            page.PageType = PageType;
            page.License = License;
            page.BFC = BFC;
            page.RotationConfig = new List<MLCadRotationConfig>(RotationConfig);
            page.RotationPoint = RotationPoint;
            page.Title = Title;
            page.Author = Author;
            page.User = User;
            page.RotationPointVisible = RotationPointVisible;
            page.History = new List<LDHistory>(History);
            page.Help = Help;
            page.InlineOnPublish = InlineOnPublish;
            page.Update = Update;

            if (AllowsDefaultColour(PageType))
                page.DefaultColour = DefaultColour;

            if (AllowsKeywords(PageType))
                page.Keywords = Keywords;

            if (AllowsCategory(PageType))
                page.Category = Category;
            else if (AllowsTheme(PageType))
                page.Theme = Theme;

            foreach (IStep step in this)
            {
                page.Add((IStep)step.Clone());
            }

            base.InitializeObject(obj);
        }

        #endregion Cloning and Serialization

        #region Code-generation

        // preferred number of chars we want to write after a '!KEYWORDS' meta-command
        private const int KeywordsLineLength = 60;

        /// <inheritdoc />
        /// <exception cref="T:System.ArgumentException"><paramref name="codeFormat"/> is <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> and
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>.</exception>
        /// <remarks>
        /// <para>
        /// If <paramref name="codeFormat"/> is set to <see cref="Digitalis.LDTools.DOM.API.CodeStandards.PartsLibrary"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.CodeStandards.OfficialModelRepository"/> and <see cref="P:Digitalis.LDTools.DOM.API.IPage.InlineOnPublish"/> is <c>true</c>,
        /// the <see href="http://www.ldraw.org/article/398.html">page header</see> will be omitted.
        /// </para>
        /// </remarks>
        public override StringBuilder ToCode(StringBuilder sb, CodeStandards codeFormat, uint overrideColour, ref Matrix4d transform, WindingDirection winding)
        {
            if (CodeStandards.PartsLibrary == codeFormat && PageType.Model == PageType)
                throw new ArgumentException("LDPages of PageType Model cannot be encoded in PartsLibrary format");

            if (CodeStandards.Full == codeFormat || !InlineOnPublish)
            {
                // Title
                if (!String.IsNullOrEmpty(Title))
                    sb.AppendFormat("0 {0}{1}", Title, LineTerminator);

                // Name
                sb.AppendFormat("0 Name: {0}{1}", TargetName, LineTerminator);

                // Author
                if (!String.IsNullOrEmpty(Author))
                {
                    sb.AppendFormat("0 Author: {0}", Author);

                    if (!String.IsNullOrEmpty(User))
                        sb.AppendFormat(" [{0}]", User);

                    sb.Append(LineTerminator);
                }

                // Type
                sb.Append("0 !LDRAW_ORG ");

                if (null == Update)
                    sb.Append("Unofficial_");

                // this must not be localized, so we can't use LDTranslationCatalog.GetPageType()
                switch (PageType)
                {
                    case PageType.HiresPrimitive:
                        sb.Append("48_Primitive");
                        break;

                    case PageType.Part_Alias:
                        sb.Append("Part Alias");
                        break;

                    case PageType.Part_Physical_Colour:
                        sb.Append("Part Physical_Colour");
                        break;

                    case PageType.Shortcut_Alias:
                        sb.Append("Shortcut Alias");
                        break;

                    case PageType.Shortcut_Physical_Colour:
                        sb.Append("Shortcut Physical_Colour");
                        break;

                    default:
                        sb.Append(PageType);
                        break;
                }

                if (null != Update)
                    sb.AppendFormat(" {0}", ((LDUpdate)Update).ToCode());

                sb.Append(LineTerminator);

                // License
                if (License.None != License)
                    sb.AppendFormat("0 !LICENSE {0}{1}", Configuration.GetPartLicense(License), LineTerminator);

                // Help
                if (!String.IsNullOrWhiteSpace(Help))
                {
                    sb.Append(LineTerminator);

                    string[] lines = Help.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        if (line.Length > 0)
                            sb.AppendFormat("0 !HELP {0}{1}", line, LineTerminator);
                    }
                }

                // Initial BFC statement, if any
                switch (BFC)
                {
                    case CullingMode.Disabled:
                        sb.AppendFormat("{0}0 BFC NOCERTIFY{0}", LineTerminator);
                        break;

                    case CullingMode.CertifiedClockwise:
                        sb.AppendFormat("{0}0 BFC CERTIFY CW{0}", LineTerminator);
                        break;

                    case CullingMode.CertifiedCounterClockwise:
                        sb.AppendFormat("{0}0 BFC CERTIFY CCW{0}", LineTerminator);
                        break;
                }

                // Category/Theme and Keywords
                if (PageType.Primitive != PageType && PageType.HiresPrimitive != PageType)
                {
                    bool linebreak = true;

                    // Theme
                    if (PageType.Model == PageType)
                    {
                        if (!String.IsNullOrWhiteSpace(Theme))
                        {
                            sb.AppendFormat("{0}0 !THEME {1}{0}", LineTerminator, Theme);
                            linebreak = false;
                        }
                    }
                    // Category
                    else if (Category.Unknown != Category)
                    {
                        string cat;

                        // this must not be localized, so we can't use Configuration.GetPartCategory()
                        switch (Category)
                        {
                            case Category.FigureAccessory:
                                cat = "Figure Accessory";
                                break;

                            case Category.MinifigAccessory:
                                cat = "Minifig Accessory";
                                break;

                            default:
                                cat = Category.ToString();
                                break;
                        }

                        string title = Title.Trim(new char[] { '_', '~' });

                        if (!title.StartsWith(cat, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.AppendFormat("{0}0 !CATEGORY {1}{0}", LineTerminator, cat);
                            linebreak = false;
                        }
                    }

                    // Keywords
                    int count = Keywords.Count();

                    if (count > 0)
                    {
                        int length = 0;
                        int n      = 0;

                        if (linebreak)
                            sb.Append(LineTerminator);

                        sb.Append("0 !KEYWORDS ");

                        foreach (string keyword in Keywords)
                        {
                            length += keyword.Length;
                            sb.Append(keyword);

                            if (++n == count)
                            {
                                sb.Append(LineTerminator);
                            }
                            else if (length > KeywordsLineLength)
                            {
                                sb.Append(LineTerminator);
                                sb.Append("0 !KEYWORDS ");
                                length = 0;
                            }
                            else
                            {
                                sb.Append(", ");
                                length += 2;
                            }
                        }
                    }
                }

                // DefaultColour
                if (Palette.MainColour != DefaultColour && AllowsDefaultColour(PageType))
                    sb.AppendFormat("{0}0 !CMDLINE -c{1}{0}", LineTerminator, DefaultColour);

                // History
                if (History.Count() > 0)
                {
                    sb.Append(LineTerminator);

                    foreach (LDHistory h in History)
                    {
                        sb.Append(h.ToCode());
                    }
                }

                // InlineOnPublish
                if (InlineOnPublish)
                    sb.AppendFormat("{0}0 !DIGITALIS_LDTOOLS_DOM INLINEONPUBLISH{0}", LineTerminator);

                // MLCad Rotation
                if (CodeStandards.PartsLibrary != codeFormat)
                {
                    IEnumerable<MLCadRotationConfig> rotCfg = RotationConfig;
                    int count                               = 0;

                    foreach (MLCadRotationConfig cfg in rotCfg)
                    {
                        sb.AppendFormat("{0}0 ROTATION CENTER {1} {2} {3} {4} \"{5}\"", LineTerminator, cfg.Point.X, cfg.Point.Y, cfg.Point.Z, ((cfg.AllowChange) ? "1" : "0"), cfg.Name);
                        count++;
                    }

                    if (count > 0 || MLCadRotationConfig.Type.PartOrigin != RotationPoint || RotationPointVisible)
                        sb.AppendFormat("{0}0 ROTATION CONFIG {1} {2}{0}", LineTerminator, (int)RotationPoint, ((RotationPointVisible) ? "1" : "0"));
                }

                // and finally, put a blank line ahead of the elements
                if (Count > 0)
                    sb.Append(LineTerminator);
            }

            // and the steps
            foreach (IStep step in this)
            {
                step.ToCode(sb, codeFormat, overrideColour, ref transform, winding);
            }

            return sb;
        }

        #endregion Code-generation

        #region Collection-management

        [NonSerialized]
        private UndoableList<IStep> _steps;

        private void Initialize()
        {
            _steps = new UndoableList<IStep>();

            _steps.ItemsAdded += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                if (!_skipPermissionChecks)
                {
                    if (IsFrozen)
                        throw new ObjectFrozenException();

                    foreach (IStep step in e.Items)
                    {
                        if (null == step)
                            throw new ArgumentNullException();

                        if (step.IsFrozen)
                            throw new ObjectFrozenException("step is frozen");

                        InsertCheckResult result = CanInsert(step, InsertCheckFlags.None);

                        if (InsertCheckResult.CanInsert != result)
                            throw new InvalidOperationException("Cannot insert this step: " + result);
                    }
                }

                foreach (IStep step in e.Items)
                {
                    step.Page = this;
                    step.Changed += OnStepChanged;
                }

                _boundsDirty = true;

                if (null != ItemsAdded)
                    ItemsAdded(this, e);

                OnChanged(this, "ItemsAdded", e);
            };

            _steps.ItemsRemoved += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IStep step in e.Items)
                {
                    step.Page = null;
                    step.Changed -= OnStepChanged;
                }

                _boundsDirty = true;

                if (null != ItemsRemoved)
                    ItemsRemoved(this, e);

                OnChanged(this, "ItemsRemoved", e);
            };

            _steps.ItemsReplaced += delegate(object sender, UndoableListReplacedEventArgs<IStep> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IStep step in e.ItemsAdded.Items)
                {
                    if (null == step)
                        throw new ArgumentNullException();

                    if (step.IsFrozen)
                        throw new ObjectFrozenException("step is frozen");

                    InsertCheckResult result = CanInsert(step, InsertCheckFlags.None);

                    if (InsertCheckResult.CanInsert != result)
                        throw new InvalidOperationException("Cannot insert this step: " + result);
                }

                foreach (IStep step in e.ItemsAdded.Items)
                {
                    step.Page = this;
                    step.Changed += OnStepChanged;
                }

                foreach (IStep step in e.ItemsRemoved.Items)
                {
                    step.Page = null;
                    step.Changed -= OnStepChanged;
                }

                _boundsDirty = true;

                if (null != ItemsReplaced)
                    ItemsReplaced(this, e);

                OnChanged(this, "ItemsReplaced", e);
            };

            _steps.ListCleared += delegate(object sender, UndoableListChangedEventArgs<IStep> e)
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                foreach (IStep step in e.Items)
                {
                    step.Page = null;
                    step.Changed -= OnStepChanged;
                }

                _boundsDirty = true;

                if (null != CollectionCleared)
                    CollectionCleared(this, e);

                OnChanged(this, "CollectionCleared", e);
            };
        }

        [field: NonSerialized]
        private bool _skipPermissionChecks;

        /// <inheritdoc />
        [field:NonSerialized]
        public event UndoableListChangedEventHandler<IStep> ItemsAdded;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IStep> ItemsRemoved;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListReplacedEventHandler<IStep> ItemsReplaced;

        /// <inheritdoc />
        [field: NonSerialized]
        public event UndoableListChangedEventHandler<IStep> CollectionCleared;

        /// <inheritdoc />
        public InsertCheckResult CanInsert(IStep step, InsertCheckFlags flags)
        {
            return this.CanReplaceStep(step, null, flags);
        }

        /// <inheritdoc />
        public InsertCheckResult CanReplace(IStep stepToInsert, IStep stepToReplace, InsertCheckFlags flags)
        {
            return this.CanReplaceStep(stepToInsert, stepToReplace, flags);
        }

        /// <inheritdoc />
        public bool HasLockedDescendants
        {
            get
            {
                foreach (IStep step in this)
                {
                    if (step.IsLocked || step.HasLockedDescendants)
                        return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly { get { return IsFrozen; } }

        /// <inheritdoc />
        public int Count { get { return _steps.Count; } }

        /// <inheritdoc />
        public int IndexOf(IStep step)
        {
            return _steps.IndexOf(step);
        }

        /// <inheritdoc />
        public bool Contains(IStep step)
        {
            return _steps.Contains(step);
        }

        /// <inheritdoc />
        public IStep this[int index]
        {
            get { return _steps[index]; }
            set { _steps[index] = value; }
        }

        /// <inheritdoc />
        public void Add(IStep step)
        {
            Insert(Count, step);
        }

        /// <inheritdoc />
        public void Insert(int index, IStep step)
        {
            _steps.Insert(index, step);
        }

        /// <inheritdoc />
        public bool Remove(IStep step)
        {
            return _steps.Remove(step);
        }

        /// <inheritdoc />
        public void RemoveAt(int idx)
        {
            _steps.RemoveAt(idx);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _steps.Clear();
        }

        /// <inheritdoc />
        public void CopyTo(IStep[] array, int arrayIndex)
        {
            _steps.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<IStep> GetEnumerator()
        {
            return _steps.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Collection-management

        #region Constructor

        // our names, in PageType order
        private static string[] Names = new string[]
        {
            Resources.NewModel,
            Resources.NewPart,
            Resources.NewAlias,
            Resources.NewPhysical_Colour,
            Resources.NewShortcut,
            Resources.NewShortcutAlias,
            Resources.NewPhysical_Colour,           // for Shortcut_Physical_Colour
            Resources.NewSubpart,
            Resources.NewPrimitive,
            Resources.NewHiresPrimitive
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="LDPage"/> class with default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="LDPage"/> will have a <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>, a
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> of <c>"Untitled"</c> and a <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> of <c>"New Model"</c>.
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> will be set to <see cref="Digitalis.LDTools.DOM.API.CullingMode.NotSet"/>.
        /// </para>
        /// </remarks>
        public LDPage()
            : this(PageType.Model)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDPage"/> class with the specified values.
        /// </summary>
        /// <param name="type">The <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType">type</see> of the <see cref="LDPage"/>.</param>
        /// <remarks>
        /// <para>
        /// The <see cref="LDPage"/> will have a <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> of <c>"Untitled"</c> and a
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> of <c>"New {type}"</c>.
        /// </para>
        /// <para>
        /// If <paramref name="type"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> then <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> will be set to
        /// <see cref="Digitalis.LDTools.DOM.API.CullingMode.NotSet"/>; otherwise it will be set to <see cref="Digitalis.LDTools.DOM.API.CullingMode.CertifiedCounterClockwise"/>.
        /// </para>
        /// </remarks>
        public LDPage(PageType type)
            : this(type, Resources.Untitled, Names[(int)type])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDPage"/> class with the specified values.
        /// </summary>
        /// <param name="type">The <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType">type</see> of the <see cref="LDPage"/>.</param>
        /// <param name="name">The <see cref="P:Digitalis.LDTools.DOM.API.IPage.Name"/> of the <see cref="LDPage"/>.</param>
        /// <param name="title">The <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> of the <see cref="LDPage"/>.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="name"/> was <c>null</c> or empty.</exception>
        /// <remarks>
        /// <para>
        /// If either <see cref="P:Digitalis.LDTools.DOM.Configuration.Author"/> or <see cref="P:Digitalis.LDTools.DOM.Configuration.Username"/> are set, the
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.Author"/> and <see cref="P:Digitalis.LDTools.DOM.API.IPage.User"/> fields of the new <see cref="LDPage"/> will
        /// be filled out, an <see cref="T:Digitalis.LDTools.DOM.LDHistory"/> with a <see cref="P:Digitalis.LDTools.DOM.LDHistory.Description"/> of <c>"Initial creation"</c>
        /// and the current date will be added, and <see cref="P:Digitalis.LDTools.DOM.API.IPage.License"/> will be set to <see cref="Digitalis.LDTools.DOM.API.License.CCAL2"/>.
        /// </para>
        /// <para>
        /// If <paramref name="type"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> then <see cref="P:Digitalis.LDTools.DOM.API.IPage.BFC"/> will be set to
        /// <see cref="Digitalis.LDTools.DOM.API.CullingMode.NotSet"/>; otherwise it will be set to <see cref="Digitalis.LDTools.DOM.API.CullingMode.CertifiedCounterClockwise"/>.
        /// </para>
        /// </remarks>
        public LDPage(PageType type, string name, string title)
            : base()
        {
            Elements = new ElementAccessor(this);
            Name     = name;
            PageType = type;
            Title    = title;
            Author   = Configuration.Author;
            User     = Configuration.Username;
            BFC      = (PageType.Model == type) ? CullingMode.NotSet : CullingMode.CertifiedCounterClockwise;

            if (!String.IsNullOrWhiteSpace(User) || !String.IsNullOrWhiteSpace(Author))
            {
                // just for once, we don't want to localize this as !HISTORY lines are supposed to be in English
                LDHistory history = new LDHistory(DateTime.Now, "Initial creation");
                History           = new LDHistory[] { history };
                License           = License.CCAL2;
            }

            Initialize();

            _name.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                string oldName = GetTargetName(e.OldValue, PageType);
                string newName = GetTargetName(e.NewValue, PageType);

                // target-names are not case-sensitive, but we do need to be able to change the case
                if (!e.OldValue.Equals(e.NewValue, StringComparison.OrdinalIgnoreCase))
                    ValidateNewTargetName(oldName, newName);

                _isNameValidated = false;

                if (null != NameChanged)
                    NameChanged(this, e);

                if (null != TargetNameChanged)
                    TargetNameChanged(this, new PropertyChangedEventArgs<string>(oldName, newName));

                OnChanged(this, "NameChanged", e);
            };

            _title.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                _isTitleValidated = false;
                _isKeywordsValidated = false;

                if (null != TitleChanged)
                    TitleChanged(this, e);

                OnChanged(this, "TitleChanged", e);
            };

            _theme.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != ThemeChanged)
                    ThemeChanged(this, e);

                OnChanged(this, "ThemeChanged", e);
            };

            _author.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != AuthorChanged)
                    AuthorChanged(this, e);

                OnChanged(this, "AuthorChanged", e);
            };

            _user.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != UserChanged)
                    UserChanged(this, e);

                OnChanged(this, "UserChanged", e);
            };

            _type.ValueChanged += delegate(object sender, PropertyChangedEventArgs<PageType> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                string oldName = GetTargetName(Name, e.OldValue);
                string newName = GetTargetName(Name, e.NewValue);

                ValidateNewTargetName(oldName, newName);

                _isNameValidated     = false;
                _isTitleValidated    = false;
                _isKeywordsValidated = false;

                if (null != PageTypeChanged)
                    PageTypeChanged(this, e);

                if (null != TargetNameChanged)
                    TargetNameChanged(this, new PropertyChangedEventArgs<string>(oldName, newName));

                OnChanged(this, "PageTypeChanged", e);
            };

            _license.ValueChanged += delegate(object sender, PropertyChangedEventArgs<License> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != LicenseChanged)
                    LicenseChanged(this, e);

                OnChanged(this, "LicenseChanged", e);
            };

            _category.ValueChanged += delegate(object sender, PropertyChangedEventArgs<Category> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != CategoryChanged)
                    CategoryChanged(this, e);

                OnChanged(this, "CategoryChanged", e);
            };

            _bfc.ValueChanged += delegate(object sender, PropertyChangedEventArgs<CullingMode> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != BFCChanged)
                    BFCChanged(this, e);

                OnChanged(this, "BFCChanged", e);
            };

            _defaultColour.ValueChanged += delegate(object sender, PropertyChangedEventArgs<uint> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != DefaultColourChanged)
                    DefaultColourChanged(this, e);

                OnChanged(this, "DefaultColourChanged", e);
            };

            _update.ValueChanged += delegate(object sender, PropertyChangedEventArgs<LDUpdate?> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != UpdateChanged)
                    UpdateChanged(this, e);

                OnChanged(this, "UpdateChanged", e);
            };

            _help.ValueChanged += delegate(object sender, PropertyChangedEventArgs<string> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != HelpChanged)
                    HelpChanged(this, e);

                OnChanged(this, "HelpChanged", e);
            };

            _keywords.ValueChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<string>> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                _isKeywordsValidated = false;

                if (null != KeywordsChanged)
                    KeywordsChanged(this, e);

                OnChanged(this, "KeywordsChanged", e);
            };

            _history.ValueChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<LDHistory>> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != HistoryChanged)
                    HistoryChanged(this, e);

                OnChanged(this, "HistoryChanged", e);
            };

            _rotationPoint.ValueChanged += delegate(object sender, PropertyChangedEventArgs<MLCadRotationConfig.Type> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != RotationPointChanged)
                    RotationPointChanged(this, e);

                OnChanged(this, "RotationPointChanged", e);
            };

            _rotationPointVisible.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != RotationPointVisibleChanged)
                    RotationPointVisibleChanged(this, e);

                OnChanged(this, "RotationPointVisibleChanged", e);
            };

            _rotationConfig.ValueChanged += delegate(object sender, PropertyChangedEventArgs<IEnumerable<MLCadRotationConfig>> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != RotationConfigChanged)
                    RotationConfigChanged(this, e);

                OnChanged(this, "RotationConfigChanged", e);
            };

            _inlineOnPublish.ValueChanged += delegate(object sender, PropertyChangedEventArgs<bool> e)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != InlineOnPublishChanged)
                    InlineOnPublishChanged(this, e);

                OnChanged(this, "InlineOnPublishChanged", e);
            };
        }

        #endregion Constructor

        #region Disposal

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && null != Document && Document.IsFrozen)
                throw new ObjectFrozenException();

            base.Dispose(disposing);

            if (disposing)
            {
                if (!IsFrozen && null != Document && !Document.IsDisposing)
                    ((IPage)this).Document = null;

                foreach (IStep step in this)
                {
                    step.Dispose();
                }
            }
        }

        #endregion Disposal

        #region Document-tree

        /// <inheritdoc />
        protected override void OnChanged(IDOMObject source, string operation, EventArgs parameters)
        {
            _boundsDirty = true;
            base.OnChanged(source, operation, parameters);
        }

        private void OnStepChanged(IDOMObject sender, ObjectChangedEventArgs e)
        {
            _boundsDirty = true;
            base.OnChanged(e);
        }

        /// <inheritdoc />
        public override IDocument Document { get { return _document; } }

        /// <inheritdoc />
        IDocument IPage.Document
        {
            get { return _document; }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null != Document && null != value)
                    throw new InvalidOperationException("Cannot set Document: " + InsertCheckResult.AlreadyMember);

                if (value == Document)
                    return;

                if (null == value)
                {
                    // removing
                    if (Document.Contains(this))
                    {
                        // being set directly
                        Document.Remove(this);
                    }
                    else
                    {
                        // being set by a call to IDocument.Remove()
                        PropertyChangedEventArgs<IDocument> args = new PropertyChangedEventArgs<IDocument>(Document, null);

                        _document = null;
                        OnDocumentChanged(args);
                    }
                }
                else
                {
                    // adding
                    if (value.Contains(this))
                    {
                        // being set by a call to IDocument.Insert()
                        PropertyChangedEventArgs<IDocument> args = new PropertyChangedEventArgs<IDocument>(Document, value);

                        _document = value;
                        OnDocumentChanged(args);
                    }
                    else
                    {
                        // being set directly; this will do the CanInsert() checks
                        value.Add(this);
                    }
                }
            }
        }
        [NonSerialized]
        private IDocument _document;

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IDocument> DocumentChanged;

        private void OnDocumentChanged(PropertyChangedEventArgs<IDocument> e)
        {
            if (null != DocumentChanged)
                DocumentChanged(this, e);

            OnChanged(this, "DocumentChanged", e);
            OnPathToDocumentChanged(EventArgs.Empty);
        }

        #endregion Document-tree

        #region Editor

        // our IElementEditor, if the Controls DLL is available
        private static readonly ConstructorInfo EditorFactory = Configuration.GetEditorConstructor("Digitalis.LDTools.Controls.LDPageEditor", typeof(LDPage));

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDPage"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override bool HasEditor
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return (null != EditorFactory);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <see cref="LDPage"/> provides an <see cref="T:Digitalis.LDTools.DOM.API.IElementEditor"/> if the <see cref="N:Digitalis.LDTools.Controls"/> DLL is present.
        /// </para>
        /// </remarks>
        public override IElementEditor GetEditor()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);

            if (null != EditorFactory)
                return EditorFactory.Invoke(new object[] { this }) as IElementEditor;

            return null;
        }

        #endregion Editor

        #region Geometry

        /// <inheritdoc />
        public Box3d BoundingBox
        {
            get
            {
                if (_boundsDirty)
                {
                    bool set = true;

                    foreach (IStep step in this)
                    {
                        if (set)
                        {
                            _bounds = step.BoundingBox;
                            set     = false;
                        }
                        else
                        {
                            _bounds.Union(step.BoundingBox);
                        }
                    }
                }

                return _bounds;
            }
        }
        private Box3d _bounds;
        private bool _boundsDirty = true;

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// The origin of an <see cref="T:Digtialis.LDTools.DOM.API.IPage"/> is <see cref="F:OpenTK.Vector3d.Zero"/>.
        /// </para>
        /// </remarks>
        public Vector3d Origin { get { return Vector3d.Zero; } }

        /// <inheritdoc />
        public CullingMode WindingMode { get { return BFC; } }

        /// <inheritdoc />
        public void Transform(ref Matrix4d matrix)
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (HasLockedDescendants)
                throw new ElementLockedException();

            _boundsDirty = true;

            foreach (IStep step in this)
            {
                step.Transform(ref matrix);
            }
        }

        /// <inheritdoc />
        public void ReverseWinding()
        {
            if (IsFrozen)
                throw new ObjectFrozenException();

            if (HasLockedDescendants)
                throw new ElementLockedException();

            foreach (IStep step in this)
            {
                step.ReverseWinding();
            }
        }

        #endregion Geometry

        #region Parser

        private static readonly string[] LineTypes = new string[] { "0", "1", "2", "3", "4", "5" };

        private const char LineType_HeaderProperty          = 'H';
        private const char LineType_MetaCommand             = 'M';
        private const char LineType_MetaCommand_Deactivated = 'D';
        private const char LineType_Texmap                  = 'T';
        private const char LineType_Comment                 = '0';
        private const char LineType_LDReference             = '1';
        private const char LineType_LDLine                  = '2';
        private const char LineType_LDTriangle              = '3';
        private const char LineType_LDQuadrilateral         = '4';
        private const char LineType_LDOptionalLine          = '5';

        private class GroupData
        {
            public MLCadGroup       Group;
            public bool             IsLocked;
            public bool             LockNext;
            public bool             InvertNext;
            public List<IGroupable> Elements = new List<IGroupable>();

            public GroupData(MLCadGroup group)
            {
                Group = group;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> allows the use of
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if <paramref name="type"/> allows the use of <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/>; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> and
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> do not allow <see cref="P:Digitalis.LDTools.DOM.API.IPage.Category"/> to be set.
        /// </para>
        /// </remarks>
        public static bool AllowsCategory(PageType type)
        {
            return (PageType.Primitive != type && PageType.HiresPrimitive != type && PageType.Model != type);
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> allows the use of
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.Theme"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if <paramref name="type"/> allows the use of <see cref="P:Digitalis.LDTools.DOM.API.IPage.Theme"/>; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>
        /// Only <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> allows <see cref="P:Digitalis.LDTools.DOM.API.IPage.Theme"/> to be set.
        /// </para>
        /// </remarks>
        public static bool AllowsTheme(PageType type)
        {
            return (PageType.Model == type);
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> allows the use of
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if <paramref name="type"/> allows the use of <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/>; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/> and <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> do not allow
        /// <see cref="P:Digitalis.LDTools.DOM.API.IPage.Keywords"/> to be set.
        /// </para>
        /// </remarks>
        public static bool AllowsKeywords(PageType type)
        {
            return (PageType.Primitive != type && PageType.HiresPrimitive != type);
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> allows the use of
        ///     <see cref="P:Digitalis.LDTools.DOM.API.IPage.DefaultColour"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if <paramref name="type"/> allows the use of <see cref="P:Digitalis.LDTools.DOM.API.IPage.DefaultColour"/>; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/>, <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> and
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/> do not allow <see cref="P:Digitalis.LDTools.DOM.API.IPage.DefaultColour"/> to be set.
        /// </para>
        /// </remarks>
        public static bool AllowsDefaultColour(PageType type)
        {
            return (PageType.Primitive != type && PageType.HiresPrimitive != type && PageType.Model != type);
        }

        // used only by LDDocument
        internal void Load(TextReader stream, string documentPath, string filePath, ref uint lineNum, ref string firstLine, long length, ref long bytesRead, ParserProgressCallback callback)
        {
            _skipPermissionChecks = true;

            // originally set by the constructor; reset them so that they don't override the values read from the file
            Author   = null;
            User     = null;
            License  = License.None;
            History  = null;
            PageType = PageType.Part;   // set this to Part so that the !CATEGORY line will parse correctly

            firstLine = ReadFile(stream, documentPath, filePath, ref lineNum, firstLine, length, ref bytesRead, callback);

            // strip trailing empty comments off the last step
            if (Count > 0)
            {
                LDStep lastStep = this[Count - 1] as LDStep;

                for (int i = lastStep.Count - 1; i >= 0; i--)
                {
                    LDComment c = lastStep[i] as LDComment;

                    if (null == c || !c.IsEmpty)
                        break;

                    lastStep.Remove(c);
                }
            }

            _skipPermissionChecks = false;
        }

        private string ReadFile(TextReader stream, string documentPath, string filePath, ref uint lineNum, string firstLine, long length, ref long bytesRead, ParserProgressCallback callback)
        {
            StringBuilder                 help              = new StringBuilder();
            string                        line              = firstLine;
            string                        categoryLine      = null;
            string                        name              = null;
            string[]                      fields;
            char                          lineType;
            Dictionary<string, GroupData> groups            = new Dictionary<string, GroupData>();
            GroupData                     groupData         = null;
            List<string>                  keywordLines      = new List<string>();
            List<LDHistory>               history           = new List<LDHistory>();
            List<MLCadRotationConfig>     rotationConfig    = new List<MLCadRotationConfig>();
            int                           numLines          = 0;
            int                           lastProgress      = 0;
            int                           progress;
            uint                          defaultColour     = Palette.MainColour;
            bool                          invertNext        = false;
            bool                          ghostNext         = false;
            bool                          hideNext          = false;
            bool                          lockNext          = false;
            bool                          headerComplete    = false;
            bool                          seenNameLine      = false;
            bool                          seenTypeLine      = false;
            bool                          seenAuthorLine    = false;
            bool                          seenLicenseLine   = false;
            bool                          seenDefaultColour = false;
            bool                          seenThemeLine     = false;
            bool                          endOfPageReached  = false;
            bool                          simpleTexmap      = false;
            LDStep                        step              = new LDStep();
            LDTexmap                      currentTexmap     = null;
            IElementCollection            currentTexmapGeom = null;
            IPageElement                  last              = null;
            IPageElement                  el;

            while (!endOfPageReached)
            {
                if (0 != line.Length)
                {
                    bytesRead += line.Length + LineTerminator.Length;
                    lineType = line[0];
                    el = null;

                    if (line.Length > 1 && !WhitespaceChars.Contains(line[1]))
                        throw new SyntaxException("Not an LDraw file", null, documentPath, line, lineNum);

                    if (LineType_Comment == lineType)
                    {
                        fields = line.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
                        numLines++;

                        if (fields.Length > 1)
                        {
                            if (headerComplete)
                            {
                                lineType = LineType_MetaCommand;
                            }
                            else
                            {
                                lineType = LineType_HeaderProperty;

                                // If we hit something that looks like a property-line but either cannot be parsed or is redundant, we'll assume it's really a comment.
                                // Some types of line need turning into 'deactivated' comments by prefixing them with '//' in order to prevent parser errors when the
                                // page is loaded into a less-strict parser.
                                switch (fields[1])
                                {
                                    case "Name:":
                                        // 0 Name: {filePath}
                                        if (fields.Length < 3 || seenNameLine)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            name = line.Substring(line.IndexOf("Name:") + "Name:".Length).Trim();
                                            seenNameLine = true;
                                        }
                                        break;

                                    case "Author:":
                                        // 0 Author: {author name}
                                        // 0 Author: {author name} [{username}]
                                        // 0 Author: {author name} <{email address}>
                                        if (seenAuthorLine)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            seenAuthorLine = ParseAuthorLine(fields);
                                        break;

                                    // obsolete type-line formats, but still present in the library and user-files so must be handled
                                    case "Model":
                                    case "Unofficial":
                                    case "Un-official":
                                    case "Official":
                                        if (seenTypeLine)
                                            lineType = LineType_Comment;        // don't deactivate these ones - they're fairly common English words so they probably are comments
                                        else
                                            seenTypeLine = ParseTypeLine(fields);
                                        break;

                                    // official type-line format
                                    case "!LDRAW_ORG":
                                    case "LDRAW_ORG":
                                        // 0 !LDRAW_ORG [Unofficial_]{type} [{qualifier}] [ORIGINAL|UPDATE YYYY-RR]
                                        // 0 LDRAW_ORG [Unofficial_]{type} [{qualifier}] [ORIGINAL|UPDATE YYYY-RR]
                                        if (seenTypeLine)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            seenTypeLine = ParseTypeLine(fields);
                                        break;

                                    case "!LICENSE":
                                        // 0 !LICENSE {license details}
                                        if (seenLicenseLine)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            seenLicenseLine = ParseLicenseLine(line);
                                        break;

                                    case "!THEME":
                                        if (fields.Length < 3 || seenThemeLine)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            seenThemeLine = true;
                                            Theme = line.Substring(line.IndexOf("!THEME") + "!THEME".Length).Trim();
                                        }
                                        break;

                                    case "!CATEGORY":
                                        // 0 !CATEGORY {category name}
                                        if (fields.Length < 3 || null != categoryLine)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            categoryLine = line;
                                        break;

                                    case "!KEYWORDS":
                                        // 0 !KEYWORDS {word}[, {word}...]
                                        if (fields.Length < 3)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            keywordLines.Add(line);
                                        break;

                                    case "!HISTORY":
                                        // 0 !HISTORY {YYYY-MM-DD} [{username}]|{{realname}} {description}
                                        try
                                        {
                                            history.Add(new LDHistory(line));
                                        }
                                        catch
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        break;

                                    case "!HELP":
                                        // 0 !HELP [{text}]
                                        if (help.Length > 0)
                                            help.Append(LineTerminator);

                                        help.Append(line.Substring(line.IndexOf("!HELP") + "!HELP".Length).Trim());
                                        break;

                                    case "!CMDLINE":
                                    case "CMDLINE":
                                        // 0 !CMDLINE -c{colour-value}
                                        // 0 CMDLINE -c{colour-value}
                                        if (fields.Length < 2 || !fields[2].StartsWith("-c") || seenDefaultColour)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            seenDefaultColour = uint.TryParse(fields[2].Substring(2), out defaultColour);
                                        break;

                                    case "!DIGITALIS_LDTOOLS_DOM":
                                        if (fields.Length < 3)
                                        {
                                            lineType = LineType_MetaCommand;
                                        }
                                        else
                                        {
                                            switch (fields[2])
                                            {
                                                case "INLINEONPUBLISH":
                                                    InlineOnPublish = true;
                                                    break;

                                                default:
                                                    lineType = LineType_MetaCommand;
                                                    break;
                                            }
                                        }
                                        break;

                                    default:
                                        // the line was not recognised as a header property so we'll try for a meta-command
                                        lineType = LineType_MetaCommand;
                                        break;
                                }
                            }

                            if (LineType_MetaCommand == lineType)
                            {
                                switch (fields[1])
                                {
                                    // page-terminators
                                    case "FILE":
                                        // 0 FILE {filePath}
                                        if (fields.Length <= 2)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            endOfPageReached = true;
                                        break;

                                    case "NOFILE":
                                        // 0 NOFILE
                                        if (2 != fields.Length)
                                            lineType = LineType_MetaCommand_Deactivated;
                                        else
                                            endOfPageReached = true;
                                        break;

                                    // these may only appear in the header section; if they turn up in the body then they need to be handled as comments
                                    case "!LDRAW_ORG":
                                    case "LDRAW_ORG":
                                    case "!CMDLINE":
                                    case "CMDLINE":
                                    case "!CATEGORY":
                                    case "!THEME":
                                    case "!KEYWORDS":
                                    case "!HISTORY":
                                    case "!HELP":
                                        lineType = LineType_MetaCommand_Deactivated;
                                        break;

                                    // these may appear anywhere in the file
                                    case "STEP":
                                        if (fields.Length > 2)
                                        {
                                            lineType = LineType_Comment;
                                        }
                                        else
                                        {
                                            el = step;
                                            step = new LDStep();
                                        }
                                        break;

                                    case "ROTSTEP":
                                        if (fields.Length < 3 || fields.Length > 6)
                                        {
                                            lineType = LineType_Comment;
                                        }
                                        else
                                        {
                                            // as we don't get the parameters for each step until we've reached its end, we have to copy them over
                                            LDStep s = step;
                                            el = step;
                                            step = new LDStep(line);
                                            s.Mode = step.Mode;
                                            s.X = step.X;
                                            s.Y = step.Y;
                                            s.Z = step.Z;
                                            step = new LDStep();
                                        }
                                        break;

                                    case "BFC":
                                        if (-1 != Array.IndexOf(fields, "INVERTNEXT"))
                                        {
                                            if (null != groupData)
                                                groupData.InvertNext = true;
                                            else
                                                invertNext = true;
                                        }
                                        else if (!headerComplete && -1 != Array.IndexOf(fields, "CERTIFY"))
                                        {
                                            BFC = (-1 != Array.IndexOf(fields, "CW")) ? CullingMode.CertifiedClockwise : CullingMode.CertifiedCounterClockwise;
                                        }
                                        else if (!headerComplete && -1 != Array.IndexOf(fields, "NOCERTIFY"))
                                        {
                                            BFC = CullingMode.Disabled;
                                        }
                                        else
                                        {
                                            goto default;
                                        }
                                        break;

                                    case "!DIGITALIS_LDTOOLS_DOM":
                                        if (fields.Length < 3)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            switch (fields[2])
                                            {
                                                case "LOCKNEXT":
                                                    if (null != groupData)
                                                        groupData.LockNext = true;
                                                    else
                                                        lockNext = true;
                                                    break;

                                                default:
                                                    lineType = LineType_MetaCommand_Deactivated;
                                                    break;
                                            }
                                        }
                                        break;

                                    case "!TEXMAP":
                                        if (fields.Length < 3)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            switch (fields[2])
                                            {
                                                case "START":
                                                    lineType = LineType_Texmap;
                                                    simpleTexmap = false;
                                                    currentTexmap = null;
                                                    currentTexmapGeom = null;
                                                    break;

                                                case "NEXT":
                                                    lineType = LineType_Texmap;
                                                    simpleTexmap = true;
                                                    currentTexmap = null;
                                                    currentTexmapGeom = null;
                                                    break;

                                                case "FALLBACK":
                                                    if (null == currentTexmap)
                                                        lineType = LineType_MetaCommand_Deactivated;
                                                    else
                                                        currentTexmapGeom = currentTexmap.FallbackGeometry;
                                                    break;

                                                case "END":
                                                    if (null == currentTexmap)
                                                    {
                                                        lineType = LineType_MetaCommand_Deactivated;
                                                    }
                                                    else
                                                    {
                                                        currentTexmap = null;
                                                        currentTexmapGeom = null;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;

                                    case "!:":
                                        // 0 !: {ldraw line}
                                        if (null == currentTexmap || fields.Length < 3 || !LineTypes.Contains(fields[2]))
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            line = line.Substring(line.IndexOf("!:") + "!:".Length).Trim();
                                            lineType = line[0];
                                            currentTexmapGeom = currentTexmap.TextureGeometry;
                                            continue;
                                        }
                                        break;

                                    case "ROTATION":
                                        if (fields.Length < 3)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else if ("CONFIG" == fields[2] && 5 == fields.Length)
                                        {
                                            // 0 ROTATION CONFIG {id} {visible}
                                            // we set this directly to ensure that it doesn't get clipped if RotationConfig hasn't been set yet
                                            _rotationPoint.Value = (MLCadRotationConfig.Type)int.Parse(fields[3], CultureInfo.InvariantCulture);
                                            RotationPointVisible = ("1" == fields[4]) ? true : false;

                                            // the 'part rotation point' is actually just the first Custom point, if any
                                            if (MLCadRotationConfig.Type.PartRotationPoint == _rotationPoint.Value)
                                                _rotationPoint.Value = (MLCadRotationConfig.Type)1;
                                        }
                                        else if ("CENTER" == fields[2] && fields.Length >= 8)
                                        {
                                            try
                                            {
                                                rotationConfig.Add(new MLCadRotationConfig(line));
                                            }
                                            catch
                                            {
                                                lineType = LineType_MetaCommand_Deactivated;
                                            }
                                        }
                                        else
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        break;

                                    case "GHOST":
                                        // 0 GHOST {ldraw line}
                                        if (fields.Length < 3 || !LineTypes.Contains(fields[2]))
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else
                                        {
                                            ghostNext = true;
                                            line = line.Substring(line.IndexOf("GHOST") + "GHOST".Length).Trim();
                                            lineType = line[0];
                                            continue;
                                        }
                                        break;

                                    case "MLCAD":
                                        if (fields.Length < 3)
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        else if ("HIDE" == fields[2] && fields.Length >= 4 && LineTypes.Contains(fields[3]))
                                        {
                                            // 0 MLCAD HIDE {ldraw line}
                                            hideNext = true;
                                            line = line.Substring(line.IndexOf("HIDE") + "HIDE".Length).Trim();
                                            lineType = line[0];
                                            continue;
                                        }
                                        else if ("BTG" == fields[2] && fields.Length >= 4)
                                        {
                                            // 0 MLCAD BTG {group name}
                                            string groupName = line.Substring(line.IndexOf("BTG") + "BTG".Length).Trim();
                                            groups.TryGetValue(groupName, out groupData);

                                            if (null == currentTexmap)
                                            {
                                                if (null == groupData)
                                                {
                                                    groupData = new GroupData(new MLCadGroup(groupName));
                                                    groups[groupName] = groupData;
                                                }

                                                invertNext = groupData.InvertNext;
                                                lockNext = groupData.LockNext;
                                            }
                                            else
                                            {
                                                if (null == groupData || !groupData.Elements.Contains(currentTexmap))
                                                    lineType = LineType_MetaCommand_Deactivated;

                                                groupData = null;
                                            }
                                        }
                                        else
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        break;

                                    case "GROUP":
                                        // 0 GROUP {numElements} {group name}
                                        if (fields.Length >= 4)
                                        {
                                            string groupName = line.Substring(line.IndexOf(fields[2]) + fields[2].Length).Trim();
                                            groups.TryGetValue(groupName, out groupData);

                                            if (null == currentTexmap)
                                            {
                                                if (null == groupData)
                                                {
                                                    groupData = new GroupData(new MLCadGroup(groupName));
                                                    groups[groupName] = groupData;
                                                }

                                                if (null == groupData.Group.Parent)
                                                    el = groupData.Group;

                                                groupData.IsLocked = lockNext;
                                                lockNext = false;
                                            }
                                            else if (null == groupData || !groupData.Elements.Contains(currentTexmap))
                                            {
                                                lineType = LineType_MetaCommand_Deactivated;
                                            }

                                            groupData = null;
                                        }
                                        else
                                        {
                                            lineType = LineType_MetaCommand_Deactivated;
                                        }
                                        break;

                                    default:
                                        el = Configuration.CreateMetaCommand(line);

                                        // the line wasn't recognised as a meta-command, so it must be a comment
                                        if (null == el)
                                            lineType = LineType_Comment;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // the header is assumed to be complete on the first non-empty line which doesn't start with '0'
                        headerComplete = true;
                    }

                    try
                    {
                        if (null != currentTexmap && simpleTexmap)
                        {
                            // only these elements are permitted in simple texmap syntax
                            if (LineType_LDReference     != lineType &&
                                LineType_LDLine          != lineType &&
                                LineType_LDTriangle      != lineType &&
                                LineType_LDQuadrilateral != lineType &&
                                LineType_LDOptionalLine  != lineType)
                            {
                                step.Remove(currentTexmap);
                                step.AddUnchecked(new LDComment("0 // " + currentTexmap.LDrawCode));

                                currentTexmap     = null;
                                currentTexmapGeom = null;
                            }
                        }

                        switch (lineType)
                        {
                            case LineType_Texmap:
                                currentTexmap           = new LDTexmap(line);
                                currentTexmap.LDrawCode = line.Substring(1).Trim();
                                currentTexmapGeom       = currentTexmap.SharedGeometry;
                                el                      = currentTexmap;
                                break;

                            case LineType_MetaCommand_Deactivated:
                                // if this is the first line then it's assumed to be the Title property; otherwise it's a regular comment
                                if (1 != numLines)
                                    line = "0 // " + line.Substring(1).Trim();

                                goto case LineType_Comment;

                            case LineType_Comment:
                                // if this is the first line then it's assumed to be the Title property; otherwise it's a regular comment
                                if (1 == numLines)
                                {
                                    if (1 == line.Length)
                                    {
                                        Title = Resources.Untitled;
                                        break;
                                    }
                                    else
                                    {
                                        string title = line.Substring(1).Trim();

                                        if (!title.StartsWith("//"))
                                        {
                                            Title = title;
                                            break;
                                        }
                                    }
                                }

                                LDComment c = last as LDComment;

                                // collapse blocks of empty comments
                                if (null == c || !c.IsEmpty || line.Length > 1)
                                    el = new LDComment(line);
                                break;

                            case LineType_LDReference:
                                el                          = new LDReference(line, invertNext);
                                ((IGraphic)el).IsGhosted    = ghostNext;
                                ((IGraphic)el).IsVisible    = !hideNext;
                                ((LDReference)el).LDrawCode = line;
                                ((LDReference)el).CodeLine  = lineNum;
                                invertNext                  = false;
                                break;

                            case LineType_LDLine:
                                el                       = new LDLine(line);
                                ((IGraphic)el).IsGhosted = ghostNext;
                                ((IGraphic)el).IsVisible = !hideNext;
                                invertNext               = false;
                                break;

                            case LineType_LDTriangle:
                                el                       = new LDTriangle(line);
                                ((IGraphic)el).IsGhosted = ghostNext;
                                ((IGraphic)el).IsVisible = !hideNext;
                                invertNext               = false;
                                break;

                            case LineType_LDQuadrilateral:
                                el                       = new LDQuadrilateral(line);
                                ((IGraphic)el).IsGhosted = ghostNext;
                                ((IGraphic)el).IsVisible = !hideNext;
                                invertNext               = false;
                                break;

                            case LineType_LDOptionalLine:
                                el                       = new LDOptionalLine(line);
                                ((IGraphic)el).IsGhosted = ghostNext;
                                ((IGraphic)el).IsVisible = !hideNext;
                                invertNext               = false;
                                break;

                            case LineType_HeaderProperty:
                            case LineType_MetaCommand:
                                // already processed
                                break;

                            default:
                                throw new SyntaxException("Unrecognised line-type '" + lineType + "'", null, documentPath, line, lineNum);
                        }

                        if (null != el)
                        {
                            el.IsLocked = lockNext;

                            if (DOMObjectType.Step == el.ObjectType)
                            {
                                // STEP commands act as an implicit terminator for block-commands
                                currentTexmap     = null;
                                currentTexmapGeom = null;
                                _steps.Add(el as LDStep);
                            }
                            else if ((el as IElement).IsTopLevelElement)
                            {
                                // some elements can only go into the step itself
                                step.AddUnchecked(el as IElement);
                            }
                            else if (null != currentTexmap && el != currentTexmap)
                            {
                                // add to the current texmap
                                (currentTexmapGeom as ElementCollection).AddUnchecked(el as IElement);

                                if (simpleTexmap)
                                {
                                    currentTexmap     = null;
                                    currentTexmapGeom = null;
                                }
                            }
                            else if (null != groupData)
                            {
                                step.AddUnchecked(el as IElement);
                                groupData.Elements.Add(el as IGroupable);
                                groupData.LockNext   = false;
                                groupData.InvertNext = false;
                                groupData            = null;
                            }
                            else if (el is IElement)
                            {
                                // add to the step; we cannot run through the regular Add() because the document-tree hasn't been fully constructed yet
                                // and the CanInsertElement() checks would fail
                                if (null == (el as IElement).Parent)
                                    step.AddUnchecked(el as IElement);
                            }

                            progress = (int)(50 * bytesRead / length);

                            if (progress != lastProgress)
                            {
                                if (progress > 50)
                                    progress = 50;

                                lastProgress = progress;

                                string elementName = (DOMObjectType.Reference == el.ObjectType) ? (el as LDReference).TargetName : el.TypeName;

                                if (!callback(elementName, progress))
                                    throw new OperationCanceledException();
                            }

                            last      = el;
                            ghostNext = false;
                            hideNext  = false;
                            lockNext  = false;
                        }
                    }
                    catch (FormatException e)
                    {
                        throw new SyntaxException(e.Message, e, documentPath, line, lineNum);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new SyntaxException(e.Message, e, documentPath, line, lineNum);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new CircularReferenceException(e.Message, e, documentPath, line, lineNum);
                    }
                }

                if (!endOfPageReached)
                {
                    line = stream.ReadLine();

                    if (null == line)
                    {
                        endOfPageReached = true;
                    }
                    else
                    {
                        line = line.Trim();
                        lineNum++;
                    }
                }

                if (endOfPageReached)
                {
                    // there was an implicit STEP at the end of the page
                    if (0 != step.Count)
                        _steps.Add(step);

                    foreach (GroupData gd in groups.Values)
                    {
                        // add any orphaned groups, as this is better than simply losing their contents
                        if (null == gd.Group.Parent)
                            step.AddUnchecked(gd.Group);

                        // add the elements to the group
                        foreach (IGroupable element in gd.Elements)
                        {
                            if (element.IsLocked)
                            {
                                element.IsLocked = false;
                                element.Group    = gd.Group;
                                element.IsLocked = true;
                            }
                            else
                            {
                                element.Group = gd.Group;
                            }
                        }

                        // finally, lock the group if required
                        gd.Group.IsLocked = gd.IsLocked;
                    }

                    Name = filePath;
                    SetTypeFromFilePath(filePath, name, seenTypeLine);

                    History        = history;
                    RotationConfig = rotationConfig;
                    Help           = help.ToString();

                    // do these last as some Types do not permit certain properties, and for these we must preserve the lines as deactivated Comments
                    if (keywordLines.Count > 0)
                    {
                        if (AllowsKeywords(PageType))
                        {
                            Keywords = ParseKeywordLines(keywordLines);
                        }
                        else
                        {
                            keywordLines.Reverse();

                            foreach (string s in keywordLines)
                            {
                                step.Insert(0, new LDComment("0 // " + s.Substring(1).Trim()));
                            }
                        }
                    }

                    if (null != categoryLine)
                    {
                        if (!AllowsCategory(PageType) || !ParseCategoryLine(categoryLine))
                            step.Insert(0, new LDComment("0 // " + categoryLine.Substring(1).Trim()));
                    }

                    if (Palette.MainColour != defaultColour && AllowsDefaultColour(PageType))
                        DefaultColour = defaultColour;
                    break;
                }
            }

            return line;
        }

        private IList<string> ParseKeywordLines(IList<string> keywordLines)
        {
            // 0 !KEYWORDS {word}[, {word}...]
            List<string> keywords = new List<string>();
            string[] fields;

            foreach (string line in keywordLines)
            {
                fields = line.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length > 2 && AllowsKeywords(PageType))
                {
                    fields = line.Substring(line.IndexOf(fields[2])).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string keyword in fields)
                    {
                        keywords.Add(keyword.Trim());
                    }
                }
            }

            return keywords;
        }

        private bool ParseAuthorLine(string[] fields)
        {
            // 0 Author: {author name}
            // 0 Author: {author name} [{username}]
            // 0 Author: {author name} <{email address}>
            if (fields.Length < 3)
                return false;

            string author = String.Empty;

            for (int i = 2; i < fields.Length; i++)
            {
                if ('[' == fields[i][0] && ']' == fields[i][fields[i].Length - 1] ||
                    '<' == fields[i][0] && '>' == fields[i][fields[i].Length - 1])
                {
                    User = fields[i].Trim(new char[] { '[', ']', '<', '>' });
                    break;
                }

                author += fields[i] + " ";
            }

            Author = author.Trim();
            return true;
        }

        private bool ParseLicenseLine(string line)
        {
            // 0 !LICENSE {license details}
            foreach (License license in Enum.GetValues(typeof(License)))
            {
                if (line.Contains(Configuration.GetPartLicense(license)))
                {
                    License = license;
                    return true;
                }
            }

            return false;
        }

        private bool ParseCategoryLine(string line)
        {
            // 0 !CATEGORY {category name}
            Category category;
            string[] fields = line.Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 3)
                return false;

            if (EnumHelper.ToValue(fields[2], true, out category))
            {
                if (fields.Length > 3)
                {
                    if (Category.Minifig == category)
                    {
                        if (fields[3].Equals("Accessory", StringComparison.OrdinalIgnoreCase))
                            category = Category.MinifigAccessory;
                        else if (fields[3].Equals("Footwear", StringComparison.OrdinalIgnoreCase))
                            category = Category.MinifigFootwear;
                        else if (fields[3].Equals("Headwear", StringComparison.OrdinalIgnoreCase))
                            category = Category.MinifigHeadwear;
                        else if (fields[3].Equals("Hipwear", StringComparison.OrdinalIgnoreCase))
                            category = Category.MinifigHipwear;
                        else if (fields[3].Equals("Neckwear", StringComparison.OrdinalIgnoreCase))
                            category = Category.MinifigNeckwear;
                    }
                    else if (Category.Figure == category && fields[3].Equals("Accessory", StringComparison.OrdinalIgnoreCase))
                    {
                        category = Category.FigureAccessory;
                    }
                }

                Category = category;
                return true;
            }

            return false;
        }

        private bool ParseTypeLine(string[] fields)
        {
            PageType type;

            switch (fields[1])
            {
                case "Model":
                    if (fields.Length > 2)
                        return false;

                    PageType = PageType.Model;
                    return true;

                case "Unofficial":
                case "Un-official":
                    // 0 Known type-lines:
                    // 0 Unofficial  Model
                    // 0 Unofficial  Part
                    // 0 Unofficial  Shortcut
                    // 0 Unofficial  LDraw sub-part
                    // 0 Unofficial  subfile
                    // 0 Unofficial  Sub-Part
                    // 0 Un-official Element
                    // 0 Un-official LCad Part
                    if (fields.Length < 3 || fields.Length > 4)
                        return false;

                    switch (fields[fields.Length - 1])
                    {
                        case "Model":
                            PageType = PageType.Model;
                            break;

                        case "Part":
                        case "Element":
                            PageType = PageType.Part;
                            break;

                        case "Shortcut":
                            PageType = PageType.Shortcut;
                            break;

                        case "sub-part":
                        case "Sub-Part":
                        case "subfile":
                            PageType = PageType.Subpart;
                            break;

                        default:
                            return false;
                    }
                    return true;

                case "Official":
                    // Known type-lines:
                    // 0 Official LCad update 99-04
                    // 0 Official LCad Part 2000-01
                    // 0 Official LCad Part - 2000-01 UPDATE
                    // 0 Official LCad Subpart - 2000-01 UPDATE
                    if (fields.Length < 5 || fields.Length > 7 || "LCad" != fields[2])
                        return false;

                    switch (fields[3])
                    {
                        case "Subpart":
                            type = PageType.Subpart;
                            break;

                        case "Part":
                        case "update":
                            type = PageType.Part;
                            break;

                        default:
                            return false;
                    }

                    if (fields.Length > 4)
                    {
                        try
                        {
                            string[] date;

                            if ("-" == fields[4])
                                date = fields[5].Split(new char[] { '-' });
                            else
                                date = fields[4].Split(new char[] { '-' });

                            uint year    = uint.Parse(date[0]);
                            uint release = uint.Parse(date[1]);

                            if (year < 100)
                                year += 1900;

                            Update = new LDUpdate(year, release);
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    PageType = type;
                    return true;

                case "!LDRAW_ORG":
                case "LDRAW_ORG":
                    // 0 !LDRAW_ORG [Unofficial_]{type} [{qualifier}] [ORIGINAL|UPDATE YYYY-RR]
                    // 0 LDRAW_ORG [Unofficial_]{type} [{qualifier}] [ORIGINAL|UPDATE YYYY-RR]
                    if (fields.Length < 3 || fields.Length > 6)
                        return false;

                    // {type}
                    string typeField = fields[2];

                    if (typeField.StartsWith("Unofficial_"))
                        typeField = typeField.Substring("Unofficial_".Length);

                    switch (typeField)
                    {
                        case "Part":
                            type = PageType.Part;
                            break;

                        case "Subpart":
                            type = PageType.Subpart;
                            break;

                        case "Primitive":
                            type = PageType.Primitive;
                            break;

                        case "48_Primitive":
                            type = PageType.HiresPrimitive;
                            break;

                        case "Shortcut":
                            type = PageType.Shortcut;
                            break;

                        case "Model":
                            type = PageType.Model;
                            break;

                        default:
                            return false;
                    }

                    int updateIdx;

                    // {qualifier}
                    if (fields.Length > 3)
                    {
                        switch (fields[3])
                        {
                            case "Alias":
                                if (PageType.Part == type)
                                    type = PageType.Part_Alias;
                                else if (PageType.Shortcut == type)
                                    type = PageType.Shortcut_Alias;

                                updateIdx = 4;
                                break;

                            case "Physical_Colour":
                                if (PageType.Part == type)
                                    type = PageType.Part_Physical_Colour;
                                else if (PageType.Shortcut == type)
                                    type = PageType.Shortcut_Physical_Colour;

                                updateIdx = 4;
                                break;

                            default:
                                updateIdx = 3;
                                break;
                        }

                        if (updateIdx < fields.Length)
                        {
                            try
                            {
                                string updateLine = "";

                                while (updateIdx < fields.Length)
                                {
                                    updateLine += fields[updateIdx++] + " ";
                                }

                                Update = new LDUpdate(updateLine);
                            }
                            catch (FormatException)
                            {
                                // ignore these, it means an invalid update-tag format, which we want to eliminate
                            }
                        }
                    }

                    PageType = type;
                    return true;
            }

            return false;
        }

        private void SetTypeFromFilePath(string filePath, string name, bool seenTypeLine)
        {
            // at this point, PageType will be set to whatever value was read from the stream, or PageType.Part if no type-information was found
            // this may not be correct, though, so check the filePath and name for a prefix
            string folder = Path.GetFileName(Path.GetDirectoryName(filePath)).ToLower();

            if ("s" == folder)
            {
                PageType = PageType.Subpart;
                return;
            }

            if ("48" == folder)
            {
                PageType = PageType.HiresPrimitive;
                return;
            }

            if (!String.IsNullOrWhiteSpace(name))
            {
                folder = Path.GetDirectoryName(name).ToLower();

                if ("s" == folder)
                {
                    PageType = PageType.Subpart;
                    return;
                }

                if ("48" == folder)
                {
                    PageType = PageType.HiresPrimitive;
                    return;
                }
            }

            // no prefix, so check the suffixes
            switch (Path.GetExtension(filePath).ToLower())
            {
                case ".ldr":
                    PageType = PageType.Model;
                    break;

                case ".dat":
                    // if we've seen the !LDRAW_ORG line (or equivalent) we will assume that it's correct; otherwise we assume Part
                    if (!seenTypeLine)
                        PageType = PageType.Part;
                    break;

                default:
                    if (!String.IsNullOrWhiteSpace(name))
                    {
                        switch (Path.GetExtension(name).ToLower())
                        {
                            case ".ldr":
                                PageType = PageType.Model;
                                return;

                            case ".dat":
                                // if we've seen the !LDRAW_ORG line (or equivalent) we will assume that it's correct; otherwise we assume Model
                                if (!seenTypeLine)
                                    PageType = PageType.Part;
                                return;

                            default:
                                break;
                        }
                    }

                    // if we've seen the !LDRAW_ORG line (or equivalent) we will assume that it's correct; otherwise we assume Model
                    if (!seenTypeLine)
                        PageType = PageType.Model;
                    break;
            }
        }

        #endregion Parser

        #region Properties

        /// <inheritdoc />
        public IDOMObjectCollection<IElement> Elements { get; private set; }

        /// <inheritdoc />
        public string TargetName { get { return GetTargetName(Name, PageType); } }

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> TargetNameChanged;

        private string GetTargetName(string name, PageType type)
        {
            switch (type)
            {
                case PageType.Subpart:
                    return Path.Combine("s", name) + ".dat";

                case PageType.HiresPrimitive:
                    return Path.Combine("48", name) + ".dat";

                case PageType.Model:
                    return name + ".ldr";

                default:
                    return name + ".dat";
            }
        }

        private bool CheckForCircularReference(IDOMObjectCollection<IElement> collection, string newTargetName)
        {
            // if we're still loading the page, we don't need to do the check
            if (_skipPermissionChecks)
                return false;

            IElementCollection c;
            IReference r;

            foreach (IElement el in collection)
            {
                c = el as IElementCollection;

                if (null != c)
                {
                    if (CheckForCircularReference(c, newTargetName))
                        return true;

                    continue;
                }

                r = el as IReference;

                if (null != r)
                {
                    if (r.TargetName.Equals(newTargetName, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (null != r.Target)
                    {
                        if (CheckForCircularReference(r.Target.Elements, newTargetName))
                            return true;
                    }
                    else if (TargetStatus.CircularReference == r.TargetStatus)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ValidateNewTargetName(string oldTargetName, string newTargetName)
        {
            IDocument doc = Document;

            if (null != doc)
            {
                IPage page = doc[newTargetName];

                if (null != page && this != page)
                    throw new DuplicateNameException("A page with this name already exists in the document: " + newTargetName);
            }

            if (CheckForCircularReference(Elements, newTargetName))
                throw new InvalidOperationException("This will create a circular reference");
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _name.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("Name cannot be null or empty");

                if (value.Length > 255)
                    throw new ArgumentException("Name must not exceed 255 characters");

                // strip off everything except the filename
                value = Path.GetFileName(value).Trim();

                string extension = Path.GetExtension(value).ToLower();

                if (".dat" == extension || ".ldr" == extension || ".mpd" == extension)
                    value = Path.GetFileNameWithoutExtension(value);

                if (-1 != value.IndexOfAny(InvalidNameChars))
                    throw new ArgumentException("Name may not contain any of the characters " + new String(InvalidNameChars));

                if (Name != value)
                    _name.Value = value;
            }
        }
        private UndoableProperty<string> _name = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> NameChanged;

        /// <inheritdoc />
        public string Title
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _title.Value;
            }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null == value)
                    value = String.Empty;
                else
                    value = value.Trim();

                if (Title != value)
                    _title.Value = value;
            }
        }
        private UndoableProperty<string> _title = new UndoableProperty<string>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> TitleChanged;

        /// <inheritdoc />
        public string Theme
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (PageType.Model != PageType)
                    return String.Empty;

                return _theme.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (PageType.Model != PageType)
                    throw new InvalidOperationException("Pages of Type " + PageType + " cannot have a Theme");

                if (null == value)
                    value = String.Empty;
                else
                    value = value.Trim();

                if (Theme != value)
                    _theme.Value = value;
            }
        }
        private UndoableProperty<string> _theme = new UndoableProperty<string>(String.Empty);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> ThemeChanged;

        /// <inheritdoc />
        public string Author
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _author.Value;
            }
            set
            {
                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null == value)
                    value = String.Empty;
                else
                    value = value.Trim();

                if (Author != value)
                    _author.Value = value;
            }
        }
        private UndoableProperty<string> _author = new UndoableProperty<string>(Configuration.Author);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> AuthorChanged;

        /// <inheritdoc />
        public string User
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _user.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null == value)
                    value = String.Empty;
                else
                    value = value.Trim();

                if (User != value)
                    _user.Value = value;
            }
        }
        private UndoableProperty<string> _user = new UndoableProperty<string>(Configuration.Username);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> UserChanged;

        /// <inheritdoc />
        public PageType PageType
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _type.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (PageType == value)
                    return;

                if (!Enum.IsDefined(typeof(PageType), value))
                    throw new ArgumentOutOfRangeException("Type value " + value + " is not valid");

                _type.Value = value;
            }
        }
        private UndoableProperty<PageType> _type = new UndoableProperty<PageType>(PageType.Model);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<PageType> PageTypeChanged;

        /// <inheritdoc />
        public License License
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _license.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (License == value)
                    return;

                if (!Enum.IsDefined(typeof(License), value))
                    throw new ArgumentOutOfRangeException("License value " + value + " is not valid");

                _license.Value = value;
            }
        }
        private UndoableProperty<License> _license = new UndoableProperty<License>(License.None);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<License> LicenseChanged;

        /// <inheritdoc />
        public Category Category
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (!AllowsCategory(PageType) || Category.Unknown == _category.Value)
                    return Configuration.GetCategoryFromName(Title, Name, PageType);

                return _category.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (_category.Value == value)
                    return;

                if (!AllowsCategory(PageType))
                    throw new InvalidOperationException("Pages of Type " + PageType + " cannot have a Category");

                if (value >= Category.Primitive_Unknown || !Enum.IsDefined(typeof(Category), value))
                    throw new ArgumentOutOfRangeException("Category value " + value + " is not valid");

                _category.Value = value;
            }
        }
        private UndoableProperty<Category> _category = new UndoableProperty<Category>(Category.Unknown);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<Category> CategoryChanged;

        /// <inheritdoc />
        public CullingMode BFC
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _bfc.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (BFC == value)
                    return;

                if (!Enum.IsDefined(typeof(CullingMode), value))
                    throw new ArgumentOutOfRangeException("BFC value " + value + " is not valid");

                _bfc.Value = value;
            }
        }
        private UndoableProperty<CullingMode> _bfc = new UndoableProperty<CullingMode>(CullingMode.NotSet);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<CullingMode> BFCChanged;

        /// <inheritdoc />
        public uint DefaultColour
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _defaultColour.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (!AllowsDefaultColour(PageType))
                    throw new InvalidOperationException("Pages of Type " + PageType + " cannot have a DefaultColour");

                if (LDColour.IsDirectColour(value))
                    throw new InvalidOperationException("DefaultColour may not be a Direct Colours value");

                if (DefaultColour != value)
                    _defaultColour.Value = value;
            }
        }
        private UndoableProperty<uint> _defaultColour = new UndoableProperty<uint>(Palette.MainColour);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<uint> DefaultColourChanged;

        /// <inheritdoc />
        public LDUpdate? Update
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _update.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (Update != value)
                    _update.Value = value;
            }
        }
        private UndoableProperty<LDUpdate?> _update = new UndoableProperty<LDUpdate?>(null);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<LDUpdate?> UpdateChanged;

        /// <inheritdoc />
        public string Help
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _help.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null == value)
                    value = String.Empty;

                if (Help != value)
                    _help.Value = value;
            }
        }
        private UndoableProperty<string> _help = new UndoableProperty<string>(String.Empty);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<string> HelpChanged;

        /// <inheritdoc />
        public IEnumerable<string> Keywords
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _keywords.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (!AllowsKeywords(PageType))
                    throw new InvalidOperationException("Pages of Type " + PageType + " cannot have Keywords");

                List<string> keywords = new List<string>();

                if (null != value)
                {
                    foreach (string keyword in value)
                    {
                        if (!String.IsNullOrWhiteSpace(keyword))
                            keywords.Add(Regex_Whitespace.Replace(keyword.Trim(), " "));
                    }

                    if (keywords.Count == Keywords.Count())
                    {
                        for (int i = 0; i < keywords.Count; i++)
                        {
                            if (keywords[i] != Keywords.ElementAt(i))
                            {
                                _keywords.Value = keywords;
                                break;
                            }
                        }

                        return;
                    }
                }

                _keywords.Value = keywords;
            }
        }
        private UndoableProperty<IEnumerable<string>> _keywords = new UndoableProperty<IEnumerable<string>>(new List<string>());

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IEnumerable<string>> KeywordsChanged;

        /// <inheritdoc />
        public IEnumerable<LDHistory> History
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _history.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (null == value)
                {
                    _history.Value = new List<LDHistory>();
                }
                else
                {
                    foreach (LDHistory history in value)
                    {
                        if (String.IsNullOrWhiteSpace(history.Description))
                            throw new ArgumentNullException();
                    }

                    int count = value.Count();

                    if (count == History.Count())
                    {
                        int i;

                        for (i = 0; i < count; i++)
                        {
                            if (value.ElementAt(i) != History.ElementAt(i))
                                break;
                        }

                        if (i == count)
                            return;
                    }

                    _history.Value = new List<LDHistory>(value.OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day));
                }
            }
        }
        private UndoableProperty<IEnumerable<LDHistory>> _history = new UndoableProperty<IEnumerable<LDHistory>>(new List<LDHistory>());

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IEnumerable<LDHistory>> HistoryChanged;

        /// <inheritdoc />
        public MLCadRotationConfig.Type RotationPoint
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _rotationPoint.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                if (RotationPoint == value)
                    return;

                if (!Enum.IsDefined(typeof(MLCadRotationConfig.Type), value) && ((int)value < 1 || (int)value > _rotationConfig.Value.Count()))
                    throw new ArgumentOutOfRangeException("RotationPoint value " + value + " is not valid");

                _rotationPoint.Value = value;
            }
        }
        private UndoableProperty<MLCadRotationConfig.Type> _rotationPoint = new UndoableProperty<MLCadRotationConfig.Type>(MLCadRotationConfig.Type.PartOrigin);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<MLCadRotationConfig.Type> RotationPointChanged;

        /// <inheritdoc />
        public bool RotationPointVisible
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _rotationPointVisible.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (RotationPointVisible != value)
                    _rotationPointVisible.Value = value;
            }
        }
        private UndoableProperty<bool> _rotationPointVisible = new UndoableProperty<bool>(false);

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<bool> RotationPointVisibleChanged;

        /// <inheritdoc />
        public IEnumerable<MLCadRotationConfig> RotationConfig
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _rotationConfig.Value;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                if (IsFrozen)
                    throw new ObjectFrozenException();

                List<MLCadRotationConfig> config = null;

                if (null == value)
                {
                    config = new List<MLCadRotationConfig>();
                }
                else
                {
                    foreach (MLCadRotationConfig rotationConfig in value)
                    {
                        if (String.IsNullOrWhiteSpace(rotationConfig.Name))
                            throw new ArgumentException();
                    }

                    int count = value.Count();

                    if (count == RotationConfig.Count())
                    {
                        int i;

                        for (i = 0; i < count; i++)
                        {
                            if (value.ElementAt(i) != RotationConfig.ElementAt(i))
                                break;
                        }

                        if (i == count)
                            return;
                    }

                    config = new List<MLCadRotationConfig>(value);
                }

                _rotationConfig.Value = config;

                if ((int)RotationPoint > config.Count)
                    RotationPoint = (MLCadRotationConfig.Type)config.Count;
            }
        }
        private UndoableProperty<IEnumerable<MLCadRotationConfig>> _rotationConfig = new UndoableProperty<IEnumerable<MLCadRotationConfig>>(new List<MLCadRotationConfig>());

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<IEnumerable<MLCadRotationConfig>> RotationConfigChanged;

        /// <inheritdoc />
        public bool InlineOnPublish
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(null);

                return _inlineOnPublish.Value;
            }
            set
            {
                if (_inlineOnPublish.Value != value)
                    _inlineOnPublish.Value = value;
            }
        }
        private UndoableProperty<bool> _inlineOnPublish = new UndoableProperty<bool>();

        /// <inheritdoc />
        [field: NonSerialized]
        public event PropertyChangedEventHandler<bool> InlineOnPublishChanged;

        #endregion Properties

        #region Self-description

        /// <inheritdoc />
        public override DOMObjectType ObjectType { get { return DOMObjectType.Page; } }

        /// <inheritdoc />
        public override bool IsImmutable { get { return false; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns an icon which represents the <see cref="P:Digitalis.LDTools.DOM.API.IPage.PageType"/> of the <see cref="T:Digitalis.LDTools.DOM.API.IPage"/>.
        /// </para>
        /// </remarks>
        public override Image Icon { get { return Configuration.PageTypeIcons[(int)PageType]; } }

        /// <inheritdoc />
        public override string TypeName { get { return Resources.Page; } }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// Returns <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/>, localized if possible.
        /// </para>
        /// </remarks>
        public override string Description { get { return LDTranslationCatalog.GetPartTitle(Title); } }

        /// <inheritdoc />
        /// <remarks>
        /// Returns <see cref="F:System.String.Empty"/>.
        /// </remarks>
        public override string ExtendedDescription { get { return String.Empty; } }

        #endregion Self-description
    }
}
