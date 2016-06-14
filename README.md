[![Build status](https://ci.appveyor.com/api/projects/status/dhjqe9t4s9vel1uu?svg=true)](https://ci.appveyor.com/project/nelsoncvjr/enjoy-cqrs) 
[![myget status](https://www.myget.org/BuildSource/Badge/enjoy?identifier=1427e6c3-0c6f-4a1a-b5bf-2d02e2ad908c)](https://www.myget.org/BuildSource/Badge/enjoy?identifier=1427e6c3-0c6f-4a1a-b5bf-2d02e2ad908c)

| Test Coverage |                                                                                                                                                                     |
|---------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| master        | [![CoverageStatus](https://coveralls.io/repos/github/ircnelson/enjoy.cqrs/badge.svg?branch=master)](https://coveralls.io/github/ircnelson/enjoy.cqrs?branch=master) |
| dev           | [![CoverageStatus](https://coveralls.io/repos/github/ircnelson/enjoy.cqrs/badge.svg?branch=dev)](https://coveralls.io/github/ircnelson/enjoy.cqrs?branch=dev)       |

# eNJoy CQRS + Event Sourcing
The motivation to create this project was born through the curiosity about CQRS (Command Query Responsibility Separation) & Event Sourcing architecture.

I'm tired to build software using classical architecture, then I decided to start adventure in this world.
The new way of thinking about the area where everything occurs are events, like a timeline in real life is very cool and different!

Obviously, have much paradigms and fears that should be left over the long of the time, but never is too late.

Any suggestion is welcome.

## Features

* Unit of Work
    - You can work with BASE or ACID
* Command dispatcher abstraction
* Event publisher
* Event Store abstraction
* Snapshot (custom strategy implementation)

## Architecture

![CQRS high level architecture](http://s32.postimg.org/ty18uww45/090615_1544_introductio1_png_w_604.png)

## Concept

[![CQRS concept](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8.png)](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8/detail)

### [See wiki for more details](https://github.com/ircnelson/enjoy.cqrs/wiki)
