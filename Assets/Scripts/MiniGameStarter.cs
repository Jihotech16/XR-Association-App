using UnityEngine;
using Oculus.Interaction;

public class MiniGameStarter : MonoBehaviour
{
    [Header("미니게임 설정")]
    [SerializeField] private TargetMiniGame targetMiniGame; // 타겟 미니게임 참조
    [SerializeField] private bool isGameActive = false;
    
    [Header("UI 설정")]
    [SerializeField] private GameObject startUI; // 시작 UI
    [SerializeField] private GameObject gameUI;  // 게임 중 UI
    
    private IInteractableView interactableView;
    
    void Start()
    {
        // Poke Interaction 컴포넌트 찾기
        interactableView = GetComponent<IInteractableView>();
        
        if (interactableView != null)
        {
            // 이벤트 구독
            interactableView.WhenStateChanged += OnInteractionStateChanged;
        }
        
        // 초기 상태 설정
        if (targetMiniGame != null)
            targetMiniGame.gameObject.SetActive(false);
            
        if (startUI != null)
            startUI.SetActive(true);
            
        if (gameUI != null)
            gameUI.SetActive(false);
    }
    
    private void OnInteractionStateChanged(InteractableStateChangeArgs args)
    {
        // 버튼이 눌렸을 때 (Select 상태가 되었을 때)
        if (args.NewState == InteractableState.Select)
        {
            HandleButtonPress();
        }
    }
    
    private void HandleButtonPress()
    {
        if (isGameActive)
        {
            // 게임이 진행 중이면 종료
            EndMiniGame();
        }
        else
        {
            // 게임이 종료되었거나 시작 전이면 시작/재시작
            StartMiniGame();
        }
    }
    
    public void StartMiniGame()
    {
        if (isGameActive) return;
        
        Debug.Log("타겟 미니게임 시작!");
        isGameActive = true;
        
        // 미니게임 활성화 및 시작
        if (targetMiniGame != null)
        {
            targetMiniGame.gameObject.SetActive(true);
            targetMiniGame.StartGame();
        }
            
        // UI 변경
        if (startUI != null)
            startUI.SetActive(false);
            
        if (gameUI != null)
            gameUI.SetActive(true);
    }
    
    public void EndMiniGame()
    {
        if (!isGameActive) return;
        
        Debug.Log("타겟 미니게임 종료!");
        isGameActive = false;
        
        // 미니게임 종료
        if (targetMiniGame != null)
        {
            targetMiniGame.EndGame();
            targetMiniGame.gameObject.SetActive(false);
        }
            
        // UI 변경
        if (startUI != null)
            startUI.SetActive(true);
            
        if (gameUI != null)
            gameUI.SetActive(false);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (interactableView != null)
            interactableView.WhenStateChanged -= OnInteractionStateChanged;
    }
}
