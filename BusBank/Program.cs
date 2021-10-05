using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NLog;
using NLog.Config;
using NLog.Targets;
using RestSharp;

namespace BusBank
{
    class Program
    {
        static void Main(string[] args)
        {
            // Logging configuration
            var config = new LoggingConfiguration();
            var target = new FileTarget { FileName = @"C:\Work\Logs\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;
            
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
                    var nextBuses = APIInterface.FindNextBuses(response.naptanId);
                    foreach (var bus in nextBuses.Take(5))
                    {
                        TimeSpan timeDelta = DateTime.UtcNow - bus.expectedArrival;
                        Console.WriteLine($"    StationName: {bus.stationName}, Direction: {bus.direction}, Towards: {bus.towards}, " +
                                     $"DestinationName: {bus.destinationName}, LineName: {bus.lineName}, " +
                                     $"Expected Arrival: {timeDelta.Humanize()}");
                    }
                }
            }
        }
    }
}