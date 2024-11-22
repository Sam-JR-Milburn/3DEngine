// Filename: GraphicsException.cs

namespace RenderEngine {
  /// <remarks>
  /// No special functionality, just wanted to handle exceptions that aren't raised with OpenTK.
  /// </remarks>
  public class GraphicsException : Exception {
    public GraphicsException() {}
    public GraphicsException(string message) : base(message) {}
    public GraphicsException(string message, Exception inner) : base(message, inner) {}
  }
}
