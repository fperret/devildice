using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testcharactercontroller : MonoBehaviour
{
    public CharacterController characterController;
    private Player player;
    public Vector3 m_moveOverload;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        player = GetComponent<Player>();
        m_moveOverload = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
        if (m_moveOverload != Vector3.zero) {
        //if (player.m_isPushing) {
            move = m_moveOverload;
        }
        characterController.SimpleMove(speed * move);
    }
}
