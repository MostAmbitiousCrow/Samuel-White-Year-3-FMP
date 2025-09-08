public interface IBoatSpaceMovement
{
    public void MoveToSpace(int direction, float speed);
    public void GoToSpace(int lane, int space);
    public void VaultToSpace(int lane, int space, float speed);
    
    public int GetCurrentSpace();
    public int GetCurrentLane();
}
