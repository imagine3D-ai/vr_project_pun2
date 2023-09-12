using Photon.Pun;
using RootMotion.FinalIK;
using RootMotion.Demos;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;

public class PlayerHealthManager : MonoBehaviourPunCallbacks
{
    [SerializeField] HitReactionCollision leftHandScript, rightHandScript;
    [SerializeField] VRIK ik;
    public float maxHealth, currentHealth, minDamage, maxDamage, minVelocity, maxVelocity, headshotMultiplier, respawnTime;
    [SerializeField] Animator faceAnim;
    private bool canPlayHealthAnimations = true;
    [SerializeField] private GameObject hurtScreen, mildHurtScreen;
    [SerializeField] float regeneration;

    [SerializeField] private Animator damageAnimator;
    [SerializeField] private GameObject respawnScreen, physicsLeftHand, physicsRightHand, playerController;
    private GameObject deadBody;
    [SerializeField] private TextMeshProUGUI respawnTimer;
    public bool dead;
    private PhotonView pv;

    #region Audio Hands
    [PunRPC]
    void PlayHitAudioInHands(bool isRight, float volume)
    {
        if (isRight)
        {
            rightHandScript.PlayHitAudio(volume);
        }
        else
        {
            leftHandScript.PlayHitAudio(volume);
        }

    }

    [PunRPC]
    void PlayBlockAudioInHands(bool isRight, float volume)
    {
        if (isRight)
        {
            rightHandScript.PlayBlockAudio(volume);
        }
        else
        {
            leftHandScript.PlayBlockAudio(volume);
        }

    }

    [PunRPC]
    void PlayPadAudioInHands(bool isRight, float volume)
    {
        if (isRight)
        {
            rightHandScript.PlayPadAudio(volume);
        }
        else
        {
            leftHandScript.PlayPadAudio(volume);
        }

    }
    #endregion



    private void Awake()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            pv = PhotonView.Get(this);

        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (pv.IsMine)
            {
                currentHealth = maxHealth;
            }
            else
            {
                damageAnimator.gameObject.SetActive(false);
                physicsLeftHand.gameObject.SetActive(false);
                physicsRightHand.gameObject.SetActive(false);
                playerController.gameObject.SetActive(false);
            }
        }

    }

    private void Update()
    {
        if (currentHealth < maxHealth)
            currentHealth += regeneration * Time.deltaTime;
    }

    public void TakeDamage(float velocity, Vector3 direction, GameObject hitObject)
    {
        pv.RPC("SyncDamageRPC", RpcTarget.All, new object[] { velocity, direction, hitObject.tag });

    }

    [PunRPC]
    void SyncDamageRPC(float velocity, Vector3 direction, string hitObjectTag)
    {
        if (!dead)
        {

            if (hitObjectTag == "PlayerHead")
            {

                damageAnimator.SetTrigger("HeadHit");

                currentHealth -= CalculateDamage(velocity) * headshotMultiplier;

                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity) * headshotMultiplier);

                StartCoroutine(EnableHealthAnimationsAfterDelay());



            }
            else if (hitObjectTag == "Player")
            {

                damageAnimator.SetTrigger("BodyHit");

                currentHealth -= CalculateDamage(velocity); ;


                Debug.Log("VELOCITY: " + velocity + " DAMAGE: " + CalculateDamage(velocity));

                StartCoroutine(EnableHealthAnimationsAfterDelay());



            }

            else { return; }
            if (currentHealth <= 0)
                Death();


        }
    }


    private float CalculateDamage(float velocity)
    {
        return velocity;
    }


    private IEnumerator EnableHealthAnimationsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // 2 seconds or put here the time the animation takes
    }

    private IEnumerator RespawnTimer()
    {
        float timer = respawnTime;
        respawnScreen.SetActive(true);
        while (dead)
        {
            respawnTimer.text = "Respawning in " + timer.ToString("F2") + " seconds";
            timer -= Time.deltaTime;
            if (timer < 0)
                dead = false;
            yield return null;
        }
        Respawn();
        yield return null;

    }


    public void callDeath()
    {
        pv.RPC("Death", RpcTarget.All);
    }

    private void Death()
    {

        dead = true;
        ik.gameObject.SetActive(false);
        deadBody = Instantiate(ik.gameObject, ik.transform.position, ik.transform.rotation);
        deadBody.gameObject.SetActive(true);

        foreach (Rigidbody rb in deadBody.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;
        deadBody.GetComponent<Animator>().enabled = false;
        deadBody.GetComponent<VRIK>().enabled = false;
        deadBody.GetComponent<PhotonAnimatorView>().enabled = false;

        StartCoroutine(RespawnTimer());

    }


    private void Respawn()
    {

        Destroy(deadBody);
        deadBody = null;
        respawnScreen.SetActive(false);
        ik.gameObject.SetActive(true);

        currentHealth = maxHealth / 2;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.currentHealth);
        }
        else
        {
            this.currentHealth = (float)stream.ReceiveNext();

        }
    }


}

