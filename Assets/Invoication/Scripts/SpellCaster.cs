using UnityEngine;
using PurrNet;

public class SpellCaster : NetworkBehaviour
{
    public GameObject fireballPrefab;
    public GameObject blazingImpactPrefab;
    public GameObject iceballPrefab;
    public GameObject fireWallPrefab;
    public GameObject emberCirclePrefab;
    public GameObject iceWallPrefab;
    public GameObject lightningBoltPrefab;

    [Header("Shockwave")]
    public float shockwaveRadius = 6f;
    public float shockwaveDamage = 40f;
    public GameObject shockwaveVFXPrefab;
    public float shockwaveVFXDuration = 1.5f;

    public Transform fireballSpawnPoint;
    public float fireballSpeed = 40f;
    public float iceballSpeed = 30f;
    public float fireWallSpeed = 12f;
    public float emberCircleSpeed = 15f;
    public float iceWallSpeed = 12f;
    public float lightningBoltSpeed = 50f;

    [Header("Spell Text Popup")]
    public GameObject textPopupPrefab;
    public float textFloatSpeed = 1f;
    public float textFadeDuration = 2f;

    [System.NonSerialized]
    public LayerMask aimLayers = ~0;

    private RaycastPickup pickupScript;
    private PlayerAnimationController anim;

    public KeyCode castKey = KeyCode.Mouse0;

    void Start()
    {
        pickupScript = GetComponent<RaycastPickup>();
        anim = GetComponent<PlayerAnimationController>();

        if (pickupScript == null)
            Debug.LogError("RaycastPickup script not found!");
    }

    void Update()
    {
        if (!isOwner) return;

        if (Input.GetKeyDown(castKey) && IsHoldingStaff())
        {
            if (!SkillTreeBridge.IsUnlocked("IceSpike_0"))
            {
                Debug.Log("Ice Spell is locked!");
                return;
            }

            CastIceball();
        }
    }

    bool IsHoldingStaff()
    {
        if (pickupScript != null && pickupScript.heldItem != null)
        {
            string itemName = pickupScript.heldItem.name.ToLower();
            return itemName.Contains("staff") || itemName.Contains("wand") || itemName.Contains("magic");
        }

        return false;
    }

    void PlayCastAnimation()
    {
        if (anim == null) return;

        if (isServer)
            anim.PlayAttack();
        else
            anim.PlayAttack_ServerRPC();
    }

    void LaunchProjectile_Server(GameObject prefab, float speed, Vector3 spawnPos, Vector3 direction, GameObject player)
    {
        if (!isServer) return;
        if (prefab == null || player == null) return;

        Quaternion spawnRotation = Quaternion.LookRotation(direction);

        // Fix lightning orientation and height
        SpellProjectile spCheck = prefab.GetComponent<SpellProjectile>();
        if (spCheck != null && spCheck.spellType == SpellProjectile.SpellType.LightningStrike)
        {
            spawnRotation *= Quaternion.Euler(0f, -90f, 0f);
            spawnPos += Vector3.down * 1.5f; // tweak to taste
        }

        GameObject projectile = Instantiate(prefab, spawnPos, spawnRotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            rb.useGravity = false;
        }

        Collider projectileCollider = projectile.GetComponent<Collider>();
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();

        if (projectileCollider != null)
        {
            for (int i = 0; i < playerColliders.Length; i++)
            {
                if (playerColliders[i] != null)
                    Physics.IgnoreCollision(projectileCollider, playerColliders[i]);
            }
        }

        SpellProjectile spellProjectile = projectile.GetComponent<SpellProjectile>();
        if (spellProjectile != null)
        {
            spellProjectile.SetOwner(player);
            spellProjectile.SetTravelDirection(direction); // ADD THIS
        }
        

        Fireball fireball = projectile.GetComponent<Fireball>();
        if (fireball != null)
            fireball.Initialize(player);

        PotionShatter potionShatter = projectile.GetComponent<PotionShatter>();
        if (potionShatter != null)
            potionShatter.Initialize(player);

        Destroy(projectile, 5f);
    }

    [ServerRpc]
    void LaunchProjectile_ServerRpc(NetworkIdentity playerIdentity, int spellType, Vector3 clientForward)
    {
        if (!isServer) return;
        if (playerIdentity == null) return;

        GameObject prefab = null;
        float speed = fireballSpeed;

        switch (spellType)
        {
            case 0: prefab = fireballPrefab;      speed = fireballSpeed;      break;
            case 1: prefab = blazingImpactPrefab; speed = fireballSpeed;      break;
            case 2: prefab = iceballPrefab;       speed = iceballSpeed;       break;
            case 3: prefab = emberCirclePrefab;   speed = emberCircleSpeed;   break;
            case 4: prefab = lightningBoltPrefab; speed = lightningBoltSpeed; break;
        }
        

        if (prefab == null) return;

        GameObject player = playerIdentity.gameObject;
        SpellCaster caster = player.GetComponent<SpellCaster>();
        Camera cam = player.GetComponentInChildren<Camera>();

        if (caster == null || cam == null) return;

        Transform spawnPoint = caster.fireballSpawnPoint;
        

        Vector3 forward = clientForward.normalized;
        if (forward == Vector3.zero)
            forward = player.transform.forward;

        float spawnForwardOffset = (spellType == 4) ? 0.1f : 2f;

        Vector3 spawnPos = spawnPoint != null
            ? spawnPoint.position + forward * spawnForwardOffset
            : cam.transform.position + forward * spawnForwardOffset;

        Ray aimRay = new Ray(cam.transform.position, forward);
        Vector3 targetPoint;

        if (Physics.Raycast(aimRay, out RaycastHit hit, 100f, aimLayers) && hit.distance > 3f)
            targetPoint = hit.point;
        else
            targetPoint = aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;
        LaunchProjectile_Server(prefab, speed, spawnPos, shootDirection, player);
        
        // 🔊 SPELL CAST SOUND (SERVER ONLY)
        if (spellType == 0) SoundManager.Instance.PlayFireball(player.transform.position);
        if (spellType == 2) SoundManager.Instance.PlayIceball(player.transform.position);
        if (spellType == 4) SoundManager.Instance.PlayShockwave(player.transform.position);
        
        
    }

