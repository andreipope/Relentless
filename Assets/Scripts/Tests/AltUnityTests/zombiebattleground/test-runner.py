import unittest
import xmlrunner
import os

reports_directory = './test-reports/'

try:
    for test_xml in os.listdir(reports_directory):
        os.remove(reports_directory + test_xml)
except:
    print "test-reports directory not present, no need to delete it..."

loader = unittest.TestLoader()
start_dir = 'tests/'
suite = loader.discover(start_dir, '*_tests.py')
runner = xmlrunner.XMLTestRunner(output='test-reports')
runner.run(suite)