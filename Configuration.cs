#region License

//
// Configuration.cs
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
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Digitalis.LDTools.DOM.API;
    using Digitalis.LDTools.DOM.Properties;
    using Digitalis.LDTools.Library;

    using Settings = Digitalis.LDTools.DOM.Properties.Settings;

    #endregion Usings

    /// <summary>
    /// Describes a class of <see cref="T:Digitalis.LDTools.DOM.API.IElement"/> provided by the DLL or one of its plugins.
    /// </summary>
    public class ElementDefinition
    {
        #region Internals

        private ConstructorInfo _defaultCtor;
        private ConstructorInfo _codeCtor;

        #endregion Internals

        #region Properties

        /// <summary>
        /// Gets the implementing type of the <see cref="T:Digitalis.LDTools.DOM.API.IElement"/>.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the type-name of the <see cref="Type"/> in a form suitable for display to the user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is normally the same as <see cref="P:Digitalis.LDTools.DOM.API.IDocumentElement.TypeName"/>, but the latter may change on a per-instance basis.
        /// </para>
        /// </remarks>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets an icon which represents the <see cref="Type"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="Type"/> supports it, this returns a 16x16 96dpi <see cref="T:System.Drawing.Image"/> which can be used to represent objects of the
        /// type; otherwise returns <c>null</c>.
        /// </para>
        /// </remarks>
        public Image DefaultIcon { get; private set; }

        /// <summary>
        /// Gets the category to which the <see cref="Type"/> belongs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value returned by this attribute may be used by applications to 'group' classes together for presentation to the user. It is a free-form string.
        /// If no category is specified by the <see cref="Type"/>, <c>null</c> is returned/
        /// </para>
        /// </remarks>
        public string Category { get; private set; }

        /// <summary>
        /// Gets the flags specified by the <see cref="Type"/>.
        /// </summary>
        public ElementFlags Flags { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinition"/> class with the specified values.
        /// </summary>
        /// <param name="type">The type of the <see cref="T:Digitalis.LDTools.DOM.API.IElement"/>.</param>
        /// <remarks>
        /// <para>
        /// The specified type must have a default (parameter-less) constructor, a constructor which takes a line of LDraw-code as a single parameter of type
        /// <see cref="T:System.String"/> and the <see cref="T:Digitalis.LDTools.DOM.API.TypeNameAttribute"/>. The
        /// <see cref="T:Digitalis.LDTools.DOM.API.ElementCategoryAttribute"/>, <see cref="T:Digitalis.LDTools.DOM.API.DefaultIconAttribute"/>
        /// and <see cref="T:Digitalis.LDTools.DOM.API.ElementFlagsAttribute"/> will be read if present.
        /// </para>
        /// </remarks>
        public ElementDefinition(Type type)
        {
            _defaultCtor = type.GetConstructor(Type.EmptyTypes);

            if (null == _defaultCtor)
                throw new ArgumentException("Type does not have a default constructor");

            if (type != typeof(LDPage))
            {
                _codeCtor = type.GetConstructor(new Type[] { typeof(string) });

                if (null == _codeCtor)
                    throw new ArgumentException("Type does not have a LDraw-code constructor");
            }

            TypeNameAttribute nameAttr = Attribute.GetCustomAttribute(type, typeof(TypeNameAttribute)) as TypeNameAttribute;

            if (null == nameAttr || null == nameAttr.Description)
                throw new ArgumentException("Type does not have a TypeName attribute");

            TypeName = nameAttr.Description;

            DefaultIconAttribute iconAttr = Attribute.GetCustomAttribute(type, typeof(DefaultIconAttribute)) as DefaultIconAttribute;

            if (null != iconAttr)
                DefaultIcon = iconAttr.Icon;

            ElementCategoryAttribute categoryAttr = Attribute.GetCustomAttribute(type, typeof(ElementCategoryAttribute)) as ElementCategoryAttribute;

            if (null != categoryAttr)
                Category = categoryAttr.Description;

            ElementFlagsAttribute flagsAttr = Attribute.GetCustomAttribute(type, typeof(ElementFlagsAttribute)) as ElementFlagsAttribute;

            if (null != flagsAttr)
                Flags = flagsAttr.Flags;

            Type = type;
        }

        #endregion Constructor

        #region Factory

        /// <summary>
        /// Instantiates <see cref="Type"/> via its default constructor.
        /// </summary>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public IElement Create()
        {
            return _defaultCtor.Invoke(null) as IElement;
        }

        /// <summary>
        /// Instantiates <see cref="Type"/> via its LDraw-code constructor.
        /// </summary>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public IElement Create(string code)
        {
            return _codeCtor.Invoke(new object[] { code }) as IElement;
        }

        #endregion Factory
    }

    /// <summary>
    /// Provides search and file paths, plugin-management and general utility functions.
    /// </summary>
    public sealed class Configuration
    {
        #region Inner Types

        private class PluggableElementDefinition
        {
            public readonly IList<Regex> Patterns = new List<Regex>();

            public Type Type
            {
                get { return _type; }
                set
                {
                    _ci   = value.GetConstructor(new Type[] { typeof(string) });
                    _type = value;
                }
            }
            private Type _type;

            private ConstructorInfo _ci;

            public IElement Create(string code)
            {
                return _ci.Invoke(new object[] { code }) as IElement;
            }
        }

        #endregion Inner Types

        #region Internals

        // used to do late-binding of the Controls DLL for the ElementEditorPanels
        private static Assembly ControlsDLL
        {
            get
            {
                try
                {
                    if (null == _controlsDLL)
                        _controlsDLL = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Digitalis.LDTools.Controls.dll"));
                }
                catch
                {
                }

                return _controlsDLL;
            }
        }
        private static Assembly _controlsDLL;

        internal static ConstructorInfo GetEditorConstructor(string name, Type elementType)
        {
            if (null == ControlsDLL)
                return null;

            Type type = ControlsDLL.GetType(name);

            if (null == type)
                return null;

            return type.GetConstructor(new Type[] { elementType });
        }

        static Configuration()
        {
            new Configuration();

            Settings.Default.Reload();

            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
            }

            if (String.IsNullOrWhiteSpace(Settings.Default.AuthorName))
                Settings.Default.AuthorName = Environment.UserName;

            // see if we can locate the installed LDraw library
            List<string> paths = new List<string>();

            // this is where our own config says it is
            if (!String.IsNullOrEmpty(Settings.Default.BasePath))
                paths.Add(Settings.Default.BasePath);

            // check the user's LDraw.ini file if present
            string iniFile = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "LDraw.ini");

            if (File.Exists(iniFile))
            {
                using (TextReader tr = File.OpenText(iniFile))
                {
                    string line;

                    while (null != (line = tr.ReadLine()))
                    {
                        if (line.StartsWith("BaseDirectory="))
                        {
                            paths.Add(line.Substring("BaseDirectory=".Length).Trim());
                            break;
                        }
                    }
                }
            }

            // and a few 'obvious' places
            paths.Add(@"C:\LDraw");
            paths.Add(Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "LDraw"));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LDraw"));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LDraw"));

            foreach (string path in paths)
            {
                try
                {
                    ValidateBasePath(path, out _primarySearchPath, out _fullSearchPath);
                    Settings.Default.BasePath = path;
                    break;
                }
                catch
                {
                    // ignore exceptions
                    Settings.Default.BasePath = null;
                }
            }

            DecimalPlacesCoordinatesFormatter = Formatters[Settings.Default.DpCoords];
            DecimalPlacesPrimitivesFormatter = Formatters[Settings.Default.DpPrimitives];
            DecimalPlacesTransformsFormatter = Formatters[Settings.Default.DpTransforms];

            Settings.Default.Save();
        }

        #endregion Internals

        #region LDraw Library location

        private static void ValidateBasePath(string value, out List<string> primary, out List<string> full)
        {
            string path;

            if (!Directory.Exists(value))
                throw new DirectoryNotFoundException(value);

            LDConfigPath = Path.Combine(value, "ldconfig.ldr");

            if (!File.Exists(LDConfigPath))
                throw new FileNotFoundException(LDConfigPath);

            primary = new List<string>();
            full = new List<string>();

            // these are optional
            path = Path.Combine(value, "My Parts");
            AddPath(path, false, primary, full);
            AddPath(Path.Combine(path, "s"), false, null, full);
            string unofficial = Path.Combine(value, "unofficial");
            path = Path.Combine(unofficial, "parts");
            AddPath(path, false, primary, full);
            AddPath(Path.Combine(path, "s"), false, null, full);
            path = Path.Combine(unofficial, "p");
            AddPath(path, false, primary, full);
            AddPath(Path.Combine(path, "48"), false, null, full);
            AddPath(Path.Combine(value, "models"), false, primary, full);

            // these are mandatory
            path = Path.Combine(value, "parts");
            AddPath(path, true, primary, full);
            AddPath(Path.Combine(path, "s"), true, null, full);
            path = Path.Combine(value, "p");
            AddPath(path, true, primary, full);
            AddPath(Path.Combine(path, "48"), true, null, full);
        }

        private static void AddPath(string path, bool mandatory, List<string> primary, List<string> full)
        {
            if (mandatory && !Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            if (null != primary)
                primary.Add(path);

            full.Add(path);
        }

        /// <summary>
        /// Gets the main search-path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This consists of the following folders:
        /// <list type="bullet">
        ///   <item><term><see cref="LDrawBase"/>\My Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\P</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Models</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\P</term></item>
        /// </list>
        /// </para>
        /// <para>
        /// This is the search-path that <see cref="LDReference"/> will use when attempting to resolve its <see cref="LDReference.Target"/>.
        /// </para>
        /// </remarks>
        public static IEnumerable<string> PrimarySearchPath { get { return _primarySearchPath; } }
        private static List<string> _primarySearchPath = new List<string>();

        /// <summary>
        /// Gets the full search-path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This consists of the following folders:
        /// <list type="bullet">
        ///   <item><term><see cref="LDrawBase"/>\My Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\My Parts\S</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\Parts\S</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\P</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Unofficial\P\48</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Models</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Parts\S</term></item>
        ///   <item><term><see cref="LDrawBase"/>\P</term></item>
        ///   <item><term><see cref="LDrawBase"/>\P\48</term></item>
        /// </list>
        /// </para>
        /// <para>
        /// This is the search-path that <see cref="LibraryManager"/> will use when building its <see cref="LibraryManager.Cache">cache</see>.
        /// </para>
        /// </remarks>
        public static IEnumerable<string> FullSearchPath { get { return _fullSearchPath; } }
        private static List<string> _fullSearchPath = new List<string>();

        /// <summary>
        /// Gets or sets the path of the LDraw base folder.
        /// </summary>
        /// <exception cref="T:System.IO.FileNotFoundException">The ldconfig.ldr file cannot be read.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">One of the required folders cannot be found.</exception>
        /// <remarks>
        /// <para>
        /// This is the folder where the LDraw parts library is stored, and is required to contain at least the following items:
        /// <list type="bullet">
        ///   <item><term>ldconfig.ldr</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Parts</term></item>
        ///   <item><term><see cref="LDrawBase"/>\Parts\S</term></item>
        ///   <item><term><see cref="LDrawBase"/>\P</term></item>
        ///   <item><term><see cref="LDrawBase"/>\P\48</term></item>
        /// </list>
        /// On loading, the DLL attempts to locate the LDraw base folder by checking the following locations in order:
        /// <list type="bullet">
        ///   <item><term>The value stored in the application's Application Settings file</term></item>
        ///   <item><term>The user's LDraw.ini file</term></item>
        ///   <item><term>C:\LDraw</term></item>
        ///   <item><term>The user's home folder</term></item>
        ///   <item><term>The user's My Documents folder</term></item>
        ///   <item><term>The Program Files folder</term></item>
        /// </list>
        /// If any of these is found to contain a valid LDraw library, it will be used.
        /// </para>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// <para>
        /// Raises the <see cref="LDrawBaseChanged"/> event when its value changes.
        /// </para>
        /// </remarks>
        public static string LDrawBase
        {
            get { return Settings.Default.BasePath; }
            set
            {
                if (value != Settings.Default.BasePath)
                {
                    List<string> primary;
                    List<string> full;

                    ValidateBasePath(value, out primary, out full);

                    Settings.Default.BasePath = value;
                    Settings.Default.Save();

                    _primarySearchPath = primary;
                    _fullSearchPath    = full;

                    if (null != LDrawBaseChanged)
                        LDrawBaseChanged(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when <see cref="LDrawBase"/> changes.
        /// </summary>
        public static event EventHandler LDrawBaseChanged;

        /// <summary>
        /// Gets the path to the ldconfig.ldr file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="LDrawBase"/> is not valid, this will return <c>null</c>.
        /// </para>
        /// </remarks>
        public static string LDConfigPath { get; private set; }

        #endregion LDraw Library location

        #region Properties

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Model"/>.
        /// </summary>
        public static readonly Image ModelIcon = Resources.ModelIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/>.
        /// </summary>
        public static readonly Image PartIcon = Resources.PartIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Alias"/>.
        /// </summary>
        public static readonly Image PartAliasIcon = Resources.PartAliasIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Part_Physical_Colour"/>.
        /// </summary>
        public static readonly Image PartPhysicalColourIcon = Resources.PartPhysicalColourIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut"/>.
        /// </summary>
        public static readonly Image ShortcutIcon = Resources.ShortcutIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Alias"/>.
        /// </summary>
        public static readonly Image ShortcutAliasIcon = Resources.ShortcutAliasIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Shortcut_Physical_Colour"/>.
        /// </summary>
        public static readonly Image ShortcutPhysicalColourIcon = Resources.ShortcutPhysicalColourIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Subpart"/>.
        /// </summary>
        public static readonly Image SubpartIcon = Resources.SubpartIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/> or <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/>.
        /// </summary>
        public static readonly Image PrimitiveIcon = Resources.PrimitiveIcon;

        /// <summary>
        /// Gets an icon representing LDraw files of type <see cref="Digitalis.LDTools.DOM.API.PageType.Part"/> which are classed as 'sub-assembly' parts.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A 'sub-assembly' part is a part whose <see cref="P:Digitalis.LDTools.DOM.API.IPage.Title"/> begins with a tilde.
        /// </para>
        /// </remarks>
        public static readonly Image SubassemblyIcon = Resources.SubassemblyIcon;

        /// <summary>
        /// Returns an array of icons representing the values in <see cref="Digitalis.LDTools.DOM.API.PageType"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The array is indexed by <see cref="Digitalis.LDTools.DOM.API.PageType"/>.
        /// </para>
        /// </remarks>
        public static readonly Image[] PageTypeIcons =
        {
            Resources.ModelIcon,
            Resources.PartIcon,
            Resources.PartAliasIcon,
            Resources.PartPhysicalColourIcon,
            Resources.ShortcutIcon,
            Resources.ShortcutAliasIcon,
            Resources.ShortcutPhysicalColourIcon,
            Resources.SubpartIcon,
            Resources.PrimitiveIcon,
            Resources.PrimitiveIcon         // for HiresPrimitive
        };

        /// <summary>
        /// Gets or sets the LDraw.org username of the current user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will be used when creating new <see cref="T:Digitalis.LDTools.DOM.LDHistory"/> and <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> objects.
        /// </para>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// </remarks>
        public static string Username { get { return Settings.Default.UserName; } set { Settings.Default.UserName = value; Settings.Default.Save(); } }

        /// <summary>
        /// Gets or sets the real name of the current user.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will be used when creating new <see cref="T:Digitalis.LDTools.DOM.LDHistory"/> and <see cref="T:Digitalis.LDTools.DOM.API.IPage"/> objects.
        /// </para>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// <para>
        /// Default value is the name of the currently logged-in user.
        /// </para>
        /// </remarks>
        public static string Author { get { return Settings.Default.AuthorName; } set { Settings.Default.AuthorName = value; Settings.Default.Save(); } }

        /// <summary>
        /// Gets or sets the number of decimal-places that should be used for coordinates when displaying them to the user or writing them to a file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This setting applies to all coordinates other than those of elements in a primitive, which use the <see cref="DecimalPlacesPrimitives"/> setting.
        /// </para>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// <para>
        /// Valid range is <c>0..15</c>. Defaults to <c>3</c>.
        /// </para>
        /// </remarks>
        public static uint DecimalPlacesCoordinates
        {
            get { return Settings.Default.DpCoords; }
            set
            {
                DecimalPlacesCoordinatesFormatter = Formatters[value];
                Settings.Default.DpCoords         = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets a string-formatter that should be used for coordinates when displaying them to the user or writing them to a file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Coordinates of elements in a primitive should use <see cref="DecimalPlacesPrimitivesFormatter"/> instead.
        /// </para>
        /// </remarks>
        public static string DecimalPlacesCoordinatesFormatter { get; private set; }

        /// <summary>
        /// Gets or sets the number of decimal-places that should be used for coordinates of elements in a primitive when displaying them to the user or writing them to a file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// <para>
        /// Valid range is <c>0..15</c>. Defaults to <c>4</c>.
        /// </para>
        /// </remarks>
        public static uint DecimalPlacesPrimitives
        {
            get { return Settings.Default.DpPrimitives; }
            set
            {
                DecimalPlacesPrimitivesFormatter = Formatters[value];
                Settings.Default.DpPrimitives    = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets a string-formatter that should be used for coordinates of elements in a primitive when displaying them to the user or writing them to a file.
        /// </summary>
        public static string DecimalPlacesPrimitivesFormatter { get; private set; }

        /// <summary>
        /// Gets or sets the number of decimal-places that should be used for transforms when displaying them to the user or writing them to a file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is persisted in the calling application's Application Settings file. Applications are recommended to
        /// provide the user with a means to set the value.
        /// </para>
        /// <para>
        /// Valid range is <c>0..15</c>. Defaults to <c>5</c>.
        /// </para>
        /// </remarks>
        public static uint DecimalPlacesTransforms
        {
            get { return Settings.Default.DpTransforms; }
            set
            {
                DecimalPlacesTransformsFormatter = Formatters[value];
                Settings.Default.DpTransforms    = value;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets a string-formatter that should be used for transforms when displaying them to the user or writing them to a file.
        /// </summary>
        public static string DecimalPlacesTransformsFormatter { get; private set; }

        /// <summary>
        /// Gets an array of string-formatters for floating-point numbers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returns a 16-element array of format strings which can be used with <b>ToString()</b> to output <c>double</c> or <c>float</c> values
        /// with a specific number of decimal-places. The array is indexed by the number of decimal-places required.
        /// </para>
        /// </remarks>
        public static readonly string[] Formatters =
        {
            "0",
            "0.#",
            "0.##",
            "0.###",
            "0.####",
            "0.#####",
            "0.######",
            "0.#######",
            "0.########",
            "0.#########",
            "0.##########",
            "0.###########",
            "0.############",
            "0.#############",
            "0.##############",
            "0.###############"
        };

        #endregion Properties

        #region Constructor

        // private to prevent anyone else from instantiating us
        private Configuration()
        {
            LoadPluggableElements();
            LoadElements();
        }

        #endregion Constructor

        #region Element Discovery

        /// <summary>
        /// Returns details of the element-types available in the system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This includes both the classes built-in to the DLL and any loaded from its plugins.
        /// </para>
        /// </remarks>
        public static IEnumerable<ElementDefinition> ElementTypes { get; private set; }

        private void LoadElements()
        {
            Dictionary<Type, ElementDefinition> elements = new Dictionary<Type, ElementDefinition>();

            // first do the built-in types
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && typeof(IElement).IsAssignableFrom(type))
                {
                    try
                    {
                        elements[type] = new ElementDefinition(type);
                    }
                    catch
                    {
                    }
                }
            }

            // then iterate over the meta-commands
            foreach (PluggableElementDefinition definition in _pluggableElements)
            {
                if (!elements.ContainsKey(definition.Type))
                {
                    try
                    {
                        elements[definition.Type] = new ElementDefinition(definition.Type);
                    }
                    catch
                    {
                    }
                }
            }

            // TODO: CompositeElements

            ElementTypes = elements.Values;
        }

        #endregion Element Discovery

        #region Pluggable Elements

        [ImportMany]
        private IEnumerable<IMetaCommand> _metaCommandImports = null;

        private static List<PluggableElementDefinition> _pluggableElements = new List<PluggableElementDefinition>();

        private void LoadPluggableElements()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            using (CompositionContainer components = new CompositionContainer(catalog))
            {
                components.ComposeParts(this);

                foreach (IElement element in _metaCommandImports)
                {
                    if (CheckPluggableMetaCommand(element))
                    {
                        PluggableElementDefinition def = new PluggableElementDefinition();
                        def.Type = element.GetType();

                        foreach (MetaCommandPatternAttribute attr in Attribute.GetCustomAttributes(def.Type, typeof(MetaCommandPatternAttribute)))
                        {
                            def.Patterns.Add(new Regex(attr.Pattern));
                        }

                        _pluggableElements.Add(def);
                    }
                }
            }
        }

        private bool CheckPluggableElement(Type type)
        {
            if (null == type.GetConstructor(Type.EmptyTypes))
                return false;

            if (null == type.GetConstructor(new Type[] { typeof(string) }))
                return false;

            TypeNameAttribute typeName = Attribute.GetCustomAttribute(type, typeof(TypeNameAttribute)) as TypeNameAttribute;

            if (null == typeName || String.IsNullOrWhiteSpace(typeName.Description))
                return false;

            DefaultIconAttribute defaultIcon = Attribute.GetCustomAttribute(type, typeof(DefaultIconAttribute)) as DefaultIconAttribute;

            if (null == defaultIcon || null == defaultIcon.Icon)
                return false;

            return true;
        }

        private bool CheckPluggableMetaCommand(IElement element)
        {
            Type type = element.GetType();

            if (!CheckPluggableElement(type))
                return false;

            if (element is IMetaCommand)
            {
                MetaCommandPatternAttribute[] patterns = Attribute.GetCustomAttributes(type, typeof(MetaCommandPatternAttribute)) as MetaCommandPatternAttribute[];

                if (null == patterns || 0 == patterns.Length)
                    return false;
            }

            return true;
        }

        static internal IMetaCommand CreateMetaCommand(string code)
        {
            foreach (PluggableElementDefinition def in _pluggableElements)
            {
                foreach (Regex pattern in def.Patterns)
                {
                    if (pattern.IsMatch(code))
                    {
                        try
                        {
                            return def.Create(code) as IMetaCommand;
                        }
                        catch
                        {
                            // carry on
                        }
                    }
                }
            }

            return null;
        }

        #endregion Pluggable Elements

        #region API

        private static readonly string[] Licenses = { "Unknown", "Redistributable under CCAL version 2.0 : see CAreadme.txt", "Not redistributable : see NonCAreadme.txt" };

        /// <summary>
        /// Returns the text of the specified <see cref="Digitalis.LDTools.DOM.API.License"/>.
        /// </summary>
        /// <param name="license">The license to return.</param>
        /// <returns>The license string.</returns>
        public static string GetPartLicense(License license)
        {
            return Licenses[(int)license];
        }

        private static readonly Regex RxEdge = new Regex(@"^\d+-\d+edg[eh](\.dat)?$");
        private static readonly Regex RxNdis = new Regex(@"^\d+-\d+ndis(\.dat)?$");
        private static readonly Regex RxDisc = new Regex(@"^\d+-\d+disc(\.dat)?$");
        private static readonly Regex RxChrd = new Regex(@"^\d+-\d+chrd?(\.dat)?$");
        private static readonly Regex RxCyli = new Regex(@"^\d+-\d+cyli?2?(\.dat)?$");
        private static readonly Regex RxCylo = new Regex(@"^\d+-\d+cylo(\.dat)?$");
        private static readonly Regex RxCyls = new Regex(@"^\d+-\d+cyls2?(\.dat)?$");
        private static readonly Regex RxCylc = new Regex(@"^\d+-\d+cylc2?(\.dat)?$");
        private static readonly Regex RxSphe = new Regex(@"^\d+-\d+sphe(\.dat)?$");
        private static readonly Regex RxBump = new Regex(@"^bump\d+(\.dat)?$");
        private static readonly Regex RxRing = new Regex(@"^(\d+-\d+)?(aring|(r|ri|rin|ring)\d+)(\.dat)?$");
        private static readonly Regex RxCone = new Regex(@"^\d+-\d+(co|con|cone)\d+(\.dat)?$");
        private static readonly Regex RxBox  = new Regex(@"^box(\d?[-otu]?\d?[ap]?|jcyl\d+)(\.dat)?$");
        private static readonly Regex RxTri  = new Regex(@"^(\d+-\d+tric|tri\d+([au]\d+s?)?)(\.dat)?$");
        private static readonly Regex RxType = new Regex(@"^type.+(\.dat)?$");

        /// <summary>
        /// Attempts to work out a category from a page's title and/or name.
        /// </summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="name">The filename of the page.</param>
        /// <param name="type">The type of the page.</param>
        /// <returns>One of the <see cref="Digitalis.LDTools.DOM.API.Category"/> values.</returns>
        /// <remarks>
        /// <para>
        /// If it is not possible to determine the category from the supplied title and name, either <see cref="Digitalis.LDTools.DOM.API.Category.Primitive_Unknown"/>
        /// (when <paramref name="type"/> is <see cref="Digitalis.LDTools.DOM.API.PageType.Primitive"/> or <see cref="Digitalis.LDTools.DOM.API.PageType.HiresPrimitive"/>)
        /// or <see cref="Digitalis.LDTools.DOM.API.Category.Unknown"/> is returned.
        /// </para>
        /// </remarks>
        public static Category GetCategoryFromName(string title, string name, PageType type)
        {
            Category category = Category.Unknown;

            switch (type)
            {
                case PageType.Part:
                case PageType.Part_Alias:
                case PageType.Part_Physical_Colour:
                case PageType.Shortcut:
                case PageType.Shortcut_Alias:
                case PageType.Shortcut_Physical_Colour:
                case PageType.Subpart:
                    title = title.Split()[0].Trim(new char[] { '_', '~' });

                    if (!EnumHelper.ToValue(title, true, out category))
                        category = Category.Unknown;
                    break;

                case PageType.Model:
                    category = Category.Unknown;
                    break;

                case PageType.Primitive:
                case PageType.HiresPrimitive:
                    name     = name.ToLower();
                    category = Category.Primitive_Unknown;

                    if (RxEdge.IsMatch(name))
                    {
                        category = Category.Primitive_Edge;
                    }
                    else if (RxNdis.IsMatch(name) || RxDisc.IsMatch(name))
                    {
                        category = Category.Primitive_Disc;
                    }
                    else if (RxChrd.IsMatch(name))
                    {
                        category = Category.Primitive_Chord;
                    }
                    else if (RxCyli.IsMatch(name) || RxCyls.IsMatch(name) || RxCylc.IsMatch(name) || RxCylo.IsMatch(name))
                    {
                        category = Category.Primitive_Cylinder;
                    }
                    else if (RxCone.IsMatch(name))
                    {
                        category = Category.Primitive_Cone;
                    }
                    else if (RxSphe.IsMatch(name) || RxBump.IsMatch(name))
                    {
                        category = Category.Primitive_Sphere;
                    }
                    else if (RxRing.IsMatch(name))
                    {
                        category = Category.Primitive_Ring;
                    }
                    else if (RxBox.IsMatch(name) || RxTri.IsMatch(name))
                    {
                        category = Category.Primitive_Box;
                    }
                    else if (RxType.IsMatch(name))
                    {
                        category = Category.Primitive_Text;
                    }
                    else
                    {
                        title = title.ToLower();

                        string firstWord = title.Split()[0].Trim(new char[] { '_', '~' });

                        switch (firstWord)
                        {
                            case "axle":
                            case "bush":
                                category = Category.Primitive_Technic;
                                break;

                            default:
                                if (!EnumHelper.ToValue("Primitive_" + firstWord, true, out category))
                                {
                                    if (title.Contains("stud"))
                                        category = Category.Primitive_Stud;
                                    else if (title.Contains("technic"))
                                        category = Category.Primitive_Technic;
                                    else if (title.Contains("torus"))
                                        category = Category.Primitive_Torus;
                                    else
                                        category = Category.Primitive_Unknown;
                                }
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }

            return category;
        }

        #endregion API
    }
}
