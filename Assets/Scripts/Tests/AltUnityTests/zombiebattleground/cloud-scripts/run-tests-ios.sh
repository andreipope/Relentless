#!/bin/bash
export TESTPLATFORM=ios
# Name of the test file
TEST=${TEST:="test-runner.py"}

echo "Extracting tests.zip..."
unzip -o tests.zip


#########################################################
#
# Intalling PIP and required Python libraries
#  - required libraries are in requirement.txt file that
#    is uploaded with this script file
#
#########################################################

echo "Installing pip"
curl https://bootstrap.pypa.io/get-pip.py | python - --user

echo "Exporting PATH for pip"
export PATH=/Users/testdroid/Library/Python/2.7/bin:${PATH}
echo "New PATH: ${PATH}"

echo "Installing requirements from requirements.txt"
chmod 0755 requirements.txt
pip install --user  --requirement requirements.txt

sudo pip uninstall --yes altunityrunner || true
sudo pip install -e "altunitybindings"

#########################################################
#
# Preparing to start Appium
# - UDID is the device ID on which test will run and
#   required parameter on iOS test runs
# - appium - is a wrapper tha calls the latest installed
#   Appium server. Additional parameters can be passed
#   to the server here.
#
#########################################################

echo "UDID set to ${IOS_UDID}"
echo "Starting Appium ..."
appium -U ${IOS_UDID}  --log-no-colors --log-timestamp --show-ios-log --command-timeout 120


#########################################################
#
# Setting of environment variables used later in test
# - used for Appium desired capabilities
# - note, APPIUM_URL is same for local and cloud server
#   runs
#########################################################
export APPIUM_APPFILE="$PWD/application.ipa"
export APPIUM_URL="http://localhost:4723/wd/hub"
export APPIUM_DEVICE="Local Device"
export APPIUM_PLATFORM="IOS"
export APPIUM_AUTOMATION="XCUITest"

## Clean local screenshots directory
rm -rf screenshots

## Start test execution
echo "Running tests..."

python ${TEST}


./combine-junit-xml.sh -i test-reports -o TEST-all.xml