# Kinetica_VS

This project is a very crude way of capturing incremental changes in a single source file inside Visual Studio. This is done in the project `Kinetica`. It works by capturing the `LineChanged` text editor event. Whenever a line changes, the system grabs the entire file (!!!) and saves its text. Not the most elegant of solutions.

All projects except `Kinetica` should be ignored (disabled) as they are no longer used.
