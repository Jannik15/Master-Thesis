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
    private float timeWhenPlayerEnteredRoom;
    private Enemy thisEnemy;
    private float nextTimeToFire;
    [HideInInspector] public GameObject lineInstanced;
    private LineRenderer lineRend;
    private Vector3 targetOffset = new Vector3(0, 0.15f, 0);

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
        // Only when in the same room and can see the player
        if (thisEnemy.Health > 0 && _canShoot)
        {
            // Look at the player
            Vector3 target = playerCam.position - targetOffset;
            gun.transform.LookAt(target);

            // Raycast to the player, hitting either the player or any object that should occlude the player
            if (Physics.Raycast(barrelPoint.position, (target - barrelPoint.position), out RaycastHit hit, 100f, layerMask))
            {
                if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Weapon"))
                {
                    if (lineInstanced == null)
                    {
                        lineInstanced = Instantiate(line);
                        lineRend = lineInstanced.GetComponentInChildren<LineRenderer>();
                        ResetTimeToFire();
                    }

                    Vector3 hitDirectionExtended = (target - barrelPoint.position) * 1.2f;
                    lineRend.SetPositions(new Vector3[] { barrelPoint.position, hitDirectionExtended });
                    if (Time.time >= nextTimeToFire)
                    {
                        EnemyShoot();
                        ResetTimeToFire();
                    }
                }
                else if (lineInstanced != null)
                {
                    Destroy(lineInstanced);
                }
            }
            else if (lineInstanced != null)
            {
                Destroy(lineInstanced);
            }
        }
    }

    private void UpdatePlayerRoom(Room playerRoom, Portal p)
    {
        if (playerRoom == thisEnemy.inRoom)
        {
            _canShoot = true;
            ResetTimeToFire();
        }
        else
        {
            _canShoot = false;
        }
    }

    private void ResetTimeToFire()
    {
        nextTimeToFire = Time.time + Random.Range(3.0f, 9.0f) / fireRate;
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
        Instantiate(laserBullet, barrelPoint.position, Quaternion.LookRotation(barrelPoint.forward));
    }
}
