# kosmorro(1) -- a program that computes the ephemerides

## SYNOPSIS

`kosmorro-lite`  
`kosmorro-lite` [_OPTIONS_]...

## OPTIONS

`-h`, `--help`  
    show a help message and exit

`--version`, `-v`  
    show the program version

`--latitude=`_LATITUDE_, `-lat` _LATITUDE_  
    the observer's latitude on Earth

`--longitude=`_LONGITUDE_, `-lon` _LONGITUDE_  
    the observer's longitude on Earth

`--date=`_DATE_, `-d` _DATE_  
    The date for which the ephemerides must be computed, either in the YYYY-MM-DD format or as an interval in the "[+-]YyMmDd" format (with Y, M, and D numbers); defaults to the current date

`--timezone=`_TIMEZONE_, `-t` _TIMEZONE_  
    the timezone to display the hours in; e.g. 2 for UTC+2 or -3 for UTC-3

## ENVIRONMENT VARIABLES

The environment variable listed below may be used instead of the options.
The options have a higher priority than the environment variable.
As a consequence, any option that would be given to `kosmorro` will override its corresponding environment variable.

Available environment variables are:

`KOSMORRO_LATITUDE`  
    the observer's latitude on Earth (alternative to `--latitude`)
    
`KOSMORRO_LONGITUDE`  
    the observer's longitude on Earth (alternative to `--longitude`)
    
`KOSMORRO_TIMEZONE`  
    the observer's timezone (alternative to `--timezone`)

## EXAMPLES

Compute the events only for the current date:

```
kosmorro
```

Compute the ephemerides for Lille, France, on April 1st, 2022:

```
kosmorro --latitude=50.5876 --longitude=3.0624 --date=2022-04-01
```



## AUTHOR

Written by Jérôme Deuchnord.

## REPORTING BUGS

Please report any encountered bugs on Kosmorro's [GitHub project](https://github.com/Deuchnord/kosmorro).

## COPYRIGHT

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

