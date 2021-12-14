/********************************************************************************************************************************************
 * Copyright (C) 2016 Pieter-Uys Fourie
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as 
 * published by the Free Software Foundation, either version 3 of the License, or any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty 
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with this program. If not, see 
 * http://www.gnu.org/licenses/.
 */

using System.Collections.Generic;
using KodiRPC.Responses.Types.Video.Details;
using KodiRPC.RPC.RequestResponse.Params;
using Newtonsoft.Json;

namespace KodiRPC.Responses.VideoLibrary
{
    public class GetActivePlayersResponse : List<ActivePlayer>
    {
        //[JsonProperty(PropertyName = "result")]
        //public List<Player> Result { get; set; }
    }

    public class ActivePlayer
    {
        [JsonProperty(PropertyName = "playerid")]
        public int PlayerId { get; set; }

        [JsonProperty(PropertyName = "playertype")]
        public string PlayerType { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    public class PlayerSpeed
    {
        [JsonProperty(PropertyName = "speed")]
        public int Speed { get; set; }
    }
}