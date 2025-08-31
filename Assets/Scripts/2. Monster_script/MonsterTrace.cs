// using UnityEngine;

// public class MonsterTrace : MonoBehaviour
// {
//     private MonsterController monsterController;
//     private string target = "Player";

//     void Start()
//     {
//         monsterController = GetComponentInParent<MonsterController>();
//     }

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag(target))
//         {
//             monsterController.GetComponent<MonsterMovement>()?.SetTraceTarget(other.gameObject);
//         }
//     }

//     void OnTriggerStay2D(Collider2D other)
//     {
//         if (other.CompareTag(target))
//         {
//             monsterController.GetComponent<MonsterMovement>()?.SetTracing(true);
//         }
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag(target))
//         {
//             monsterController.GetComponent<MonsterMovement>()?.SetTracing(false);
//         }
//     }
// }
