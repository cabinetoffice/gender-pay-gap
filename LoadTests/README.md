# LoadTests

This is the module for Load Testing the GPG project.

## Opening this module

This module can't be opened in Visual Studio so you'll need another IDE such as IntelliJ.

1. Download and install [IntelliJ](https://www.jetbrains.com/idea/download) (Community edition is fine)
1. Open just this folder (./LoadTests) in IntelliJ
1. Set up your JDK if you didn't already - you'll need to see a valid Java 1.8 under File > Project Structure > Project SDKs
1. Install the Scala plugin: File > Settings > Plugins > search for Scala and hit Install
1. Maven should automatically sync, but if it doesn't, open the Maven window and hit "Reimport all Maven projects"

You should now be able to run
```
mvn gatling:verify
```
and have it succeed.

## Creating or modifying a scenario

The scenarios in this project were originally created using the Gatling Recorder. For minor tweaks (e.g. just to a specific page) you can edit `RecordingSimulation.scala` directly, or for a more substantial change you should consider recording a new scenario from scratch. See the [Gatling recorder docs](https://gatling.io/docs/current/quickstart/#using-the-recorder) for more.

## Running a scenario

```
mvn gatling:test
```
or, in IntelliJ, Maven > LoadTests > Plugins > gatling > gatling:test.

For more, see the [Gatling Maven plugin docs](https://gatling.io/docs/current/extensions/maven_plugin).
