using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] [ConditionalHide("m_UseFovKick")] private FOVKick m_FovKick = new FOVKick();
    
    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private PlayerControllerExtension m_ControllerExt;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private bool m_Jumping;
        
    void Start() {
        m_CharacterController = GetComponent<CharacterController>();
        m_ControllerExt = GetComponent<PlayerControllerExtension> ();
        m_Camera = GetComponentInChildren<Camera> ();
        m_Camera = m_Camera == null ? Camera.main : m_Camera;
        m_FovKick.Setup(m_Camera);
        m_Jumping = false;
		m_MouseLook.Init(transform , m_Camera.transform);

        if (GameController.Instance) {
            GameController.Instance.player = gameObject;
        }
    }
        
    void Update() {
        if (GameController.Instance) {
            if (GameController.Instance.isPaused || !GameController.Instance.isPausable) {
                m_MouseLook.SetCursorLock (false);
            } else {
                m_MouseLook.SetCursorLock (true);
                RotateView ();
            }
        } else {
            RotateView ();
        }

        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = Input.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            // landing
            if (m_ControllerExt) {
                m_ControllerExt.OnLanding ();
            }
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        if (Input.GetButtonDown ("Interact")) {
            // We create a ray
            Ray ray = m_Camera.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;

            // If the ray hits
            if (Physics.Raycast (ray, out hit, 100)) {
                Interactable interactable = hit.collider.transform.parent.GetComponentInChildren<Interactable> ();
                if (interactable) {
                    interactable.InteractFrom (hit.distance);
                }
            }
        }
    }
    
    void FixedUpdate() {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                            m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                m_Jump = false;
                m_Jumping = true;
                if (m_ControllerExt) {
                    m_ControllerExt.OnJump ();
                }
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;

            // #TODO fall
        }

        // move the character
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        
        if (m_ControllerExt) {
            // head bobbing
            m_ControllerExt.UpdateCameraPosition ();
        }

        m_MouseLook.UpdateCursorLock();
    }
    
    void GetInput(out float speed) {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        speed = DesiredSpeed ();
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }

        if (m_ControllerExt) {
            bool isMoving = (m_Input != Vector2.zero);
            Vector2 animationSpeed = m_IsWalking ? m_Input * 0.5f : m_Input;
            m_ControllerExt.OnMove (isMoving, animationSpeed, false);
        }
    }
    
    void RotateView() {
        m_MouseLook.LookRotation (transform, m_Camera.transform);
    }
    
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }

    public bool IsWalking () {
        return m_IsWalking;
    }

    public float DesiredSpeed () {
        // set the desired speed to be walking or running
        return m_IsWalking ? m_WalkSpeed : m_RunSpeed;
    }
}
