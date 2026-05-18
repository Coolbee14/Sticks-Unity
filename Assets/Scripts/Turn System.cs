using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnSystem : MonoBehaviour
{
    /// <summary>
    /// Color used to show when it is clicked on - light yellow
    /// </summary>
    private Color activatedColor = new Color(1f, 1f, 150f / 255f, 1f);

    /// <summary>
    /// The current player that can be selected. 1 or 2.
    /// </summary>
    private int currPlayer = 1;
    /// <summary>
    /// The current player whose turn it is. 1 or 2.
    /// </summary>
    private int currTurn = 1;

    /// <summary>
    /// GameObject of the hands that are selected to be attacked. Index 0 is always player 1's hand, index 1 is always player 2's hand. Null if no hand is selected.
    /// </summary>
    private GameObject[] hands = new GameObject[2];

    /// <summary>
    /// Player 1's hand 1
    /// </summary>
    [Header("Game Objects")]
    [SerializeField] private GameObject P1H1;
    /// <summary>
    /// Player 1's hand 2
    /// </summary>
    [SerializeField] private GameObject P1H2;
    /// <summary>
    /// Player 2's hand 1
    /// </summary>
    [SerializeField] private GameObject P2H1;
    /// <summary>
    /// Player 2's hand 2
    /// </summary>
    [SerializeField] private GameObject P2H2;

    /// <summary>
    /// The empty GameObject containing Add One, Subt One, Split and Split Dropdown. <br/><br/> Used to activate and deactivate the buttons and move them to the correct position
    /// </summary>
    [SerializeField] private GameObject alterButtons;
    /// <summary>
    /// Button says "+1" that adds one to the selected hand.
    /// </summary>
    [SerializeField] private GameObject addOne;
    /// <summary>
    /// Button says "-1" that subtracts one to the selected hand.
    /// </summary>
    [SerializeField] private GameObject subtOne;
    /// <summary>
    /// Button says "Split" that opens the split dropdown.
    /// </summary>
    [SerializeField] private GameObject split;
    /// <summary>
    /// Position of the alter buttons relative to the hands. 
    /// </summary>
    private float alterButtonsYCoefficent;
    private float P1Y;
    private float P2Y;


    /// <summary>
    /// The split dropdown is an empty GameObject that contains the baby split buttons that pop up when you click on Split. <br/></br/>
    /// Used to assign splitB array
    /// </summary>
    [SerializeField] private GameObject splitDropdown;
    /// <summary>
    /// "Baby" Split Buttons that pop up when you click on Split. They are 3 because the most split combos avaliable is 3. <br/><br/> 
    /// They are assigned the correct numbers for splitting based on the hand values when you click on Split.
    /// </summary>
    private GameObject[] splitB = new GameObject[3];

    /// <summary>
    /// Both of Player 1's hands
    /// </summary>
    private GameObject[] P1 = new GameObject[2];
    /// <summary>
    /// Both of Player 2's hands
    /// </summary>
    private GameObject[] P2 = new GameObject[2];
    /// <summary>
    /// Both player's two hands in a 2D array. The first index is which player, the second index is which hand. <br/><br/>
    /// </summary>
    private GameObject[][] P = new GameObject[2][];

    /// <summary>
    /// Used to determine if a player can use an alter button. Only able to use once every other turn
    /// </summary>
    private bool[] canAlter = new bool[2];

    /// <summary>
    /// End Screen GameObject
    /// </summary>
    [SerializeField] private GameObject endScreen;

    [SerializeField] private GameObject playerText;

    /// <summary>
    /// Reference to the ML-Agents AI handler script.
    /// </summary>
    [Header("AI Optimization")]
    public SticksAgent agentP1;
    public SticksAgent agentP2; //this is only for self play




    //Finding and Storing GameObjects

    /// <summary>
    /// Stores all of the GameObjects that will be used in the game in variables and arrays for easy access. <br/> <br/>
    /// Starts off with player 1's turn by calling P1Turn() at the end.
    /// </summary>
    void Start()
    {
        P1[0] = P1H1;
        P1[1] = P1H2;
        P2[0] = P2H1;
        P2[1] = P2H2;
        P[0] = P1;
        P[1] = P2;

        canAlter[0] = true;
        canAlter[1] = true;

        for (int i = 0; i < splitDropdown.transform.childCount; i++)
        {
            splitB[i] = GameObject.Find("Split (" + (i + 1) + ")");
        }

        P1Y = P1H1.transform.position.y;
        P2Y = P2H1.transform.position.y;

        alterButtonsYCoefficent = Math.Abs(P1Y - alterButtons.transform.position.y);

        playerText.SetActive(true);

        PlayerTurn(1);



    }

    /// <summary>
    /// Finds a GameObject by name. <br/><br/>If it's one of the hands, it will return the variable instead of using GameObject.Find() to save time since those are used a lot. <br/><br/>If it's not one of the hands, it will use GameObject.Find() to find it. <br/><br/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject Find(string name)
    {
        switch (name)
        {
            case "P1H1": return P1H1;
            case "P1H2": return P1H2;
            case "P2H1": return P2H1;
            case "P2H2": return P2H2;
            case "Alter Buttons": return alterButtons;
            case "Add One": return addOne;
            case "Subt One": return subtOne;
            case "Split": return split;
            case "Split Dropdown": return splitDropdown;
            case "End Screen": return endScreen;
            case "Turn System": return gameObject;
        }
        return GameObject.Find(name);
    }


    //Regarding Active Player & Current Turn


    /// <summary>
    /// The screen turns red and the text changes to the error message. <br/><br/>
    /// Used for when there is an error in the turn system and the game cannot continue. <br/><br/>
    /// The screen will not fade in, it will just immediately turn red and show the error message.
    /// </summary>
    public void Error(string log)
    {
        endScreen.GetComponent<EndScreen>().Error(log);
        Debug.Log(log);
    }



    /// <summary>
    /// Checks for a win and if there isn't one, selects the player to be selected based on currPlayer.
    /// </summary>
    public void PlayerSelect()
    {
        bool isAIGame = agentP1 != null || agentP2 != null;
        bool isGameOver = isAIGame ? (CheckWinSilent() != 0) : CheckWin(); //if it's an AI game, use CheckWinSilent to avoid unnecessary UI updates, if it's not, use CheckWin to update the UI with the win

        if (!isGameOver)
        {
            Deselect();
            if (currPlayer != 1 && currPlayer != 2) Error("PlayerSelect() > currPlayer did not equal 1 or 2");

            else
            {
                //AI AGENT LOGIC
                if(currTurn == 1 && agentP1 != null)
                {
                    agentP1.RequestAction();
                    return; // Stop running human UI logic below
                } else if (currTurn == 2 && agentP2 != null)
                {
                    agentP2.RequestAction();
                    return; // Stop running human UI logic below
                }




                for (int i = 0; i < P[0].Length; i++)
                {
                    
                    if (CanSelect(currPlayer - 1, i))
                    {
                        P[currPlayer - 1][i].GetComponent<HighlightScript>().ChangeDefColor(activatedColor);
                        P[currPlayer - 1][i].GetComponent<HighlightScript>().SelectOn();
                    }
                    else
                    {
                        P[currPlayer - 1][i].GetComponent<HighlightScript>().ChangeDefColor(new Color(0.5f, 0.5f, 0.5f, 0.5f)); //grey
                    }

                }


                if (hands[0] != null && hands[1] != null) //if both hands are selected
                {
                    Attack();
                }

                else if (hands[0] != null || hands[1] != null) //if one hand is selected
                {
                    AlterButtonsActive(true);
                }

            }

        }
    }

    /// <summary>
    /// Checks if you can select based on the following rules: <br/><br/>
    /// 1. You cannot attack dead hands <br/><br/>
    /// 2. You cannot use dead hands to attack <br/><br/>
    /// 3. You can use split or add one to a dead hand, but not subt one <br/><br/>
    /// </summary>
    /// <param name="playerIndex">The index of the player to check if it can be selected</param>
    /// <param name="handIndex">The index of the hand to check if it can be selected</param>
    /// <returns>Bool if it can be selected</returns>
    private bool CanSelect(int playerIndex, int handIndex)
    {
        /*
         * 
        * You cannot select if:
        * You are selecting your own hands and you cannot alter and the hand is dead
        * You are selecting the opponent's hands after you have selected a dead hand
        * You are selecting the opponent's dead hand
        * 
        */

        bool isOwnHand = playerIndex + 1 == currTurn; //are you trying to select your own hands?
        bool canIAlter = canAlter[currTurn - 1]; //can you use an alter button this turn?
        int mySelectedHandValue = hands[currTurn - 1] != null ? GetHand(hands[currTurn - 1]) : -1; //if you have selected a hand, get its value, if not set to -1 so it doesn't interfere with rules

        if (isOwnHand) //if you are selecting your own hands
        {
            if (canIAlter) return true; //if you can alter, you can select regardless of if it's dead or not
            else if (GetHand(P[currTurn - 1][handIndex]) == 0) return false; //if you cannot alter and it's dead, cannot select
            return true; //if it's not dead, can select
        }
        else //if you are selecting opponent's hands
        {
            if (mySelectedHandValue==0) return false; //if you have selected a dead hand, cannot select
            if (GetHand(P[playerIndex][handIndex]) == 0) return false; //if opponent's hand is dead, cannot select
        }

        return true;

    }


    /// <summary>
    /// Turns off Alter Buttons and deselects all hands by changing them back to default color and turning off their highlight. <br/><br/>
    /// </summary>
    public void Deselect()
    {

        AlterButtonsActive(false);

        for (int j = 0; j < P.Length; j++)
        {
            for (int i = 0; i < P[0].Length; i++)
            {
                if (CanSelect(j, i))
                {
                    P[j][i].GetComponent<HighlightScript>().ChangeDefColor(Color.white);
                    P[j][i].GetComponent<HighlightScript>().SelectOff();
                }
                else
                {
                    P[j][i].GetComponent<HighlightScript>().ChangeDefColor(new Color(0.5f, 0.5f, 0.5f, 0.5f)); //grey
                    P[j][i].GetComponent<HighlightScript>().SelectOff();
                }



            }
        }
    }

    /// <summary>
    /// Used at the end of each turn: <br/><br/>
    /// Hands array is reset to null and all players are deactivated and changed back to default color. <br/><br/>
    /// </summary>
    public void Reset()
    {
        Deselect();
        hands[0] = null;
        hands[1] = null;

        for (int j = 0; j < P.Length; j++)
        {

            for (int i = 0; i < P[0].Length; i++)
            {
                P[j][i].GetComponent<HighlightScript>().ActivateOff();
            }

            for (int i = 0; i < splitB.Length; i++)
            {
                splitB[0].GetComponent<SplitButtons>().ActivateOff();
            }

        }
    }

    /// <summary>
    /// Switches the player to be selected and calls PlayerSelect() to select them. <br/><br/>
    /// </summary>
    public void PlayerSwitch()
    {
        currPlayer = GetOppPlayer(currPlayer);
        PlayerSelect();
    }


    /// <summary>
    /// Allows the original player to use alter buttons, switches turn to the opposite player, resets everything, and selects the new player. <br/><br/>
    /// </summary>
    private void TurnSwitch()
    {
        canAlter[currTurn - 1] = true;
        currTurn = GetOppPlayer(currTurn);
        currPlayer = currTurn;
        Reset();
        PlayerSelect();
    }



    /// <summary>
    /// Adds the hand  "name" to the Hands array to be attacked and then switches the player to be selected. <br/><br/>
    /// </summary>
    /// <param name="name"> The name of the GameObject being added to Hands array</param>
    public void ActivatedAdd(string name)
    {
        hands[currPlayer - 1] = Find(name);
        PlayerSwitch();
    }

    /// <summary>
    /// Removes the hand from the Hands array using the current turn and then switches the player to be selected. <br/><br/>
    /// </summary>
    public void ActivatedRemove()
    {
        hands[currTurn - 1] = null;
        PlayerSwitch();
    }

    /// <summary>
    /// Used to manually set the turn. <br/><br/>
    /// Sets current player to player, resets everything, and selects the new player. <br/><br/>
    /// TurnSwitch() is the primary way to switch turns
    /// </summary>
    /// <param name="player"></param>
    public void PlayerTurn(int player)
    {
        if (player == 1 || player == 2)
        {
            currPlayer = player;
            Reset();
            PlayerSelect();
            currTurn = player;
        }

        else Error("PlayerTurn() > player did not equal 1 or 2");


    }


    /// <summary>
    /// Gets the opposite player, if player 1 then 2 and vice versa. Uses mod to switch between 1 and 2. <br/><br/>
    /// </summary>
    /// <param name="p"> Current player to be switched </param>
    /// <returns></returns>
    public int GetOppPlayer(int p)
    {                               //if player 1, adds 1 to make player 2
        return p % 2 + 1;           //if player 2, turn into 0 then add 1 to be player 1
    }



    //Regarding Player Hand Values



    /// <summary>
    /// Gets the hand value by taking the text from the GameObject and turning it into an int. <br/><br/>
    /// </summary>
    /// <param name="obj"> GameObject to get hand value from</param>
    /// <returns></returns>
    public int GetHand(GameObject obj)
    {
        return int.Parse(obj.GetComponentInChildren<TextMeshPro>().text);
    }

    /// <summary>
    /// Sets the hand value of obj to num, making sure to mod it by 5 for overflow. <br/><br/>
    /// </summary>
    /// <param name="obj"> GameObject to set the hand</param>
    /// <param name="num"> Number to set the hand to</param>
    private void SetHand(GameObject obj, int num)
    {
        obj.GetComponentInChildren<TextMeshPro>().text = (num % 5) + "";
    }

    /// <summary>
    /// Gets the text of the GameObject, currently not used
    /// </summary>
    /// <param name="obj"> GameObject to get text from</param>
    /// <returns></returns>
    private string GetText(GameObject obj)
    {
        return obj.GetComponentInChildren<TextMeshPro>().text;
    }

    /// <summary>
    /// Set text of the GameObject
    /// </summary>
    /// <param name="obj"> GameObject to set text</param>
    /// <param name="text"> Text to be inserted to GameObject</param>
    private void SetText(GameObject obj, string text)
    {
        obj.GetComponentInChildren<TextMeshPro>().text = text;
    }


    /// <summary>
    /// Uses the current turn to figure out the attacker and defender, adds hand values together and mods by 5 for overflow<br/><br/>
    /// Sets the defender's hand to the new value, and then switches turns. 
    /// </summary>
    public void Attack()
    {
        int attacker = GetHand(hands[currTurn - 1]);
        int defender = GetHand(hands[GetOppPlayer(currTurn) - 1]);
        defender = (defender + attacker) % 5;
        SetHand(hands[GetOppPlayer(currTurn) - 1], defender);
        TurnSwitch();

    }


    //Altering Hand Values (for Split, Add, Subtract)

    /// <summary>
    /// If player Can Alter and b = true, moves the Alter Buttons to the correct position and activates them.<br/><br/>
    /// If selected hand is zero, Subt One will not be active <br/><br/>
    /// If player cannot alter, it will not do anything. <br/><br/>
    /// If b = false, deactivates everything, regardless of if they can alter <br/><br/>
    /// </summary>
    public void AlterButtonsActive(bool b)
    {
        if (!b || canAlter[currTurn - 1])
        {
            ButtonsPos();

            for (int i = 0; i < alterButtons.transform.childCount; i++)
            {
                alterButtons.transform.GetChild(i).GetComponent<AlterButtons>().ActivateOff();
            }
            if (!b) SplitButtonsOff();
            alterButtons.SetActive(b);
            if (hands[currTurn - 1] != null)
            {
                subtOne.GetComponent<AlterButtons>().SetActive(GetHand(hands[currTurn - 1]) == 0 ? false : true);
                addOne.GetComponent<AlterButtons>().SetActive(GetHand(hands[currTurn - 1]) == 4 ? false : true);
            }
        }
    }


    /// <summary>
    /// If it's player 1's turn, the buttons will be on the bottom of the screen, if it's player 2's turn, they will be on the top. <br/><br/>
    /// </summary>
    private void ButtonsPos()
    {
        Vector3 pos = alterButtons.transform.position;
        if (currTurn == 1)
        {
            pos = new Vector3(pos.x, P1Y + alterButtonsYCoefficent, pos.z);
        }
        else if (currTurn == 2)
        {
            pos = new Vector3(pos.x, P2Y + alterButtonsYCoefficent, pos.z);
        }
        else Error("ButtonsPos() > currTurn did not equal 1 or 2");
        alterButtons.transform.position = pos;
        splitDropdown.transform.position = pos;

    }

    /// <summary>
    /// Uses name of Alter Button to determine which action to take (Add One, Subt One, Split). <br/><br/>
    /// </summary>
    /// <param name="s"> Name of Alter Button</param>
    public void Alter(string s)
    {
        switch (s)
        {
            case "+":
                AddOne();
                break;
            case "-":
                SubtOne();
                break;
            case "s":
                SplitButtonsSetup();
                break;
        }
    }
    /// <summary>
    /// Adds one to current hand selected, mods by 5 for overflow, sets Can Alter to false, and switches turns <br/><br/>
    /// </summary>
    private void AddOne()
    {
        SetHand(hands[currTurn - 1], GetHand(hands[currTurn - 1]) + 1);
        int playerWhoAltered = currTurn - 1;
        TurnSwitch();
        canAlter[playerWhoAltered] = false;

    }

    /// <summary>
    /// Subtracts one to current hand selected, sets Can Alter to false, and switches turns<br/><br/>
    /// Does not appear if selected hand is zero
    /// </summary>
    private void SubtOne()
    {
        SetHand(hands[currTurn - 1], GetHand(hands[currTurn - 1]) - 1);
        int playerWhoAltered = currTurn - 1;
        TurnSwitch();
        canAlter[playerWhoAltered] = false;
    }

    /// <summary>
    /// Takes two values and sets it to the current player's hands based on currTurn, sets Can Alter to false, and switches turns<br/><br/>
    /// </summary>
    public void Split(int hand1, int hand2)
    {
        SetHand(P[currTurn - 1][0], hand1);
        SetHand(P[currTurn - 1][1], hand2);
        int playerWhoAltered = currTurn - 1;
        TurnSwitch();
        canAlter[playerWhoAltered] = false;
    }


    /// <summary>
    /// Takes both hand values, adds them together, and finds all the possible combinations of splitting those values into two hands <br/><br/>
    /// Runs through combinations until halfway through total to find all unique combinations (ex: once 2-3 is found, 3-2 is not unique) <br/><br/>
    /// Allows killing one hand, but not overflowing after kill (ex: 3-4 can turn into 2-0 but not 1-1)<br/><br/>
    /// Does not allow for killing of both hands (ex: 2-3 can turn into 1-4 but not 0-0)<br/><br/>
    /// </summary>
    private void SplitButtonsSetup()
    {

        int split1 = GetHand(P[currTurn - 1][0]);
        int split2 = GetHand(P[currTurn - 1][1]);
        int total = split1 + split2;
        List<int> combos = new List<int>();

        for (int i = 0; i < total / 2 + 1; i++)
        {
            if (total - i >= 5) continue;
            combos.Add(i);
        }

        int[,] c = new int[combos.Count, 2];

        for (int i = 0; i < c.GetLength(0); i++)
        {
            c[i, 0] = combos[i];
            c[i, 1] = (total - combos[i]) % 5;
        }

        AssignSplitButtons(c);


    }

    /// <summary>
    /// Assigns baby Split Buttons the correct numbers based on the combinations found in SplitButtonsSetup() and activates them. <br/><br/>
    /// Three is the maximum combinations possible, so there are three baby Split Buttons. <br/><br/>
    /// If only one, middle one is on, if two, left and right are on, if three, all are on. Works from first number least to greatest<br/><br/>
    /// </summary>
    /// <param name="combos"> The 2 by (1 thru 3) array of combinations found in SplitButtonsSetup</param>
    private void AssignSplitButtons(int[,] combos)
    {

        switch (combos.GetLength(0))
        {
            case 1:
                SetText(splitB[1], combos[0, 0] + "-" + combos[0, 1]);
                splitB[1].GetComponent<SplitButtons>().SetActive(true);
                break;
            case 2:
                SetText(splitB[0], combos[0, 0] + "-" + combos[0, 1]);
                SetText(splitB[2], combos[1, 0] + "-" + combos[1, 1]);
                splitB[0].GetComponent<SplitButtons>().SetActive(true);
                splitB[2].GetComponent<SplitButtons>().SetActive(true);
                break;
            case 3:
                SetText(splitB[0], combos[0, 0] + "-" + combos[0, 1]);
                SetText(splitB[1], combos[1, 0] + "-" + combos[1, 1]);
                SetText(splitB[2], combos[2, 0] + "-" + combos[2, 1]);
                splitB[0].GetComponent<SplitButtons>().SetActive(true);
                splitB[1].GetComponent<SplitButtons>().SetActive(true);
                splitB[2].GetComponent<SplitButtons>().SetActive(true);
                break;
        }


    }

    /// <summary>
    /// Turns off all the baby Split Buttons <br/><br/>
    /// </summary>
    public void SplitButtonsOff()
    {
        for (int i = 0; i < splitB.Length; i++)
        {
            splitB[i].GetComponent<SplitButtons>().SetActive(false);
        }
    }






    //End of Turn



    /// <summary>
    /// Checks if either player has both hands at zero, meaning the other player wins.<br/><br/>
    /// Returns true if there is a win and calls EndGame, false if there isn't and does nothing. <br/><br/>
    /// </summary>
    /// <returns></returns>
    private bool CheckWin()
    {
        if (GetHand(P1H1) == 0 && GetHand(P1H2) == 0)
        {
            EndGame(2);
            return true;
        }
        else if (GetHand(P2H1) == 0 && GetHand(P2H2) == 0)
        {
            EndGame(1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tells the End Screen who the winner is and then activates the End Screen to fade in. <br/><br/>
    /// </summary>
    /// <param name="player"> The player that won (1 or 2)</param>
    private void EndGame(int player)
    {
        endScreen.SetActive(true);
        endScreen.GetComponent<EndScreen>().EndGame(player);
    }





    //AI AGENT THINGS

    /// <summary>
    /// Allows the agent to get the hand value of any hand by inputting the player index and hand index. <br/><br/>
    /// </summary>
    /// <param name="playerIndex">Index of the player</param>
    /// <param name="handIndex">Index of the hand from the player 'playerIndex'</param>
    /// <returns>The hand value 'handIndex' of the player 'playerIndex'</returns>
    public int GetHand(int playerIndex, int handIndex)
    {
        return GetHand(P[playerIndex][handIndex]);
    }

    /// <summary>
    /// Tells the agent if they can use an alter button this turn. <br/><br/>
    /// </summary>
    /// <returns>Boolean value from the canAlter array</returns>
    public bool CanCurrPlayerAlter()
    {
        return canAlter[currTurn - 1];
    }

    /// <summary>
    /// Tells the agent the index of the current turn. <br/><br/>
    /// </summary>
    /// <returns>currTurn - 1</returns>
    public int GetCurrTurnIndex()
    {
        return currTurn - 1;
    }

    /// <summary>
    /// Allows the agent to directly attack by inputting the hand index of the attacker and defender. <br/><br/>
    /// </summary>
    /// <param name="attackerHandIndex">Index of the agent's attacking hand</param>
    /// <param name="defenderHandIndex">Index of the other player's target hand</param>
    public void DirectAttack(int attackerHandIndex, int defenderHandIndex)
    {
        int opponentIndex = GetOppPlayer(currTurn) - 1;

        hands[currTurn - 1] = P[currTurn - 1][attackerHandIndex];
        hands[opponentIndex] = P[opponentIndex][defenderHandIndex];

        Attack();

    }

    /// <summary>
    /// Allows the agent to directly use an alter button by inputting the action and hand index. <br/><br/>
    /// </summary>
    /// <param name="action">+ or - depending on the action (split is DirectSplit)</param>
    /// <param name="handIndex">Index of the hand they want to alter</param>
    public void DirectAlter(string action, int handIndex)
    {
        hands[currTurn - 1] = P[currTurn - 1][handIndex];
        Alter(action);
    }

    /// <summary>
    /// Allows the agent to get the number of split combinations available for the current hand values. <br/><br/>
    /// </summary>
    /// <returns>Number of split combinations (1 - 3)</returns>
    public int GetCurrSplitComboCount()
    {
        int split1 = GetHand(P[currTurn - 1][0]);
        int split2 = GetHand(P[currTurn - 1][1]);
        int total = split1 + split2;
        List<int> combos = new List<int>();
        for (int i = 0; i < total / 2 + 1; i++)
        {
            if (total - i >= 5) continue;
            combos.Add(i);
        }
        return combos.Count;
    }

    /// <summary>
    /// Allows the agent to directly split by inputting the combo index they want to split into based on the combinations found in SplitButtonsSetup(). <br/><br/>
    /// </summary>
    /// <param name="comboIndex">A number (0 - 2) to split by</param>
    public void DirectSplit(int comboIndex)
    {
        int split1 = GetHand(P[currTurn - 1][0]);
        int split2 = GetHand(P[currTurn - 1][1]);
        int total = split1 + split2;
        List<int> combos = new List<int>();

        for (int i = 0; i < total / 2 + 1; i++)
        {
            if (total - i >= 5) continue;
            combos.Add(i);
        }

        int[,] c = new int[combos.Count, 2];

        for (int i = 0; i < c.GetLength(0); i++)
        {
            c[i, 0] = combos[i];
            c[i, 1] = (total - combos[i]) % 5;
        }

        Split(c[comboIndex, 0], c[comboIndex, 1]);

    }

    /// <summary>
    /// Forciably resets the entire board state back to a default 1-1 match setup. <br/><br/>
    /// Called automatically by the Agent when an episode ends.
    /// </summary>
    public void ResetEnvironmentForAI()
    {
        // Reset all hand values back to 1
        SetHand(P1H1, 1);
        SetHand(P1H2, 1);
        SetHand(P2H1, 1);
        SetHand(P2H2, 1);

        // Reset alteration availability flags
        canAlter[0] = true;
        canAlter[1] = true;

        // Clean out stale selection array data
        hands[0] = null;
        hands[1] = null;

        // Force turn back to Player 1 and restart loop
        PlayerTurn(1);
    }

    /// <summary>
    /// Check if the game is over without triggering visual UI panels. <br/><br/>
    /// Returns 0 if active, 1 if P1 wins, 2 if P2 wins.
    /// </summary>
    public int CheckWinSilent()
    {
        if (GetHand(P1H1) == 0 && GetHand(P1H2) == 0) return 2; // P2 Wins
        if (GetHand(P2H1) == 0 && GetHand(P2H2) == 0) return 1; // P1 Wins
        return 0; // Game still active
    }

    /// <summary>
    /// Returns the opposing agent based on the caller's player number.
    /// </summary>
    public SticksAgent GetOpponent(int playerIndex)
    {
        return playerIndex == 0 ? agentP1 : agentP2;
    }

}
