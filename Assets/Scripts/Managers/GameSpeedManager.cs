using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    [SerializeField] private float speedShiftRate = 1f;
    [SerializeField] private AnimationCurve speedShiftCurve;
    
    private static float velocity, targetSpeed, currentSpeed, targetDuration, currentTime;

    private void OnEnable()
    {
        GameManager.GameLogic.onGameInitialised += ResetGameSpeed;
    }

    private void OnDisable()
    {
        GameManager.GameLogic.onGameInitialised -= ResetGameSpeed;
    }

    public static void SetGameSpeed(float speed = .1f, float duration = 2.65f)
    {
        Time.timeScale = speed;
        targetSpeed = speed;
        targetDuration = duration;
        currentTime = duration;
    }

    private static void ResetGameSpeed()
    {
        velocity = 0f;
        targetSpeed = 1f;
        currentTime = 1f;
        currentSpeed = 1f;

        currentTime = 0f;
        targetDuration = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            SetGameSpeed();

        if (GameManager.GameLogic.IsGamePaused || currentTime < 0f)
            return;

        var dt = Mathf.InverseLerp(0f, targetDuration, currentTime);
        var t = speedShiftCurve.Evaluate(dt);
        var speed = Mathf.Lerp(1f, targetSpeed, t);

        currentSpeed = speed;

        Time.timeScale = currentSpeed;
        currentTime -= Time.unscaledDeltaTime;
        Debug.Log($"dt = {dt}, t = {t} speed = {speed}\nCurrent speed = {currentSpeed}, CurrentTime = {currentTime}");
    }
}
