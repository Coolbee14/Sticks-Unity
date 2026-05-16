using TMPro;

public class SplitButtons : AlterButtons
{

    /// <summary>
    /// Takes number from text (#-#) and sends them to turn system to split it
    /// </summary>
    protected override void OnMouseDown()
    { 
        string s = gameObject.GetComponentInChildren<TextMeshPro>().text;
        int h1 = (int)char.GetNumericValue(s[0]);
        int h2 = (int)char.GetNumericValue(s[2]);
        TurnSystem.Split(h1,h2);
    }







}
