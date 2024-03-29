﻿using AuraLang.Cli.Options;
using AuraLang.Lsp.LanguageServer;

namespace AuraLang.Cli.Commands;

public class Lsp : AuraCommand
{
	public Lsp(LspOptions opts) : base(opts) { }

	public override async Task<int> ExecuteAsync() => await ExecuteCommandAsync();

	protected override async Task<int> ExecuteCommandAsync()
	{
		//logger.LogSuccinct("Starting new LSP server...");
		var server = new AuraLanguageServer(true);
		await server.InitAsync();
		return 0;
	}
}
