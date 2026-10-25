using System.Runtime.InteropServices;
using System.Text.Json;

namespace depo;

public static class DepoTool {
  public static string ninja   = path_to("ninja") ?? throw new Exception("Ninja not found");
  public static string sz      = path_to("7z") ?? throw new Exception("7z not found");
  public static string vswhere = path_to("vswhere");

  private static string path_to(string tool_name) {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      tool_name += ".exe";
    } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
      return find_in_path(tool_name);
    }
    return Path.Join(AppContext.BaseDirectory, "depo-tools", tool_name);
  }

  private static string find_in_path(string tool) {
    var path = Environment.GetEnvironmentVariable("PATH");
    if (path == null) {
      return null;
    }
    var dirs = path.Split(':');
    foreach (var dir in dirs) {
      var test = Path.Join(dir, tool);
      if (File.Exists(test)) {
        return test;
      }
    }
    return null;
  }
}

public record class DepoCreds(string login, string password);

public static class DepoSettings {
  public static string config_dir  = get_config_dir();
  public static string msvc_env    = Path.Join(config_dir, "msvc_env");
  public static string creds_path  = Path.Join(config_dir, "creds.json");

  private static string get_config_dir() {
    var settings = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var dir      = Path.Join(settings, "depo");
    Log.debug("Settings path: {0}", dir);
    if (!Directory.Exists(dir)) {
      Directory.CreateDirectory(dir);
    }
    return dir;
  }

  public static DepoCreds get_creds() {
    try {
      var creds = JsonSerializer.Deserialize(File.ReadAllText(creds_path), TheJsonContext.Default.DepoCreds);
      if (string.IsNullOrEmpty(creds.login)) {
        Log.error("Credentials file empty ({0}), please fill it!", creds_path);
      }
      return creds;
    } catch {
      var default_creds = new DepoCreds(login: "", password: "");
      if (!File.Exists(creds_path)) {
        Log.error("Credentials file does not exists ({0}), creating empty!", creds_path);
        File.WriteAllText(creds_path, JsonSerializer.Serialize(default_creds, TheJsonContext.Default.DepoCreds));
      }
      return default_creds;
    }
  }
}
