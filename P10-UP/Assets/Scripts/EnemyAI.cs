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
    private ProceduralLayoutGeneration layout;
    public bool _canShoot;

    private Enemy thisEnemy;
    private float nextTimeToFire = 0f;

    private bool looking;
    // Start is called before the first frame update
    void Start()
    {
        thisEnemy = GetComponentInParent<Enemy>();
        playerCam = Camera.main.transform;
        looking = true;

        layout = FindObjectOfType<ProceduralLayoutGeneration>();
        layout.roomSwitched += UpdatePlayerRoom;
    }

    // Update is called once per frame
    void Update()
    {
        if (thisEnemy.Health > 0 && _canShoot) // Only when in the same room and can see the player
        {
            EnemyShoot();
        }
    }

    private void UpdatePlayerRoom(Room playerRoom, Portal p)
    {
        _canShoot = playerRoom == thisEnemy.inRoom;
    }

    void OnAnimatorIK(int layerIndex)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPosition(AvatarIKGoal.RightHand, gun.transform.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, gun.transform.rotation);
    }

    void EnemyShoot()
    {
        
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
