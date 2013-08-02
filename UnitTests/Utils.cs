#region License

//
// Utils.cs
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
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Digitalis.LDTools.DOM.API;

    #endregion Usings

    [TestClass]
    public class Utils
    {
        public delegate void DisposalAccessCallback();

        public static void DisposalAccessTest(IDOMObject obj, DisposalAccessCallback callback)
        {
            if (obj is ITexmapGeometry)
                ((ITexmapGeometry)obj).Texmap.Dispose();
            else
                obj.Dispose();

            try
            {
                callback();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), e.GetType(), "A disposed " + obj.GetType().FullName + " should not be accessible");
            }
        }

        private static readonly Regex RemoveWhitespace   = new Regex(@"(^\s*$[\r\n]*)|(^[ \t]+)|([ \t]+(?=(?>\r$|$)))|(^\s*0\s*$[\r\n]*)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex CollapseWhitespace = new Regex(@"[ \t]+", RegexOptions.Compiled | RegexOptions.Multiline);

        [TestMethod]
        public void TestPreProcessCode()
        {
            StringBuilder code = new StringBuilder();

            code.Append("12345\r\n");
            Assert.AreEqual("12345\r\n", PreProcessCode(code).ToString());
            code.Clear();

            // leading and trailing whitespace should be removed
            code.Append(" \t  12345\t   \r\n");
            Assert.AreEqual("12345\r\n", PreProcessCode(code).ToString());
            code.Clear();

            // empty lines should be removed
            code.Append("12345\r\n  \t  \r\n\r\n67890\r\n");
            Assert.AreEqual("12345\r\n67890\r\n", PreProcessCode(code).ToString());
            code.Clear();

            // excess space and tabs between fields should be collapsed
            code.Append("12345   67890\tab\r\n");
            Assert.AreEqual("12345 67890 ab\r\n", PreProcessCode(code).ToString());
            code.Clear();

            // empty comments should be removed
            code.Append("12345\r\n0\r\n0 \r\n67890\r\n");
            Assert.AreEqual("12345\r\n67890\r\n", PreProcessCode(code).ToString());
            code.Clear();

            // but empty comments with the '//' prefix should not
            code.Append("0 //\r\n");
            Assert.AreEqual("0 //\r\n", PreProcessCode(code).ToString());
        }

        public static StringBuilder PreProcessCode(StringBuilder sb)
        {
            string code = sb.ToString();

            code = RemoveWhitespace.Replace(code, "");
            code = CollapseWhitespace.Replace(code, " ");

            sb.Clear();
            sb.Append(code);

            return sb;
        }

        public static Stream SerializeBinary(object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            formatter.Serialize(stream, obj);
            stream.Position = 0;
            return stream;
        }

        public static object DeserializeBinary(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }
    }
}
