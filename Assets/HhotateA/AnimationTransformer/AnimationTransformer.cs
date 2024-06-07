using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationTransformer : MonoBehaviour
{
    [SerializeField,Tooltip("再生したいアニメーション")]
    AnimationClip clip;

    [SerializeField, Range(0f, 1f),Tooltip("再生時間（MotionTime）")] 
    float playTime = 1f;
    
    const string layerName = "temp";
    const string stateName = "temp";
    const string timeParameter = "temp";

    RuntimeAnimatorController controllerCurrent;

    /// <summary>
    /// アニメーターを元に戻す（2022.3.6f1では、アニメーションも元に戻る）
    /// </summary>
    public void RevertAnimation()
    {
        var anim = gameObject.GetComponent<Animator>();
        anim.runtimeAnimatorController = controllerCurrent;
    }

    /// <summary>
    /// アニメーターを設定する
    /// </summary>
    public void TransformAnimation()
    {
        // アニメーターの初期値取得
        var anim = gameObject.GetComponent<Animator>();
        controllerCurrent = anim.runtimeAnimatorController;
        anim.runtimeAnimatorController = CreateAnimator();
        
        // このままだと動かないので、ちょっとだけアニメーターを進める
        anim.Play(stateName);
        anim.Update(1f/60f);
    }
    
    /// <summary>
    /// 独自のアニメーターを作成する
    /// </summary>
    /// <param name="controller"></param>
    AnimatorController CreateAnimator()
    {
        var controller = new AnimatorController();
        controller.AddLayer(layerName);
        controller.parameters = new AnimatorControllerParameter[]
        {
            new AnimatorControllerParameter
            {
                name = timeParameter,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = playTime
            }
        };
        var stateMachine = controller.layers[0].stateMachine;
        var state = stateMachine.AddState(stateName);
        state.motion = clip;
        state.timeParameter = timeParameter;
        state.timeParameterActive = true;
        return controller;
    }
    
    [CustomEditor(typeof(AnimationTransformer))]
    public class AnimationTransformerEditor : Editor
    {
        private bool revertEnable = true;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI ();
            if (GUILayout.Button("Transform"))
            {
                AnimationTransformer animationTransformer = target as AnimationTransformer;
                animationTransformer.TransformAnimation();
                revertEnable = false;
            }
            using (new EditorGUI.DisabledScope(revertEnable))
            {
                if (GUILayout.Button("Revert"))
                {
                    AnimationTransformer animationTransformer = target as AnimationTransformer;
                    animationTransformer.RevertAnimation();
                    revertEnable = true;
                }
            }
        }
    }
}
