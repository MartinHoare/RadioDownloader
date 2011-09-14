﻿/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace RadioDld.Model
{
    using System;

    internal class Programme
    {
        public int Progid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Favourite { get; set; }

        public bool Subscribed { get; set; }

        public bool SingleEpisode { get; set; }

        public string ProviderName { get; set; }
    }
}