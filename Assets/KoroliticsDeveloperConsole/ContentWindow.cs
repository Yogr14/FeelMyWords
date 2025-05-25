using System.Net.Http;
using UnityEngine;

namespace Services.Korolitics.DeveloperConsole
{
    public abstract class ContentWindow
    {
        protected HttpClient HttpClient;
        protected Config Config;
        public bool AuthentificationPassed { get; protected set; }

        public ContentWindow(HttpClient httpClient, Config config, bool authentificationPassed)
        {
            this.HttpClient = httpClient;
            Config = config;
            this.AuthentificationPassed = authentificationPassed;
        }

        internal abstract void Draw();
    }
}
