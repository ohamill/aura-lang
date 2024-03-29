﻿using AuraLang.Shared;
using AuraLang.Token;
using AuraLang.Types;
using AuraLang.Visitor;
using Range = AuraLang.Location.Range;

namespace AuraLang.AST;

/// <summary>
///     Represents an untyped Aura Abstract Syntax Tree node
/// </summary>
public interface IUntypedAuraAstNode : IAuraAstNode
{
}

/// <summary>
///     Represents an untyped Aura expression
/// </summary>
public interface IUntypedAuraExpression : IUntypedAuraAstNode, IUntypedAuraExprVisitable
{
}

/// <summary>
///     Represents an untyped Aura statement
/// </summary>
public interface IUntypedAuraStatement : IUntypedAuraAstNode, IUntypedAuraStmtVisitable
{
}

/// <summary>
///     Represents an untyped callable AST node
/// </summary>
public interface IUntypedAuraCallable : IUntypedAuraAstNode
{
	string GetName();
}

/// <summary>
///     Represents an assignment expression in Aura, which assigns a value to a previously-declared
///     variable. An example in Aura might look like:
///     <code>x = 5</code>
/// </summary>
/// <param name="Name">The name of the variable being assigned a new value</param>
/// <param name="Value">The variable's new value</param>
public record UntypedAssignment(Tok Name, IUntypedAuraExpression Value) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Name.Range.Start, Value.Range.End);
}

/// <summary>
///     Represents an increment operation where the value of the variable is incremented by 1.
/// </summary>
/// <param name="Name">The variable being incremented</param>
/// <param name="PlusPlus">
///     A token representing the <c>++</c> suffix operator, which determines the node's ending position
///     in the Aura source file
/// </param>
public record UntypedPlusPlusIncrement(IUntypedAuraExpression Name, Tok PlusPlus) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Name.Range.Start, PlusPlus.Range.End);
}

/// <summary>
///     Represents a decrement operation where the value of the variable is decremented by 1.
/// </summary>
/// <param name="Name">The variable being decremented</param>
/// <param name="MinusMinus">
///     A token representing the <c>--</c> suffix operator, which determines the node's ending
///     position in the Aura source file
/// </param>
public record UntypedMinusMinusDecrement(IUntypedAuraExpression Name, Tok MinusMinus) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Name.Range.Start, MinusMinus.Range.End);
}

/// <summary>
///     Represents a binary expression containing a left and right operand and an operator. A simple
///     binary expression might look like:
///     <code>5 + 5</code>
///     However, binary expression can become more complex, and the left and right operands of a binary
///     expression can themselves be binary expressions or any other expression.
///     The operator in a binary expression are confined to a subset of the operators available in Aura.
///     Other operators, such as the logical `and` and `or`, are available in other expression types. In
///     the case of the logical operators, they can be used in a <c>logical</c> expression.
/// </summary>
/// <param name="Left">The expression on the left side of the binary expression</param>
/// <param name="Operator">The binary expression's operator</param>
/// <param name="Right">The expression on the right side of the binary expression</param>
public record UntypedBinary
	(IUntypedAuraExpression Left, Tok Operator, IUntypedAuraExpression Right) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Left.Range.Start, Right.Range.End);
}

