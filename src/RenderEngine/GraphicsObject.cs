// Filename: GraphicsObject.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Misc; // Logger.

namespace RenderEngine {

  /// <remarks> Represents the form, and texture, of an object. </remarks>
  public class GraphicsObject {
    /// <summary> Local-space position vertices + UV mapping + texture to map to. </summary>
    private float[] _vertices;
    private Texture _texture;

    /// <summary> Draw the textured object in space. </summary>
    /// <remarks> Will not work without the shader being supplied it's matrices. </remarks>
    public void Draw(Shader targetShader){
      if(targetShader == null){ throw new ArgumentException("GraphicsObject: Invalid target shader on Draw()."); }
      // Activate texture and shader.
      this._texture.Use(); targetShader.Use();
      // Pass data to buffer, draw.
      GL.BufferData(BufferTarget.ArrayBuffer,
        this._vertices.Length * sizeof(float),
        this._vertices,
        BufferUsageHint.DynamicDraw);
      int objectSize = this._vertices.Length / 5; // 5: 3x position, 2x texture UV.
      GL.DrawArrays(PrimitiveType.Triangles, 0, objectSize);
    }

    /// <remarks> ... </remarks>
    public GraphicsObject(String textureFilename){
      /// <remarks> Delete me later... </remarks>
      float[] manualVertices = {
        -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
         0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
         0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
         0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
        -0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
        -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
         0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 1.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 1.0f,
        -0.5f,  0.5f,  0.5f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
        -0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
        -0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
         0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
         0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
         0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
         0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
         0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
         0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
        -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
         0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.5f, 1.0f, 0.0f,
        -0.5f,  0.5f,  0.5f, 0.0f, 0.0f,
        -0.5f,  0.5f, -0.5f, 0.0f, 1.0f
      };
      /// <remarks> This should be loaded from file in the future </remarks>
      this._vertices = manualVertices; // Workaround with the initialiser.

      try { // Above is just temporary, until Obj style files can be worked out.
          this._texture = new Texture(textureFilename); // This *can* throw exceptions.
      } catch {
        Logger.LogToFile("GraphicsObject: Couldn't load texture ["+textureFilename+"].");
        throw;
      }
      // EOF Constructor
    }
  }
}
