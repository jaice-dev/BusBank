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

        public static IOrderedEnumerable<StopPoints> FindNearestBusStops(PostCodeResult postCode)
        {
            var nearestBusStopRequest = new RestRequest("/StopPoint")
                .AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                .AddQueryParameter("lat", $"{postcode.latitude}").AddQueryParameter("lon", $"{postcode.longitude}");

            var nearestBusStopResponses = GetResponse<NearestBusStopResponse>(tflclient, nearestBusStopRequest);
            return nearestBusStopResponses.stopPoints.OrderBy(item => item.distance);
        }

        public static IEnumerable<TFLResponse> GetNextBuses(string stopID)
        {
            var busStopRequest = new RestRequest($"/StopPoint/{stopID}/Arrivals");
            var tflResponses = GetResponse<List<TFLResponse>>(tflclient, busStopRequest);
           
            return tflResponses.OrderBy(item => item.timeToStation).Take(5);

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