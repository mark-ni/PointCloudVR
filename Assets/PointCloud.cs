using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using Valve.VR;
using System;

/* TODO FOR NEXT WEEK
 * smooth movement
 * deep dive into mesh visualization
 */

public class PointCloud : MonoBehaviour
{
    private readonly bool VR = true;
    private readonly int NUM_MONTHS = 2;
    private readonly int NUM_AREAS = 2;
    //number of point clouds: NUM_MONTHS * NUM_AREAS

    public GameObject cloudJan0;
    private ArrayList vectorJan0;
    public GameObject cloudJan1;
    private ArrayList vectorJan1;
    public GameObject cloudJuly0;
    private ArrayList vectorJuly0;
    public GameObject cloudJuly1;
    private ArrayList vectorJuly1;

    private GameObject currCloud;
    private ArrayList currTraj;

    private GameObject[,] pcArray;
    private ArrayList[,] vectorArray;
    private int currTime;
    private int currMonth;
    private int currArea;
    private float currHeight;

    //Path Visualization
    public LineRenderer line;

    public Transform playerBody;

    // VR VARIABLES
    public SteamVR_Action_Boolean iTimeForward;
    public SteamVR_Action_Boolean iTimeBackward;
    public SteamVR_Action_Boolean iMonthForward;
    public SteamVR_Action_Boolean iMonthBackward;
    public SteamVR_Action_Boolean iRise;
    public SteamVR_Action_Boolean iFall;
    public SteamVR_Input_Sources handType;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log((-6 % 12));
        vectorArray = new ArrayList[12, 11];
        pcArray = new GameObject[12, 11];

        /* Hard-coded point clouds, using this for now since there are only 4 clouds but
         * if a way is found to abstract the point cloud loading process then this can
         * easily be changed */
        cloudJan0.SetActive(true);
        pcArray[0, 0] = cloudJan0;
        cloudJan1.SetActive(false);
        pcArray[0, 1] = cloudJan1;
        cloudJuly0.SetActive(false);
        pcArray[6, 0] = cloudJuly0;
        cloudJuly1.SetActive(false);
        pcArray[6, 1] = cloudJuly1;

        vectorJan0 = ReadCSVFile("semantic_icp_0_1.csv");
        vectorArray[0, 0] = vectorJan0;
        vectorJan1 = ReadCSVFile("semantic_icp_0_7.csv");
        vectorArray[0, 1] = vectorJan1;
        vectorJuly0 = ReadCSVFile("semantic_icp_6_1.csv");
        vectorArray[6, 0] = vectorJuly0;
        vectorJuly1 = ReadCSVFile("semantic_icp_6_7.csv");
        vectorArray[6, 1] = vectorJuly1;

        currArea = 0;
        currTime = 0;
        currHeight = 0.0f;
        currMonth = 0;
        currCloud = cloudJan0;
        currTraj = vectorJan0;
        DrawTrajectory();

        playerBody.transform.position = (Vector3) vectorJan0[0];
        Debug.Log(playerBody.transform.position.ToString());

