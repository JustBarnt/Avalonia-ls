namespace Main;
using System.Net;
using System.IO;
using System.Net.Sockets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Avalonia.Remote.Protocol;
using Avalonia.Remote.Protocol.Designer;
using Avalonia.Remote.Protocol.Viewport;
public class BsonProtocol(Args _args, PreviewerParams _params)
{
    private IDisposable? _listener;
    private IAvaloniaRemoteTransportConnection? _connection;
    private Process? _process;
    private bool IsRunning => _process != null && !_process.HasExited;
    private bool IsReady => IsRunning && _connection != null;
    private bool PreviewIsBeingDisplayed = false;
    public void StartPreviewerProcess()
    {
        var port = FreeTcpPort();
        var tcs = new TaskCompletionSource<object>();
        _listener = new BsonTcpTransport().Listen(
            IPAddress.Loopback,
            port,
            async t =>
            {
                ConnectionInitializedAsync(t);
                tcs.TrySetResult(null);
            }
        );
        if (_params is null) return;
        string args = $"exec --runtimeconfig {_params.runtimeConfigPath} --depsfile {_params.depsFilePath} {_params.hostappPath} --transport tcp-bson://127.0.0.1:{port} {_params.targetPath}"; Console.WriteLine(args);
        var process_info = new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        var process = _process = new Process() { StartInfo = process_info };
        process.OutputDataReceived += (sender, args) =>
        {
            //only log if there is no image being displayed
            if (!PreviewIsBeingDisplayed)
            {
                Console.WriteLine(args.Data);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            Console.WriteLine(args.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        PreviewFile().ConfigureAwait(false);
        process.WaitForExit();
    }
    private async Task PreviewFile()
    {
        if (_args.file is null) return;
        Console.WriteLine("Preparing connection...");
        //we need to make sure the connection is alive before proceeding, pause until it is ready. Not an ideal solution.
        while (_connection is null) { }
        //wait a bit before sending the initial message to ensure the frame message will be recieved.
        Thread.Sleep(500);
        await UpdateXamlAsync(File.ReadAllText(Path.GetFullPath(_args.file)));

    }
    private static int FreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }
    private async Task ConnectionInitializedAsync(IAvaloniaRemoteTransportConnection conn)
    {
        _connection = conn;
        _connection.OnMessage += (IAvaloniaRemoteTransportConnection connection, object message) =>
        {
            switch (message)
            {
                case UpdateXamlResultMessage update:
                    break;
                case FrameMessage frame:
                    using (var image = Image.WrapMemory<Bgra32>(frame.Data, frame.Width, frame.Height))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsPng(ms);
                            var pngData = ms.ToArray();
                            var base64data = Convert.ToBase64String(pngData);
                            Console.WriteLine("Processing");
                            Console.Write("\x1b[2J");
                            Console.Write("\x1b[H");
                            PreviewIsBeingDisplayed = true;
                            Console.Write($"\x1b_Gf=100,a=T,z=1;{base64data}\x1b\\");
                        }
                    }
                    break;
                default: break;
            }
        };
        _connection.OnException += (IAvaloniaRemoteTransportConnection connection, Exception Ex) =>
        {
            System.Console.WriteLine(Ex);
        };
        await SendAsync(new ClientSupportedPixelFormatsMessage
        {
            Formats = new[]
                {
                    Avalonia.Remote.Protocol.Viewport.PixelFormat.Bgra8888,
                    // Avalonia.Remote.Protocol.Viewport.PixelFormat.Rgba8888,
                }
        });
        await SetScalingAsync(2);
    }
    private async Task SendAsync(object message)
    {
        if (_connection is IAvaloniaRemoteTransportConnection connection)
            await connection.Send(message);
    }

    private async Task UpdateXamlAsync(string xaml_path)
    {
        if (_process is null)
        {
            System.Console.WriteLine("Process has not been started");
            return;
        }
        if (_connection is null)
        {
            System.Console.WriteLine("Process has not finished initing");
        }
        await SendAsync(new UpdateXamlMessage
        {
            AssemblyPath = _params.targetPath,
            Xaml = xaml_path,
        });
    }
    public async Task SetScalingAsync(double scaling)
    {
        // _scaling = scaling;
        if (IsReady)
        {
            await SendAsync(new ClientRenderInfoMessage
            {
                DpiX = 96 * scaling,
                DpiY = 96 * scaling,
            });
        }
    }
}
