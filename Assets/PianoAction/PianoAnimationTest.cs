using UnityEngine;
using UnityEngine.InputSystem;

public class PianoAnimationTest : MonoBehaviour
{

   
    [SerializeField] InputActionReference playPianoAction;
    Animator anim;
    

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
    }

    void Start()
    {
        playPianoAction.action.Enable();
        playPianoAction.action.performed += PlayPianoAnimation;
    }

    private void PlayPianoAnimation(InputAction.CallbackContext context)
    {
        Debug.Log("play");
        anim.SetTrigger("onPlay");
    }
}
