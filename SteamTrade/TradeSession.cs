using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using SteamKit2;

namespace SteamTrade
{
    /// <summary>
    /// This class handles the web-based interaction for Steam trades.
    /// </summary>
    public partial class Trade
    {
        static string SteamCommunityDomain = "steamcommunity.com";
        static string SteamTradeUrl = "http://steamcommunity.com/trade/{0}/";

        string sessionId;
        string sessionIdEsc;
        string baseTradeURL;
        string steamLogin;
        CookieContainer cookies;
        

        internal int LogPos { get; set; }

        internal int Version { get; set; }

        StatusObj GetStatus ()
        {
            var data = new NameValueCollection ();

            data.Add ("sessionid", sessionIdEsc);
            data.Add ("logpos", "" + LogPos);
            data.Add ("version", "" + Version);
            
            string response = Fetch (baseTradeURL + "tradestatus", "POST", data);
            return JsonConvert.DeserializeObject<StatusObj>(response);
        }


        /// <summary>
        /// Gets the foriegn inventory.
        /// </summary>
        /// <param name="otherId">The other id.</param>
        /// <param name="contextId">The current trade context id.</param>
        /// <returns>A dynamic JSON object.</returns>
        dynamic GetForiegnInventory(SteamID otherId, int contextId )
        {
            var data = new NameValueCollection();

            data.Add("sessionid", sessionIdEsc);
            data.Add("steamid", otherId.ConvertToUInt64().ToString());
            data.Add("appid", "440"); // <---------- not portable.
            data.Add("contextid", contextId.ToString());

            try
            {
                string response = Fetch(baseTradeURL + "foreigninventory", "POST", data);
                return JsonConvert.DeserializeObject(response);
            }
            catch (Exception)
            {
                return JsonConvert.DeserializeObject("{\"success\":\"false\"}");
            }
        }

        #region Trade Web command methods

