using AEAssist.CombatRoutine.Module;
using ElliotZ.Rpr.SlotResolvers.GCD;
using ElliotZ.Rpr.SlotResolvers.oGCD;

namespace ElliotZ.Rpr;

public static class RotationPrioSys
{
    public static readonly List<SlotResolverData> SlotResolvers =
    [
        // GCD
        new(new EnshroudSk(), SlotMode.Gcd),
        new(new GibGall(), SlotMode.Gcd),
        new(new PerfectioHighPrio(), SlotMode.Gcd),
        new(new HarvestMoonHighPrio(), SlotMode.Gcd),
        new(new SoulSow(), SlotMode.Gcd),
        new(new BuffMaintain(), SlotMode.Gcd),
        new(new GaugeGainCD(), SlotMode.Gcd),
        new(new Perfectio(), SlotMode.Gcd),
        new(new PlentifulHarvest(), SlotMode.Gcd),
        new(new Base(), SlotMode.Gcd),
        new(new HarvestMoon(), SlotMode.Gcd),
        new(new Harpe(), SlotMode.Gcd),

        // oGCD
        new(new EnshroudAb(), SlotMode.OffGcd),
        new(new Sacrificum(), SlotMode.OffGcd),
        new(new EnshroudHighPrio(), SlotMode.OffGcd),
        new(new ArcaneCircle(), SlotMode.OffGcd),
        new(new TrueNorth(), SlotMode.OffGcd),
        new(new Gluttony(), SlotMode.OffGcd),
        new(new Enshroud(), SlotMode.OffGcd),
        new(new BloodStalk(), SlotMode.OffGcd),
        new(new AutoCrest(), SlotMode.OffGcd),
        new(new AutoFeint(), SlotMode.OffGcd),
        new(new AutoBloodBath(), SlotMode.OffGcd),
        new(new AutoSecondWind(), SlotMode.OffGcd),

        // Low Prio Always
        new(new Ingress(), SlotMode.Always),
    ];
    

}