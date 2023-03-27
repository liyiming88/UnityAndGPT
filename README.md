1. Clone the project.
2. OpenAI authentication-> config your local variable :
   Create a file called auth.json in the .openai folder' under your PC user folder and paste your openai key and organizaiton.
   ```
   {
    "api_key": "sk-...W6yi",
    "organization": "org-...L7W"
    }
    ```
    option2 (You cannot use this file since maybe you device is a android phone or VR), so you need to write the apikey in code.
      Open your project, goto Package file OpenAIApi.cs and locate to public OpenApi methor: and replace the content with these code, then replace your apikey and organizition:
      ```
      public OpenAIApi(string apiKey = null, string organization = null)
        {
            if (apiKey != null)
            {
                configuration = new Configuration(apiKey, organization);
            }
            else {
                // Make this change want to try if 
                configuration = new Configuration("sk-GtEKVgCxxxxxxxxxxxxxxxxxxxxx", "org-H8pZxPbhxxxxxxxxxx");
            }
        }
        
3. Azure speech service authentication -> go to SpeechToText.cs file, in void Start method, replace your Azure Speech Studio service region and resouce key.
   ```
   config = SpeechConfig.FromSubscription("e512bf00xxxxxxxxxxxxxxxxx", "easxxx");
4. Scenes:
   STTScene is combine STT and ChatGPT, ChatGPT sample is just ChatGPT usecase
