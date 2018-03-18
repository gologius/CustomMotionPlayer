using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections.Generic;
using System.Collections;

public class MotionMixer
{
    //再生時のパラメータ
    public MotionPlayerParam playParam;

    //モーション再生状態
    public bool isFinishedPlay
    {
        get
        {
            //playableが初期化されていない場合
            if (nowPlayable.IsValid() == false)
                return false;

            //予定されている再生時間を超えていれば、再生終了とみなす
            if (nowPlayable.GetTime() > nowPlayable.GetAnimationClip().length)
                return true;

            return false;
        }
    }

    //遷移状態
    public bool isFinishedTrans { get; private set; }

    //コルーチン用
    private MotionPlayer player;
    private Coroutine fadeCoroutine = null;

    //Playable系
    public AnimationClipPlayable beforePlayable;
    public AnimationClipPlayable nowPlayable;
    public AnimationMixerPlayable mixer;
    private PlayableGraph graph;

    //コンストラクタ
    public MotionMixer(MotionPlayer player, PlayableGraph graph)
    {
        this.player = player;
        this.playParam = new MotionPlayerParam();
        this.beforePlayable = AnimationClipPlayable.Create(graph, null);
        this.mixer = AnimationMixerPlayable.Create(graph, 2);
        this.graph = graph;
    }

    //AnimationClipの切り替え
    public void reconnect(AnimationClip clip)
    {
        //切断
        graph.Disconnect(mixer, 0);
        graph.Disconnect(mixer, 1);
        if (beforePlayable.IsValid())
            beforePlayable.Destroy();

        //更新
        beforePlayable = nowPlayable;
        nowPlayable = AnimationClipPlayable.Create(graph, clip);

        //再接続
        mixer.ConnectInput(1, beforePlayable, 0);
        mixer.ConnectInput(0, nowPlayable, 0);

        if (playParam.reverse)
        {
            //逆再生時の時間設定
            nowPlayable.SetTime(nowPlayable.GetAnimationClip().length);
            nowPlayable.SetSpeed(-1f);
        }
        else
        {
            //通常時の時間設定
            nowPlayable.SetSpeed(1f);
        }

        //同期遷移時の時間設定
        if (playParam.syncTrans)
            nowPlayable.SetTime(beforePlayable.GetTime());

        //遷移コルーチンがあるなら停止する(重複して遷移コルーチンが走らないようにする)
        if (fadeCoroutine != null)
            player.StopCoroutine(fadeCoroutine);

        //遷移中のモーションミックス開始
        fadeCoroutine = player.StartCoroutine(crossFadeCoroutine(playParam.fadeDuration, playParam.waitFade));
    }

    /// <summary>
    /// AnimatorのCrossFade()のようなもの
    /// 参考
    /// http://tsubakit1.hateblo.jp/entry/2017/07/30/032008
    /// </summary>
    /// <param name="duration">次のAnimationに「完全に」遷移する時間</param>
    /// <param name="waitCrossFade">次のAnimationに「完全に」遷移してから、次のモーションの再生を開始する</param>
    /// <returns></returns>
    private IEnumerator crossFadeCoroutine(float duration, bool waitCrossFade = false)
    {
        isFinishedTrans = false;
        if (waitCrossFade)
            nowPlayable.SetSpeed(0f); //遷移先のモーション時間を止めておく

        // 指定時間でアニメーションをブレンド
        float waitTime = Time.time + duration;
        yield return new WaitWhile(() =>
        {
            var diff = waitTime - Time.time;
            if (diff <= 0)
            {
                mixer.SetInputWeight(1, 0);
                mixer.SetInputWeight(0, 1);
                return false;
            }
            else
            {
                var rate = Mathf.Clamp01(diff / duration);
                mixer.SetInputWeight(1, rate);
                mixer.SetInputWeight(0, 1 - rate);
                return true;
            }
        });

        if (waitCrossFade)
        {
            float play_speed = playParam.reverse ? -1f : 1f;
            nowPlayable.SetSpeed(play_speed); //遷移先のモーション再生時間を元に戻す
        }

        isFinishedTrans = true;
        fadeCoroutine = null;
    }
}
