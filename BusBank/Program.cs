using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BusBank
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://api.digital.tfl.gov.uk/StopPoint/{id}/ArrivalDepartures
            var softwireBusStop = "490008660N";
            var postcode = "nw51tl";
            
            var tflclient = new RestClient("https://api.tfl.gov.uk/StopPoint");
            var busStopRequest = new RestRequest($"/{softwireBusStop}/Arrivals");

            var nearestBusStopRequest = new RestRequest().AddQueryParameter(stopTypes, "NaptanBusWayPoint");

            var postcodeclient = new RestClient("http://api.postcodes.io/postcodes");
            var postcoderequest = new RestRequest($"/{postcode}");
            
            

            var postcodeResponse = postcodeclient.Get<PostcodeResponse>(postcoderequest);
            
            var tflResponses = tflclient.Get<List<TFLResponse>>(busStopRequest);
            foreach (var response in tflResponses.Data.OrderBy(item => item.timeToStation).Take(5))
            {
                Console.WriteLine(response.expectedArrival);
            }
            
            Console.WriteLine($"Longitude: {postcodeResponse.Data.result.longitude}, Latitude: {postcodeResponse.Data.result.latitude}");

            
        }
    }

    class TFLResponse
    {
        public string platformName { get; set; }
        public string vehicleId { get; set; }
        public string stationName { get; set; }
        public string direction { get; set; }
        public string towards { get; set; }
        public string destinationName { get; set; }
        public int timeToStation { get; set; }
        public DateTime expectedArrival { get; set; }
        
    }

    class PostcodeResponse
    {
        public Result result { get; set; }
    }

    class Result
    {
        public float longitude { get; set; }
        public float latitude { get; set; }
    }
}