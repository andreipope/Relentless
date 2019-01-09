using UnityEngine;

public class GoogleAnalyticsController : MonoBehaviour
{
    private static GoogleAnalyticsController Instance { get; set; }

    private void Start()
    {
        if (Instance != null)
        {
            GoogleAnalyticsV4.instance = Instance.GetComponent<GoogleAnalyticsV4>();
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
