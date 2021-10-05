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
            var target = new FileTarget { FileName = @"C:\Work\Logs\BusBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;
            
            //TODO What happens if there aren’t any busses coming?
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

                float[] longAndLat;
                
                try
                {
                    longAndLat= APIInterface.GetLongAndLat(userPostcode);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Incorrect Postcode, please enter another.");
                    continue;
                }
                
                var longitude = longAndLat[0];
                var latitude = longAndLat[1];
                
                Console.WriteLine($"\nYour location - Longitude: {longitude}, Latitude: {latitude}");

                var nearestBusStops = APIInterface.FindNearestBusStops(longitude, latitude).Take(2).ToArray();

                if (nearestBusStops.Length == 0)
                {
                    Console.WriteLine("There are no Bus Stops within a 200m radius. Please enter another postcode.");
                    continue;
                }
                
                foreach (var response in nearestBusStops)
                {
                    Console.WriteLine(
                        $"\nStopNaptanID: {response.naptanId}, Distance to stop: {response.distance}m\n");
                    var nextBuses = APIInterface.FindNextBuses(response.naptanId).Take(5).ToArray();

                    if (nextBuses.Length == 0)
                    {
                        Console.WriteLine("There are no buses coming to this stop. Sorry!");
                        continue;
                    }
                    
                    foreach (var bus in nextBuses)
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