        /// <summary>
        /// Sends a message to the user over the trade chat.
        /// </summary>
        bool SendMessageWebCmd (string msg)
        {
            var data = new NameValueCollection ();
            data.Add ("sessionid", sessionIdEsc);
            data.Add ("message", msg);
            data.Add ("logpos", "" + LogPos);
            data.Add ("version", "" + Version);

            string result = Fetch (baseTradeURL + "chat", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(result);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Adds a specified itom by its itemid.  Since each itemid is
        /// unique to each item, you'd first have to find the item, or
        /// use AddItemByDefindex instead.
        /// </summary>
        /// <returns>
        /// Returns false if the item doesn't exist in the Bot's inventory,
        /// and returns true if it appears the item was added.
        /// </returns>
        bool AddItemWebCmd (ulong itemid, int slot)
        {
            var data = new NameValueCollection ();

            data.Add ("sessionid", sessionIdEsc);
            data.Add ("appid", "440");
            data.Add ("contextid", "2");
            data.Add ("itemid", "" + itemid);
            data.Add ("slot", "" + slot);

            string result = Fetch(baseTradeURL + "additem", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(result);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Removes an item by its itemid.  Read AddItem about itemids.
        /// Returns false if the item isn't in the offered items, or
        /// true if it appears it succeeded.
        /// </summary>
        bool RemoveItemWebCmd (ulong itemid, int slot)
        {
            var data = new NameValueCollection ();

            data.Add ("sessionid", sessionIdEsc);
            data.Add ("appid", "440");
            data.Add ("contextid", "2");
            data.Add ("itemid", "" + itemid);
            data.Add ("slot", "" + slot);

            string result = Fetch (baseTradeURL + "removeitem", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(result);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Sets the bot to a ready status.
        /// </summary>
        bool SetReadyWebCmd (bool ready)
        {
            var data = new NameValueCollection ();
            data.Add ("sessionid", sessionIdEsc);
            data.Add ("ready", ready ? "true" : "false");
            data.Add ("version", "" + Version);
            
            string result = Fetch (baseTradeURL + "toggleready", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(result);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Accepts the trade from the user.  Returns a deserialized
        /// JSON object.
        /// </summary>
        bool AcceptTradeWebCmd ()
        {
            var data = new NameValueCollection ();

            data.Add ("sessionid", sessionIdEsc);
            data.Add ("version", "" + Version);

            string response = Fetch (baseTradeURL + "confirm", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(response);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Cancel the trade.  This calls the OnClose handler, as well.
        /// </summary>
        bool CancelTradeWebCmd ()
        {
            var data = new NameValueCollection ();

            data.Add ("sessionid", sessionIdEsc);

            string result = Fetch (baseTradeURL + "cancel", "POST", data);

            dynamic json = JsonConvert.DeserializeObject(result);

            if (json == null || json.success != "true")
            {
                return false;
            }

            return true;
        }

        #endregion Trade Web command methods
        
        string Fetch (string url, string method, NameValueCollection data = null)
        {
            return SteamWeb.Fetch (url, method, OtherSID, data, cookies);
        }

        void Init()
        {
            sessionIdEsc = Uri.UnescapeDataString(sessionId);

            Version = 1;

            cookies = new CookieContainer();
            cookies.Add (new Cookie ("sessionid", sessionId, String.Empty, SteamCommunityDomain));
            cookies.Add (new Cookie ("steamLogin", steamLogin, String.Empty, SteamCommunityDomain));

            cookies.Add(new Cookie("bCompletedTradeTutorial", "true", String.Empty, SteamCommunityDomain));
            cookies.Add(new Cookie("strTradeLastInventoryContext", "440_2", String.Empty, SteamCommunityDomain));
            cookies.Add(new Cookie("recentlyVisitedAppHubs", "440", String.Empty, SteamCommunityDomain));
            //cookies.Add(new Cookie("strInventoryLastContext", "2", String.Empty, SteamCommunityDomain));
            cookies.Add(new Cookie("Steam_Language", "english", String.Empty, SteamCommunityDomain));
            cookies.Add(new Cookie("fakeCC", "US", String.Empty, SteamCommunityDomain));
            cookies.Add(new Cookie("timezoneOffset", HttpUtility.UrlEncode("-14400,0"), String.Empty, SteamCommunityDomain));

            baseTradeURL = String.Format (SteamTradeUrl, OtherSID.ConvertToUInt64 ());

            var response = SteamWeb.Request(baseTradeURL, "GET", cookies: cookies);

            //cookies.Add(response.Cookies);
        }

        public class StatusObj
        {
            public string error { get; set; }
            
            public bool newversion { get; set; }
            
            public bool success { get; set; }
            
            public long trade_status { get; set; }
            
            public int version { get; set; }
            
            public int logpos { get; set; }
            
            public TradeUserObj me { get; set; }
            
            public TradeUserObj them { get; set; }
            
            public TradeEvent[] events { get; set; }
        }

        public class TradeEvent : IEquatable<TradeEvent>
        {
            public string steamid { get; set; }
            
            public int action { get; set; }
            
            public ulong timestamp { get; set; }
            
            public int appid { get; set; }
            
            public string text { get; set; }
            
            public int contextid { get; set; }
            
            public ulong assetid { get; set; }

            /// <summary>
            /// Determins if the TradeEvent is equal to another.
            /// </summary>
            /// <param name="other">TradeEvent to compare to</param>
            /// <returns>True if equal, false if not</returns>
            public bool Equals(TradeEvent other)
            {
                if (this.steamid == other.steamid && this.action == other.action
                    && this.timestamp == other.timestamp && this.appid == other.appid
                    && this.text == other.text && this.contextid == other.contextid
                    && this.assetid == other.assetid)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        public class TradeUserObj
        {
            public int ready { get; set; }
            
            public int confirmed { get; set; }
            
            public int sec_since_touch { get; set; }
        }

        public enum TradeEventType : int
        {
            ItemAdded = 0,
            ItemRemoved = 1,
            UserSetReady = 2,
            UserSetUnReady = 3,
            UserAccept = 4,
            UserChat = 7
        }
    }


}

