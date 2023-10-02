using UnityEngine;

public class LayerManager : Singleton<LayerManager>
{
    [field: SerializeField] public LayerMask EnemyLayer { get; private set; }
    [field: SerializeField] public LayerMask GrazeLayer { get; private set; }
    [field: SerializeField] public LayerMask PlayerBulletLayer { get; private set; }
    [field: SerializeField] public LayerMask KickLayer { get; private set; }
    [field: SerializeField] public LayerMask PlayerHurtboxLayer { get; private set; }
    [field: SerializeField] public LayerMask LanternLayer { get; private set; }

    public static bool IsInLayer(int layer, LayerMask _layerMask)
    {
        return _layerMask == (_layerMask | (1 << layer));
    }
}
