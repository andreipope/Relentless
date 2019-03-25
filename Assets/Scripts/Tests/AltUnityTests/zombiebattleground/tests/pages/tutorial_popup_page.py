from .base import CZBTests
import unittest

import xmlrunner

class Tutorial_Popup_Page(CZBTests):
    
    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.tutorial_popup_page=self.get_tutorial_popup_page()
        self.yes_button=self.get_yes_button()
        self.no_button=self.get_no_button()
        self.text_mesage=self.get_text_message()
        expectedMessage="Welcome, Zombie Slayer!\nWould you like a tutorial to get you started?"
        actualMessage=self.read_tmp_UGUI_text(self.text_mesage)
       
    
    def get_tutorial_popup_page(self):
        return self.altdriver.wait_for_element('QuestionPopup(Clone)')
    def get_yes_button(self):
        return self.altdriver.wait_for_element(self.tutorial_popup_page.name+'/Button_Yes')
    def get_no_button(self):
        return self.altdriver.wait_for_element(self.tutorial_popup_page.name+'/Button_No')
    def get_text_message(self):
        return self.altdriver.wait_for_element(self.tutorial_popup_page.name+'/Text_Message')
   

    def press_No_button(self):
        self.button_pressed(self.no_button)
    def press_Yes_button(self):
        self.button_pressed(self.yes_button)
    
