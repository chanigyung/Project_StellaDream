public interface IMovementSkill
{
    //이동을 수반하는 스킬에 적용될 인터페이스
    bool IsExecutingMovement { get; } //이동기 실행중 여부 확인용
    event System.Action OnMovementEnd; //외부에서 이동 스킬 종료시 발생시킬 이벤트 예약
}