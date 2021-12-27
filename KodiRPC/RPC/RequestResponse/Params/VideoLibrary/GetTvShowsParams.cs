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

using Newtonsoft.Json;
using System.Collections.Generic;

namespace KodiRPC.RPC.RequestResponse.Params.VideoLibrary
{
    public class GetTvShowsParams : KodiProperties
    {
        [JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
        public Filter Filter { get; set; }

        [JsonProperty("limits", NullValueHandling = NullValueHandling.Ignore)]
        public Limits Limits { get; set; }

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public Sort Sort { get; set; }
    }

    public class ActivateWindowParams : KodiProperties
    {
        [JsonProperty("window", Required = Required.Always)]
        public string Window { get; set; }
    }

    public class ExecuteAddonParams : KodiProperties
    {
        [JsonProperty("addonid", Required = Required.Always)]
        public string AddonId { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Params { get; set; }

        [JsonProperty("wait")]
        public bool Wait { get; set; } = false;
    }

    public class InputHomeParams : KodiProperties
    {
    }

    public class QuitApplicationParams : KodiProperties
    {
    }

    public class GetActivePlayersParams : KodiProperties
    {
    }

    public class StopPlayerParams : KodiProperties
    {
        [JsonProperty("playerid", Required = Required.Always)]
        public int PlayerId { get; set; }
    }

    public class PlayerGotoParams : KodiProperties
    {
        [JsonProperty("playerid", Required = Required.Always)]
        public int PlayerId { get; set; }

        [JsonProperty("to", Required = Required.Always)]
        public string To { get; set; }
    }

    public class TogglePlayPauseParams : KodiProperties
    {
        [JsonProperty("playerid", Required = Required.Always)]
        public int PlayerId { get; set; }
    }

    public class SetMuteParams : KodiProperties
    {
        [JsonProperty("mute", Required = Required.Always)]
        public string Mute { get; set; } = "toggle";
    }

    public class SetVolumeParams : KodiProperties
    {
        [JsonProperty("volume", Required = Required.Always)]
        public int Volume { get; set; }
    }

    public class EjectOpticalDriveParams : KodiProperties
    {
    }    
}