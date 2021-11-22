using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private enum ExitDirection
    {
        NONE = 0,
        LEFT,
        RIGHT,
        BACK,
        FRONT
    };
    public const float DICE_EDGE_LIMIT = 0.25f;
    public const float CEILING_PRESS_PUSH = 0.1f;
    public const float DICE_CLIMB_LIMIT = 0.5f;

    private Rigidbody m_rigidBody;
    private CharacterController m_controller;

    [SerializeField]
    private GameObject m_attachedDice = null;
    private GameObject m_diceToPush = null;
    public bool m_isPushing = false;
    public bool m_onFloor = false;
    public bool m_collidingDice = false;
    // Start is called before the first frame update
    public Vector3 m_movement;

    private float m_pressPushTime = 0;
    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private bool approximateCheck(float a, float b)
    {
        return ((a >= (b - 0.01f)) && (a <= b + 0.01f));
    }

    private ExitDirection getExitDirection(Vector3 movement, Vector3 nextPosition)
    {
        // Verifier / Ajuster le comportement quand on va vers un coin
        float x_diff = nextPosition.x - m_attachedDice.transform.position.x;
        float z_diff = nextPosition.z - m_attachedDice.transform.position.z;
        // Going left on left border
        if (movement.x < 0 && x_diff < 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT)) {
            return ExitDirection.LEFT;
        }
        // Going right on right border
        else if (movement.x > 0 && x_diff > 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT)) {
            return ExitDirection.RIGHT;
        }
        // Going back on back border
        else if (movement.z < 0 && z_diff < 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT)) {
            return ExitDirection.BACK;
        }
        // Going front on front border
        else if (movement.z > 0 && z_diff > 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT)) {
            return ExitDirection.FRONT;
        }
        return ExitDirection.NONE;
    }

    private bool isLeavingDice(ExitDirection exitDirection)
    {
        switch (exitDirection) {
            case ExitDirection.NONE:
                return false;

            case ExitDirection.LEFT:
                if (m_attachedDice.GetComponent<Dice>().m_isOtherDiceLeft)
                {
                    return true;
                }
                break;

            case ExitDirection.RIGHT:
                if (m_attachedDice.GetComponent<Dice>().m_isOtherDiceRight)
                {
                    return true;
                }
                break;

            case ExitDirection.BACK:
                if (m_attachedDice.GetComponent<Dice>().m_isOtherDiceBack)
                {
                    return true;
                }
                break;

            case ExitDirection.FRONT:
                if (m_attachedDice.GetComponent<Dice>().m_isOtherDiceFront)
                {
                    return true;
                }
                break;
        }
        return false;
    }
    // Do not call if m_attachedDice == null
    // Return true if dice is rotated, else false
    private bool rotateDiceIfNeeded(ExitDirection exitDirection)
    {
        // If the dice is already rotating, do nothing
        if (m_attachedDice.GetComponent<Dice>().m_rotating == true) {
            return false;
        }
        if (m_attachedDice.GetComponent<Dice>().m_diceInteractable == false) {
            return false;
        }

        switch (exitDirection) {
            // do nothing
            case ExitDirection.NONE:
                return false;

            case ExitDirection.LEFT:
                m_attachedDice.GetComponent<Dice>().StartCoroutine("rotateLeft");
                break;

            case ExitDirection.RIGHT:
                m_attachedDice.GetComponent<Dice>().StartCoroutine("rotateRight");
                break;

            case ExitDirection.BACK:
                m_attachedDice.GetComponent<Dice>().StartCoroutine("rotateBackward");
                break;

            case ExitDirection.FRONT:
                m_attachedDice.GetComponent<Dice>().StartCoroutine("rotateForward");
                break;
        }
        return true;
    }

    IEnumerator pushDice(Vector3 movement)
    {
        Dice.FacePosition pushingFace = m_diceToPush.GetComponent<Dice>().getClosestFace(transform.position);
        Vector3 direction = Vector3.zero;
        switch (pushingFace)
        {
            case Dice.FacePosition.BACK:
                direction = Vector3.forward;
                break;
            case Dice.FacePosition.LEFT:
                direction = Vector3.right;
                break;
            case Dice.FacePosition.RIGHT:
                direction = Vector3.left;
                break;
            case Dice.FacePosition.FRONT:
                direction = Vector3.back;
                break;
            default:
                Debug.Log("Unsupported closest face value");
                yield break;
        }

        // This coroutine can be called if we are close to the dice and moving in parallel
        // We only push if we are moving in the direction of the dice
        if (Vector3.Dot(movement, direction) != 1) {
            yield break;
        }
        // We do not want player to input movement during the animation so we overload the movement of the character
        m_isPushing = true;
        GetComponent<testcharactercontroller>().m_moveOverload = direction;
        for (uint i = 0; i < 100; ++i)
        {
            // parfois c'est null
            m_diceToPush.transform.Translate(direction * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        GetComponent<testcharactercontroller>().m_moveOverload = Vector3.zero;
        m_isPushing = false;
        m_diceToPush = null;
        yield break;
    }


    // Update is called once per frame
    void Update()
    {
        if (m_isPushing)
            return ;

        float x_movement = Input.GetAxis("Horizontal");
        float z_movement = Input.GetAxis("Vertical");
        m_movement = new Vector3(x_movement, 0, z_movement);
        if (m_movement.sqrMagnitude <= 0) {
            m_isPushing = false;
            return ;
        }

        Vector3 nextPosition = transform.position + m_movement * Time.deltaTime;
        
        if (m_attachedDice != null) {
            ExitDirection exitDirection = getExitDirection(m_movement, nextPosition);

            if (m_attachedDice.GetComponent<Dice>().m_diceInteractable) {
                if (isLeavingDice(exitDirection)) {
                    m_attachedDice.GetComponent<Dice>().onPlayerGetOffDice();
                } else if (rotateDiceIfNeeded(exitDirection)) {
                    // If we rotate the dice we don't update the position
                    return ;
                }
            } else {
                // Get off the dice
                if (exitDirection != ExitDirection.NONE) {
                    m_attachedDice.GetComponent<Dice>().onPlayerGetOffDice();
                    m_attachedDice = null;
                }
            }
        }

        if (m_attachedDice == null) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), m_movement, out hit, 0.2f)) {
                if (hit.collider.CompareTag("Dice")) {
                    Debug.Log("Hit dice raycast");
                }
            }
        }

        // Ajouter mecanismes liés a l'apparition de dés
    }


    // Maybe other conditions to add
    private bool isDiceClimbable(Dice dice)
    {
        float diff_y = dice.gameObject.transform.position.y - transform.position.y;
        return (approximateCheck(diff_y, 0) || diff_y < 0);
    }

    // Maybe other conditions to add
    private bool isDiceWalkable(Dice dice)
    {
        float diff_y = dice.gameObject.transform.position.y - transform.position.y;
        // True if we can either move down on the dice or if it is on the same level
        return (diff_y < -0.5f || approximateCheck(diff_y, -0.5f));
    }

    private void OnCollisionEnter(Collision other) {
        Debug.Log("Collision enter");
    }

    private void OnCollisionStay(Collision other) {
        if (m_isPushing) {
            return ;
        }
        if (other.gameObject.CompareTag("Dice")) {
            m_collidingDice = true;
            if (m_attachedDice != other.gameObject) {

                Dice diceCollided = other.gameObject.GetComponent<Dice>();
                float diff_y = other.transform.position.y - transform.position.y;
                if (diff_y < 0) {// Dice is below the player
                    m_attachedDice = other.gameObject;
                    // Les colliders au bord du dice s'activent avant que le joueur soit vraiment dessus donc on tombe
                    diceCollided.onPlayerGetOnDice();
                } else { // Need to push the dice

                    if (m_onFloor && diceCollided.m_diceInteractable) {

                        // We will start the coroutine even if we are not moving in the direction of the dice
                        // It is not a big deal because the coroutine will do the check
                        // Still kind of a problem because we can move while moving along a dice (not pushing)
                        // and instantly push it because the timer is increased even if not moving against the dice
                        float horizontalMove = Input.GetAxis("Horizontal");
                        float verticalMove = Input.GetAxis("Vertical");
                        if (!(horizontalMove == 0 && verticalMove == 0)) {
                            m_pressPushTime += Time.deltaTime;
                            if (m_pressPushTime > CEILING_PRESS_PUSH) {
                                m_diceToPush = other.gameObject;
                                StartCoroutine("pushDice", m_movement);
                                m_pressPushTime = 0;
                            }
                        }
                        
                    }
                }
            }
        } else if (other.gameObject.CompareTag("Floor")) {
            m_onFloor = true;
        }
    }

    private void OnCollisionExit(Collision other) {
        Debug.Log("Collision exit");
        if (other.gameObject.CompareTag("Dice")) {
            m_collidingDice = false;
            // Maybe not necessary anymore
            if (m_isPushing == false) {
                m_diceToPush = null;
            }
        } else if (other.gameObject.CompareTag("Floor")) {
            m_onFloor = false;
            m_attachedDice = null;
        }
    }
}