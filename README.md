Copyright (C) 2013 Russell Bockhorst
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

CBHelper.NET
=================

The cloudbase.io .NET helper class (http://cloudbase.io)

The Visual Studio 2010 solution contains one project
- CBHelper.NET - the actual helper class which compiles to a dll library

The cloudbase.io helper class utilizes the Newtonsoft.Json package. While this package
is included in the solution you can get new updates and fixes using NuGet.

This library is a modified version of the Windows 8 library that compiles to a
pure .NET library. Some functionality is lost, such as registering for push notifications,
logging navigation, and some geolocation features.
However you can still send push notifications using this library. It was mainly created
for use in a .NET web server.