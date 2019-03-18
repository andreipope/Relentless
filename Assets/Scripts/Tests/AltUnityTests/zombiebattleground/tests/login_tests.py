import datetime
import time
import unittest

import xmlrunner
from appium import webdriver

from altunityrunner import AltrunUnityDriver, NotFoundException
from area_bar_popup_page import Area_Bar_Popup_Page
from base import CZBTests
from forgot_password_page import Forgot_Password_Page
from login_popup_page import Login_Popup_Page
from main_menu_page import Main_Menu_Page
from register_popup_page import Regitration_Popup_Page
from succes_forgot_page import Succes_Forgot_Page
from wait_page import Wait_Page


class CZBLoginTests(CZBTests):
    def setUp(self):
        super(CZBLoginTests, self).setUp()
        self.skip_tutorials()
        
        

    def test_login_with_fake_account(self):
        Area_Bar_Popup_Page(self.altdriver).press_login_button()
        Login_Popup_Page(self.altdriver).login('fakeAccount@testsonbitbar.com','password123')

        expectedMessage='The process could not be completed with error:\n The Username and/or Password are not correct. \n\nPlease try again.'
        actualMessage=self.altdriver.wait_for_element('Canvas3/WarningPopup(Clone)/Text_Message').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
        
        # self.assertEqual(expectedMessage,actualMessage)
        self.altdriver.wait_for_element('Button_GotIt').mobile_tap()
        self.altdriver.find_element('LoginPopup(Clone)')


    def test_login_with_good_account(self):
        Area_Bar_Popup_Page(self.altdriver).press_login_button()
        Login_Popup_Page(self.altdriver).login('secondTestAccount@testsonbitbar.com','password123')
        Wait_Page(self.altdriver)

        try:
            self.altdriver.find_element('Button_Login',)
            self.assertTrue(False,"Failed to login")
        except NotFoundException:
            self.assertTrue(True)

        
    
    def test_send_registration_request(self):
        Area_Bar_Popup_Page(self.altdriver).press_login_button()
        Login_Popup_Page(self.altdriver).go_to_registration_form()
        fakeEmail='testAccount'+str(datetime.datetime.now().time())+'@testsonbitbar.com'
        Regitration_Popup_Page(self.altdriver).register(fakeEmail,'password123','password123')

        self.altdriver.wait_for_element_to_not_be_present('LoginPopup(Clone)')
        try:
            self.altdriver.find_element('Button_Login')
            self.assertTrue(False)
        except NotFoundException:
            self.assertTrue(True)
    
    def test_send_forgot_password_request(self):
        Area_Bar_Popup_Page(self.altdriver).press_login_button()
        Login_Popup_Page(self.altdriver).go_to_forgot_password_form()
        Forgot_Password_Page(self.altdriver).forgot_password('goodTestAccount@testsonbitbar.com')

        succes_forgot_page=Succes_Forgot_Page(self.altdriver)
        
        expectedMessage='Success! Go check your Email'
        actualMessage=succes_forgot_page.read_tmp_UGUI_text(succes_forgot_page.title_text)
        self.assertEqual(expectedMessage,actualMessage)

        expectedMessage='We just sent you a unique link to reset your password.\\nGo ahead and click that link to get back your account.\\nAnd welcome back to Zombie Battleground!'
        actualMessage=succes_forgot_page.read_tmp_UGUI_text(succes_forgot_page.desc_text)
        self.assertEqual(expectedMessage,actualMessage)

        expectedMessage='(Note: Double-check your spam folder and "Promotions" tab if you don\\\'t see the email.)'
        actualMessage=succes_forgot_page.read_tmp_UGUI_text(succes_forgot_page.note_text)
        self.assertEqual(expectedMessage,actualMessage)




if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
