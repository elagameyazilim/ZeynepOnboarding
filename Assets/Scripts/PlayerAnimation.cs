using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.SetBool(IsRunning, false);
    }

    public void PlayGame()
    {
        _animator.SetBool(IsRunning, true);
    }

    public void UpdateAnimationState(bool isRunning)
    {
        _animator.SetBool(IsRunning, isRunning);
    }
}