        // VR operations
        iTimeForward.AddOnStateDownListener(InputTimeForward, handType);
        iTimeBackward.AddOnStateDownListener(InputTimeBackward, handType);
        iMonthForward.AddOnChangeListener(InputMonthForward, handType);
        iMonthBackward.AddOnChangeListener(InputMonthBackward, handType);
        iRise.AddOnStateDownListener(InputRise, handType);
        iFall.AddOnStateDownListener(InputFall, handType);
    }

    private void InputFall(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (currHeight > 0)
        {
            SetHeight(currHeight - 1);
        }
    }

    private void InputRise(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        SetHeight(currHeight + 1);
    }

    private void InputMonthBackward(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (newState)
        {
            IncrementMonth(-1);
        }
    }

    private void InputMonthForward(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (newState)
        {
            IncrementMonth(1);
        }
    }

    private void InputTimeBackward(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        IncrementTime(-30);
    }

    private void InputTimeForward(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        IncrementTime(30);
    }

    // Update is called once per frame
    void Update()
    {
        /* if (!VR)
        {
            updateNoVR();
            return;
        } */
    }

    ArrayList ReadCSVFile(string id)
    {
        ArrayList trajectory = new ArrayList();
        StreamReader strReader = new StreamReader("Assets/" + id);
        bool endOfFile = false;
        int line = 0;
        while (!endOfFile)
        {
            string data_String = strReader.ReadLine();
            if (data_String == null)
            {
                break;
            }
            var data = data_String.Split(',');
            trajectory.Add(new Vector3(-float.Parse(data[2]),
                                            -float.Parse(data[3]),
                                            float.Parse(data[1])));
            line++;
        }
        strReader.Close();

        return trajectory;
    }

    void DrawTrajectory()
    {
        Vector3[] empty = new Vector3[0];
        line.SetPositions(empty);

        line.positionCount = currTraj.Count;
        for (int i = 0; i < currTraj.Count; i++)
        {
            line.SetPosition(i, (Vector3)currTraj[i]);
        }
        Debug.Log("DONE DRAWING LINE!");
    }

    private void IncrementTime(int amount)
    {
        currTime += amount;
        if (currTime >= currTraj.Count)
        {
            //Load next zone, deload this zone.
            /* The %2 part is here because I have only implemented
             * the first 2 areas. When the other areas have been
             * implemented, change this % 2 to a % 11. */

            SetCloud(currMonth, mod(currArea + 1, 2), "START");
        }
        else if (currTime < 0)
        {
            SetCloud(currMonth, mod(currArea - 1, 2), "END");
        }
        else
        {
            Vector3 newPos = (Vector3)currTraj[currTime];
            newPos.y = currHeight;
            playerBody.transform.position = newPos;
        }
    }

    private void IncrementMonth(int amount)
    {
        /* The *6 multiplier is here only because I have only
         * implemented January and July as months. When other months
         * get implemented, this multiplier can be removed. */
        SetCloud(mod(currMonth + amount * 6, 12), currArea, "STAY");
    }

    private void SetCloud(int month, int area, String position)
    {
        currCloud.SetActive(false);
        currMonth = month;
        currArea = area;
        currCloud = pcArray[month, area];
        currTraj = vectorArray[month, area];
        if (position.Equals("STAY"))
        {
            if (currTime >= currTraj.Count)
            {
                currTime = currTraj.Count - 1;
            }
            Vector3 pos = playerBody.transform.position;
            int minTime = 0;
            double minDist = 99999999;
            double dist;
            double xdiff, ydiff, zdiff;

            for (int i = 0; i < currTraj.Count; i++)
            {
                xdiff = pos.x - ((Vector3)currTraj[i]).x;
                ydiff = pos.y - ((Vector3)currTraj[i]).y;
                zdiff = pos.z - ((Vector3)currTraj[i]).z;
                dist = Math.Sqrt(xdiff * xdiff + ydiff * ydiff + zdiff * zdiff);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTime = i;
                }
            }
            playerBody.transform.position = (Vector3)currTraj[minTime];
            SetHeight(currHeight);
        }
        else if (position.Equals("START"))
        {
            playerBody.transform.position = (Vector3)currTraj[0];
            SetHeight(currHeight);
            currTime = 0;
        }
        else if (position.Equals("END"))
        {
            playerBody.transform.position = (Vector3)currTraj[-1];
            SetHeight(currHeight);
            currTime = currTraj.Count;
        }

        currCloud.SetActive(true);
        DrawTrajectory();
    }

    private void SetHeight(float height)
    {
        currHeight = height;

        Vector3 newPos = playerBody.transform.position;
        newPos.y = height;
        playerBody.transform.position = newPos;
    }

    private int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    /* private void updateNoVR()
    {
        if (!Input.anyKey)
        {
            keyWasDown = false;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (!currCloud.Equals(cloud0))
                {
                    cloud0.SetActive(true);
                    currCloud.SetActive(false);
                    currCloud = cloud0;
                    currTraj = vector0;
                    playerBody.transform.position = (Vector3)vector0[0];
                    DrawTrajectory();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (!currCloud.Equals(cloud1))
                {
                    currCloud.SetActive(false);
                    currCloud = cloud1;
                    cloud1.SetActive(true);
                    currTraj = vector1;
                    playerBody.transform.position = (Vector3)vector1[0];
                    DrawTrajectory();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (!currCloud.Equals(cloud3))
                {
                    cloud3.SetActive(true);
                    currCloud.SetActive(false);
                    currCloud = cloud3;
                    currTraj = vector3;
                    playerBody.transform.position = (Vector3)vector3[0];
                    DrawTrajectory();
                }
            }

            if (!keyWasDown)
            {
                if (Input.GetKeyDown(KeyCode.D)) IncrementTime(9);
                else if (Input.GetKeyDown(KeyCode.F)) IncrementTime(50);
                else if (Input.GetKeyDown(KeyCode.Space)) IncrementTime(-1 * time);
                else if (Input.GetKeyDown(KeyCode.S)) IncrementTime(-7);
            }
            keyWasDown = true;
        }
    } */
}
