using UnityEngine;
using Oculus.Interaction;
using TMPro;

public class MiniGameStarter : MonoBehaviour
{
    [Header("미니게임 설정")]
    [SerializeField] private TargetMiniGame targetMiniGame; // 타겟 미니게임 참조
    [SerializeField] private bool isGameActive = false;
    
    [Header("UI 텍스트")]
    [SerializeField] private GameObject welcomeText; // 환영 텍스트
    [SerializeField] private GameObject scoreText;   // 점수 텍스트
    [SerializeField] private GameObject timerText;   // 타이머 텍스트
    [SerializeField] private GameObject gameOverText; // 게임 오버 텍스트
    
    [Header("텍스트 관리")]
    [SerializeField] private float welcomeReturnDelay = 60f; // 게임 종료 후 환영 텍스트로 돌아가는 시간 (초)
    
    
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
            
        // 초기 텍스트 상태 설정 (Welcome만 활성화)
        SetWelcomeTextActive();
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
        
        // 게임 중 텍스트 활성화 (Score, Timer만)
        SetGameTextActive();
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
        
        // 게임 오버 텍스트 활성화
        SetGameOverTextActive();
        
        // 1분 후 환영 텍스트로 돌아가기
        Invoke(nameof(ReturnToWelcomeText), welcomeReturnDelay);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (interactableView != null)
            interactableView.WhenStateChanged -= OnInteractionStateChanged;
    }
    
    // 환영 텍스트만 활성화
    private void SetWelcomeTextActive()
    {
        if (welcomeText != null) welcomeText.SetActive(true);
        if (scoreText != null) scoreText.SetActive(false);
        if (timerText != null) timerText.SetActive(false);
        if (gameOverText != null) gameOverText.SetActive(false);
        
        Debug.Log("환영 텍스트 활성화");
    }
    
    // 게임 중 텍스트 활성화 (Score, Timer만)
    private void SetGameTextActive()
    {
        if (welcomeText != null) welcomeText.SetActive(false);
        if (scoreText != null) scoreText.SetActive(true);
        if (timerText != null) timerText.SetActive(true);
        if (gameOverText != null) gameOverText.SetActive(false);
        
        Debug.Log("게임 중 텍스트 활성화 (Score, Timer)");
    }
    
    // 게임 오버 텍스트만 활성화
    private void SetGameOverTextActive()
    {
        if (welcomeText != null) welcomeText.SetActive(false);
        if (scoreText != null) scoreText.SetActive(false);
        if (timerText != null) timerText.SetActive(false);
        if (gameOverText != null) 
        {
            gameOverText.SetActive(true);
            
            // 최종 점수 표시
            if (targetMiniGame != null)
            {
                int finalScore = targetMiniGame.GetCurrentScore();
                if (gameOverText.GetComponent<TextMeshPro>() != null)
                {
                    gameOverText.GetComponent<TextMeshPro>().text = $"게임 종료!\n최종 점수: {finalScore}\n\n다시 도전해보세요!";
                }
            }
        }
        
        Debug.Log("게임 오버 텍스트 활성화");
    }
    
    // 환영 텍스트로 돌아가기 (1분 후 자동 호출)
    private void ReturnToWelcomeText()
    {
        SetWelcomeTextActive();
        Debug.Log("1분 경과 - 환영 텍스트로 복귀");
    }
    
    // TargetMiniGame에서 시간 종료 시 호출되는 메서드
    public void OnGameTimeEnded()
    {
        if (isGameActive)
        {
            EndMiniGame();
        }
    }
}
