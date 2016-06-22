# PZLogger.NET

This program will parse all specified in Settings/settings.[yml|json] Project Zomboid logs (%USERPROFILE%\Project Zomboid\Zomboid\Logs) and copy them to a MySQL database.  
PvP not supported yet.  


## Start Options  
### Command line arguments  
**-c --console** Enables [Fast Console](https://github.com/Aragas/ConsoleManager).  
**--fps=** FPS of the console, integer.  
**-s --silent** Enables silent mode.  
**-m --minimize** Enables minimize mode.  
**-cf --config=** Used Config(YAML, JSON).  
**-h --help** Shows help.  
  
## Settings  
**Host:** IP  
**Port:** Port  
**Database:** Database name  
**Username:** Used Username  
**Password:** Password for used Username  
**LoggedFilePaths:**  All files that should be parsed in format:  
\- %PATH%  
\- %PATH%  
\- %PATH%  
