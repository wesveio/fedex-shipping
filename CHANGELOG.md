# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- (FEDEXSHIP-48) Added option to set a custom delivery estimate if not provided by FedEx
- Added Kuwait to country list

## [1.19.2] - 2022-11-24

## [1.19.1] - 2022-11-23

### Changed
- (FEDEXSHIP-44) Changed transit time calculation to use current date instead of ship date

### Changed
- GitHub reusable workflow and Cy-Runner updated to version 2

## [1.19.0] - 2022-10-11

### Added

- Moved cypress tests inside cy-runner

### Added

- (FEDEXSHIP-43) App key and Token required for Get Rates API

## [1.18.4] - 2022-09-16

### Added

- (FEDEXSHIP-41) Logging optimization
- (FEDEXSHIP-41) GraphQL mutation security
- (FEDEXSHIP-41) Admin navigation permissions

## [1.18.3] - 2022-09-06

### Added

- Added more postal code regex validations.

## [1.18.3] - 2022-09-06

### Added

- Added more postal code regex validations.

## [1.18.2] - 2022-08-03

### Fixed

- Fixed a bug where cache was never saving

### Added

- Redid caching
- Improved Cache invalidation strategy to O(log(N))

## [1.18.1] - 2022-07-29

### Addeed

- Updated README

## [1.18.0] - 2022-07-25

### Added

- Added test credentials

## [1.17.2] - 2022-07-05

### Added

- Added parallel processing for FedEx API

## [1.17.1] - 2022-06-14

### Added

- Added freight shipping

## [1.17.0] - 2022-06-13

### Added

- Added support for the following countries: Mexico, Brazil, Britain, Canada, France, Italy, and Germany

## [1.16.0] - 2022-06-06

### Added

- Added test key for smart packing

## [1.15.1] - 2022-06-02

### Added

- Read Me Documentation

## [1.15.0] - 2022-05-31

### Added

- Added packing app integration

## [1.14.1] - 2022-05-27

### Fixed

- Fixed a bug with unit conversion and grams

## [1.14.0] - 2022-05-20

### Added

- Removed static carrier param and enabled conditional logs for beta

## [1.13.0] - 2022-05-19

### Added

- Added multiple tabs to admin page

## [1.12.0] - 2022-05-16

### Added

- Allow users to connect docks to the app
- Allow users to remove docks from the app

## [1.11.0] - 2022-05-11

### Added

- Added caching for the getRates function

## [1.10.0] - 2022-05-09

### Added

- Added a new table for surcharges and hiding SLAs

## [1.9.2] - 2022-05-05

### Added

- Added grams as a unit of measurement

## [1.9.1] - 2022-05-03

### Fixed

- Resolved SonarCloud security concerns

### Changed

- Added auth to getSettings and setSettings

## [1.9.0] - 2022-05-03

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
