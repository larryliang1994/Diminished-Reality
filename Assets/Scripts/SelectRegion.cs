using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectRegion : MonoBehaviour 
{
    // The selection square we draw when we drag the mouse to select units
    public RectTransform selectionSquareTrans;

    // The start and end coordinates of the square we are making
    Vector3 squareStartPos;
    Vector3 squareEndPos;

    private Vector3 finalPosition = new Vector3();
    private Vector2 finalSize = new Vector2();

    private bool canSelect = false;

	// Use this for initialization
	void Start () 
    {
        // Deactivate the square selection image
        selectionSquareTrans.gameObject.SetActive(false);
	}

    public void StartSelecting()
    {
        canSelect = true;
        Debug.Log(Screen.width+"*"+Screen.height);
    }

    public void CancelSelecting()
    {
        canSelect = false;
    }

    public void DoneSelecting()
    {
        CancelSelecting();

        double x = finalPosition.x - finalSize.x / 2;
        double y = finalPosition.y + finalSize.y / 2;
        double width  = finalSize.x;
        double height = finalSize.y;

        //DRUtil.Instance.SetCurrentRect(x, y, width, height);

        Debug.Log("finalPosition = " + finalPosition);
        Debug.Log("finalSize = " + finalSize);
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (canSelect)
        {
            Selecting();
        }
	}

    void Selecting()
    {
        // Click the mouse button
        if (Input.GetMouseButtonDown(0))
        {
            squareStartPos = Input.mousePosition;
        }

        // Release the mouse button
        if (Input.GetMouseButtonUp(0))
        {
            if (Mathf.Abs(squareStartPos.x - squareEndPos.x) > 10)
            {
                finalPosition = selectionSquareTrans.position;
                finalSize = selectionSquareTrans.sizeDelta;
            }
        }

        // Drag the mouse to select
        if (Input.GetMouseButton(0))
        {
            // Activate the square selection image
            if (!selectionSquareTrans.gameObject.activeInHierarchy)
            {
                selectionSquareTrans.gameObject.SetActive(true);
            }

            // Get the latest coordinate of the square
            squareEndPos = Input.mousePosition;

            // Display the selection with a GUI image
            DisplaySquare();
        }
    }

    //Display the selection with a GUI square
    void DisplaySquare()
    {
        // Get the middle position of the square
        Vector3 middle = (squareStartPos + squareEndPos) / 2f;
        middle.z = selectionSquareTrans.position.z;

        // Set the middle position of the GUI square
        selectionSquareTrans.position = middle;

        // Change the size of the square
        float sizeX = Mathf.Abs(squareStartPos.x - squareEndPos.x);
        float sizeY = Mathf.Abs(squareStartPos.y - squareEndPos.y);

        // Set the size of the square
        selectionSquareTrans.sizeDelta = new Vector2(sizeX, sizeY);
    }
}
