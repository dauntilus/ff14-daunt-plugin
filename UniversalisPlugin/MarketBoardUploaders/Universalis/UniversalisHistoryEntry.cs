using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dalamud.Game.Network.MarketBoardUploaders.Universalis
{
    class UniversalisHistoryEntry
    {
        [JsonProperty("hq")]
        public bool Hq { get; set; }

        [JsonProperty("pricePerUnit")]
        public uint PricePerUnit { get; set; }

        [JsonProperty("quantity")]
        public uint Quantity { get; set; }

        [JsonProperty("buyerName")]
        public string BuyerName { get; set; }

        [JsonProperty("onMannequin")]
        public bool OnMannequin { get; set; }

        [JsonProperty("sellerID")]
        public ulong SellerId { get; set; }

        [JsonProperty("buyerID")]
        public ulong BuyerId { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
