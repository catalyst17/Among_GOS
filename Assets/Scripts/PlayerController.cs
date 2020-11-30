using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool hasControl;
    public static PlayerController localPlayer;

    private Rigidbody myRB;
    private Transform myAvatar;
    private Animator myAnim;

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

    private List<PlayerController> targets;
    [SerializeField] private Collider myCollider;

    private bool isDead;

    [SerializeField] private GameObject bodyPrefab;

    private void Awake()
    {
        KILL.performed += KillTarget;
    }

    private void OnEnable()
    {
        WASD.Enable();
        KILL.Enable();
    }
    
    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
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
                    // Debug.Log(target.name);
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

    public void Die()
    {
        isDead = true;
        
        myAnim.SetBool("IsDead", isDead);
        myCollider.enabled = false;

        Body tmpBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<Body>();
        tmpBody.SetColor(myAvatarSprite.color);
    }
}
