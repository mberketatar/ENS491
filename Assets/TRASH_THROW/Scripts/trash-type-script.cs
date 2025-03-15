using UnityEngine;

// Enum to define the different types of trash
public enum TrashType
{
    Metal,
    Organic,
    Paper,
    Plastic
}

// Component to be attached to each trash item to identify its type
public class TrashItem : MonoBehaviour
{
    public TrashType trashType;
}