/// <summary>
///     Represents a block, which is a series of statements wrapped in curly braces. Blocks in Aura
///     are used in many places, including function bodies, `if` expression bodies, and loop bodies.
///     Each block defines its own local scope, and users may define local variables inside a block
///     that shadow variables with the same name defined outside of the block.
///     Blocks themselves are expressions, and so return a value, but that value is not used everywhere
///     that blocks are used. For example, <c>while</c> and <c>for</c> loops do not return a value.
/// </summary>
/// <param name="OpeningBrace">
///     A token representing the block's opening brace, which determines the node's starting
///     position in the Aura source file
/// </param>
/// <param name="Statements">The block's statements</param>
/// <param name="ClosingBrace">
///     A token representing the block's closing brace, which determines the node's ending
///     position in the Aura source file
/// </param>
public record UntypedBlock
	(Tok OpeningBrace, List<IUntypedAuraStatement> Statements, Tok ClosingBrace) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(OpeningBrace.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents a function call
/// </summary>
/// <param name="Callee">The expression being called</param>
/// <param name="Arguments">
///     The call's arguments. Each argument is a tuple containing an optional tag and the argument's value.
///     An argument's tag must precede the argument's value, and the two are separated by a colon. The tag must match the
///     name
///     of one of the function's parameters. Tags can be used to specify arguments in a different order than the function's
///     parameters
///     were defined. For example, the stdlib's <c>printf</c> function could be called with tags like so:
///     <code>printf(a: 5, format: "%d\n")</code>
/// </param>
/// <param name="ClosingParen">
///     A token representing the call's closing parenthesis, which determines the node's ending
///     position in the Aura source file
/// </param>
public record UntypedCall
	(IUntypedAuraCallable Callee, List<(Tok?, IUntypedAuraExpression)> Arguments, Tok ClosingParen)
	: IUntypedAuraExpression, IUntypedAuraCallable
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public string GetName() { return Callee.GetName(); }

	public Range Range => new(Callee.Range.Start, ClosingParen.Range.End);
}

/// <summary>
///     Represents an Aura expression that fetches an attribute from a compound object, such as a
///     module or class. The syntax of a <c>get</c> expression is <code>object.attribute</code> An
///     example might look like:
///     <code>greeter.name</code>
///     where <c>greeter</c> is a class and <c>name</c> is an attribute of that class.
/// </summary>
/// <param name="Obj">
///     The compound object being queried. This compound object should contain the attribute being fetched via
///     the <see cref="Name" /> parameter
/// </param>
/// <param name="Name">The attribute being fetched</param>
public record UntypedGet(IUntypedAuraExpression Obj, Tok Name) : IUntypedAuraExpression, IUntypedAuraCallable
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public string GetName() { return Name.Value; }

	public Range Range => new(Obj.Range.Start, Name.Range.End);
}

/// <summary>
///     Represents an Aura expression that fetches a single item from an indexable data type
/// </summary>
/// <param name="Obj">The compound object being queried</param>
/// <param name="Index">The index being fetched</param>
/// <param name="ClosingBracket">
///     A token representing the closing bracket, which determines the node's ending
///     position in the Aura source file
/// </param>
public record UntypedGetIndex
	(IUntypedAuraExpression Obj, IUntypedAuraExpression Index, Tok ClosingBracket) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Obj.Range.Start, ClosingBracket.Range.End);
}

/// <summary>
///     Represents an Aura expression that fetches a range of items from an indexable data type
/// </summary>
/// <param name="Obj">The compound object being queried</param>
/// <param name="Lower">The lower bound of the range. This value is inclusive.</param>
/// <param name="Upper">The upper bound of the range. This value is exclusive.</param>
/// <param name="ClosingBracket">
///     A token representing the closing bracket, which determines the node's ending position
///     in the Aura source file
/// </param>
public record UntypedGetIndexRange
	(IUntypedAuraExpression Obj, IUntypedAuraExpression Lower, IUntypedAuraExpression Upper, Tok ClosingBracket)
	: IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Obj.Range.Start, ClosingBracket.Range.End);
}

/// <summary>
///     Represents one or more Aura expressions grouped together with parentheses. A simple
///     grouping expression would look like:
///     <code>(5 + 5)</code>
/// </summary>
/// <param name="OpeningParen">
///     A token representing the grouping's opening parenthesis, which determines the node's starting
///     position in the Aura source file
/// </param>
/// <param name="Expr">The grouped expression</param>
/// <param name="ClosingParen">
///     A token representing the grouping's closing parenthesis, which determines the node's ending
///     position in the Aura source file
/// </param>
public record UntypedGrouping(Tok OpeningParen, IUntypedAuraExpression Expr, Tok ClosingParen) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(OpeningParen.Range.Start, ClosingParen.Range.End);
}

