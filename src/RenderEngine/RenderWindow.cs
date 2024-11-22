// Filename: RenderWindow.cs
using System;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics; // Just for Vector2i.
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Misc; // Logger.

namespace RenderEngine {

  public class RenderWindow : GameWindow {

    /// <remarks>
    /// RenderWindow is used like a singleton, it probably should act like one.
    /// </remarks>
    private static RenderWindow? _instance;
    public static RenderWindow? GetInstance(){
      return RenderWindow._instance;
    }

    /// <remarks>
    /// Not as protected as it should be, but the Window
    /// should have a consistent reference, and be accessible from everywhere.
    /// </remarks>
    public static RenderWindow InitialiseInstance(NativeWindowSettings nws, double framerate){
      if(!(RenderWindow._instance is RenderWindow)){
        RenderWindow._instance = new RenderWindow(nws, framerate);
      }
      return RenderWindow._instance;
    }

    /// <summary>
    /// Message observers: communicate to modules interested in RenderWindow messages.
    /// </summary>
    private List<IWindowObserver> _observers = new List<IWindowObserver>();
    private void SendToAllObservers(String message){
      foreach(IWindowObserver iwo in this._observers){
        if(iwo != null){ iwo.Notify(message); }
      }
    }
    public void AddObserver(IWindowObserver iwo){
      this._observers.Add(iwo);
    }

    /// <summary> Graphics calls are made to the renderer class. </summary>
    /// <remarks> Handles initialisation, resource management and rendering. </remarks>
    private Renderer? _renderer;

    /// <remarks> Called when the Window is first instantiated. </remarks>
    protected override void OnLoad(){
      base.OnLoad();
      this.SwapBuffers();
      this.CursorState = CursorState.Grabbed; // Makes the mouse invisible when the window is up.
      // Resulting aspect ratio will change depending on taskbar orientation and 'fullscreenity'.
      Vector2i clientSize = this.ClientSize;
      try {
        this._renderer = new Renderer(clientSize.X, clientSize.Y); // Initialises the renderer.
      } catch {
        Logger.LogToFile("Couldn't load Renderer in RenderWindow.");
        throw;
      }
    }

    /// <summary> Cleanup all components that need cleanup. </summary>
    protected override void OnUnload(){
      base.OnUnload();
      if(this._renderer != null){
        this._renderer.Dispose(); // OpenGL references, textures etc.
      }
    }

    /// <summary> Render graphics from the game engine's state. </summary>
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e); // Necessary.
      if(this._renderer != null){
        this._renderer.Render();
      }
    }

    /// <summary> Adjust the OpenGL viewport to some new size. </summary>
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e){
      base.OnFramebufferResize(e);
      GL.Viewport(0, 0, e.Width, e.Height);
    }

    /// <remarks>
    /// Override Run(): close other threads when close button is clicked.
    /// </remarks>
    public override void Run(){
      base.Run(); // When .Close() is called, program control will move past this.
      this.SendToAllObservers("WINDOWCLOSE"); // Send signal to GameRunner.
    }

    /// <summary>
    /// RenderWindow: Settings passed externally via nws and framerate.
    /// private: Singleton pattern to-be-called via InitialiseInstance(*).
    /// </summary>
    private RenderWindow(NativeWindowSettings nws, double framerate) : base (
      GameWindowSettings.Default, nws){
        base.UpdateFrequency = framerate; // 60.0 hz/fps: adjustable.
    }
  }
}
