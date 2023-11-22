﻿namespace AuraLang.Exceptions.Compiler;

public class CompilerExceptionContainer : AuraExceptionContainer
{
	public void Add(CompilerException ex)
	{
		Exs.Add(ex);
	}
}

public abstract class CompilerException : AuraException
{
	protected CompilerException(string message, int line) : base(message, line) { }
}

public class UnknownStatementException : CompilerException
{
	public UnknownStatementException(int line) : base("Unknown statement", line) { }
}

public class UnknownExpressionException : CompilerException
{
	public UnknownExpressionException(int line) : base("Unknown expression", line) { }
}
