using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;
using Yarn;
using Yarn.Unity;

namespace Yarn.Unity.Bolt 
{
    public class BoltYarnSpinnerHelper : MonoBehaviour
    {
        [Tooltip("Leave this on if you want the Helper to automatically create a Bolt Scene Variable called YS_BoltHelper, " +
            "if you turn this off then make sure you have a system to get a reference to the Dialogue Runner in your graphs")]
        public bool CreateSceneVariable = true;
        [Tooltip("Stops the Helper from creating too many messages in the debug console, recomended to leave this off")]
        public bool SuppressLogMessages = false;

        [HideInInspector]
        public DialogueRunner DialogueRunnerReference; // Hidden in the inspector, but scripts and graphs can access it

        private void Awake()
        {
            // Attempt to find the Dialogue Runner on current game object
            DialogueRunnerReference = GetComponent<DialogueRunner>();

            if (DialogueRunnerReference == null)
                Debug.LogError("The Bolt YarnSpinner Helper can't find the Dialogue Runner! Did you put me on the same game object as the DialogueRunner script?", this.gameObject);

            // Create Bolt Scene variable
            if (!Variables.ActiveScene.IsDefined("YS_BoltHelper") && CreateSceneVariable)
            {
                Variables.ActiveScene.Set("YS_BoltHelper", this.gameObject);

                if (!SuppressLogMessages)
                    Debug.Log("Bolt YarnSpinner Helper scene variable created successfully", this.gameObject);
            }
            else
            {
                if (!SuppressLogMessages)
                    Debug.LogWarning("Bolt YarnSpinner Helper variable not created, make sure to edit GetYarnSpinnerComponets Super Unit to ensure everything still works", this.gameObject);
            }
        }

        /// <summary>
        /// Subscribing to DialogueRunner Unity Events
        /// Good practice when subscribing to Unity Events
        /// </summary>
        private void OnEnable()
        {
            if (DialogueRunnerReference != null)
            {
                DialogueRunnerReference.onNodeStart.AddListener(TriggerOnNodeStart);
                DialogueRunnerReference.onNodeComplete.AddListener(TriggerOnNodeComplete);
                DialogueRunnerReference.onDialogueComplete.AddListener(TriggerOnDialogueComplete);
            }
        }

        /// <summary>
        /// Unubscribing to DialogueRunner Unity Events
        /// While normally this wouldn't be called in this application, its still good practice to do this
        /// </summary>
        private void OnDisable()
        {
            if (DialogueRunnerReference != null)
            {
                DialogueRunnerReference.onNodeStart.RemoveListener(TriggerOnNodeStart);
                DialogueRunnerReference.onNodeComplete.RemoveListener(TriggerOnNodeComplete);
                DialogueRunnerReference.onDialogueComplete.RemoveListener(TriggerOnDialogueComplete);
            }
        }

        /// <summary>
        /// Attatch to the default DialogueUI OnLineUpdate event to use it in Bolt graphs
        /// </summary>
        public void TriggerOnLineUpdate(string line) 
        {
            CustomEvent.Trigger(this.gameObject, "ON_LINE_UPDATE", line);
        }

        /// <summary>
        /// Attatch to the default DialogueUI OnCommand event to use it in Bolt graphs
        /// </summary>
        public void TriggerOnCommand(string command) 
        {
            CustomEvent.Trigger(this.gameObject, "RUN_COMMAND", command);
        }

        /// <summary>
        /// Automatically attatched by the helper to use the OnNodeStart Dialogue Runner events in Bolt graphs
        /// </summary>
        private void TriggerOnNodeStart(string nodeName) 
        {
            CustomEvent.Trigger(this.gameObject, "ON_NODE_START", nodeName);
        }

        /// <summary>
        /// Automatically attatched by the helper to use the OnNodeComplete events Dialogue Runner in Bolt graphs
        /// </summary>
        private void TriggerOnNodeComplete(string nodeName)
        {
            CustomEvent.Trigger(this.gameObject, "ON_NODE_COMPLETE", nodeName);
        }

        /// <summary>
        /// Automatically attatched by the helper to use the OnDialogueFinish events Dialogue Runner in Bolt graphs
        /// </summary>
        private void TriggerOnDialogueComplete()
        {
            CustomEvent.Trigger(this.gameObject, "ON_DIALOGUE_FINISH");
        }
    }
}
