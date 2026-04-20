using UnityEngine;

[CreateAssetMenu(menuName = "Player/VisualData")]
public class PlayerVisualData : ScriptableObject
{
    public string characterID;

    public Sprite head;
    public Sprite hair0;
    public Sprite hair1;
    public Sprite hair2;
    public Sprite leftArm;
    public Sprite leftHand;
    public Sprite rightArm;
    public Sprite rightHand;
    public Sprite leftLeg;
    public Sprite leftFoot;
    public Sprite rightLeg;
    public Sprite rightFoot;

    [Header("Animated Parts")]
    public AnimationClip faceClip;
    public AnimationClip bodyClip;
    public AnimationClip tailClip;
    public AnimationClip backEffectClip;
}
