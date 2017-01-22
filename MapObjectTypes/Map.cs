/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: Map.cs                                                                                             *
 *      - trieda reprezentujuca mapu budovy                                                                 *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MapObjectTypes
{
    /// <summary>
    /// trieda reprezentujuca mapu budovy
    /// </summary>
    [ Serializable ]
    public class Map
    {

        /// <summary>
        /// pole obsahujuce informacie o poschodiach
        /// </summary>
        private List<Floor> floors = new List<Floor>();
        public List<Floor> Floors {
            get { return this.floors; }
            set { this.floors = value; }
        }

        /// <summary>
        /// startovaci bod
        /// </summary>
        private MapPoint start;
        public MapPoint Start {
            get { return this.start; }
            set { this.start = value; }
        }

        /// <summary>
        /// cielovy bod
        /// </summary>
        private MapPoint final;
        public MapPoint Final {
            get { return this.final; }
            set { this.final = value; }
        }

        /// <summary>
        /// ulozi aktualny stav mapy budovy do XML suboru
        /// </summary>
        /// <param name="path">cesta, kde vytvorit XML subor</param>
        public void SaveToXML(string fileName) {
            XmlSerializer s = new XmlSerializer(typeof(Map));
            XmlTextWriter xmlWriter = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
            s.Serialize(xmlWriter, this);
            xmlWriter.Close();     
       }
            
        /// <summary>
        /// metoda ktora z XML suboru nacita mapu budovy
        /// </summary>
        /// <param name="fileName">nazov XML suboru</param>
        public string LoadFromXML(string fileName) {
            XmlSerializer serializer = new XmlSerializer(typeof(Map));
            XmlTextReader xmlReader = new XmlTextReader(fileName);
            try {
                Map newMap = new Map();
                newMap = (Map)serializer.Deserialize(xmlReader);
                //prekopirujeme novu mapu do aktualnej
                this.floors = newMap.Floors;
                this.final = newMap.Final;
                this.start = newMap.Start;
            } catch (Exception exception) {
                Console.WriteLine("Wrong XML file. File is unable to read.");
                return exception.InnerException.Message;
            } 
            xmlReader.Close();
            return "Loaded";
        }

        /// <summary>
        /// vyhlada bariery a prekazky v podlazi
        /// </summary>
        /// <returns>vrati zoznam barier v miestnosti</returns>
        /// <param name="floorNumber">cislo poschodia</param>
        public List<ObjectOfBuilding> GetBarriers(int floorNumber) {
            List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
            foreach (Room room in this.Floors[floorNumber].Rooms) {
                foreach (ObjectOfBuilding barrier in room.Barriers) {
                    barriers.Add(barrier);
                }
                foreach (ObjectOfBuilding barrier in room.Transits) {
                    barriers.Add(barrier);
                }
            }
            return barriers;
        }

        /// <summary>
        /// vyhlada bariery a prekazky v podlazi
        /// </summary>
        /// <returns>vrati zoznam barier v miestnosti</returns>
        /// <param name="floorNumber">cislo poschodia</param>
        public List<ObjectOfBuilding> GetBarriers() {
            List<ObjectOfBuilding> barriers = new List<ObjectOfBuilding>();
            foreach (Floor floor in this.Floors) {
                foreach (Room room in this.Floors[floor.FloorNumber].Rooms) {
                    foreach (ObjectOfBuilding barrier in room.Barriers) {
                        barriers.Add(barrier);
                    }
                    foreach (ObjectOfBuilding barrier in room.Transits) {
                        barriers.Add(barrier);
                    }
                }
            }   
            return barriers;
        }

        /// <summary>
        /// vyhlada prechody v podlazi
        /// </summary>
        /// <returns>vrati zoznam prechodov v miestnosti</returns>
        /// <param name="floorNumber">cislo poschodia</param>
        public List<Transit> GetTransits(int floorNumber) {
            List<Transit> transits = new List<Transit>();
            foreach (Room room in this.Floors[floorNumber].Rooms) {
                foreach (Transit transit in room.Transits) {
                    transits.Add(transit);
                }
            }
            return transits;
        }

        /// <summary>
        /// vyhlada prechody v podlazi
        /// </summary>
        /// <returns>vrati zoznam prechodov v miestnosti</returns>
        /// <param name="floorNumber">cislo poschodia</param>
        public List<Transit> GetTransits() {
            List<Transit> transits = new List<Transit>();
            foreach (Floor floor in Floors) {
                foreach (Room room in this.Floors[floor.FloorNumber].Rooms) {
                    foreach (Transit transit in room.Transits) {
                        transits.Add(transit);
                    }
                }
            }
            return transits;
        }

        /// <summary>
        /// zvacsi barieru +1 do vysky a +1 do sirky
        /// </summary>
        /// <param name="barrier">bariera</param>
        /// <returns>vracia zvacsenu barieru</returns>
        public ObjectOfBuilding EnlargeBarrier(ObjectOfBuilding barrier) {
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            foreach (MapPoint item in barrier.Corners) {
                if (item.X > maxX) {
                    maxX = item.X;
                }
                if (item.Y > maxY) {
                    maxY = item.Y;
                }
                if (item.X < minX) {
                    minX = item.X;
                }
                if (item.Y < minY) {
                    minY = item.Y;
                }
            }
            ObjectOfBuilding enlargeBarrier = new ObjectOfBuilding();
            enlargeBarrier.Corners.Add(new MapPoint(minX - 1, minY - 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(minX - 1, maxY + 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX + 1, maxY + 1, barrier.Corners[0].Z));
            enlargeBarrier.Corners.Add(new MapPoint(maxX + 1, minY - 1, barrier.Corners[0].Z));
            return enlargeBarrier;
        }

        /// <summary>
        /// zisti ci sa dany bod nenachadza v kolizii s barierou v miestnostiach 
        /// </summary>
        /// <param name="point">preverovany bod</param>
        /// <param name="rooms">miestnosti ktore testujeme</param>
        /// <returns></returns>
        private bool InBar(MapPoint point, List<Room> rooms) {
            foreach (Room room in rooms) {
                foreach (ObjectOfBuilding barrier in room.Barriers) {
                    ObjectOfBuilding newBarrier = EnlargeBarrier(barrier);
                    if (point.Z == newBarrier.Corners[0].Z) {
                        bool result = false;
                        int k = barrier.Corners.Count - 1;
                        for (int j = 0; j < barrier.Corners.Count; j++) {
                            if ((newBarrier.Corners[j].Y < point.Y && newBarrier.Corners[k].Y >= point.Y
                              || newBarrier.Corners[k].Y < point.Y && newBarrier.Corners[j].Y >= point.Y)
                              && (newBarrier.Corners[j].X <= point.X || newBarrier.Corners[k].X <= point.X)) {
                                try {
                                    if (newBarrier.Corners[j].X + (point.Y - newBarrier.Corners[j].Y) / (newBarrier.Corners[k].Y - newBarrier.Corners[j].Y) * (newBarrier.Corners[k].X - newBarrier.Corners[j].X) < point.X) {
                                        result = !result;
                                    }
                                } catch (DivideByZeroException) {
                                    Console.WriteLine("Error: Dividing by zero!");
                                }
                            }
                            k = j;
                        }
                        if (result == true) {
                            return result;
                        }
                    }
                }
                foreach (ObjectOfBuilding barrier in room.Transits) {
                    ObjectOfBuilding newBarrier = EnlargeBarrier(barrier);
                    if (point.Z == newBarrier.Corners[0].Z) {
                        bool result = false;
                        int k = newBarrier.Corners.Count - 1;
                        for (int j = 0; j < newBarrier.Corners.Count; j++) {
                            if ((newBarrier.Corners[j].Y < point.Y && newBarrier.Corners[k].Y >= point.Y
                              || newBarrier.Corners[k].Y < point.Y && newBarrier.Corners[j].Y >= point.Y)
                              && (newBarrier.Corners[j].X <= point.X || newBarrier.Corners[k].X <= point.X)) {
                                try {
                                    if (newBarrier.Corners[j].X + (point.Y - newBarrier.Corners[j].Y) / (newBarrier.Corners[k].Y - newBarrier.Corners[j].Y) * (newBarrier.Corners[k].X - newBarrier.Corners[j].X) < point.X) {
                                        result = !result;
                                    }
                                } catch (DivideByZeroException) {
                                    Console.WriteLine("Error: Dividing by zero!");
                                }
                            }
                            k = j;
                        }
                        if (result == true) {
                            return result;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// zisti ci sa bod nachadza v budove, mimo prekazok
        /// </summary>
        /// <param name="point">bod ktory testujeme</param>
        /// <returns>vracia true/false ci sa dany bod nachadza v budove (mimo prekazok)</returns>
        public bool IsPointInMap(MapPoint point) {
            bool result = false;
            int floor = int.MinValue;
            for (int i = 0; i < this.floors.Count; i++) {
			    if(this.floors[i].FloorNumber == point.Z) {
                    floor = i;
                } 
			}
            if(floor != int.MinValue) {
                for (int i = 0; i < this.floors[floor].Rooms.Count; i++) {
                    if (InBar(point, this.floors[floor].Rooms)) {
                        return false;
                    }
                    int k = this.floors[floor].Rooms[i].Corners.Count - 1;
                    for (int j = 0; j < this.floors[floor].Rooms[i].Corners.Count; j++) {
                        if (point.X == this.floors[floor].Rooms[i].Corners[j].X || point.Y == this.floors[floor].Rooms[i].Corners[j].Y) {
                            return false;
                        }
                        if ((this.floors[floor].Rooms[i].Corners[j].Y < point.Y && this.floors[floor].Rooms[i].Corners[k].Y >= point.Y
                          || this.floors[floor].Rooms[i].Corners[k].Y < point.Y && this.floors[floor].Rooms[i].Corners[j].Y >= point.Y)
                          && (this.floors[floor].Rooms[i].Corners[j].X <= point.X || this.floors[floor].Rooms[i].Corners[k].X <= point.X)) {
                            try {
                                if (this.floors[floor].Rooms[i].Corners[j].X + (point.Y - this.floors[floor].Rooms[i].Corners[j].Y) / (this.floors[floor].Rooms[i].Corners[k].Y - this.floors[floor].Rooms[i].Corners[j].Y) * (this.floors[floor].Rooms[i].Corners[k].X - this.floors[floor].Rooms[i].Corners[j].X) < point.X) {
                                    result = !result;
                                }
                            } catch (DivideByZeroException) {
                                Console.WriteLine("Error: Dividing by zero!");
                            }
                        }
                        k = j;
                    }
                }
                return result;
            } else {
                return false;
            }
        }


        /// <summary>
        /// vracia rohovy bod poschodia budovy
        /// </summary>
        /// <param name="floorNumber">cislo poschodia budovy</param>
        /// <param name="left">true = lavy / false = pravy</param>
        /// <param name="top">true = horny / false = dolny</param>
        /// <returns>rohovy bod</returns>
        public MapPoint GetLeftTop(int floorNumber, bool left, bool top) {
            if (this.Floors[floorNumber].Rooms.Count > 0 && this.Floors[floorNumber].Rooms[0].Corners.Count > 0) {
                MapPoint leftTop = this.Floors[floorNumber].Rooms[0].Corners[0];
                foreach (Room room in this.Floors[floorNumber].Rooms) {
                    foreach (MapPoint corner in room.Corners) {
                        if (left && top) {
                            if (leftTop.X >= corner.X && leftTop.Y >= corner.Y) {
                                leftTop = corner;
                            }
                        } else if (!left && top) {
                            if (leftTop.X <= corner.X && leftTop.Y >= corner.Y) {
                                leftTop = corner;
                            }
                        } else if (left && !top) {
                            if (leftTop.X >= corner.X && leftTop.Y <= corner.Y) {
                                leftTop = corner;
                            }
                        } else {
                            if (leftTop.X <= corner.X && leftTop.Y <= corner.Y) {
                                leftTop = corner;
                            }
                        }
                        
	                } 
                }
                return leftTop;
            } else {
                return null;
            }
        }

        /// <summary>
        /// zisti ci sa body nachadzaju v rovnakej miestnosti
        /// </summary>
        /// <param name="a">prvy bod</param>
        /// <param name="b">druhy bod</param>
        /// <returns>vracia ci sa body nachadzaju v rovnakej miestnosti</returns>
        public bool InSameRoom(MapPoint a, MapPoint b) {
            if (a.Z == b.Z) {
                foreach (Room room in this.Floors[a.Z].Rooms) {
                    if (room.InRoom(a) && room.InRoom(b)) {
                        return true;
                    } else if ((room.InRoom(a) && !room.InRoom(b)) || (!room.InRoom(a) && room.InRoom(b))) {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// najde hranicne suradnice podlazia
        /// </summary>
        /// <param name="limit">true = max / false = min</param>
        /// <param name="coord">true = x / false = y</param>
        /// <param name="floorNumber">cislo podlazia</param>
        /// <returns></returns>
        public int GetLimitCoord(bool limit, bool coord, int floorNumber) {
            if (this.Floors[floorNumber].Rooms.Count > 0 && this.Floors[floorNumber].Rooms[0].Corners.Count > 0) {
                int limitCoord;
                if (coord) {
                    limitCoord = this.Floors[floorNumber].Rooms[0].Corners[0].X;
                } else {
                    limitCoord = this.Floors[floorNumber].Rooms[0].Corners[0].Y;
                }
                foreach (Room room in this.Floors[floorNumber].Rooms) {
                    foreach (MapPoint corner in room.Corners) {
                        if (limit && coord && corner.X > limitCoord) {
                            limitCoord = corner.X;
                        } else if (limit && !coord && corner.Y > limitCoord) {
                            limitCoord = corner.Y;
                        } else if (!limit && coord && corner.X < limitCoord) {
                            limitCoord = corner.X;
                        } else if (!limit && !coord && corner.Y < limitCoord) {
                            limitCoord = corner.Y;
                        }
                    }
                }
                return limitCoord;
            }
            return -1;
        }

        /// <summary>
        /// najde hranicne suradnice mapy
        /// </summary>
        /// <param name="limit">true = max / false = min</param>
        /// <param name="coord">true = x / false = y</param>
        /// <returns></returns>
        public int GetLimitCoord(bool limit, bool coord) {
            int limitCoord;
            if (coord) {
                limitCoord = this.Floors[0].Rooms[0].Corners[0].X;
            } else {
                limitCoord = this.Floors[0].Rooms[0].Corners[0].Y;
            }
            foreach (Floor floor in this.Floors) {
                foreach (Room room in this.Floors[floor.FloorNumber].Rooms) {
                    foreach (MapPoint corner in room.Corners) {
                        if (limit && coord && corner.X > limitCoord) {
                            limitCoord = corner.X;
                        } else if (limit && !coord && corner.Y > limitCoord) {
                            limitCoord = corner.Y;
                        } else if (!limit && coord && corner.X < limitCoord) {
                            limitCoord = corner.X;
                        } else if (!limit && !coord && corner.Y < limitCoord) {
                            limitCoord = corner.Y;
                        }
                    }
                }
            }
            return limitCoord;
        }
    }
}
