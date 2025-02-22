﻿using System;
using System.Linq;
using Humanizer;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BusBank
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Logging configuration
            var config = new LoggingConfiguration();
            var target = new FileTarget
                {FileName = @"C:\Work\Logs\BusBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}"};
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;

            while (true)
            {
                Console.WriteLine("\nWelcome to BusBank. Please enter a postcode: ");
                var userPostcode = Console.ReadLine()?.Replace(" ", "").ToLower();

                if (userPostcode == "quit") break;

                var location = GetLongAndLat(userPostcode);
                if (location is null)
                {
                    Console.WriteLine("Incorrect Postcode, please enter another.");
                    continue;
                }

                var result = GetBusses(location);
                if (result is null) continue;

                PromptJourneryPlanning(result.Item1, result.Item2, userPostcode);
            }
        }

        public static PostCodeResult GetLongAndLat(string userPostcode)
        {
            PostCodeResult longAndLat;

            try
            {
                longAndLat = APIClient.GetLongAndLat(userPostcode);
            }
            catch (Exception e)
            {
                return null;
            }

            return longAndLat;
        }

        public static Tuple<int, StopPoints[]> GetBusses(PostCodeResult longandlat)
        {
            Console.WriteLine($"\nYour location - Longitude: {longandlat.longitude}, Latitude: {longandlat.latitude}");

            var nearestBusStops = APIClient.FindNearestBusStops(longandlat).Take(2).ToArray();

            if (nearestBusStops.Length == 0)
            {
                Console.WriteLine("There are no Bus Stops within a 200m radius. Please enter another postcode.");
                return null;
            }

            var busStopCounter = 0;

            foreach (var response in nearestBusStops)
            {
                busStopCounter++;
                Console.WriteLine(
                    $"\n{busStopCounter}) StopID: {response.naptanId}, Distance to stop: {response.distance}m\n");
                var nextBuses = APIClient.GetNextBuses(response.naptanId).Take(5).ToArray();

                if (nextBuses.Length == 0)
                {
                    Console.WriteLine("There are no buses coming to this stop. Sorry!");
                    continue;
                }

                foreach (var bus in nextBuses)
                {
                    var timeDelta = DateTime.UtcNow - bus.expectedArrival;
                    Console.WriteLine(
                        $"    StationName: {bus.stationName}, Direction: {bus.direction}, Towards: {bus.towards}, " +
                        $"DestinationName: {bus.destinationName}, LineName: {bus.lineName}, " +
                        $"Expected Arrival: {timeDelta.Humanize()}");
                }
            }

            return Tuple.Create(busStopCounter, nearestBusStops);
        }

        public static void PromptJourneryPlanning(int busStopCounter, StopPoints[] nearestBusStops, string userPostcode)
        {
            while (true)
            {
                Console.WriteLine(
                    $"\nPlan your journey to a bus stop? (Press enter to quit). Please enter a number between 1 and {busStopCounter}: ");
                var userInput = Console.ReadLine();
                if (string.IsNullOrEmpty(userInput)) break;

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

                if (userValue < 1 || userValue > busStopCounter)
                {
                    Console.WriteLine("Sorry, that number is not in range.");
                    continue;
                }

                var userNaptanId = nearestBusStops[userValue - 1].naptanId;

                DisplayJourney(userPostcode, userNaptanId);
            }
        }

        public static void DisplayJourney(string postcode, string busStopId)
        {
            var journey = APIClient.JourneyPlanner(postcode, busStopId);
            foreach (var leg in journey.journeys[0].legs)
            foreach (var instruction in leg.instruction)
            {
                Console.WriteLine($"Summary: {instruction.summary}");

                foreach (var step in instruction.steps)
                    Console.WriteLine($"{step.descriptionHeading} {step.description}");
            }
        }
    }
}