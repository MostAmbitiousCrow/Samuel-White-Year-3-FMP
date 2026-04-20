using EditorAttributes;
using UnityEngine;
using static River_Manager;

namespace GameCharacters
{
    /// <summary>
    /// The class for characters that can move on the rivers lanes
    /// </summary>
    public class RiverLaneCharacter : Character, IRiverLaneMovement
    {
        [Header("Lane Information")]
        public RiverLane CurrentLane { get; set; }

        #region Movement

        protected override void TimeUpdate()
        {
            if (isMoving) Movement();
        }

        private float _moveElapsed = 0f;
        private Vector3 _currentMoveTarget;
        private Vector3 _startMovePosition;
        
        private void Movement()
        {
            _moveElapsed += Time.deltaTime / Mathf.Max(groundedMovementTime, 0.0001f);

            float t = Mathf.Clamp01(_moveElapsed);
            float moveT = groundedMovementCurve?.Evaluate(t) ?? t;

            // Move the character
            Vector3 newPosition = Vector3.Lerp(_startMovePosition, _currentMoveTarget, moveT);
            transform.localPosition = newPosition;

            // Set the character to its move target once travel time has ended
            if (!(t >= 1f)) return;
            transform.localPosition = _currentMoveTarget;
            transform.localRotation = Quaternion.identity;

            isMoving = false;
        }

        #endregion
        
        #region Movement Events
        public void MoveToLaneFromDirection(int direction)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToLane(int lane)
        {
            throw new System.NotImplementedException();
        }

        public void GoToLane(int lane)
        {
            // Get the River Lane Reference from the River Manager
            var rl = Instance.GetLane(lane);

            // Immediately move the character to the lane
            var pos = rl.transform.localPosition;
            CurrentLane = rl;
            transform.localPosition = new Vector3(pos.x, pos.y, transform.localPosition.z);
        }

        public int GetCurrentLane()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Health Events

        public override void OnTookDamage(DamageType damageType)
        {
            base.OnTookDamage(damageType);
        }

        public override void OnDied(DamageType damageType)
        {
            base.OnDied(damageType);
        }

        #endregion
    }
}

