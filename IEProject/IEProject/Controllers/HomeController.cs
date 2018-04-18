// ***********************************************************************
// Assembly         : IEProject
// Author           : Kenneth
// Created          : 03-23-2018
//
// Last Modified By : Kenneth
// Last Modified On : 03-28-2018
// ***********************************************************************
// <copyright file="HomeController.cs" company="">
//     Copyright ©  2018
// </copyright>
// <summary>This is a controller class that determines what request to send back to the user when a request is made on the website.
// This controller is part of the MVC design pattern. This class controls all the action links on the website.</summary>
// ***********************************************************************
using IEProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.IO;
using System.Web.Mvc;
using System.Device.Location;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// The Controllers namespace.
/// </summary>
namespace IEProject.Controllers
{
    /// <summary>
    /// Class HomeController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class HomeController : Controller
    {
        /// <summary>
        /// The database
        /// </summary>
        private AccessibilityEntities db = new AccessibilityEntities();
        /// <summary>
        /// This method returns the landing page of the website.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// This method returns the About page of the website.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// This method returns the contact page of the website.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        /// <summary>
        /// This method returns the Wheelchair Public Toilet page upon clicking the menu button that is named Wheelchair public toilets.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult Publictoilet()
        {
            ViewBag.Message = "Wheelchair public toilets.";
            var model = new PublicToiletForm();
            var PublicToilets = new List<PublicToilet>();
            PublicToilets = db.PublicToilets.ToList();



            return View(model);
        }

        /// <summary>
        /// This is a post method that returns the nearest Wheelchair accessibility toilets after the destination address has been typed into the search box.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>ActionResult.</returns>
        [HttpPost]
        public ActionResult Publictoilet(PublicToiletForm model)
        {
            ViewBag.Message = "Wheelchair public toilets.";
            var PublicToilets = new List<PublicToilet>();
            var TrainStations = new List<TrainStation>();
            var Buildings = new List<building>();
            var parkingspots = new List<Parking>();
            PublicToilets = db.PublicToilets.ToList();
            TrainStations = db.TrainStations.ToList();
            Buildings = db.buildings.ToList();
            parkingspots = db.Parkings.ToList();
            GeoCoordinate distanceFrom = new GeoCoordinate();
            GeoCoordinate distanceTo = new GeoCoordinate();
            model.toilets = PublicToilets;
            model.stations = TrainStations;
            model.buildings = Buildings;
            distanceFrom.Latitude = model.Latitude;
            distanceFrom.Longitude = model.Longitude;
            List<ToiletNearby> ordered = new List<ToiletNearby>();
            List<StationsNearby> nearest = new List<StationsNearby>();
            List<BuildingsNearby> nearestbuilding = new List<BuildingsNearby>();
            List<ParkingNearby> nearestparking = new List<ParkingNearby>();



            /// <summary>
            /// This is a for loop that calculates the distance for all the wheelchair public toilets from the destination address, and adds them to a new List.
            /// </summary>
            foreach (var toilet in PublicToilets)
            {
                string[] tempLocation = toilet.location.Trim().Substring(toilet.location.IndexOf("(") + 1, toilet.location.LastIndexOf(")") - 1).Split(',');
                double latitude = Double.Parse(tempLocation[0]);
                double longitude = Double.Parse(tempLocation[1]);
                distanceTo.Latitude = latitude;
                distanceTo.Longitude = longitude;
                ordered.Add(new ToiletNearby { name = toilet.name, distance = Math.Round(distanceFrom.GetDistanceTo(distanceTo), 0), address = toilet.Address });
            }

            foreach (var station in TrainStations)
            {
                string[] tempLocation = station.location.Trim().Substring(station.location.IndexOf("(") + 1, station.location.LastIndexOf(")") - 1).Split(',');
                double latitude = Double.Parse(tempLocation[0]);
                double longitude = Double.Parse(tempLocation[1]);
                distanceTo.Latitude = latitude;
                distanceTo.Longitude = longitude;
                nearest.Add(new StationsNearby { name = station.station, distance = Math.Round(distanceFrom.GetDistanceTo(distanceTo), 0) });
            }
            foreach (var building in Buildings)
            {
               
                double latitude = building.y_coordinate;
                double longitude =building.x_coordinate;
                distanceTo.Latitude = latitude;
                distanceTo.Longitude = longitude;
                nearestbuilding.Add(new BuildingsNearby {  distance = Math.Round(distanceFrom.GetDistanceTo(distanceTo), 0), address = building.Street_address, type = building.Trading_name });
            }

            foreach (var parkingspot in parkingspots)
            {

                double latitude = parkingspot.lat;
                double longitude = parkingspot.lon;
                distanceTo.Latitude = latitude;
                distanceTo.Longitude = longitude;
                nearestparking.Add(new ParkingNearby { distance = Math.Round(distanceFrom.GetDistanceTo(distanceTo), 0), id = parkingspot.BayId });
            }

            /// <summary>
            /// This sorts all items in the list named ordered and adds the top 5 items(wheelchair public toilets) to another list.
            /// </summary>
            if (model.select == false)
            {
                model.Address = "";
            }
            var regex = "^[#.0-9a-zA-Z\\s,-]+$";
            Match match = Regex.Match(model.Address ?? "", regex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                List<StationsNearby> SortedTrains = nearest.OrderBy(a => a.distance).ToList();
                List<ToiletNearby> SortedList = ordered.OrderBy(a => a.distance).ToList();
                List<BuildingsNearby> SortedBuildings = nearestbuilding.OrderBy(a => a.distance).ToList();
                List<ParkingNearby> SortedParkingspots = nearestparking.OrderBy(a => a.distance).ToList();
                for (int i = 0; i < 5; i++)
                {
                    if (SortedList[i].distance <= 1000)
                    {
                        model.sorttoilets.Add(SortedList[i]);

                        model.sorttoilets[i].name = model.sorttoilets[i].name.Substring(model.sorttoilets[i].name.LastIndexOf('-') + 1);

                    }

                }
                for (int i = 0; i < 1; i++)
                {

                    model.sorttrains.Add(SortedTrains[i]);




                }
                for (int i = 0; i < 5; i++)
                {

                    model.sortbuildings.Add(SortedBuildings[i]);




                }
                for (int i = 0; i < 5; i++)
                {

                    model.sortparkingspots.Add(SortedParkingspots[i]);




                }

                return View(model);
            }
            else
            {
                model.sorttoilets.Clear();
                return View(model);
            }
        }

    }
}