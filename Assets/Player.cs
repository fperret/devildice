using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public const float DICE_EDGE_LIMIT = 0.3f;

    private Rigidbody m_rigidBody;
    private CharacterController m_controller;

    public GameObject attachedDice = null;
    public bool m_needToClimb = false;
    // Start is called before the first frame update
    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    // Do not call if attachedDice == null
    // Return true if dice is rotated, else false
    // changer en "mouvement vers bordure de de"
    // et utiliser la fonction pour mettre attachedDice a null quand la fonction est true + le de en not interactable
    private bool rotateDiceIfNeeded(Vector3 movement, Vector3 nextPosition)
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
            return true;
        }
        // Going right on right border
        else if (movement.x > 0 && x_diff > 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateRight");
            }
            return true;
        }
        // Going back on back border
        else if (movement.z < 0 && z_diff < 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateBackward");
            }
            return true;
        }
        // Going front on front border
        else if (movement.z > 0 && z_diff > 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
        {
            if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                attachedDice.GetComponent<Dice>().StartCoroutine("rotateForward");
            }
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        float x_movement = Input.GetAxis("Horizontal");
        float z_movement = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(x_movement, 0, z_movement);
        if (movement.sqrMagnitude <= 0)
            return ;

        Vector3 nextPosition = transform.position + movement * Time.deltaTime;
        if (attachedDice != null) {
            if (attachedDice.GetComponent<Dice>().m_diceInteractable) {
                //rotateDiceIfNeeded(movement, nextPosition);
            } else {
                attachedDice = null;
            }
        }
        if (m_needToClimb && attachedDice != null) {
            nextPosition += (attachedDice.transform.position - transform.position) / 10;
            nextPosition += new Vector3(0, attachedDice.transform.Find("Top").position.y, 0);
            m_needToClimb = false;
        }
        transform.position = nextPosition;
    }


    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Dice") && attachedDice == null) {
            Debug.Log("Collision enter");
            m_needToClimb = true;
        
            // Attach dice after we are on it
            attachedDice = other.gameObject;
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.CompareTag("Dice")) {
            m_needToClimb = false;
        }
    }
}