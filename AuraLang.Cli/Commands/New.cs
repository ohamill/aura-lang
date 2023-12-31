﻿using System.Diagnostics;
using AuraLang.Cli.Options;
using AuraLang.Cli.Toml;

namespace AuraLang.Cli.Commands;

public class New : AuraCommand
{
	/// <summary>
	/// The project's name. Provided as an argument to `aura new`
	/// </summary>
	private string Name { get; }

	public New(NewOptions opts) : base(opts)
	{
		Name = opts.Name!;
	}

	/// <summary>
	/// Creates a new Aura project
	/// </summary>
	/// <returns>An integer status indicating if the process succeeded</returns>
	public override int Execute()
	{
		var projPath = $"./{Name}";
		Directory.CreateDirectory(projPath);
		Directory.CreateDirectory($"{projPath}/src");
		File.WriteAllText($"{projPath}/src/{Name}.aura", "mod main\n\nimport aura/io\n\nfn main() {\n\tio.println(\"Hello world!\")\n}\n");
		Directory.CreateDirectory($"{projPath}/test");
		Directory.CreateDirectory($"{projPath}/build");
		Directory.CreateDirectory($"{projPath}/build/pkg");
		Directory.CreateDirectory($"{projPath}/build/pkg/stdlib");
		var homeDir = Environment.GetEnvironmentVariable("HOME");

		var cp = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "cp",
				Arguments = $"-R \"{homeDir}/.aura/stdlib/\" \"{projPath}/build/pkg/stdlib\""
			}
		};
		cp.Start();
		cp.WaitForExit();

		File.WriteAllText($"{projPath}/README.md", string.Empty);
		File.WriteAllText($"{projPath}/aura.toml", string.Empty);
		new AuraToml(projPath).InitProject(Name);

		Directory.SetCurrentDirectory($"./{projPath}/build/pkg");

		var modInit = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "go",
				Arguments = $"mod init {Name}",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			}
		};
		modInit.Start();
		modInit.WaitForExit();
		if (modInit.ExitCode > 0)
		{
			Console.WriteLine(modInit.StandardError.ReadToEnd());
		}
		else
		{
			Console.WriteLine($"New Aura project `{Name}` successfully created!");
		}

		return 0;
	}
}
