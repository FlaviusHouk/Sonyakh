namespace Sonyakh.Lexing;

// Add Location type to able to later report problem for specific place in the input?
public record BasicLexem(LexemType Type) : ILexem;
public sealed record ValueLexem<T>(LexemType Type, T Value) : BasicLexem(Type);
