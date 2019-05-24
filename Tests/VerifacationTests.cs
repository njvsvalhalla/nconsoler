using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
	public class VerificationTests
	{
		[Test]
		public void ShouldObtainDefaultValueForParameterInformation()
		{
			var parameterInfo = typeof (ClassWithOptionalParameters)
				.GetMethods()
				.Where(m => m.Name == "Method")
				.Single()
				.GetParameters()
				.First();
			Assert.That(parameterInfo.IsOptional, Is.True);
			Assert.That(parameterInfo.DefaultValue, Is.EqualTo("test"));
		}

		public class ClassWithOptionalParameters
		{
			public void Method(string value = "test")
			{
			}

			public void MethodWithDifferentDefaultValues(
				bool? test = null,
				DateTime? date = null,
				long l = long.MaxValue,
				object o = null,
				ClassWithOptionalParameters c = null)
			{
			}
		}

		[Test]
		public void ShouldObtainInformationWhetherTheTypeIsPrimitive()
		{
			Assert.That(typeof (int).IsPrimitive, Is.True);
			Assert.That(typeof (double).IsPrimitive, Is.True);
			Assert.That(typeof (string).IsPrimitive, Is.False);
			Assert.That(typeof (char).IsPrimitive, Is.True);
			Assert.That(typeof (decimal).IsPrimitive, Is.False);

			Assert.That(typeof (SomeEnum).IsPrimitive, Is.False);
			Assert.That(typeof (DateTime).IsPrimitive, Is.False);
			Assert.That(typeof (Enum).IsPrimitive, Is.False);
			Assert.That(typeof (ValueType).IsPrimitive, Is.False);
			Assert.That(typeof (int[]).IsPrimitive, Is.False);
			Assert.That(typeof (SomeStruct).IsPrimitive, Is.False);
		}

		public enum SomeEnum
		{
			FirstItem,
			SecondItem
		}

		public struct SomeStruct
		{
		}

		[Test]
		public void ShouldObtainInformationAboutNullableUnderlyingType()
		{
			var nullableType = typeof (int?);
			Assert.That(nullableType.IsGenericType, Is.True);
			Assert.That(nullableType.GetGenericTypeDefinition(), Is.EqualTo(typeof (Nullable<>)));
			Assert.That(Nullable.GetUnderlyingType(nullableType), Is.EqualTo(typeof (int)));
		}

		[Test]
		public void ShouldObtainInfromationAboutArrayElementsType()
		{
			var arrayType = typeof (int[]);
			Assert.That(arrayType.IsArray, Is.True);
			Assert.That(arrayType.GetElementType(), Is.EqualTo(typeof (int)));
		}

		[Test]
		public void ShouldObtainInformationAboutEnumItems()
		{
			var enumType = typeof (SomeEnum);
			Assert.That(enumType.IsEnum, Is.True);
			Assert.That(enumType.GetEnumNames().First(), Is.EqualTo("FirstItem"));
			Assert.That(Enum.Parse(enumType, "FirstItem"), Is.EqualTo(SomeEnum.FirstItem));
		}

		[Test]
		public void ShouldConvertValuesFromStringUsingTypeDescriptor()
		{
			var converter = TypeDescriptor.GetConverter(typeof (int));
			Assert.That(converter.ConvertFrom("10"), Is.EqualTo(10));
		}

		[Test]
		public void ShouldConvertFromEnum()
		{
			var converter = TypeDescriptor.GetConverter(typeof(SomeEnum));
			Assert.That(converter.ConvertFrom("FirstItem"), Is.EqualTo(SomeEnum.FirstItem));
		}
}
}