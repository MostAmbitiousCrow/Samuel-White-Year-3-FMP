namespace GameCharacters
{
    public abstract class BoatEnemyStateController : BoatCharacter
    {
        protected IState CurrentState { get; private set; }
        protected IState StoredState { get; private set; }
    
        public void ChangeState(IState newState)
        {
            CurrentState?.OnExit();
            CurrentState = newState;
            print($"New State: {CurrentState}");
            CurrentState.OnEnter();
        }
    
        public void StoreState(IState newState)
        {
            StoredState = newState;
        }
    }
}

