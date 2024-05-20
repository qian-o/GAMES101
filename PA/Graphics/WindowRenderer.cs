﻿using Silk.NET.Input;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using SDLWindow = Silk.NET.SDL.Window;
using SilkWindow = Silk.NET.Windowing.Window;

namespace PA.Graphics;

public unsafe class WindowRenderer : IDisposable
{
    private readonly IWindow _window;
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;
    private readonly IInputContext _inputContext;
    private readonly IMouse _mouse;
    private readonly IKeyboard _keyboard;

    public event Action? Load;
    public event Action<double>? Update;
    public event Action<double>? Render;

    public WindowRenderer()
    {
        _window = SilkWindow.Create(WindowOptions.DefaultVulkan);
        _sdl = Sdl.GetApi();

        _window.Initialize();

        _renderer = _sdl.CreateRenderer((SDLWindow*)_window.Native!.Sdl!, -1, (int)RendererFlags.Accelerated);
        _inputContext = _window.CreateInput();
        _mouse = _inputContext.Mice[0];
        _keyboard = _inputContext.Keyboards[0];
    }

    public Sdl Sdl => _sdl;

    public Renderer* Renderer => _renderer;

    public int Width => _window.Size.X;

    public int Height => _window.Size.Y;

    public IMouse Mouse => _mouse;

    public IKeyboard Keyboard => _keyboard;

    public void Run()
    {
        Load?.Invoke();

        _window.Update += delta => Update?.Invoke(delta);

        _window.Render += delta =>
        {
            Render?.Invoke(delta);

            _sdl.RenderPresent(_renderer);
        };

        _window.Run();
    }

    public void Dispose()
    {
        _sdl.DestroyRenderer(_renderer);
        _sdl.Dispose();

        _window.Close();
        _window.Dispose();

        GC.SuppressFinalize(this);
    }
}