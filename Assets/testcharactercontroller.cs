using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testcharactercontroller : MonoBehaviour
{
    public CharacterController characterController;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
        if (characterController.enabled) {
            characterController.SimpleMove(speed * move);
        }
    }
}
