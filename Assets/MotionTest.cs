using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotionTest : MonoBehaviour
{
    public Text lowerText;
    public Text upperText;

    public Toggle overrideToggle;
    public Toggle loopToggle;
    public Toggle reverseToggle;
    public Toggle waitToggle;
    public Toggle syncToggle;

    public List<AnimationClip> lowerClips = new List<AnimationClip>();
    public List<AnimationClip> upperClips = new List<AnimationClip>();
    private int lowerCounter = 0;
    private int upperCounter = 0;

    private MotionPlayer motionPlayer; //GetComponent()による自動取得

    void Start()
    {
        motionPlayer = GetComponent<MotionPlayer>();

        playLower();
        playUpper();
    }

    public void play(AnimationClip clip, int layer)
    {
        MotionPlayerParam param = new MotionPlayerParam(
            layer_: layer,
            loop_: loopToggle.isOn,
            reverse_: reverseToggle.isOn,
            fadeDuration_: 0.5f,
            waitFade_: waitToggle.isOn,
            syncTrans_: syncToggle.isOn);

        motionPlayer.play(clip, param);

        if (overrideToggle.isOn)
            motionPlayer.setLayerEnabled(1, true);
        else
            motionPlayer.setLayerEnabled(1, false);
    }

    public void playLower()
    {
        play(lowerClips[lowerCounter], 0);
        lowerText.text = lowerClips[lowerCounter].name;

        lowerCounter++;
        if (lowerCounter >= lowerClips.Count)
            lowerCounter = 0;
    }

    public void playUpper()
    {
        play(upperClips[upperCounter], 1);
        upperText.text = upperClips[upperCounter].name;
        
        upperCounter++;
        if (upperCounter >= upperClips.Count)
            upperCounter = 0;
    }

    public void pause()
    {
        
    }
}
