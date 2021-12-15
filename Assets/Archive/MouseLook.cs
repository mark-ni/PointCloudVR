using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool> { }

public class MouseLook : MonoBehaviour
{
    // VR VARIABLES
    public SteamVR_Action_Boolean timeForward;
    public SteamVR_Input_Sources handType;
    // private bool timeForwardWasDown;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started!");
    }

    // Update is called once per frame
    void Update()
    {
        /* if (timeForward.GetLastState(handType))
        {
            Debug.Log("TRUE!!!");
        }
        else if (timeForward.GetActive(handType))
        {
            Debug.Log("TRUE 2!!!!");
        } */
    }

    public void TimeForwardTriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("trigger is up");
        //if (timeForwardWasDown) timeForwardWasDown = false;
    }

    public void TimeForwardTriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("trigger is down");
        /*if (!timeForwardWasDown)
        {
            IncrementTime(10);
            timeForwardWasDown = true;
        }*/
    }
}