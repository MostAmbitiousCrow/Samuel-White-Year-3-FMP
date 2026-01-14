using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static River_Manager;

namespace GameCharacters
{
    /// <summary>
    /// The class for characters that can move on the rivers lanes
    /// </summary>
    public class RiverLaneCharacter : Character
    {
        [Header("Lane Information")]
        [Tooltip("The current lane on the river this character is on")]
        [SerializeField, ReadOnly] private RiverLane currentLane;

        protected void GoToLane()
        {

        }
    }
}

