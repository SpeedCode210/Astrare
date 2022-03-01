# ![Kosmorro](https://raw.githubusercontent.com/Kosmorro/logos/main/png/kosmorro-logo-grey.png)
## Kosmorro Lite

A program that calculates your astronomical ephemerides!

It's a minimal version of [Kosmorro](https://github.com/Kosmorro/kosmorro) which only outputs json, it's made for be used as an interface with [Kosmorrolib](https://github.com/Kosmorro/lib) from programs that are written with other programming languages than Python.

## Installation

### Production environment

If you want to give a try to Kosmorro, head to [its official download page](https://kosmorro.space/cli/download/) and follow the instructions that correspond to your operating system.

### Development environment

Before you run Kosmorro in your development environment, check you have installed these programs on your system:

- Python ≥ 3.7.0 (needed run Kosmorro)
- PIP3 (needed for package management, usually installed among with Python 3)
- [Pipenv](https://pypi.org/project/pipenv/) (needed to manage the virtual environment)

Clone this repository and run `pipenv sync` to install all the dependencies.
Then, run Kosmorro by invoking `pipenv run python kosmorro-lite`.

For comfort, you may want to invoke `pipenv shell` first and then just `python kosmoro-lite`.

## Using Kosmorro

Using Kosmorro is as simple as invoking `kosmorro-lite` in your terminal!

By default, it will give you the current Moon phase and, if any, the events that will occur today.
To get the rise, culmination and set of the objects of the Solar system, you will need to give it your position on Earth: get your current coordinates (with [OpenStreetMap](https://www.openstreetmap.org) for instance), and give them to Kosmorro by invoking it with the following parameters: `--latitude=X --longitude=Y` (replace `X` by the latitude and `Y` by the longitude).

Kosmorro has a lot of available options. To get a list of them, run `kosmorro-lite --help`, or read its manual with `man kosmorro-lite`.

Note: the first time it runs, Kosmorro will download some important files needed to make the computations. They are stored in a cache folder named `.kosmorro-cache` located in your home directory (`/home/<username>` on Linux, `/Users/<username>` on macOS).


