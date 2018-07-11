/*************************************************************************
 * 
 * MIXT CONFIDENTIAL
 * ________________
 * 
 *  [2016] - [2018] Mixt Ltd
 *  All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is, and remains
 * the property of Mixt Ltd and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to Mixt Ltd
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Mixt Ltd.
 */

//Creator: Jonathan Adams
//Contributors: Jonathan Adams, Tristan Scott


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// Handles score, ammo, focus meter and time left. 
/// Checking or changing any of these stats should be done in here.
/// </summary>
public class MixtUIManager : MonoBehaviour, TextCountdownTimer.Events {

    [SerializeField]
    private FocusMeter focusMeter;

    [SerializeField]
    private GameObject mags;
    [SerializeField]
    private AmmunitionCounter ammunitionCounter;

    [SerializeField]
    private TextCountdownTimer timer;
    Gun gun;
    [SerializeField]
    private StartGame startGameButton;

    private static MixtUIManager instance;

    public static MixtUIManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.Find("MixtUIManager").GetComponent<MixtUIManager>();
            }

            return instance;
        }
    }

    private string SCORE_START_PART;

    public bool TimerRunning {

        get {
            return timer.IsRunning;
        }
    }

    public enum HitType {
        HEADSHOT = 2,
        BODYSHOT = 1
    }

    [SerializeField]
    private TMP_Text scoreLabel, feedBackLabel;
    private int score;
    private string feedBack;
    private void Awake() {
        gun = FindObjectOfType<Gun>();
    }
    // Use this for initialization
    void Start() {
        timer.addListener(this);
        SCORE_START_PART = "Hit Score: ";
        score = 0;
        
    }


    /// <summary>
    /// Hit points are multplied by a difficulty level.
    /// In this case the difficulty corresponds to the distance
    /// </summary>
    /// <param name="hittype"></param>
    /// <param name="multiplier"></param>
    public void notifyHit(HitType hittype, int multiplier) {
        score += (int)hittype * multiplier;
        scoreLabel.text = score.ToString(); // SCORE_START_PART + score;

        switch (hittype) {
            case HitType.HEADSHOT:
                feedBack = "Head Shot!";
                addFocus();
                break;

            case HitType.BODYSHOT:
                feedBack = "Body Shot!";
                break;
        }

        //stopping any previous tweens
        feedBackLabel.DOKill();

        feedBackLabel.text = feedBack;
        feedBackLabel.alpha = 1;
        feedBackLabel.rectTransform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f);

        feedBackLabel.DOFade(0.0f, 0.5f).SetDelay(0.5f);

    }

    public void reset() {

        if (MagsManager.instance != null) {
            Destroy(MagsManager.instance.gameObject);
        }
        Instantiate(mags, mags.transform.position, mags.transform.rotation);
        timer.reset();

        focusMeter.reset();

        ammunitionCounter.ClearMag();

        gun.reloading = true;
        gun.Reload();
        TargetManager.instance.deActivate();

        startGameButton.reset();
    }

    public void startTimer() {
        score = 0;
        scoreLabel.text = score.ToString();
        timer.start();
    }

    #region focus methods
    public bool canUseFocus() {
        /* don't want to enable focus if the game isn't running 
         * (also adding in a 5 second delay to ensure lifting the hand off 
         * the button doesn't use the focus straight away)
         */
        if (!timer.IsRunning || !(timer.Value < timer.StartValue - 5)) {
            return false;
        }

        return focusMeter.canUseFocus();
    }

    public void beginFocusDepletion() {
        focusMeter.beginDepletion();
    }

    public void endFocusDepletion() {
        focusMeter.endDepletion();
    }

    public void addFocus() {
        focusMeter.addFocus();
    }
    #endregion


    private IEnumerator test() {
        yield return new WaitForSeconds(1f);

        notifyHit(MixtUIManager.HitType.HEADSHOT, 2);

        yield return new WaitForSeconds(1f);

        notifyHit(MixtUIManager.HitType.BODYSHOT, 1);

        yield return new WaitForSeconds(1f);

        notifyHit(MixtUIManager.HitType.HEADSHOT, 2);

        yield return new WaitForSeconds(1f);

        notifyHit(MixtUIManager.HitType.HEADSHOT, 1);

        yield return new WaitForSeconds(1f);

    }

    public void onCountDownTimerComplete() {
        print("Timer Down");
        reset();
    }
}
