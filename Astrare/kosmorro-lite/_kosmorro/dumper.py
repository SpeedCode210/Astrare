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

from abc import ABC, abstractmethod
import datetime
import json

from kosmorrolib import AsterEphemerides, Event, EventType
from kosmorrolib.model import ASTERS, MoonPhase
from kosmorrolib.enum import SeasonType, LunarEclipseType

from .i18n.utils import _, FULL_DATE_FORMAT


def serialize_season_change(event) -> dict:
    event.details['season'] = event.details['season'].name
    return {
        "objects": [object.serialize() for object in event.objects],
        "EventType": event.event_type.name,
        "starts_at": event.start_time.isoformat(),
        "ends_at": event.end_time.isoformat() if event.end_time is not None else None,
        "details": event.details,
    }

def serialize_lunar_eclipse(event) -> dict:

    event.details['type'] = event.details['type'].name

    event.details['maximum'] = event.details['maximum'].isoformat() if event.details['maximum'] is not None else None

    return {
        "objects": [object.serialize() for object in event.objects],
        "EventType": event.event_type.name,
        "starts_at": event.start_time.isoformat(),
        "ends_at": event.end_time.isoformat() if event.end_time is not None else None,
        "details": event.details,
    }


class Dumper(ABC):
    ephemerides: [AsterEphemerides]
    moon_phase: MoonPhase
    events: [Event]
    date: datetime.date
    timezone: int

    def __init__(
        self,
        ephemerides: [AsterEphemerides],
        moon_phase: MoonPhase,
        events: [Event],
        date: datetime.date,
        timezone: int,
    ):
        self.ephemerides = ephemerides
        self.moon_phase = moon_phase
        self.events = events
        self.date = date
        self.timezone = timezone

    def get_date_as_string(self, capitalized: bool = False) -> str:
        date = self.date.strftime(FULL_DATE_FORMAT)

        if capitalized:
            return "".join([date[0].upper(), date[1:]])

        return date

    def __str__(self):
        return self.to_string()

    @abstractmethod
    def to_string(self):
        pass

    @staticmethod
    def is_file_output_needed() -> bool:
        return False


class JsonDumper(Dumper):
    SUPPORTED_EVENTS = [
        EventType.OPPOSITION,
        EventType.CONJUNCTION,
        EventType.OCCULTATION,
        EventType.MAXIMAL_ELONGATION,
        EventType.PERIGEE,
        EventType.APOGEE,
        EventType.LUNAR_ECLIPSE,
        EventType.SEASON_CHANGE
    ]

    def to_string(self):
        return json.dumps(
            {
                "ephemerides": [
                    ephemeris.serialize() for ephemeris in self.ephemerides
                ],
                "moon_phase": self.moon_phase.serialize(),
                "events": list(self.get_events()),
            }
        )

    def get_events(self) -> [{str: any}]:
        for event in self.events:
            if event.event_type not in self.SUPPORTED_EVENTS:
                continue

            if event.event_type == EventType.SEASON_CHANGE:
                yield serialize_season_change(event)
            elif event.event_type == EventType.LUNAR_ECLIPSE:
                yield serialize_lunar_eclipse(event)
            else:
                yield event.serialize()
