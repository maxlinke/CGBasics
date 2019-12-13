using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour {

    private static InputSystem instance;
    
    Stack<Subscription> subscriptions;
    List<KeyCode> keysDown;
    List<KeyCode> keysHeld;
    List<KeyCode> keysUp;
    KeyCode[] keyCodes;

    public const KeyCode sensitivityIncreaseKey = KeyCode.LeftControl;
    public const KeyCode sensitivityReduceKey = KeyCode.LeftShift;

    public static float shiftCtrlMultiplier;

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(InputSystem)} is not null! Aborting");
            return;
        }
        instance = this;
        subscriptions = new Stack<Subscription>();
        keysDown = new List<KeyCode>();
        keysHeld = new List<KeyCode>();
        keysUp = new List<KeyCode>();
        var allKeyCodes = System.Enum.GetValues(typeof(KeyCode));
        keyCodes = new KeyCode[allKeyCodes.Length];
        for(int i=0; i<allKeyCodes.Length; i++){
            keyCodes[i] = (KeyCode)(allKeyCodes.GetValue(i));
        }
    }

    void Update () {
        UpdateShiftCtrlMultiplier();
        UpdateSubscribers();

        void UpdateShiftCtrlMultiplier () {
            float mult = 1f;
            if(Input.GetKey(sensitivityReduceKey)){
                mult *= 0.1f;
            }
            if(Input.GetKey(sensitivityIncreaseKey)){
                mult *= 10f;
            }
            shiftCtrlMultiplier = mult;
        }

        void UpdateSubscribers () {
            if(subscriptions.Count < 1){
                return;                     // don't even need to execute all the stuff below...
            }
            keysDown.Clear();
            keysHeld.Clear();
            keysUp.Clear();
            foreach(var keyCode in keyCodes){
                if(Input.GetKeyDown(keyCode)){
                    keysDown.Add(keyCode);
                }
                if(Input.GetKey(keyCode)){
                    keysHeld.Add(keyCode);
                }
                if(Input.GetKeyUp(keyCode)){
                    keysUp.Add(keyCode);
                }
            }
            var top = subscriptions.Peek();
            foreach(var keyEvent in top){
                if(keysDown.Contains(keyEvent.keyCode)){
                    keyEvent.onKeyDown?.Invoke();
                }
                if(keysHeld.Contains(keyEvent.keyCode)){
                    keyEvent.onKeyHeld?.Invoke();
                }
                if(keysUp.Contains(keyEvent.keyCode)){
                    keyEvent.onKeyUp?.Invoke();
                }
            }
        }
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    public static void Subscribe (object subscriber, params KeyEvent[] keyEvents) {
        instance.Sub(subscriber, keyEvents);
    }

    private void Sub (object subscriber, KeyEvent[] keyEvents) {
        subscriptions.Push(new Subscription(subscriber, keyEvents));
    }

    public static void UnSubscribe (object subscriber) {
        instance.UnSub(subscriber);
    }

    private void UnSub (object subscriber) {
        if(subscriptions.Count < 1){
            Debug.LogError($"{subscriber} tried to unsubscribe but the stack was already empty!");
            return;
        }
        if(subscriptions.Peek().subscriber == subscriber){
            subscriptions.Pop();
        }else{
            var otherStack = new Stack<Subscription>();
            bool done = false;
            while(subscriptions.Count > 0){
                var top = subscriptions.Pop();
                if(top.subscriber == subscriber){
                    Debug.LogWarning($"{subscriber} is unsubcribing while not on top of the stack! This is odd, but I'll do it...");
                    done = true;
                    break;
                }else{
                    otherStack.Push(top);
                }
            }
            if(!done){
                Debug.LogError($"{subscriber} tried to unsubscribe but wasn't a subscriber!");
            }
            while(otherStack.Count > 0){
                subscriptions.Push(otherStack.Pop());
            }
        }
    }

    public class KeyEvent {

        public readonly KeyCode keyCode;
        public readonly System.Action onKeyDown;
        public readonly System.Action onKeyHeld;
        public readonly System.Action onKeyUp;

        public KeyEvent (KeyCode keyCode, System.Action onKeyDown = null, System.Action onKeyHeld = null, System.Action onKeyUp = null) {
            this.keyCode = keyCode;
            this.onKeyDown = onKeyDown;
            this.onKeyHeld = onKeyHeld;
            this.onKeyUp = onKeyUp;
        }

    }

    private class Subscription {

        public readonly object subscriber;
        private KeyEvent[] keyEvents;

        public Subscription (object subscriber, KeyEvent[] input) {
            this.subscriber = subscriber;
            this.keyEvents = new KeyEvent[input.Length];
            input.CopyTo(keyEvents, 0);
        }

        public IEnumerator<KeyEvent> GetEnumerator () {
            foreach(var keyEvent in keyEvents){
                yield return keyEvent;
            }
        }

    }
	
}
