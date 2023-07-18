using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public Text playerText;
    public GameObject contentTextField;

    public string input;
    

    List<Leg> legs; //vertical, basically map of the game
    public GhostLegRenderer renderer;
    public float minInterval = 0.1f;
    public float maxInterval = 0.85f;
    public float STOP = 0.2f;
    private int numsPlayer = 2;
    public int startIndex;

    // Start is called before the first frame update
    void Start()
    {
        createGhostLeg();
        renderer.createGhostLeg(legs);
        //startIndex = Random.Range(0, numsPlayer);
        //createGhostLeg();
        //renderer.createGhostLeg(legs);
        //Vector3[] path = renderer.computeMovePath(legs, startIndex);
        //StartCoroutine(renderer.moveFollower(path, startIndex, numsPlayer));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void increamnetPlayers()
    {
        if(numsPlayer < 6)
        {
            numsPlayer++;
            playerText.text = numsPlayer.ToString();
            createGhostLeg();
            renderer.createGhostLeg(legs);
        }
    }

    public void decreamnetPlayers()
    {
        if (numsPlayer > 2)
        {
            numsPlayer--;
            playerText.text = numsPlayer.ToString();
            createGhostLeg();
            renderer.createGhostLeg(legs);
        }
    }

    public void createGhostLeg()
    {
        legs = new List<Leg>();

        for (int i = 0; i < numsPlayer; i++)
        {
            legs.Add(new Leg());
        }
        // curr progress of each index
        float[] arr = new float[numsPlayer];

        int tries = 0;
        while (!checkDone(arr) && tries <= 50)
        {
            tries++;
            Bridge bridge = makeBridge(arr);
            legs[bridge.index1].bridges.Add(bridge);
            legs[bridge.index2].bridges.Add(bridge);
        }

    }

    private bool checkDone(float[] arr)
    {
        int numFail = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] < 1.0f)
            {
                numFail++;
                if (numFail >= 2)
                {
                    return false;
                }
            }

        }
        return true;
    }


    private Bridge makeBridge(float[] arr)
    {
        int randomIndex1 = Random.Range(0, arr.Length);
        while (arr[randomIndex1] >= 1.0f)
        {
            randomIndex1 = Random.Range(0, arr.Length);
        }

        int randomIndex2 = Random.Range(0, arr.Length);
        while (randomIndex1 == randomIndex2 || arr[randomIndex2] >= 1.0)
        {
            randomIndex2 = Random.Range(0, arr.Length);
        }

        float currPosition1 = arr[randomIndex1];
        float currPosition2 = arr[randomIndex2];

        float where1 = Random.Range(minInterval, maxInterval);
        float where2 = Random.Range(minInterval, maxInterval);

        float destination1 = where1 + currPosition1;
        float destination2 = where2 + currPosition2;

        if (destination1 > 1.0f - minInterval)
        {
            destination1 = 1.0f - minInterval;
        }

        if (destination2 > 1.0f - minInterval)
        {
            destination2 = 1.0f - minInterval;
        }

        if (destination1 + STOP > 1.0f - minInterval)
        {
            //stop connecting bridge?
            arr[randomIndex1] = 1.0f;
        }
        else
        {
            arr[randomIndex1] = destination1;
        }



        if (destination2 + STOP > 1.0f - minInterval)
        {
            arr[randomIndex2] = 1.0f;
        }
        else
        {
            arr[randomIndex2] = destination2;
        }

        Bridge bridge = new Bridge(randomIndex1, randomIndex2, destination1, destination2);


        return bridge;
    }

    public void RedStringInput(string s)
    {
        input = s;
        Debug.Log(input);
    }
}

public class Bridge //horizontal
{
    public int index1, index2;
    public float position1, position2; //connected one

    public Bridge(int index1, int index2, float position1, float position2)
    {
        this.index1 = index1;
        this.index2 = index2;
        this.position1 = position1;
        this.position2 = position2;
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(Bridge)) return false;

        Bridge other = (Bridge)obj;

        return (other.index1 == this.index1)
            && (other.index2 == this.index2)
            && (other.position1 == this.position1)
            && (other.position2 == this.position2);
    }
}

public class Leg
{
    public AnimalEnum animal;
    public List<Bridge> bridges;
    public TextField destinationText;

    public Leg()
    {
       // animal = AnimalEnum.byung;
        bridges = new List<Bridge>();
        destinationText = null;
    }

    public Leg(AnimalEnum animal, TextField textField)
    {
        this.animal = animal;
        bridges = new List<Bridge>();
        destinationText = textField;
    }

}

public enum AnimalEnum
{
    bear,
    byung,
    dog,
    tiger,
    panda,
    pig
}