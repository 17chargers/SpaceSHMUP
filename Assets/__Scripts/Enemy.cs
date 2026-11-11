using UnityEngine;

[RequireComponent( typeof(BoundsCheck) )]
public class Enemy : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed = 10f;
    public float fireRate = 0.3f;
    public float health = 10;
    public int score = 100;
    public float powerUpDropChance = 1f;

    protected bool calledShipDestroyed = false;
    protected BoundsCheck bndCheck;
    protected BlinkColorOnHit blinker;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        blinker = GetComponent<BlinkColorOnHit>();
    }

    public Vector3 pos{
        get
        {
            return this.transform.position;
        }
        set {
            this.transform.position = value;
        }
    }

    void Update() 
    {
        Move();

        if ( bndCheck.LocIs( BoundsCheck.eScreenLocs.offDown) ) 
        {
            Destroy(gameObject);
        }
    }

    public virtual void Move() 
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    void OnCollisionEnter( Collision coll ) {
        GameObject otherGO = coll.gameObject;

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if ( p != null ) {
            TakeDamage( Main.GET_WEAPON_DEFINITION( p.type ).damageOnHit, otherGO );
            Destroy( otherGO );
        } else {
            print( "Enemy hit by non-ProjectileHero: " + otherGO.name );
        }
    }

    /// <summary>
    /// Applies damage to this Enemy. Used by both projectiles (all at once)
    /// and the laser (a small amount every frame).
    /// </summary>
    /// <param name="dmg">Amount of damage to apply</param>
    /// <param name="hitGO">The specific GameObject that was hit (used by Enemy_4 for its shields)</param>
    public virtual void TakeDamage( float dmg, GameObject hitGO = null ) {
        // Enemies can't be damaged until they're on screen
        if ( !bndCheck.isOnScreen ) return;

        // Flash red (needed for the laser, which never triggers OnCollisionEnter)
        if ( blinker != null ) blinker.SetColors();

        health -= dmg;
        if ( health <= 0 ) {
            if (!calledShipDestroyed) {
                calledShipDestroyed = true;
                Main.SHIP_DESTROYED( this );
            }
            Destroy( this.gameObject );
        }
    }
}
