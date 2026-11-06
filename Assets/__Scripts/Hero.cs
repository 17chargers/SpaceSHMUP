
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

        if (Input.GetKeyDown( KeyCode.Space) ) {
            TempFire();
        }
    }

    void TempFire() {
        GameObject projGO = Instantiate<GameObject>( projectilePrefab );
        projGO.transform.position = transform.position;
        Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
        rigidB.velocity = Vector3.up * projectileSpeed;
    }

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
