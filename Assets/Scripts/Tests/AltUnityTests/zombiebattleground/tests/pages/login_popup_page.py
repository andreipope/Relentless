from pages.base import CZBTests

class Login_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.login_popup=self.get_login_popup()
        self.login_button=self.get_login_button()
        self.close_button=self.get_close_button()
        self.forgot_password_button=self.get_forgot_password_button()
        self.email_input_field=self.get_email_input_field()
        self.password_input_field=self.get_password_input_field()
        self.register_button=self.get_register_button()
    
    def get_login_popup(self):
        element=self.altdriver.wait_for_element('LoginPopup(Clone)/Login_Group')
        while element==None:
            element=self.altdriver.wait_for_element('LoginPopup(Clone)/Login_Group')
            print(element)
        return self.altdriver.wait_for_element('LoginPopup(Clone)/Login_Group')
    def get_login_button(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Button_Login_BG/Button_Login')
    def get_close_button(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Button_Close_BG/Button_Close')
    def get_forgot_password_button(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Button_ForgotPassword')
    def get_email_input_field(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Email_BG/Email_InputField')
    def get_password_input_field(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Password_BG/Password_InputField')
    def get_register_button(self):
        return self.altdriver.wait_for_element(self.login_popup.name+'/Button_Register_BG/Button_Register')

    def go_to_registration_form(self):
        self.button_pressed(self.register_button)
    def go_to_forgot_password_form(self):
        self.button_pressed(self.forgot_password_button)


    def login(self,email,password):
        self.write_in_input_field(self.email_input_field,email)
        self.write_in_input_field(self.password_input_field,password)
        self.button_pressed(self.login_button)



