using System;
using System.Collections.Generic;
using RestSharp;

namespace BusBank
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://api.digital.tfl.gov.uk/StopPoint/{id}/ArrivalDepartures
            var softwireBusStop = "490008660N";
            
            var client = new RestClient("https://api.tfl.gov.uk/StopPoint");
            var request = new RestRequest($"/{softwireBusStop}/Arrivals");

            var responses = client.Get<List<Response>>(request);
            foreach (var response in responses.Data)
            {
                Console.WriteLine(response.expectedArrival);
            }

            
        }
    }

    class Response
    {
        public string platformName;
        public string vehicleId;
        public string stationName;
        public string direction;
        public string towards;
        public string destinationName;
        public int timeToStation;
        public DateTime expectedArrival;

    }
}