/* Copyright (C) 2012 cloudbase.io
 
 This program is free software; you can redistribute it and/or modify it under
 the terms of the GNU General Public License, version 2, as published by
 the Free Software Foundation.
 
 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 for more details.
 
 You should have received a copy of the GNU General Public License
 along with this program; see the file COPYING.  If not, write to the Free
 Software Foundation, 59 Temple Place - Suite 330, Boston, MA
 02111-1307, USA.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloudbase.DataCommands;
using Newtonsoft.Json.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace Cloudbase.GeoDataStream
{
    /**
     * Represents an object returned by a CBGeoDataStream with its 
     * coordinates, altitude and additional information stored in the 
     * cloud database collection
     */
    public class CBGeoLocatedObject
    {
        /**
         * The coordinate position of the object
         */
        public double Latitude;
        public double Longitude;
        /**
         * The altitude of the object if the cb_location_altitude field
         * exists in the document
         */
        public double Altitude;
        /**
         * All of the other data stored in the cloud database for the 
         * document
         */
        public Dictionary<string, Object> ObjectData;

        /// <summary>
        /// Generates a unique hash representing this object
        /// </summary>
        /// <returns>int</returns>
        public int Hash()
        {
            int prime = 31;
            int result = 1;

            result = prime * result + (int)this.Latitude;
            result = prime * result + (int)this.Longitude;
            result = prime * result + (int)this.Altitude;
            result = prime * result + this.ObjectData.GetHashCode();

            return result;
        }
    }

    /**
     * Opens a persistent connection to a particular geo-coded connection on a
     * cloudbase.io Cloud Database and retrieves geo located data for the application.
     *
     * Data is handed back to the application using the protocol.
     *
     * This is meant to be used for augment reality applications.
     */
    public class CBGeoDataStream
    {
        public const int CBGEODATASTREAM_UPDATE_SPEED = 1;
        public const int CBGEODATASTREAM_RADIUS_RATIO = 4;

        private DispatcherTimer callTimer;
        private CBHelper helper;
        private double previousSpeed;
        private double queryRadius;
        private Dictionary<string, CBGeoLocatedObject> foundObjects;
        private string streamName;
        private Geocoordinate previousPosition;
        /**
         * The collection on which to run the search
         */
        public string CollectionName;
        /**
         * The radius for the next search in meters from the point returned by the 
         * getLatestPosition method
         */
        private double _searchRadius;
        public double SearchRadius
        {
            get { return _searchRadius; }
            set { _searchRadius = value; queryRadius = value; }
        }

        public Func<string, Geocoordinate> GetLatestPosition;
        public Func<string, CBGeoLocatedObject, bool> ReceivedPoint;
        public Func<string, CBGeoLocatedObject, bool> RemovingPoint;

        /**
         * Initializes a new CBGeoDataStream object and uses the given CBHelper
         * object to retrieve data from the cloudbase.io APIS.
         *
         * @param name A unique identifier for the stream object. This will be handed over to all the delegate calls
         * @param CBHelper An initialized CBHelper object
         * @param NSString The name of the collection to search
         */
        public CBGeoDataStream(string name, CBHelper helper, string collectionName)
        {
            this.streamName = name;
            this.helper = helper;
            this.CollectionName = collectionName;
            this.SearchRadius = 50; // we start with 50 meters
            this.previousSpeed = 0.0;
            this.foundObjects = new Dictionary<string, CBGeoLocatedObject>();
        }

        public void StartStream()
        {
            this.callTimer = new DispatcherTimer();
            this.callTimer.Interval = TimeSpan.FromSeconds(CBGEODATASTREAM_UPDATE_SPEED);
            this.callTimer.Tick += updateObjects;
            this.callTimer.Start();
        }

        public void StopStream()
        {
            this.callTimer.Stop();
            this.foundObjects.Clear();
            this.previousPosition = null;
        }

        private double GetDistanceBetween(double originLat, double originLng, double destLat, double destLng)
        {
            if (double.IsNaN(originLat) || double.IsNaN(originLng) || double.IsNaN(destLat) || double.IsNaN(destLng))
            {
                throw new ArgumentException("Argument_LatitudeOrLongitudeIsNotANumber");
            }
            else
            {
                double latitude = originLat * 0.0174532925199433;
                double longitude = originLng * 0.0174532925199433;
                double num = destLat * 0.0174532925199433;
                double longitude1 = destLng * 0.0174532925199433;
                double num1 = longitude1 - longitude;
                double num2 = num - latitude;
                double num3 = Math.Pow(Math.Sin(num2 / 2), 2) + Math.Cos(latitude) * Math.Cos(num) * Math.Pow(Math.Sin(num1 / 2), 2);
                double num4 = 2 * Math.Atan2(Math.Sqrt(num3), Math.Sqrt(1 - num3));
                double num5 = 6376500 * num4;
                return num5;
            }
        }

        private double GetDistanceBetween(Geocoordinate origin, Geocoordinate other)
        {
            if (double.IsNaN(origin.Latitude) || double.IsNaN(origin.Longitude) || double.IsNaN(other.Latitude) || double.IsNaN(other.Longitude))
            {
                throw new ArgumentException("Argument_LatitudeOrLongitudeIsNotANumber");
            }
            else
            {
                double latitude = origin.Latitude * 0.0174532925199433;
                double longitude = origin.Longitude * 0.0174532925199433;
                double num = other.Latitude * 0.0174532925199433;
                double longitude1 = other.Longitude * 0.0174532925199433;
                double num1 = longitude1 - longitude;
                double num2 = num - latitude;
                double num3 = Math.Pow(Math.Sin(num2 / 2), 2) + Math.Cos(latitude) * Math.Cos(num) * Math.Pow(Math.Sin(num1 / 2), 2);
                double num4 = 2 * Math.Atan2(Math.Sqrt(num3), Math.Sqrt(1 - num3));
                double num5 = 6376500 * num4;
                return num5;
            }
        }

        private void updateObjects(object sender, object e)
        {
            Geocoordinate currentLocation = this.GetLatestPosition(this.streamName);

            if (this.previousPosition != null)
            {
                double distance = this.GetDistanceBetween(this.previousPosition, currentLocation);

                if (distance < this.queryRadius / CBGEODATASTREAM_RADIUS_RATIO)
                {
                    if (this.helper.DebugMode)
                    {
                        System.Diagnostics.Debug.WriteLine("Not enough distance between the two points. returning without fetching data");
                    }
                    return;
                }

                double speed = distance / CBGEODATASTREAM_UPDATE_SPEED;
                double ratio = 1.0;

                if (this.helper.DebugMode)
                {
                    System.Diagnostics.Debug.WriteLine("Computed speed " + speed + " meters per second");
                }

                if (this.previousSpeed != 0.0)
                {
                    ratio = speed / previousSpeed;
                }
                if (this.queryRadius * ratio < this.SearchRadius)
                {
                    this.queryRadius = this.SearchRadius;
                }
                else
                {
                    this.queryRadius = this.queryRadius * ratio;
                }

                this.previousSpeed = speed;
            }
            this.previousPosition = currentLocation;

            CBHelperSearchCondition condition = new CBHelperSearchCondition(currentLocation, this.queryRadius);

            this.helper.SearchDocument(this.CollectionName, condition, delegate(CBResponseInfo resp)
            {
                if (resp.Status && resp.Data != null)
                {
                    List<object> data = ((JArray)resp.Data).ToObject<List<object>>();
                    foreach (JObject curPointObject in data)
                    {
                        Dictionary<string, object> curPoint = curPointObject.ToObject<Dictionary<string, object>>();
                        Dictionary<string, double> locationData = ((JObject)curPoint["cb_location"]).ToObject<Dictionary<string, double>>();
                        CBGeoLocatedObject newObj = new CBGeoLocatedObject();

                        newObj.Latitude = locationData["lat"];
                        newObj.Longitude = locationData["lng"];
                        if (curPoint["cb_location_altitude"] != null)
                        {
                            newObj.Altitude = Convert.ToDouble(curPoint["cb_location_altitude"]);
                        }
                        newObj.ObjectData = curPoint;

                        if (!this.foundObjects.Keys.Contains(Convert.ToString(newObj.Hash())))//(this.foundObjects[Convert.ToString(newObj.Hash())] == null)
                        {
                            this.foundObjects.Add(Convert.ToString(newObj.Hash()), newObj);
                            this.ReceivedPoint(this.streamName, newObj);
                        }
                    }

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed call to the cloudbase.io APIs");
                }
                return true;
            });

            List<string> itemsToRemove = new List<string>();
            foreach (KeyValuePair<string, CBGeoLocatedObject> item in this.foundObjects)
            {
                if (this.GetDistanceBetween(item.Value.Latitude, item.Value.Longitude, currentLocation.Latitude, currentLocation.Longitude) > this.SearchRadius)
                {
                    this.RemovingPoint(this.streamName, item.Value);
                    itemsToRemove.Add(item.Key);
                }
            }

            foreach (string itemKey in itemsToRemove)
            {
                this.foundObjects.Remove(itemKey);
            }
        }
    }
}
