[![Build status](https://ci.appveyor.com/api/projects/status/eprwudlh0eskijof?svg=true)](https://ci.appveyor.com/project/nelsoncvjr/enjoy-cqrs)

# eNJoy CQRS
The motivation to create this project was born through the curiosity about CQRS (Command Query Responsibility Separation) & Event Sourcing architecture.

I'm tired to build software using classical architecture, then I decided to start adventure in this world.
The new way of thinking about the area where everything occurs are events, like a timeline in real life is very cool and different!

Obviously, has much paradigms and fears that should be left over the long of the time, but never is too late.

The implementation is not complete, e.g. need to implement the abstraction for Snapshot.

Any suggestion is welcome.

## Features

* Direct Bus (publish Commands & Events with transaction)
* Unit of Work

## Architecture

![CQRS high level architecture](http://s32.postimg.org/ty18uww45/090615_1544_introductio1_png_w_604.png)

## Installing (or restore) by Nuget

If you don't have a nuget.exe installed, please download [here](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

to install: 

    nuget.exe install EnjoyCQRS -Source https://www.myget.org/F/enjoy/api/v3/index.json

to restore:

    nuget.exe restore EnjoyCQRS -Source https://www.myget.org/F/enjoy/api/v3/index.json

## References
### People
* Michael Plöd [@bitboss](https://twitter.com/bitboss)
* Greg Young [@gregyoung](https://twitter.com/gregyoung)
* Mark Nijhof [@MarkNijhof](https://twitter.com/MarkNijhof)

### Presentations
* http://www.infoq.com/presentations/microservices-event-sourcing-cqrs
* https://speakerdeck.com/mploed/microservices-love-domain-driven-design-why-and-how
* https://www.innoq.com/de/talks/2016/02/topconf2016-microservices-event-sourcing/
* http://pt.slideshare.net/dbellettini/cqrs-and-event-sourcing-with-mongodb-and-php
* http://pt.slideshare.net/ziobrando/loosely-coupled-complexity-unleash-the-power-of-your-domain-model-with-command-query-responsibility-segregation-and-event-sourcing
* http://ookami86.github.io/event-sourcing-in-practice/index.html

### Blogs
* http://danielwhittaker.me/
* http://vadimcomanescu.net/

### Projects
* https://github.com/edumentab/cqrs-starter-kit
* https://github.com/mastoj/CQRSShop
* https://github.com/EventStore/getting-started-with-event-store
* https://github.com/gregoryyoung/m-r/
* https://github.com/MarkNijhof/Fohjin
* https://github.com/Ookami86/event-sourcing-in-practice
