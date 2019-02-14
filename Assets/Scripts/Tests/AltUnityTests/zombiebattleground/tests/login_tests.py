# import unittest
# from appium import webdriver
# from altunityrunner import AltrunUnityDriver
# import xmlrunner
# from base import CZBTests


# class CZBLoginTests(CZBTests):
#     def test_login_invalid_test_key(self):

#         self.altdriver.wait_for_element('PressAnyText').mobile_tap()
#         self.altdriver.wait_for_element('InputField_Beta').set_component_property(
#             'UnityEngine.UI.InputField', 'text', '123456')
#         self.altdriver.find_element('Button_Beta').tap()
#         self.altdriver.wait_for_element('WarningPopup(Clone)')
#         self.assertTrue(self.altdriver.find_element('Text_Message').get_component_property(
#             'TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro') == 'Input a valid Tester Key')
#         self.driver.save_screenshot('./screenshots/invalid-login.png')

#     def test_login_valid_test_key(self):
#         self.altdriver.wait_for_element('PressAnyText').mobile_tap()
#         self.altdriver.wait_for_element('InputField_Beta').set_component_property(
#             'UnityEngine.UI.InputField', 'text', self.tester_key)
#         self.altdriver.find_element('Button_Beta').tap()

#         self.altdriver.wait_for_element('TermsPopup(Clone)/Toggle').tap()
#         self.driver.save_screenshot('./screenshots/term-and-conditions.png')

#         self.driver.save_screenshot(
#             './screenshots/term-and-conditions-toggle-checked.png')

#         button_got_it = self.altdriver.find_element('Button_GotIt')
#         self.assertNotEqual(button_got_it, None)

#         button_got_it.tap()

#         self.driver.save_screenshot('./screenshots/main-menu.png')


# if __name__ == '__main__':
#     unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
