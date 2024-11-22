// Filename: Logger.cs
using System;

namespace Misc {

  /// <summary> Report error data by simply appending to a file.</summary>
  public class Logger {

    private static bool _firstCommit = true;
    public static void LogToFile(String message){
      if(Logger._firstCommit){
        File.AppendAllText("logfile", "[Logging started at: "+DateTime.Now+"]\n");
        Logger._firstCommit = false;
      }
      File.AppendAllText("logfile", message+"\n"); // The actual log message.
    }
  }
}
