using UnityEngine;

public class LeapingCrocodile_EmergeState : EnemyEmergeState
{
    public LeapingCrocodile_StateController CrocSc => Sc as LeapingCrocodile_StateController;
    private float _currentEmergeTime = 0f;

    public override void OnEnter()
    {
        base.OnEnter();
        _currentEmergeTime = 0f;

        CrocSc.BoatCharacterController.EnterBoat(false);

        // TEMP: Forcing the Crocoidle to a space until leap and target values are available
        CrocSc.BoatCharacterController.GoToSideSpace
            (CrocSc.BoatData.TargetSideSpace, CrocSc.BoatData.TargetLeftSide); // TODO: Add variables to enemies to define what side they should emerge from
    }

    public override void OnExit()
    {
        base.OnExit();
        // Revoke access to outer sides after they've landed on the boat
        CrocSc.BoatCharacterController.CanAccessOuterSides = false;

        // Set the direction of the enemy upon landing on the boat
        CrocSc.SetDirection(CrocSc.BoatData.BoardingMoveDirection);

        // Set the enemy to the targetted boatspace upon landed
        CrocSc.BoatCharacterController.GoToBoatSpace
            (CrocSc.BoatData.TargetSideSpace, CrocSc.BoatData.TargetSpace);
    }

    public override void OnHurt()
    {

        base.OnHurt();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        _currentEmergeTime += Time.deltaTime;
        //Debug.Log(_currentEmergeTime);

        if (_currentEmergeTime > CrocSc.EnemyData.TimeToEmerge)
        {
            /* After the emerge wait time is complete, 
             * enter the boats parent and begin leaping towards 
             * the targetted space on the boat
             */

            // TODO: Make the Crocodile leap towards the a targeted space on the boat

            //TODO: Once landed, swap to Moving State
            CrocSc.BoatCharacterController.GoToBoatSpace
                (CrocSc.BoatData.TargetSideSpace, CrocSc.BoatData.TargetSpace);

            CrocSc.ChangeState(CrocSc.MovingState);
        }

    }
    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }
}
