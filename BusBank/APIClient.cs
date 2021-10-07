using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using RestSharp;

namespace BusBank
{
    public static class APIClient
    {
        //Logger
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly RestClient tflclient = new("https://api.tfl.gov.uk");

        private static TData GetResponse<TData>(RestClient restClient, IRestRequest restRequest)
        {
            var response = restClient.Get<TData>(restRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Logger.Error(
                    $"Incorrect response from server 'http://api.postcodes.io': expected '200', received {response.StatusCode}");
                throw new Exception($"Received incorrect status code: {response.StatusCode}");
            }

            try
            {
                return response.Data;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise postcode response from request.");
                throw;
            }
        }

        public static PostCodeResult GetLongAndLat(string postcode)
        {
            var postcodeClient = new RestClient("http://api.postcodes.io/postcodes");
            var postcodeRequest = new RestRequest($"/{postcode}");

            var postcodeResponse = GetResponse<PostCodeResponse>(postcodeClient, postcodeRequest);
            return postcodeResponse.result;
        }

        public static List<StopPoints> Test(PostCodeResult postCode)
        {
            var nearestBusStopRequest = new RestRequest("/StopPoint")
                .AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                .AddQueryParameter("lat", $"{postCode.latitude}").AddQueryParameter("lon", $"{postCode.longitude}");

            var nearestBusStopResponses = GetResponse<NearestBusStopResponse>(tflclient, nearestBusStopRequest);
            return nearestBusStopResponses.stopPoints;
        }

        public static IOrderedEnumerable<StopPoints> FindNearestBusStops(float lon, float lat)
        {
            var nearestBusStopRequest = new RestRequest("/StopPoint")
                .AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                .AddQueryParameter("lat", $"{lat}").AddQueryParameter("lon", $"{lon}");
            var nearestBusStopResponses = tflclient.Get<NearestBusStopResponse>(nearestBusStopRequest);

            if (nearestBusStopResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal(
                    $"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint': expected '200', received {nearestBusStopResponses.StatusCode}");
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
            var busStopRequest = new RestRequest($"/StopPoint/{stopID}/Arrivals");
            var tflResponses = tflclient.Get<List<TFLResponse>>(busStopRequest);

            if (tflResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal(
                    $"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint/stopID/Arrivals': expected '200', received {tflResponses.StatusCode}");
                throw new Exception($"Received incorrect status code: {tflResponses.StatusCode}");
            }

            try
            {
                return tflResponses.Data.OrderBy(item => item.timeToStation).Take(5);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise next buses response from request.");
                throw;
            }
        }

        public static JourneyPlannerResponse JourneyPlanner(string postcode, string NaptanId)
        {
            var journeyPlannerRequest = new RestRequest($"/Journey/JourneyResults/{postcode}/to/{NaptanId}");
            var journeyPlannerResponses = tflclient.Get<JourneyPlannerResponse>(journeyPlannerRequest);

            if (journeyPlannerResponses.StatusCode != HttpStatusCode.OK)
            {
                Logger.Fatal(
                    $"Incorrect response from server 'https://api.tfl.gov.uk/StopPoint//Journey/JourneyResults/from/to/to: expected '200', received {journeyPlannerResponses.StatusCode}");
                throw new Exception($"Received incorrect status code: {journeyPlannerResponses.StatusCode}");
            }

            try
            {
                return journeyPlannerResponses.Data;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to deserialise next buses response from request.");
                throw;
            }
        }
    }
}