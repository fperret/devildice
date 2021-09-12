using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public const float DICE_EDGE_LIMIT = 0.3f;

    private Rigidbody m_rigidBody;
    private CharacterController m_controller;

    public GameObject attachedDice = null;
    // Start is called before the first frame update
    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_rigidBody = GetComponent<Rigidbody>();
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
            // Verifier / Ajuster le comportement quand on va vers un coin
            float x_diff = nextPosition.x - attachedDice.transform.position.x;
            float z_diff = nextPosition.z - attachedDice.transform.position.z;
            // Going left on left border
            if (movement.x < 0 && x_diff < 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT))
            {
                if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                    attachedDice.GetComponent<Dice>().StartCoroutine("rotateLeft");
                }
                return ;
            }
            // Going right on right border
            else if (movement.x > 0 && x_diff > 0 && (Mathf.Abs(x_diff) > DICE_EDGE_LIMIT))
            {
                if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                    attachedDice.GetComponent<Dice>().StartCoroutine("rotateRight");
                }
                return ;
            }
            // Going back on back border
            else if (movement.z < 0 && z_diff < 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
            {
                if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                    attachedDice.GetComponent<Dice>().StartCoroutine("rotateBackward");
                }
                return ;
            }
            // Going front on front border
            else if (movement.z > 0 && z_diff > 0 && (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
            {
                if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                    attachedDice.GetComponent<Dice>().StartCoroutine("rotateForward");
                }
                return ;
            }
        }
        transform.position = nextPosition;
    }


    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Dice") {
            Debug.Log("Collision enter");
            attachedDice = other.gameObject;
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.tag == "Dice") {
            attachedDice = null;
        }
    }
}