using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateFourPoints : MonoBehaviour {

    public GameObject myPoint1, myPoint2, myPoint3, myPoint4;

    public static Vector2[] currentPoints = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };

    public static Vector2[] frame0Points = { new Vector2(), new Vector2(), new Vector2(), new Vector2() };

    private bool selecting = false;

    private bool selected = false;

    private int currentPoint = 1;

	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (selecting && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //get the mask to raycast against either the player or ground layer
            int lMask = LayerMask.GetMask("Player", "Ground");
            //player only
            //int lMask = LayerMask.GetMask("Player");

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, lMask))
            {
                switch (currentPoint)
                {
                    case 1:
                        myPoint1.transform.position = hit.point;
                        break;

                    case 2:
                        myPoint2.transform.position = hit.point;
                        break;

                    case 3:
                        myPoint3.transform.position = hit.point;
                        break;

                    case 4:
                        myPoint4.transform.position = hit.point;
                        break;

                    default:
                        return;
                }

                currentPoint++;


                //Vector3 screenPos = Camera.main.WorldToScreenPoint(myPoint.transform.position);

                //Debug.Log(hit.point);
                //Debug.Log("mousePosition = "+Input.mousePosition);
                //Debug.Log("screenPos = "+screenPos);
            }
        }

        if (selected)
        {
            UpdateFourPoints();
        }
	}

    public void StartSelecting()
    {
        selecting = true;
    }

    public void DoneSelecting()
    {
        selecting = false;
        currentPoint = 1;

        Vector2 screenPos1 = Camera.main.WorldToScreenPoint(myPoint1.transform.position);
        Vector2 screenPos2 = Camera.main.WorldToScreenPoint(myPoint2.transform.position);
        Vector2 screenPos3 = Camera.main.WorldToScreenPoint(myPoint3.transform.position);
        Vector2 screenPos4 = Camera.main.WorldToScreenPoint(myPoint4.transform.position);

        Debug.Log("screenPos1 = " + screenPos1);
        Debug.Log("screenPos2 = " + screenPos2);
        Debug.Log("screenPos3 = " + screenPos3);
        Debug.Log("screenPos4 = " + screenPos4);

        frame0Points[0] = screenPos1;
        frame0Points[1] = screenPos2;
        frame0Points[2] = screenPos3;
        frame0Points[3] = screenPos4;

        DRUtil.Instance.InpaintWithFourPoints(frame0Points);

        selected = true;
    }

    public void UpdateFourPoints()
    {
        Vector2 screenPos1 = Camera.main.WorldToScreenPoint(myPoint1.transform.position);
        Vector2 screenPos2 = Camera.main.WorldToScreenPoint(myPoint2.transform.position);
        Vector2 screenPos3 = Camera.main.WorldToScreenPoint(myPoint3.transform.position);
        Vector2 screenPos4 = Camera.main.WorldToScreenPoint(myPoint4.transform.position);

        //Debug.Log("screenPos1 = " + screenPos1);
        //Debug.Log("screenPos2 = " + screenPos2);
        //Debug.Log("screenPos3 = " + screenPos3);
        //Debug.Log("screenPos4 = " + screenPos4);

        currentPoints[0] = screenPos1;
        currentPoints[1] = screenPos2;
        currentPoints[2] = screenPos3;
        currentPoints[3] = screenPos4;
    }
}
