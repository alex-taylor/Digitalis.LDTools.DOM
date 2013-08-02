#region License

//
// IndexCard.cs
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

namespace Digitalis.LDTools.Library
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Digitalis.LDTools.DOM;
    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    /// <summary>
    /// Represents an entry in a <see cref="T:Digitalis.LDTools.DOM.LibraryManager"/>.
    /// </summary>
    [Serializable]
    public class IndexCard
    {
        #region Internals

        private static readonly Regex Regex_Name_IsPatterned = new Regex(@"^[sux]?\d+[a-z]??p([abcdefghjkmnqrswxyz0-9][abcdefghjkmnqrstuvwxyz0-9]+|[tuv][a-z0-9]+)", RegexOptions.IgnoreCase);

        private uint GetNumber(string name)
        {
            int idx = 0;

            if ('u' == name[0] || 'x' == name[0] || 's' == name[0])
                idx++;

            if (!Char.IsDigit(name[idx]))
                return uint.MaxValue;

            uint number = (uint)name[idx++] - '0';

            while (idx < name.Length && Char.IsDigit(name[idx]))
            {
                number = (number * 10) + (uint)name[idx++] - '0';
            }

            return number;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // reset the primitive categories; these are generated algorithmically, but the resulting values get serialized out
            if (PageType.Primitive == Type || PageType.HiresPrimitive == Type)
                Category = Category.Primitive_Unknown;
        }

        // used to store the index into Configuration.FullSearchPath that this IndexCard represents
        internal int Rank { get; private set; }

        #endregion Internals

        #region Properties

        /// <summary>
        /// Gets the timestamp when the underlying file was last changed.
        /// </summary>
        public virtual long Modified { get; private set; }

        /// <summary>
        /// Gets the name of the <see cref="IndexCard"/>. This is the name of the underlying file.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the target-name of the <see cref="IndexCard"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="Type"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Subpart"/> the returned string is <see cref="Name"/> prefixed with 's\';
        /// if <see cref="Type"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/> the returned string is <see cref="Name"/> prefixed with '48\';
        /// otherwise <see cref="Name"/> is returned.
        /// </para>
        /// </remarks>
        public virtual string TargetName { get; private set; }

        /// <summary>
        /// Gets the full path of the <see cref="IndexCard"/>'s underlying file.
        /// </summary>
        public virtual string Filepath { get; private set; }

        /// <summary>
        /// Gets the part-number of the <see cref="IndexCard"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For files whose <see cref="Name"/> starts with a number, this is that number. For files whose <see cref="Name"/> starts with either
        /// an 's', a 'u' or an 'x' (denoting an LDraw.org-assigned number), this is the numeric part of the <see cref="Name"/>.
        /// For files whose <see cref="Name"/> does not contain a part-number, this returns <see cref="uint.MaxValue"/>.
        /// </para>
        /// </remarks>
        public uint Number { get; private set; }

        /// <summary>
        /// Gets the part-number alias of the <see cref="IndexCard"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An alias is generated when the file is an <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/> or
        /// <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Physical_Colour"/>; the alias is the <see cref="Number"/>
        /// of the referenced part. For convenience, if the file is not one of these types this returns the same value as <see cref="Number"/>.
        /// </para>
        /// </remarks>
        public uint Alias { get; private set; }

        /// <summary>
        /// Gets the title of the <see cref="IndexCard"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the file does not contain a title, this returns <see cref="F:System.String.Empty"/>.
        /// </para>
        /// </remarks>
        public virtual string Title { get; private set; }

        /// <summary>
        /// Gets the type of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual PageType Type { get; private set; }

        /// <summary>
        /// Gets the category of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual Category Category
        {
            get
            {
                if (IsRedirect && null != LibraryManager.Cache)
                {
                    string redirectName = Title.Substring("~Moved to ".Length).Trim();
                    IndexCard redirect  = LibraryManager.Cache[redirectName + ".dat"];

                    if (null != redirect)
                        return redirect.Category;
                }

                if (Category.Primitive_Unknown == _category)
                    _category = Configuration.GetCategoryFromName(Title, Name, Type);

                return _category;
            }
            private set { _category = value; }
        }
        private Category _category;

        /// <summary>
        /// Gets the Theme of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual string Theme { get; private set; }

        /// <summary>
        /// Gets the default colour of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual uint DefaultColour { get; private set; }

        /// <summary>
        /// Gets the keywords of the <see cref="IndexCard"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Keywords are converted to lower-case.
        /// </para>
        /// </remarks>
        public virtual IEnumerable<string> Keywords { get { return _keywords; } }
        private List<string> _keywords = new List<string>();

        /// <summary>
        /// Gets the 'update' information of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual LDUpdate? Update { get; private set; }

        /// <summary>
        /// Gets or sets the help string of the <see cref="IndexCard"/>.
        /// </summary>
        public virtual string Help { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a part.
        /// </summary>
        public bool IsPart { get { return PageType.Part == Type || PageType.Part_Alias == Type || PageType.Part_Physical_Colour == Type || PageType.Shortcut == Type || PageType.Shortcut_Alias == Type || PageType.Shortcut_Physical_Colour == Type; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a subpart.
        /// </summary>
        public bool IsSubpart { get { return PageType.Subpart == Type; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a primitive.
        /// </summary>
        public bool IsPrimitive { get { return PageType.Primitive == Type || PageType.HiresPrimitive == Type; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a model.
        /// </summary>
        public bool IsModel { get { return PageType.Model == Type; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents an obsolete part.
        /// </summary>
        public bool IsObsolete { get { return Title.Contains("Obsolete"); } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a "~Moved to" part.
        /// </summary>
        public bool IsRedirect { get { return Title.StartsWith("~Moved to"); } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a sub-assembly part.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A sub-assembly part is a <see cref="IsPart">part</see> whose <see cref="Title"/> starts with a '~' (tilde) character.
        /// </para>
        /// </remarks>
        public bool IsSubAssembly { get { return (IsPart && '~' == Title[0]); } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexCard"/> represents a patterned part.
        /// </summary>
        public bool IsPatterned { get { return (IsPart && Regex_Name_IsPatterned.IsMatch(Name)); } }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexCard"/> class with default values.
        /// </summary>
        /// <param name="name">The <see cref="Name"/> for the <see cref="IndexCard"/>. This will be used to determine the value for <see cref="Number"/>.</param>
        /// <remarks>
        /// <para>
        /// This constructor is provided for subclasses which do not wish to use the public constructor.
        /// </para>
        /// </remarks>
        protected IndexCard(string name)
        {
            Name   = name;
            Number = GetNumber(name);
            Alias  = Number;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="IndexCard"/> class with the specified values.
        /// </summary>
        /// <param name="file">The file to load from.</param>
        /// <exception cref="T:System.FormatException">The filename did not end with one of the recognised extensions (.dat, .ldr or .mpd).</exception>
        public IndexCard(FileInfo file)
            : this(Path.GetFileName(Path.GetDirectoryName(file.FullName)), file.Name, file.FullName, file.LastWriteTime)
        {
        }

        // used by LibraryManager
        internal IndexCard(string folder, string name, string fullName, DateTime lastWriteTime)
        {
            string   line;
            string   extension;
            string   category  = null;
            string[] fields;
            int      n         = 0;
            int      titlePos  = 1;

            extension = Path.GetExtension(name).ToLower();

            if (".dat" != extension && ".ldr" != extension && ".mpd" != extension)
                throw new FormatException(name);

            Modified      = lastWriteTime.ToFileTimeUtc();
            Name          = name;
            Filepath      = fullName;
            Number        = GetNumber(name);
            Alias         = Number;
            Title         = String.Empty;
            DefaultColour = Palette.MainColour;

            if ("S" == folder || "s" == folder || "48" == folder)
                TargetName = Path.Combine(folder, name);
            else
                TargetName = name;

            Category = Category.Unknown;
            Type     = PageType.Model;

            string pathFolder = Path.GetDirectoryName(fullName);

            foreach (string path in Configuration.FullSearchPath)
            {
                if (path.Equals(pathFolder, StringComparison.OrdinalIgnoreCase))
                    break;

                Rank++;
            }

            using (TextReader stream = File.OpenText(fullName))
            {
                while (null != (line = stream.ReadLine()))
                {
                    line = line.TrimStart(null);
                    n++;

                    if (0 == line.Length)
                        continue;

                    if ('0' != line[0])
                        break;

                    fields = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

                    if (fields.Length > 1)
                    {
                        switch (fields[1].ToUpper())
                        {
                            case "FILE":
                                titlePos = n + 1;
                                break;

                            case "NAME:":
                                break;

                            case "!CATEGORY":
                                category = fields[2];

                                for (int i = 3; i < fields.Length; i++)
                                {
                                    category += fields[i];
                                }
                                break;

                            case "!THEME":
                                if (fields.Length > 2)
                                    Theme = line.Substring(line.IndexOf(fields[2]));
                                break;

                            case "OFFICIAL":
                            case "UNOFFICIAL":
                            case "UN-OFFICIAL":
                            case "!LDRAW_ORG":
                            case "LDRAW_ORG":
                                int    typeIdx;
                                string filetype;

                                if (fields[2].Equals("LCAD", StringComparison.OrdinalIgnoreCase) ||
                                    fields[2].Equals("LDraw", StringComparison.OrdinalIgnoreCase))
                                    typeIdx = 3;
                                else
                                    typeIdx = 2;

                                filetype = fields[typeIdx];

                                if (filetype.StartsWith("Unofficial_", StringComparison.OrdinalIgnoreCase))
                                    filetype = filetype.Substring(filetype.IndexOf('_') + 1);

                                PageType t;

                                if (EnumHelper.ToValue(filetype, true, out t))
                                {
                                    Type = t;

                                    // check for a qualifier
                                    if ((PageType.Part == Type || PageType.Shortcut == Type) && fields.Length > ++typeIdx)
                                    {
                                        if ("Alias" == fields[typeIdx])
                                            Type = (PageType.Part == Type) ? PageType.Part_Alias : PageType.Shortcut_Alias;
                                        else if ("Physical_Colour" == fields[typeIdx])
                                            Type = (PageType.Part == Type) ? PageType.Part_Physical_Colour: PageType.Shortcut_Physical_Colour;
                                    }
                                }
                                else
                                {
                                    switch (filetype.ToUpper())
                                    {
                                        case "48_PRIMITIVE":
                                            Type = PageType.HiresPrimitive;
                                            break;

                                        case "SUB-PART":
                                            Type = PageType.Subpart;
                                            break;
                                    }
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

                                        Update = new LDUpdate(year, release);
                                    }
                                    catch
                                    {
                                        // ignore errors
                                    }
                                }
                                break;

                            case "!HELP":
                                if (fields.Length > 2)
                                    Help += line.Substring(line.IndexOf(fields[2])).Trim() + "\r\n";
                                break;

                            case "!KEYWORDS":
                                if (fields.Length > 2)
                                {
                                    fields = line.Substring(line.IndexOf(fields[2])).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (string keyword in fields)
                                    {
                                        _keywords.Add(keyword.ToLower().Trim());
                                    }
                                }
                                break;

                            case "!CMDLINE":
                            case "CMDLINE":
                                if (fields.Length > 2 && fields[2].StartsWith("-c"))
                                {
                                    try
                                    {
                                        DefaultColour = uint.Parse(fields[2].Substring(2));
                                    }
                                    catch
                                    {
                                    }
                                }
                                break;

                            default:
                                if (titlePos == n)
                                    Title = line.Substring(1).Trim();
                                break;
                        }
                    }
                }
            }

            if (".dat" == extension)
            {
                if ("S" == folder || "s" == folder)
                    Type = PageType.Subpart;
                else if ("48" == folder)
                    Type = PageType.HiresPrimitive;
                else if ("P" == folder || "p" == folder)
                    Type = PageType.Primitive;
                else if (Title.StartsWith("_"))
                    Type = (PageType.Part == Type) ? PageType.Part_Physical_Colour : PageType.Shortcut_Physical_Colour;
                else if (PageType.Part_Physical_Colour == Type)
                    Type = PageType.Part;
                else if (PageType.Shortcut_Physical_Colour == Type)
                    Type = PageType.Shortcut;
                else if (PageType.Shortcut != Type && PageType.Part_Alias != Type && PageType.Shortcut_Alias != Type)
                    Type = PageType.Part;
            }

            switch (Type)
            {
                case PageType.Part:
                case PageType.Part_Physical_Colour:
                case PageType.Part_Alias:
                case PageType.Subpart:
                case PageType.Shortcut:
                case PageType.Shortcut_Physical_Colour:
                case PageType.Shortcut_Alias:
                    {
                        Category c;

                        if (EnumHelper.ToValue(category, true, out c))
                            Category = c;
                        else
                            Category = Configuration.GetCategoryFromName(Title, Name, Type);
                    }
                    break;

                case PageType.Primitive:
                case PageType.HiresPrimitive:
                    Category = Category.Primitive_Unknown;
                    break;

                default:
                    break;
            }

            if (null != line && '1' == line[0] && (PageType.Part_Alias == Type || PageType.Part_Physical_Colour == Type || PageType.Shortcut_Alias == Type || PageType.Shortcut_Physical_Colour == Type))
            {
                fields = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length > 0)
                    Alias = GetNumber(fields[fields.Length - 1]);
            }
        }

        #endregion Constructor
    }
}