using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace UniversalisPlugin
{
    public class Definitions
    {
        public short PlayerSetup = 0x18F;
        public short MarketBoardItemRequestStart = 192;
        public short MarketBoardOfferings = 363;
        public short MarketBoardHistory = 451;

        //private static readonly Uri DefinitionStoreUrl = new Uri("https://ffxivmb.com/definitions.json");

        //public static string GetJson() => JsonConvert.SerializeObject(new Definitions());

        //public static Definitions Get()
        //{
        //    using (WebClient client = new WebClient())
        //    {
        //        try
        //        {
        //            var definitionJson = client.DownloadString(DefinitionStoreUrl);
        //            var deserializedDefinition = JsonConvert.DeserializeObject<Definitions>(definitionJson);

        //            return deserializedDefinition;
        //        }
        //        catch (WebException exc)
        //        {
        //            throw new Exception("Could not get definitions for FFXIVMB ACT Plugin.", exc);
        //        }
        //    }
        //}
    }
}
