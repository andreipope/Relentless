from base import CZBTests
class Overlord_Ability_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.overlord_ability_popup_page=self.get_overlord_ability_popup_page()
        self.back_button=self.get_back_button()
        self.continue_button=self.get_continue_button()
    
    def get_overlord_ability_popup_page(self):
        return self.altdriver.wait_for_element('Tab_SelectOverlordSkill')
    def get_back_button(self):
        return self.altdriver.wait_for_element('Image_ButtonBackTray/Button_Back')
    def get_continue_button(self):
        return self.altdriver.wait_for_element(self.overlord_ability_popup_page.name+'/Canvas_BackLayer/Button_Continue')
    
    def press_continue(self):
        self.button_pressed(self.continue_button)
    def press_back(self):
        self.button_pressed(self.back_button)


