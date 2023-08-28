namespace Extensions;

public static class ZNetSceneExtension
{
    public static ItemDrop GetPrefab(this ZNetScene zNetScene, string name)
    {
        return zNetScene.GetPrefab(name)?.GetComponent<ItemDrop>();
    }

    public static ItemDrop GetPrefab(this ZNetScene zNetScene, int hash)
    {
        return zNetScene.GetPrefab(hash)?.GetComponent<ItemDrop>();
    }
}