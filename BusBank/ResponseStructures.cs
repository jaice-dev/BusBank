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
        public int distance { get; set; }
    }
}