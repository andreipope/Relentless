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


class CZBTests():
    platform = "android"  # set to `ios` or `android` to change platform
    tester_key = "c0ca1ecde904"

    def setUp(self):
        print('Hello')
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
            # self.get_android_device_screen_size()
        else:
            self.setup_ios()
        self.driver = webdriver.Remote(
            'http://localhost:4723/wd/hub', self.desired_caps)
        self.get_appium_device_screen_size()
        self.altdriver = AltrunUnityDriver(self.driver, self.platform,screen_height=self.device_screen_height,screen_width=self.device_screen_width)
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
        self.desired_caps['orientation']='LANDSCAPE'

    def setup_ios(self):
        try:
            udid = os.environ['IOS_UDID']
        except:
            print('ERROR: IOS_UDID environment variable not se. To run the tests locally on iOS, use `export IOS_UDID=xxx` where `xxx` is the UDID of the iOS device you have connected')
        self.desired_caps['platformName'] = 'iOS'
        self.desired_caps['deviceName'] = 'iOSDevice'
        self.desired_caps['automationName'] = 'XCUITest'
        self.desired_caps['app'] = PATH('../application.ipa')
        self.desired_caps['orientation']='LANDSCAPE'
    
    def get_appium_device_screen_size(self):
        size=self.driver.get_window_size()
        print(size)
        self.device_screen_height=size['height']
        self.device_screen_width=size['width']
        print(self.device_screen_height)
        print(self.device_screen_width)


    def get_android_device_screen_size(self):
        # The output of the command looks like this:
        # Physical size: 1440x2560
        # Override size: 1080x1920
        # Also it can only have just the first line if the device uses the physical size resolution

        sub = subprocess.Popen(['adb','shell','wm','size'],
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT)
        commant_output=sub.stdout.read()
        if('error:' in commant_output):
            print(commant_output)
        else:
            splited_text=commant_output.split("\n")
            print(splited_text)
            if(len(splited_text)==3):
                screen_size=splited_text[1].split(" ")[2]
            else:
                screen_size=splited_text[0].split(" ")[2]
            screen_size_splited=screen_size.split("x")
            self.device_screen_width=int(screen_size_splited[1])
            self.device_screen_height=int(screen_size_splited[0])

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
        time.sleep(1)
        self.altdriver.find_element('JumpToTutorial').tap()
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')
    def jump_to_tutorial_from_another_tutorial(self,tutorialNumber):
        self.altdriver.wait_for_element('HiddenUI')
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','true','UnityEngine.CoreModule')
        self.altdriver.find_element('SkipTutorial').mobile_tap()
        self.altdriver.find_element('InputField').set_component_property('UnityEngine.UI.InputField','text',tutorialNumber,'UnityEngine.UI')
        self.altdriver.find_element('JumpToTutorial').mobile_tap()
        time.sleep(1)
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')


    def write_in_input_field(self,input_field,text):
        self.altdriver.wait_for_element(input_field.name).set_component_property('UnityEngine.UI.InputField','text',text,'UnityEngine.UI')
    def write_in_tmp_input_field(self,input_field,text):
        self.altdriver.wait_for_element(input_field.name).set_component_property('TMPro.TMP_InputField','text',text,'Unity.TextMeshPro')
    def button_pressed(self,button):
        button.mobile_tap()
    def double_tap(self,alt_element):
        alt_element.mobile_tap(0.2)
        alt_element.mobile_tap(0.2)
    def read_tmp_UGUI_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
    def read_tmp_GUI_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TextMeshProGUI', 'text', 'Unity.TextMeshPro')
    def read_tmp_text(self,alt_element):
        return alt_element.get_component_property('TMPro.TMP_Text', 'text', 'Unity.TextMeshPro')

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
