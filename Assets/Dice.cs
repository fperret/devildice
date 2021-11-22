using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Dice : MonoBehaviour
{
    public enum Face {
        ONE = 1,
        TWO = 2,
        THREE,
        FOUR,
        FIVE,
        SIX,
        UNKNOWN
    };

    // We do not define UP and DOWN as we do not need them for now
    public enum FacePosition {
        BACK,
        LEFT,
        RIGHT,
        FRONT,
        UNKNOWN
    }

    // Start is called before the first frame update
    public Transform[] m_rotationPoints = new Transform[12];
    public Transform[] m_orientationPoints = new Transform[4];
    public GameObject m_orientationPointsParent;
    public BoxCollider m_leftCollider;
    public BoxCollider m_frontCollider;
    public BoxCollider m_rightCollider;
    public BoxCollider m_backCollider;
    public GameObject m_box;
    public bool m_diceInteractable = true;

    // Should be false by default
    public bool m_isClimbable = false;
    public bool m_rotating = false;
    public bool m_spawning = false;

    public bool m_isOtherDiceLeft = false;
    public bool m_isOtherDiceRight = false;
    public bool m_isOtherDiceFront = false;
    public bool m_isOtherDiceBack = false;
    public bool m_isPlayerOnDice = false;
    private Transform[] m_faces = new Transform[6];

    void Awake()
    {
        uint i = 0;
        foreach (Transform face in transform.Find("Faces"))
        {
            m_faces[i] = face;
            ++i;
        }
    }

    void Start()
    {
        //updateCheckNeighbors();
    }

    public void onPlayerGetOnDice()
    {
        m_isPlayerOnDice = true;
        if (m_diceInteractable) {
            updateCheckNeighbors();
        }
    }

    public void onPlayerGetOffDice()
    {
        // When the cube rotate the player can stop touching the dice
        // But it is not something that needs to be handled here
        if (m_rotating) {
            return ;
        }
        Debug.Log("player get off dice");
        m_isPlayerOnDice = false;
        setPrisonBoxCollidersState(false);
    }

    public FacePosition getClosestFace(Vector3 referencePosition)
    {
        float distBack = (referencePosition - m_orientationPoints[0].position).sqrMagnitude;
        float distLeft = (referencePosition - m_orientationPoints[1].position).sqrMagnitude;
        float distRight = (referencePosition - m_orientationPoints[2].position).sqrMagnitude;
        float distFront = (referencePosition - m_orientationPoints[3].position).sqrMagnitude;
        float[] allDists = { distBack, distLeft, distRight, distFront };
        switch (Array.IndexOf(allDists, allDists.Min())) {
            case 0:
                return FacePosition.BACK;
            case 1:
                return FacePosition.LEFT;
            case 2:
                return FacePosition.RIGHT;
            case 3:
                return FacePosition.FRONT;
            default:
                Debug.Log("Index of closest face not known");
                return FacePosition.UNKNOWN;
        }
    }

    public Face getFaceUp()
    {
        float maxY = m_faces[0].position.y;
        uint indexMaxY = 0;
        for (uint i = 0; i < 6; ++i)
        {
            if (m_faces[i].position.y > maxY) {
                maxY = m_faces[i].position.y;
                indexMaxY = i;
            }
        }
        switch (indexMaxY) {
            case 0:
                return Face.ONE;
            case 1:
                return Face.TWO;
            case 2:
                return Face.THREE;
            case 3:
                return Face.FOUR;
            case 4:
                return Face.FIVE;
            case 5:
                return Face.SIX;
            default:
                Debug.Log("Unknown index value for faces");
                return Face.UNKNOWN;
        }
    }

    public void setPrisonBoxCollidersState(bool enabled)
    {
        m_leftCollider.enabled = enabled;
        m_frontCollider.enabled = enabled;
        m_rightCollider.enabled = enabled;
        m_backCollider.enabled = enabled;
    }

    private bool approximateCheck(float a, float b)
    {
        return ((a >= (b - 0.01f)) && (a <= b + 0.01f));
    }

    private bool isAnotherDiceInDirection(Vector3 direction)
    {
        Debug.DrawLine(transform.position, transform.position + direction, Color.red, 1);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, 1);
        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].collider.CompareTag("Dice"))
                return true;
        }
        return false;
    }

    private void updateCheckNeighbors()
    {
        m_isOtherDiceLeft = isAnotherDiceInDirection(Vector3.left);
        m_isOtherDiceRight = isAnotherDiceInDirection(Vector3.right);
        m_isOtherDiceFront = isAnotherDiceInDirection(Vector3.forward);
        m_isOtherDiceBack = isAnotherDiceInDirection(Vector3.back);

        m_leftCollider.enabled = !m_isOtherDiceLeft;
        m_rightCollider.enabled = !m_isOtherDiceRight;
        m_frontCollider.enabled = !m_isOtherDiceFront;
        m_backCollider.enabled = !m_isOtherDiceBack;
    }

    private bool isRotationPossible(Vector3 direction)
    {
        // Ajouter check bordure de map
        if (direction == Vector3.left) {
            return !m_isOtherDiceLeft;
        } else if (direction == Vector3.right) {
            return !m_isOtherDiceRight;
        } else if (direction == Vector3.forward) {
            return !m_isOtherDiceFront;
        } else if (direction == Vector3.back) {
            return !m_isOtherDiceBack;
        }
        return false;
    }

    IEnumerator spawn()
    {
        if (m_spawning) {
            yield break;
        }
        m_spawning = true;
        while (transform.position.y < 0.5f) {
            transform.Translate(Vector3.up * Time.deltaTime * 0.3f);
            yield return new WaitForSeconds(0.005f);
        }
        m_spawning = false;
        updateCheckNeighbors();
        yield break;
    }

    IEnumerator rotateLeft()
    {
        float lowest_x = findGroundLowestX();
        Vector3? rotationPoint = findGroundPointWithXCoord(lowest_x);
        if (rotationPoint.HasValue) {
            if (!isRotationPossible(Vector3.left))
                yield break;
            m_rotating = true;

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                m_box.transform.Rotate(0, 0, -1);
                m_orientationPointsParent.transform.Rotate(0, 0, -1);
                transform.RotateAround(rotationPoint.Value, Vector3.forward, 1);
                yield return new WaitForSeconds(0.005f);
            }

            yield return new WaitForSeconds(0.1f);
            updateCheckNeighbors();
            m_rotating = false;
            
        }
        yield break;
    }

    IEnumerator rotateRight()
    {
        float highest_x = findGroundHighestX();
        Vector3? rotationPoint = findGroundPointWithXCoord(highest_x);
        if (rotationPoint.HasValue)
        {
            if (!isRotationPossible(Vector3.right))
                yield break;
            m_rotating = true;

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                m_box.transform.Rotate(0, 0, 1);
                m_orientationPointsParent.transform.Rotate(0, 0, 1);
                transform.RotateAround(rotationPoint.Value, Vector3.back, 1);
                yield return new WaitForSeconds(0.005f);
            }
            
            yield return new WaitForSeconds(0.1f);
            updateCheckNeighbors();
            m_rotating = false;
        }
        yield break;
    }

    IEnumerator rotateForward()
    {
        float highest_z = findGroundHighestZ();
        Vector3? rotationPoint = findGroundPointWithZCoord(highest_z);
        if (rotationPoint.HasValue)
        {
            if (!isRotationPossible(Vector3.forward))
                yield break;
            m_rotating = true;

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                m_box.transform.Rotate(-1, 0, 0);
                m_orientationPointsParent.transform.Rotate(-1, 0, 0);
                transform.RotateAround(rotationPoint.Value, Vector3.right, 1);
                yield return new WaitForSeconds(0.005f);
            }
        
            yield return new WaitForSeconds(0.1f);
            updateCheckNeighbors();
            m_rotating = false;
        }
        yield break;
    }

    IEnumerator rotateBackward()
    {
        float lowest_z = findGroundLowestZ();
        Vector3? rotationPoint = findGroundPointWithZCoord(lowest_z);
        if (rotationPoint.HasValue)
        {
            if (!isRotationPossible(Vector3.back))
                yield break;
            m_rotating = true;

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                m_box.transform.Rotate(1, 0, 0);
                m_orientationPointsParent.transform.Rotate(1, 0, 0);
                transform.RotateAround(rotationPoint.Value, Vector3.left, 1);
                yield return new WaitForSeconds(0.005f);
            }
            
            yield return new WaitForSeconds(0.1f);
            updateCheckNeighbors();
            m_rotating = false;
        }
        yield break;
    }

    private float findGroundLowestZ()
    {
        float lowest_z = m_rotationPoints[0].position.z;
        for (int i = 0; i < 12; ++i) {
            if (m_rotationPoints[i].position.z < lowest_z)
                lowest_z = m_rotationPoints[i].position.z;
        }
        return lowest_z;
    }

    private float findGroundHighestZ()
    {
        float highest_z = m_rotationPoints[0].position.z;
        for (int i = 0; i < 12; ++i) {
            if (m_rotationPoints[i].position.z > highest_z)
                highest_z = m_rotationPoints[i].position.z;
        }
        return highest_z;
    }

    private float findGroundLowestX()
    {
        float lowest_x = m_rotationPoints[0].position.x;
        for (int i = 0; i < 12; ++i) {
            if (m_rotationPoints[i].position.x < lowest_x)
                lowest_x = m_rotationPoints[i].position.x;
        }
        return lowest_x;
    }

    private float findGroundHighestX()
    {
        float highest_x = m_rotationPoints[0].position.x;
        for (int i = 0; i < 12; ++i) {
            if (m_rotationPoints[i].position.x > highest_x)
                highest_x = m_rotationPoints[i].position.x;
        }
        return highest_x;
    }

    private Vector3? findGroundPointWithXCoord(float x)
    {
        foreach (Transform pointTransform in m_rotationPoints)
        {
            if (approximateCheck(pointTransform.position.y, 0))
            {
                if (approximateCheck(pointTransform.position.x, x))
                    return pointTransform.position;
            }
        }
        Debug.LogError("NOT NORMAL Rotation point not found for x : " + x);
        return null;
    }

    private Vector3? findGroundPointWithZCoord(float z)
    {
        foreach (Transform pointTransform in m_rotationPoints)
        {
            if (approximateCheck(pointTransform.position.y, 0))
            {
                if (approximateCheck(pointTransform.position.z, z)) {
                    return pointTransform.position;
                }
            }
        }
        Debug.LogError("NOT NORMAL Rotation point not found for z : " + z);
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            isAnotherDiceInDirection(Vector3.left);
            isAnotherDiceInDirection(Vector3.right);
            isAnotherDiceInDirection(Vector3.forward);
            isAnotherDiceInDirection(Vector3.back);
            Debug.Log(getFaceUp());
        }
        // For testing
        if (Input.GetKey(KeyCode.Space) && name == "Dice")
        {
            transform.Translate(Vector3.down * Time.deltaTime * 0.3f, Space.World);
        }
        if (Input.GetKey(KeyCode.U) && transform.position.y < 0.5f)
        {
            StartCoroutine("spawn");
            //transform.Translate(Vector3.up * Time.deltaTime * 0.3f, Space.World);
        }

        // Dice is interactable only when it is at its max height
        m_diceInteractable = approximateCheck(transform.position.y, 0.5f);
        if (transform.position.y < 0.25f)
        {
            setPrisonBoxCollidersState(false);
            //m_isClimbable = true;                    
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < 12; ++i)
        {
            Gizmos.DrawSphere(m_rotationPoints[i].position, 0.1f);
        }
        for (int i = 0; i < 4; ++i)
        {
            Gizmos.DrawSphere(m_orientationPoints[i].position, 0.1f);
        }
    }
}
