|           | master                                                                                                                                                      | dev                                                                                                                                                          |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor  | [![master](https://ci.appveyor.com/api/projects/status/dhjqe9t4s9vel1uu?svg=true)](https://ci.appveyor.com/project/nelsoncvjr/enjoy-cqrs)                   | [![dev](https://ci.appveyor.com/api/projects/status/dhjqe9t4s9vel1uu/branch/dev?svg=true)](https://ci.appveyor.com/project/nelsoncvjr/enjoy-cqrs/branch/dev) |
| Travis CI | [![master](https://travis-ci.org/ircnelson/enjoy.cqrs.svg?branch=master)](https://travis-ci.org/ircnelson/enjoy.cqrs)                                       | [![dev](https://travis-ci.org/ircnelson/enjoy.cqrs.svg?branch=dev)](https://travis-ci.org/ircnelson/enjoy.cqrs)                                              |
| Coverage  | [![master](https://coveralls.io/repos/github/ircnelson/enjoy.cqrs/badge.svg?branch=master)](https://coveralls.io/github/ircnelson/enjoy.cqrs?branch=master) | [![dev](https://coveralls.io/repos/github/ircnelson/enjoy.cqrs/badge.svg?branch=dev)](https://coveralls.io/github/ircnelson/enjoy.cqrs?branch=dev)           |
| Nuget     | ![nuget.org](https://buildstats.info/nuget/enjoycqrs)                                                                                                       | ![myget](https://buildstats.info/myget/enjoy/enjoycqrs)                                                                                                      |

# eNJoy CQRS + Event Sourcing
This framework can help you with two things together and easy. 
First, your entities could use event sourcing technique. The second one you could use Command Query Segregation Responsability (CQRS) pattern. 

Any suggestion is welcome.

## Features

* Unit of Work
* Command dispatcher abstraction
* Event publisher
* Event Store abstraction
* Snapshot (custom strategy implementation)
* Custom events metadata

## Configure development enviroment

1. Install MongoDB
2. Set environment variable called 'MONGODB_HOST' with MongoDB's IP/HOST

## Event store implementations

* MongoDB: Install-Package EnjoyCQRS.EventStore.MongoDB

## Architecture

![CQRS high level architecture](http://s32.postimg.org/ty18uww45/090615_1544_introductio1_png_w_604.png)

## Concept

[![CQRS concept](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8.png)](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8/detail)

### [See wiki for more details](https://github.com/ircnelson/enjoy.cqrs/wiki)
