
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S {get; private set;}


    [Header("Inscribed")]
    public float speed = 30f;
    public float rollMult = -45f;
    public float pitchMult = 30f;

    [Header("Dynamic")] [Range(0, 4)]
    public float shieldLevel = 1;

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
    }
}
