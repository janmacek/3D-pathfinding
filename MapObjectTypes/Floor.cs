/************************************************************************************************************
 * Author: Jan Macek                                                                                        *
 * Project: MapObjectTypes.csproj                                                                           *
 * File: Floor.cs                                                                                           *
 *      - trieda reprezentuje poschodie budovy                                                              *
 * Date: 27.4.2013                                                                                          *
 * Mail: xmacek18@fit.vutbr.cz                                                                              *
 *                                                                                                          *
 * Copyright © Jan Macek 2013                                                                               *
 * *********************************************************************************************************/
using System;
using System.Collections.Generic;

namespace MapObjectTypes {
    /// <summary>
    /// reprezentuje poschodie budovy
    /// </summary>
    [Serializable]
    public class Floor {

        /// <summary>
        /// pole obsahujuce informacie o miestnostiach
        /// </summary>
        private List<Room> rooms = new List<Room>();
        public List<Room> Rooms {
            get { return this.rooms; }
            set { this.rooms = value; }
        }

        /// <summary>
        /// cislo poschodia
        /// </summary>
        private int floorNumber;
        public int FloorNumber {
            get { return this.floorNumber; }
            set { this.floorNumber = value; }
        }
    }
}