/// <summary>
///     Represents an <c>if</c> expression, which consists of at least two components -
///     the condition and one or more conditional branches of execution. A simple <c>if</c>
///     expression might look like this:
///     <code>
/// if true {
///     io.println("true")
/// } else {
///     io.println("false")
/// }
/// </code>
///     The expression's condition must be a valid Aura expression and does not need to be surrounded
///     by parentheses.
/// </summary>
/// <param name="If">
///     A token representing the <c>if</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Condition">
///     The condition that will be evaluated first when entering the <c>if</c> expression. If the condition evaluates
///     to true, the <see cref="Then" /> branch will be executed.
/// </param>
/// <param name="Then">The branch that will be executed if the <see cref="Condition" /> evaluates to true</param>
/// <param name="Else">The branch that will be executed if the <see cref="Condition" /> evalutes to false</param>
public record UntypedIf
	(Tok If, IUntypedAuraExpression Condition, UntypedBlock Then, IUntypedAuraExpression? Else) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(If.Range.Start, Else?.Range.End ?? Then.Range.End);
}

/// <summary>
///     Represents Aura's <c>nil</c> keyword.
/// </summary>
/// <param name="Nil">A token representing the <c>nil</c> keyword</param>
public record UntypedNil(Tok Nil) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Nil.Range;
}

/// <summary>
///     Represents a logical expression, which is any binary expression that evaluates
///     to a boolean value
/// </summary>
/// <param name="Left">The expression on the left side of the expression</param>
/// <param name="Operator">The logical expression's operator</param>
/// <param name="Right">The expression on the right side of the expression</param>
public record UntypedLogical
	(IUntypedAuraExpression Left, Tok Operator, IUntypedAuraExpression Right) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Left.Range.Start, Right.Range.End);
}

/// <summary>
///     Represents a set expression, which is a binary expression whose operator must be an equal sign,
///     and consisting of a <c>get</c> expression on the left hand side of the expression and an expression
///     on the right hand side whose result will be assigned to the left hand side operand. A <c>set</c>
///     expression is similar to an assignment expression except the expression on the left hand side must be
///     a <c>get</c> expression on a compound object.
///     A simple <c>set</c> expression would look like:
///     <code>greeter.name = "Bob"</code>
/// </summary>
/// <param name="Obj">The compound object whose attribute is getting a new value</param>
/// <param name="Name">The name of the attribute being assigned a new value</param>
/// <param name="Value">The new value</param>
public record UntypedSet(IUntypedAuraExpression Obj, Tok Name, IUntypedAuraExpression Value) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Obj.Range.Start, Value.Range.End);
}

/// <summary>
///     Represents the <c>this</c> keyword when it's used inside of a class's declaration body
/// </summary>
/// <param name="This">The <c>this</c> keyword</param>
public record UntypedThis(Tok This) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => This.Range;
}

/// <summary>
///     Represents a unary expression, which consists of an operator and an operand.
///     A simple unary expression in Aura looks like:
///     <code>!true</code>
/// </summary>
/// <param name="Operator">The expression's operator, must be one of <c>!</c>, <c>-</c></param>
/// <param name="Right">
///     The expression's type is determined by the <see cref="Operator" />. If the operator is
///     <c>!</c>, the expression must be a boolean value, and if the operator is <c>-</c>, the expression must
///     be either an integer or a float.
/// </param>
public record UntypedUnary(Tok Operator, IUntypedAuraExpression Right) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Operator.Range.Start, Right.Range.End);
}

/// <summary>
///     Represents an Aura variable. At the parsing stage of the compilation process, a variable is any token
///     that doesn't match an Aura reserved keyword or reserved token.
/// </summary>
/// <param name="Name">The variable's name</param>
public record UntypedVariable(Tok Name) : IUntypedAuraExpression, IUntypedAuraCallable
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public string GetName() { return Name.Value; }

	public Range Range => new(Name.Range.Start, Name.Range.End);
}

/// <summary>
///     Represents an <c>is</c> expression, which determines if the supplied expression matches the expected type
/// </summary>
/// <param name="Expr">The expression whose type is being tested</param>
/// <param name="Expected">The expected type that the expression's type is compared against</param>
public record UntypedIs(IUntypedAuraExpression Expr, UntypedInterfacePlaceholder Expected) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Expr.Range.Start, Expected.Range.End);
}

