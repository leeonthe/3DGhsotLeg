using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostLegRenderer : MonoBehaviour
{
    private GameObject root = null;

    public Sprite[] animalSprites;

    public float radius = 5f;
    public float legLength = 20f;

    public Color legStartColor = Color.blue;
    public Color legEndColor = Color.blue;
    public float legLineWidth = 1f;

    public Color bridgeColor = Color.red;
    public float bridgeLineWidth = 2f;

    public Material lineRendererMaterial;

    public float moveSpeed = 5f;

    public GameObject follower;
    private GameObject path;

    public GameObject canvas;
    public GameObject aniamlIcon;


    private List<Leg> currentMap;
    private List<GameObject> legGameObjects;

    private void Awake()
    {
    }

    private void Start()
    {
        //test();

    }

    [ContextMenu("refresh map")]
    public void test()
    {
        if (path != null)
        {
            Destroy(path);
        }
        int players = 5;
        List<Leg> newMap = new List<Leg>();
        for (int i = 0; i < players; i++)
        {
            newMap.Add(new Leg());
        }

        createGhostLeg(newMap);
        CreatePlayerInputFields(newMap.Count);
        int choice = UnityEngine.Random.Range(0, players);
        Vector3[] waypoints = computeMovePath(newMap, choice);
        for (int i = 0; i < waypoints.Length; i++)
        {
            Debug.Log("waypoint " + i + ": " + waypoints[i]);
        }

        path = new GameObject("path");
        LineRenderer lr = path.AddComponent<LineRenderer>();
        lr.positionCount = waypoints.Length;
        int idx = 0;
        foreach (Vector3 wp in waypoints)
        {
            lr.SetPosition(idx++, wp);
        }
        lr.material = lineRendererMaterial;
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;
        lr.startWidth = 0.7f;
        lr.endWidth = 0.7f;

        StartCoroutine(moveFollower(waypoints, choice, players));
    }

    public IEnumerator moveFollower(Vector3[] path, int startIndex, int numLeg)
    {
        float angleDelta = Mathf.PI * 2 / numLeg;
        Vector3 from = Vector3.zero;
        if (startIndex >= 0)
        {
            float x = root.transform.position.x + radius * Mathf.Cos(angleDelta * startIndex);
            float y = root.transform.position.y + radius * Mathf.Sin(angleDelta * startIndex);

            from = new Vector3(x, legLength, y);
        }
        else
        {
            from = follower.transform.position;
        }
        Vector3 to;
        int currentWaypoint = 0;
        follower.transform.position = from;

        while (currentWaypoint < path.Length)
        {
            to = path[currentWaypoint];
            float dist = (to - from).magnitude;
            float timeToGetThere = dist / moveSpeed;
            float currentTime = 0;
            while (currentTime < timeToGetThere)
            {
                Vector3 midPt = Vector3.Lerp(from, to, currentTime / timeToGetThere); //linear interpolation
                follower.transform.position = midPt;
                yield return new WaitForEndOfFrame();
                currentTime += Time.deltaTime;
            }
            follower.transform.position = to;

            from = to;
            currentWaypoint++;
        }
    }

    public Vector3[] computeMovePath(List<Leg> map, int startIndex)
    {
        List<Vector3> toReturn = new List<Vector3>();

        int numLeg = map.Count;

        int currentIndex = startIndex;
        Bridge curBridge = map[currentIndex].bridges[0];
        toReturn.Add(getPositionFromBridgeAndIndex(curBridge, currentIndex, numLeg));
        while (curBridge != null)
        {
            int otherIndex = (curBridge.index1 == currentIndex) ? curBridge.index2 : curBridge.index1;
            int curIdx = 0;
            foreach (Bridge b in map[otherIndex].bridges)
            {
                if (b == curBridge)
                {
                    break;
                }
                curIdx++;
            }
            //curBridge = map[otherIndex][curIdx];

            currentIndex = otherIndex;

            toReturn.Add(getPositionFromBridgeAndIndex(curBridge, currentIndex, numLeg));

            if (curIdx + 1 == map[currentIndex].bridges.Count)
            {
                curBridge = null;
            }
            else
            {
                curBridge = map[currentIndex].bridges[curIdx + 1];
                toReturn.Add(getPositionFromBridgeAndIndex(curBridge, currentIndex, numLeg));
            }
        }

        //at the last bridge end 
        toReturn.Add(getPositionFromBridgeAndIndex(null, currentIndex, numLeg));

        return toReturn.ToArray();
    }

    private Vector3 getPositionFromBridgeAndIndex(Bridge bridge, int index, int numLeg)
    {
        float pos = 0.0f;
        float angleDelta = Mathf.PI * 2 / numLeg;
        float x = root.transform.position.x + radius * Mathf.Cos(angleDelta * index);
        float y = root.transform.position.y + radius * Mathf.Sin(angleDelta * index);
        if (bridge != null)
        {
            if (index == bridge.index1)
            {
                pos = bridge.position1;
            }
            else
            {
                pos = bridge.position2;
            }
        }
        else
        {
            pos = 1.0f;
        }
        float height = (1 - pos) * legLength;
        return new Vector3(x, height, y);
    }

    public void createGhostLeg(List<Leg> map)
    {
        if (root != null)
        {
            Destroy(root);
        }
        root = new GameObject("GhostLeg");

        currentMap = map;

        int numLeg = map.Count;
        float angleDelta = Mathf.PI * 2 / numLeg;

        GameObject[] legs = new GameObject[numLeg];
        for (int i = 0; i < map.Count; i++)
        {
            float x = root.transform.position.x + radius * Mathf.Cos(angleDelta * i);
            float y = root.transform.position.y + radius * Mathf.Sin(angleDelta * i);

            GameObject line = new GameObject("Leg " + i);
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.material = lineRendererMaterial;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x, legLength, y));
            lr.SetPosition(1, new Vector3(x, 0, y));
            lr.startColor = legStartColor;
            lr.endColor = legEndColor;

            lr.startWidth = legLineWidth;
            lr.endWidth = legLineWidth;

            legs[i] = line;

            line.transform.position = new Vector3(x, 0, y);

            GameObject legSprite = createAnimalIcon(map[i].animal);
            GameObject spriteAnchor = new GameObject("Sprite Anchor " + i);
            spriteAnchor.transform.position = line.transform.position + Vector3.up * legLength;
            spriteAnchor.transform.SetParent(line.transform);
            FollowWorldObject fwo = legSprite.GetComponent<FollowWorldObject>();
            fwo.setTarget(spriteAnchor);
            legSprite.transform.SetParent(canvas.transform);

            line.transform.SetParent(root.transform);
        }
        legGameObjects = new List<GameObject>(legs);

        List<Bridge> created = new List<Bridge>();
        for (int i = 0; i < map.Count; i++)
        {
            foreach (Bridge b in map[i].bridges)
            {
                if (!created.Contains(b))
                {
                    Vector3 pos1 = legs[b.index1].transform.position;
                    Vector3 pos2 = legs[b.index2].transform.position;

                    float height1 = (1.0f - b.position1) * legLength;
                    float height2 = (1.0f - b.position2) * legLength;

                    pos1.y = height1;
                    pos2.y = height2;

                    GameObject newBridge = new GameObject($"bridge:({b.index1}:{b.position1})-({b.index2}:{b.position2})");
                    LineRenderer lr = newBridge.AddComponent<LineRenderer>();
                    lr.material = lineRendererMaterial;
                    lr.positionCount = 2;
                    lr.SetPosition(0, pos1);
                    lr.SetPosition(1, pos2);
                    lr.startColor = bridgeColor;
                    lr.endColor = bridgeColor;

                    lr.startWidth = bridgeLineWidth;
                    lr.endWidth = bridgeLineWidth;

                    created.Add(b);
                    newBridge.transform.SetParent(root.transform);
                }

            }
        }
        CreatePlayerInputFields(map.Count);
    }

    private GameObject createAnimalIcon(AnimalEnum animalType)
    {
        GameObject aIcon = Instantiate(aniamlIcon);
        Image img = aIcon.GetComponent<Image>();

        if (animalType == AnimalEnum.bear)
        {
            img.sprite = animalSprites[0];
        }
        else if (animalType == AnimalEnum.byung)
        {
            img.sprite = animalSprites[1];
        }
        else if (animalType == AnimalEnum.panda)
        {
            img.sprite = animalSprites[2];
        }
        else if (animalType == AnimalEnum.tiger)
        {
            img.sprite = animalSprites[3];
        }
        else if (animalType == AnimalEnum.pig)
        {
            img.sprite = animalSprites[4];
        }
        else //AnimalEnum.F
        {
            img.sprite = animalSprites[5];
        }
        
        //create if else for other animals 
        return aIcon;
    }

    public void CreatePlayerInputFields(int numPlayers)
    {
        foreach (Transform child in canvas.transform)
        {
            if (child.name.StartsWith("PlayerInput"))
                Destroy(child.gameObject);
        }

        float angleDelta = Mathf.PI * 2 / numPlayers;
        for (int i = 0; i < numPlayers; i++)
        {
            float x = root.transform.position.x + radius * Mathf.Cos(angleDelta * i);
            float y = root.transform.position.y + radius * Mathf.Sin(angleDelta * i);

            GameObject inputFieldObj = new GameObject("PlayerInput" + i);
            inputFieldObj.transform.SetParent(canvas.transform);
            InputField inputField = inputFieldObj.AddComponent<InputField>();
            inputFieldObj.transform.position = new Vector3(x, -3, y);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputFieldObj.transform);

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = "";  
            textComponent.fontSize = 14; 
            textComponent.color = Color.black;  

            inputField.textComponent = textComponent;
        }
    }






    public void startFollowingWithIndex(int i)
    {
        GameObject startLeg = legGameObjects[i];
        GameObject childAnchor = startLeg.transform.GetChild(0).gameObject;

        Vector3[] path = computeMovePath(currentMap, i);
        follower = childAnchor;
        StartCoroutine(moveFollower(path, -1, currentMap.Count));
    }

    public void startRandom()
    {
        int random = UnityEngine.Random.Range(0, currentMap.Count);
        startFollowingWithIndex(random);
    }
}

