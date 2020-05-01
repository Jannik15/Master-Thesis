﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    public float damage = 1f;
    public float range = 1000f;
    public float fireRate = 5f;
    public float impactForce = 30f;
    public GameObject line, laserBullet;
    public int currentAmmo = 200;
    public LayerMask layerMask;

    public Transform barrelPoint;

    private float nextTimeToFire = 0f;
    public Text ammoAmount;

    void Awake()
    {
        ammoAmount.text = currentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

   public void Shoot()
   {
       if (currentAmmo > 0)
       {
           RaycastHit hit;
           bool hasHit = Physics.Raycast(barrelPoint.position, barrelPoint.right, out hit, range, layerMask);
           if (Physics.Raycast(barrelPoint.position, barrelPoint.right, out hit, range, layerMask))
           {
               Enemy enemy = hit.transform.GetComponentInParent<Enemy>();
               if (enemy == null)
               {
                   enemy = hit.transform.GetComponentInChildren<Enemy>();
               }
               if (enemy != null && enemy.transform.tag == "Enemy")
               {
                   enemy.TakeDamage(damage, hit.point);
               } 
               else if (enemy != null && enemy.transform.tag == "InteractableTarget")
               {
                    enemy.InteractableTarget(hit.transform.gameObject);
               }

               if (hit.rigidbody != null)
               {
                   hit.rigidbody.AddForceAtPosition(barrelPoint.right * impactForce, hit.point);
               }

               if (line != null)
               {
                   GameObject liner = Instantiate(line);
                  liner.GetComponent<LineRenderer>().SetPositions(new Vector3[] { barrelPoint.position, hasHit ? hit.point : barrelPoint.position + barrelPoint.right * 100 });
                  Destroy(liner,0.3f);
               }
           }
           currentAmmo--;
           ammoAmount.text = currentAmmo.ToString();
            GameObject bullet = Instantiate(laserBullet, barrelPoint.position, Quaternion.LookRotation(barrelPoint.right));
        }
       else if (currentAmmo <= 0)
       {
           ammoAmount.material.SetColor("_Color", Color.red);
       }



    }
}
