using DemoFile.Extensions;

namespace DemoFile.Sdk;

public partial class CCSPlayerPawn
{
    public IEnumerable<CCSWeaponBase> Weapons =>
        WeaponServices?.MyWeapons.Select(handle => handle.Get<CCSWeaponBase>(Demo)).WhereNotNull()
        ?? Enumerable.Empty<CCSWeaponBase>();

    public IEnumerable<CBaseCSGrenade> Grenades =>
        Weapons.Select(weapon => weapon as CBaseCSGrenade).Where(x => x != null)!;

    public CCSPlayerController? CSController => Controller.Get<CCSPlayerController>(Demo);

    public InputButtons InputButtons => MovementServices is {} csMovement
        ? (InputButtons) csMovement.ButtonDownMaskPrev
        : default;
}
