// ReSharper disable FieldCanBeMadeReadOnly.Global
namespace ElliotZ.Common.ModernJobViewFramework.HotKey;

public struct HotKeySpell(string n, uint s, int t)
{
    public string Name = n;
    public uint Spell = s;
    public int Target = t;
}