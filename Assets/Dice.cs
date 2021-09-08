using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform[] m_rotationPoints = new Transform[12];
    public BoxCollider m_leftCollider;
    public BoxCollider m_frontCollider;
    public BoxCollider m_rightCollider;
    public BoxCollider m_backCollider;
    public GameObject m_box;

    public bool m_rotating = false;

    void Start()
    {
    }

    private void setPrisonBoxCollidersState(bool enabled)
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

    IEnumerator rotateLeft()
    {
        float lowest_x = findGroundLowestX();
        Vector3? rotationPoint = findGroundPointWithXCoord(lowest_x);
        if (rotationPoint.HasValue)
        {
            m_rotating = true;

            // remove colliders
            setPrisonBoxCollidersState(false);

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                transform.RotateAround(rotationPoint.Value, Vector3.forward, 1);
                yield return new WaitForSeconds(0.01f);
            }
            m_rotating = false;

            // Replace colliders on top and re-activate them
            m_box.transform.Rotate(0, 0, -90);
            setPrisonBoxCollidersState(true);
            
        }
        yield break;
    }

    IEnumerator rotateRight()
    {
        float highest_x = findGroundHighestX();
        Vector3? rotationPoint = findGroundPointWithXCoord(highest_x);
        if (rotationPoint.HasValue)
        {
            m_rotating = true;

            setPrisonBoxCollidersState(false);

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                transform.RotateAround(rotationPoint.Value, Vector3.back, 1);
                yield return new WaitForSeconds(0.01f);
            }
            m_rotating = false;

            // Replace colliders on top and re-activate them
            m_box.transform.Rotate(0, 0, 90);
            setPrisonBoxCollidersState(true);
        }
        yield break;
    }

    IEnumerator rotateForward()
    {
        float highest_z = findGroundHighestZ();
        Vector3? rotationPoint = findGroundPointWithZCoord(highest_z);
        if (rotationPoint.HasValue)
        {
            m_rotating = true;

            setPrisonBoxCollidersState(false);

            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                transform.RotateAround(rotationPoint.Value, Vector3.right, 1);
                yield return new WaitForSeconds(0.01f);
            }
            m_rotating = false;

            // Replace colliders on top and re-activate them
            m_box.transform.Rotate(0, 0, -90);
            setPrisonBoxCollidersState(true);
        }
        yield break;
    }

    IEnumerator rotateBackward()
    {
        float lowest_z = findGroundLowestZ();
        Vector3? rotationPoint = findGroundPointWithZCoord(lowest_z);
        if (rotationPoint.HasValue)
        {
            m_rotating = true;

            setPrisonBoxCollidersState(false);


            for (int rotateLoopIndex = 0; rotateLoopIndex < 90; ++rotateLoopIndex) {
                transform.RotateAround(rotationPoint.Value, Vector3.left, 1);
                yield return new WaitForSeconds(0.01f);
            }
            m_rotating = false;

            // Replace colliders on top and re-activate them
            m_box.transform.Rotate(0, 0, 90);
            setPrisonBoxCollidersState(true);
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
                if (approximateCheck(pointTransform.position.z, z))
                    return pointTransform.position;
            }
        }
        Debug.LogError("NOT NORMAL Rotation point not found for z : " + z);
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_rotating)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                StartCoroutine("rotateLeft");
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                StartCoroutine("rotateRight");
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                StartCoroutine("rotateForward");
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                StartCoroutine("rotateBackward");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < 12; ++i)
        {
            Gizmos.DrawSphere(m_rotationPoints[i].position, 0.1f);
        }
    }
}
