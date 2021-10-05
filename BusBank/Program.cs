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
            //TODO Don’t forget logging and error handling - Are you confident you’ve handled anything that can go wrong? - What happens if your user enters an invalid postcode? - What happens if there aren’t any bus stops nearby? - What happens if there aren’t any busses coming?
            //TODO The TFL API also has a 'Journey Planner'. Edit your program so that (when requested) it will also display directions on how to get to your nearest bus stops.
            var appRunning = true;
            while (appRunning)
            {
                Console.WriteLine("Welcome to BusBank. Please enter a postcode: ");
                string userPostcode = Console.ReadLine().Replace(" ", "").ToLower();

                var longAndLat= APIInterface.GetLongAndLat(userPostcode);
                var longitude = longAndLat[0];
                var latitude = longAndLat[1];
                
                Console.WriteLine($"Your location - Longitude: {longitude}, Latitude: {latitude}");

                var nearestBusStops = APIInterface.FindNearestBusStops(longitude, latitude);
                
                foreach (var response in nearestBusStops)
                {
                    Console.WriteLine(
                        $"StopNaptanID: {response.naptanId}, StopID: {response.stationNaptan}, Distance to stop: {response.distance}");
                    //TODO modify your program so that the user can supply a postcode and see the next buses at the two nearest bus stops.
                }




            }
            
            // var softwireBusStop = "490008660N";
            // var postcode = "nw51tl";


            var tflResponses = tflclient.Get<List<TFLResponse>>(busStopRequest);
            foreach (var response in tflResponses.Data.OrderBy(item => item.timeToStation).Take(5))
            {
                Console.WriteLine($"VehicleId: {response.vehicleId}, StationName: {response.stationName}, Direction: {response.direction}, Towards: {response.towards}, " +
                                  $"DestinationName: {response.destinationName}, LineName: {response.lineName}, TimeToStation: {response.timeToStation}, " +
                                  $"Expected Arrival: {response.expectedArrival}");
                //TODO print a list of the next five buses at that stop code, with their routes, destinations, and the time until they arrive in minutes.
                //TODO Try to ensure you're using a sensible class structure with well-named methods. Remember to commit and push your changes as you go.


            }

            


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
        public string lineName { get; set; }

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

    class NearestBusStopResponse
    {
        public List<StopPoints> stopPoints { get; set; }
    }

    public class StopPoints
    {
        public string naptanId { get; set; }
        public string stationNaptan { get; set; }
        public int distance { get; set; }
    }
}