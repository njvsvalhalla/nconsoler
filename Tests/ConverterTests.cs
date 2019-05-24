//      The contents of this file are subject to the Mozilla Public License
//      Version 1.1 (the "License"); you may not use this file except in
//      compliance with the License. You may obtain a copy of the License at
//      https://www.mozilla.org/MPL/

//      Software distributed under the License is distributed on an "AS IS"
//      basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//      License for the specific language governing rights and limitations
//      under the License.
//      The Original Code is located at the nconsoler github:
//      https://github.com/csharpus/nconsoler.

//      The Initial Developer of the Original Code is csharupus.
//      Portions created by Neal Daniel (neal@nealmdaniel.com) are Copyright (C)
//      Neal Daniel (neal@nealmdaniel.com). All Rights Reserved.
//      Contributor(s): Neal Daniel (neal@nealmdaniel.com).

using NConsoler;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ConverterTests
    {
        public enum TestEnum
        {
            First,
            Second,
            FIRST
        }

        [Test]
        public void TestDecimalParameter()
        {
            var result = (decimal)StringToObject.ConvertValue("10.00", typeof (decimal));
            Assert.That(result == 10.00m);
        }

        [Test]
        public void TestStringParameter()
        {
            var result = (string)StringToObject.ConvertValue("test", typeof(string));
            Assert.That(result == "test");
        }

        [Test]
        public void TestIntegerParameter()
        {
            var result = (int)StringToObject.ConvertValue("11", typeof(int));
            Assert.That(result == 11);
        }

        [Test]
        public void TestDoubleParameter()
        {
            var result = (double)StringToObject.ConvertValue("11.11", typeof(double));
            Assert.That(result == 11.11d);
        }

        [Test]
        public void TestCharParameter()
        {
            var result = (char)StringToObject.ConvertValue("a", typeof(char));
            Assert.That(result == "a".ToCharArray()[0]);
        }

        [Test]
        public void TestBooleanParameter()
        {
            var result = (bool)StringToObject.ConvertValue("true", typeof(bool));
            Assert.That(result);
        }

        [Test]
        public void TestEnumParameter()
        {
            var result = (TestEnum)StringToObject.ConvertValue("First", typeof(TestEnum));
            Assert.That(result == TestEnum.First);
        }

        [Test]
        public void TestNullableParameter()
        {
            var result = (int?)StringToObject.ConvertValue("10", typeof(int?));
            var nullResult = (int?)StringToObject.ConvertValue("", typeof(int?));
			
            Assert.That(result == 10);
            Assert.That(nullResult == null);
        }

        [Test]
        public void TestStringEmptyParameter()
        {
            var result = (int)StringToObject.ConvertValue("", typeof(int));
            Assert.That(result == 0);
        }
    }
}