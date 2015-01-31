About
=====
    
Application allows you to watch how much RAM do SQL queries and stored procedures at your MS SQL Server do consume.

You just set the server's domain-name (IP address should work as well) and the value of the timer. The application will execute a "monitoring" query which returns the most RAM consuming query at the server.

[queryRAMconsumption queries list screenshot](/img/qrc1.png?raw=true "queryRAMconsumption queries list screenshot")

In that list there are most consuming queries at the moment of timer event. If there was a heavy one, it'll be marked with such note.

The next screen shows how you can see the details of the heavy queries:

[queryRAMconsumption heavy query details screenshot](/img/qrc2.png?raw=true "queryRAMconsumption heavy query details screenshot")

Settings
========

You can set some splitting settings in the `.config` file.

#### db_login

Login to connect to your MS SQL Server.

#### db_password

Password to connect to your MS SQL Server.

#### defaultServer

Default MS SQL Server to work with.

#### limit4query

Value of RAM requested by query that will be considered as "heavy".

3rd party
=========
The application is written in C#, WPF with Visual Studio 2013 (if that matters) and .NET 4.5.1. Those nasty bastards did all the work, I just wrote a few lines of code.

And I snatched some (all) icons from [Iconfinder](https://www.iconfinder.com/).