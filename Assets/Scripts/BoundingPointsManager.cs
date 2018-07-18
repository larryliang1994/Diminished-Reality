using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class BoundingPointsManager : MonoBehaviour {
    
    public static Vector2[] currentBoundingPointsPos;
    public static Vector2[] frame0BoundingPointsPos;

    private GameObject[] boundingPoints;

    private GameObject boundingPoint;

    private bool selecting = false;

    private bool selected = false;

    private int currentPoint = 0;

    public int boundingPointSize = 4;

    private GameObject sofa;

	void Start () 
    {
        boundingPoints = new GameObject[boundingPointSize];
        currentBoundingPointsPos = new Vector2[boundingPointSize];
        frame0BoundingPointsPos = new Vector2[boundingPointSize];

        boundingPoint = Resources.Load("Prefabs/BoundingPoint") as GameObject;

        //sofa = Resources.Load("ArchViz Sofa Pack - Lite/Sofa2/Prefabs/Pref_Sofa2_Cotton") as GameObject;
        //sofa = Instantiate(sofa);
        //sofa.transform.localScale = new Vector3(0f, 0f, 0f);

        for (int i = 0; i < boundingPointSize; i++)
        {
            boundingPoints[i] = Instantiate(boundingPoint);
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
                        //sofa.transform.position =
                        //    (boundingPoints[0].transform.position + boundingPoints[1].transform.position
                        //     + boundingPoints[2].transform.position + boundingPoints[3].transform.position) / 4;
                        //sofa.transform.position = new Vector3(sofa.transform.position.x, sofa.transform.position.y, boundingPoints[0].transform.position.z * 2f);
                        //sofa.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
                        break;

                    default:    
                        return;
                }

                currentPoint++;
            }
        }

        if (selected)
        {
            UpdateBoundingPoints();
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
            frame0BoundingPointsPos[i] = Camera.main.WorldToScreenPoint(boundingPoints[i].transform.position);
        }

        for (int i = 0; i < boundingPointSize; i++)
        {
            boundingPoints[i].GetComponent<MeshRenderer>().enabled = false;
        }

        GameObject.Find("Control Points").GetComponent<ControlPointsManager>().HideControlPoints();

        DRUtil.Instance.InpaintWithFourPoints();

        selected = true;
    }

    public void UpdateBoundingPoints()
    {
        for (int i = 0; i < boundingPointSize; i++)
        {
            currentBoundingPointsPos[i] = Camera.main.WorldToScreenPoint(boundingPoints[i].transform.position);
        }
    }
}
