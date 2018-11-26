using UnityEngine;

public class TestScriptProtector : MonoBehaviour
{
  private void Awake ()
  {
    DontDestroyOnLoad (gameObject);
  }
}