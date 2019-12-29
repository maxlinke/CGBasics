using UnityEngine;

public class CloseableScreen : MonoBehaviour {

    public void SetupCloseAction (System.Action onCloseCalled) {
        this.DoNextFrame(() => {
            InputSystem.Subscribe(this, new InputSystem.KeyEvent(KeyCode.Escape, () => {
                InputSystem.UnSubscribe(this);
                onCloseCalled?.Invoke();
            }));
        });
    }
	
}
