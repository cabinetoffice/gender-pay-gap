# LoadTests

This is the project for Load Testing the GPG project.

## Creating or modifying a scenario

The scenarios in this project were originally created using the Gatling Recorder. For minor tweaks (e.g. just to a specific page) you can edit `RecordingSimulation.scala` directly, or for a more substantial change you should consider recording a new scenario from scratch. See the [Gatling recorder docs](https://gatling.io/docs/current/quickstart/#using-the-recorder) for more.

## Running a scenario

```
mvn gatling:test
```
or, in IntelliJ, Maven > LoadTests > Plugins > gatling > gatling:test.

For more, see the [Gatling Maven plugin docs](https://gatling.io/docs/current/extensions/maven_plugin).
