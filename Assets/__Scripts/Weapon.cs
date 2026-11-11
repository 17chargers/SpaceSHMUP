using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType {
    none,
    blaster,
    spread,
    phaser,
    missle,
    laser,shield
}

[System.Serializable]
public class WeaponDefinition {
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;
    [Tooltip("Prefab of Weapon model that is attached to the Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;
    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;
    [Tooltip("Damage caused per second by the Laser")]
    public float damagePerSec = 0;
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
}

public class Weapon : MonoBehaviour
{
    static public Transform  PROJECTILE_ANCHOR;

    [Header("Laser Settings")]
    [Tooltip("Width of the laser beam")]
    public float laserWidth = 0.4f;
    [Tooltip("How far the laser reaches if it doesn't hit anything")]
    public float laserMaxDistance = 100f;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    private Transform shotPointTrans;

    private LineRenderer laserLine;     // Draws the visible laser beam
    private bool laserFiring = false;   // Set true by Fire() each frame the button is held

    void Start() {
        if (PROJECTILE_ANCHOR == null) {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild( 0 );
        
        SetType(_type);

        Hero hero = GetComponentInParent<Hero>();
        if ( hero != null ) hero.fireEvent += Fire;
    }

    public eWeaponType type {
        get { return(_type ); }
        set { SetType( value ); }
    }

    public void SetType(eWeaponType wt ) {
        _type = wt;
        if (type == eWeaponType.none) {
            this.gameObject.SetActive(false);
            return;
        } else {
            this.gameObject.SetActive(true);
        }

        def = Main.GET_WEAPON_DEFINITION(_type);

        if ( weaponModel != null ) Destroy( weaponModel );
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0;

        // Set up the beam if this is a laser; otherwise make sure no beam is showing
        if ( _type == eWeaponType.laser ) {
            SetupLaserLine();
        } else if ( laserLine != null ) {
            laserLine.enabled = false;
        }
    }

    private void Fire() {
        if ( !gameObject.activeInHierarchy ) return;

        if ( Time.time < nextShotTime ) return;
        
        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type) {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;
            
            case eWeaponType.spread:
                p = MakeProjectile();
                p.vel = vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis( 10, Vector3.back );
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back );
                p.vel = p.transform.rotation * vel;
                break;

            case eWeaponType.laser:
                // The laser doesn't spawn a projectile. It just records that the
                // fire button is held this frame; LateUpdate() does the rest.
                laserFiring = true;
                break;
        }
    }

    // LateUpdate() runs after every Update(), so by now Hero.Update() has
    // already called Fire() if the fire button is being held this frame.
    void LateUpdate() {
        if ( _type != eWeaponType.laser || laserLine == null ) return;

        if ( laserFiring ) {
            FireLaser();
        } else {
            laserLine.enabled = false;  // Button released: hide the beam
        }
        laserFiring = false;            // Reset for next frame
    }

    private void FireLaser() {
        Vector3 start = shotPointTrans.position;
        start.z = 0;
        Vector3 end = start + Vector3.up * laserMaxDistance;

        // Only look for things on the Enemy layer
        int enemyMask = LayerMask.GetMask( "Enemy" );

        RaycastHit hit;
        if ( Physics.Raycast( start, Vector3.up, out hit, laserMaxDistance, enemyMask ) ) {
            end = hit.point;    // Beam stops at the first thing it hits

            // The collider might be on a child (e.g., an Enemy_4 shield), so look up the hierarchy
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if ( enemy != null ) {
                // damagePerSec * Time.deltaTime = the damage for just this frame
                enemy.TakeDamage( def.damagePerSec * Time.deltaTime, hit.collider.gameObject );
            }
        }

        laserLine.SetPosition( 0, start );
        laserLine.SetPosition( 1, end );
        laserLine.enabled = true;
    }

    private void SetupLaserLine() {
        if ( laserLine == null ) {
            laserLine = gameObject.AddComponent<LineRenderer>();
            laserLine.material = new Material( Shader.Find( "Sprites/Default" ) );
            laserLine.positionCount = 2;
            laserLine.useWorldSpace = true;
            laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserLine.receiveShadows = false;
        }
        laserLine.startWidth = laserLine.endWidth = laserWidth;
        laserLine.startColor = laserLine.endColor = def.projectileColor;
        laserLine.enabled = false;
    }

    private ProjectileHero MakeProjectile() {
        GameObject go;
        go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;
        return( p );
    }
}
