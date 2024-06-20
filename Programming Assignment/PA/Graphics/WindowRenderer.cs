using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace PA.Graphics;

public unsafe class WindowRenderer : IDisposable
{
    public const string InfoPanel = "Info";

    private readonly IWindow _window;

    private GL? gl;
    private IInputContext? inputContext;
    private ImGuiController? imGuiController;
    private IMouse? mouse;
    private IKeyboard? keyboard;
    private bool isInitialized;
    private bool firstFrame = true;

    public event Action? Load;
    public event Action<float>? Update;
    public event Action<float>? Render;

    public WindowRenderer(string title)
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Title = title;
        windowOptions.API = new GraphicsAPI()
        {
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            Version = new APIVersion(4, 6)
        };
        windowOptions.VSync = false;

        _window = Window.Create(windowOptions);
    }

    public GL GL => ThrowIfNotInitialized(gl);

    public ImGuiController ImGuiController => ThrowIfNotInitialized(imGuiController);

    public IMouse Mouse => ThrowIfNotInitialized(mouse);

    public IKeyboard Keyboard => ThrowIfNotInitialized(keyboard);

    public int Width => _window.Size.X;

    public int Height => _window.Size.Y;

    public void Run()
    {
        _window.Load += () =>
        {
            _window.Center();

            gl = _window.CreateOpenGL();
            inputContext = _window.CreateInput();
            imGuiController = new ImGuiController(gl, _window, inputContext);
            mouse = inputContext.Mice[0];
            keyboard = inputContext.Keyboards[0];

            isInitialized = true;

            Load?.Invoke();
        };

        _window.Update += delta =>
        {
            Update?.Invoke((float)delta);
        };

        _window.Render += delta =>
        {
            GL.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));
            GL.Viewport(0, 0, (uint)Width, (uint)Height);

            ImGuiController.Update((float)delta);
            {
                if (firstFrame)
                {
                    ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

                    firstFrame = true;
                }

                ImGui.DockSpaceOverViewport();

                Render?.Invoke((float)delta);

                if (ImGui.Begin(InfoPanel))
                {
                    ImGui.Text($"FPS : {ImGui.GetIO().Framerate}");

                    ImGui.End();
                }
            }
            ImGuiController.Render();
        };

        _window.Closing += () =>
        {
            imGuiController?.Dispose();
            inputContext?.Dispose();
            gl?.Dispose();
        };

        _window.Run();
    }

    public void Dispose()
    {
        _window.Dispose();

        GC.SuppressFinalize(this);
    }

    private T ThrowIfNotInitialized<T>(T? value)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Window not initialized yet.");
        }

        return value!;
    }
}