/// <summary>
///     Represents a <c>defer</c> statement that is responsible for deferring the supplied function call
///     until the end of the enclosing function's execution.
/// </summary>
/// <param name="Defer">
///     A token representing the <c>defer</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Call">The call expression to be deferred until the end of the enclosing function scope</param>
public record UntypedDefer(Tok Defer, IUntypedAuraCallable Call) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Defer.Range.Start, Call.Range.End);
}

/// <summary>
///     Represents any expression used in a context where a statement is expected. In these situations,
///     the expression's return value is ignored.
/// </summary>
/// <param name="Expression">The expression that was present in a context where a statement was expected</param>
public record UntypedExpressionStmt(IUntypedAuraExpression Expression) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Expression.Range;
}

/// <summary>
///     Represents an Aura <c>for</c> loop, which has the same structure as a C <c>for</c>
///     loop, except that the parentheses are not required around the loop's signature. For example,
///     a <c>for</c> loop in Aura would look like:
///     <code>
/// for i := 0; i < 10; i++ {
///                   io.printf("%d\b", i)
/// }
/// </code>
/// </summary>
/// <param name="For">
///     A token representing the <c>for</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Initializer">Used to initialize a variable that will be available in the loop's body</param>
/// <param name="Condition">
///     The condition that will be evaluated after each iteration. If the condition evaluates to false, the loop
///     will exit
/// </param>
/// <param name="Increment">The increment expression, which is executed after each iteration of the loop</param>
/// <param name="Body">Collection of statements that will be executed on each iteration</param>
/// <param name="ClosingBrace">
///     A token representing the closing brace, which determines the node's ending position in the
///     Aura source file
/// </param>
public record UntypedFor
(
	Tok For,
	IUntypedAuraStatement? Initializer,
	IUntypedAuraExpression? Condition,
	IUntypedAuraExpression? Increment,
	List<IUntypedAuraStatement> Body,
	Tok ClosingBrace
) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(For.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents a simplified <c>for</c> loop that supports iterating through an
///     iterable data type. The syntax for a <c>foreach</c> loop looks like this:
///     <code>for <c>var_name</c> in <c>iterable</c> { ... }</code>
/// </summary>
/// <param name="ForEach">
///     A token representing the <c>foreach</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="EachName">
///     The name of the variable that will be available in the loop's body and will represent each successive item
///     in the iterable
/// </param>
/// <param name="Iterable">The collection being iterated over</param>
/// <param name="Body">Collection of statements that will be executed on each iteration</param>
/// <param name="ClosingBrace">
///     A token representing the loop's closing brace, which determines the node's ending position
///     in the Aura source file
/// </param>
public record UntypedForEach
	(Tok ForEach, Tok EachName, IUntypedAuraExpression Iterable, List<IUntypedAuraStatement> Body, Tok ClosingBrace)
	: IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(ForEach.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents a named function declaration. The syntax for declaring a named function looks like:
///     <code>fn <c>function_name</c>(<c>param_name</c>: <c>param_type</c>[,...]_ -> <c>return_type</c> { ... }</code>
/// </summary>
/// <param name="Fn">
///     A token representing the <c>fn</c> keyword, which helps determine the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Name">The function's name</param>
/// <param name="Params">The function's parameters</param>
/// <param name="Body">The function's body</param>
/// <param name="ReturnType">The function's return type(s).</param>
/// <param name="Public">Indicates if the function is public or private</param>
/// <param name="Documentation">
///     The optional documentation comment that appears on the line directly above the function
///     in the Aura source file
/// </param>
public record UntypedNamedFunction
(
	Tok Fn,
	Tok Name,
	List<Param> Params,
	UntypedBlock Body,
	List<AuraType>? ReturnType,
	Visibility Public,
	string? Documentation
) : IUntypedAuraStatement, IUntypedFunction
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public List<Param> GetParams() { return Params; }

	public List<ParamType> GetParamTypes() { return Params.Select(p => p.ParamType).ToList(); }

	public Range Range => new(Fn.Range.Start, Body.Range.End);
}

/// <summary>
///     Represents an anonymous function, which can be declared the same way as a named function,
///     just without including the function's name.
/// </summary>
/// <param name="Fn">
///     A token representing the <c>fn</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Params">The function's parameters</param>
/// <param name="Body">The function's body</param>
/// <param name="ReturnType">
///     The function's return type. This struct stores it as a token instead of a type because it hasn't
///     /// been type checked yet.
/// </param>
public record UntypedAnonymousFunction
	(Tok Fn, List<Param> Params, UntypedBlock Body, List<AuraType>? ReturnType)
	: IUntypedAuraExpression, IUntypedFunction
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public List<Param> GetParams() { return Params; }

	public List<ParamType> GetParamTypes() { return Params.Select(p => p.ParamType).ToList(); }

	public Range Range => new(Fn.Range.Start, Body.Range.End);
}

