#!/bin/bash

function print_help() {
    echo "Usage: $0 <ios|android>"
    exit 0
}

if [[ "$#" -eq 1 ]]; then
    PLATFORM="$1"
    rm test-package-${PLATFORM}.zip
else
    print_help
fi

TEST_FILE="test-package-${PLATFORM}.zip"

echo "Creating test file for platform: ${PLATFORM}"
cp run-tests-${PLATFORM}.sh run-tests.sh
zip -r "${TEST_FILE}" requirements.txt ../altunitybindings ../tests/*.py ../tests/pages/*.py ../*.py combine-junit-xml.sh run-tests.sh
echo "You should now upload test file '${TEST_FILE}' to Bitbar Cloud"