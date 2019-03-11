from pages.base import CZBTests

class Main_Menu_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.main_menu_page=self.get_main_menu_page()
        self.play_button=self.get_play_button()
        self.shop_button=self.get_shop_button()
        self.open_packs_button=self.get_open_packs_button()
        self.army_button=self.get_army_button()
        self.settings_button=self.get_settings_button()
        self.login_button=self.get_login_button()
    
    def get_main_menu_page(self):
        return self.altdriver.wait_for_element('MainMenuPage(Clone)')
    def get_login_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_Login')
    def get_play_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_Play')
    def get_shop_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_Shop')
    def get_open_packs_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_OpenPacks')
    def get_army_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_Army')
    def get_settings_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Button_Settings')

    def go_to_login_form(self):
        self.login_button.mobile_tap()
    def press_play_button(self):
        self.button_pressed(self.play_button)
    def press_open_packs_button(self):
        self.button_pressed(self.open_packs_button)
