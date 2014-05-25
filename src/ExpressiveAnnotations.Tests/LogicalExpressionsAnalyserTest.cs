﻿using System;
using ExpressiveAnnotations.LogicalExpressionsAnalysis;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.LexicalAnalysis;
using ExpressiveAnnotations.LogicalExpressionsAnalysis.SyntacticAnalysis;
using ExpressiveAnnotations.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ExpressiveAnnotations.Tests
{
    [TestClass]
    public class LogicalExpressionsAnalyserTest
    {
        [TestMethod]
        public void Verify_infix_lexer_logic()
        {
            const string expression = "( true && (true) ) || false";
            var lexer = new InfixLexer();

            var tokens = lexer.Analyze(expression, false);
            Assert.AreEqual(tokens.Length, 15);
            Assert.AreEqual(tokens[0], "(");
            Assert.AreEqual(tokens[1], " ");
            Assert.AreEqual(tokens[2], "true");
            Assert.AreEqual(tokens[3], " ");
            Assert.AreEqual(tokens[4], "&&");
            Assert.AreEqual(tokens[5], " ");
            Assert.AreEqual(tokens[6], "(");
            Assert.AreEqual(tokens[7], "true");
            Assert.AreEqual(tokens[8], ")");
            Assert.AreEqual(tokens[9], " ");
            Assert.AreEqual(tokens[10], ")");
            Assert.AreEqual(tokens[11], " ");
            Assert.AreEqual(tokens[12], "||");
            Assert.AreEqual(tokens[13], " ");
            Assert.AreEqual(tokens[14], "false");

            tokens = lexer.Analyze(expression, true);
            Assert.AreEqual(tokens.Length, 9);
            Assert.AreEqual(tokens[0], "(");
            Assert.AreEqual(tokens[1], "true");
            Assert.AreEqual(tokens[2], "&&");
            Assert.AreEqual(tokens[3], "(");
            Assert.AreEqual(tokens[4], "true");
            Assert.AreEqual(tokens[5], ")");
            Assert.AreEqual(tokens[6], ")");
            Assert.AreEqual(tokens[7], "||");
            Assert.AreEqual(tokens[8], "false");            

            try
            {
                lexer.Analyze("true + false", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at \"+ false\".");
            }

            try
            {
                lexer.Analyze("true && 7", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at \"7\".");
            }
        }

        [TestMethod]
        public void Verify_postfix_lexer_logic()
        {
            const string expression = "true true && false ||";
            var lexer = new PostfixLexer();

            var tokens = lexer.Analyze(expression, false);
            Assert.AreEqual(tokens.Length, 9);
            Assert.AreEqual(tokens[0], "true");
            Assert.AreEqual(tokens[1], " ");
            Assert.AreEqual(tokens[2], "true");
            Assert.AreEqual(tokens[3], " ");
            Assert.AreEqual(tokens[4], "&&");
            Assert.AreEqual(tokens[5], " ");
            Assert.AreEqual(tokens[6], "false");
            Assert.AreEqual(tokens[7], " ");
            Assert.AreEqual(tokens[8], "||");

            tokens = lexer.Analyze(expression, true);
            Assert.AreEqual(tokens.Length, 5);
            Assert.AreEqual(tokens[0], "true");
            Assert.AreEqual(tokens[1], "true");
            Assert.AreEqual(tokens[2], "&&");
            Assert.AreEqual(tokens[3], "false");
            Assert.AreEqual(tokens[4], "||");

            try
            {
                lexer.Analyze("true && (false)", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at \"(false)\".");
            }

            try
            {
                lexer.Analyze("true + 7", false);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at \"+ 7\".");
            }
        }

        [TestMethod]
        public void Verify_infix_to_postfix_conversion()
        {
            var converter = new InfixToPostfixConverter();
            Assert.AreEqual(converter.Convert("()"), "");
            Assert.AreEqual(converter.Convert("( true && (true) ) || false"), "true true && false ||");
            Assert.AreEqual(converter.Convert(
                "(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"),
                "true true false true || || || true true false false true true true false || && && || || && && false && ||");
            Assert.AreEqual(converter.Convert("!!((!(!!true))) && true"), "true ! ! ! ! ! true &&");

            try
            {
                converter.Convert("(");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert(")");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert("(( true )");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }

            try
            {
                converter.Convert("( true && false ))");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Infix expression parsing error. Incorrect nesting.");
            }
        }

        [TestMethod]
        public void Verify_postfix_parser()
        {
            var parser = new PostfixParser();

            Assert.IsTrue(parser.Evaluate("true"));
            Assert.IsTrue(!parser.Evaluate("false"));

            Assert.IsTrue(parser.Evaluate("true true &&"));
            Assert.IsTrue(!parser.Evaluate("true false &&"));
            Assert.IsTrue(!parser.Evaluate("false true &&"));
            Assert.IsTrue(!parser.Evaluate("false false &&"));

            Assert.IsTrue(parser.Evaluate("true true ||"));
            Assert.IsTrue(parser.Evaluate("true false ||"));
            Assert.IsTrue(parser.Evaluate("false true ||"));
            Assert.IsTrue(!parser.Evaluate("false false ||"));

            Assert.IsTrue(parser.Evaluate("true true false true || || || true true false false true true true false || && && || || && && false && ||"));

            try
            {
                parser.Evaluate("(true)");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                Assert.IsTrue(e.Message == "Lexer error. Unexpected token started at \"(true)\".");
            }
        }

        [TestMethod]
        public void Verify_complex_expression_evaluation()
        {
            var evaluator = new Evaluator();
            Assert.IsTrue(evaluator.Compute("(true || ((true || (false || true)))) || (true && true && false || (false || true && (true && true || ((false))))) && false"));
            Assert.IsTrue(evaluator.Compute("( !!((!(!!!true || !!false || !true))) && true && !(true && false) ) && (!((!(!true))) || !!!(((!true))))"));

            try
            {
                evaluator.Compute("");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Logical expression computation failed. Expression is broken.");
            }
            try
            {
                evaluator.Compute(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Logical expression computation failed. Expression is broken.");
            }
        }

        [TestMethod]
        public void Verify_comparison_equals_non_empty()
        {
            Assert.IsTrue(Comparer.Compute("aAa", "aAa", "=="));
            Assert.IsTrue(Comparer.Compute(0, 0, "=="));
            Assert.IsTrue(Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), "=="));
            Assert.IsTrue(Comparer.Compute(new {}, new {}, "=="));
            Assert.IsTrue(Comparer.Compute(new {error = true}, new {error = true}, "=="));
            Assert.IsTrue(Comparer.Compute(new[] {"a", "b"}, new[] {"a", "b"}, "=="));

            Assert.IsTrue(!Comparer.Compute("aAa", "aAa ", "=="));
            Assert.IsTrue(!Comparer.Compute("aAa", " aAa ", "=="));
            Assert.IsTrue(!Comparer.Compute("aAa", "aaa", "=="));
            Assert.IsTrue(!Comparer.Compute(0, 1, "=="));
            Assert.IsTrue(!Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), "=="));
            Assert.IsTrue(!Comparer.Compute(new {error = true}, new {error = false}, "=="));
            Assert.IsTrue(!Comparer.Compute(new[] {"a", "b"}, new[] {"a", "B"}, "=="));
        }

        [TestMethod]
        public void Verify_comparison_equals_empty()
        {
            Assert.IsTrue(Comparer.Compute("", "", "=="));
            Assert.IsTrue(Comparer.Compute(" ", " ", "=="));
            Assert.IsTrue(Comparer.Compute("\t", "\n", "=="));
            Assert.IsTrue(Comparer.Compute(null, null, "=="));
            Assert.IsTrue(Comparer.Compute("", " ", "=="));
            Assert.IsTrue(Comparer.Compute("\n\t ", null, "=="));
        }

        [TestMethod]
        public void Verify_comparison_greater_and_less()
        {
            // assumption - arguments provided have exact types

            Assert.IsTrue(Comparer.Compute("a", "A", ">"));
            Assert.IsTrue(Comparer.Compute("abcd", "ABCD", ">"));
            Assert.IsTrue(Comparer.Compute(1, 0, ">"));
            Assert.IsTrue(Comparer.Compute(0, -1, ">"));
            Assert.IsTrue(Comparer.Compute(1.1, 1.01, ">"));
            Assert.IsTrue(Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), ">"));

            Assert.IsTrue(!Comparer.Compute("a", "A", "<="));
            Assert.IsTrue(!Comparer.Compute("abcd", "ABCD", "<="));
            Assert.IsTrue(!Comparer.Compute(1, 0, "<="));
            Assert.IsTrue(!Comparer.Compute(0, -1, "<="));
            Assert.IsTrue(!Comparer.Compute(1.1, 1.01, "<="));
            Assert.IsTrue(!Comparer.Compute(DateTime.Parse("Wed, 09 Aug 1995 00:00:01 GMT"), DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT"), "<="));

            Assert.IsTrue(!Comparer.Compute("a", null, ">"));
            Assert.IsTrue(!Comparer.Compute(null, "a", ">"));
            Assert.IsTrue(!Comparer.Compute("a", null, "<"));
            Assert.IsTrue(!Comparer.Compute(null, "a", "<"));

            try
            {
                Comparer.Compute(new {}, new {}, ">");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message == "Greater than and less than relational operations not allowed for arguments of types other than: numeric, string or datetime.");
            }
        }

        [TestMethod]
        public void Verify_composed_error_message()
        {
            const string expression = "{0} && ( (!{1} && {2}) || ({3} && {4}) ) && {5} && {6} && {7} || {1}";
var dependentProperties = new[] {"aaa", "bbb",  "ccc",  "ddd",  "ddd",  "eee",  "fff",  "ggg"};
            var relationalOperators = new[] {"==",  "==",   "==",   ">",    "<=",   "!=",   "!=",   "=="};
            var targetValues = new object[] {true,  "xXx",  "[yYy]",-1,     1.2,    null,   "*",    ""};

            Assert.AreEqual(
                PropHelper.ComposeExpression(expression, dependentProperties, targetValues, relationalOperators),
                "(aaa == true) && ( (!(bbb == \"xXx\") && (ccc == [yYy])) || ((ddd > -1) && (ddd <= 1.2)) ) && (eee != null) && (fff != *) && (ggg == \"\") || (bbb == \"xXx\")");
        }

        [TestMethod]
        public void Verify_typehelper_is_empty()
        {
            object nullo = null;
            Assert.IsTrue(nullo.IsEmpty());
            Assert.IsTrue("".IsEmpty());
            Assert.IsTrue(" ".IsEmpty());
            Assert.IsTrue("\t".IsEmpty());
            Assert.IsTrue("\n".IsEmpty());
            Assert.IsTrue("\n\t ".IsEmpty());
        }

        [TestMethod]
        public void Verify_typehelper_is_numeric()
        {
            Assert.IsTrue(1.IsNumeric());
            Assert.IsTrue(!"1".IsNumeric());
        }

        [TestMethod]
        public void Verify_typehelper_is_date()
        {
            Assert.IsTrue(DateTime.Parse("Wed, 09 Aug 1995 00:00:00 GMT").IsDateTime());
            Assert.IsTrue(!"Wed, 09 Aug 1995 00:00:00 GMT".IsDateTime());
            Assert.IsTrue(!807926400000.IsDateTime());
        }

        [TestMethod]
        public void Verify_typehelper_is_string()
        {
            object nullo = null;
            Assert.IsTrue("".IsString());
            Assert.IsTrue("123".IsString());
            Assert.IsTrue(!123.IsString());
            Assert.IsTrue(!new {}.IsString());
            Assert.IsTrue(!nullo.IsString());
        }

        [TestMethod]
        public void Verify_typehelper_is_bool()
        {
            Assert.IsTrue(true.IsBool());
            Assert.IsTrue(!"true".IsBool());
            Assert.IsTrue(!0.IsBool());
        }
    }
}