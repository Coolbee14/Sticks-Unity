using System.Collections;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    /// <summary>
    /// The entire canvas and the text that says "Player # wins!"
    /// </summary>
    private  GameObject screen;
    /// <summary>
    /// The text that says "Player # wins!" that is a child of the canvas.
    /// </summary>
    private  GameObject text;

    /// <summary>
    /// Assigns the Screen and Text objects and changes the canvas to be completely transparent and deactivates the text.
    /// </summary>
    void Awake()
    {
        gameObject.SetActive(true);
        screen = gameObject;
        text = screen.transform.GetChild(0).gameObject;
        ChangeColor(new Color(1, 1, 1, 0));
        HideText();
    }

    /// <summary>
    /// Changes color of the canvas (includes the alpha value)
    /// </summary>
    private void ChangeColor(Color color)
    {
        screen.GetComponent<CanvasRenderer>().SetColor(color);
    }

    /// <summary>
    /// Changes only the alpha value of the canvas (the transparency)
    /// </summary>
    /// <param name="alpha"></param>
    private void ChangeAlpha(float alpha)
    {
        screen.GetComponent<CanvasRenderer>().SetAlpha(alpha);
    }

    /// <summary>
    /// Changes alpha of selected object to alpha value (the transparency)
    /// </summary>
    /// <param name="obj"> GameObject to change the transparency</param>
    /// <param name="alpha"> The transparency float value (0-1) </param>
    private void ChangeAlpha(GameObject obj, float alpha)
    {
        obj.GetComponent<CanvasRenderer>().SetAlpha(alpha);
    }

    /// <summary>
    /// Fades the screen in and changes the text to say which player wins. Fades in gradually over two seconds.
    /// </summary>
    public void EndGame(int player)
    {
        StartCoroutine(FadeMain(text, 2f, player));
    }
    /// <summary>
    /// Deactivates the text
    /// </summary>
    private void HideText()
    {
        text.GetComponent<TextMeshProUGUI>().enabled = false;
    }

    /// <summary>
    /// Activates the text and changes it to say which player wins.
    /// </summary>
    /// <param name="player"> The player number that wins (1 or 2)</param>
    private void ActivateText(int player)
    {
        text.GetComponent<TextMeshProUGUI>().enabled = true;
        text.GetComponent<TextMeshProUGUI>().text = "Player "+player+" wins!";
    }

    private void ActivateText(string s)
    {
        text.GetComponent<TextMeshProUGUI>().enabled = true;
        text.GetComponent<TextMeshProUGUI>().text = s;
    }
    /// <summary>
    /// Fades in the Screen and then fades in the Text
    /// </summary>
    /// <param name="obj"> The object to fade in (the text)</param>
    /// <param name="sec"> Number of seconds to fade in</param>
    /// <param name="player"> The player value that won</param>
    /// <returns></returns>
    IEnumerator FadeMain(GameObject obj, float sec, int player)
    {
        yield return StartCoroutine(Fade(sec, player));
        yield return StartCoroutine(FadeObj(obj, sec, player));
    }

    /// <summary>
    /// Fades in the Screen over a certain number of seconds and then activates the text to say which player wins. 
    /// </summary>
    /// <param name="sec"> Number of seconds to fade in</param>
    /// <param name="player"> Player value that won</param>
    /// <returns></returns>
    IEnumerator Fade(float sec, int player)
    { 
        float elapsed = 0f;
        while (elapsed < sec)
        {
            elapsed += Time.deltaTime;
            ChangeAlpha(elapsed / sec);
            yield return null;
        }
        ChangeAlpha(1);
        ActivateText(player);
    }

    /// <summary>
    /// Fades in an object (the text) over a certain number of seconds. It is used after the screen has already faded in and the text is activated to say which player wins.
    /// </summary>
    /// <param name="obj"> The GameObject to be faded in, usually the Text</param>
    /// <param name="sec"> Number of seconds to fade in</param>
    /// <param name="player"> Player value that won</param>
    /// <returns></returns>
    IEnumerator FadeObj(GameObject obj, float sec, int player) 
    {
        float elapsed = 0f;
        while (elapsed < sec)
        {
            elapsed += Time.deltaTime;
            ChangeAlpha(obj, elapsed / sec);
            yield return null;
        }
        
        ChangeAlpha(obj, 1);
    }

    /// <summary>
    /// The screen turns red and the text changes to the error message. <br/><br/>
    /// Used for when there is an error in the turn system and the game cannot continue. <br/><br/>
    /// The screen will not fade in, it will just immediately turn red and show the error message.
    /// </summary>
    public void Error(string log)
    {
        ChangeColor(Color.red);
        ChangeAlpha(1);
        ActivateText(log);
    }



}
