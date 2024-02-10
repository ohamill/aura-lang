﻿using AuraLang.Lsp.DocumentManager;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace AuraLang.Lsp.LanguageServer;

public class AuraLanguageServer : IDisposable
{
	private JsonRpc? _rpc;
	private readonly ManualResetEvent _disconnectEvent = new(false);
	private bool _isDisposed;
	private static readonly object Object = new();
	private bool Verbose { get; }
	private readonly AuraDocumentManager _documents = new();

	public AuraLanguageServer(bool verbose)
	{
		Verbose = verbose;
	}

	public async Task InitAsync()
	{
		_rpc = JsonRpc.Attach(Console.OpenStandardOutput(), Console.OpenStandardInput(), this);
		_rpc.Disconnected += OnRpcDisconnected;
		await _rpc.Completion;
	}

	[JsonRpcMethod(Methods.InitializeName)]
	public InitializeResult Initialize(JToken arg)
	{
		lock (Object)
		{
			if (Verbose) Console.Error.WriteLine("<-- Initialize");

			var capabilities = new ServerCapabilities
			{
				TextDocumentSync =
					new TextDocumentSyncOptions
					{
						Change = TextDocumentSyncKind.Full,
						OpenClose = true,
						Save = new SaveOptions { IncludeText = true },
						WillSave = true,
						WillSaveWaitUntil = true
					},
				CompletionProvider = null,
				HoverProvider = false,
				SignatureHelpProvider = null,
				DefinitionProvider = false,
				TypeDefinitionProvider = false,
				ImplementationProvider = false,
				ReferencesProvider = false,
				DocumentHighlightProvider = false,
				DocumentSymbolProvider = false,
				CodeLensProvider = null,
				DocumentLinkProvider = null,
				DocumentFormattingProvider = true,
				DocumentRangeFormattingProvider = false,
				RenameProvider = false,
				ExecuteCommandProvider = null,
				WorkspaceSymbolProvider = false,
				SemanticTokensOptions = null,
			};

			InitializeResult result = new InitializeResult { Capabilities = capabilities };

			var json = JsonConvert.SerializeObject(result);
			if (Verbose) Console.Error.WriteLine("--> " + json);
			return result;
		}
	}

	[JsonRpcMethod(Methods.InitializedName)]
	public void InitializedName(JToken arg)
	{
		lock (Object)
		{
			try
			{
				if (Verbose)
				{
					Console.Error.WriteLine("<-- Initialized");
					Console.Error.WriteLine(arg.ToString());
				}
			}
			catch (Exception)
			{
			}
		}
	}

	[JsonRpcMethod(Methods.TextDocumentDidOpenName)]
	public void DidOpenTextDocument(JToken jToken)
	{
		var @params = DeserializeJToken<DidOpenTextDocumentParams>(jToken);
		_documents.UpdateDocument(@params.TextDocument.Uri.LocalPath, @params.TextDocument.Text);
		Console.Error.WriteLine(
			$"Updated document at path '{@params.TextDocument.Uri.LocalPath}' with new content: {@params.TextDocument.Text}");
	}

	[JsonRpcMethod(Methods.TextDocumentDidChangeName)]
	public List<Diagnostic>? DidChangeTextDocument(JToken jToken)
	{
		Console.Error.WriteLine("Received `didChange` notification!");
		var @params = DeserializeJToken<DidChangeTextDocumentParams>(jToken);
		_documents.UpdateDocument(@params!.TextDocument.Uri.LocalPath, @params.ContentChanges.First().Text);
		Console.Error.WriteLine(
			$"Updated document at path '{@params.TextDocument.Uri.LocalPath}' with new content: {@params.ContentChanges.First().Text}");
		var d = new Diagnostic
		{
			Code = "Warning",
			Message = "Test message",
			Severity = DiagnosticSeverity.Error,
			Range = new Range
			{
				Start = new Position { Line = 6, Character = 4 },
				End = new Position { Line = 6, Character = 10 }
			}
		};
		Console.Error.WriteLine("writing diagnostic");
		return new List<Diagnostic> { d };
	}

	[JsonRpcMethod(Methods.TextDocumentWillSaveName)]
	public void WillSaveTextDocument(JToken jToken)
	{
		var @params = DeserializeJToken<WillSaveTextDocumentParams>(jToken);
		Console.Error.WriteLine(
			$"Received `willSave` notification: {@params.TextDocument.Uri.LocalPath} with reason {@params.Reason}");
	}

	[JsonRpcMethod(Methods.TextDocumentWillSaveWaitUntilName)]
	public TextEdit[]? WillSaveWaitUntilTextDocument(JToken jToken)
	{
		var @params = DeserializeJToken<WillSaveTextDocumentParams>(jToken);
		Console.Error.WriteLine(
			$"Received `willSaveWaitUntil` notification: {@params.TextDocument.Uri.LocalPath} with reason {@params.Reason}");
		return null;
	}

	[JsonRpcMethod(Methods.TextDocumentDidSaveName)]
	public void DidSaveTextDocument(JToken jToken)
	{
		var @params = DeserializeJToken<DidSaveTextDocumentParams>(jToken);
		_documents.UpdateDocument(@params.TextDocument.Uri.LocalPath, @params.Text!);
		Console.Error.WriteLine(
			$"Received `didSave` notification for file '{@params.TextDocument.Uri.LocalPath}' with content {@params.Text!}");
	}

	[JsonRpcMethod(Methods.TextDocumentDidCloseName)]
	public void DidCloseTextDocument(JToken jToken)
	{
		var @params = DeserializeJToken<DidCloseTextDocumentParams>(jToken);
		_documents.DeleteDocument(@params.TextDocument.Uri.LocalPath);
		Console.Error.WriteLine($"Received `didClose` notification for file '{@params.TextDocument.Uri.LocalPath}'");
	}

	[JsonRpcMethod(Methods.ShutdownName)]
	public JToken? ShutdownName()
	{
		lock (Object)
		{
			if (Verbose) Console.Error.WriteLine("<-- Shutdown");
			return null;
		}
	}

	[JsonRpcMethod(Methods.ExitName)]
	public void ExitName()
	{
		lock (Object)
		{
			try
			{
				if (Verbose) Console.Error.WriteLine("<-- Exit");
				Exit();
			}
			catch (Exception) { }
		}
	}

	~AuraLanguageServer()
	{
		Dispose(false);
	}

	public void Start()
	{
		_disconnectEvent.WaitOne();
	}

	private void OnRpcDisconnected(object? sender, JsonRpcDisconnectedEventArgs e) => Exit();

	private void Exit()
	{
		Environment.Exit(0);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_isDisposed) return;
		if (disposing)
		{
			_disconnectEvent.Dispose();
		}

		_isDisposed = true;
	}

	private T DeserializeJToken<T>(JToken jToken)
	{
		var s = JsonConvert.SerializeObject(jToken);
		var t = JsonConvert.DeserializeObject<T>(s);
		return t!;
	}
}
