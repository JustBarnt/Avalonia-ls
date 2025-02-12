namespace Main;
using System.Text.Json;
using System.Diagnostics;
public class Args
{
    public string? file;
    public string port = "8080";
    public string target = "browser";
}
public class PreviewerParams
{
    public required string depsFilePath;
    public required string runtimeConfigPath;
    public required string hostappPath;
    public required string targetPath;
    public required string targetFile;
    public required string targetPort;
}
internal class Program
{
    private static Args _args = new();
    private static PreviewerParams? _params;
    internal static void Main(String[] args)
    {
        uint index = 0;
        foreach (string arg in args)
        {
            if (args.Length == index) break;
            switch (arg)
            {
                case "--file":
                    _args.file = args[index + 1];
                    break;
                case "--port":
                    _args.port = args[index + 1];
                    break;
                case "--target":
                    if (args[index + 1] == "browser" || args[index + 1] == "terminal")
                    {
                        _args.target = args[index + 1];
                    }
                    else
                    {
                        Console.WriteLine($"Unknown Target {args[index + 1]}. Using 'browser' target");
                    }
                    break;
                default: break;
            }
            index++;
        }
        if (_args.file is null)
        {
            Die("Input AXAML file not given");
        }
        SetMetadata();
        if (_args.target == "browser")
        {
            StartPrevierServer();
        }
        else{
            if(_params is null) return;
            new BsonProtocol(_args,_params).StartPreviewerProcess();
        }
    }
    private static void SetMetadata()
    {
        if (_args.file is null) return;
        if (!File.Exists(_args.file)) Die("Input XAML file does not exist");
        if (Path.GetExtension(_args.file) != ".axaml") Die("Provided file is not a AXAML file");

        string? full_path = Path.GetDirectoryName(Path.GetFullPath(_args.file));
        if (full_path is null) return;

        //traverse upwards until we locate a csproj or fsproj
        string? projectDirectory = null;
        while (projectDirectory is null)
        {
            var projectFiles = Directory.GetFiles(full_path, "*.fsproj").Concat(Directory.GetFiles(full_path, "*.csproj")).ToArray();
            if (projectFiles.Length == 0)
            {
                var parent = Directory.GetParent(full_path);
                if (parent is null) { Die("Could not find find a valid .NET Project."); return; }
                full_path = parent.FullName;
            }
            else
            {
                projectDirectory = full_path;
            }
        }
        var slnFile = Path.GetFileName(projectDirectory);
        if (slnFile is null) return;
        var slnFilePath = Path.Combine(Path.GetTempPath(), $"{slnFile}.json");
        if (!File.Exists(slnFilePath))
        {
            Die("Could not locate metadata. Please Build and run solution parser");
        }
        var solution = JsonSerializer.Deserialize<AvaloniaLanguageServer.Models.SolutionData>(File.ReadAllText(slnFilePath));
        if (solution is null) return;
        var executable_project = solution.GetExecutableProject();
        if (executable_project is null) return;

        _params = new()
        {
            depsFilePath = executable_project.DepsFilePath,
            runtimeConfigPath = executable_project.RuntimeConfigFilePath,
            hostappPath = executable_project.DesignerHostPath,
            targetPath = executable_project.TargetPath,
            targetFile = Path.GetFullPath(_args.file),
            targetPort = _args.port,
        };
    }
    private static void StartPrevierServer()
    {
        if (_params is null) return;
        string args = $"exec --runtimeconfig {_params.runtimeConfigPath} --depsfile {_params.depsFilePath} {_params.hostappPath} --transport file://{_params.targetFile} --html-url http://127.0.0.1:{_params.targetPort} {_params.targetPath}"; Console.WriteLine(args);
        var process_info = new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        var process = new Process() { StartInfo = process_info };
        process.OutputDataReceived += (sender, args) =>
        {
            Console.WriteLine(args.Data);
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            Console.WriteLine(args.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }
    private static void Die(string message)
    {
        Console.WriteLine(message);
        Environment.Exit(1);
    }
}
