// Filename: Shader.cs
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

using Misc; // Logger.

namespace RenderEngine {

  /// <remarks>
  /// The Shader class is designed to instantiate objects in the GPU,
  /// hold their reference, and delete them when they're not needed, to avoid memory leaks.
  /// </remarks>
  public class Shader : IDisposable {

    private int _handle; // Stores the shader program in GPU memory.
    public int GetHandle(){
      return this._handle;
    }

    // OpenGL Uniforms section: pass CPU data to the GPU, like matrices.
    private readonly Dictionary<String, int> _uniformLocations;

    /// <summary> Runs the Shader 'program' in the GPU. </summary>
    public void Use(){
      GL.UseProgram(this._handle);
    }

    /// <remarks> At runtime, we can find the location reference. </remarks>
    public int GetAttribLocation(String attribName){
      return GL.GetAttribLocation(this._handle, attribName);
    }

    /// <remarks> Frees GPU memory, protected - inherited from IDisposable. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
        if (!this._disposed){
            GL.DeleteProgram(this._handle);
            this._disposed = true;
        }
    }

    /// <remarks> Keeps track of resource leaks. </remarks>
    ~Shader(){
        if(!this._disposed){
            Logger.LogToFile("GPU resource leak from Shader: "+this._handle+" at "+DateTime.Now);
        }
    }

    /// <remarks> Frees GPU memory, public - inherited from IDisposable. </remarks>
    public void Dispose(){
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary> Loads vertex and fragment shader data and combines them. </summary>
    public Shader(String vertexPath, String fragmentPath){
      if(!File.Exists(vertexPath) || !File.Exists(fragmentPath)){
        throw new FileNotFoundException("Couldn't load "+vertexPath+" or "+fragmentPath+".");
      }
      String vertexSource = File.ReadAllText(vertexPath);
      String fragmentSource = File.ReadAllText(fragmentPath);

      // Generate Shaders.
      int vertexShader = GL.CreateShader(ShaderType.VertexShader);
      GL.ShaderSource(vertexShader, vertexSource);
      int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
      GL.ShaderSource(fragmentShader, fragmentSource);

      int success; // Compile Shaders.

      GL.CompileShader(vertexShader);
      GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
      if(success == 0){ // Vertex.
        string infoLog = GL.GetShaderInfoLog(vertexShader);
        Logger.LogToFile("Issue with vertex shader compilation: "+infoLog);
        throw new GraphicsException(infoLog);
      }

      GL.CompileShader(fragmentShader);
      GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
      if(success == 0){ // Fragment.
        string infoLog = GL.GetShaderInfoLog(fragmentShader);
        Logger.LogToFile("Issue with fragment shader compilation: "+infoLog);
        throw new GraphicsException(infoLog);
      }

      // Generate GPU 'program'.
      this._handle = GL.CreateProgram();
      GL.AttachShader(this._handle, vertexShader); GL.AttachShader(this._handle, fragmentShader);
      GL.LinkProgram(this._handle);
      GL.GetProgram(this._handle, GetProgramParameterName.LinkStatus, out success);
      if(success == 0){
        string infoLog = GL.GetProgramInfoLog(this._handle);
        Logger.LogToFile("GPU program creation: "+infoLog);
        throw new GraphicsException(infoLog);
      }

      // Cleanup, now that the Shader is loaded into GPU memory.
      GL.DetachShader(this._handle, vertexShader);
      GL.DetachShader(this._handle, fragmentShader);
      GL.DeleteShader(vertexShader);
      GL.DeleteShader(fragmentShader);

      // Establish the GPU locations of uniforms while loading the Shader.
      this._uniformLocations = new Dictionary<String, int>();
      int uniformsNumber;
      GL.GetProgram(this._handle, GetProgramParameterName.ActiveUniforms, out uniformsNumber);
      for(int index = 0; index < uniformsNumber; index++){

        String key = GL.GetActiveUniform(this._handle, index, out _, out _);
        int location = GL.GetUniformLocation(this._handle, key);

        this._uniformLocations.Add(key, location);
      }
    }

    /// <summary> This section defines how to pass uniform data to the shader in GPU memory. </summary>
    public void SetMatrix4(string name, Matrix4 data){
      GL.UseProgram(this._handle);
      GL.UniformMatrix4(_uniformLocations[name], true, ref data);
    }
  }
}
