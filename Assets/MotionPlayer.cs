using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class MotionPlayer : MonoBehaviour
{
    [SerializeField, Tooltip("上半身のみ選択したAvaterMask")]
    private AvatarMask upperMask;
    
    //実際にモーション遷移を担当するミキサー
    private List<MotionMixer> motionMixers = new List<MotionMixer>();

    //Playable API
    private AnimationLayerMixerPlayable layerMixer;
    private AnimationPlayableOutput output;
    private PlayableGraph graph;

    //GetComponent()による自動取得
    private Animator animator;

    //============================================================================================================

    void Awake()
    {
        animator = this.GetComponent<Animator>();

        //PlayableAPIの準備
        graph = PlayableGraph.Create();
        motionMixers.Add(new MotionMixer(this, graph));
        motionMixers.Add(new MotionMixer(this, graph));
        layerMixer = AnimationLayerMixerPlayable.Create(graph, 2);
        if (upperMask != null)
            layerMixer.SetLayerMaskFromAvatarMask(1, upperMask);

        output = AnimationPlayableOutput.Create(graph, "output", animator);
        graph.Play();
    }

    void Start()
    {
    }

    private void Update()
    {
        for (int i = 0; i < motionMixers.Count; i++)
        {
            if (motionMixers[i].isFinishedPlay && motionMixers[i].playParam.loop)
            {
                motionMixers[i].nowPlayable.SetTime(0f); //時間を巻き戻しループ再生
            }
            else if (motionMixers[i].isFinishedPlay && motionMixers[i].playParam.loop == false)
            {
                //モーション再生終了
            }
        }
    }

    void OnDestroy()
    {
        graph.Destroy();
    }

    //============================================================================================================

    //モーションを再生する
    public bool play(AnimationClip clip, MotionPlayerParam param)
    {
        if (clip == null || param == null)
            return false;

        MotionMixer motionMixer = motionMixers[param.layer];
        motionMixer.playParam = param;

        //切断
        graph.Disconnect(layerMixer, param.layer);

        //再接続
        motionMixer.reconnect(clip);
        layerMixer.ConnectInput(param.layer, motionMixer.mixer, sourceOutputIndex: 0, weight:1f);

        //出力
        output.SetSourcePlayable(layerMixer);
        
        return true;
    }

    //モーションを再生する(引数単純化版)
    public bool play(AnimationClip clip, bool loop, int layer = 0)
    {
        MotionPlayerParam param = new MotionPlayerParam(layer, loop_: loop);
        return play(clip, param);
    }

    //モーションを再生する(全身)
    public bool playAllLayer(AnimationClip clip, MotionPlayerParam param)
    {
        for (int i = 0; i < motionMixers.Count; i++)
            play(clip, param);

        return true;
    }

    //指定した時間でモーション再生を止める(=静止させる)
    public void pause(AnimationClip clip, int layer, float normalizedTime)
    {
        play(clip, loop: false, layer: layer); //再生

        double t = motionMixers[layer].nowPlayable.GetAnimationClip().length * normalizedTime;
        motionMixers[layer].nowPlayable.SetTime(t); //時間を指定→その時間におけるポーズになる
        motionMixers[layer].nowPlayable.SetSpeed(0f); //再生速度を0に→停止
    }
    
    //============================================================================================================

    //指定したモーションの再生が完了しているか
    public bool isPlayFinish(int layer)
    {
        return motionMixers[layer].isFinishedPlay;
    }

    //再生しているモーション名を取得する
    public string nowPlayClipName(int layer)
    {
        return motionMixers[layer].nowPlayable.GetAnimationClip().name;
    }

    //部位(上半身)の上書きをするか決定する
    public void setLayerEnabled(int layer, bool enable)
    {
        if (enable == true)
        {
            layerMixer.SetInputWeight(layer, 1f);
        }
        else
        {
            layerMixer.SetInputWeight(layer, 0f);
        }
    }
}