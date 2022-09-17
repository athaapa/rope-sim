using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RopeSimulation : MonoBehaviour
{

    public GameObject parent;

    public Camera mainCamera;
    public Sprite dotTexture;
    public GameObject lineParent;

    public Material normalTexture;

    bool startRender = false;

    public float gravity;
    public float energyLossFactor;

    List<Point> points;
    List<Stick> sticks;

    void Start()
    {
        points = new List<Point>();
        sticks = new List<Stick>();

    }

    void InputLoop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreatePoint(Input.mousePosition);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Returned");
            startRender = true;
            CreateSticks();
        }
    }
    
    void CreatePoint(Vector2 mousePos)
    {
        GameObject newPoint = new GameObject();
        newPoint.name = "Point " + points.Count;
        Image image = newPoint.AddComponent<Image>();
        image.sprite = dotTexture;
        image.color = Color.black;
        newPoint.transform.SetParent(parent.transform);
        RectTransform rt = newPoint.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(32, 32);
        newPoint.transform.position = mousePos;
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            points.Add(new Point(mousePos, newPoint, true));
            image.color = Color.red;
        }
        else
            points.Add(new Point(mousePos, newPoint));
    }
    void CreateSticks()
    {
        for (int i = 1; i < points.Count; i++)
        {

            Point pointA = points[i-1];
            Point pointB = points[i];
            // add a new stick to the list for every point

            GameObject line = new GameObject();
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();

            float magnitude = (pointA.position - pointB.position).magnitude;

            sticks.Add(new Stick(pointA, pointB, magnitude, lineRenderer));

            line.transform.SetParent(lineParent.transform);

            Vector3 screenPointA = Camera.main.ScreenToWorldPoint(pointA.position);
            Vector3 screenPointB = Camera.main.ScreenToWorldPoint(pointB.position);

            lineRenderer.SetPosition(0, new Vector3(screenPointA.x, screenPointA.y, 0f));
            lineRenderer.SetPosition(1, new Vector3(screenPointB.x, screenPointB.y, 0f));
            
            lineRenderer.startWidth = 0.2f;
            lineRenderer.startColor = Color.red;

            lineRenderer.material = normalTexture;
        }
    }

    void Update()
    {
        InputLoop();
        if (startRender)
        {
            Simulate();
        }
    }

    void Simulate()
    {
        foreach (Point p in points)
        {
            if (!p.locked)
            {
                Vector2 posBeforeUpdate = p.position;
                Vector2 delta = (p.position - p.prevPosition) * (1 - energyLossFactor);
                p.position += delta;
                p.position += Vector2.down * gravity * Time.deltaTime * Time.deltaTime;
                p.prevPosition = posBeforeUpdate;
                p.Update();
            }
        }

        foreach (Stick s in sticks)
        {
            Vector2 stickCenter = (s.pointA.position + s.pointB.position) / 2;
            Vector2 stickDir = (s.pointA.position - s.pointB.position).normalized;
            if (!s.pointA.locked)
                s.pointA.position = stickCenter + stickDir * s.length / 2;
            if (!s.pointB.locked)
                s.pointB.position = stickCenter - stickDir * s.length / 2;
            s.Update();
        }
    }
}

public class Point
{
    public Vector2 position, prevPosition;
    public bool locked;
    public GameObject obj;

    public Point(Vector2 pos, GameObject obj, bool locked = false)
    {
        this.position = pos;
        this.prevPosition = pos;
        this.locked = locked;
        this.obj = obj;
    }

    public void Update()
    {
        obj.transform.position = position;
    }
}

public class Stick
{
    public Point pointA, pointB;
    public float length;
    public LineRenderer lineRenderer;

    public Stick(Point pointA, Point pointB, float length, LineRenderer lineRenderer)
    {
        this.pointA = pointA;
        this.pointB = pointB;
        this.length = length;

        this.lineRenderer = lineRenderer;
    }

    public void Update()
    {
        Vector3 screenPointA = Camera.main.ScreenToWorldPoint(pointA.position);
        Vector3 screenPointB = Camera.main.ScreenToWorldPoint(pointB.position);

        lineRenderer.SetPosition(0, new Vector3(screenPointA.x, screenPointA.y, 0f));
        lineRenderer.SetPosition(1, new Vector3(screenPointB.x, screenPointB.y, 0f));
    }
}