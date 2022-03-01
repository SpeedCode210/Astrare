#!/usr/bin/env python3

#    Kosmorro - Compute The Next Ephemerides
#    Copyright (C) 2019  Jérôme Deuchnord <jerome@deuchnord.fr>
#
#    This program is free software: you can redistribute it and/or modify
#    it under the terms of the GNU Affero General Public License as
#    published by the Free Software Foundation, either version 3 of the
#    License, or (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU Affero General Public License for more details.
#
#    You should have received a copy of the GNU Affero General Public License
#    along with this program.  If not, see <https://www.gnu.org/licenses/>.

import argparse
import sys

from kosmorrolib import Position, get_ephemerides, get_events, get_moon_phase
from kosmorrolib.__version__ import __version__ as kosmorrolib_version
from kosmorrolib.exceptions import OutOfRangeDateError
from datetime import date, timedelta

from . import dumper, environment, debug
from .date import parse_date
from .__version__ import __version__ as kosmorro_version
from .exceptions import UnavailableFeatureError, OutOfRangeDateError as DateRangeError
from _kosmorro.i18n.utils import _


def main():
    env_vars = environment.get_env_vars()
    args = get_args()
    debug.show_debug_messages = args.show_debug_messages

    if args.special_action is not None:
        return 0 if args.special_action() else 1

    try:
        compute_date = parse_date(args.date)
    except ValueError as error:
        print(error.args[0])
        return -1

    position = None

    if args.latitude is not None or args.longitude is not None:
        position = Position(args.latitude, args.longitude)
    elif env_vars.latitude is not None and env_vars.longitude is not None:
        position = Position(float(env_vars.latitude), float(env_vars.longitude))

    timezone = args.timezone

    if timezone is None and env_vars.timezone is not None:
        timezone = int(env_vars.timezone)
    elif timezone is None:
        timezone = 0

    try:
        output = "["
        for i in range(1, args.days+1):
            if i != 1:
                output = output + ","
            output = output + str(get_information(
                compute_date + timedelta(days=i-1),
                position,
                timezone,
            ))
        output = output + "]"
    except UnavailableFeatureError as error:
        print(error.msg)
        debug.debug_print(error)
        return 2
    except DateRangeError as error:
        print(error.msg)
        debug.debug_print(error)
        return 1

    print(output)

    return 0


def get_information(
    compute_date: date,
    position: Position,
    timezone: int,
) -> dumper.Dumper:
    if position is not None:
        try:
            eph = get_ephemerides(
                for_date=compute_date, position=position, timezone=timezone
            )
        except OutOfRangeDateError as error:
            raise DateRangeError(error.min_date, error.max_date)
    else:
        eph = []

    try:
        moon_phase = get_moon_phase(for_date=compute_date, timezone=timezone)
    except OutOfRangeDateError as error:
        moon_phase = None

    events_list = get_events(compute_date, timezone)

    return dumper.JsonDumper(
        ephemerides=eph,
        moon_phase=moon_phase,
        events=events_list,
        date=compute_date,
        timezone=timezone,
    )


def output_version() -> bool:
    python_version = "%d.%d.%d" % (
        sys.version_info[0],
        sys.version_info[1],
        sys.version_info[2],
    )
    print("Kosmorro Lite %s" % kosmorro_version)
    print(
        _(
            "Running on Python {python_version} "
            "with Kosmorrolib v{kosmorrolib_version}"
        ).format(python_version=python_version, kosmorrolib_version=kosmorrolib_version)
    )

    return True


def get_args():
    today = date.today()

    parser = argparse.ArgumentParser(
        description=_(
            "Compute the ephemerides and the events for a given date and a given position on Earth."
        ),
        epilog=_(
            "By default, only the events will be computed for today ({date}).\n"
            "To compute also the ephemerides, latitude and longitude arguments"
            " are needed."
        ).format(date=today.strftime(dumper.FULL_DATE_FORMAT)),
    )

    parser.add_argument(
        "--version",
        "-v",
        dest="special_action",
        action="store_const",
        const=output_version,
        default=None,
        help=_("Show the program version"),
    )
    parser.add_argument(
        "--latitude",
        "-lat",
        type=float,
        default=None,
        help=_(
            "The observer's latitude on Earth. Can also be set in the KOSMORRO_LATITUDE environment "
            "variable."
        ),
    )
    parser.add_argument(
        "--longitude",
        "-lon",
        type=float,
        default=None,
        help=_(
            "The observer's longitude on Earth. Can also be set in the KOSMORRO_LONGITUDE "
            "environment variable."
        ),
    )
    parser.add_argument(
        "--date",
        "-d",
        type=str,
        default=today.strftime("%Y-%m-%d"),
        help=_(
            "The date for which the ephemerides must be calculated. Can be in the YYYY-MM-DD format "
            'or an interval in the "[+-]YyMmDd" format (with Y, M, and D numbers). '
            "Defaults to today ({default_date})."
        ).format(default_date=today.strftime("%Y-%m-%d")),
    )
    parser.add_argument(
        "--timezone",
        "-t",
        type=int,
        default=None,
        help=_(
            "The timezone to display the hours in (e.g. 2 for UTC+2 or -3 for UTC-3). "
            "Can also be set in the KOSMORRO_TIMEZONE environment variable."
        ),
    )
    parser.add_argument(
        "--days",
        "-n",
        type=int,
        default=1,
        help=_(
            "The number of days to compute "
            "Defaults to 1."
        ),
    )
    parser.add_argument(
        "--debug",
        dest="show_debug_messages",
        action="store_true",
        help=_("Show debugging messages"),
    )


    return parser.parse_args()
