using Extensions;

namespace MerchantSpawnTweaks;

public static class ZDOExtension
{
    public static int Get_ID(this ZDO zdo) => zdo.GetPosition().RoundCords().GetHashCode() + zdo.GetPrefab();
}