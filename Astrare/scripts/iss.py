from datetime import date, timezone, timedelta, datetime
import argparse
from skyfield.api import EarthSatellite, wgs84, Loader
from skyfield_data import get_skyfield_data_path
import json

def get_args():
    today = date.today()

    parser = argparse.ArgumentParser(
        description=
            "Compute the ephemerides and the events for a given date and a given position on Earth.",
        epilog=(
            "By default, only the events will be computed for today ({date}).\n"
            "To compute also the ephemerides, latitude and longitude arguments"
            " are needed."
        ).format(date=today.strftime("%Y-%m-%d")),
    )

    parser.add_argument(
        "--latitude",
        "-lat",
        type=float,
        default=0,
        help="The observer's latitude on Earth.",
    )
    parser.add_argument(
        "--longitude",
        "-lon",
        type=float,
        default=0,
        help="The observer's longitude on Earth.",
    )
    parser.add_argument(
        "--date",
        "-d",
        type=str,
        default=today.strftime("%Y-%m-%d"),
        help=(
            "The date for which the ephemerides must be calculated. Can be in the YYYY-MM-DD format "
            'or an interval in the "[+-]YyMmDd" format (with Y, M, and D numbers). '
            "Defaults to today ({default_date})."
        ).format(default_date=today.strftime("%Y-%m-%d")),
    )
    parser.add_argument(
        "--timezone",
        "-t",
        type=int,
        default=0,
        help="The timezone to display the hours in (e.g. 2 for UTC+2 or -3 for UTC-3). ",
    )
    return parser.parse_args()

args = get_args()
compute_date = datetime.strptime(args.date, '%Y-%m-%d')
load = Loader(get_skyfield_data_path(),verbose=False)
eph = load('de421.bsp')
ts = load.timescale()
n = 25544
url = 'https://celestrak.com/satcat/tle.php?CATNR={}'.format(n)
filename = 'tle-CATNR-{}.txt'.format(n)
try:
    satellite = load.tle_file(url, filename=filename, reload=True)[0]
except:
    try:
        satellite = load.tle_file(url, filename=filename)[0]
    except:
        line1 = '1 25544U 98067A   22145.42967688  .00010935  00000-0  20155-3 0  9998'
        line2 = '2 25544  51.6435  89.2993 0005214 156.6938 245.6671 15.49672700341657'
        satellite = EarthSatellite(line1, line2, 'ISS (ZARYA)', ts)

bluffton = wgs84.latlon(args.latitude, args.longitude)
t0 = ts.utc(compute_date.year, compute_date.month, compute_date.day, -args.timezone)
t1 = ts.utc(compute_date.year, compute_date.month, compute_date.day + 1, -args.timezone)
t, events = satellite.find_events(bluffton, t0, t1, altitude_degrees=10.0)


def get_satillite_at(timestamp):
    time = ts.utc(timestamp.year, timestamp.month, timestamp.day, timestamp.hour, timestamp.minute, timestamp.second)
    difference = satellite - bluffton
    topocentric = difference.at(time)
    ra, dec, distance = topocentric.radec()
    return (ra, dec)

z = []

for ti, event in zip(t, events):
    z.append((ti, event))

i = 0
results = []

while i < len(z)/3:
    rise = get_satillite_at(z[3*i][0].utc_datetime())
    culmination = get_satillite_at(z[3*i+1][0].utc_datetime())
    set = get_satillite_at(z[3*i+2][0].utc_datetime())
    visible = bool(satellite.at(z[3*i][0]).is_sunlit(eph) | satellite.at(z[3*i+1][0]).is_sunlit(eph) | satellite.at(z[3*i+2][0]).is_sunlit(eph))
    source_tz1 = z[3 * i][0].utc_datetime().tzinfo if z[3*i][0].utc_datetime().tzinfo is not None else timezone.utc
    source_tz2 = z[3 * i + 1][0].utc_datetime().tzinfo if z[3 * i + 1][0].utc_datetime().tzinfo is not None else timezone.utc
    source_tz3 = z[3 * i + 2][0].utc_datetime().tzinfo if z[3 * i + 1][0].utc_datetime().tzinfo is not None else timezone.utc
    results.append({
            "rise": {
                "time": z[3*i][0].utc_datetime().replace(tzinfo=source_tz1).astimezone(tz=timezone(timedelta(hours=args.timezone))).isoformat(),
                "ra": rise[0]._degrees,
                "dec": rise[1]._degrees,
            },
            "culmination": {
                "time": z[3 * i + 1][0].utc_datetime().replace(tzinfo=source_tz2).astimezone(tz=timezone(timedelta(hours=args.timezone))).isoformat(),
                "ra": culmination[0]._degrees,
                "dec": culmination[1]._degrees
            },
            "set": {
                "time": z[3 * i + 2][0].utc_datetime().replace(tzinfo=source_tz3).astimezone(tz=timezone(timedelta(hours=args.timezone))).isoformat(),
                "ra": set[0]._degrees,
                "dec": set[1]._degrees
            },
            "occulted": not(visible)
        })

    i = i + 1

print(json.dumps(results))
