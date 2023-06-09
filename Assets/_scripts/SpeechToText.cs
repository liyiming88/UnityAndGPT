﻿using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAI
{
    public class SpeechToText : MonoBehaviour
    {
        public GameObject thinkDots;
        private Animator thinkAnimator;
        private static SpeechToText speechy;

        private Animator animator;
        public static SpeechToText Speechy
        {
            get { return speechy; }
        }
        public TMP_Text outputText;
        // PULLED OUT OF BUTTON CLICK
        SpeechRecognizer recognizer;
        SpeechConfig config;
        SpeechSynthesizer synthesizer;
        // if the whole message has been transcribed over
        bool isMessageOver = false;

        private object threadLocker = new object();
        private bool speechStarted = false; //checking to see if you've started listening for speech
        private string message;
        private bool ifAvatarTalking;
        private bool ifAvatarListening;
        private bool ifAvatarThinking;
        private bool isInit = true;

        // 用来识别整句输出，并且输出完整文本
        private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                isMessageOver = true;
                
            }
        }
        
        private void StartToThink(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
               if (isInit) {
                    isInit = false;
                    Debug.Log("Start to think");
                    ifAvatarThinking = true;
                }
                
            }
        }

        // 当文字转语音开始时，开始记录口型与表情
        private void StartViseme(object sender, SpeechSynthesisVisemeEventArgs e)
        {
            // to do, 这里参考azure viseme文档
            // https://learn.microsoft.com/zh-cn/azure/cognitive-services/speech-service/how-to-speech-synthesis-viseme?pivots=programming-language-csharp&tabs=3dblendshapes
            // ，收集口型和表情
        }

        // 开始录音
        public async void Recording()
        {
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
            }
        }

        public async void KillRecord()
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }

        // 文字转语音
        public async void SynthesizeAudioAsync(string text)
        {
            await synthesizer.SpeakTextAsync(text);
        }

        // avatar is talking
        private async void StopRecord(object sender, SpeechSynthesisEventArgs e)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
                ifAvatarTalking = true;
                ifAvatarListening = false;
                ifAvatarThinking = false;
                isInit = true;
            }
        }

        // player is talking
        private async void RestartRecord(object sender, SpeechSynthesisEventArgs e)
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
                ifAvatarTalking = false;
                ifAvatarListening = true;
                ifAvatarThinking = false;
            }
        }


        void Start()
        {
            // 认证Azure speech sdk的权限
            config = SpeechConfig.FromSubscription("e512bf00442d427e9158b0e381563240", "eastus");
            // 这两项是配置文字转语音的语言和人物
            config.SpeechSynthesisLanguage = "en-US";
            config.SpeechSynthesisVoiceName = "en-US-JennyNeural";
            // 新建合成语音转换器
            synthesizer = new SpeechSynthesizer(config);
            synthesizer.SynthesisStarted += StopRecord;
            synthesizer.SynthesisCompleted += RestartRecord;
            // 新建语音识别器
            recognizer = new SpeechRecognizer(config);
            // 订阅事件：当用户语音完整输出后，调用Handler
            recognizer.Recognized += RecognizedHandler;
            recognizer.Recognizing += StartToThink;
            // // 当文字转语音开始调用时，也开始调用该方法，输出口型数据
            synthesizer.VisemeReceived += StartViseme;
            string[] aaa = Microphone.devices;
            animator = gameObject.GetComponent<Animator>();
            thinkAnimator = thinkDots.GetComponent<Animator>();

        }

        void Update()
        {

            lock (threadLocker)
            {
                if (outputText != null)
                {
                    outputText.text = message;
                }
            }

            if (isMessageOver)
            {
                isMessageOver = false;
                ChatGPTSTT.Chatty.CallChatGPT(message);
            }

            if (ifAvatarTalking)
            {
                ifAvatarTalking = false;
                animator.SetTrigger("talk");
                thinkAnimator.SetBool("isThink", false);

            }
            if (ifAvatarListening)
            {
                ifAvatarListening = false;
                animator.SetTrigger("listen");
                thinkAnimator.SetBool("isThink", false);
            }
            if (ifAvatarThinking)
            {
                ifAvatarThinking = false;
                thinkAnimator.SetBool("isThink", true);
            }
        }

        void Awake()
        {
            if (speechy != null && speechy != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                speechy = this;
                DontDestroyOnLoad(gameObject);
            }
        }


        void OnDestroy()
        {
            Debug.Log("");
        }
    }
}
