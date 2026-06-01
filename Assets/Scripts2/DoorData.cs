using UnityEngine;

[System.Serializable]
public class DoorData
{
    public DoorResultType doorResultType;
    public string displayText;
    public bool isSuccessDoor;
    public bool canOpen;

    public DoorData(DoorResultType newDoorResultType, string newDisplayText, bool newIsSuccessDoor, bool newCanOpen)
    {
        doorResultType = newDoorResultType;
        displayText = newDisplayText;
        isSuccessDoor = newIsSuccessDoor;
        canOpen = newCanOpen;
    }
}

public enum DoorResultType
{
    Answer,
    Wrong,
    Luck,
    Closed
}