#region License

//
// LDTranslationCatalog.cs
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using System.Text;

    #endregion Usings

    /// <summary>
    /// Represents an LDraw translation-file as defined in <see href="http://www.ldraw.org/article/559.html">the Localisation Guidelines</see> and
    /// provides functions to manage installed translation-files.
    /// </summary>
    public class LDTranslationCatalog : IDictionary<string, string>
    {
        #region Catalog-management

        private static Dictionary<string, LDTranslationCatalog[]> _locales = new Dictionary<string, LDTranslationCatalog[]>();

        static LDTranslationCatalog()
        {
            Configuration.LDrawBaseChanged += delegate(object sender, EventArgs e)
            {
                lock (_locales)
                {
                    _locales.Clear();
                }
            };
        }

        /// <summary>
        /// Loads the catalogs for a locale.
        /// </summary>
        /// <param name="localeName">Name of the locale to load.</param>
        /// <returns>An array of <see cref="LDTranslationCatalog"/> for the specified locale, indexed by <see cref="Digitalis.LDTools.DOM.CatalogType"/>; or <c>null</c> if
        ///     no catalogs are available for the locale</returns>
        /// <remarks>
        /// <para>
        /// <paramref name="localeName"/> is in the format specifed by <see cref="P:System.Globalization.CultureInfo.Name"/>. The set of catalogs returned for a locale may not be
        /// complete if a full set of translation-files could not be found; applications should check the values in the returned array before using them.
        /// </para>
        /// </remarks>
        public static LDTranslationCatalog[] LoadCatalog(string localeName)
        {
            lock (_locales)
            {
                LDTranslationCatalog[] locale;

                if (_locales.TryGetValue(localeName, out locale))
                    return locale;

                if (Directory.Exists(Path.Combine(Configuration.LDrawBase, "localisations", localeName)))
                {
                    LDTranslationCatalog catalog;
                    int count = 0;

                    locale = new LDTranslationCatalog[4];

                    foreach (CatalogType type in Enum.GetValues(typeof(CatalogType)))
                    {
                        catalog = new LDTranslationCatalog(localeName, type);

                        if (catalog.Load())
                        {
                            locale[(int)type] = catalog;
                            count++;
                        }
                    }

                    if (count > 0)
                    {
                        _locales.Add(localeName, locale);
                        return locale;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the locale-names of all installed LDraw translation-files.
        /// </summary>
        /// <returns>An array of locale-names, or <c>null</c> if no translation-files are installed.</returns>
        /// <remarks>
        /// <para>
        /// Note that the default LDraw locale, <c>en-AU</c>, is not represented by translation-files so will not be included in the returned array.
        /// </para>
        /// </remarks>
        public static string[] InstalledLocales
        {
            get
            {
                try
                {
                    DirectoryInfo di     = new DirectoryInfo(Path.Combine(Configuration.LDrawBase, "localisations"));
                    List<string> locales = new List<string>();

                    foreach (DirectoryInfo locale in di.EnumerateDirectories())
                    {
                        locales.Add(locale.Name);
                    }

                    if (0 == locales.Count)
                        return null;

                    return locales.ToArray();
                }
                catch (IOException)
                {
                    return null;
                }
            }
        }

        #endregion Catalog-management

        #region Internals

        private static readonly string[] Filenames = { "parts.txt", "categories.txt", "keywords.txt", "colours.txt" };
        private static readonly string[] TypeNames = { "PARTS", "CATEGORIES", "KEYWORDS", "COLOURS" };
        private static readonly Regex Translation  = new Regex("^\\s*\"(.*?)\"\\s*=\\s*\"(.*?)\"$");
        private static readonly Regex MovedTo      = new Regex("^~Moved to (.+)$");
        private static readonly Regex MovedToCheck = new Regex("^[^\\{\\}]*?\\{0\\}[^\\{\\}]*?$");

        private string _update;
        private string _author;

        #endregion Internals

        #region Properties

        /// <summary>
        /// Gets or sets the template to use when translating a "~Moved to" <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/>.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException"><see cref="Type"/> is not <see cref="Digitalis.LDTools.DOM.CatalogType.Titles"/>.</exception>
        /// <exception cref="T:System.ArgumentException">The supplied value is incorrectly formatted.</exception>
        /// <remarks>
        /// <para>
        /// If <see cref="Type"/> is <see cref="Digitalis.LDTools.DOM.CatalogType.Titles"/> and the key passed to <see cref="this"/> is in the form "~Moved to {name}",
        /// where "{name}" is an arbitrary string, then it is not required that an explicit match exist in the <see cref="LDTranslationCatalog"/> for a translation
        /// to be returned; instead, if no explicit match can be found, the translated string can be constructed automatically using the key and the value of
        /// this property.
        /// </para>
        /// <para>
        /// To enable this behaviour, the property must be set to a string which contains the character-sequence <c>{0}</c>. It must contain the sequence
        /// exactly once, and may not contain any other instances of the <c>{</c> or <c>}</c> characters. The sequence may appear at any point in the string.
        /// If the string is absent or invalid automatic translation will be disabled, and "~Moved to" keys will only be translated if they have an explict
        /// match in the <see cref="LDTranslationCatalog"/>.
        /// </para>
        /// <para>
        /// Default value is <c>null</c>.
        /// </para>
        /// </remarks>
        public string MovedToTemplate
        {
            get { return _movedToTemplate; }
            set
            {
                if (CatalogType.Titles != Type)
                    throw new InvalidOperationException("MovedToTemplate can only be set if Type is Titles");

                if (String.IsNullOrWhiteSpace(value))
                {
                    _movedToTemplate = null;
                    return;
                }

                if (!MovedToCheck.IsMatch(value))
                    throw new ArgumentException();

                _movedToTemplate = value;
            }
        }
        private string _movedToTemplate;

        /// <summary>
        /// Gets the type of the <see cref="LDTranslationCatalog"/>.
        /// </summary>
        public CatalogType Type { get; private set; }

        /// <summary>
        /// Gets the locale-name of the <see cref="LDTranslationCatalog"/>.
        /// </summary>
        public string LocaleName { get; private set;  }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LDTranslationCatalog"/> class with the specified values.
        /// </summary>
        /// <param name="localeName">Name of the locale to create.</param>
        /// <param name="type">The type of catalog to create.</param>
        public LDTranslationCatalog(string localeName, CatalogType type)
        {
            LocaleName = localeName;
            Type = type;
        }

        #endregion Constructor

        #region API

        /// <summary>
        /// Loads the <see cref="LDTranslationCatalog"/> from its underlying file in the <see cref="P:Digitalis.LDTools.DOM.Configuration.LDrawBase"/> localizations folder.
        /// </summary>
        /// <returns><c>true</c> if the translations-file existed and could be read; <c>false</c> otherwise</returns>
        /// <remarks>
        /// <para>
        /// Translation-files are stored as described in <see href="http://www.ldraw.org/article/559.html">the Localisation Guidelines</see>. If the
        /// <see cref="LDTranslationCatalog"/> has any values stored in it, they will be removed.
        /// </para>
        /// </remarks>
        public bool Load()
        {
            string path = Path.Combine(Configuration.LDrawBase, "localisations", LocaleName, Filenames[(int)Type]);

            if (!File.Exists(path))
                return false;

            using (TextReader reader = File.OpenText(path))
            {
                string line = reader.ReadLine();

                if (line != "0 LDraw.org Configuration File")
                    return false;

                Dictionary<string, string> translations = new Dictionary<string, string>();
                string[] fields;
                char[] separators = new char[] { ' ', '\t' };
                char[] translationSep = new char[] { '"' };

                line = reader.ReadLine();

                while (null != line)
                {
                    fields = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    if (fields.Length > 1)
                    {
                        switch (fields[1])
                        {
                            case "Name:":
                                if (3 != fields.Length || fields[2] != Filenames[(int)Type])
                                    return false;
                                break;

                            case "Author:":
                                if (fields.Length > 2)
                                    _author = line.Substring(line.IndexOf(fields[2]));
                                break;

                            case "!LICENSE":
                                break;

                            case "!LDRAW_ORG":
                                if (fields.Length >= 5 && "UPDATE" == fields[3])
                                    _update = fields[4];
                                break;

                            case "!TRANSLATION":
                                if (4 != fields.Length || fields[2] != TypeNames[(int)Type] || fields[3] != LocaleName)
                                    return false;
                                break;

                            default:
                                if (!Translation.IsMatch(line))
                                    return false;

                                GroupCollection groups = Translation.Match(line).Groups;
                                string key             = groups[1].Value;
                                string value           = groups[2].Value;

                                if (!String.IsNullOrWhiteSpace(value))
                                {
                                    if ("~Moved to %s" == key)
                                    {
                                        if (CatalogType.Titles == Type)
                                            MovedToTemplate = value.Replace("%s", "{0}");
                                    }
                                    else if (!translations.ContainsKey(key))
                                    {
                                        translations.Add(key, value);
                                    }
                                }
                                break;
                        }
                    }

                    line = reader.ReadLine();
                }

                if (0 == translations.Count && null == MovedToTemplate)
                    return false;

                Clear();
                _translations = translations;
            }

            return true;
        }

        /// <summary>
        /// Saves the <see cref="LDTranslationCatalog"/> to its file in the <see cref="P:Digitalis.LDTools.DOM.Configuration.LDrawBase"/> localizations folder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Translation-files are stored as described in <see href="http://www.ldraw.org/article/559.html">the Localisation Guidelines</see>. If the
        /// file this <see cref="LDTranslationCatalog"/> represents already exists it will be overwritten.
        /// </para>
        /// </remarks>
        public void Save()
        {
            if (Count < 0)
                return;

            string path   = Path.Combine(Configuration.LDrawBase, "localisations", LocaleName, Filenames[(int)Type]);
            string folder = Path.GetDirectoryName(path);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            using (TextWriter writer = File.CreateText(path))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("0 LDraw.org Configuration File\r\n");
                sb.AppendFormat("0 Name: {0}\r\n", Filenames[(int)Type]);

                if (!String.IsNullOrWhiteSpace(_author))
                {
                    sb.AppendFormat("0 Author: {0}\r\n", _author);
                }
                else
                {
                    sb.AppendFormat("0 Author: {0}", Configuration.Author);

                    if (!String.IsNullOrWhiteSpace(Configuration.Username))
                        sb.AppendFormat(" [{0}]\r\n", Configuration.Username);
                    else
                        sb.Append("\r\n");
                }

                sb.Append("0 !LDRAW_ORG Configuration");

                if (null != _update)
                    sb.AppendFormat(" UPDATE {0}\r\n", _update);
                else
                    sb.Append("\r\n");

                sb.AppendFormat("0 !TRANSLATION {0} {1}\r\n", TypeNames[(int)Type], LocaleName);
                sb.AppendFormat("0 !LICENSE {0}\r\n", Configuration.GetPartLicense(License.CCAL2));
                sb.Append("\r\n");

                IEnumerable<KeyValuePair<string, string>> entries = from n in this orderby n.Key select n;

                foreach (KeyValuePair<string, string> entry in entries)
                {
                    sb.AppendFormat("\"{0}\" = \"{1}\"\r\n", entry.Key, entry.Value);
                }

                if (null != MovedToTemplate)
                    sb.AppendFormat("\"~Moved to %s\" = \"{0}\"\r\n", MovedToTemplate.Replace("{0}", "%s"));

                writer.Write(sb.ToString());
            }
        }

        /// <summary>
        /// Returns the specified <see cref="Digitalis.LDTools.DOM.API.PageType"/> as a localized string.
        /// </summary>
        /// <param name="type">The type to return.</param>
        /// <returns>The type string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns the English name for <paramref name="type"/>.
        /// </para>
        /// <para>
        /// Localization for this method is provided by the DLL's translation catalogs, not the LDraw ones.
        /// </para>
        /// </remarks>
        public static string GetPageType(PageType type)
        {
            return Resources.ResourceManager.GetString(type.ToString());
        }

        /// <summary>
        /// Returns the localized form of the supplied part title.
        /// </summary>
        /// <param name="englishTitle">The title to localize.</param>
        /// <returns>The localised title.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns <paramref name="englishTitle"/>.
        /// </para>
        /// </remarks>
        public static string GetPartTitle(string englishTitle)
        {
            return GetPartTitle(CultureInfo.CurrentCulture.Name, englishTitle);
        }

        /// <summary>
        /// Returns the localized form of the supplied part title.
        /// </summary>
        /// <param name="localeName">The name of the locale to use, in the format <c>languagecode2-country/regioncode2</c>.</param>
        /// <param name="englishTitle">The title to localize.</param>
        /// <returns>The localised title.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <paramref name="localeName">specified locale</paramref>,
        /// the method returns <paramref name="englishTitle"/>.
        /// </para>
        /// </remarks>
        public static string GetPartTitle(string localeName, string englishTitle)
        {
            LDTranslationCatalog[] locale = LoadCatalog(localeName);

            if (null != locale)
            {
                LDTranslationCatalog catalog = locale[(int)CatalogType.Titles];

                if (null != catalog)
                {
                    string translation;

                    if (catalog.TryGetValue(englishTitle, out translation))
                        englishTitle = translation;
                }
            }

            return englishTitle;
        }

        /// <summary>
        /// Returns the localized form of the supplied keyword.
        /// </summary>
        /// <param name="englishKeyword">The keyword to localize.</param>
        /// <returns>The localized keyword.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns <paramref name="englishKeyword"/>.
        /// </para>
        /// </remarks>
        public static string GetKeyword(string englishKeyword)
        {
            return GetKeyword(CultureInfo.CurrentCulture.Name, englishKeyword);
        }

        /// <summary>
        /// Returns the localized form of the supplied keyword.
        /// </summary>
        /// <param name="localeName">The name of the locale to use, in the format <c>languagecode2-country/regioncode2</c>.</param>
        /// <param name="englishKeyword">The keyword to localize.</param>
        /// <returns>The localized keyword.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <paramref name="localeName">specified locale</paramref>,
        /// the method returns <paramref name="englishKeyword"/>.
        /// </para>
        /// </remarks>
        public static string GetKeyword(string localeName, string englishKeyword)
        {
            LDTranslationCatalog[] locale = LoadCatalog(localeName);

            if (null != locale)
            {
                LDTranslationCatalog catalog = locale[(int)CatalogType.Keywords];

                if (null != catalog)
                {
                    string translation;

                    if (catalog.TryGetValue(englishKeyword, out translation))
                        englishKeyword = translation;
                }
            }

            return englishKeyword;
        }

        /// <summary>
        /// Returns the specified <see cref="Category"/> as a localized string.
        /// </summary>
        /// <param name="category">The category to return.</param>
        /// <returns>The category string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns the English name of <paramref name="category"/>.
        /// </para>
        /// </remarks>
        public static string GetCategory(Category category)
        {
            return GetCategory(CultureInfo.CurrentCulture.Name, category);
        }

        /// <summary>
        /// Returns the specified <see cref="Category"/> as a localized string.
        /// </summary>
        /// <param name="localeName">The name of the locale to use, in the format <c>languagecode2-country/regioncode2</c>.</param>
        /// <param name="category">The category to return.</param>
        /// <returns>The category string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <paramref name="localeName">specified locale</paramref>,
        /// the method returns the English name for <paramref name="category"/>.
        /// </para>
        /// </remarks>
        public static string GetCategory(string localeName, Category category)
        {
            string categoryName;

            switch (category)
            {
                case Category.FigureAccessory:
                    categoryName = "Figure Accessory";
                    break;

                case Category.MinifigAccessory:
                    categoryName = "Minifig Accessory";
                    break;

                case Category.MinifigFootwear:
                    categoryName = "Minifig Footwear";
                    break;

                case Category.MinifigHeadwear:
                    categoryName = "Minifig Headwear";
                    break;

                case Category.MinifigHipwear:
                    categoryName = "Minifig Hipwear";
                    break;

                case Category.MinifigNeckwear:
                    categoryName = "Minifig Neckwear";
                    break;

                default:
                    if (category >= Category.Primitive_Unknown)
                        return category.ToString().Substring("Primitive_".Length);

                    categoryName = category.ToString();
                    break;
            }

            LDTranslationCatalog[] locale = LoadCatalog(localeName);

            if (null != locale)
            {
                LDTranslationCatalog catalog = locale[(int)CatalogType.Categories];

                if (null != catalog)
                {
                    string translation;

                    if (catalog.TryGetValue(categoryName, out translation))
                        return translation;
                }
            }

            return categoryName;
        }

        /// <summary>
        /// Returns the localized form of the specified colour name.
        /// </summary>
        /// <param name="englishName">The name to localize.</param>
        /// <returns>The localized string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns <paramref name="englishName"/>.
        /// </para>
        /// </remarks>
        public static string GetColourName(string englishName)
        {
            return GetColourName(CultureInfo.CurrentCulture.Name, englishName);
        }

        /// <summary>
        /// Returns the localized form of the specified colour name.
        /// </summary>
        /// <param name="localeName">The name of the locale to use, in the format <c>languagecode2-country/regioncode2</c>.</param>
        /// <param name="englishName">The name to localize.</param>
        /// <returns>The localized string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <paramref name="localeName">specified locale</paramref>,
        /// the method returns <paramref name="englishName"/>.
        /// </para>
        /// </remarks>
        public static string GetColourName(string localeName, string englishName)
        {
            LDTranslationCatalog[] locale = LoadCatalog(localeName);

            if (null != locale)
            {
                LDTranslationCatalog catalog = locale[(int)CatalogType.Colours];

                if (null != catalog)
                {
                    string translation;

                    if (catalog.TryGetValue(englishName, out translation))
                        englishName = translation;
                }
            }

            return englishName.Replace('_', ' ');
        }

        /// <summary>
        /// Returns the specified <see cref="Digitalis.LDTools.DOM.API.DocumentStatus"/> as a localized string.
        /// </summary>
        /// <param name="status">The status to return.</param>
        /// <returns>The status string.</returns>
        /// <remarks>
        /// <para>
        /// If no localized form is available for the <see cref="P:System.Globalization.CultureInfo.CurrentCulture">current locale</see>,
        /// the method returns the English name of <paramref name="status"/>.
        /// </para>
        /// <para>
        /// Localization for this method is provided by the DLL's translation catalogs, not the LDraw ones.
        /// </para>
        /// </remarks>
        public static string GetDocumentStatus(DocumentStatus status)
        {
            return Resources.ResourceManager.GetString(status.ToString());
        }

        #endregion API

        #region List-management

        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        /// <inheritdoc />
        public void Add(string key, string value)
        {
            _translations.Add(key, value);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _translations.ContainsKey(key);
        }

        /// <inheritdoc />
        public ICollection<string> Keys { get { return _translations.Keys; } }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            return _translations.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out string value)
        {
            if (_translations.TryGetValue(key, out value))
                return true;

            if (null != MovedToTemplate && MovedTo.IsMatch(key))
            {
                value = String.Format(MovedToTemplate, MovedTo.Match(key).Groups[1]);
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc />
        public ICollection<string> Values { get { return _translations.Values; } }

        /// <inheritdoc />
        public string this[string key]
        {
            get
            {
                string translation;

                if (TryGetValue(key, out translation))
                    return translation;

                return null;
            }
            set
            {
                _translations[key] = value;
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, string> item)
        {
            _translations.Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _translations.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, string> item)
        {
            return _translations.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection)_translations).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public int Count { get { return _translations.Count; } }

        /// <inheritdoc />
        public bool IsReadOnly { get { return false; } }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, string> item)
        {
            return _translations.Remove(item.Key);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _translations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _translations.GetEnumerator();
        }

        #endregion List-management
    }
}
