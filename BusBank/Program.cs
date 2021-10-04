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
        public string platformName { get; set; }
        public string vehicleId { get; set; }
        public string stationName { get; set; }
        public string direction { get; set; }
        public string towards { get; set; }
        public string destinationName { get; set; }
        public int timeToStation { get; set; }
        public DateTime expectedArrival { get; set; }
        
        

    }
}