import unittest
from appium import webdriver
from altunityrunner import AltrunUnityDriver,NotFoundException
import xmlrunner
from base import CZBTests
import time
import datetime


class CZBLoginTests(CZBTests):
    def setUp(self):
        super(CZBLoginTests, self).setUp()

        self.altdriver.wait_for_element('HiddenUI')
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','true','UnityEngine.CoreModule')
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn')
        time.sleep(2)
        self.altdriver.wait_for_element("SkipTutorial").tap()
       
        time.sleep(1)
        self.altdriver.find_element('Root',enabled=True).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')

    def test_login_with_fake_account(self):

        self.altdriver.wait_for_element('Button_Login').mobile_tap()
        self.altdriver.wait_for_element('LoginPopup(Clone)')
        self.altdriver.wait_for_element('Email_InputField').set_component_property('UnityEngine.UI.InputField','text','fakeAccount@mailinator.com','UnityEngine.UI')
        self.altdriver.wait_for_element('Password_InputField').set_component_property('UnityEngine.UI.InputField','text','password123','UnityEngine.UI')

        self.altdriver.wait_for_element('Button_Login_BG/Button_Login').mobile_tap()

        expectedMessage='The process could not be completed with error:\n The Username and/or Password are not correct. \n\nPlease try again.'
        actualMessage=self.altdriver.wait_for_element('Canvas3/WarningPopup(Clone)/Text_Message').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
        
        # self.assertEqual(expectedMessage,actualMessage)
        self.altdriver.wait_for_element('Button_GotIt').mobile_tap()
        self.altdriver.find_element('LoginPopup(Clone)')


    def test_login_with_good_account(self):
        self.altdriver.wait_for_element('Button_Login').mobile_tap()
        self.altdriver.wait_for_element('LoginPopup(Clone)')
        self.altdriver.wait_for_element('Email_InputField').set_component_property('UnityEngine.UI.InputField','text','goodTestAccount@testEmail.test','UnityEngine.UI')
        self.altdriver.wait_for_element('Password_InputField').set_component_property('UnityEngine.UI.InputField','text','password123','UnityEngine.UI')

        self.altdriver.wait_for_element('Button_Login_BG/Button_Login').mobile_tap()
        self.altdriver.wait_for_element_to_not_be_present('LoginPopup(Clone)')
        try:
            self.altdriver.find_element('Button_Login')
            self.assertTrue(False)
        except NotFoundException:
            self.assertTrue(True)

        
    
    def test_send_registration_request(self):
        self.altdriver.wait_for_element('Button_Login').mobile_tap()
        self.altdriver.wait_for_element('LoginPopup(Clone)')
        self.altdriver.wait_for_element('Button_Register_BG/Button_Register').mobile_tap()
        self.altdriver.wait_for_element('Register_Group')
        fakeEmail='testAccount'+str(datetime.datetime.now().time())+'@testsonbitbar.com'
        self.altdriver.wait_for_element('Register_Group/Email_BG/Email_InputField').set_component_property('UnityEngine.UI.InputField','text',fakeEmail,'UnityEngine.UI')
        self.altdriver.wait_for_element('Register_Group/Password_BG/Password_InputField').set_component_property('UnityEngine.UI.InputField','text','password123','UnityEngine.UI')
        self.altdriver.wait_for_element('Register_Group/Confirm_BG/Confirm_InputField').set_component_property('UnityEngine.UI.InputField','text','password123','UnityEngine.UI')
        self.altdriver.wait_for_element('Register_Group/Button_Register_BG/Button_Register').mobile_tap()
        self.altdriver.wait_for_element_to_not_be_present('LoginPopup(Clone)')
        try:
            self.altdriver.find_element('Button_Login')
            self.assertTrue(False)
        except NotFoundException:
            self.assertTrue(True)
    
    def test_send_forgot_password_request(self):
        self.altdriver.wait_for_element('Button_Login').mobile_tap()
        self.altdriver.wait_for_element('LoginPopup(Clone)')
        self.altdriver.wait_for_element('Button_ForgotPassword').mobile_tap()
        self.altdriver.wait_for_element('Forgot_Group')
        self.altdriver.wait_for_element('Forgot_Group/Email_BG/Email_InputField').set_component_property('UnityEngine.UI.InputField','text','goodTestAccount@testEmail.test','UnityEngine.UI')
        self.altdriver.wait_for_element('Forgot_Group/Button_Send_BG/Button_Send').mobile_tap()
        self.altdriver.wait_for_element('Waiting_Group')

        self.altdriver.wait_for_element('SuccessForgot_Group')

        expectedMessage='Success! Go check your Email'
        actualMessage=self.altdriver.wait_for_element('SuccessForgot_Group/Title_Text').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
        self.assertEqual(expectedMessage,actualMessage)

        expectedMessage='We just sent you a unique link to reset your password.\nGo ahead and click that link to get back your account.\nAnd welcome back to Zombie Battleground!'
        actualMessage=self.altdriver.wait_for_element('SuccessForgot_Group/Desc_Text').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
        self.assertEqual(expectedMessage,actualMessage)

        expectedMessage='(Note: Double-check your spam folder and "Promotions" tab if you don\'t see the email.)'
        actualMessage=self.altdriver.wait_for_element('SuccessForgot_Group/Note_Text').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
        self.assertEqual(expectedMessage,actualMessage)




if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
