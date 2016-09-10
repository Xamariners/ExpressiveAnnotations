using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using ExpressiveAnnotations.Analysis;
using Moq;
using Xunit;

namespace ExpressiveAnnotations.Tests
{
    using ExpressiveAnnotations.Infrastructure;

    public class UtilsTest
    {
       

      

        [Fact]
        public void verify_fields_values_extraction_from_given_instance()
        {
            var model = new Model
            {
                Value1 = 1,
                Value2 = 2,
                Internal = new Model
                {
                    Value1 = 11,
                    Value2 = null
                }
            };

            Assert.Equal(1, ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "Value1"));
            Assert.Equal(11, ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "Internal.Value1"));
            Assert.Equal(null, ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "Internal.Value2"));

            var e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "internal"));
            Assert.Equal("Value extraction interrupted. Field internal not found.\r\nParameter name: internal", e.Message);

            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "Internal.Value123"));
            Assert.Equal("Value extraction interrupted. Field Value123 not found.\r\nParameter name: Internal.Value123", e.Message);

            model.Internal = null;
            e = Assert.Throws<ArgumentException>(() => ExpressiveAnnotations.Infrastructure.Helper.ExtractValue(model, "Internal.Value1"));
            Assert.Equal("Value extraction interrupted. Field Internal is null.\r\nParameter name: Internal.Value1", e.Message);
        }
        
       

        [Fact]
        public void type_load_exceptions_are_handled_and_null_type_instances_are_filtered_out()
        {
            var typeProviderMock = new Mock<ITypeProvider>();

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new Type[] {null}, null));
            Assert.Empty(typeProviderMock.Object.GetLoadableTypes());

            typeProviderMock.Setup(p => p.GetTypes()).Throws(new ReflectionTypeLoadException(new[] {typeof (object), null}, null));
            Assert.Equal(1, typeProviderMock.Object.GetLoadableTypes().Count());
        }

        public static IEnumerable<object[]> ErrorData => new[]
        {            
            new object[] {new Location(1,1), "\r", @"
" },
            new object[] {new Location(1,1), "\r", "\r\n" },
            new object[] {new Location(1,1), "abcde", "abcde" },
            new object[] {new Location(1,3), "cde", "abcde" },
            new object[] {new Location(1,5), "e", "abcde" },
            new object[] {new Location(2,1), "abcde", "12345\r\nabcde" },
            new object[] {new Location(2,3), "cde", "12345\r\nabcde" },
            new object[] {new Location(2,5), "e", "12345\r\nabcde" },
            new object[] {new Location(1,6), "  \r", "abcde  \r\nabcde" },
            new object[] {new Location(1,7), " \r", "abcde  \r\nabcde" },
            new object[] {new Location(1,8), "\r", "abcde  \r\nabcde" },
            new object[] {new Location(1,1), new string('a', 100), new string('a', 100) },
            new object[] {new Location(1,1), new string('a', 100), new string('a', 101) } // max 100 chars of expression is displayed
        };

        [Theory]
        [MemberData("ErrorData")]
        public void verify_error_message_construction(Location location, string indication, string expression)
        {
            Assert.Equal(
                $@"Parse error on line {location.Line}, column {location.Column}:
... {indication} ...
    ^--- error message",
                location.BuildParseError("error message", expression));
        }

        public static IEnumerable<object[]> BoundaryErrorData => new[]
        {
            new object[] {new Location(1,6), "abcde" },
            new object[] {new Location(2,6), "12345\r\nabcde" }
        };

        [Theory]
        [MemberData("BoundaryErrorData")]
        public void verify_error_message_construction_for_boundary_conditions(Location location, string expression)
        {
            Assert.Equal(
                $@"Parse error on line {location.Line}, last column: error message",
                location.BuildParseError("error message", expression));
        }

        [Fact]
        public void throw_when_non_positive_parameters_are_provided_for_error_message_construction()
        {
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => new Location(0, 1));
            Assert.Equal("Line number should be positive.\r\nParameter name: line", e.Message);
            e = Assert.Throws<ArgumentOutOfRangeException>(() => new Location(1, 0));
            Assert.Equal("Column number should be positive.\r\nParameter name: column", e.Message);
        }

        [Fact]
        public void print_token_for_debug_purposes()
        {
            var token = new Token(TokenType.FLOAT, 1.0, "1.0", new Location(1, 2));
            Assert.Equal(@"""1.0"" FLOAT (1, 2)", token.ToString());
        }

        private class Model
        {
            [Display(Name = "Value_1")]
            public int? Value1 { get; set; }

            [Display(ResourceType = typeof (Resources), Name = "Value2")]
            public int? Value2 { get; set; }

            public string NoName { get; set; }

            [DisplayName("internal")]
            public Model Internal { get; set; }
        }
    }
}