/// <summary>
///     Represents a <c>let</c> expression that declares a new variable and optionally assigns it an initial value.
///     Variable declarations in Aura can be written one of two ways. The full <c>let</c>-style declaration looks like:
///     <code>let i: int = 5</code>
///     where the variable's type annotation is required. For a shorter syntax, one can write:
///     <code>i := 5</code>
/// </summary>
/// <param name="Let">
///     A token representing the <c>let</c> keyword, which helps determine the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Names">The name(s) of the newly-declared variable(s), along with a boolean value indicating if they are mutable or not</param>
/// <param name="NameTyps">
///     The variable(s)'s types, if they were declared with an explicit type annotation. If not, the value of this field
///     will
///     be <see cref="AuraUnknown" />
/// </param>
/// <param name="Initializer">
///     The initializer expression whose result will be assigned to the new variable. This expression
///     may be omitted.
/// </param>
public record UntypedLet
	(Tok? Let, List<(bool, Tok)> Names, List<AuraType> NameTyps, IUntypedAuraExpression? Initializer)
	: IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range =>
		new(
			Let is not null ? Let.Value.Range.Start : Names.First().Item2.Range.Start,
			Initializer is not null ? Initializer.Range.End : Names.Last().Item2.Range.End
		);
}

/// <summary>
///     Represents the current source file's module declaration. It should appear at the top of the file and have
///     the format:
///     <code>mod <c>mod_name</c></code>
/// </summary>
/// <param name="Mod">
///     A token representing the <c>mod</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Value">The module's name</param>
public record UntypedMod(Tok Mod, Tok Value) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Mod.Range.Start, Value.Range.End);
}

/// <summary>
///     Represents a <c>return</c> statement, which can be either explicit (i.e. <code>return 5</code>) or, in specific
///     circumstances, implicit.
/// </summary>
/// <param name="Return">
///     A token representing the <c>return</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Value">The value to be returned</param>
public record UntypedReturn(Tok Return, List<IUntypedAuraExpression>? Value) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Return.Range.Start, Value is not null ? Value.Last().Range.End : Return.Range.End);
}

/// <summary>
///     Represents an Aura struct declaration
/// </summary>
/// <param name="Struct">
///     A token representing the <c>struct</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Name">The struct's name</param>
/// <param name="Params">The struct's parameters</param>
/// <param name="ClosingParen">
///     A token representing the struct's closing parenthesis, which determines the node's ending
///     position in the Aura source file
/// </param>
/// <param name="Documentation">
///     The optional documentation comment located on the line directly above the struct's
///     declaration in the Aura source file
/// </param>
public record UntypedStruct
	(Tok Struct, Tok Name, List<Param> Params, Tok ClosingParen, string? Documentation) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Struct.Range.End, ClosingParen.Range.End);
}

/// <summary>
///     Represents an Aura anonymous struct
/// </summary>
/// <param name="Struct">
///     A token representing the <c>struct</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Params">The anonymous struct's parameters</param>
/// <param name="Values">The values provided when instantiating the anonymous struct</param>
/// <param name="ClosingParen">
///     A token representing the anonymous struct's closing parenthesis, which determines the node's
///     ending position in the Aura source file
/// </param>
public record UntypedAnonymousStruct
	(Tok Struct, List<Param> Params, List<IUntypedAuraExpression> Values, Tok ClosingParen) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Struct.Range.Start, ClosingParen.Range.End);
}

