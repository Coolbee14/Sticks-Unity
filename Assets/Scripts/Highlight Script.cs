using UnityEngine;



public class HighlightScript : MonoBehaviour
{
    protected TurnSystem TurnSystem;
    /// <summary>
    /// The SpriteRenderer component of current object. Used to change color
    /// </summary>
    protected SpriteRenderer obj;

    /// <summary>
    /// Used for the default color
    /// </summary>
    protected Color color = Color.white;
    /// <summary>
    /// Has it been clicked on? Used to determine if it should be highlighted light yellow after it's been clicked
    /// </summary>
    protected bool activated = false;
    /// <summary>
    /// Can the player select you? Used to determine if it should be highlighted faded yellow to be clicked on
    /// </summary>
    private bool selected = false;

    /// <summary>
    /// Assigns SpriteRenderer to obj and sets it to default color
    /// </summary>
    protected virtual void Awake()
    {
        obj = GetComponent<SpriteRenderer>();
        DefColor();
    }

    protected void Start()
    {
        TurnSystem = GameObject.Find("TurnSystem").GetComponent<TurnSystem>();
    }

    /// <summary>
    /// If you are able to click on it (selected) and it has not already been clicked on (!activated), it will be highlighted faded yellow to be clicked on
    /// </summary>
    protected virtual void OnMouseOver()
    {
        if (selected && !activated) obj.color = new Color(1f, 1f, 53f/255f, 1f); //yellow
    }

    /// <summary>
    /// If it has not already been clicked on (!activated), it will be changed back to default color when you move your mouse away
    /// </summary>
    protected virtual void OnMouseExit()
    {
        if(!activated) DefColor();
    }

    /// <summary>
    /// When clicked, activates or deactivates the object and tells turn system accordingly
    /// </summary>
    protected virtual void OnMouseDown()
    {
        if (activated) //deactivates it if you click on it again
        {
            activated = false;
            TurnSystem.ActivatedRemove();
        }
        else if (selected) //makes sure you can only click on it if it's selected 
        {
            activated = true;
            TurnSystem.ActivatedAdd(gameObject.name);
        }
        
    }

    /// <summary>
    /// Makes it selected, meaning you can click on it
    /// </summary>
    public void SelectOn()
    {
        selected = true;
    }

    /// <summary>
    /// Deselects it, meaning you can no longer click on it
    /// </summary>
    public void SelectOff()
    {
        selected = false;
    }

    /// <summary>
    /// Toggles selection
    /// </summary>
    public void SelectToggle()
    {
        selected = !selected;
    }

    /// <summary>
    /// Deactivates it, meaning it will no longer be highlighted light yellow but does not tell turn system
    /// </summary>
    public virtual void ActivateOff()
    {
        activated = false;
    }



    /// <summary>
    /// Changes current object to default color if it wasn't activated (clicked on)
    /// </summary>
    protected virtual void DefColor() //default color
    {
        if(!activated) ChangeColor(color);
    }
    /// <summary>
    /// Changes newobj to default color if it wasn't activated (clicked on)
    /// </summary>
    public void DefColor(GameObject newobj) //default color
    {
        ChangeColor(newobj, color);
    }

    /// <summary>
    /// Changes color to newcolor
    /// </summary>
    protected void ChangeColor(Color newcolor)
    {
        obj.color = newcolor;
    }

    /// <summary>
    /// Changes color of newobj to newcolor
    /// </summary>
    public void ChangeColor(GameObject newobj, Color newcolor)
    {
        newobj.GetComponent<SpriteRenderer>().color = newcolor;
    }

    /// <summary>
    /// Changes multiple objects to newcolor
    /// </summary>
    /// <param name="objs"> Array of multiple GameObjects </param>
    public void ChangeColor(GameObject[] objs, Color newcolor)
    {
        for (int i = 0; i < objs.Length; i++)
            ChangeColor(objs[i], newcolor);
        
    }

    /// <summary>
    /// Changes default color to newcolor and runs DefColor() to change current color to new default color
    /// </summary>
    public void ChangeDefColor(Color newcolor)
    {
        color = newcolor;
        DefColor();
    }

    /// <summary>
    /// When it turns on, goes back to DefColor unless activated
    /// </summary>
    /// <param name="b"></param>
    public virtual void SetActive(bool b)
    {
        gameObject.SetActive(b);
        OnMouseExit();
    }


}
