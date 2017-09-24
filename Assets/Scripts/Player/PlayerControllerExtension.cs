using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent (typeof (CharacterController))]
[RequireComponent (typeof (AudioSource))]
public class PlayerControllerExtension : MonoBehaviour {
    
    [Header ("Head Bob")]
    [SerializeField] bool useHeadBob;
    [SerializeField] [ConditionalHide ("useHeadBob", true)]
    private CurveControlledBob headBob = new CurveControlledBob ();
    [SerializeField] [ConditionalHide ("useHeadBob", true)]
    private LerpControlledBob jumpBob = new LerpControlledBob ();
    [Header ("Footsteps")]
    [SerializeField] float stepInterval;
    [SerializeField] [Range (0f, 1f)] float runstepLenghten;
    [SerializeField] AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] AudioClip jumpSound;           // the sound played when character leaves the ground.
    [SerializeField] AudioClip landSound;           // the sound played when character touches back on ground.

    Animator animator;
    AudioSource audioSource;
    CharacterController charController;
    FirstPersonController fpController;
    Camera cam;
    Vector3 originalCameraPosition;

    void Start () {
        animator = GetComponentInChildren<Animator> ();
        audioSource = GetComponent<AudioSource> ();
        charController = GetComponent<CharacterController> ();
        fpController = GetComponent<FirstPersonController> ();
        cam = GetComponentInChildren<Camera> ();
        originalCameraPosition = cam.transform.localPosition;
        headBob.Setup (cam, stepInterval);
    }
    
    public void UpdateCameraPosition () {
        Vector3 newCameraPosition;
        if (!useHeadBob) {
            return;
        }
        if (charController.velocity.magnitude > 0 && charController.isGrounded) {
            cam.transform.localPosition = headBob.DoHeadBob (charController.velocity.magnitude +
                                            (fpController.DesiredSpeed () * (fpController.IsWalking () ? 1f : runstepLenghten)));
            newCameraPosition = cam.transform.localPosition;
            newCameraPosition.y = cam.transform.localPosition.y - jumpBob.Offset ();
        } else {
            newCameraPosition = cam.transform.localPosition;
            newCameraPosition.y = originalCameraPosition.y - jumpBob.Offset ();
        }
        cam.transform.localPosition = newCameraPosition;
    }

    public void OnMove (bool isMoving, Vector2 vel, bool isStrafing) {
        if (isMoving && vel.y == 0) {
            isStrafing = true;
        }

        animator.SetBool ("Moving", isMoving);
        animator.SetBool ("Strafing", isStrafing);
        animator.SetFloat ("VeloX", vel.x);
        animator.SetFloat ("VeloZ", vel.y);
    }

    public void OnJump () {
        animator.SetInteger ("Jumping", 1);
        animator.SetTrigger ("JumpTrigger");
        PlayJumpSound ();
    }

    public void OnFall () {
        animator.SetInteger ("Jumping", 2);
        animator.SetTrigger ("JumpTrigger");
    }

    public void OnLanding () {
        StartCoroutine (jumpBob.DoBobCycle ());
        animator.SetInteger ("Jumping", 0);
        PlayLandingSound ();
    }

    void PlayFootStepSound () {
        if (!charController.isGrounded) {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range (1, footstepSounds.Length);
        audioSource.clip = footstepSounds[n];
        audioSource.PlayOneShot (audioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }

    void PlayJumpSound () {
        audioSource.clip = jumpSound;
        audioSource.Play ();
    }

    void PlayLandingSound () {
        audioSource.clip = landSound;
        audioSource.Play ();
    }


    //Animation Events
    void Hit () {
        Debug.Log ("PlayerControllerExtension: Hit! (Animation event)");
    }

    void FootL () {
        PlayFootStepSound ();
    }

    void FootR () {
        PlayFootStepSound ();
    }

    void Jump () {
        Debug.Log ("PlayerControllerExtension: Jump! (Animation event)");
    }

    void Land () {
        Debug.Log ("PlayerControllerExtension: Land! (Animation event)");
    }

    //method to keep character from moveing while attacking, etc
    public IEnumerator _LockMovementAndAttack (float delayTime, float lockTime) {
        yield return new WaitForSeconds (delayTime);
        //canAction = false;
        //canMove = false;
        animator.SetBool ("Moving", false);
        //rb.velocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
        //inputVec = new Vector3 (0, 0, 0);
        animator.applyRootMotion = true;
        yield return new WaitForSeconds (lockTime);
        //canAction = true;
        //canMove = true;
        animator.applyRootMotion = false;
    }
}
