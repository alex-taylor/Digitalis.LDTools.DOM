#region License

//
// IMetaCommandTest.cs
//
// Copyright (C) 2009-2013 Alex Taylor.  All Rights Reserved.
//
// This file is part of Digitalis.LDTools.DOM.UnitTests.dll
//
// Digitalis.LDTools.DOM.UnitTests.dll is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Digitalis.LDTools.DOM.UnitTests.dll is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Digitalis.LDTools.DOM.UnitTests.dll.  If not, see <http://www.gnu.org/licenses/>.
//

#endregion License

namespace UnitTests
{
    #region Usings

    using System;
    using System.ComponentModel.Composition;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public abstract class IMetaCommandTest : IGroupableTest
    {
        #region Infrastructure

        protected sealed override IGroupable CreateTestGroupable()
        {
            return CreateTestMetaCommand();
        }

        protected abstract IMetaCommand CreateTestMetaCommand();

        protected abstract string[] SyntaxExamples { get; }

        #endregion Infrastructure

        #region Definition Test

        [TestMethod]
        public override void DefinitionTest()
        {
            IMetaCommand metaCommand = CreateTestMetaCommand();
            Assert.AreEqual(DOMObjectType.MetaCommand, metaCommand.ObjectType);

            ExportAttribute exportAttr = Attribute.GetCustomAttribute(TestClassType, typeof(ExportAttribute)) as ExportAttribute;
            Assert.IsNotNull(exportAttr);
            Assert.AreEqual(typeof(IMetaCommand), exportAttr.ContractType);

            // all pluggable elements must support these
            Assert.IsNotNull(TestClassType.GetConstructor(Type.EmptyTypes));
            Assert.IsNotNull(TestClassType.GetConstructor(new Type[] { typeof(string) }));

            TypeNameAttribute typeNameAttr = Attribute.GetCustomAttribute(TestClassType, typeof(TypeNameAttribute)) as TypeNameAttribute;
            Assert.IsNotNull(typeNameAttr);
            Assert.IsFalse(String.IsNullOrWhiteSpace(typeNameAttr.Description));

            DefaultIconAttribute defaultIconAttr = Attribute.GetCustomAttribute(TestClassType, typeof(DefaultIconAttribute)) as DefaultIconAttribute;
            Assert.IsNotNull(defaultIconAttr);
            Assert.IsNotNull(defaultIconAttr.Icon);

            // pluggable IMetaCommands must also support these
            MetaCommandPatternAttribute[] patterns = Attribute.GetCustomAttributes(TestClassType, typeof(MetaCommandPatternAttribute)) as MetaCommandPatternAttribute[];
            Assert.IsNotNull(patterns);
            Assert.AreNotEqual(0, patterns.Length);

            foreach (string code in SyntaxExamples)
            {
                bool matched = false;

                foreach (MetaCommandPatternAttribute attr in patterns)
                {
                    Regex regex = new Regex(attr.Pattern);

                    if (regex.IsMatch(code))
                    {
                        matched = true;
                        break;
                    }
                }

                Assert.IsTrue(matched, TestClassType + " failed to match '" + code + "'");
            }

            base.DefinitionTest();
        }

        #endregion Definition Test
    }
}
