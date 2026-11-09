
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S {get; private set;}


    [Header("Inscribed")]
    public float speed = 30f;
    public float rollMult = -45f;
    public float pitchMult = 30f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;

    [Header("Dynamic")] [Range(0, 4)] [SerializeField]
    private float _shieldLevel = 1;
    [Tooltip( "This field holds a reference to the last triggering GameObject" )]
    private GameObject lastTriggerGo = null;
    public delegate void WeaponFireDelegate();
    public event WeaponFireDelegate fireEvent;

    void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else 
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        //fireEvent += TempFire;
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += hAxis * speed * Time.deltaTime;
        pos.y += vAxis * speed * Time.deltaTime;
        transform.position = pos;

        transform.rotation = Quaternion.Euler(vAxis*pitchMult, hAxis*rollMult, 0);

        if (Input.GetAxis("Jump") == 1 && fireEvent != null) {
            fireEvent();
        }
    }

    /*void TempFire() {
        GameObject projGO = Instantiate<GameObject>( projectilePrefab );
        projGO.transform.position = transform.position;
        Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
        
        ProjectileHero proj = projGO.GetComponent<ProjectileHero>();
        proj.type = eWeaponType.blaster;
        float tSpeed = Main.GET_WEAPON_DEFINITION( proj.type ).velocity;
        rigidB.velocity = Vector3.up * tSpeed;

    }*/

    void OnTriggerEnter(Collider other){
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        
        if (go == lastTriggerGo ) return;
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy!=null) {
            shieldLevel--;
            Destroy(go);
        } else {
            Debug.LogWarning("Shield trigger hit by non-Enemy: " +go.name);
        }
    }

    public float shieldLevel {
        get { return ( _shieldLevel ); }
        private set {
            _shieldLevel = Mathf.Min( value, 4);
            if (value<0) {
                Destroy(this.gameObject);
                Main.HERO_DIED();
            }
        }
    }
}
