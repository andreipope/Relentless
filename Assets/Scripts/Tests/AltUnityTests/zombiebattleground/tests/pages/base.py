import os
import unittest
import sys
from appium import webdriver
from altunityrunner import AltrunUnityDriver
import xmlrunner
import subprocess
import time


def PATH(p): return os.path.abspath(
    os.path.join(os.path.dirname(__file__), p)
)


class CZBTests(unittest.TestCase):
    platform = "android"  # set to `ios` or `android` to change platform
    tester_key = "c0ca1ecde904"

    def setUp(self):
        set_platform = None
        try:
            set_platform = os.environ['TESTPLATFORM']
        except:
            print("TESTPLATFORM environemnt variable not set, using default `android`")
        if (set_platform):
            self.platform = set_platform
        self.desired_caps = {}
        if (self.platform == "android"):
            self.setup_android()
        else:
            self.setup_ios()
        self.driver = webdriver.Remote(
            'http://localhost:4723/wd/hub', self.desired_caps)
        self.altdriver = AltrunUnityDriver(self.driver, self.platform)
        self.altdriver.wait_for_current_scene_to_be('APP_INIT')
        try:
            subprocess.Popen(['iproxy', '8100', '8100'])
        except:
            print('tried to forward WebdriverAgent')

    def tearDown(self):
        self.altdriver.stop()
        self.driver.quit()

    def setup_android(self):
        self.desired_caps['platformName'] = 'Android'
        self.desired_caps['deviceName'] = 'device'
        self.desired_caps['app'] = PATH('../../application.apk')
        self.desired_caps['androidInstallTimeout'] = 300000

    def setup_ios(self):
        try:
            udid = os.environ['IOS_UDID']
        except:
            print('ERROR: IOS_UDID environment variable not se. To run the tests locally on iOS, use `export IOS_UDID=xxx` where `xxx` is the UDID of the iOS device you have connected')
        self.desired_caps['platformName'] = 'iOS'
        self.desired_caps['deviceName'] = 'iOSDevice'
        self.desired_caps['automationName'] = 'XCUITest'
        self.desired_caps['app'] = PATH('../application.ipa')

    def wait_for_element_with_tmp_text(self, name, text, camera_name='', timeout=20, interval=0.5, enabled=True):
        t = 0
        alt_element = None
        while (t <= timeout):
            try:
                alt_element = self.altdriver.find_element(name, camera_name)
                if alt_element.get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro') == text:
                    break
                raise Exception('Not the wanted text')
            except Exception:
                print('Waiting for element ' + name + ' to have text ' + text+'actual text ' +
                      alt_element.get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro'))
                time.sleep(interval)
                t += interval
        if t >= timeout:
            raise Exception('Element ' + name + ' should have text `' + text + '` but has `' +
                            alt_element.get_text() + '` after ' + str(timeout) + ' seconds')
        return alt_element


    def check_name_is_in_list(self, list, name):
        for i in list:
            if i == name:
                return True
        return False

    def skip_tutorials(self):
        # self.altdriver.wait_for_element('HiddenUI')
        # self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','true','UnityEngine.CoreModule')
        # self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        # self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn')
        # time.sleep(2)
        # self.altdriver.wait_for_element("SkipTutorial").tap()
        # self.altdriver.wait_for_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')
        questionPopUp=self.altdriver.wait_for_element('QuestionPopup(Clone)')
        self.altdriver.find_element(questionPopUp.name+'/Button_No').mobile_tap()
    def jump_to_tutorial(self, tutorialNumber):
        self.skip_tutorials()
        self.altdriver.wait_for_element('HiddenUI')
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','true','UnityEngine.CoreModule')
        self.altdriver.find_element('InputField').set_component_property('UnityEngine.UI.InputField','text',tutorialNumber,'UnityEngine.UI')
        self.altdriver.find_element('JumpToTutorial').mobile_tap()
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')


    def write_in_input_field(self,input_field,text):
        self.altdriver.wait_for_element(input_field.name).set_component_property('UnityEngine.UI.InputField','text',text,'UnityEngine.UI')
    def write_in_tmp_input_field(self,input_field,text):
        self.altdriver.wait_for_element(input_field.name).set_component_property('TMPro.TMP_InputField','text',text,'Unity.TextMeshPro')
    def button_pressed(self,button):
        button.mobile_tap()
    def read_tmp_UGUI_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
    def read_tmp_GUI_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TextMeshProGUI', 'text', 'Unity.TextMeshPro')
    def read_tmp_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TMP_Text', 'text', 'Unity.TextMeshPro')

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
