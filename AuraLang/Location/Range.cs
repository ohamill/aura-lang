﻿namespace AuraLang.Location;

/// <summary>
/// Represents a range in an Aura source file. Ranges can be used to place both tokens and AST nodes in an source file.
/// </summary>
public record Range
{
	/// <summary>
	/// The starting position, inclusive
	/// </summary>
	public Position Start;
	/// <summary>
	/// The ending position, exclusive
	/// </summary>
	public Position End;

	public Range(Position start, Position end)
	{
		Start = start;
		End = end;
	}

	public Range()
	{
		Start = new Position();
		End = new Position();
	}

	public bool Contains(Position position)
	{
		if (position.Line < Start.Line || position.Line > End.Line) return false;
		if (position.Character < Start.Character || position.Character >= End.Character) return false;
		return true;
	}

	public override string ToString() => $"{Start}-{End}";
}
