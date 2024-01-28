using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Sonyakh.Lexing;
using Sonyakh.Reporting;

namespace Sonyakh.Tests;

public class Tests
{
    private static IEnumerable<TestCaseData> Lexer_CanLex_SimpleLexems_TestCases
    {
        get
        {
            //Single char lexem
            yield return new TestCaseData(CreateInput("+"), 
                                          new ILexem[] 
                                          { 
                                            new BasicLexem(LexemType.Plus)
                                          });

            //Single char lexem, the one that might be used with multiple characters.
            yield return new TestCaseData(CreateInput("&"), 
                                          new ILexem[] 
                                          { 
                                            new BasicLexem(LexemType.Ampersand)
                                          });

            //Multichar token
            yield return new TestCaseData(CreateInput(">="), 
                                          new ILexem[] 
                                          { 
                                            new BasicLexem(LexemType.GreaterOrEqual)
                                          });

            //True
            yield return new TestCaseData(CreateInput("true"), 
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<bool>(LexemType.Logical, true)
                                          });

            //False
            yield return new TestCaseData(CreateInput("false"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<bool>(LexemType.Logical, false)
                                          });

            //Char
            yield return new TestCaseData(CreateInput("'a'"), 
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<char>(LexemType.Char, 'a')
                                          });

            //Int
            yield return new TestCaseData(CreateInput("123"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<int>(LexemType.Integer, 123)
                                          });

            //Double
            yield return new TestCaseData(CreateInput("123.321"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<double>(LexemType.Float, 123.321)
                                          });

            //String
            yield return new TestCaseData(CreateInput("\"abs\""),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<string>(LexemType.String, "abs")
                                          });

            //id
            yield return new TestCaseData(CreateInput("myVal"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<string>(LexemType.Id, "myVal")
                                          });

            //Multiple tokens, no spaces
            yield return new TestCaseData(CreateInput("2-1"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<int>(LexemType.Integer, 2),
                                            new BasicLexem(LexemType.Minus),
                                            new ValueLexem<int>(LexemType.Integer, 1),
                                          });

            //Multiple tokens, with spaces
            yield return new TestCaseData(CreateInput("6+(1.00 - 2.3 * 3)"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<int>(LexemType.Integer, 6),
                                            new BasicLexem(LexemType.Plus),
                                            new BasicLexem(LexemType.LeftRoundPar),
                                            new ValueLexem<double>(LexemType.Float, 1.0),
                                            new BasicLexem(LexemType.Minus),
                                            new ValueLexem<double>(LexemType.Float, 2.3),
                                            new BasicLexem(LexemType.Star),
                                            new ValueLexem<int>(LexemType.Integer, 3),
                                            new BasicLexem(LexemType.RightRoundPar),
                                          });

            //Multiple tokens, with spaces
            yield return new TestCaseData(CreateInput("false || true && 1 == 2"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<bool>(LexemType.Logical, false),
                                            new BasicLexem(LexemType.LogicalOr),
                                            new ValueLexem<bool>(LexemType.Logical, true),
                                            new BasicLexem(LexemType.LogicalAnd),
                                            new ValueLexem<int>(LexemType.Integer, 1),
                                            new BasicLexem(LexemType.Equal),
                                            new ValueLexem<int>(LexemType.Integer, 2)
                                          });

            //Multiple keywords, with spaces
            yield return new TestCaseData(CreateInput("public static class Program"),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Public),
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Static),
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Class),
                                            new ValueLexem<string>(LexemType.Id, "Program"),
                                          });

            string input = @"private double CalculateAvg(int a, int b)
            {
                var sum = a + b;
                return sum / 2;
            }";

            //Multiline input, simple function
            yield return new TestCaseData(CreateInput(input),
                                          new ILexem[] 
                                          { 
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Private),
                                            new ValueLexem<string>(LexemType.Id, "double"),
                                            new ValueLexem<string>(LexemType.Id, "CalculateAvg"),
                                            new BasicLexem(LexemType.LeftRoundPar),
                                            new ValueLexem<string>(LexemType.Id, "int"),
                                            new ValueLexem<string>(LexemType.Id, "a"),
                                            new BasicLexem(LexemType.Comma),
                                            new ValueLexem<string>(LexemType.Id, "int"),
                                            new ValueLexem<string>(LexemType.Id, "b"),
                                            new BasicLexem(LexemType.RightRoundPar),
                                            new BasicLexem(LexemType.LeftCurvyPar),
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Var),
                                            new ValueLexem<string>(LexemType.Id, "sum"),
                                            new BasicLexem(LexemType.Assign),
                                            new ValueLexem<string>(LexemType.Id, "a"),
                                            new BasicLexem(LexemType.Plus),
                                            new ValueLexem<string>(LexemType.Id, "b"),
                                            new BasicLexem(LexemType.Semicolumn),
                                            new ValueLexem<Keyword>(LexemType.Keyword, Keyword.Return),
                                            new ValueLexem<string>(LexemType.Id, "sum"),
                                            new BasicLexem(LexemType.Slash),
                                            new ValueLexem<int>(LexemType.Integer, 2),
                                            new BasicLexem(LexemType.Semicolumn),
                                            new BasicLexem(LexemType.RightCurvyPar),
                                          });
        }
    }

    [Test]
    [TestCaseSource(nameof(Lexer_CanLex_SimpleLexems_TestCases))]
    public void Lexer_CanLex_PositiveCases(Stream input, IReadOnlyCollection<ILexem> expected)
    {
        List<ReportItem> reports = [];
        Lexer lexer = new(input, reports, "Lexer_CanLex_SimpleLexems");
        
        List<ILexem> produced = [];
        while(lexer.Advance())
        {
            produced.Add(lexer.Current);
        }

        Assert.That(lexer.Current.Type, Is.EqualTo(LexemType.EOF));
        Assert.That(produced, Is.EqualTo(expected));
        Assert.That(reports.IsFailed(), Is.False);
        
        input.Dispose();
    }

    private static MemoryStream CreateInput(string text)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream, leaveOpen: true);

        writer.Write(text);

        writer.Dispose();
        stream.Seek(0, SeekOrigin.Begin);
        
        return stream;
    }
}