    [ServerRpc]
    void CastWall_ServerRPC(NetworkIdentity playerIdentity, int wallType, Vector3 clientForward)
    {
        if (!isServer) return;
        if (playerIdentity == null) return;

        GameObject player = playerIdentity.gameObject;
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam == null) return;

        GameObject prefab = null;
        float speed = 0f;

        if (wallType == 0) { prefab = fireWallPrefab; speed = fireWallSpeed; }
        else if (wallType == 1) { prefab = iceWallPrefab; speed = iceWallSpeed; }

        if (prefab == null) return;
        
        

        Vector3 forward = clientForward.normalized;
        if (forward == Vector3.zero)
            forward = player.transform.forward;

        Vector3 spawnPos = cam.transform.position + forward * 3f;

        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit floorHit, 10f, aimLayers))
            spawnPos.y = floorHit.point.y + 1f;

        Ray aimRay = new Ray(cam.transform.position, forward);
        Vector3 targetPoint = Physics.Raycast(aimRay, out RaycastHit hit, 100f, aimLayers) && hit.distance > 3f
            ? hit.point
            : aimRay.GetPoint(100f);

        Vector3 shootDirection = (targetPoint - spawnPos).normalized;
        shootDirection.y = 0f;
        if (shootDirection == Vector3.zero)
            shootDirection = player.transform.forward;
        shootDirection.Normalize();

        Quaternion spawnRotation = Quaternion.LookRotation(shootDirection) * Quaternion.Euler(0f, 0f, -90f);

        GameObject wall = Instantiate(prefab, spawnPos, spawnRotation);
        
        // 🔊 WALL SOUND
        if (wallType == 0) SoundManager.Instance.PlayFirewall(spawnPos);
        if (wallType == 1) SoundManager.Instance.PlayIceball(spawnPos);

        Rigidbody rb = wall.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * speed;
            rb.useGravity = false;
        }

        FireWall fireWall = wall.GetComponent<FireWall>();
        if (fireWall != null) fireWall.Initialize(player);

        IceWall iceWall = wall.GetComponent<IceWall>();
        if (iceWall != null) iceWall.Initialize(player);

        Destroy(wall, 5f);
    }

    // Server does the damage + VFX broadcast for shockwave
    [ServerRpc]
    void CastShockwave_ServerRPC(NetworkIdentity playerIdentity)
    {
        if (!isServer) return;
        if (playerIdentity == null) return;
        
        

        GameObject player = playerIdentity.gameObject;
        SpellCaster caster = player.GetComponent<SpellCaster>();
        if (caster == null) return;

        Vector3 origin = player.transform.position + Vector3.down * 1f; // tweak to taste

        // Damage all enemies in radius
        Collider[] hits = Physics.OverlapSphere(origin, caster.shockwaveRadius);
        foreach (Collider col in hits)
        {
            if (col.transform.root.gameObject == player) continue;

            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(caster.shockwaveDamage, player);
        }

        // 🔊 SHOCKWAVE SOUND (ONCE ONLY)
        SoundManager.Instance.PlayShockwave(origin);
        // All clients play the VFX at caster's position
        PlayShockwaveVFX_ObserversRPC(origin);
    }

    [ObserversRpc]
    void PlayShockwaveVFX_ObserversRPC(Vector3 origin)
    {
        if (shockwaveVFXPrefab == null) return;

        GameObject vfx = Instantiate(shockwaveVFXPrefab, origin, Quaternion.identity);
        Destroy(vfx, shockwaveVFXDuration);
    }

    // Public cast methods called by VoiceSpellCaster

    public void CastFireball()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 0, cam.transform.forward);
    }

    public void CastBlazingImpact()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 1, cam.transform.forward);
    }

    public void CastIceball()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 2, cam.transform.forward);
    }

    public void CastEmberCircle()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 3, cam.transform.forward);
    }

    public void CastLightningStrike()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        LaunchProjectile_ServerRpc(GetComponent<NetworkIdentity>(), 4, cam.transform.forward);
    }

    public void CastShockwave()
    {
        PlayCastAnimation();
        CastShockwave_ServerRPC(GetComponent<NetworkIdentity>());
    }

    public void CastFireWall()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        CastWall_ServerRPC(GetComponent<NetworkIdentity>(), 0, cam.transform.forward);
    }

    public void CastIceWall()
    {
        PlayCastAnimation();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        CastWall_ServerRPC(GetComponent<NetworkIdentity>(), 1, cam.transform.forward);
    }
}