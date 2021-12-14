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

using System.Configuration;
using KodiRPC.Responses.Files;
using KodiRPC.Responses.VideoLibrary;
using KodiRPC.RPC.Connector;
using KodiRPC.RPC.RequestResponse;
using KodiRPC.RPC.RequestResponse.Params.Files;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;

namespace KodiRPC.Services
{
    public class KodiService : IKodiService
    {
        private readonly RpcConnector _rpcConnector;

        public readonly string ApiVersion = "v6";

        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5156";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public KodiService()
        {
            _rpcConnector = new RpcConnector(this);
        }

        #region JSONRPC

        public JsonRpcResponse<string> Ping()
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.Ping, new object(), timeout:2000);
        }

        #endregion

        #region VideoLibrary

        #region Shows
        public JsonRpcResponse<string> Clean(CleanParams parameters, string requestId = "VideoLibrary.Clean")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.Clean, parameters, requestId);
        }

        public JsonRpcResponse<string> Scan(ScanParams parameters, string requestId = "VideoLibrary.Scan")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.Scan, parameters, requestId);
        }

        public JsonRpcResponse<string> ActivateWindow(ActivateWindowParams parameters, string requestId = "VideoLibrary.Scan")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.ActivateWindow, parameters, requestId);
        }

        public JsonRpcResponse<string> ActivateWindow(string window)
        {
            return ActivateWindow(new ActivateWindowParams() { Window = window });
        }

        public JsonRpcResponse<string> EjectOpticalDrive(EjectOpticalDriveParams parameters, string requestId = KodiMethods.EjectOpticalDrive)
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.EjectOpticalDrive, parameters, requestId);
        }       

        public JsonRpcResponse<string> ExecuteAddon(ExecuteAddonParams parameters, string requestId = "Addons.ExecuteAddon")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.ExecuteAddon, parameters, requestId);
        }

        public JsonRpcResponse<GetActivePlayersResponse> GetActivePlayers(GetActivePlayersParams parameters, string requestId = KodiMethods.GetActivePlayers)
        {
            return _rpcConnector.MakeRequest<GetActivePlayersResponse>(KodiMethods.GetActivePlayers, parameters, requestId);
        }

        public JsonRpcResponse<string> StopPlayer(StopPlayerParams parameters, string requestId = KodiMethods.StopPlayer)
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.StopPlayer, parameters, requestId);
        }

        public JsonRpcResponse<PlayerSpeed> TogglePlayerPlayPause(TogglePlayPauseParams parameters, string requestId = KodiMethods.PlayPause)
        {
            return _rpcConnector.MakeRequest<PlayerSpeed>(KodiMethods.PlayPause, parameters, requestId);
        }

        public JsonRpcResponse<string> SetMute(SetMuteParams parameters, string requestId = KodiMethods.SetMute)
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.SetMute, parameters, requestId);
        }
        
        public JsonRpcResponse<string> InputHome(string requestId = "Input.Home")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.InputHome, new InputHomeParams(), requestId);
        }

        public JsonRpcResponse<string> InputBack(string requestId = "Input.Back")
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.InputBack, new InputHomeParams(), requestId);
        }

        public JsonRpcResponse<string> QuitApplication(string requestId = KodiMethods.QuitApplication)
        {
            return _rpcConnector.MakeRequest<string>(KodiMethods.QuitApplication, new QuitApplicationParams(), requestId);
        }

        public JsonRpcResponse<GetTvShowsResponse> GetTvShows(GetTvShowsParams parameters, string requestId = "GetTvShowsResponse")
        {
            return _rpcConnector.MakeRequest<GetTvShowsResponse>(KodiMethods.GetTvShows, parameters, requestId);
        }

        public JsonRpcResponse<GetTvShowDetailsResponse> GetTvShowDetails(GetTvShowDetailsParams parameters, string requestId="GetTvShowDetailsResponse")
        {
            return _rpcConnector.MakeRequest<GetTvShowDetailsResponse>(KodiMethods.GetTvShowDetails, parameters, requestId);
        }

        public JsonRpcResponse<GetSeasonsResponse> GetSeasons(GetSeasonsParams parameters = null, string requestId = "GetSeasonsResponse")
        {
            return _rpcConnector.MakeRequest<GetSeasonsResponse>(KodiMethods.GetSeasons, parameters, requestId);
        }

        public JsonRpcResponse<GetEpisodesResponse> GetEpisodes(GetEpisodesParams parameters, string requestId = "GetEpisodesResponse")
        {
            return _rpcConnector.MakeRequest<GetEpisodesResponse>(KodiMethods.GetEpisodes, parameters, requestId);
        }

        public JsonRpcResponse<GetEpisodeDetailsResponse> GetEpisodeDetails(GetEpisodeDetailsParams parameters, string requestId="GetTvShowDetailsResponse")
        {
            return _rpcConnector.MakeRequest<GetEpisodeDetailsResponse>(KodiMethods.GetEpisodeDetails, parameters, requestId);
        }

        public JsonRpcResponse<GetRecentlyAddedEpisodesResponse> GetRecentlyAddedEpisodes(GetRecentlyAddedEpisodesParams parameters = null,
            string requestId = "GetRecentlyAddedEpisodesResponse")
        {
            return _rpcConnector.MakeRequest<GetRecentlyAddedEpisodesResponse>(KodiMethods.GetRecentlyAddedEpisodes, parameters, requestId);
        }
        #endregion

        #region Movies
        public JsonRpcResponse<GetMoviesResponse> GetMovies(GetMoviesParams parameters, string requestId = "GetMoviesResponse")
        {
            return _rpcConnector.MakeRequest<GetMoviesResponse>(KodiMethods.GetMovies, parameters, requestId);
        }

        public JsonRpcResponse<GetMovieDetailsResponse> GetMovieDetails(GetMovieDetailsParams parameters, string requestId="GetMovieDetailsResponse")
        {
            return _rpcConnector.MakeRequest<GetMovieDetailsResponse>(KodiMethods.GetMovieDetails, parameters, requestId);
        }

        public JsonRpcResponse<GetRecentlyAddedMoviesResponse> GetRecentlyAddedMovies(GetRecentlyAddedMoviesParams parameters = null,
            string requestId = "GetRecentlyAddedMoviesResponse")
        {
            return _rpcConnector.MakeRequest<GetRecentlyAddedMoviesResponse>(KodiMethods.GetRecentlyAddedMovies, parameters, requestId);
        }
        #endregion

        #endregion

        #region Files
        public JsonRpcResponse<GetFileDetailsResponse> GetFileDetails(GetFileDetailsParams parameters, string requestId = "GetFileDetails")
        {
            return _rpcConnector.MakeRequest<GetFileDetailsResponse>(KodiMethods.GetFileDetails, parameters, requestId);
        }

        public JsonRpcResponse<GetDirectoryResponse> GetDirectory(GetDirectoryParams parameters, string requestId = "GetDirectory")
        {
            return _rpcConnector.MakeRequest<GetDirectoryResponse>(KodiMethods.GetDirectory, parameters, requestId);
        }

        public JsonRpcResponse<PrepareDownloadResponse> PrepareDownload(PrepareDownloadParams parameters, string requestId = "PrepareDownload")
        {
            return _rpcConnector.MakeRequest<PrepareDownloadResponse>(KodiMethods.PrepareDownload, parameters, requestId);
        }
        #endregion
    }
}
