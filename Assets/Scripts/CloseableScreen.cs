using UnityEngine;

public abstract class CloseableScreen : MonoBehaviour {

    protected abstract bool CanBeClosed ();

    public void SetupCloseAction (System.Action onCloseCalled) {
        InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, () => {
            if(CanBeClosed()){
                InputSystem.UnSubscribe(this);
                onCloseCalled?.Invoke();
            }
        }));
    }
	
}
