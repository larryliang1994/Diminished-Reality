using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointsManager : MonoBehaviour {
    
    public static Vector2[] currentControlPointsPos;
    public static Vector2[] frame0ControlPointsPos;

    private GameObject[] controlPoints;

    private GameObject controlPoint;

    public int controlPointSize = 8;
    public int illuminationSize = 15;

    private bool set = false;

	void Start () 
    {
        controlPoints = new GameObject[controlPointSize];
        currentControlPointsPos = new Vector2[controlPointSize];
        frame0ControlPointsPos = new Vector2[controlPointSize];

        controlPoint = Resources.Load("Prefabs/ControlPoint") as GameObject;

        for (int i = 0; i < controlPointSize; i++)
        {
            controlPoints[i] = Instantiate(controlPoint);
        }
	}
	
	void Update () 
    {
        if (set)
        {
            UpdateControlPoints();
        }
	}

    public void HideControlPoints()
    {
        for (int i = 0; i < controlPointSize; i++)
        {
            controlPoints[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void CreateControlPoints(GameObject[] boundingPoints)
    {
        for (int i = 0; i < controlPointSize; i++)
        {
            controlPoints[i].GetComponent<MeshRenderer>().enabled = true;
        }

        float offset = 0.035f;

        Vector3 position0 = boundingPoints[0].transform.position;
        Vector3 position1 = boundingPoints[1].transform.position;
        Vector3 position2 = boundingPoints[3].transform.position;
        Vector3 position3 = boundingPoints[2].transform.position;

        int corner = controlPointSize / 4;

        // top left
        position0 = new Vector3(position0.x - offset, 0.0f, position0.z + offset);
        controlPoints[corner * 0].transform.position = position0;
        // top right
        position1 = new Vector3(position1.x + offset, 0.0f, position1.z + offset);
        controlPoints[corner * 1].transform.position = position1;
        // bottom right
        position2 = new Vector3(position2.x + offset, 0.0f, position2.z - offset);
        controlPoints[corner * 2].transform.position = position2;
        // bottom left
        position3 = new Vector3(position3.x - offset, 0.0f, position3.z - offset);
        controlPoints[corner * 3].transform.position = position3;

        float x, z;
        for (int i = 0; i < controlPointSize; i++)
        {
            // top
            if (i > corner * 0 && i < corner * 1)
            {
                if (position0.x < position1.x)
                {
                    x = (position1.x - position0.x) / corner * (i % corner) + position0.x;
                }
                else
                {
                    x = (position0.x - position1.x) / corner * (corner * 1 - i) + position1.x;
                }

                if (position0.z < position1.z)
                {
                    z = (position1.z - position0.z) / corner * (i % corner) + position0.z;
                }
                else
                {
                    z = (position0.z - position1.z) / corner * (corner * 1 - i) + position1.z;
                }

                z += Random.Range(0, offset);

                controlPoints[i].transform.position = new Vector3(x, 0.0f, z);
            }
            // right
            else if (i > corner * 1 && i < corner * 2)
            {
                if (position1.x < position2.x)
                {
                    x = (position2.x - position1.x) / corner * (i % corner) + position1.x;
                }
                else
                {
                    x = (position1.x - position2.x) / corner * (corner * 2 - i) + position2.x;
                }

                if (position1.z < position2.z)
                {
                    z = (position2.z - position1.z) / corner * (i % corner) + position1.z;
                }
                else
                {
                    z = (position1.z - position2.z) / corner * (corner * 2 - i) + position2.z;
                }

                x += Random.Range(0, offset);

                controlPoints[i].transform.position = new Vector3(x, 0.0f, z);
            }
            // bottom
            else if (i > corner * 2 && i < corner * 3)
            {
                if (position2.x < position3.x)
                {
                    x = (position3.x - position2.x) / corner * (i % corner) + position2.x;
                }
                else
                {
                    x = (position2.x - position3.x) / corner * (corner * 3 - i) + position3.x;
                }

                if (position2.z < position3.z)
                {
                    z = (position3.z - position2.z) / corner * (i % corner) + position2.z;
                }
                else
                {
                    z = (position2.z - position3.z) / corner * (corner * 3 - i) + position3.z;
                }

                z -= Random.Range(0, offset);

                controlPoints[i].transform.position = new Vector3(x, 0.0f, z);
            }
            // left
            else if (i > corner * 3 && i < corner * 4)
            {
                if (position3.x < position0.x)
                {
                    x = (position0.x - position3.x) / corner * (i % corner) + position3.x;
                }
                else
                {
                    x = (position3.x - position0.x) / corner * (corner * 4 - i) + position0.x;
                }

                if (position3.z < position0.z)
                {
                    z = (position0.z - position3.z) / corner * (i % corner) + position3.z;
                }
                else
                {
                    z = (position3.z - position0.z) / corner * (corner * 4 - i) + position0.z;
                }

                x -= Random.Range(0, offset);

                controlPoints[i].transform.position = new Vector3(x, 0.0f, z);
            }
        }

        for (int i = 0; i < controlPointSize; i++)
        {
            frame0ControlPointsPos[i] = Camera.main.WorldToScreenPoint(controlPoints[i].transform.position);
        }

        set = true;
    }

    void UpdateControlPoints()
    {
        for (int i = 0; i < controlPointSize; i++)
        {
            currentControlPointsPos[i] = Camera.main.WorldToScreenPoint(controlPoints[i].transform.position);
        }
    }
}
