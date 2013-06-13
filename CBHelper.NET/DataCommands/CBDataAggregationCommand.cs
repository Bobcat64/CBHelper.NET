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
using System.Threading.Tasks;

namespace Cloudbase.DataCommands
{
    public enum CBDataAggregationCommandType
    {
        CBDataAggregationProject = 0,
        CBDataAggregationUnwind = 1,
        CBDataAggregationGroup = 2,
        CBDataAggregationMatch = 3,
    }

    /**
     * This abstract class should be implemented by any class to send 
     * Data Aggregation commands to cloudbase.io
     *
     * The serializeAggregateConditions should resturn a Map
     * exactly in the format needed by the CBHelper class to be added
     * to the list of parmeters, serliazed and sent to cloudbase.io
     */
    public abstract class CBDataAggregationCommand
    {
        private static string[] CBDataAggregationCommandType_ToString = { 
            "$project",
            "$unwind",
            "$group",
            "$match"
        };

        public CBDataAggregationCommandType CommandType { get; set; }

        public string GetCommandTypeString() {
            return CBDataAggregationCommandType_ToString[(int)this.CommandType];
        }

        /**
	     * Serializes the Command object to its JSON representation
	     *
	     * @return An Object representation of the Command object. This
	     *  method should be implemented in each subclass
	     */
        public abstract object SerializeAggregateConditions();
    }
}
