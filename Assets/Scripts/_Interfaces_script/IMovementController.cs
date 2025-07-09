public interface IMovementController
{
    //넉백을 제외한 모든 이동 제어에 사용되는 인터페이스
    void SetRooted(bool isRooted);
    void SetStunned(bool isStunned);
    void SetPowerKnockbacked(bool powerKnockbacked);
}