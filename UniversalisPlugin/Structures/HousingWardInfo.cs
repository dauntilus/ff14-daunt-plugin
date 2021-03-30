using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UniversalisPlugin;

namespace Dalamud.Game.Network.Structures
{
    public class HousingWardInfo
    {

        public short LandId;
        public int WardNumber;
        public short TerritoryTypeId;
        public short WorldId;
        public List<HouseInfoEntry> HouseListings;

        public class HouseInfoEntry
        {
            public uint HousePrice;
            public uint fluff;
            public string EstateOwnerName;
        }


        public int ListingIndexEnd;
        public int ListingIndexStart;
        public int RequestId;

        public static HousingWardInfo Read(byte[] message, UniversalisPluginControl plugin)
        {
            var output = new HousingWardInfo();

            using (var stream = new MemoryStream(message)) {
                using (var reader = new BinaryReader(stream)) {
                    output.LandId = reader.ReadInt16();
                    output.WardNumber = 1+reader.ReadInt16();
                    output.TerritoryTypeId = reader.ReadInt16();
                    output.WorldId = reader.ReadInt16();

                    output.HouseListings = new List<HouseInfoEntry>();

                    for (var i = 0; i < 60; i++)
                    {
                        var listingEntry = new HouseInfoEntry();


                        listingEntry.HousePrice = reader.ReadUInt32();
                        listingEntry.fluff = reader.ReadUInt32();
                        listingEntry.EstateOwnerName = Encoding.UTF8.GetString(reader.ReadBytes(32)).TrimEnd(new[] { '\u0000' });
                        output.HouseListings.Add(listingEntry);

                        if(listingEntry.EstateOwnerName.Length == 0)
                        {
                            plugin.Log($"House found in ward {output.WardNumber.ToString()},  House {(i + 1).ToString()} Price {listingEntry.HousePrice.ToString()}"); ;
                        }
                    }

                }
            }

            return output;
        }
    }
}
