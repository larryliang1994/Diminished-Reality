using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class BoundingPointsManager : MonoBehaviour 
{
    private Vector2[] frame0BoundingPointsPos;
    private Vector2[] currentBoundingPointsPos;

    private GameObject[] boundingPoints;

    private GameObject boundingPoint;

    private bool selecting = false;

    private int currentPoint = 0;

    public int boundingPointSize = 4;

    private GameObject cube;

	void Start () 
    {
        boundingPoints = new GameObject[boundingPointSize];
        currentBoundingPointsPos = new Vector2[boundingPointSize];
        frame0BoundingPointsPos = new Vector2[boundingPointSize];

        boundingPoint = Resources.Load("Prefabs/BoundingPoint") as GameObject;

        cube = Resources.Load("Prefabs/Cube") as GameObject;
        cube = Instantiate(cube);
        cube.transform.localScale = new Vector3(0f, 0f, 0f);

        for (int i = 0; i < boundingPointSize; i++)
        {
            boundingPoints[i] = Instantiate(boundingPoint);
            boundingPoints[i].GetComponent<MeshRenderer>().enabled = false;
        }
	}
	
	void Update () 
    {
        if (selecting && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // get the mask to raycast against either the player or ground layer
            int lMask = LayerMask.GetMask("Player", "Ground");

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, lMask))
            {
                switch (currentPoint)
                {
                    case 0: boundingPoints[0].transform.position = hit.point;    
                        break;

                    case 1: boundingPoints[1].transform.position = hit.point;    
                        break;

                    case 2: boundingPoints[2].transform.position = hit.point;    
                        break;

                    case 3: boundingPoints[3].transform.position = hit.point;
                        GameObject.Find("Control Points").GetComponent<ControlPointsManager>().CreateControlPoints(boundingPoints);
                        cube.transform.position = (boundingPoints[0].transform.position + boundingPoints[1].transform.position) / 2;
                        cube.transform.position = new Vector3(cube.transform.position.x, cube.transform.position.y, boundingPoints[0].transform.position.z + 0.15f);
                        cube.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
                        break;

                    default:    
                        return;
                }

                currentPoint++;
            }
        }
	}

    public void StartSelecting()
    {
        for (int i = 0; i < boundingPointSize; i++)
        {
            boundingPoints[i].GetComponent<MeshRenderer>().enabled = true;
        }

        selecting = true;
        currentPoint = 0;
    }

    public void DoneSelecting()
    {
        selecting = false;
        currentPoint = 1;

        for (int i = 0; i < boundingPointSize; i++)
        {
            boundingPoints[i].GetComponent<MeshRenderer>().enabled = false;
        }

        GameObject.Find("Control Points").GetComponent<ControlPointsManager>().HideControlPoints();

        for (int i = 0; i < boundingPointSize; i++)
        {
            frame0BoundingPointsPos[i] = Camera.main.WorldToScreenPoint(boundingPoints[i].transform.position);
        }

        DRUtil.Instance.InpaintWithFourPoints(frame0BoundingPointsPos);
    }

    public Vector2[] GetCurrentBoundingPointsPos()
    {
        for (int i = 0; i < boundingPointSize; i++)
        {
            currentBoundingPointsPos[i] = Camera.main.WorldToScreenPoint(boundingPoints[i].transform.position);
        }

        return currentBoundingPointsPos;
    }
}
