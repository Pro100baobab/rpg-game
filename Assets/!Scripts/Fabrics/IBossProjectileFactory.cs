using UnityEngine;


// Возвращает префаб и устанавливает материал по стихии (для снарядов босса)
public interface IBossProjectileFactory
{
    GameObject CreateProjectile(IElement.Elements element, Vector3 position, Quaternion rotation, Transform parent = null);
}

public class BossProjectileFactory : IBossProjectileFactory
{
    private readonly GameObject stonePrefab;
    private readonly Material[] stoneMaterials;

    public BossProjectileFactory(GameObject stonePrefab, Material[] stoneMaterials)
    {
        this.stonePrefab = stonePrefab;
        this.stoneMaterials = stoneMaterials;
    }

    public GameObject CreateProjectile(IElement.Elements element, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (stonePrefab == null) return null;

        var go = Object.Instantiate(stonePrefab, position, rotation, parent);
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null && stoneMaterials != null && (int)element < stoneMaterials.Length)
        {
            renderer.material = stoneMaterials[(int)element];
        }
        return go;
    }
}