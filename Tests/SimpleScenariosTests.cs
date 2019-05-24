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
using System.Dynamic;
using NConsoler;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
        public class SimpleScenarios
        {
            private static dynamic _verifier;

            private class OneParameterProgram
            {
                [Action]
                public static void RunProgram([Required]string parameter)
                {
                    _verifier.parameter = parameter;
                }
            }
            
            #region Private Test Helpers
            
            private enum TestEnum
            {
                One,
                Two
            }
            
            private class SpecificException : Exception
            {
                public SpecificException() { }
            }
            
            private class Net40OptionalArgumentsProgram
            {
                [Action]
                public static void Test(int required, bool optional = true)
                {
                    _verifier.required = required;
                    _verifier.optional = optional;
                }
            }
            
            private class EnumParameterProgram
            {
                [Action]
                public static void RunProgram([Required]TestEnum testEnum)
                {
                    _verifier.testEnum = testEnum;
                }
            }
            
            private class ManyParametersProgram
            {
                [Action]
                public static void RunProgram(
                    [Required]
                    string sParameter,
                    int iParameter,
                    [Required]
                    bool bParameter,
                    [Optional("0", "os")]
                    string osParameter,
                    [Optional(0, "oi")]
                    int oiParameter,
                    [Optional(false, "ob")]
                    bool obParameter)
                {
                    _verifier.sParameter = sParameter;
                    _verifier.iParameter = iParameter;
                    _verifier.bParameter = bParameter;
                    _verifier.osParameter = osParameter;
                    _verifier.oiParameter = oiParameter;
                    _verifier.obParameter = obParameter;
                }
            }
            
            private class ExceptionalProgram
            {
                [Action]
                public static void RunProgram([Optional(true)]bool parameter)
                {
                    throw new SpecificException();
                }
            }
            
            private class NullableParameterProgram
            {
                [Action]
                public static void RunProgram([Required]int? i)
                {
                    _verifier.i = i;
                }
            }

            private class EnumDecimalProgram
            {
                [Action]
                public static void RunProgram([Required]decimal d)
                {
                    _verifier.d = d;
                }
            }
            
            private class InstanceActionsProgram
            {
                [Action]
                public void Test(string arg)
                {
                    _verifier.arg = arg;
                }
            }
            
            private class WithoutArgumentsProgram
            {
                [Action]
                public static void Test()
                {
                    _verifier.TestCalled = true;
                }

                [Action]
                public static void Test1()
                {
                    _verifier.Test1Called = true;
                }
            }
            
            private class OptionalDateTimeProgram
            {
                [Action]
                public static void Test(DateTime required, [Optional("31-12-2008", "dtDate")]DateTime optional)
                {
                    _verifier.date = optional;
                }
            }
            
            private class OnlyOptionalParametersProgram
            {
                [Action]
                public static void RunProgram([Optional(true)]bool parameter)
                {
                    _verifier.parameter = parameter;
                }
            }
            
            #endregion
            
            [SetUp]
            public void Setup()
            {
                _verifier = new ExpandoObject();
            }

            [Test]
            public void OneParameter()
            {
                Consolery.Run(typeof(OneParameterProgram), new[] { "parameter" });

                Assert.That(_verifier.parameter, Is.EqualTo("parameter"));
            }

            [Test]
            public void OnlyOptionalParametersSpecified()
            {
                Consolery.Run(typeof(OnlyOptionalParametersProgram), new string[] { });

                Assert.That(_verifier.parameter, Is.EqualTo(true));
            }

            [Test]
            public void OptionalArgumentInconsistentWithActualParameters()
            {
                Consolery.Run(typeof(OnlyOptionalParametersProgram), new[] { "/-PARAMETER" });

                Assert.That(_verifier.parameter, Is.EqualTo(false));
            }
            

            [Test]
            public void ManyParameters()
            {
                Consolery.Run(typeof(ManyParametersProgram),
                    new[] { "string", "1", "true", "/os:string", "/oi:1", "/ob" });

                Assert.That(_verifier.sParameter, Is.EqualTo("string"));
                Assert.That(_verifier.iParameter, Is.EqualTo(1));
                Assert.That(_verifier.bParameter, Is.EqualTo(true));
                Assert.That(_verifier.osParameter, Is.EqualTo("string"));
                Assert.That(_verifier.oiParameter, Is.EqualTo(1));
                Assert.That(_verifier.obParameter, Is.EqualTo(true));
            }

            [Test]
            public void RunConsoleProgramWithoutOptionalParameters()
            {
                Consolery.Run(typeof(ManyParametersProgram),
                    new[] { "string", "1", "true" });

                Assert.That(_verifier.sParameter, Is.EqualTo("string"));
                Assert.That(_verifier.iParameter, Is.EqualTo(1));
                Assert.That(_verifier.bParameter, Is.EqualTo(true));
                Assert.That(_verifier.osParameter, Is.EqualTo("0"));
                Assert.That(_verifier.oiParameter, Is.EqualTo(0));
                Assert.That(_verifier.obParameter, Is.EqualTo(false));
            }

            [Test]
            public void NegativeBooleanParameter()
            {
                Consolery.Run(typeof(ManyParametersProgram),
                    new[] { "string", "1", "true", "/-ob" });

                Assert.That(_verifier.sParameter, Is.EqualTo("string"));
                Assert.That(_verifier.iParameter, Is.EqualTo(1));
                Assert.That(_verifier.bParameter, Is.EqualTo(true));
                Assert.That(_verifier.osParameter, Is.EqualTo("0"));
                Assert.That(_verifier.oiParameter, Is.EqualTo(0));
                Assert.That(_verifier.obParameter, Is.EqualTo(false));
            }

            [Test]
            public void NullableParameter()
            {
                Consolery.Run(typeof(NullableParameterProgram), new[] { "10" });

                Assert.That(_verifier.i, Is.EqualTo(10));
            }

            [Test]
            public void EnumParameterTest()
            {
                Consolery.Run(typeof(EnumParameterProgram), new[] { "One" });

                Assert.That(_verifier.testEnum, Is.EqualTo(TestEnum.One));
            }

            [Test]
            public void EnumDecimalTest()
            {
                Consolery.Run(typeof(EnumDecimalProgram), new[] { "1" });

                Assert.That(_verifier.d, Is.EqualTo(1));
            }

            [Test]
            public void should_work_with_instance_actions()
            {
                var instance = new InstanceActionsProgram();
                Consolery.Run(instance, new[] { "test" });

                Assert.That(_verifier.arg, Is.EqualTo("test"));
            }
            
            [Test]
            public void ShouldWorkWithNet40OptionalArguments()
            {
                Consolery.Run(typeof(Net40OptionalArgumentsProgram), new[] { "1" });

                Assert.That(_verifier.required, Is.EqualTo(1));
                Assert.That(_verifier.optional, Is.EqualTo(true));
            }

            [Test]
            public void ShouldWorkWithoutArgumentsInAction()
            {
                Consolery.Run(typeof(WithoutArgumentsProgram), new[] { "Test" });

                Assert.That(_verifier.TestCalled, Is.True);
            }

            [Test]
            public void WhenSpecifiedCaseForOptionalArgumentInconsistentWithActualParameters()
            {
                Consolery.Run(typeof(OnlyOptionalParametersProgram), new[] { "/-PARAMETER" });

                Assert.That(_verifier.parameter, Is.EqualTo(false));
            }

            [Test]
            public void WhenMetadataValidationFailsShouldSetErrorCode()
            {
                Consolery.Run(typeof(ExceptionalProgram), new[] { "wrong" });

                Assert.That(Environment.ExitCode, Is.EqualTo(1));
            }

            [Test]
            public void WhenTargetMethodThrowsAnException()
            {
                Exception exception = null;
                try
                {
                    Consolery.Run(typeof(ExceptionalProgram), new[] { "/-parameter" });
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.That(exception, Is.Not.Null);
                Assert.That(exception.GetType(), Is.EqualTo(typeof(SpecificException)));
            }
            

            [Test]
            public void ShouldCorrectlyConvertToDateTimeFromOptionalAttributeDefaultValue()
            {
                Consolery.Run(typeof(OptionalDateTimeProgram), new[] { "01-01-2009", "/dtDate:31-12-2008" });

                Assert.That(_verifier.date, Is.EqualTo(new DateTime(2008, 12, 31)));
            }
        }
}