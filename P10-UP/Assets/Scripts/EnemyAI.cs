using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public Transform playerCam;
    public GameObject line, laserBullet, gun;
    public float fireRate = 3f;
    public Transform barrelPoint;
    public LayerMask layerMask;
    public Animator animator;


    private Enemy enemy;
    private float nextTimeToFire = 0f;

    private bool looking;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        playerCam = Camera.main.transform;
        looking = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.Health > 0)
        {
            EnemyShoot();
        }

    }


    void EnemyShoot()
    {
        //if bool is true
        gun.transform.LookAt(new Vector3(playerCam.position.x, playerCam.position.y -0.5f, playerCam.position.z));

        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            GameObject bullet = Instantiate(laserBullet, barrelPoint.position, Quaternion.LookRotation(barrelPoint.forward));

            RaycastHit hit;
            bool hasHit = Physics.Raycast(barrelPoint.position, barrelPoint.forward, out hit, 1000f, layerMask);
            if (line != null)
            {
                GameObject liner = Instantiate(line);
                liner.GetComponent<LineRenderer>().SetPositions(new Vector3[] { barrelPoint.position, hasHit ? hit.point : barrelPoint.position + barrelPoint.forward * 100 });
                Destroy(liner, 0.3f);
            }

        }

    }
}
