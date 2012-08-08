using System;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using System.Xml.Linq;

namespace ApiExample.Importer
{
    class Program
    {
        static string databaseConnection = "Data Source=.;Initial Catalog=ApiImport;Integrated Security=true";
        static string apiUrl = "http://api.nestoria.co.uk/api?country=uk&action=search_listings&place_name={0}&encoding=xml&listing_type=buy";
        static double pollIntervalMilliseconds = 5000;
        static string[] locations = new[] { "London", "Birmingham", "Manchester" };
        private static int currentLocation = 0;

        static void Main(string[] args)
        {
            var timer = new Timer(pollIntervalMilliseconds);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            PollApi();

            while (Console.Read() != 'q') ;
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PollApi();
        }

        static void PollApi()
        {
            var url = string.Format(apiUrl, locations[currentLocation]);
            currentLocation = currentLocation + 1;
            if (currentLocation == locations.Length)
            {
                currentLocation = 0;
            }

            var doc = XDocument.Load(url);
            var listings = doc.Descendants("listings");

            using (var con = new SqlConnection(databaseConnection))
            {
                con.Open();

                var cmd = new SqlCommand(@"
IF EXISTS (SELECT * FROM Listing WHERE Guid = @Guid)
BEGIN
    UPDATE Listing
    SET Longitude = @longitude, Latitude = @latitude, Summary = @summary
    WHERE [Guid] = @guid
END
ELSE
BEGIN
    INSERT INTO Listing ([Guid], Longitude, Latitude, Summary)
    VALUES (@guid, @longitude, @latitude, @summary)
END    
", con);
                cmd.Parameters.Add("guid", SqlDbType.NVarChar, 50);
                cmd.Parameters.Add("longitude", SqlDbType.NVarChar, 50);
                cmd.Parameters.Add("latitude", SqlDbType.NVarChar, 50);
                cmd.Parameters.Add("summary", SqlDbType.NVarChar, -1);

                foreach (var listing in listings)
                {
                    cmd.Parameters["guid"].Value = listing.Attribute("guid").Value;
                    cmd.Parameters["longitude"].Value = listing.Attribute("longitude") != null ? listing.Attribute("longitude").Value : "";
                    cmd.Parameters["latitude"].Value = listing.Attribute("latitude") != null ? listing.Attribute("latitude").Value : "";
                    cmd.Parameters["summary"].Value = listing.Attribute("summary").Value;

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
