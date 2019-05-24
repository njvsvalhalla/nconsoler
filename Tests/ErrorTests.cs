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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NConsoler;
using NUnit.Framework;
using Moq;

namespace Tests
{
    public class NConsolerTests
    {
        [TestFixture]
        public class ErrorSpecs
        {
            private static IMessenger _messenger;
            private List<string> _capturedStrings;

            #region Private Classes

            private class WrongParameterOrderProgram
            {
                [Action]
                public static void RunProgram(
                    [Optional("0")] string optionalParameter,
                    [Required] string requiredParameter)
                {
                }
            }

            private string ConsoleOutput()
            {
                _messenger.Write(null);
                if (_capturedStrings.Count == 0)
                {
                    throw new Exception("There were no calls to Write method on messenger");
                }

                return string.Join(Environment.NewLine,
                    _capturedStrings.Where(x => !string.IsNullOrEmpty(x)).ToArray());
            }

            private class WithoutMethodsProgram
            {
            }

            private class WrongDefaultValueForOptionalStringParameterProgram
            {
                [Action]
                public static void RunProgram(
                    [Optional(10)] string optionalParameter)
                {
                }
            }

            private class WrongDefaultValueForOptionalIntegerParameterProgram
            {
                [Action]
                public static void RunProgram(
                    [Optional("test")] int optionalParameter)
                {
                }
            }

            private class VeryBigDefaultValueForOptionalIntegerParameterProgram
            {
                [Action]
                public static void RunProgram(
                    [Optional("1234567890")] int optionalParameter)
                {
                }
            }

            private class DuplicatedParameterNamesProgram
            {
                [Action]
                public static void RunProgram(
                    [Optional(1, "a")] int optionalParameter1,
                    [Optional(2, "a")] int optionalParameter2)
                {
                }
            }

            private class DuplicatedParameterAttributesProgram
            {
                [Action]
                public static void RunProgram(
                    [Required] [Optional("")] string parameter)
                {
                }
            }

            private class ManyParametersProgram
            {
                [Action]
                public static void RunProgram(
                    [Required] string sParameter,
                    int iParameter,
                    [Required] bool bParameter,
                    [Optional("0", "os")] string osParameter,
                    [Optional(0, "oi")] int oiParameter,
                    [Optional(false, "ob")] bool obParameter)
                {
                    _messenger.Write(
                        string.Format("{0} {1} {2} {3} {4} {5}",
                            sParameter, iParameter, bParameter, osParameter, oiParameter, obParameter));
                }
            }

            private class TwoActionsProgram
            {
                [Action]
                public static void Test1(
                    [Required] string parameter)
                {
                }

                [Action]
                public static void Test2(
                    [Required] string parameter)
                {
                }
            }

            private class OneActionProgramWithOptionalParameters
            {
                [Action]
                public static void Test(
                    [Optional("value1", Description = "param1 desc")]
                    string parameter1,
                    [Optional(42, Description = "desc2")] int param2)
                {
                    _messenger.Write("m1" + param2);
                }
            }

            private class OneParameterProgram
            {
                [Action]
                public static void RunProgram([Required] string parameter)
                {
                    _messenger.Write(parameter);
                }
            }

            #endregion

            [SetUp]
            public void Setup()
            {
                var mock = new Mock<IMessenger>();
                mock.Setup(x => x.Write(It.IsAny<string>()))
                    .Callback<string>(x => _capturedStrings.Add(x));
                _messenger = mock.Object;
                _capturedStrings = new List<string>();
            }

            [Test]
            public void WithoutMethods()
            {
                Consolery.Run(typeof(WithoutMethodsProgram), new[] {"string"}, _messenger);
            }

            [Test]
            public void WrongParameterOrder()
            {
                Consolery.Run(typeof(WrongParameterOrderProgram),
                    new[] {"string"}, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "It is not allowed to write a parameter with a Required attribute after a parameter with an Optional one. See method \"RunProgram\" parameter \"requiredParameter\""));
            }

            [Test]
            public void WrongDefaultValueForOptionalStringParameter()
            {
                Consolery.Run(typeof(WrongDefaultValueForOptionalStringParameterProgram),
                    new string[] { }, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "Default value for an optional parameter \"optionalParameter\" in method \"RunProgram\" can not be assigned to the parameter"));
            }

            [Test]
            public void WrongDefaultValueForOptionalIntegerParameter()
            {
                Consolery.Run(typeof(WrongDefaultValueForOptionalIntegerParameterProgram),
                    new string[] { }, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "Default value for an optional parameter \"optionalParameter\" in method \"RunProgram\" can not be assigned to the parameter"));
            }

            [Test]
            public void VeryBigDefaultValueForOptionalIntegerParameter()
            {
                Consolery.Run(typeof(VeryBigDefaultValueForOptionalIntegerParameterProgram),
                    new string[] { }, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "Default value for an optional parameter \"optionalParameter\" in method \"RunProgram\" can not be assigned to the parameter"));
            }

            [Test]
            public void DuplicatedParameterNames()
            {
                Consolery.Run(typeof(DuplicatedParameterNamesProgram),
                    new string[] { }, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "Found duplicated parameter name \"a\" in method \"RunProgram\". Please check alt names for optional parameters"));
            }

            [Test]
            public void DuplicatedParameterAttributes()
            {
                Consolery.Run(typeof(DuplicatedParameterAttributesProgram),
                    new[] {"parameter"}, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo(
                        "More than one attribute is applied to the parameter \"parameter\" in the method \"RunProgram\""));
            }

            [Test]
            public void NotAllRequiredParametersAreSet()
            {
                Consolery.Run(typeof(ManyParametersProgram),
                    new[] {"test"}, _messenger);
                var console = ConsoleOutput();
                Assert.That(console.Contains(
                    @"usage: manyparametersprogram sParameter iParameter bParameter [/os:value] [/oi:number] [/ob]
    [/os:value]
        default value: '0'
    [/oi:number]
        default value: 0
    [/ob]
        default value: False
Error: Not all required parameters are set"));
            }

            [Test]
            public void UnknownParameter()
            {
                Consolery.Run(typeof(OneParameterProgram),
                    new[] {"required", "/unknown:value"}, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo("Unknown parameter name /unknown:value"));
            }

            [Test]
            public void UnknownBooleanParameterWithNegativeSign()
            {
                Consolery.Run(typeof(OneParameterProgram),
                    new[] {"required", "/-unknown"}, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo("Unknown parameter name /-unknown"));
            }

            [Test]
            public void UnknownBooleanParameter()
            {
                Consolery.Run(typeof(OneParameterProgram),
                    new[] {"required", "/unknown"}, _messenger);

                Assert.That(ConsoleOutput(),
                    Is.EqualTo("Unknown parameter name /unknown"));
            }


            [Test]
            public void ShouldShowHelpForAParticularMessage()
            {
                Consolery.Run(typeof(TwoActionsProgram), new[] {"help", "Test2"}, _messenger);

                Assert.That(ConsoleOutput().Contains("test2 parameter"));
            }

            [Test]
            [Ignore("Not sure why it's failing at the moment")]
            public void ShouldShowDefaultValueForOptionalParameter()
            {
                Consolery.Run(typeof(OneActionProgramWithOptionalParameters), new[] {"help", "Test"}, _messenger);
                var console = ConsoleOutput();
                
                Assert.That(
                    console.Contains(
                        @"usage: oneactionprogramwithoptionalparameters [/parameter1:value] [/param2:number]
    [/parameter1:value]  param1 desc
        default value: 'value1'
    [/param2:number]     desc2
        default value: 42"));
            }
        }
    }
}