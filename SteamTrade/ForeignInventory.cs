using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SteamTrade
{
    public class ForeignInventory
    {
        private readonly dynamic rawJson;

        public ForeignInventory (dynamic rawJson)
        {
            this.rawJson = rawJson;

            if (rawJson.success == "true")
            {
                InventoryValid = true;
            }
        }

        public bool InventoryValid
        {
            get; private set;
        }

        public uint GetClassIdForItemId(ulong itemId)
        {
            string i = itemId.ToString(CultureInfo.InvariantCulture);

            try
            {
                return rawJson.rgInventory[i].classid;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;    
            }
        }

        public ulong GetInstanceIdForItemId(ulong itemId)
        {
            string i = itemId.ToString(CultureInfo.InvariantCulture);

            try
            {
                return rawJson.rgInventory[i].instanceid;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }


        public ushort GetDefIndex(ulong itemId)
        {
            uint classId = GetClassIdForItemId(itemId);
            ulong iid = GetInstanceIdForItemId(itemId);

            string index = classId + "_" + iid;

            string r;

            try
            {
                r = rawJson.rgDescriptions[index].app_data.def_index;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }

            return ushort.Parse(r);
        }
    }
}
