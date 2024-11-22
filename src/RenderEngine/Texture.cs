// Filename: Texture.cs

using System;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

using Misc; // Logger.

namespace RenderEngine {

  /// <remarks> Loads an image and stores it as a texture with the GPU. </remarks>
  public class Texture {

    private int _handle; // Stores the texture in OpenGL memory.
    public int GetHandle(){
      return this._handle;
    }

    /// <summary> Activate the texture in GPU memory. </summary>
    public void Use(){
      GL.ActiveTexture(TextureUnit.Texture0); // This may not need to be actually 'dynamic'.
      GL.BindTexture(TextureTarget.Texture2D, this._handle);
    }

    /// <summary> Frees GPU memory, protected - inherited from IDisposable. </summary>
    private bool _disposed;
    protected virtual void Dispose(bool disposing){ // ignoring 'disposing'.
      if(!this._disposed){
        // -- // Anything Here? // -- //
        this._disposed = true;
      }
    }

    /// <remarks> Keeps track of resource leaks. </remarks>
    ~Texture(){
        if(!this._disposed){
            Logger.LogToFile("GPU resource leak from Texture: "+this._handle+" at "+DateTime.Now);
        }
    }

    /// <remarks> Frees GPU memory, public - inherited from IDisposable. </remarks>
    public void Dispose(){
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <remarks> We use a C# port of C's stb_image.h. </remarks>
    public Texture(String filename){
      if(!File.Exists(filename)){
        throw new FileNotFoundException("Couldn't locate texture file: "+filename);
      }
      this._handle = GL.GenTexture();
      GL.ActiveTexture(TextureUnit.Texture0);
      GL.BindTexture(TextureTarget.Texture2D, this._handle); // --
      // Load the image as a texture
      StbImage.stbi_set_flip_vertically_on_load(1); // OpenGL flips images vertically.
      ImageResult teximage = ImageResult.FromStream(File.OpenRead(filename), ColorComponents.RedGreenBlueAlpha);
      // Upload our texture to the GPU.
      GL.TexImage2D(
        TextureTarget.Texture2D, 0,
        PixelInternalFormat.Rgba,
        teximage.Width, teximage.Height,
        0, PixelFormat.Rgba, PixelType.UnsignedByte,
        teximage.Data);

      // Define how textures render.
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
      // Wrap mode.
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

      // --// Generate MipMap here? // -- //
    }
  }
}