/// <summary>
///     Represents a class declaration, which follows the syntax:
///     <code>class <c>class_name</c>(<c>param</c>: <c>param_type</c>[,...]) { ... }</code>
/// </summary>
/// <param name="Class">
///     A token representing the <c>class</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Name">The class's name</param>
/// <param name="Params">The class's parameters</param>
/// <param name="Body">The class's body</param>
/// <param name="Public">Indicates if the class is public or not</param>
/// <param name="Implementing">A list of interfaces implemented by the class</param>
/// <param name="ClosingBrace">
///     A token representing the class's closing brace, which determines the node's ending
///     position in the Aura source file
/// </param>
/// <param name="Documentation">
///     The optional documentation comment appearing on the line directly above the class declaration
///     in the Aura source file
/// </param>
public record UntypedClass
(
	Tok Class,
	Tok Name,
	List<Param> Params,
	List<IUntypedAuraStatement> Body,
	Visibility Public,
	List<Tok> Implementing,
	Tok ClosingBrace,
	string? Documentation
) : IUntypedAuraStatement, IUntypedFunction
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public List<Param> GetParams() { return Params; }

	public List<ParamType> GetParamTypes() { return Params.Select(p => p.ParamType).ToList(); }

	public List<UntypedNamedFunction> Methods =>
		Body.Where(item => item is not UntypedComment).Select(m => (UntypedNamedFunction)m).ToList();

	public Range Range => new(Class.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents an interface declaration, which follows the syntax:
///     <code>interface <c>interface_name</c> { ... } </code>
/// </summary>
/// <param name="Interface">
///     A token representing the <c>interface</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Name">The interface's name</param>
/// <param name="Methods">The interface's methods</param>
/// <param name="Public">Indicates if the interface is public or not</param>
/// <param name="ClosingBrace">
///     A token representing the interface's closing brace, which determines the node's ending
///     position in the Aura source file
/// </param>
/// <param name="Documentation">
///     The optional documentation comment appearing on the line directly above the interface in
///     the Aura source file
/// </param>
public record UntypedInterface
(
	Tok Interface,
	Tok Name,
	List<UntypedFunctionSignature> Methods,
	Visibility Public,
	Tok ClosingBrace,
	string? Documentation
) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range =>
		new(
			Public == Visibility.Public
				? Interface.Range.Start with { Character = Interface.Range.Start.Character - 4 }
				: Interface.Range.Start,
			ClosingBrace.Range.End
		);
}

/// <summary>
///     Represents an interface placeholder, which is used in an <c>is</c> expression to represent the expected interface
///     type
/// </summary>
/// <param name="InterfaceValue">A token representing a specific interface type</param>
public record UntypedInterfacePlaceholder(Tok InterfaceValue) : IUntypedAuraExpression
{
	public T Accept<T>(IUntypedAuraExprVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => InterfaceValue.Range;
}

/// <summary>
///     Represents an Aura function signature. This AST node is used to represent the function signatures that appear in
///     the
///     body of an interface.
/// </summary>
/// <param name="Visibility">
///     An optional token representing the <c>pub</c> keyword, which determines the node's starting
///     position in the Aura source file
/// </param>
/// <param name="Fn">
///     A token representing the <c>fn</c> keyword, which determines the node's starting position in the Aura
///     source file when the <c>pub</c> keyword is omitted
/// </param>
/// <param name="Name">The function's name</param>
/// <param name="Params">The function's parameters</param>
/// <param name="ClosingParen">
///     A token representing the function signature's closing parenthesis, which helps determine
///     the node's ending position in the Aura source file when the return type is omitted
/// </param>
/// <param name="ReturnType">The function's return type</param>
/// <param name="Documentation">
///     The optional documentation comment appearing on the line directly above the function
///     signature in the Aura source file
/// </param>
public record UntypedFunctionSignature
(
	Tok? Visibility,
	Tok Fn,
	Tok Name,
	List<Param> Params,
	Tok ClosingParen,
	AuraType ReturnType,
	string? Documentation
)
	: IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range =>
		new(Visibility is not null ? Visibility.Value.Range.Start : Fn.Range.Start, ClosingParen.Range.End);
}

