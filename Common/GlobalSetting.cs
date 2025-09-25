using System.Numerics;
using AEAssist.Helper;
using AEAssist.IO;

// ReSharper disable MemberCanBePrivate.Global
namespace ElliotZ;

public class GlobalSetting {
  public static GlobalSetting? Instance;
  
  public bool HotKey配置窗口 = false;
  public bool QtShow = true;
  public bool HotKeyShow = true;
  public bool TempQtShow = true;
  public bool TempHotShow = true;
  public bool Qt快捷栏随主界面隐藏 = true;
  public bool 缩放同时隐藏qt = false;
  public bool 缩放同时隐藏Hotkey = false;
  public Vector2 缩放后窗口大小 = new(225f, 100f);
  public bool 关闭动效 = false;
  //public float configHeight = 500f;

  private static string _path = "";
  
  public static void Build(string settingPath, bool rebuild) {
    if (!rebuild) {
      if (Instance != null) {
        return;
      }
    }

    Init(settingPath);
  }

  private static void Init(string settingPath) {
    _path = Path.Combine(settingPath, "GlobalSettings.json");

    if (!File.Exists(_path)) {
      Instance = new GlobalSetting();
      Instance.Save();
      return;
    }

    try {
      Instance = JsonHelper.FromJson<GlobalSetting>(File.ReadAllText(_path));
    } catch (Exception ex) {
      Instance = new GlobalSetting();
      LogHelper.Error(ex.ToString());
    } finally {
      Instance.TempQtShow = true;
      Instance.TempHotShow = true;
    }
  }

  public void Save() {
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(_path)
                             ?? throw new InvalidOperationException());
      File.WriteAllText(_path, JsonHelper.ToJson(this));
    } catch (Exception ex) {
      LogHelper.Error(ex.ToString());
    }
  }
}
