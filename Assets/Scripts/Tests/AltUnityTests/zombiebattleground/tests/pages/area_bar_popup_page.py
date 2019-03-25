from .base import CZBTests

class Area_Bar_Popup_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.area_bar_popup_page=self.get_area_bar_popup_page()
        self.settings_button=self.get_settings_button()
        self.login_button=self.get_login_button()
        self.player_name_object=self.get_player_name_object()
    
    def get_area_bar_popup_page(self):
        return self.altdriver.wait_for_element('AreaBarPopup(Clone)')
    def get_login_button(self):
        return self.altdriver.wait_for_element(self.area_bar_popup_page.name+'/Button_Login')
    def get_player_name_object(self):
        return self.altdriver.wait_for_element(self.area_bar_popup_page.name+'/Text_PlayerName')
    def get_settings_button(self):
        return self.altdriver.wait_for_element(self.area_bar_popup_page.name+'/Button_Setting')

    def get_player_name_text(self):
        return self.read_tmp_UGUI_text(self.player_name_object)
    def press_login_button(self):
        self.button_pressed(self.login_button)
    def press_setting_button(self):
        self.button_pressed(self.settings_button)
