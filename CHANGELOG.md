# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added a Residential Toggle

## [1.8.4] - 2022-04-29

### Changed

- Added a timeLapse field to logs for easier Splunk querying

## [1.8.3] - 2022-04-26

### Changed

- Improved admin panel styling

## [1.8.2] - 2022-04-22

### Fixed

- Fixed incorrect weight units

## [1.8.1] - 2022-04-20

### Changed

- Added helper functions and reduced repeated code

## [1.8.0] - 2022-04-19

### Added

- Added ship alone logic

### Fixed

- Fixed a bug where NONE in modal maps does not align with empty item modals

## [1.7.0] - 2022-04-15

### Added

- Added optimize packaging by packing items to the biggest box
- Added killswitch for optimizing packaging

## [1.6.0] - 2022-04-14

### Added

- Added split requests for different FedEx handling types

## [1.5.0] - 2022-04-12

### Added

- Added modal mapping

## [1.4.0] - 2022-04-08

### Added

- Added a selection to choose what services are returned

## [1.3.0] - 2022-04-07

### Added

- Added admin panel
- Added weight/dimension units settings

## [1.2.1] - 2022-03-31

### Added

- Added logging for getRates
- Added time spent metrics for getRates

## [1.2.0] - 2022-03-25

### Added

- Added rate splits for multiple items

## [1.1.0] - 2022-03-24

### Added

- Added estimate date

## [1.0.0] - 2022-03-22

## [0.2.0] - 2022-03-11

## [0.1.0] - 2022-03-09

### Added

- Added modal mapping for various goods types

## [0.0.6] - 2022-03-09

### Added

- Modified existing request/response to accomodate dynamic rate hub request/response
- Allowed rounding for length, width, and height
- Modified the routing to connect with the dynamic rates hub
- Added ISO Mapping for USA
- Added zipcode trimming

### Fixed

- Fixed a bug where Width was taking the Weight value
