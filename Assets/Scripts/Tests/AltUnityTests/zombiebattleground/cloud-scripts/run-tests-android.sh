#!/bin/bash
export TESTPLATFORM=android
TEST=${TEST:="test-runner.py"} #Name of the test file
# TEST=${TEST:="tests/gameplay_tests_in_progress.py"} #Name of the test file

##### Cloud testrun dependencies start
echo "Extracting tests.zip..."
unzip tests.zip

echo "Installing pip for python"
curl https://bootstrap.pypa.io/get-pip.py -o get-pip.py
sudo python get-pip.py

echo "Installing Appium Python Client 0.24 and xmlrunner 1.7.7"
chmod 0755 requirements.txt
sudo pip install -r requirements.txt

sudo pip uninstall --yes altunityrunner || true
sudo pip install -e "altunitybindings"

## AltUnityTester - Forward abd port from device
echo "Forwarding AltUnityTester port 13000 from device to localhost..."
adb forward tcp:13000 tcp:13000

echo "Starting Appium ..."

appium --log-no-colors --log-timestamp

ps -ef|grep appium
##### Cloud testrun dependencies end.

export APPIUM_APPFILE=$PWD/application.apk #App file is at current working folder



## Desired capabilities:
export APPIUM_URL="http://localhost:4723/wd/hub" # Local & Cloud
export APPIUM_DEVICE="Local Device"
export APPIUM_PLATFORM="android"

APILEVEL=$(adb shell getprop ro.build.version.sdk)
APILEVEL="${APILEVEL//[$'\t\r\n']}"
echo "API level is: ${APILEVEL}"

## APPIUM_AUTOMATION 
if [ "$APILEVEL" -gt "16" ]; then
  echo "Setting APPIUM_AUTOMATION=Appium"
  export APPIUM_AUTOMATION="Appium"
else
  echo "Setting APPIUM_AUTOMATION=selendroid"
  export APPIUM_AUTOMATION="Selendroid"
fi

## Run the test:
echo "Running tests..."
rm -rf screenshots
mkdir screenshots

echo $TEST_CLOUD
echo ${TEST_CLOUD}

python ${TEST}


./combine-junit-xml.sh -i test-reports -o TEST-all.xml