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
            float longitude;
            float latitude;
            
            var postcodeclient = new RestClient("http://api.postcodes.io/postcodes");
            var postcoderequest = new RestRequest($"/{postcode}");
            var postcodeResponse = postcodeclient.Get<PostcodeResponse>(postcoderequest);

            if (postcodeResponse.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'http://api.postcodes.io': expected '200', received {postcodeResponse.StatusCode}");
                throw new Exception($"Received incorrect status code: {postcodeResponse.StatusCode}");
            }

            try
            {
                longitude = postcodeResponse.Data.result.longitude;
                latitude = postcodeResponse.Data.result.latitude;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise postcode response from request.");
                throw;
            }

            float[] locationArray = new[] {longitude, latitude};
            return locationArray;
        }

        public static IOrderedEnumerable<StopPoints> FindNearestBusStops(float lon, float lat)
        {
            
            var nearestBusStopRequest = new RestRequest().AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                .AddQueryParameter("lat", $"{lat}").AddQueryParameter("lon", $"{lon}");
            var nearestBusStopResponses = tflclient.Get<NearestBusStopResponse>(nearestBusStopRequest);
            
            if (nearestBusStopResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint': expected '200', received {nearestBusStopResponses.StatusCode}");
                throw new Exception($"Received incorrect status code: {nearestBusStopResponses.StatusCode}");
            }

            try
            {
                return nearestBusStopResponses.Data.stopPoints.OrderBy(item => item.distance);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise Nearest Bus Stop response from request.");
                throw;
            }
            
        }

        public static IEnumerable<TFLResponse> FindNextBuses(string stopID)
        {
            var busStopRequest = new RestRequest($"/{stopID}/Arrivals");
            var tflResponses = tflclient.Get<List<TFLResponse>>(busStopRequest);

            if (tflResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal($"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint/stopID/Arrivals': expected '200', received {tflResponses.StatusCode}");
            }

            try
            {
                return tflResponses.Data.OrderBy(item => item.timeToStation).Take(5);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise next buses response from request.");
                throw new Exception($"Received incorrect status code: {tflResponses.StatusCode}");
            }
            
        }
    }
}