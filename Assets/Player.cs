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
        Vector3 nextPosition = transform.position + new Vector3(x_movement, 0, z_movement) * Time.deltaTime;
        if (attachedDice != null) {
            float x_diff = nextPosition.x - attachedDice.transform.position.x;
            float z_diff = nextPosition.z - attachedDice.transform.position.z;
            if ((Mathf.Abs(x_diff) > DICE_EDGE_LIMIT) || (Mathf.Abs(z_diff) > DICE_EDGE_LIMIT))
            {
                Debug.Log("Going too far");
                //attachedDice.transform.position = nextPosition;
                //m_rigidBody.isKinematic = true;
                if (attachedDice.GetComponent<Dice>().m_rotating == false) {
                    attachedDice.GetComponent<Dice>().StartCoroutine("rotateLeft");
                }
                return ;
                // rotate + move cube
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