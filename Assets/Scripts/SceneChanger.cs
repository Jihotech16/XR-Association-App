using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

/// <summary>
/// 씬을 전환하는 컴포넌트.
/// VR에서 씬을 즉시 로드하면 화면이 한순간 끊겨 보이므로,
/// OVRScreenFade로 화면을 어둡게 만든 뒤 비동기로 로드한다.
/// </summary>
public class SceneChanger : MonoBehaviour
{
    [Header("전환할 씬")]
    [Tooltip("Build Settings에 등록되고 활성화된 씬 이름 (확장자 없이)")]
    [SerializeField] private string sceneName;

    [Header("페이드")]
    [Tooltip("음수면 OVRScreenFade의 fadeTime을 그대로 사용 (기본값 2초는 전환용으로는 길다)")]
    [SerializeField] private float fadeDurationOverride = 0.5f;

    [Header("Poke 버튼 연결")]
    [Tooltip("켜면 같은 오브젝트의 Interactable을 찾아, 눌렸을 때 자동으로 전환한다")]
    [SerializeField] private bool hookInteractable = true;

    private IInteractableView interactableView;
    private bool isTransitioning;

    void Start()
    {
        if (hookInteractable)
        {
            // MiniGameStarter와 같은 방식으로 Poke/Ray Interactable에 붙는다
            interactableView = GetComponent<IInteractableView>();

            if (interactableView != null)
                interactableView.WhenStateChanged += OnInteractionStateChanged;
            else
                Debug.LogWarning($"[SceneChanger] '{name}'에 Interactable이 없어 버튼 연결을 건너뛴다. " +
                                 "Interactable 오브젝트에 붙이거나 hookInteractable을 끌 것.", this);
        }
    }

    void OnDestroy()
    {
        if (interactableView != null)
            interactableView.WhenStateChanged -= OnInteractionStateChanged;
    }

    private void OnInteractionStateChanged(InteractableStateChangeArgs args)
    {
        // 버튼이 눌렸을 때 (Select 상태가 되었을 때)
        if (args.NewState == InteractableState.Select)
            ChangeScene();
    }

    /// <summary>
    /// 씬 전환을 시작한다. UnityEvent나 다른 스크립트에서 호출할 수 있다.
    /// </summary>
    public void ChangeScene()
    {
        // Poke는 한 번 누를 때 이벤트가 여러 번 들어올 수 있어 중복 전환을 막는다
        if (isTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"[SceneChanger] '{name}'의 sceneName이 비어 있어 전환할 수 없다.", this);
            return;
        }

        if (!IsSceneRegistered(sceneName))
        {
            // 등록 안 된 씬을 로드하면 Unity가 모호한 에러만 내므로 먼저 걸러낸다
            Debug.LogError($"[SceneChanger] '{sceneName}' 씬이 Build Settings에 없거나 비활성이다. " +
                           "File > Build Profiles에서 추가하고 체크할 것.", this);
            return;
        }

        isTransitioning = true;
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        // instance는 OVRScreenFade.Start()에서 설정되므로 호출 시점에 가져온다
        OVRScreenFade fade = OVRScreenFade.instance;

        if (fade != null)
        {
            // FadeOut은 fade.fadeTime 동안 진행된다.
            // 기다리는 시간이 이보다 짧으면 덜 어두운 상태로 로드되므로 값을 맞춰둔다.
            if (fadeDurationOverride >= 0f)
                fade.fadeTime = fadeDurationOverride;

            fade.FadeOut();

            // FadeOut은 코루틴을 반환하지 않으므로 직접 기다린다.
            // 일시정지 중에도 전환되도록 Realtime을 쓴다.
            yield return new WaitForSecondsRealtime(fade.fadeTime);
        }
        else
        {
            Debug.LogWarning("[SceneChanger] 씬에 OVRScreenFade가 없어 페이드 없이 전환한다. " +
                             "Camera Rig의 CenterEyeAnchor에 OVRScreenFade를 추가할 것.", this);
        }

        // 도착한 씬의 OVRScreenFade가 fadeOnStart로 알아서 밝아진다
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Build Settings에 등록되고 활성화된 씬인지 확인한다.
    /// sceneCountInBuildSettings는 활성 씬만 세므로 체크가 풀린 씬은 걸러진다.
    /// </summary>
    private static bool IsSceneRegistered(string targetName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);

            if (System.IO.Path.GetFileNameWithoutExtension(path) == targetName)
                return true;
        }

        return false;
    }
}
