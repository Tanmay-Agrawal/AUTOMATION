using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Automation.WebServices.Master
{

    //Description:- This class contains method for creating HTTP requests , getting response & validating response.
    //Author:-      Tanmay Agrawal 

    public class Master
    {
        private static string _baseUrl;
        private static string _authorisation;
        private static string _xUser;
        private static string _serviceurl;

        //Intialising BaseUrl , Autorization Details , User Details
        private static void SetUp()
        {
            _baseUrl = Environment.GetEnvironmentVariable("BaseServiceUrl", EnvironmentVariableTarget.Process) ??
                       ApplicationSettings.Default.BaseServiceUrl;
            _authorisation = Environment.GetEnvironmentVariable("Authorization", EnvironmentVariableTarget.Process) ??
                             ApplicationSettings.Default.Authorization;
            _xUser = Environment.GetEnvironmentVariable("XActualUser", EnvironmentVariableTarget.Process) ??
                      ApplicationSettings.Default.XActualUser;
        }

        //Creating Http Request
        public static HttpWebRequest CreateHttpRequest(string requesturl, string requestType, string user,
            string inputFileName)
        {
            SetUp();
            if (user != null)
            {
                _xUser = user;
            }
            _serviceurl = _baseUrl + requesturl;
            var wRequest = (HttpWebRequest) WebRequest.Create(_serviceurl);
            wRequest.Method = requestType;
            wRequest.ContentType = "application/json";
            wRequest.Timeout = 300000;
            wRequest.Headers.Add("Authorization",
                _authorisation);
            wRequest.Headers.Add("X-Actual-User", _xUser);
            wRequest.ContentLength = 0;

            if ((requestType == "PUT" || requestType == "POST") && inputFileName!= String.Empty)
            {
                var data =
                    File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "WebServices", "Test Data",
                        inputFileName));
                var byteArray = Encoding.UTF8.GetBytes(data);
                wRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = wRequest.GetRequestStream())
                {

                    try
                    {

                        // Write the data to the request stream.
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        // Close the Stream object.           
                        dataStream.Close();
                        dataStream.Dispose();
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine("An exception has occurred " + e);
                    }
                }
                
            }
            return wRequest;
        }

    

    //Getting the response of WebService.
        public static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    return (HttpWebResponse) e.Response;
                }
                Console.WriteLine("\r\nWebException Raised. The following error occured : {0}", e.Status);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nThe following Exception was raised : {0}", e.Message);
                throw;
            }
            }

        //Extracting Response data from Http Response & Comparing it with Expected Response.
        public static bool CompareResponse(HttpWebResponse response, string fileToCompare)
        {
            try
            {
                var stream = response.GetResponseStream();
                Debug.Assert(stream != null, "stream != null");
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    var responsedata = reader.ReadToEnd();
                    var expectedData =
                        File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "WebServices", "API Response",
                            fileToCompare));
                    if (responsedata.Equals(expectedData))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
    }
}