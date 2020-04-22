using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 1f;
    public float range = 100f;
    public float fireRate = 3f;
    public float impactForce = 30f;
    public GameObject line, laserBullet;
    public int currentAmmo = 200;

    public Transform barrelPoint;

    private float nextTimeToFire = 0f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

   public void Shoot()
    {
        RaycastHit hit;
        bool hasHit = Physics.Raycast(barrelPoint.position, barrelPoint.right, out hit, range);
        if (Physics.Raycast(barrelPoint.position, barrelPoint.right, out hit, range) && currentAmmo > 0)
        {
            currentAmmo--;

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            //if (line != null)
            //{
            //    GameObject liner = Instantiate(line);
            //    liner.GetComponent<LineRenderer>().SetPositions(new Vector3[] { barrelPoint.position, hasHit ? hit.point : barrelPoint.position + barrelPoint.right * 100 });
            //    Destroy(liner,0.2f);
            //}

            GameObject bullet = Instantiate(laserBullet, barrelPoint.position, Quaternion.LookRotation(barrelPoint.right));
            //bullet.transform.rotation = Quaternion.LookRotation(barrelPoint.right);
        }
        else if (currentAmmo <= 0)
        {
            //Play out of ammo sound
        }
    }
}
