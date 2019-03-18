from .base import CZBTests
class Forgot_Password_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.forgot_password_popup=self.get_forgot_password_popup()
        self.send_button=self.get_send_button()
        self.cancel_button=self.get_cancel_button()
        self.email_input_field=self.get_email_input_field()
    
    def get_forgot_password_popup(self):
        return self.altdriver.wait_for_element('LoginPopup(Clone)/Forgot_Group')
    def get_send_button(self):
        return self.altdriver.wait_for_element(self.forgot_password_popup.name+'/Button_Send_BG/Button_Send')
    def get_cancel_button(self):
        return self.altdriver.wait_for_element(self.forgot_password_popup.name+'/Button_Cancel_BG/Button_Cancel')
    def get_email_input_field(self):
        return self.altdriver.wait_for_element(self.forgot_password_popup.name+'/Email_BG/Email_InputField')
    

    def press_send_button(self):
        self.button_pressed(self.send_button)
        self.altdriver.wait_for_element('Waiting_Group')
        self.altdriver.wait_for_element_to_not_be_present('Waiting_Group')

    def press_cancel_button(self):
        self.button_pressed(self.cancel_button)

    def forgot_password(self,email):
        self.write_in_input_field(self.email_input_field,email)
        self.press_send_button()
