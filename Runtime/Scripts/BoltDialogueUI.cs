using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;
using Yarn;
using Yarn.Unity;

namespace Yarn.Unity.Bolt 
{ 
    public class BoltDialogueUI : Yarn.Unity.DialogueUIBehaviour
    {
        private bool isRunningLine = false;                     // True when a line has been sent to be displayed by the dialogue runner and execution is paused.
        private bool isRunningOptions = false;                  // True when options has been sent to be displayed by the dialogue runner and execution is paused.
        private int optionsCount = 0;                           // Number of options sent to be displayed

        private Action lineCompleteHandler;                     // Action supplied by RunLine to be triggered later
        private Action<int> currentOptionSelectionHandler;      // Action supplied but RunOptions to be triggered later

        /// <summary>
        /// Runs a line supplied by the DialogueRunner. Refer to YarnSpinner docs for more information.
        /// </summary>
        public override Dialogue.HandlerExecutionType RunLine(Line line, ILineLocalisationProvider localisationProvider, Action onLineComplete)
        {
            isRunningLine = true;
            lineCompleteHandler = onLineComplete;
            string text = localisationProvider.GetLocalisedTextForLine(line);

            // Trigger Bolt event with line information so it can be used in Bolt graphs.
            CustomEvent.Trigger(this.gameObject, "RUN_LINE", text);

            return Dialogue.HandlerExecutionType.PauseExecution;
        }

        /// <summary>
        /// Runs Options supplied by the DialogueRunner. Refer to YarnSpinner docs for more information.
        /// </summary>
        public override void RunOptions(OptionSet optionSet, ILineLocalisationProvider localisationProvider, Action<int> onOptionSelected)
        {
            isRunningOptions = true;
            currentOptionSelectionHandler = onOptionSelected;

            List<string> OptionsText = new List<string>();
            optionsCount = 0;

            // Convert the options into a String List and count the number of options.
            foreach (var optionString in optionSet.Options)
            {
                var optionText = localisationProvider.GetLocalisedTextForLine(optionString.Line);
                OptionsText.Add(optionText);
                optionsCount++;
            }

            // Trigger Bolt event with Options information
            CustomEvent.Trigger(this.gameObject, "RUN_OPTIONS", OptionsText, optionsCount);
        }

        /// <summary>
        /// Runs a Yarn Command triggered by the DialogueRunner. Refer to YarnSpinner docs for more information.
        /// </summary>
        public override Dialogue.HandlerExecutionType RunCommand(Command command, Action onCommandComplete)
        {
            // Trigger a Bolt event with the full command string
            CustomEvent.Trigger(this.gameObject, "RUN_COMMAND", command.Text);

            // Tell the Dialogue Runner to continue on and not wait for the command signal its completed
            // Mirrors the functionality of the default DialogueUI
            return Dialogue.HandlerExecutionType.ContinueExecution;
        }

        /// <summary>
        /// Called when the dialogue system has started
        /// </summary>
        public override void DialogueStarted()
        {
            // Trigger Bolt event
            CustomEvent.Trigger(this.gameObject, "DIALOGUE_STARTED");
        }

        /// <summary>
        /// Called when the dialogue system has finished
        /// </summary>
        public override void DialogueComplete()
        {
            // Trigger Bolt event
            CustomEvent.Trigger(this.gameObject, "DIALOGUE_COMPLETED");
        }

        /// <summary>
        /// Called when a node has been completed
        /// </summary>
        public override Dialogue.HandlerExecutionType NodeComplete(string nextNode, Action onComplete)
        {
            // Trigger Bolt event with next node string
            CustomEvent.Trigger(this.gameObject, "NODE_COMPLETED", nextNode);

            // Tell the Dialogue Runner to continue on, we can't do much about actions in Bolt
            return Dialogue.HandlerExecutionType.ContinueExecution;
        }

        /// <summary>
        /// Called when the player wants to request the next step of the story.
        /// To be used by something like a continue button.
        /// </summary>
        public void FinishLine() 
        {
            // If the method was called when it wasn't expected to
            if (!isRunningLine) 
            {
                Debug.LogWarning("Finish Line method was called but no line is being displayed currently", this.gameObject);
                return;
            }

            isRunningLine = false;

            // Invoke the action with a safety null check
            lineCompleteHandler?.Invoke();
        }

        /// <summary>
        /// Called when an option is selected
        /// To be used by something like the options buttons when selecting a dialogue choice
        /// </summary>
        public void SelectOption(int optionID)
        {
            // If the method was called when it wasn't expected to
            if (!isRunningOptions) 
            {
                Debug.LogWarning("Select Option was called but no options are being displayed", this.gameObject);
                return;
            }

            // If we get a possible invalid option ID, throw and error before it gets passed to the Dialogue Runner
            if (optionID > optionsCount)
            {
                Debug.LogError("Select Option was called with an Option ID greater than the number of possible options, preventing option selection", this.gameObject);
                return;
            }

            isRunningOptions = false;
            optionsCount = 0;

            // Invoke the action with a safety null check
            currentOptionSelectionHandler?.Invoke(optionID);
        }
    }
}
