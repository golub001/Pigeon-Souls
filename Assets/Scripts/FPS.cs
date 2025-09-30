using UnityEngine;

public class AdvancedFPSDisplay : MonoBehaviour
{
    public int currentFPS;
    public int minFPS = int.MaxValue;
    public int maxFPS;
    private float deltaTime = 0.0f;
    public float updateInterval = 0.5f; // koliko često update-uje u sekundama
    private float timeLeft;

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        timeLeft -= Time.unscaledDeltaTime;

        if (timeLeft <= 0.0f)
        {
            currentFPS = Mathf.RoundToInt(1f / deltaTime);
            minFPS = Mathf.Min(minFPS, currentFPS);
            maxFPS = Mathf.Max(maxFPS, currentFPS);
            timeLeft = updateInterval;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.green;

        Rect rect = new Rect(10, 10, w, h * 2 / 10);
        float ms = deltaTime * 1000.0f;
        string text = string.Format(
            "FPS: {0}\nMin FPS: {1}\nMax FPS: {2}\nFrame Time: {3:0.0} ms",
            currentFPS, minFPS, maxFPS, ms
        );
        GUI.Label(rect, text, style);
    }
}
