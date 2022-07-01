# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.2] - 2022-06-24

* Renamed some functions:
** ServerReadyForPlayersAsync -> ReadyServerForPlayersAsync
** ServerUnreadyAsync -> UnreadyServerAsync
** ConnectToServerCheckAsync -> StartServerQueryHandlerAsync
* Fixed connection to payloadproxy
* Windows builds now uses HOMEPATH instead of HOME to read server.json

## [1.0.0-pre.1] - 2022-03-28

* Initial version of the Multiplay SDK package.
