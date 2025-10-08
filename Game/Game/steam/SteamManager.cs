using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.view;

namespace Vexillum.steam
{
    public class SteamManager
    {
        public static bool Initialized;
        private static SteamAPIWarningMessageHook_t SteamAPIWarningMessageHook;
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Util.Debug(pchDebugText.ToString());
        }

        public static bool Initialize()
        {
            if (!Packsize.Test())
            {
                Util.Debug("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }
            if (!DllCheck.Test())
            {
                Util.Debug("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the 
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/documentation/drm#FAQ
                if (SteamAPI.RestartAppIfNecessary((AppId_t)349380))
                {
                    Vexillum.game.Exit();
                    return false;
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurence of it.
                Util.Debug("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                return false;
            }

            // Initialize the SteamAPI, if Init() returns false this can happen for many reasons.
            // Some examples include:
            // Steam Client is not running.
            // Launching from outside of steam without a steam_appid.txt file in place.
            // Running under a different OS User or Access level (for example running "as administrator")
            // Valve's documentation for this is located here:
            // https://partner.steamgames.com/documentation/getting_started
            // https://partner.steamgames.com/documentation/example // Under: Common Build Problems
            // https://partner.steamgames.com/documentation/bootstrap_stats // At the very bottom

            // If you're running into Init issues try running DbgView prior to launching to get the internal output from Steam.
            // http://technet.microsoft.com/en-us/sysinternals/bb896647.aspx
            Initialized = SteamAPI.Init();
            if (!Initialized)
            {
                Util.Debug("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
            }
            return Initialized;
        }

        public static Callback<T> CreateCallback<T>(Callback<T>.DispatchDelegate callback) {
            return Callback<T>.Create(callback);
        }

        public static SteamAuthTicket GetSessionTicket()
        {
            byte[] token = new byte[1024];
            uint tokenLength = 0;
            HAuthTicket ticket = SteamUser.GetAuthSessionTicket(token, token.Length, out tokenLength);
            return new SteamAuthTicket(token, (int) tokenLength, ticket);
        }

        public static string GetDisplayName(CSteamID steamId)
        {
            return steamId.ToString();
        }

        public static void Shutdown()
        {
            if (Initialized)
            {
                Initialized = false;
                SteamAPI.Shutdown();
            }
        }

        public static void Update()
        {
            if (Initialized)
                SteamAPI.RunCallbacks();
        }

        public class SteamAuthTicket
        {
            public byte[] token;
            public int tokenLength;
            public HAuthTicket ticket;

            public SteamAuthTicket(byte[] t, int tl, HAuthTicket ticket)
            {
                token = t;
                tokenLength = tl;
                this.ticket = ticket;
            }
        }
    }
}
