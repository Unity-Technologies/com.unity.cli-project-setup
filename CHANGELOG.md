# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.16-preview] - 2022-02-10
* Add empty string/null checks for package metadata to fix bug where no package metadata exists. Add unit tests for these.

## [0.3.15-preview] - 2022-02-10
* Update metadata manager dependency

## [0.3.14-preview] - 2021-11-17
* Add OpenXR configuration

## [0.3.13-preview] - 2021-09-13
* Update metadata manager dependency

## [0.3.12-preview] - 2021-09-09
* Fix folder creation to use AssetDatabase API instead of .NET API

## [0.3.11-preview] - 2021-04-20
* XR support OpenXR fixes
* Added stereorenderingpath option as an alias to stereorenderingmode for backwards compatibility with the cli-config-manager package.

## [0.3.10-preview] - 2021-02-15
* XR support

## [0.3.9-preview] - 2020-11-20

* Update dependency version for com.unity.cli-project-setup

## [0.3.8-preview] - 2020-11-16

* Ensure VR setup is guarded using ENABLE_VR

## [0.3.7-preview] - 2020-11-11

* Fix where vsync level was not being set for each Quality Setting
* Enable "scripting-backend" or "scriptingbackend" for easier matching
* Move IL2CPP as default scripting backend logic closer to method where it's set

## [0.3.6-preview] - 2020-11-05

* Simplify adding test scenes to build settings

## [0.3.5-preview] - 2020-11-05

* Update to use metadata manager, use singleton currentsettings

## [0.3.4-preview] - 2020-10-01

* Don't set api compatibility level by default; perservice setting in project.

## [0.3.3-preview] - 2020-10-01

* Update com.unity.test.performance.runtimesettings dependency to 0.2.1-preview

## [0.3.2-preview] - 2020-09-24

* Update com.unity.test.performance.runtimesettings dependency to 0.2.0-preview

## [0.3.1-preview] - 2020-09-24

* Bug fix for line ending
* Remove empty array field in asmdef causing upm-ci error

## [0.3.0-preview] - 2020-09-24

* Remove XrConfigurator

## [0.2.0-preview] - 2020-09-23

* Change asmdef name

## [0.1.0-preview] - 2019-06-18

### This is the first release of *Unity Package \com.unity.cli-config-manager*.

