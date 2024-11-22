// Filename: Camera.cs

using OpenTK.Mathematics;
using Misc; // Logger.

namespace RenderEngine {

  /// <summary> Control the camera in world-space. </summary>
  public class Camera {
    /// <remarks> Vector constants for OpenGL's axes. </remarks>
    private Vector3 _cameraFront            = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 _cameraUp               = new Vector3(0.0f, 1.0f,  0.0f);
    private Vector3 _cameraRight            = Vector3.Normalize(Vector3.Cross(
      new Vector3(0.0f, 0.0f, -1.0f),   // Field initialiser: 'Camera Front'
      new Vector3(0.0f, 1.0f, 0.0f)));  // Field initialiser: 'Camera Up'

    /// <summary> Position in world-space. </summary>
    private Vector3 _cameraPosition         = new Vector3(0.0f, 0.0f, 0.0f);
    public void MoveToPosition(Vector3 newPosition){
      this._cameraPosition = newPosition;
    }
    /// <remarks> Should be tied to time, not the frame-rate. </remarks>
    public void AddPosition(float deltaSpeed, float deltaTime){
      this._cameraPosition += this._cameraFront * deltaSpeed * deltaTime;
    }
    public void AddPositionAngular(float deltaSpeed, float deltaTime){
      this._cameraRight = Vector3.Normalize(Vector3.Cross(this._cameraFront, this._cameraUp)); // Readjust this
      this._cameraPosition += this._cameraRight * deltaSpeed * deltaTime;
    }

    /// <summary> Euler angles adjust the camera view matrix. </summary>
    /// ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ----

    /// <remarks> Yaw: rotation around y-axis. </remarks>
    private float _yaw    = 0.0f;
    public float GetYaw(){ return this._yaw; }
    public void AddYaw(float deltaYaw){
      this._yaw += deltaYaw;
    }
    /// SetYaw?

    /// <remarks> Pitch: rotation around x-axis. </remarks>
    private float _pitch  = 0.0f;
    public float GetPitch(){ return this._pitch; }
    public void AddPitch(float deltaPitch){
      if(this._pitch > 89.9f && deltaPitch > 0.0f){ return; }   // Don't look more than 'up'.
      if(this._pitch < -89.9f && deltaPitch < 0.0f){ return; }  // Don't look more than 'down'.
      this._pitch += deltaPitch;
    }
    /// SetPitch?

    /// <remarks> Field of view: ... </remarks>
    private float _fov = 45.0f;
    public void SetFov(float fov){
      if(fov <= 0.0f || fov >= 180.0f){
        throw new ArgumentException("FOV can't exceed 180 degrees or be less than 0 degrees.");
      }
      this._fov = fov;
    }

    // Sniper scope-like functionality.
    /*
    public void AddFov(float deltaFov){
      if(this._fov+deltaFov >= 180.0f || this._fov+deltaFov <= 0.0f){
        return;
      }
      this._fov += deltaFov;
      this.ResetProjection();
    }
    */

    /// ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ----

    /// <remarks> Need to pass this to the camera. </remarks>
    private float _aspectRatio;
    public void SetAspectRatio(float aspectRatio){
      if(aspectRatio <= 0.0f){
        throw new ArgumentException("Camera: aspect ratio below or equal to 0.");
      }
      this._aspectRatio = aspectRatio;
    }
    public void SetAspectRatio(int width, int height){
      if(width <= 0 || height <= 0){
        throw new ArgumentException("Camera: invalid width or height. [Width: "+width+", Height: "+height+"]");
      }
      this._aspectRatio = width / height;
    }
    public float GetAspectRatio(){
      return this._aspectRatio;
    }

    /// <summary> Translation matrix for the camera view. </summary>
    private Matrix4 _view;
    public Matrix4 GenerateView(){
      // Adjust camera direction.
      this._cameraFront.X =
        (float)Math.Cos(MathHelper.DegreesToRadians(this._pitch)) *
        (float)Math.Cos(MathHelper.DegreesToRadians(this._yaw));
      this._cameraFront.Y =
        (float)Math.Sin(MathHelper.DegreesToRadians(this._pitch));
      this._cameraFront.Z =
        (float)Math.Cos(MathHelper.DegreesToRadians(this._pitch)) *
        (float)Math.Sin(MathHelper.DegreesToRadians(this._yaw));
      this._cameraFront = Vector3.Normalize(this._cameraFront);

      // Generate view matrix.
      this._view = Matrix4.LookAt(
        this._cameraPosition,
        this._cameraPosition+this._cameraFront,
        this._cameraUp
      );
      return this._view;
    }

    /// <summary> Translation matrix for culling-space (screen). </summary>
    private Matrix4 _projection;
    private void ResetProjection(){
      this._projection = Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(this._fov),
        this._aspectRatio, 0.01f, 100.0f);
    }
    public Matrix4 GetProjection(){
      return this._projection;
    }

    /// <remarks> ... </remarks>
    public Camera(float fov, float aspectRatio) : this(new Vector3(0.0f,0.0f,0.0f), fov, aspectRatio){}
    /// <summary> Generate a camera in world-space. </summary>
    public Camera(Vector3 cameraPosition, float fov, float aspectRatio){
      try {
        this._cameraPosition = cameraPosition;
        this.SetFov(fov);
        this.SetAspectRatio(aspectRatio);
        this._projection = Matrix4.CreatePerspectiveFieldOfView(
          MathHelper.DegreesToRadians(this._fov),
          this._aspectRatio, 0.01f, 100.0f);
      } catch {
        Logger.LogToFile("Can't instantiate Camera object.");
        throw;
      }
    }
  }
}
