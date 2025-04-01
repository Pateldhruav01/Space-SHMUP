using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType {
    none,      
    blaster,    
    spread,     
    phaser,     
    missile,    
    laser,      
    shield      
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
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
}

public class Weapon : MonoBehaviour {

    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime; 
    private GameObject weaponModel;
    private Transform shotPointTrans;

    void Start() {
        if (PROJECTILE_ANCHOR == null) {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        if (transform.childCount > 0) {
            shotPointTrans = transform.GetChild(0);
        } else {
            Debug.LogError("Weapon is missing a child for the shot point.");
        }
        SetType(_type);
        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt) {
        _type = wt;
        if (type == eWeaponType.none) {
            this.gameObject.SetActive(false);
            return;
        } else {
            this.gameObject.SetActive(true);
        }
        def = Main.GET_WEAPON_DEFINITION(_type);
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localScale = Vector3.one;
        nextShotTime = 0; 
    }

    private void Fire() {
        if (!gameObject.activeInHierarchy) {
            Debug.Log("Weapon is not active, Fire() is not being called");
            return;
        }
        if (Time.time < nextShotTime) {
            Debug.Log("Not enough time between shots");
            return;
        }

        Debug.Log("Firing Projectile!");

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
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;
        }
    }

    private ProjectileHero MakeProjectile() {
    if (def.projectilePrefab == null) {
        Debug.LogError("Projectile Prefab is not assigned for weapon type: " + _type);
        return null;
    }
    GameObject go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
    Debug.Log("Instantiated Projectile: " + go.name);
    ProjectileHero p = go.GetComponent<ProjectileHero>();
    if (p == null) {
        Debug.LogError("ProjectileHero script is missing on the projectile prefab");
        return null;
    }

    Vector3 pos = shotPointTrans.position;
    pos.z = 0;  
    Debug.Log("Projectile Position: " + pos);
    p.transform.position = pos;

    p.type = type;
    nextShotTime = Time.time + def.delayBetweenShots;

    return p;
}

}
