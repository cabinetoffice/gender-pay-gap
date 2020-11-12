# LoadTests

We use [Gatling](https://gatling.io/) for the purpose of Load Testing. Gatling is an open-source load and performance testing framework based on Scala, Akka and Netty.

## Zero to Hero

This module can't be opened in Visual Studio so you'll need another IDE such as IntelliJ.

1. Download and install [IntelliJ](https://www.jetbrains.com/idea/download) (Community edition is fine)
1. Install **Java** 64bits JDK 8 (not compatible with JDK 12+)
1. Set up your JDK if you didn't already - you'll need to see a valid Java 1.8 under File > Project Structure > Project SDKs
1. Install **Scala** 2.12 (not compatible with Scala 2.11 or Scala 2.13)
1. Install the Scala plugin for IntelliJ: File > Settings > Plugins > search for Scala and hit Install
1. Increase the stack size for the Scala compiler so you don't suffer from StackOverflowErrors. Recommended setting Xss to 100M.  (File -> Settings -> Build, Execution, Deployment -> Compiler -> Scala Compiler -> Scala Compile Server -> in the JVM options textbox)
1. Open just this folder (./LoadTests) in IntelliJ
1. Maven should automatically sync, but if it doesn't, open the Maven window and hit "Reimport all Maven projects" (the "refresh" icon)

You should now be able to run
```
mvn gatling:verify
```
and have it succeed.

### Creating or modifying a scenario

The scenarios in this project were originally created using the Gatling Recorder. For minor tweaks (e.g. just to a specific page) you can edit `RecordingSimulation.scala` directly, or for a more substantial change you should consider recording a new scenario from scratch. See the [Gatling recorder docs](https://gatling.io/docs/current/quickstart/#using-the-recorder) for more.

## Running the load test

### Prep

Don't run load tests against Live. Ideally, create a completely fresh environment to run your load tests against and collapse it when you're done.

* Bring the `load-test` branch up to date with master.
* Set up a new environment in GOV.UK PaaS, building off the `load-test` branch and using `Web.loadtest.Config`.
* Set the `TESTING-SkipSpamProtection` config var to True
* Run the sql script `SET_UP_TEST_ENTITIES.sql` against the chosen environment database to initialise some test users. You can run this script either using your Visual Studio/Razor or from the [CloudFoundry CLI](https://github.com/alphagov/paas-cf-conduit).

### Execution
* Run the test:

```
mvn gatling:test
```

* or, in IntelliJ, Maven > LoadTests > Plugins > gatling > gatling:test (see the [Gatling Maven plugin docs](https://gatling.io/docs/current/extensions/maven_plugin))
 * or right click on `Engine` and run it.

You can find the results report in `LoadTests/target/gatling`.

### Changing number of test users

The current version of the sql script and the tests are written for 1000 test users. If you want to change this number to <x>:

* Change the initial value of `@NUM_OF_USERS` in the sql script to be <x>
* Change the initial value of `MAX_NUM_USERS` in RecordingSimulation to be <x>
* Change the `users_organisations.csv` to include
  * user1@example.com through user<x>@example.com, AND
  * test_<x+1> to test_<2x>, AND
  * 99999<x+1> to 99999<2x>

### Results

The results of past load tests, along with expected peak usage, are on the wiki. Ask a member of the team for access.
