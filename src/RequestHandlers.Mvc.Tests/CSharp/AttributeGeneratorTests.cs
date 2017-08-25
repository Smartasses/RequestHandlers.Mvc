using System;
using System.Linq;
using Xunit;
using RequestHandlers.Mvc.CSharp;

namespace RequestHandlers.Mvc.Tests.CSharp
{
    public class AttributeGeneratorTests
    {
        private AttributeGenerator _sut;

        public AttributeGeneratorTests()
        {
            _sut = new AttributeGenerator();
        }

        [TestNamedArguments] class AttributeHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithoutNamedArgument_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestNamedArgumentsAttribute]", stringAttribute);
        }

        [TestNamedArguments(StringProperty = "Yenthe")] class AttributeWithNamedArgumentHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithNamedArgument_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeWithNamedArgumentHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestNamedArgumentsAttribute(StringProperty = \"Yenthe\")]", stringAttribute);
        }

        [TestNamedArguments(StringProperty = "Yenthe", IntProperty = 5)] class AttributeWithNamedArgumentsHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithMultipleNamedArguments_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeWithNamedArgumentsHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestNamedArgumentsAttribute(StringProperty = \"Yenthe\", IntProperty = 5)]", stringAttribute);
        }

        [TestNamedArguments(EnumProperty = TestEnum.SecondValue)] class AttributeWithEnumNamedArgumentHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithEnumNamedArgument_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeWithEnumNamedArgumentHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestNamedArgumentsAttribute(EnumProperty = 1)]", stringAttribute);
        }

        [TestConstructorArguments("first", 2, TestEnum.ThirdValue)] class AttributeWithConstructorArgumentsHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithConstructorArguments_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeWithConstructorArgumentsHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestConstructorArgumentsAttribute(\"first\", 2, 2)]", stringAttribute);
        }

        [TestConstructorArguments("first", 2, TestEnum.ThirdValue, Property = "Yenthe")] class AttributeWithNamedAndConstructorArgumentsHost { }

        [Fact]
        public void Generate_GivenCustomAttributeData_WithNamedAndConstructorArguments_GenerateAttributeAsString()
        {
            var stringAttribute = _sut.Generate(typeof(AttributeWithNamedAndConstructorArgumentsHost).GetCustomAttributesData().First());
            Assert.Equal("[RequestHandlers.Mvc.Tests.CSharp.TestConstructorArgumentsAttribute(\"first\", 2, 2, Property = \"Yenthe\")]", stringAttribute);
        }
    }

    public enum TestEnum
    {
        FirstValue = 0,
        SecondValue,
        ThirdValue
    }

    public class TestNamedArgumentsAttribute : Attribute
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public TestEnum EnumProperty { get; set; }
    }

    public class TestConstructorArgumentsAttribute : Attribute
    {
        public string Property { get; set; }
        public TestConstructorArgumentsAttribute(string first, int second, TestEnum third) { }
    }
}
