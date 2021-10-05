using System;
using System.Collections.Generic;

namespace BusBank
{
    public class TFLResponse
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
        
    public class Result
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
        public int distance { get; set; }
    }

    public class JourneyPlannerResponse
    {
        public List<Journey> journeys { get; set; }
    }

    public class Journey
    {
        public DateTime arrivalDateTime { get; set; }
        public List<Leg> legs { get; set; }
    }

    public class Leg
    {
        public List<Instruction> instruction { get; set; }
    }

    public class Instruction
    {
        public string summary { get; set; }
        public List<Step> steps { get; set; }
    }

    public class Step
    {
        public string description { get; set; }
        public string descriptionHeading { get; set; }
    }
}