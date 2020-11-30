using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] private Color[] allColors;

    public void SetColor(int colorIndex)
    {
        PlayerController.localPlayer.SetColor(allColors[colorIndex]);
    }

    //for testing
    public void NextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
