from .base import CZBTests


class Credits_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.credits_page=self.altdriver.wait_for_element('CreditsPopup(Clone)')
        self.back_button=self.altdriver.wait_for_element(self.credits_page.name+"/Button_Back")
        
    def press_back_button(self):
        self.button_pressed(self.back_button)
    
    