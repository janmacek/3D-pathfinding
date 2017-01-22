/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: Room.cs                                                                                            *
 *      - trieda reprezentuje miestnost v budove                                                            *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;

namespace MapObjectTypes {
    [ Serializable ]
    public class Room : ObjectOfBuilding{

        /// <summary>
        /// prekazky v miestnosti
        /// </summary>
        private List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
        public List<ObjectOfBuilding> Barriers {
            get { return this.barriers; }
            set { this.barriers = value; }
        }

        /// <summary>
        /// prechody medzi miestnostami (dvere, vytahy, schody)
        /// </summary>
        private List<Transit> transits = new List<Transit>();
        public List<Transit> Transits {
            get { return this.transits; }
            set { this.transits = value; }
        }

        /// <summary>
        /// jednoznacne pomenovanie miestnosti
        /// </summary>
        private string name;
        public string Name {
            set { this.name = value; }
            get { return this.name; }
        }

        /// <summary>
        /// konstruktor, nastavi pomenovanie miestnosti
        /// </summary>
        /// <param name="newName">nazov miestnosti</param>
        public Room(string newName) {
            this.name = newName;
        }

        /// <summary>
        /// konstruktor
        /// </summary>
        public Room() {
            this.name = "";
        }

        /// <summary>
        /// vrati bariery a prechody miestnosti
        /// </summary>
        public List<ObjectOfBuilding> GetBarriers() {
            List<ObjectOfBuilding> allBarriers = new List<ObjectOfBuilding>(Barriers);
            Transits.ForEach(transit => allBarriers.Add(transit));
            return allBarriers;
        }

        /// <summary>
        /// zisti ci sa bod nachadza v miestnosti
        /// </summary>
        /// <param name="point">trstovany bod</param>
        /// <returns>vracia ci sa bod nachadza v miestnosti</returns>
        public bool InRoom(MapPoint point) {
            if (point.Z == this.Corners[0].Z) {
                //skontrolujeme ci sa nenachadza v bariere
                int maxX = 0;
                int minX = int.MaxValue;
                int maxY = 0;
                int minY = int.MaxValue;
                foreach (ObjectOfBuilding barrier in Barriers) {
                    if (barrier.InObject(point)) {
                        return false;
                    }
                    foreach (MapPoint corner in barrier.Corners) {
                        if (corner.X > maxX) {
                            maxX = corner.X;
                        }
                        if (corner.X < minX) {
                            minX = corner.X;
                        }
                        if (corner.Y > maxY) {
                            maxY = corner.Y;
                        }
                        if (corner.Y < minY) {
                            minY = corner.Y;
                        }
                    }
                    if (point.X <= maxX && point.X >= minX && point.Y >= minY && point.Y <= maxY) {
                        return false;
                    }
                }
                //skontrolujeme ci sa nenachadza v prechode
                foreach (ObjectOfBuilding transit in Transits) {
                    if (transit.InObject(point)) {
                        return false;
                    }
                }
                //skontrolujeme ci sa nachadza v samotnej izbe
                maxX = 0;
                minX = int.MaxValue;
                maxY = 0;
                minY = int.MaxValue;
                foreach (MapPoint corner in Corners) {
                    if (corner.X > maxX) {
                        maxX = corner.X;
                    }
                    if (corner.X < minX) {
                        minX = corner.X;
                    }
                    if (corner.Y > maxY) {
                        maxY = corner.Y;
                    }
                    if (corner.Y < minY) {
                        minY = corner.Y;
                    }
                }
                if (point.X <= maxX && point.X >= minX && point.Y >= minY && point.Y <= maxY) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// vracia vsetky prekazky okrem dvier
        /// </summary>
        public List<ObjectOfBuilding> BarriersNotDoor() {
            List<ObjectOfBuilding> allBarriers = new List<ObjectOfBuilding>(Barriers);
            foreach (Transit transit in Transits) {
                if (transit.Type != Transit.transitType.door) {
                    allBarriers.Add(transit);
                }
            }
            return allBarriers;
        }

        /// <summary>
        /// vracia vsetky prechody okrem dvier
        /// </summary>
        public List<Transit> TransitsNotDoor() {
            List<Transit> allBarriers = new List<Transit>();
            foreach (Transit transit in Transits) {
                if (transit.Type != Transit.transitType.door) {
                    allBarriers.Add(transit);
                }
            }
            return allBarriers;
        }
    }
}
