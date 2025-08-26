using UnityEngine;
using UnityEngine.Events;

public class SimpleMiniGameStarter : MonoBehaviour
{
    [Header("미니게임 설정")]
    [SerializeField] private GameObject miniGameObject;
    [SerializeField] private bool isGameActive = false;
    
    [Header("이벤트")]
    [SerializeField] private UnityEvent onGameStart;
    [SerializeField] private UnityEvent onGameEnd;
    
    public void StartMiniGame()
    {
        if (isGameActive) return;
        
        Debug.Log("미니게임 시작!");
        isGameActive = true;
        
        // 미니게임 오브젝트 활성화
        if (miniGameObject != null)
            miniGameObject.SetActive(true);
            
        // 이벤트 호출
        onGameStart?.Invoke();
    }
    
    public void EndMiniGame()
    {
        if (!isGameActive) return;
        
        Debug.Log("미니게임 종료!");
        isGameActive = false;
        
        // 미니게임 오브젝트 비활성화
        if (miniGameObject != null)
            miniGameObject.SetActive(false);
            
        // 이벤트 호출
        onGameEnd?.Invoke();
    }
    
    public void ToggleMiniGame()
    {
        if (isGameActive)
            EndMiniGame();
        else
            StartMiniGame();
    }
}
