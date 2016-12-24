[![Build status](https://ci.appveyor.com/api/projects/status/dhjqe9t4s9vel1uu?svg=true)](https://ci.appveyor.com/project/nelsoncvjr/enjoy-cqrs) 
[![myget status](https://www.myget.org/BuildSource/Badge/enjoy?identifier=1427e6c3-0c6f-4a1a-b5bf-2d02e2ad908c)](https://www.myget.org/BuildSource/Badge/enjoy?identifier=1427e6c3-0c6f-4a1a-b5bf-2d02e2ad908c)

# eNJoy CQRS + Event Sourcing
This framework can help you with two things together and easy. 
First, your entities could use event sourcing technique. The second one you could use Command Query Segregation Responsability (CQRS) pattern. 

Any suggestion is welcome.

## Features

* Unit of Work
    - You can work with BASE or ACID
* Command dispatcher abstraction
* Event publisher
* Event Store abstraction
* Snapshot (custom strategy implementation)
* Custom events metadata

## Configure development enviroment

1. Install Docker
2. Pull mongo image. (See https://hub.docker.com/_/mongo/)
	* e.g.: docker run --name srv-mongo -p 27017:27017 -d mongo


* Discovering docker ip:
	* unix: $(ifconfig en0 | awk '/ *inet /{print $2}')


## Event store implementations

* MongoDB: Install-Package EnjoyCQRS.EventStore.MongoDB

## Architecture

![CQRS high level architecture](http://s32.postimg.org/ty18uww45/090615_1544_introductio1_png_w_604.png)

## Concept

[![CQRS concept](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8.png)](http://www.conceptmaps.io/maps/0acfabc1-5e39-4dd7-9590-3b32c2918ec8/detail)

### [See wiki for more details](https://github.com/ircnelson/enjoy.cqrs/wiki)
