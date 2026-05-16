using UnityEngine;

public class AlterButtons : HighlightScript
{
    
    /// <summary>
    /// Highlights faded yellow to be clicked on
    /// </summary>
    protected override void OnMouseOver()
    {
        obj.color = new Color(1f, 1f, 53f / 255f, 1f); //yellow
    }


    /// <summary>
    /// Tells Turn System which button was clicked on (Add One, Subt One, or Split) <br/><br/>
    /// Split turns on baby Split Buttons if it was off and turns them off if it was on<br/><br/>
    /// Toggles activation afterwards
    /// </summary>
    protected override void OnMouseDown()
    {

        switch(gameObject.name)
        {
            case "Add One": 
                TurnSystem.Alter("+");
                break;
            case "Subt One": 
                TurnSystem.Alter("-");
                break;
            case "Split":
                if (!activated) TurnSystem.Alter("s"); 
                else TurnSystem.SplitButtonsOff();
                break;
        }
        activated = !activated;

    }


    /// <summary>
    /// Changes current object to default color
    /// </summary>
    protected override void DefColor() //default color
    {
        ChangeColor(color);
    }

    /// <summary>
    /// Deactivates it and changes it to default color
    /// </summary>
    public override void ActivateOff()
    {
        base.ActivateOff();
        DefColor();
    }


}
