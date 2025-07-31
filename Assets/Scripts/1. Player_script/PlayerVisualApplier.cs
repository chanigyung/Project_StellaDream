using UnityEngine;

public class PlayerVisualApplier : MonoBehaviour
{
    [Header("머리")]
    public SpriteRenderer head;
    public SpriteRenderer face;
    public SpriteRenderer hair0;
    public SpriteRenderer hair1;
    public SpriteRenderer hair2;

    [Header("몸통")]
    public SpriteRenderer body;

    [Header("팔 & 손")]
    public SpriteRenderer leftArm;
    public SpriteRenderer rightArm;
    public SpriteRenderer leftHand;
    public SpriteRenderer rightHand;

    [Header("다리 & 발")]
    public SpriteRenderer leftLeg;
    public SpriteRenderer rightLeg;
    public SpriteRenderer leftFoot;
    public SpriteRenderer rightFoot;

    public void ApplyVisual(PlayerVisualData visual)
    {
        head.sprite      = visual.head;
        face.sprite      = visual.face;
        hair0.sprite     = visual.hair0;
        hair1.sprite     = visual.hair1;
        hair2.sprite     = visual.hair2;

        body.sprite      = visual.body;

        leftArm.sprite   = visual.leftArm;
        rightArm.sprite  = visual.rightArm;
        leftHand.sprite  = visual.leftHand;
        rightHand.sprite = visual.rightHand;

        leftLeg.sprite   = visual.leftLeg;
        rightLeg.sprite  = visual.rightLeg;
        leftFoot.sprite  = visual.leftFoot;
        rightFoot.sprite = visual.rightFoot;
    }
}
