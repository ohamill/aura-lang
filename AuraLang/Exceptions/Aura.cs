﻿namespace AuraLang.Exceptions;

public abstract class AuraExceptionContainer : Exception
{
	public readonly List<AuraException> Exs = new();
	protected string FilePath { get; init; }

	public AuraExceptionContainer(string filePath)
	{
		FilePath = filePath;
	}

	public bool IsEmpty() => Exs.Count == 0;

	public void Report()
	{
		var errs = Exs.Select(ex => ex.Error(FilePath));
		var output = string.Join("\n\n", errs);
		Console.WriteLine(output);
	}
}

public abstract class AuraException : Exception
{
	public int Line { get; }

	protected AuraException(string message, int line) : base(message)
	{
		Line = line;
	}

	public string Error(string filePath) => $"[{filePath} line {Line}] {Message}";
}

