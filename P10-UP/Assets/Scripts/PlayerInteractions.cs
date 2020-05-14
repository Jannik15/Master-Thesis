using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerInteractions : MonoBehaviour
{
    public LayerMask layerMaskRay;
    public float health = 100;
    public Animator fadeAnimator;
    public AudioClip deathSFX;
    public AudioClip hitSFX;

    private AudioSource auS;
    private Ray ray;
    private Camera playerCamera;
    private ProceduralLayoutGeneration proLG;
    private UIWatch uiWatch;
    public Transform virtualParent;

    // Start is called before the first frame update
    void Start()
    {
        uiWatch = transform.parent.parent.GetComponentInChildren<UIWatch>();
        proLG = FindObjectOfType<ProceduralLayoutGeneration>();
        playerCamera = Camera.main;

        if (GetComponent<AudioSource>() != null)
        {
            auS = GetComponent<AudioSource>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (virtualParent != null)
        {
            transform.position = virtualParent.position;
        }
    }

    public void TakeDamage(float amount)
    {

        if (hitSFX != null && auS != null)
        {
            auS.PlayOneShot(hitSFX);
        } 

        uiWatch.healthUpdate((int)amount);
        health -= amount;
        if (health <= 0f)
        {
            if (deathSFX != null && auS != null)
            {
                auS.PlayOneShot(deathSFX);
            }
            Die();
        }
    }

    private void Die()
    {
        fadeAnimator.SetTrigger("FadeToBlack");
        //Fade to black and restart
    }

    public void OnGameComplete()
    {
        //Plays when FadeToWhite animation is finished
        if (PlayerPrefs.GetInt("Testing", 0) == 1)
        {
            SceneManager.LoadScene("TestEnd");
        }
        else
        {
            SceneManager.LoadScene("EndScreen");
        }
    }

    public void OnFadeComplete()
    {
        //Plays when FadeToBlack animation is finished
        SceneManager.LoadScene("Main");
        fadeAnimator.SetTrigger("FadeIn");
    }

    private void OnCollisionEnter(Collision collission)
    {
        if (collission.gameObject.CompareTag("HostileProjectile"))
        {
            TakeDamage(10f);
        }
    }
}