/// <summary>
///     Represents a <c>while</c> loop, which follows this syntax:
///     <code>while true { ... }</code>
///     where its body is executed until the loop's condition evaluates to false.
/// </summary>
/// <param name="While">
///     A token representing the <c>while</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Condition">
///     The condition to be evaluated on each iteration of the loop. The loop will exit when the condition
///     evaluates to false.
/// </param>
/// <param name="Body">Collection of statements executed once per iteration</param>
/// <param name="ClosingBrace">
///     A token representing the loop's closing brace, which determines the node's ending position
///     in the Aura source file
/// </param>
public record UntypedWhile
	(Tok While, IUntypedAuraExpression Condition, List<IUntypedAuraStatement> Body, Tok ClosingBrace)
	: IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(While.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents an <c>import</c> statement
/// </summary>
/// <param name="Import">
///     A token representing the <c>import</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Package">The name of the package being imported</param>
/// <param name="Alias">Will contain a value if the import has an alias</param>
public record UntypedImport(Tok Import, Tok Package, Tok? Alias) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Import.Range.Start, Alias is not null ? Alias.Value.Range.End : Package.Range.End);
}

/// <summary>
///     Represents a multiple <c>import</c> statement
/// </summary>
/// <param name="Import">
///     A token representing the <c>import</c> keyword, which determines the node's starting position
///     in the Aura source file
/// </param>
/// <param name="Packages">The packages to import</param>
/// <param name="ClosingBrace">
///     A token representing the closing brace, which determines the node's ending position in the
///     Aura source file
/// </param>
public record UntypedMultipleImport(Tok Import, List<UntypedImport> Packages, Tok ClosingBrace) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Import.Range.Start, ClosingBrace.Range.End);
}

/// <summary>
///     Represents an Aura comment, which can be declared in two different ways. Beginning a comment with
///     <c>//</c> will declare a single-line comment that will last until the next <c>\n</c> character. Beginning
///     a comment with <c>/*</c> will declare a comment that will last until the closing <c>*/</c>, which may be
///     on the same line or a future line.
/// </summary>
/// <param name="Text">The comment's text</param>
public record UntypedComment(Tok Text) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Text.Range;
}

/// <summary>
///     Represents a <c>continue</c> statement that is used as a control flow construct inside a loop. The
///     <c>continue</c> keyword will advance execution to the next iteration in the loop.
/// </summary>
/// <param name="Continue">A token representing the <c>continue</c> keyword</param>
public record UntypedContinue(Tok Continue) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Continue.Range;
}

/// <summary>
///     Represents a <c>break</c> statement that is used as a control flow construct inside a loop. The <c>break</c>
///     keyword will immediately break out of the enclosing loop.
/// </summary>
/// <param name="Break">A token representing the <c>break</c> keyword</param>
public record UntypedBreak(Tok Break) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Break.Range;
}

/// <summary>
///     Represents a <c>yield</c> statement that is used to return a value from an <c>if</c> expression or block without
///     returning from the enclosing function.
/// </summary>
/// <param name="Yield">
///     A token representing the <c>yield</c> keyword, which determines the node's starting position in the
///     Aura source file
/// </param>
/// <param name="Value">The value to be yielded from the enclosing scope</param>
public record UntypedYield(Tok Yield, IUntypedAuraExpression Value) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Yield.Range.Start, Value.Range.End);
}

/// <summary>
///     Represents a newline character
/// </summary>
/// <param name="Newline">A token representing the newline character</param>
public record UntypedNewLine(Tok Newline) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => Newline.Range;
}

/// <summary>
///     Represents a check statement
/// </summary>
/// <param name="Check">
///     A token representing the <c>check</c> keyword, which determines the node's starting position in
///     the Aura source file
/// </param>
/// <param name="Call">The call to defer</param>
public record UntypedCheck(Tok Check, UntypedCall Call) : IUntypedAuraStatement
{
	public T Accept<T>(IUntypedAuraStmtVisitor<T> visitor) { return visitor.Visit(this); }

	public Range Range => new(Check.Range.Start, Call.Range.End);
}
