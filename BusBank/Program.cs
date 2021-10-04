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
            var request = new RestRequest($"/{softwireBusStop}/ArrivalDepartures");

            var responses = client.Get<List<Response>>(request);
            foreach (var response in responses.Data)
            {
                Console.WriteLine(response);
            }

            
        }
    }

    class Response
    {
        public string platformName { get; set; }
        public string destinationName { get; set; }
        public DateTime estimatedTimeOfArrival { get; set; }
        public DateTime estimatedTimeOfDeparture { get; set; }
        public string departureStatus { get; set; }

    }
}