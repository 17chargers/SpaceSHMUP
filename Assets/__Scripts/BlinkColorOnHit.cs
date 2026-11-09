using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BlinkColorOnHit : MonoBehaviour
{
    private static float blinkDuration = 0.1f;
    private static Color blinkColor = Color.red;

    [Header("Dynamic")]
    public bool showingColor = false;
    public float blinkCompleteTime;

    private Material[] materials;
    private Color[] originalColors;
    private BoundsCheck bndCheck;

    void Awake() {
        bndCheck = GetComponentInParent<BoundsCheck>();
        materials = Utils.GetAllMaterials( gameObject );
        originalColors = new Color[materials.Length];
        for (int i=0;i <materials.Length; i++) {
            originalColors[i] = materials[i].color;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if ( showingColor && Time.time > blinkCompleteTime ) RevertColors();
    }

    void OnCollisionEnter( Collision coll ) {
        ProjectileHero p = coll.gameObject.GetComponent<ProjectileHero>();
        if ( p != null ) {
            if ( bndCheck != null && !bndCheck.isOnScreen ) {
                return;
            }
            SetColors();
        }
    }

    void SetColors() {
        foreach (Material m in materials) {
            m.color = blinkColor;
        }
        showingColor = true;
        blinkCompleteTime = Time.time + blinkDuration;    
    }

    void RevertColors() {
        for ( int i=0; i <materials.Length; i++) {
            materials[i].color = originalColors[i];
        }
        showingColor = false;
    }
}
