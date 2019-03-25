import unittest
from appium import webdriver
from altunityrunner import AltrunUnityDriver, AltElement
import xmlrunner
from .pages.base import CZBTests
from .pages.main_menu_page import Main_Menu_Page
from .pages.credits_page import Credits_Page
from .pages.tutorial_popup_page import Tutorial_Popup_Page
import time
import subprocess


# import sys
# reload(sys)
# sys.setdefaultencoding('utf8')


class CZBMenuTests(CZBTests):
    credits_names = ['Melo the Cat', 'Nammy Salita', 'Macy Wanteh', 'Molly Thamrongvorapon', 'Tina Tang', 'Robert Hacala', 'Wang Shuwen (Katniss)', 'Stephanie Xiao', 'Molly Thamrongvorapon', 'Mami Mordovets', 'Hyunjung Ko (Hannah)', 'Michaela Thompson', 'Dilanka McLovin', 'Mohit Tater', 'Vaibhav Dixit', 'Yuneek Sehgal', 'Ryan Withrow', 'Lana Charto', 'Sing Ern Lee', 'Sing Huey Lee', 'Alexis Desuyo', 'Yuriy Tarasov', 'Alexandru Salajan', 'Pat Sevilla', 'Rastislav Le',
                     'Olga Pakskina', 'Niles Arguelles', 'Milica Celikovic', 'Lanny \xe2\x80\x9cWu\xe2\x80\x9d Suhandy', 'Karen Manalastas', 'Alex Alexandrov', 'Wilfred Dajotoy', 'Janette Ramos', 'Siddhant Mutha', 'Sebastian Klier', 'Lock Thepdusith', 'Gaurav Garg', 'Artem Shyriaiev', 'Cristian Esposito', 'Serhii Yolkin', 'Stanislav Sorokin', 'Sara Santillan', 'Alexei Menardo', 'Monika Guballa', 'Diana Shapira', 'Roy Shapira', 'James Duffy', 'Luke Zhang', 'Matthew Campbell']

    def setUp(self):
        super(CZBMenuTests, self).setUp()
        
        # self.altdriver.wait_for_element('Button_Skip').mobile_tap()
        # time.sleep(1)
        # self.altdriver.wait_for_element("Button_Yes").mobile_tap()
        # self.altdriver.wait_for_element_to_not_be_present(
        #     "LoadingGameplayPopup(Clone)")
        # self.altdriver.wait_for_element('Button_Back').mobile_tap()

    def test_credits_screen(self):
        self.altdriver.wait_for_element('Button_Credits').mobile_tap()
        self.altdriver.wait_for_element('Panel_CreditsList')
        self.driver.save_screenshot('./screenshots/credits-loaded.png')

        credits_items = self.altdriver.find_elements('Text_Name')
        found_names = []
        for item in credits_items:
            found_names.append(item.get_component_property(
                'TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro'))
        # for name in self.credits_names:
        #     self.assertIn(name, found_names)

        self.driver.save_screenshot('./screenshots/credits-rolling.png')
        self.altdriver.wait_for_element('Button_Thanks')
        self.driver.save_screenshot('./screenshots/credits-done-thank-you.png')
        self.altdriver.wait_for_element('CreditsPage(Clone)/Button_Back').tap()
        self.altdriver.wait_for_element('MainMenuPage(Clone)')

#     # def test_close_button(self):

#     #     self.altdriver.wait_for_element('Button_Quit').mobile_tap()
#     #     self.altdriver.wait_for_element('ConfirmationPopup(Clone)')
#     #     self.altdriver.wait_for_element('Text_Message')
#     #     self.altdriver.wait_for_element('Button_Yes')
#     #     no_button = self.altdriver.wait_for_element('Button_No')

#     #     self.driver.save_screenshot('./screenshots/confirmation-dialog.png')
#     #     no_button.mobile_tap()

#     #     self.altdriver.wait_for_element('Button_Quit').mobile_tap()
#     #     self.altdriver.wait_for_element('Button_Yes').mobile_tap()
#     #     time.sleep(5)
#     #     self.assertNotIn("games.loom.battleground", self.driver.page_source)


if __name__ == '__main__':
    
    
    


    # unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
