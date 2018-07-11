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
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class TextCountdownTimer : MonoBehaviour {


    public Color highLightColor;

    /// <summary>
    /// The value to count down from
    /// </summary>
    [SerializeField]
    private float startVal;

    [SerializeField]
    private TMP_Text timerText;
    private float currentTime;
    private bool isRunning;

    private bool punchOneComplete, punchTwoComplete, punchThreeComplete;

    private List<Events> listeners = new List<Events>();

    public bool IsRunning {
        get {
            return isRunning;
        }
    }

    public float Value {
        get {
            return currentTime;
        }
    }

    public float StartValue {
        get {
            return startVal;
        }
    }

    // Use this for initialization
    void Start() {

        //listeners = new List<Events>();

        currentTime = startVal;
        isRunning = false;

    }

    // Update is called once per frame
    void Update() {

        /* not interested in updating Ui etc if the clock isn't running*/
        if (!isRunning) {
            return;
        }

        /*we have reached 0 so need to stop the clock and let anyone who is interested know*/
        if ((int)currentTime <= 0) {
            stop();
            timerText.text = "0";

            foreach (Events listener in listeners) {
                listener.onCountDownTimerComplete();
            }

            return;
        }

        /*counting down */
        currentTime -= Time.deltaTime;

        /*updating the UI, adding in preceeeding 0 if counter is below 1 */
        //  timerText.text = currentTime <= 1? "0" +  currentTime.ToString("#.00").Replace('.',':') : currentTime.ToString("#.00").Replace('.', ':');

        timerText.text = ((int)currentTime).ToString();
        /* punching at every second after 3 */
        if ((int)currentTime == 3 && !punchOneComplete) {
            timerText.color = highLightColor;

            timerText.transform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f);
            punchOneComplete = true;
        }


        /* punching at every second after 2 */
        if ((int)currentTime == 2 && !punchTwoComplete) {
            timerText.transform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f);
            punchTwoComplete = true;
        }

        /* punching at every second after 1 */
        if ((int)currentTime == 1 && !punchThreeComplete) {
            timerText.transform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f);
            punchThreeComplete = true;
        }


    }


    public void start() {
        isRunning = true;
    }

    public void stop() {
        isRunning = false;
        timerText.color = Color.white;

    }

    public void reset() {
        currentTime = startVal;
        timerText.color = Color.white;

    }

    #region listener pattern
    public void addListener(Events listener) {
        if (!listeners.Contains(listener)) {
            listeners.Add(listener);
        }
    }

    public void removeListener(Events listener) {
        if (listeners.Contains(listener)) {
            listeners.Remove(listener);
        }
    }

    public interface Events {
        void onCountDownTimerComplete();
    }

    #endregion
}
