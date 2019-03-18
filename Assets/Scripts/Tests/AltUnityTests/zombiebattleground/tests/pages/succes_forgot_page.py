from .base import CZBTests
class Succes_Forgot_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.succes_forgot_popup=self.get_succes_forgot_popup()
        self.title_text=self.get_title_text()
        self.desc_text=self.get_desc_text()
        self.note_text=self.get_note_text()
        self.button_confirm=self.get_button_confirm()
    
    def get_succes_forgot_popup(self):
        return self.altdriver.wait_for_element('LoginPopup(Clone)/SuccessForgot_Group')
    def get_title_text(self):
        return self.altdriver.wait_for_element(self.succes_forgot_popup.name+'/Title_Text')
    def get_desc_text(self):
        return self.altdriver.wait_for_element(self.succes_forgot_popup.name+'/Desc_Text')
    def get_note_text(self):
        return self.altdriver.wait_for_element(self.succes_forgot_popup.name+'/Note_Text')
    def get_button_confirm(self):
        return self.altdriver.wait_for_element(self.succes_forgot_popup.name+'/Button_Confirm_BG/Button_Confirm')
    

