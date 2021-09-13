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
    public const float DICE_EDGE_LIMIT = 0.3f;

    private Rigidbody m_rigidBody;
    private CharacterController m_controller;

    public GameObject attachedDice = null;
    public bool m_needToClimb = false;
    public bool m_isPushing = false;
    // Start is called before the first frame update
    public Vector3 m_movement;
    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private ExitDirection getExitDirection(Vector3 movement, Vector3 nextPosition)
    {
        // Verifier / Ajuster le comportement quand on va vers un coin
        float x_diff = nextPosition.x - attachedDice.transform.position.x;
        float z_diff = nextPosition.z - attachedDice.transform.position.z;
        // Going left on left border
        if (movement.x < 0 && x_diff < 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateLeft");
            }
            return ExitDirection.LEFT;
        }
        // Going right on right border
        else if (movement.x > 0 && x_diff > 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateRight");
            }
            return ExitDirection.RIGHT;
        }
        // Going back on back border
        else if (movement.z < 0 && z_diff < 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateBackward");
            }
            return ExitDirection.BACK;
        }
        // Going front on front border
        else if (movement.z > 0 && z_diff > 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateForward");
            }
            return ExitDirection.FRONT;
        }
        return ExitDirection.NONE;
    }

    // Do not call if attachedDice == null
    // Return true if dice is rotated, else false
    private bool rotateDiceIfNeeded(ExitDirection exitDirection)
    {
        // If the dice is already rotating, do nothing
        if (attachedDice.GetComponent<Dice>().m_rotating == true) {
            return false;
        }
        switch (exitDirection) {
            // do nothing
            case ExitDirection.NONE:
                return false;

            case ExitDirection.LEFT:
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateLeft");
                break;

            case ExitDirection.RIGHT:
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateRight");
                break;

            case ExitDirection.BACK:
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateBackward");
                break;

            case ExitDirection.FRONT:
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateForward");
                break;
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        float x_movement = Input.GetAxis("Horizontal");
        float z_movement = Input.GetAxis("Vertical");
        m_movement = new Vector3(x_movement, 0, z_movement);
        if (m_movement.sqrMagnitude <= 0) {
            m_isPushing = false;
            return ;
        }

        // Voir comment les deplacements se passent quand on veut passer d'un cube en train d'apparaitre / disparaitre a un qui est full

        Vector3 nextPosition = transform.position + m_movement * Time.deltaTime;
        if (attachedDice != null) {
            ExitDirection exitDirection = getExitDirection(m_movement, nextPosition);

            if (attachedDice.GetComponent<Dice>().m_diceInteractable) {
                // If we rotate the dice we don't update the position
                if (rotateDiceIfNeeded(exitDirection)) {
                    return ;
                }
            } else {
                // Get off the dice
                if (exitDirection != ExitDirection.NONE) {
                    attachedDice = null;
                }
            }
        }

        if (m_needToClimb && attachedDice != null) {
            m_needToClimb = false;
            // Move to the top of the dice
            nextPosition += new Vector3(0, attachedDice.transform.Find("Top").position.y, 0);
            // Move slightly more towards the center of the dice to not fall
            nextPosition += (attachedDice.transform.position - transform.position) / 10;
        }

        if (attachedDice == null) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), m_movement, out hit, 0.2f)) {
                if (hit.collider.CompareTag("Dice")) {
                    Debug.Log("Hit dice raycast");
                }
            }
        }

        // Ajouter mecanismes liés a l'apparition de dés

        transform.position = nextPosition;
    }


    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Dice") && attachedDice == null) {
            Debug.Log("Collision enter");

            if (other.gameObject.GetComponent<Dice>().m_isClimbable == true) {
                m_needToClimb = true;
            
                // Attach dice after we are on it
                attachedDice = other.gameObject;
            } else {
                // Push dice
                // Get direction of impact
                m_isPushing = true;


            }
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.CompareTag("Dice")) {
            if (other.gameObject.GetComponent<Dice>().m_isClimbable) {
                m_needToClimb = false;
            } else {
                m_isPushing = false;
            }
        }
    }
}