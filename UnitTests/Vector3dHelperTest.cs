#region License

//
// Vector3dHelperTest.cs
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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OpenTK;

    using Digitalis.LDTools.Utils;

    #endregion Usings

    [TestClass]
    public class Vector3dHelperTest
    {
        [TestMethod]
        public void ToStringTest()
        {
            string expected = "(1, 2, 3)";
            string actual   = new Vector3d(1.0, 2.0, 3.0).ToString(1);
            Assert.AreEqual(expected, actual);

            expected = "(1.1, 2.2, 3.3)";
            actual   = new Vector3d(1.1, 2.2, 3.3).ToString(1);
            Assert.AreEqual(expected, actual);

            expected = "(1, 2, 3)";
            actual   = new Vector3d(1.1, 2.2, 3.3).ToString(0);
            Assert.AreEqual(expected, actual);

            expected = "(2, 2, 3)";
            actual   = new Vector3d(1.8, 2.2, 3.3).ToString(0);
            Assert.AreEqual(expected, actual);

            expected = "(1.1, 2.2, 3.3)";
            actual   = new Vector3d(1.11, 2.2, 3.3).ToString(1);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseTest_ValidStrings()
        {
            Vector3d expected = new Vector3d(10.0, 20.0, 30.0);
            Vector3d actual;

            string[] strings = new string[]
            {
                "(10.0, 20.0, 30.0)",
                "10.0,20.0,30.0",
                "10.0 20.0 30.0",
                "10 20 30",
                "10\t20\t30",
                expected.ToString(),
            };

            foreach (string s in strings)
            {
                actual = Vector3dHelper.Parse(s);
                Assert.AreEqual(expected, actual, "Failed to parse string '" + s + "'");
            }
        }

        [TestMethod]
        public void ParseTest_InvalidStrings()
        {
            Vector3d actual;
            int      i        = 0;

            string[] strings = new string[]
            {
                "1.0.0 20.0 30.0",      // too many decimal-points
                "1",                    // too few groups
                "1 2 3 4",              // too many groups
                "a b c",                // non-numeric input
                "[1, 2, 3]",            // unsupported brackets
                "{1, 2, 3}",            // unsupported brackets
                "1.0, -1.2, 1-9",       // negative-sign in the wrong place
                "1,,2,3",               // too many groups
            };

            try
            {
                foreach (string s in strings)
                {
                    actual = Vector3dHelper.Parse(s);
                    i++;
                }

                Assert.Fail("Successfully parsed invalid string '" + strings[i] + "'");
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }

        [TestMethod]
        public void ParseTest_InvalidGroupCount()
        {
            // too few values
            try
            {
                Vector3dHelper.Parse("1");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }

            // too many values
            try
            {
                Vector3dHelper.Parse("1 2 3 4");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(FormatException), e.GetType());
            }
        }
    }
}
