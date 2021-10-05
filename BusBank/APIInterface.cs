using System.Linq;
using RestSharp;

namespace BusBank
{
    public static class APIInterface
    {
        static RestClient tflclient = new RestClient("https://api.tfl.gov.uk/StopPoint");

        public static float[] GetLongAndLat(string postcode)
        {
            var postcodeclient = new RestClient("http://api.postcodes.io/postcodes");
            var postcoderequest = new RestRequest($"/{postcode}");
            var postcodeResponse = postcodeclient.Get<PostcodeResponse>(postcoderequest);
            
            var longitude = postcodeResponse.Data.result.longitude;
            var latitude = postcodeResponse.Data.result.latitude;

            float[] locationArray = new[] {longitude, latitude};
            return locationArray;
        }

        public static IOrderedEnumerable<StopPoints> FindNearestBusStops(float lon, float lat)
        {
            var nearestBusStopRequest = new RestRequest().AddQueryParameter("stopTypes", "NaptanPublicBusCoachTram")
                .AddQueryParameter("lat", $"{lat}").AddQueryParameter("lon", $"{lon}");
            var nearestBusStopResponses = tflclient.Get<NearestBusStopResponse>(nearestBusStopRequest);
            return nearestBusStopResponses.Data.stopPoints.OrderBy(item => item.distance);
        }

        public static void FindNextBuses(string stopID)
        {
            
        }
    }
}