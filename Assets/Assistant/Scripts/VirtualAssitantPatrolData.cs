using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VirtualAssitantPatrolData", menuName = "VirtualAssitantPatrolData", order = 1)]
public class VirtualAssitantPatrolData : ScriptableObject
{
   
    
    public PatrolData[] patrolData;

}


[System.Serializable]

    public class PatrolData
    {
        public string patrolPointName;
        public DialogueData dialogueData;
    }

