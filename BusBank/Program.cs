using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using RestSharp;

namespace BusBank
{
    class Program
    {
        static void Main(string[] args)
        {
            // var softwireBusStop = "490008660N";
            // var postcode = "nw51tl";
            
            //TODO check status codes and add logging/error handling
            //TODO Don’t forget logging and error handling - Are you confident you’ve handled anything that can go wrong? - What happens if your user enters an invalid postcode? - What happens if there aren’t any bus stops nearby? - What happens if there aren’t any busses coming?
            //TODO The TFL API also has a 'Journey Planner'. Edit your program so that (when requested) it will also display directions on how to get to your nearest bus stops.
            var appRunning = true;
            while (appRunning)
            {
                Console.WriteLine("\nWelcome to BusBank. Please enter a postcode: ");
                string userPostcode = Console.ReadLine().Replace(" ", "").ToLower();
                
                if (userPostcode == "quit")
                {
                    appRunning = false;
                }

                var longAndLat= APIInterface.GetLongAndLat(userPostcode);
                var longitude = longAndLat[0];
                var latitude = longAndLat[1];
                
                Console.WriteLine($"\nYour location - Longitude: {longitude}, Latitude: {latitude}");

                var nearestBusStops = APIInterface.FindNearestBusStops(longitude, latitude);
                
                foreach (var response in nearestBusStops.Take(2))
                {
                    Console.WriteLine(
                        $"\nStopNaptanID: {response.naptanId}, Distance to stop: {response.distance}m\n");
                    //TODO modify your program so that the user can supply a postcode and see the next buses at the two nearest bus stops.
                    var nextBuses = APIInterface.FindNextBuses(response.naptanId);
                    foreach (var bus in nextBuses.Take(5))
                    {
                        TimeSpan timeDelta = DateTime.UtcNow - bus.expectedArrival;
                        Console.WriteLine($"    StationName: {bus.stationName}, Direction: {bus.direction}, Towards: {bus.towards}, " +
                                     $"DestinationName: {bus.destinationName}, LineName: {bus.lineName}, " +
                                     $"Expected Arrival: {timeDelta.Humanize()}");
                        //TODO print a list of the next five buses at that stop code, with their routes, destinations, and the time until they arrive in minutes.
                        //TODO Try to ensure you're using a sensible class structure with well-named methods. Remember to commit and push your changes as you go.
                    }
                }
            }
        }
    }
}