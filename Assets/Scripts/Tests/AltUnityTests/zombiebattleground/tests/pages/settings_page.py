from .base import CZBTests


class Settings_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.settings_page=self.altdriver.wait_for_element('MySettingsPopup(Clone)')
        self.close_button=self.altdriver.wait_for_element(self.settings_page.name+'/Button_Close')
        self.help_button=self.altdriver.wait_for_element(self.settings_page.name+'/Button_Help')
        self.support_button=self.altdriver.wait_for_element(self.settings_page.name+'/Button_Support')
        self.credits_button=self.altdriver.wait_for_element(self.settings_page.name+'/Button_Credits')
    def press_credits_button(self):
        self.button_pressed(self.credits_button)
    def press_close_button(self):
        self.button_pressed(self.close_button)
    