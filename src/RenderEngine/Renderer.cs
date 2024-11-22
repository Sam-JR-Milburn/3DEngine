// Filename: Renderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Windowing.Common.Input; // DEBUG: Mouse Cursor.
using Misc; // Logger.
using System.Text.Json;

namespace RenderEngine {

  /// <remarks> Parameterless class for JSON deserialisation. </remarks>
  public class ResourceStrings {
    public required String name { get; set; }
    public required String filename { get; set; }
  }

  /// <summary> Renderer: establish resources, render them. </summary>
  public class Renderer : IDisposable {
    /// <remarks> Container for Textures + Vertices. </remarks>
    private Dictionary<String, GraphicsObject> _graphicsObjects = new Dictionary<String, GraphicsObject>();
    /// <remarks> Simple pink texture if one fails to load. </remarks>
    private Texture _defaultTexture;

    /// <summary> Consistent references to OpenGL render resources. </summary>
    /// <remarks> No need for ElementBufferObject any longer. </remarks>
    private int _vertexBufferObject;
    private int _vertexArrayObject;

    /// <summary>
    /// GLSL program that takes 'model', 'view' and 'projection' matrices, drawing with a texture.
    /// </summary>
    private Shader _textureShader;

    /// <summary> Our primary camera in world-space. </summary>
    /// <remarks> Move me to GameRunner??? There should be a reference here, but yea. </remarks>
    private Camera _camera;

    /// <remarks> ... </remarks>
    private readonly String _resDir = "res/"; private readonly String _textureDir = "textures/";

    /// --
    private void LoadGraphicsObjects(String resourcesListFilename){
      if(String.IsNullOrEmpty(resourcesListFilename)){
        throw new ArgumentException("Renderer: invalid resource list filename.");
      }
      if(!File.Exists(resourcesListFilename)){
        Logger.LogToFile("Renderer: couldn't load the resources file.");
        throw new FileNotFoundException("Renderer: couldn't load the resources file.");
      }
      // Load here with JSON.
      String jsonStr = File.ReadAllText(resourcesListFilename);
      List<ResourceStrings>? resourceStrings = JsonSerializer.Deserialize<List<ResourceStrings>>(jsonStr);
      // --
      if(!(resourceStrings is List<ResourceStrings>)){
        String err = "LoadGraphicsObjects() couldn't deserialize JSON from "+resourcesListFilename+".";
        Logger.LogToFile(err); throw new NullReferenceException(err);
      }

      foreach(ResourceStrings rs in resourceStrings){
        Texture texture; String fullPath = this._resDir+this._textureDir+rs.filename;
        try {
          texture = new Texture(fullPath); // This can throw exceptions if it's not there.
        } catch {
          Logger.LogToFile("Renderer: LoadGraphicsObjects() couldn't load "+fullPath+".");
          texture = this._defaultTexture; // Pink 1x1 image to indicate missing texture.
        }
        this._graphicsObjects.Add(rs.name, new GraphicsObject(texture));
      }
    }

    /// <summary> Load the shader and texture assets from self-describing data. </summary>
    private void LoadAssets(){
      try {
        this._textureShader = new Shader( // Used for most rendering.
          this._resDir+"textureshader.vert",
          this._resDir+"textureshader.frag");
      } catch {
        Logger.LogToFile("Renderer: failed to load the texture shader."); throw;
      }

      /// <remarks> We need this if other textures fail to load. </remarks>
      try {
        this._defaultTexture = new Texture(this._resDir+this._textureDir+"pink.png");
      } catch {
        Logger.LogToFile("Renderer: failed to load default pink texture.");
        throw;
      }

      /// <remarks> This should be loaded as identity string:texture filename pairs </remarks>
      /*
      Texture texture;
      texture = new Texture(resDir+textureDir+"sandstone-1.jpg");
      this._graphicsObjects.Add("object-1", new GraphicsObject(texture));

      texture = new Texture(resDir+textureDir+"sandstone-2.png");
      this._graphicsObjects.Add("object-2", new GraphicsObject(texture));

      texture = new Texture(resDir+textureDir+"sandstone-3.png");
      this._graphicsObjects.Add("object-3", new GraphicsObject(texture));
      */

      /// <summary> Load GraphicsObject(s) with a resources file, and associate them with string names. </summary>
      this.LoadGraphicsObjects(this._resDir+"resources.json");
    }

    /// <summary> Builds the OpenGL render objects, and informs how to process data. </summary>
    /// <remarks> Only handling 3x position and 2x texture mapping vertices right now. </remarks>
    private void Initialise(){
      GL.ClearColor(0.1f,0.1f,0.1f,1.0f);

      // Build the VAO (Contains VBO refs).
      this._vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this._vertexArrayObject);
      // Build the Vertex Buffer (VBO).
      this._vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);

      /// <remarks> Inform OpenTK how to render this textured vertex data. </remarks>
      int vertexLocation      = this._textureShader.GetAttribLocation("aPosition");
      int texCoordLocation    = this._textureShader.GetAttribLocation("aTexCoord");

