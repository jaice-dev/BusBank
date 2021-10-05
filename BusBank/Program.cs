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

                var busStopCounter = 0;
                
                foreach (var response in nearestBusStops)
                {
                    busStopCounter++;
                    Console.WriteLine(
                        $"\n{busStopCounter}) StopID: {response.naptanId}, Distance to stop: {response.distance}m\n");
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
                
                while (true)
                {
                    Console.WriteLine($"\nPlan your journey to a bus stop? (Press enter to quit). Please enter a number between 1 and {busStopCounter}: ");
                    var userInput = Console.ReadLine();
                    if (userInput == null)
                    {
                        break;
                    }

                    int userValue;
                    
                    try
                    {
                        userValue = int.Parse(userInput);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Sorry, not a valid integer. Try again.");
                        continue;
                    }

                    if (1 < userValue || userValue > busStopCounter)
                    {
                        Console.WriteLine($"Sorry, that number is not in range.");
                        continue;
                    }

                    var userNaptanId = nearestBusStops[userValue - 1].naptanId;
                    
                    DisplayJourney(userPostcode, userNaptanId);


                }
            }
        }

        public static void DisplayJourney(string postcode, string busStopId)
        {
            var journey = APIInterface.JourneyPlanner(postcode, busStopId);
            Console.WriteLine(journey);
        }
    }
}