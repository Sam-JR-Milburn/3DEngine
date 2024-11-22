// Filename: InitMain.cs
using System;
using System.Json;
using System.IO;
using System.Threading; // Calls the GameEngine loop.

// For the MonitorInfo functionality.
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

using GameEngine;
using RenderEngine;

using Misc; // Logger.

namespace Initialisation {

  public class InitMain {
    /// <summary> Keep track of the monitor we're on, reassign if necessary. </summary>
    /// <remarks> Needs to be called before other initialisation functions. </remarks>
    private static MonitorInfo _currentMonitorInfo = Monitors.GetPrimaryMonitor();
    private static bool SetMonitorInfo(NativeWindow nw){
      if(nw == null){ return false; }
      InitMain._currentMonitorInfo = Monitors.GetMonitorFromWindow(nw);
      return true;
    }

    /// <remarks> </remarks>
    private static bool _settingsOverride = false;

    /// <summary> Our default options from the settings.json file. </summary>
    private static NativeWindowSettings _windowSettingsDefault =
      new NativeWindowSettings(){
        ClientSize = (500,500),
        Title = "ErrorTitle",
        WindowState = WindowState.Maximized
    };
    public static NativeWindowSettings GetSettingsDefault(){
      return InitMain._windowSettingsDefault;
    }

    /// <summary>
    /// Custom, overriding options from the settings.json file: Nullable.
    /// </summary>
    private static NativeWindowSettings _windowSettingsCustom = InitMain._windowSettingsDefault;
    public static NativeWindowSettings GetSettingsCustom(){
      return InitMain._windowSettingsCustom;
    }

    /// <summary> Parse a block of JSON settings. </summary>
    private static NativeWindowSettings ParseSettingsTree(JsonValue settingsTree){
      // Check for invalid input.
      if(settingsTree == null){
        throw new ArgumentException("Invalid settings argument encountered while parsing.");
      }
      // Dynamic Settings: Window Size from MonitorInfo.
      int width   = InitMain._currentMonitorInfo.HorizontalResolution;
      int height  = InitMain._currentMonitorInfo.VerticalResolution;

      // Process the enum. I know this is finicky with JSON, but it works for now.
      String wstateRaw = settingsTree["WindowState"].ToString().Replace("\"", String.Empty);
      WindowState wstate; // Non-nullable.
      if(!Enum.TryParse<WindowState>(wstateRaw, out wstate)){
        throw new FormatException("Couldn't parse WindowState enum.");
      }
      // Instantiate, return.
      NativeWindowSettings nws = new NativeWindowSettings(){
        ClientSize = (width, height),
        Title = (String)settingsTree["Title"],
        WindowState = wstate
      };
      return nws;
    }

    /// <summary> Load settings from settings.json. </summary>
    protected static void LoadOptionsFromFile(String filename){
      // Check we have a valid json filename, that can be accessed.
      if(String.IsNullOrEmpty(filename) || !filename.ToLower().EndsWith(".json")){
        throw new ArgumentException("Invalid filename argument.");
      }
      if(!File.Exists(filename)){
        throw new FileNotFoundException("Filename provided is not a file.");
      }

      String jsonRaw; // Parse.
      JsonValue settingsJson;
      try {
        jsonRaw = File.ReadAllText(filename);
        settingsJson = JsonValue.Parse(jsonRaw);
      } catch { throw; }

      JsonValue settingsBlock; // Assign the NativeWindowSettings.

      // Does the custom override the default? Check.
      if(settingsJson.ContainsKey("customOverride")){
        String customOverride = settingsJson["customOverride"].ToString();
        if(!String.IsNullOrEmpty(customOverride)){
          customOverride = customOverride.Replace("\"", String.Empty);
          if(customOverride.Equals("true")){ InitMain._settingsOverride = true; }
          // Elsewise, do nothing.
        }
      }

      // Load our default. Necessary.
      if(!settingsJson.ContainsKey("default")){
        throw new ArgumentException("settings.json needs a default settings block.");
      }
      settingsBlock = settingsJson["default"];
      InitMain._windowSettingsDefault = InitMain.ParseSettingsTree(settingsBlock);

      // Load our custom. Not-necessary.
      if(settingsJson.ContainsKey("custom")){
        settingsBlock = settingsJson["custom"];
        InitMain._windowSettingsCustom = InitMain.ParseSettingsTree(settingsBlock);
      }
    }

		/// <summary>
    /// Process initial arguments, collect system information.
    /// </summary>
		public static void Main(String[] args){
      // Load settings file.
      try {
        InitMain.LoadOptionsFromFile("settings.json");
      } catch(Exception e){
        Logger.LogToFile("InitMain: Couldn't parse settings.json.");
        Logger.LogToFile(e.ToString());
        return;
      }

      RenderWindow window; // OpenGL Rendering must be called from the Main thread.
      if(InitMain._settingsOverride){
        window = RenderWindow.InitialiseInstance(InitMain.GetSettingsCustom(), 60.0);
      } else {
        window = RenderWindow.InitialiseInstance(InitMain.GetSettingsDefault(), 60.0);
      }
      window.CenterWindow();
      // Pass the window reference to the GameEngine first.
      GameRunner runner = new GameRunner();
      window.AddObserver(runner);
      // Game logic run in a separate thread, communicating through the observer pattern.
      new Thread(() => { runner.Run(); }).Start();
      // -- // If there are other threads, like sound FX, call them here. // -- //
      // Run Window after setting up multithreading.
      window.Run();
		}
    // EOF InitMain.
	}
}
