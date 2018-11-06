# ZombieBattleground Tests

This is a small proof-of-concept project to show the use of AltUnityTester to create Python automated tests for ZombiesBattleground.


## Setup 

### Configure game to run with AltUnityTester 

AltUnityTester is an open-source solution for Unity Automation available here: https://gitlab.com/altom/altunitytester

It has been included in this repository, under Plugins, but can be added either from the Gitlab link or from Unity Asset store. 

AltUnityTester works by adding a prefab to your initial scene and opening a TCP socket while the game is running to listen to commands from AltUnityTester clients and to send information about exising elements. 

To create a build with AltUnityTester included for testing purposes, follow the following steps:

1. Add the `AltUnityRunnerPrefab` prefab from `Plugins\AltUnityTester\Prefab\ to APP_INIT scene
2. Select the prefab in the scene and deselect the "Debug Build Needed` checkmark
3. Open Player Setttings and go to "Scripting Defined Symbols"
4. Add "ALTUNITYTESTER" as a definded symbol
5. Build the game (from Utility/Build)
6. Remove the "ALTUNITYTESTER" from Scripting Defined Symbols and remove the prefab from the scene
7. Use the built gmae for testing with Appium or in the cloud

### Apppium

IF you want to run tests locally, you need to install and configure Appium. Follow the instructions here: http://appium.io/docs/en/about-appium/getting-started/ 

Then use the `requirements.txt` to install all Python dependencies. 

## Running tests locally

Before running the tests, you need to have an Appium server started locally, and a device connected to your machine. 

By default, the tedts are run on Android. To switch to iOS, export the following 2 environment variables:

`export TESTPLATFORM=ios`

`export IOSUDID=udid-of-connected-ios`

You can run any of the test suites under `tests` by running:

`python test_file.py`

If you want to run all the tests at once, use:

`python test-runner.py` 

from the main folder. 

## Running tests in Testdroid Cloud

### Creating the test package

If you want to run all the tests in the suite, edit `run-tests-android.sh` (or `run-tests-ios.sh`) and set `TEST=${TEST:="test-runner.py"}` in the beginning of the file. If you want to run a specific suite only, edit the same file and set `TEST=${TEST:="tests/tutorial_tests.py"}` for example, to run the tutorial tests only. 


  1. From the command line, cd into the `cloud-scripts` folder (the scripts won't work from outside the folder)
  2. Use the `./create-cloud-zip.sh android` or `./create-cloud-zip.sh ios` to create the package
  3. A `test-package-android.zip` or `test-package-ios.zip` will be created. This is the zip file you need to upload to Testdroid cloud

### Running tests

   1. Go to https://cloud.bitbar.com/ and login
   2. Go to Projects and choose either eithter the Android or iOS projects by selecting them from the left side. 
   3. Click the + button on the rigth corner to create a new test run. 
   4. On the test run creator page, go to step 3 - Choose files and click on "Click to choose or upload files"
   5. Upload the `test-package-android.zip` ( or `test-package-ios.zip`) file that you created above and also upload the .ipa or .apk file if you want to upload a new version. 
   6. Select both the zip file and the wanted ipa or apk file (have the Command/Control key pressed and click on each file in turn)
   7. Press "Use selected" to return to the Test Run Creator page
   8. Choose the devices that you want to run on
   9. Click on Create new test run 