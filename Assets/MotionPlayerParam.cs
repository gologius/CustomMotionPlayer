using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPlayerParam
{
    public int layer = 0;

    //モーションをループさせる
    public bool loop = false;

    //モーションを逆再生する
    public bool reverse = false;

    //モーション遷移にかかる時間[秒]
    public float fadeDuration = 0.2f;

    //遷移時に、遷移前モーションと遷移後モーションをミックスせずに再生する
    public bool waitFade = false;

    //遷移前モーションの再生時間と、遷移後のモーションの再生時間を同期させる
    public bool syncTrans = false;

    public MotionPlayerParam()
    {
    }

    public MotionPlayerParam(int layer_, bool loop_ = false, bool reverse_ = false, float fadeDuration_ = 0.2f, bool waitFade_ = false, bool syncTrans_ = false)
    {
        layer = layer_;
        loop = loop_;
        reverse = reverse_;
        fadeDuration = fadeDuration_;
        waitFade = waitFade_;
        syncTrans = syncTrans_;
    }
}
