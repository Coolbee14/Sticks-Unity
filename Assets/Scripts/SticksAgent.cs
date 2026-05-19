using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class SticksAgent : Agent
{
    [Header("References")]
    [SerializeField] private TurnSystem turnSystem;

    [Header("AI Settings")]
    [SerializeField] private int aiPlayerNumber = 2; // AI defaults to Player 2 (Index 1)

    private int aiIndex => aiPlayerNumber - 1;
    private int enemyIndex => turnSystem.GetOppPlayer(aiPlayerNumber) - 1;

    private float maxHandValue = 4f; // Maximum fingers on a hand before it "dies"

    /// <summary>
    /// Step 1: Collect Observations. <br/><br/>
    /// Pass the state matrix down to the Python neural net.<br/><br/>
    /// Vector Observation Space Size must be set to 5 in the inspector.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(turnSystem.GetHand(aiIndex, 0) / maxHandValue);       // Observation 0: AI Hand 1 (0-4)
        sensor.AddObservation(turnSystem.GetHand(aiIndex, 1) / maxHandValue);       // Observation 1: AI Hand 2 (0-4)
        sensor.AddObservation(turnSystem.GetHand(enemyIndex, 0) / maxHandValue);   // Observation 2: Player Hand 1 (0-4)
        sensor.AddObservation(turnSystem.GetHand(enemyIndex, 1) / maxHandValue);   // Observation 3: Player Hand 2 (0-4)
        sensor.AddObservation(turnSystem.CanCurrPlayerAlter() ? 1f : 0f); // Observation 4: Alter Allowed? (1 or 0)
    }

    /// <summary>
    /// Step 2: Action Masking. <br/><br/>
    /// Prevents the AI from picking invalid moves (like splitting when it can't or picking empty variations).
    /// </summary>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // If it isn't mathematically the AI's turn, lock out all 11 choices completely
        if (turnSystem.GetCurrTurnIndex() != aiIndex)
        {
            for (int i = 0; i < 11; i++) actionMask.SetActionEnabled(0, i, false);
            actionMask.SetActionEnabled(0, 11, true);
            return;
        }

        actionMask.SetActionEnabled(0, 11, false); // Ensure the "No Action" option is always disabled on the AI's turn

        int aiH1 = turnSystem.GetHand(aiIndex, 0);
        int aiH2 = turnSystem.GetHand(aiIndex, 1);
        int enH1 = turnSystem.GetHand(enemyIndex, 0);
        int enH2 = turnSystem.GetHand(enemyIndex, 1);
        bool canAlter = turnSystem.CanCurrPlayerAlter();

        // --- Attack Constraints (Actions 0 - 3) ---
        actionMask.SetActionEnabled(0, 0, aiH1 != 0 && enH1 != 0); // H1 attacks Player H1
        actionMask.SetActionEnabled(0, 1, aiH1 != 0 && enH2 != 0); // H1 attacks Player H2
        actionMask.SetActionEnabled(0, 2, aiH2 != 0 && enH1 != 0); // H2 attacks Player H1
        actionMask.SetActionEnabled(0, 3, aiH2 != 0 && enH2 != 0); // H2 attacks Player H2

        // --- Alteration Constraints (Actions 4 - 7) ---
        if (!canAlter)
        {
            actionMask.SetActionEnabled(0, 4, false); // +1 H1
            actionMask.SetActionEnabled(0, 5, false); // +1 H2
            actionMask.SetActionEnabled(0, 6, false); // -1 H1
            actionMask.SetActionEnabled(0, 7, false); // -1 H2
        }
        else
        {
            actionMask.SetActionEnabled(0, 4, aiH1 != 4); //Cannot add to a full hand (it would kill it)
            actionMask.SetActionEnabled(0, 5, aiH2 != 4);
            actionMask.SetActionEnabled(0, 6, aiH1 != 0); // Cannot subtract from a dead hand
            actionMask.SetActionEnabled(0, 7, aiH2 != 0);
        }

        // --- Split Constraints (Actions 8 - 10) ---
        if (!canAlter)
        {
            actionMask.SetActionEnabled(0, 8, false);
            actionMask.SetActionEnabled(0, 9, false);
            actionMask.SetActionEnabled(0, 10, false);
        }
        else
        {
            int availableCombos = turnSystem.GetCurrSplitComboCount();

            // Mask dynamic available combos depending on split parameters (e.g. 1-1 scenario allows 2 choices)
            actionMask.SetActionEnabled(0, 8, availableCombos >= 1);
            actionMask.SetActionEnabled(0, 9, availableCombos >= 2);
            actionMask.SetActionEnabled(0, 10, availableCombos >= 3);
        }

    }

    /// <summary>
    /// Step 3: Receive and Execute Action. <br/><br/>
    /// Executes the move selected by the brain, evaluates win/loss conditions, 
    /// and manages the episode loop.
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Guard Check: Only proceed if it is actually this Agent's turn
        if (turnSystem.GetCurrTurnIndex() != aiIndex) return;

        int enemyH1Before = turnSystem.GetHand(enemyIndex, 0);
        int enemyH2Before = turnSystem.GetHand(enemyIndex, 1);
        int aiH1Before = turnSystem.GetHand(aiIndex, 0);
        int aiH2Before = turnSystem.GetHand(aiIndex, 1);


        // Retrieve the single discrete choice from Branch 0 (Values 0 - 10)
        int action = actions.DiscreteActions[0];
        ExecuteAIAction(action);

        // Fetch post-action hand values to verify if the match has concluded
        int en1Value = turnSystem.GetHand(enemyIndex, 0);
        int en2Value = turnSystem.GetHand(enemyIndex, 1);
        int ai1Value = turnSystem.GetHand(aiIndex, 0);
        int ai2Value = turnSystem.GetHand(aiIndex, 1);

        // ─── MINOR REWARD SHAPING ───
        // ─── OFFENSIVE REWARDS (Pat on the back) ───
        if (enemyH1Before > 0 && en1Value == 0) AddReward(0.25f); // Knocked out enemy H1
        if (enemyH2Before > 0 && en2Value == 0) AddReward(0.25f); // Knocked out enemy H2


        // ─── DEFENSIVE PUNISHMENTS (Slap on the wrist) ───
        // If the AI hand was alive, but is now dead after the action cycle
        if (action >= 0 && action <= 3)
        {
            if (aiH1Before > 0 && ai1Value == 0) AddReward(-0.1f); // AI Hand 1 went to zero
            if (aiH2Before > 0 && ai2Value == 0) AddReward(-0.1f); // AI Hand 2 went to zero
        }


        // --- Check if either player has been wiped out ---
        bool enDead = (en1Value == 0 && en2Value == 0);
        bool aiDead = (ai1Value == 0 && ai2Value == 0);

        if (enDead || aiDead)
        {
            SticksAgent opponent = turnSystem.GetOpponent(turnSystem.GetOppPlayer(aiPlayerNumber) - 1);
            if (enDead)
            {
                SetReward(1.0f);       // Winner gets full prize
                EndEpisode();          // End winner episode

                if (opponent != null)
                {
                    opponent.AddReward(-1.0f); // Loser gets penalized
                    opponent.EndEpisode();     // End loser episode
                }
            }
            else //if AI died 
            {
                SetReward(-1.0f);      // I lost
                EndEpisode();

                if (opponent != null)
                {
                    opponent.AddReward(1.0f);  // Opponent won
                    opponent.EndEpisode();
                }
            }

            // 4. Safely wipe the board clear for the next match
            turnSystem.ResetEnvironmentForAI();
            return; // Exit out of the step entirely

        }

    }



    /// <summary>
    /// Decodes the selected action index into the direct execution methods inside TurnSystem.
    /// </summary>
    private void ExecuteAIAction(int action)
    {
        switch (action)
        {
            // Attacks
            case 0: turnSystem.DirectAttack(0, 0); break; // H1 attacks Player H1
            case 1: turnSystem.DirectAttack(0, 1); break; // H1 attacks Player H2
            case 2: turnSystem.DirectAttack(1, 0); break; // H2 attacks Player H1
            case 3: turnSystem.DirectAttack(1, 1); break; // H2 attacks Player H2

            // Alterations
            case 4: turnSystem.DirectAlter("+", 0); break; // +1 to Hand 1
            case 5: turnSystem.DirectAlter("+", 1); break; // +1 to Hand 2
            case 6: turnSystem.DirectAlter("-", 0); break; // -1 to Hand 1
            case 7: turnSystem.DirectAlter("-", 1); break; // -1 to Hand 2

            // Splits
            case 8: turnSystem.DirectSplit(0); break; // Split combination option 1
            case 9: turnSystem.DirectSplit(1); break; // Split combination option 2
            case 10: turnSystem.DirectSplit(2); break; // Split combination option 3

            case 11: break; // No Action (Only when it's not the AI's turn)
        }
    }

    /// <summary>
    /// Optional Step 4: Heuristic Dev Tools. <br/><br/>
    /// Allows you to manually play as the AI in the Unity Editor using your keyboard keys
    /// when the Behavior Type is set to "Heuristic Only".
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        // Maintain the previous action selection as a fallback default
        int chosenAction = discreteActions[0];

        if (Input.GetKeyDown(KeyCode.Alpha1)) chosenAction = 0; // Attack 1->1
        if (Input.GetKeyDown(KeyCode.Alpha2)) chosenAction = 1; // Attack 1->2
        if (Input.GetKeyDown(KeyCode.Alpha3)) chosenAction = 4; // Alter +1 Hand 1
        if (Input.GetKeyDown(KeyCode.Alpha4)) chosenAction = 8; // Split Option 1

        discreteActions[0] = chosenAction;
    }



}
