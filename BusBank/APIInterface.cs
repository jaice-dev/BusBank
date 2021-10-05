using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NLog.Fluent;
using RestSharp;

namespace BusBank
{
    public static class APIInterface
    {
        //Logger
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        static RestClient tflclient = new RestClient("https://api.tfl.gov.uk/StopPoint");

        public static float[] GetLongAndLat(string postcode)
        {
            IRestResponse<PostcodeResponse> postcodeResponse;
            
            try
            {
                var postcodeclient = new RestClient("http://api.postcodes.io/postcodes");
                var postcoderequest = new RestRequest($"/{postcode}");
                postcodeResponse = postcodeclient.Get<PostcodeResponse>(postcoderequest);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise postcode response from request.");
                throw;
            }
            
            if (postcodeResponse.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'http://api.postcodes.io': expected '200', received {postcodeResponse.StatusCode}");
            }
            
            var longitude = postcodeResponse.Data.result.longitude;
            var latitude = postcodeResponse.Data.result.latitude;

            float[] locationArray = new[] {longitude, latitude};
            return locationArray;
        }

        public static IOrderedEnumerable<StopPoints> FindNearestBusStops(float lon, float lat)
        {
            IRestResponse<NearestBusStopResponse> nearestBusStopResponses;

            try
            {
                var nearestBusStopRequest = new RestRequest().AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                    .AddQueryParameter("lat", $"{lat}").AddQueryParameter("lon", $"{lon}");
                nearestBusStopResponses = tflclient.Get<NearestBusStopResponse>(nearestBusStopRequest);

            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise Nearest Bus Stop response from request.");
                throw;
            }
            
            
            if (nearestBusStopResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint': expected '200', received {nearestBusStopResponses.StatusCode}");
            }
            
            return nearestBusStopResponses.Data.stopPoints.OrderBy(item => item.distance);
        }

        public static IEnumerable<TFLResponse> FindNextBuses(string stopID)
        {
            IRestResponse<List<TFLResponse>> tflResponses;

            try
            {
                var busStopRequest = new RestRequest($"/{stopID}/Arrivals");
                tflResponses = tflclient.Get<List<TFLResponse>>(busStopRequest);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise next buses response from request.");
                throw;
            }
            
            
            if (tflResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint/stopID/Arrivals': expected '200', received {tflResponses.StatusCode}");
            }
            
            return tflResponses.Data.OrderBy(item => item.timeToStation).Take(5);
        }
    }
}