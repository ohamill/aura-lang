﻿using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace AuraLang.Types;

/// <summary>
///     Represents a type that can provide completion options to an LSP client
/// </summary>
public interface ICompletable
{
	/// <summary>
	///     Indicates which trigger characters this type supports
	/// </summary>
	IEnumerable<string> SupportedTriggerCharacters { get; }

	/// <summary>
	///     Returns a boolean value indicating if this type supports the supplied trigger character
	/// </summary>
	/// <param name="triggerCharacter">The trigger character that may be supported</param>
	/// <returns>A boolean indicating if this type supports the supplied trigger character</returns>
	bool IsTriggerCharacterSupported(string triggerCharacter);

	/// <summary>
	///     Returns a list of completion options for the supplied trigger character
	/// </summary>
	/// <param name="triggerCharacter">The trigger character</param>
	/// <returns>A list of completion options</returns>
	CompletionList ProvideCompletableOptions(string triggerCharacter);

	/// <summary>
	///     Returns an alphabetized list of completion options for the supplied trigger character
	/// </summary>
	/// <param name="triggerCharacter">The trigger character</param>
	/// <returns>An alphabetized list of completion options</returns>
	CompletionList ProvideCompletableOptionsAndAlphabetize(string triggerCharacter)
	{
		var completionOptions = ProvideCompletableOptions(triggerCharacter);
		completionOptions.Items = completionOptions.Items.OrderBy(item => item.Label).ToArray();
		return completionOptions;
	}
}
