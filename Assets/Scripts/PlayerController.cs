using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool hasControl;
    public static PlayerController localPlayer;
    
    private static bool globalLightIsOff;
    private static UnityEngine.Experimental.Rendering.Universal.Light2D globalLight;

    private Rigidbody myRB;
    private Transform myAvatar;
    private Animator myAnim;
    private UnityEngine.Experimental.Rendering.Universal.Light2D myLight;

    // movements
    [SerializeField] private InputAction WASD;
    private Vector2 movementInput;
    [SerializeField] private float movementSpeed;
    
    //colors
    private static Color myColor;
    private SpriteRenderer myAvatarSprite;
    
    //roles
    [SerializeField] private bool isImpostor;
    [SerializeField] private InputAction KILL;
    [SerializeField] private InputAction SABOTAGE;

    private List<PlayerController> targets;
    [SerializeField] private Collider myCollider;

    private bool isDead;

    [SerializeField] private GameObject bodyPrefab;

    private void Awake()
    {
        KILL.performed += KillTarget;
        SABOTAGE.performed += Sabotage;
    }

    private void OnEnable()
    {
        WASD.Enable();
        KILL.Enable();
        SABOTAGE.Enable();
    }
    
    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
        SABOTAGE.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (hasControl)
        {
            localPlayer = this;
        }
        
        targets = new List<PlayerController>();
        
        myRB = GetComponent<Rigidbody>();
        myAvatar = transform.GetChild(0);
        myAnim = GetComponent<Animator>();
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            myLight = GameObject.Find("Point Light 2D").GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
            myLight.intensity = 0f;
            if (globalLight == null)
                globalLight = GameObject.Find("Global Light 2D")
                    .GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        }

        myAvatarSprite = myAvatar.GetComponent<SpriteRenderer>();
        if (myColor == Color.clear)
            myColor = Color.white;
        
        if (!hasControl)
            return;
        
        myAvatarSprite.color = myColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasControl)
            return;
        
        movementInput = WASD.ReadValue<Vector2>();

        if (movementInput.x != 0)
        {
            myAvatar.localScale = new Vector2(Mathf.Sign(movementInput.x), 1);
        }
        
        myAnim.SetFloat("Speed", movementInput.magnitude);
    }

    private void FixedUpdate()
    {
        myRB.velocity = movementInput * movementSpeed;
    }

    public void SetColor(Color newColor)
    {
        myColor = newColor;
        if (myAvatarSprite != null)
        {
            myAvatarSprite.color = myColor;
        }
    }

    public void SetRole(bool newRole)
    {
        isImpostor = newRole;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController tmpTarget = other.GetComponent<PlayerController>();
            if (isImpostor)
            {
                if (tmpTarget.isImpostor)
                    return;
                else
                {
                    targets.Add(tmpTarget);
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController tmpTarget = other.GetComponent<PlayerController>();
            if (targets.Contains(tmpTarget))
            {
                targets.Remove(tmpTarget);
            }
        }
    }

    void KillTarget(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (targets.Count != 0)
            {
                if (targets[targets.Count - 1].isDead)
                    return;
                
                transform.position = targets[targets.Count - 1].transform.position;
                targets[targets.Count - 1].Die();
                targets.RemoveAt(targets.Count - 1);
            }
            else
                return;
        }
    }

    void Sabotage(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (isImpostor)
            {
                if (!globalLightIsOff)
                    TurnOffGlobalLight();
                else
                    TurnOnGlobalLight();
            }
            else
                return;
        }
    }

    void TurnOffGlobalLight()
    {
        globalLight.intensity = 0f;
        myLight.intensity = 1f;
        globalLightIsOff = true;
    }
    
    void TurnOnGlobalLight()
    {
        globalLight.intensity = 1f;
        myLight.intensity = 0f;
        globalLightIsOff = false;
    }

    public void Die()
    {
        isDead = true;
        
        myAnim.SetBool("IsDead", isDead);
        myCollider.enabled = false;

        Body tmpBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<Body>();
        tmpBody.SetColor(myAvatarSprite.color);
    }
}
