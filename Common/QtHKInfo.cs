using AEAssist.CombatRoutine.View.JobView;

namespace ElliotZ;

public record QtInfo(string Name, 
                     string EnName, 
                     bool DefVal, 
                     Action<bool>? Callback, 
                     string Tooltip);

public record HotKeyInfo(string Name, 
                         string EnName, 
                         IHotkeyResolver Hkr);