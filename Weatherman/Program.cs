using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Weatherman
{
	class Program
	{

		static void GetWeather(string nameInput, int zipRequested)
		{

			// Key from openweathermap.org
			var token = "6da3e51db971c48c18e3f24fc80686ef";
			var url = $"http://api.openweathermap.org/data/2.5/weather?units=imperial&zip={zipRequested},us&appid=" + token;

			//Get the current weather
			var request = WebRequest.Create(url);
			request.ContentType = "application/json; charset=utf-8";
			var rawJson = String.Empty;
			var response = request.GetResponse();
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				rawJson = reader.ReadToEnd();
			}
			var myWeather = JsonConvert.DeserializeObject<CurrWeather>(rawJson);
			//Console.WriteLine(rawJson);

			//Display the current weather
			Console.WriteLine($"Current weather in {myWeather.name}: {myWeather.weather[0].description}");
			Console.WriteLine($"Temperature: {myWeather.main.temp}");
			Console.WriteLine($"Humidity: {myWeather.main.humidity}");
			Console.ReadLine();

			//Write the weather report to the database
			var connectionStrings = @"Server=localhost\SQLEXPRESS;Database=WeatherMan;Trusted_Connection=True;";
			using (var connection = new SqlConnection(connectionStrings))
			{
				using (var cmd = new SqlCommand())
				{
					cmd.Connection = connection;
					connection.Open();
					cmd.CommandType = System.Data.CommandType.Text;
					cmd.CommandText = @"INSERT INTO Weather_Requests (requester, zip_queried, zip_city, current_weather, current_temp, current_humidity)" +
										"Values (@requester, @zip_queried, @zip_city, @current_weather, @current_temp, @current_humidity)";

					cmd.Parameters.AddWithValue("@requester", nameInput);
					cmd.Parameters.AddWithValue("@zip_queried", zipRequested);
					cmd.Parameters.AddWithValue("@zip_city", myWeather.name);
					cmd.Parameters.AddWithValue("@current_weather", myWeather.weather[0].description);
					cmd.Parameters.AddWithValue("@current_temp", myWeather.main.temp);
					cmd.Parameters.AddWithValue("@current_humidity", myWeather.main.humidity);

					cmd.ExecuteNonQuery();
					connection.Close();
				}
			}
		}

			static void Main(string[] args)
		{
				Console.Write("Please Enter Your Name:  ");
				var nameInput = Console.ReadLine();
				Console.Write("Enter a zipcode to get the current weather:  ");
				var zipInput = Console.ReadLine();
				GetWeather(nameInput, int.Parse(zipInput));
			}
		}
	}
