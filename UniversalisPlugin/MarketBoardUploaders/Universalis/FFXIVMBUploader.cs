using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dalamud.Game.Network;
using Dalamud.Game.Network.MarketBoardUploaders;
using Dalamud.Game.Network.MarketBoardUploaders.Universalis;
using Newtonsoft.Json;
using UniversalisPlugin;

namespace UniversalisPlugin.MarketBoardUploaders.Universalis
{
    class FFXIVMBUploader : IMarketBoardUploader
    {

        private UniversalisPlugin.UniversalisPluginControl dalamud;
        public string server_addr;

        public FFXIVMBUploader(UniversalisPlugin.UniversalisPluginControl dalamud, string server_addr)
        {
            this.dalamud = dalamud;
            this.server_addr = server_addr;
        }

        public void Upload(MarketBoardItemRequest request)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                this.dalamud.Log("Starting FFXIVMB upload.");
                var uploader = this.dalamud.LocalContentId;

                var listingsRequestObject = new UniversalisItemListingsUploadRequest();
                listingsRequestObject.WorldId = (int)this.dalamud.CurrentWorldId;
                listingsRequestObject.UploaderId = uploader;
                listingsRequestObject.ItemId = request.CatalogId;
                listingsRequestObject.Expected = request.AmountToArrive;

                listingsRequestObject.Listings = new List<UniversalisItemListingsEntry>();
                foreach (var marketBoardItemListing in request.Listings)
                {
                    var universalisListing = new UniversalisItemListingsEntry
                    {
                        Hq = marketBoardItemListing.IsHq,
                        SellerId = marketBoardItemListing.RetainerOwnerId,
                        RetainerName = marketBoardItemListing.RetainerName,
                        RetainerId = marketBoardItemListing.RetainerId,
                        CreatorId = marketBoardItemListing.ArtisanId,
                        CreatorName = marketBoardItemListing.PlayerName,
                        OnMannequin = marketBoardItemListing.OnMannequin,
                        LastReviewTime = ((DateTimeOffset)marketBoardItemListing.LastReviewTime).ToUnixTimeSeconds(),
                        PricePerUnit = marketBoardItemListing.PricePerUnit,
                        Quantity = marketBoardItemListing.ItemQuantity,
                        RetainerCity = marketBoardItemListing.RetainerCityId
                    };


                    universalisListing.Materia = new List<UniversalisItemMateria>();
                    foreach (var itemMateria in marketBoardItemListing.Materia)
                        universalisListing.Materia.Add(new UniversalisItemMateria
                        {
                            MateriaId = itemMateria.MateriaId,
                            SlotId = itemMateria.Index
                        });

                    listingsRequestObject.Listings.Add(universalisListing);
                }

                listingsRequestObject.History = new List<UniversalisHistoryEntry>();

                foreach (var marketBoardHistoryListing in request.History)
                {
                    listingsRequestObject.History.Add(new UniversalisHistoryEntry
                    {
                        BuyerName = marketBoardHistoryListing.BuyerName,
                        Hq = marketBoardHistoryListing.IsHq,
                        OnMannequin = marketBoardHistoryListing.OnMannequin,
                        PricePerUnit = marketBoardHistoryListing.SalePrice,
                        Quantity = marketBoardHistoryListing.Quantity,
                        Timestamp = ((DateTimeOffset)marketBoardHistoryListing.PurchaseTime).ToUnixTimeSeconds()
                    });
                }


                var upload = JsonConvert.SerializeObject(listingsRequestObject);
                client.UploadString($"http://{this.server_addr}/uploadPrice", "POST", upload);
            }
        }

      
    }
}