      GL.EnableVertexAttribArray(vertexLocation); // Object vertices [3D].
      GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 0);

      GL.EnableVertexAttribArray(texCoordLocation); // Texture vertices [2D].
      GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 3 * sizeof(float));

      GL.Enable(EnableCap.DepthTest); // Enable z-buffer.
    }

    /// <remarks> Clean up resources, track the state of that. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
      if(this._disposed){ return; }
      // Null all references.
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GL.UseProgram(0);
      // Delete all resources
      GL.DeleteBuffer(this._vertexBufferObject);
      GL.DeleteVertexArray(this._vertexArrayObject);
      // Delete the texture shader.
      GL.DeleteProgram(this._textureShader.GetHandle());

      this._disposed = true;
    }
    /// <summary> Publically accessible disposition. </summary>
    public void Dispose(){
      this.Dispose(true);
      GC.SuppressFinalize(this); // Prevent deconstruction before disposition.
    }
    /// <remarks> Just here to report deconstruction without disposition. </remarks>
    ~Renderer(){
      if(!this._disposed){
        Logger.LogToFile("GPU resource leak from Renderer "+"at "+DateTime.Now);
      }
    }






    // DEBUG: Move me to GameRunner ProcessInput()
    Vector2 lastMousePos = new Vector2(0,0);





    /// <summary> Renders a frame worth of graphics. </summary>
    /// <remarks> Should take some game state, and use OpenGL calls to represent that. </remarks>
    /// <remarks> Camera object and a container of 'graphics objects?' </remarks>
    public void Render(){
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Color *and* z-buffer.
      RenderWindow? window = RenderEngine.RenderWindow.GetInstance();
      if(window == null){ throw new InvalidOperationException("Renderer: No RenderWindow reference!"); }

      // MOVE ME TO GAMERUNNER
      KeyboardState input = window.KeyboardState; float speed = 0.25f;
      if(input.IsKeyDown(Keys.W)){
        this._camera.AddPosition(speed, 1.0f);
      }
      if(input.IsKeyDown(Keys.S)){
        this._camera.AddPosition(-speed, 1.0f);
      }
      if(input.IsKeyDown(Keys.A)){
        this._camera.AddPositionAngular(-speed, 1.0f);
      }
      if(input.IsKeyDown(Keys.D)){
        this._camera.AddPositionAngular(speed, 1.0f);
      }

      // Camera Look-Around // MOVE ME TO GAMERUNNER. //
      Vector2 mouse = window.MousePosition;
      float deltaX = mouse.X - this.lastMousePos.X;
      float deltaY = mouse.Y - this.lastMousePos.Y;
      this.lastMousePos = new Vector2(mouse.X, mouse.Y);
      float sensitivity = 0.02f;

      this._camera.AddYaw(deltaX * sensitivity);
      this._camera.AddPitch(-deltaY * sensitivity);

      // World Space -> Camera Space translation.
      Matrix4 view          = this._camera.GenerateView();
      Matrix4 projection    = this._camera.GetProjection();
      // These need to be set.
      this._textureShader.SetMatrix4("view", view);
      this._textureShader.SetMatrix4("projection", projection);
      // Activate GPU resources for drawing.
      GL.BindVertexArray(this._vertexArrayObject);

      // Draw test object 1.
      Matrix4 model = Matrix4.CreateTranslation(5.5f,-1.25f,5.5f);
      this._textureShader.SetMatrix4("model", model); // Local Space -> World Space translation.
      this._graphicsObjects["sandstone-1"].Draw(this._textureShader); // Actually draw.

      // Draw test object 2.
      model = Matrix4.CreateTranslation(7.5f,-6.25f,7.5f); // World-space.
      model *= Matrix4.CreateTranslation( // Moving back and forth.
        0.0f,
        (float)Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds/1000),
        0.0f);
      this._textureShader.SetMatrix4("model", model); // Local Space -> World Space translation.
      this._graphicsObjects["sandstone-2"].Draw(this._textureShader); // Actually draw.

      // Draw test object 3.
      model = Matrix4.CreateTranslation(-5.5f,-1.25f,5.5f);
      model *= Matrix4.CreateRotationX(45.0f);
      model *= Matrix4.CreateRotationY(45.0f);
      this._textureShader.SetMatrix4("model", model); // Local Space -> World Space translation.
      this._graphicsObjects["sandstone-3"].Draw(this._textureShader); // Actually draw.

      window.SwapBuffers(); // Swap buffers: render with the window.
    }

    /// <remarks> Pass the aspect ratio through constructor chaining. </remarks>
    public Renderer(int width, int height) : this(45.0f, (float)(width / height)){
      if(height == 0){ throw new DivideByZeroException(); }
      if(width < 128 || height < 128){ throw new ArgumentException("The height or width can't be less than 128."); }
    }

    /// <remarks> If we want custom FOV. </remarks>
    public Renderer(int width, int height, float fov) : this(fov, (float)(width/height)){
      if(height == 0){ throw new DivideByZeroException(); }
      if(width < 128 || height < 128){ throw new ArgumentException("The height or width can't be less than 128."); }
    }

    /// <summary> Build and establish the rendering portion of the RenderEngine </summary>
    public Renderer(float fov, float aspectRatio){
      try {
        this.LoadAssets();
      } catch {
        Logger.LogToFile("Renderer failed with LoadAssets.");
        throw;
      }
      // This shouldn't happen, but the compiler won't shut up about references on exit.
      if(!(this._textureShader is Shader)){
        throw new NullReferenceException("Renderer: texture shader is not instantiated. ");
      }
      // This also shouldn't happen, but likewise the compiler won't shut up.
      if(!(this._defaultTexture is Texture)){
        throw new NullReferenceException("Renderer: default texture is not instantiated. ");
      }

      /// <remarks> Consider: saved games where this will need to change. </remarks>
      /// <remarks> It will be better for GameEngine to hold this at some point. </remarks>
      this._camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f), fov, aspectRatio);

      this.Initialise(); // Build the OpenGL GPU resources.
    }
  }
}
