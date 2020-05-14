using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    public bool tutorialMode = false;

    public float damage = 1f;
    public float range = 1000f;
    public float fireRate = 5f;
    public float impactForce = 30f;
    public GameObject line, laserBullet;
    public int currentAmmo = 200;
    public LayerMask layerMask;

    public Transform barrelPoint;

    private float nextTimeToFire = 0f;
    private List<Text> ammoAmount = new List<Text>();

    void Start()
    {
        Text[] textsInChildren = GetComponentsInChildren<Text>(true);
        for (int i = 0; i < textsInChildren.Length; i++)
        {
            if (textsInChildren[i].CompareTag("AmmoDisplay"))
            {
                textsInChildren[i].text = currentAmmo.ToString();
                ammoAmount.Add(textsInChildren[i]);
            }
        }
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
                    Destroy(liner, 0.1f);
                }
            }
            currentAmmo--;
            for (int i = 0; i < ammoAmount.Count; i++)
            {
                ammoAmount[i].text = currentAmmo.ToString();
                if (currentAmmo <= 0)
                {
                    ammoAmount[i].material.SetColor("_Color", Color.red);
                }
            }
            GameObject bullet = Instantiate(laserBullet, barrelPoint.position, Quaternion.LookRotation(barrelPoint.right));
        }
    }
}
