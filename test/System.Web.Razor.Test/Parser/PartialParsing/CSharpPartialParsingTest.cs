// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;

namespace System.Web.Razor.Test.Parser.PartialParsing
{
    public class CSharpPartialParsingTest : PartialParsingTestBase<CSharpRazorCodeLanguage>
    {
        [Fact]
        public void ImplicitExpressionAcceptsInnerInsertionsInStatementBlock()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @DateTime..Now" + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @DateTime.Now" + Environment.NewLine
                                                      + "}");

            // Act and Assert
            RunPartialParseTest(new TextChange(17, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("DateTime..Now")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsInnerInsertions()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime..Now baz");
            StringTextBuffer old = new StringTextBuffer("foo @DateTime.Now baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(13, 0, old, 1, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime..Now").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")), additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsWholeIdentifierReplacement()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @date baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(5, 4, old, 8, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionRejectsWholeIdentifierReplacementToKeyword()
        {
            // Arrange
            RazorEngineHost host = CreateHost();
            RazorEditorParser parser = new RazorEditorParser(host, @"C:\This\Is\A\Test\Path");

            using (TestParserManager manager = new TestParserManager(parser))
            {
                StringTextBuffer old = new StringTextBuffer("foo @date baz");
                StringTextBuffer changed = new StringTextBuffer("foo @if baz");
                TextChange textChange = new TextChange(5, 4, old, 2, changed);
                manager.InitializeWithDocument(old);

                // Act
                PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                // Assert
                Assert.Equal(PartialParseResult.Rejected, result);
                Assert.Equal(2, manager.ParseCount);
            }
        }

        [Fact]
        public void ImplicitExpressionRejectsWholeIdentifierReplacementToDirective()
        {
            // Arrange
            RazorEngineHost host = CreateHost();
            RazorEditorParser parser = new RazorEditorParser(host, @"C:\This\Is\A\Test\Path");

            using (var manager = new TestParserManager(parser))
            {
                StringTextBuffer old = new StringTextBuffer("foo @date baz");
                StringTextBuffer changed = new StringTextBuffer("foo @inherits baz");
                TextChange textChange = new TextChange(5, 4, old, 8, changed);
                manager.InitializeWithDocument(old);

                // Act
                PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                // Assert
                Assert.Equal(PartialParseResult.Rejected | PartialParseResult.SpanContextChanged, result);
                Assert.Equal(2, manager.ParseCount);
            }
        }

        [Fact]
        public void ImplicitExpressionAcceptsPrefixIdentifierReplacements_SingleSymbol()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @dTime baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(5, 1, old, 4, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsPrefixIdentifierReplacements_MultipleSymbols()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @dTime.Now baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime.Now baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(5, 1, old, 4, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime.Now").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsSuffixIdentifierReplacements_SingleSymbol()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @Datet baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(9, 1, old, 4, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsSuffixIdentifierReplacements_MultipleSymbols()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @DateTime.n baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime.Now baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(14, 1, old, 3, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime.Now").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsSurroundedIdentifierReplacements()
        {
            // Arrange
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @DateTime.n.ToString() baz");
            StringTextBuffer changed = new StringTextBuffer("foo @DateTime.Now.ToString() baz");

            // Act and Assert
            RunPartialParseTest(new TextChange(14, 1, old, 3, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("DateTime.Now.ToString()").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotlessCommitInsertionsInStatementBlockAfterIdentifiers()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @DateTime." + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @DateTime" + Environment.NewLine
                                                      + "}");

            var textChange = new TextChange(17, 0, old, 1, changed);
            using (TestParserManager manager = CreateParserManager())
            {
                Action<TextChange, PartialParseResult, string> applyAndVerifyPartialChange = (changeToApply, expectedResult, expectedCode) =>
                {
                    PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                    // Assert
                    Assert.Equal(expectedResult, result);
                    Assert.Equal(1, manager.ParseCount);
                    ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree, new MarkupBlock(
                        factory.EmptyHtml(),
                        new StatementBlock(
                            factory.CodeTransition(),
                            factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                            factory.Code("\r\n    ").AsStatement(),
                            new ExpressionBlock(
                                factory.CodeTransition(),
                                factory.Code(expectedCode)
                                       .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                       .Accepts(AcceptedCharacters.NonWhiteSpace)),
                            factory.Code("\r\n").AsStatement(),
                            factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                        factory.EmptyHtml()));
                };

                manager.InitializeWithDocument(textChange.OldBuffer);

                // This is the process of a dotless commit when doing "." insertions to commit intellisense changes.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateTime.");

                old = changed;
                changed = new StringTextBuffer("@{" + Environment.NewLine
                                            + "    @DateTime.." + Environment.NewLine
                                            + "}");
                textChange = new TextChange(18, 0, old, 1, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateTime..");

                old = changed;
                changed = new StringTextBuffer("@{" + Environment.NewLine
                                            + "    @DateTime.Now." + Environment.NewLine
                                            + "}");
                textChange = new TextChange(18, 0, old, 3, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateTime.Now.");
            }
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotlessCommitInsertionsInStatementBlock()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @DateT." + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @DateT" + Environment.NewLine
                                                      + "}");

            var textChange = new TextChange(14, 0, old, 1, changed);
            using (TestParserManager manager = CreateParserManager())
            {
                Action<TextChange, PartialParseResult, string> applyAndVerifyPartialChange = (changeToApply, expectedResult, expectedCode) =>
                {
                    PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                    // Assert
                    Assert.Equal(expectedResult, result);
                    Assert.Equal(1, manager.ParseCount);
                    ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree, new MarkupBlock(
                        factory.EmptyHtml(),
                        new StatementBlock(
                            factory.CodeTransition(),
                            factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                            factory.Code("\r\n    ").AsStatement(),
                            new ExpressionBlock(
                                factory.CodeTransition(),
                                factory.Code(expectedCode)
                                       .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                       .Accepts(AcceptedCharacters.NonWhiteSpace)),
                            factory.Code("\r\n").AsStatement(),
                            factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                        factory.EmptyHtml()));
                };

                manager.InitializeWithDocument(textChange.OldBuffer);

                // This is the process of a dotless commit when doing "." insertions to commit intellisense changes.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateT.");

                old = changed;
                changed = new StringTextBuffer("@{" + Environment.NewLine
                                            + "    @DateTime." + Environment.NewLine
                                            + "}");
                textChange = new TextChange(14, 0, old, 3, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateTime.");
            }
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDotlessCommitInsertions()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            var changed = new StringTextBuffer("foo @DateT. baz");
            var old = new StringTextBuffer("foo @DateT baz");
            var textChange = new TextChange(10, 0, old, 1, changed);
            using (TestParserManager manager = CreateParserManager())
            {
                Action<TextChange, PartialParseResult, string> applyAndVerifyPartialChange = (changeToApply, expectedResult, expectedCode) =>
                {
                    PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                    // Assert
                    Assert.Equal(expectedResult, result);
                    Assert.Equal(1, manager.ParseCount);

                    ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree, new MarkupBlock(
                        factory.Markup("foo "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code(expectedCode).AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Markup(" baz")));
                };

                manager.InitializeWithDocument(textChange.OldBuffer);

                // This is the process of a dotless commit when doing "." insertions to commit intellisense changes.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateT.");

                old = changed;
                changed = new StringTextBuffer("foo @DateTime. baz");
                textChange = new TextChange(10, 0, old, 3, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateTime.");
            }
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDotlessCommitInsertionsAfterIdentifiers()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            var changed = new StringTextBuffer("foo @DateTime. baz");
            var old = new StringTextBuffer("foo @DateTime baz");
            var textChange = new TextChange(13, 0, old, 1, changed);
            using (TestParserManager manager = CreateParserManager())
            {
                Action<TextChange, PartialParseResult, string> applyAndVerifyPartialChange = (changeToApply, expectedResult, expectedCode) =>
                {
                    PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                    // Assert
                    Assert.Equal(expectedResult, result);
                    Assert.Equal(1, manager.ParseCount);

                    ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree, new MarkupBlock(
                        factory.Markup("foo "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code(expectedCode).AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Markup(" baz")));
                };

                manager.InitializeWithDocument(textChange.OldBuffer);

                // This is the process of a dotless commit when doing "." insertions to commit intellisense changes.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateTime.");

                old = changed;
                changed = new StringTextBuffer("foo @DateTime.. baz");
                textChange = new TextChange(14, 0, old, 1, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateTime..");

                old = changed;
                changed = new StringTextBuffer("foo @DateTime.Now. baz");
                textChange = new TextChange(14, 0, old, 3, changed);

                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateTime.Now.");
            }
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsCaseInsensitiveDotlessCommitInsertions_NewRoslynIntegration()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer old = new StringTextBuffer("foo @date baz");
            StringTextBuffer changed = new StringTextBuffer("foo @date. baz");
            TextChange textChange = new TextChange(9, 0, old, 1, changed);
            using (TestParserManager manager = CreateParserManager())
            {
                Action<TextChange, PartialParseResult, string> applyAndVerifyPartialChange = (changeToApply, expectedResult, expectedCode) =>
                {
                    PartialParseResult result = manager.CheckForStructureChangesAndWait(textChange);

                    // Assert
                    Assert.Equal(expectedResult, result);
                    Assert.Equal(1, manager.ParseCount);

                    ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree, new MarkupBlock(
                        factory.Markup("foo "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code(expectedCode).AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Markup(" baz")));
                };

                manager.InitializeWithDocument(textChange.OldBuffer);

                // This is the process of a dotless commit when doing "." insertions to commit intellisense changes.

                // @date => @date.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "date.");

                old = changed;
                changed = new StringTextBuffer("foo @date baz");
                textChange = new TextChange(9, 1, old, 0, changed);

                // @date. => @date
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "date");

                old = changed;
                changed = new StringTextBuffer("foo @DateTime baz");
                textChange = new TextChange(5, 4, old, 8, changed);

                // @date => @DateTime
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted, "DateTime");

                old = changed;
                changed = new StringTextBuffer("foo @DateTime. baz");
                textChange = new TextChange(13, 0, old, 1, changed);

                // @DateTime => @DateTime.
                applyAndVerifyPartialChange(textChange, PartialParseResult.Accepted | PartialParseResult.Provisional, "DateTime.");
            }
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDeleteOfIdentifierPartsIfDotRemains()
        {
            SpanFactory factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @User.Name baz");
            RunPartialParseTest(new TextChange(10, 4, old, 0, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("User.").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsDeleteOfIdentifierPartsIfSomeOfIdentifierRemains()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @Us baz");
            StringTextBuffer old = new StringTextBuffer("foo @User baz");
            RunPartialParseTest(new TextChange(7, 2, old, 0, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("Us").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsMultipleInsertionIfItCausesIdentifierExpansionAndTrailingDot()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @U baz");
            RunPartialParseTest(new TextChange(6, 0, old, 4, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("User.").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsMultipleInsertionIfItOnlyCausesIdentifierExpansion()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @barbiz baz");
            StringTextBuffer old = new StringTextBuffer("foo @bar baz");
            RunPartialParseTest(new TextChange(8, 0, old, 3, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("barbiz").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierExpansionAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @food" + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @foo" + Environment.NewLine
                                                      + "}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("food")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierAfterDotAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @foo.d" + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @foo." + Environment.NewLine
                                                      + "}");
            RunPartialParseTest(new TextChange(13, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.d")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{" + Environment.NewLine
                                                          + "    @foo." + Environment.NewLine
                                                          + "}");
            StringTextBuffer old = new StringTextBuffer("@{" + Environment.NewLine
                                                      + "    @foo" + Environment.NewLine
                                                      + "}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code(@"foo.")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionRejectsChangeWhichWouldHaveBeenAcceptedIfLastChangeWasProvisionallyAcceptedOnDifferentSpan()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo @bar"), 1, new StringTextBuffer("foo @foo. @bar"));
            TextChange charTyped = new TextChange(14, 0, new StringTextBuffer("foo @foo. @bar"), 1, new StringTextBuffer("foo @foo. @barb"));
            using (TestParserManager manager = CreateParserManager())
            {
                manager.InitializeWithDocument(dotTyped.OldBuffer);

                // Apply the dot change
                Assert.Equal(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

                // Act (apply the identifier start char change)
                PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

                // Assert
                Assert.Equal(PartialParseResult.Rejected, result);
                Assert.False(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
                ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                    new MarkupBlock(
                        factory.Markup("foo "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Markup(". "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("barb")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.EmptyHtml()));
            }
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierTypedAfterDotIfLastChangeWasProvisionalAcceptanceOfDot()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo bar"), 1, new StringTextBuffer("foo @foo. bar"));
            TextChange charTyped = new TextChange(9, 0, new StringTextBuffer("foo @foo. bar"), 1, new StringTextBuffer("foo @foo.b bar"));
            using (TestParserManager manager = CreateParserManager())
            {
                manager.InitializeWithDocument(dotTyped.OldBuffer);

                // Apply the dot change
                Assert.Equal(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

                // Act (apply the identifier start char change)
                PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

                // Assert
                Assert.Equal(PartialParseResult.Accepted, result);
                Assert.False(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
                ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                    new MarkupBlock(
                        factory.Markup("foo "),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.b")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Markup(" bar")));
            }
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDotAfterIdentifierInMarkup()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @foo. bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foo.")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" bar")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsAdditionalIdentifierCharactersIfEndOfSpanIsIdentifier()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @foob bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foob")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" bar")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsAdditionalIdentifierStartCharactersIfEndOfSpanIsDot()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{@foo.b}");
            StringTextBuffer old = new StringTextBuffer("@{@foo.}");
            RunPartialParseTest(new TextChange(7, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.EmptyCSharp().AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.b")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.EmptyCSharp().AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotIfTrailingDotsAreAllowed()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{@foo.}");
            StringTextBuffer old = new StringTextBuffer("@{@foo}");
            RunPartialParseTest(new TextChange(6, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.EmptyCSharp().AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.EmptyCSharp().AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfIfKeywordTyped()
        {
            RunTypeKeywordTest("if");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfDoKeywordTyped()
        {
            RunTypeKeywordTest("do");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfTryKeywordTyped()
        {
            RunTypeKeywordTest("try");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForKeywordTyped()
        {
            RunTypeKeywordTest("for");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForEachKeywordTyped()
        {
            RunTypeKeywordTest("foreach");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfWhileKeywordTyped()
        {
            RunTypeKeywordTest("while");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSwitchKeywordTyped()
        {
            RunTypeKeywordTest("switch");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLockKeywordTyped()
        {
            RunTypeKeywordTest("lock");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfUsingKeywordTyped()
        {
            RunTypeKeywordTest("using");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSectionKeywordTyped()
        {
            RunTypeKeywordTest("section");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfInheritsKeywordTyped()
        {
            RunTypeKeywordTest("inherits");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfHelperKeywordTyped()
        {
            RunTypeKeywordTest("helper");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfFunctionsKeywordTyped()
        {
            RunTypeKeywordTest("functions");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfNamespaceKeywordTyped()
        {
            RunTypeKeywordTest("namespace");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfClassKeywordTyped()
        {
            RunTypeKeywordTest("class");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLayoutKeywordTyped()
        {
            RunTypeKeywordTest("layout");
        }
    }
}
