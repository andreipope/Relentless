from .base import CZBTests
class Regitration_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.registration_popup=self.get_registration_popup()
        self.login_button=self.get_login_button()
        self.close_button=self.get_close_button()
        self.register_button=self.get_register_button()
        self.email_input_field=self.get_email_input_field()
        self.password_input_field=self.get_password_input_field()
        self.password_confirmation_input_field=self.get_password_confirmation_input_field()
    
    def get_registration_popup(self):
        return self.altdriver.wait_for_element('LoginPopup(Clone)/Register_Group')
    def get_login_button(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Button_Login')
    def get_close_button(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Button_Close_BG/Button_Close')
    def get_register_button(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Button_Register_BG/Button_Register')   
    def get_email_input_field(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Email_BG/Email_InputField')
    def get_password_input_field(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Password_BG/Password_InputField')
    def get_password_confirmation_input_field(self):
        return self.altdriver.wait_for_element(self.registration_popup.name+'/Confirm_BG/Confirm_InputField')
    
    def register(self,email,password,confimation_password):
        self.write_in_input_field(self.email_input_field,email)
        self.write_in_input_field(self.password_input_field,password)
        self.write_in_input_field(self.password_confirmation_input_field,confimation_password)
        self.button_pressed(self.register_button)
    
