using PurrNet;
using UnityEngine;

public class FreezableWater : NetworkBehaviour
{
    [Header("References")]
    public Renderer waterRenderer;
    public Material waterMaterial;
    public Material iceMaterial;
    public Collider iceCollider; // drag WaterMesh here

    [Header("Settings")]
    public SpellProjectile.SpellType freezeSpell = SpellProjectile.SpellType.Iceball;
    public SpellProjectile.SpellType meltSpell = SpellProjectile.SpellType.Fireball;

    private bool _frozen = false;

    private void Start()
    {
        iceCollider.enabled = false; // not walkable by default
    }

    private void OnTriggerEnter(Collider other)
    {
        SpellProjectile spell = other.GetComponent<SpellProjectile>();
        if (spell == null) return;

#if UNITY_EDITOR
        HandleSpell(spell);
        return;
#endif
        if (isServer) HandleSpell(spell);
    }

    private void HandleSpell(SpellProjectile spell)
    {
        if (!_frozen && spell.spellType == freezeSpell)
            FreezeRpc();
        else if (_frozen && spell.spellType == meltSpell)
            MeltRpc();
    }

    [ObserversRpc]
    private void FreezeRpc()
    {
        _frozen = true;
        waterRenderer.material = iceMaterial;
        iceCollider.enabled = true;  // now you can walk on it
    }

    [ObserversRpc]
    private void MeltRpc()
    {
        _frozen = false;
        waterRenderer.material = waterMaterial;
        iceCollider.enabled = false; // falls through again
    }
}