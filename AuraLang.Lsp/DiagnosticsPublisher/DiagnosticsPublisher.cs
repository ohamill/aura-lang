﻿using AuraLang.Exceptions;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;
using LspRange = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace AuraLang.Lsp.DiagnosticsPublisher;

/// <summary>
///     Responsible for publishing diagnostics to the LSP client
/// </summary>
public class AuraDiagnosticsPublisher
{
	/// <summary>
	///     The JSON RPC connection used to transmit diagnostics
	/// </summary>
	private JsonRpc Rpc { get; }

	public AuraDiagnosticsPublisher(JsonRpc rpc)
	{
		Rpc = rpc;
	}

	/// <summary>
	///     Sends diagnostics to the LSP client corresponding to the supplied exception and Aura source file URI
	/// </summary>
	/// <param name="ex">
	///     The <see cref="AuraExceptionContainer" /> encountered during the compilation process. The specific details of
	///     the diagnostic will be extracted from this exception
	/// </param>
	/// <param name="uri">The path of the Aura source file where the error was encountered</param>
	public async Task SendAsync(AuraExceptionContainer ex, Uri uri)
	{
		var diagnostics = ex.Exs.SelectMany(
				e =>
				{
					return e.Range.Select(
						r => new Diagnostic
						{
							Code = "Warning",
							Message = e.Message,
							Severity = DiagnosticSeverity.Error,
							Range = new LspRange
							{
								Start = new Position { Line = r.Start.Line, Character = r.Start.Character },
								End = new Position { Line = r.End.Line, Character = r.End.Character }
							}
						}
					);
				}
			)
			.ToArray();
		var publish = new PublishDiagnosticParams { Uri = uri, Diagnostics = diagnostics };

		// Send 'textDocument/publishDiagnostics' notification to the client
		await Rpc.NotifyWithParameterObjectAsync("textDocument/publishDiagnostics", publish);
	}

	/// <summary>
	///     Clears any existing diagnostics
	/// </summary>
	/// <param name="uri">The path of the Aura source file where the diagnostics will be cleared</param>
	public async Task ClearAsync(Uri uri)
	{
		var publish = new PublishDiagnosticParams { Uri = uri, Diagnostics = Array.Empty<Diagnostic>() };
		await Rpc.NotifyWithParameterObjectAsync("textDocument/publishDiagnostics", publish);
	}
}
