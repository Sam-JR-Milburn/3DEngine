// Filename: IWindowObserver.cs

namespace RenderEngine {
  /// <summary>
  /// Used by the Window to send signals to observers/subscribers.
  /// </summary>
  public interface IWindowObserver {
    public void Notify(String message);
  }
}
