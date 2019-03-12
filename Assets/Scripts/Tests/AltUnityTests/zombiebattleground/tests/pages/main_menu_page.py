from pages.base import CZBTests

class Main_Menu_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.main_menu_page=self.get_main_menu_page()
        self.battle_button=self.get_battle_button()
        self.shop_button=self.get_shop_button()
        self.open_packs_button=self.get_open_packs_button()
        self.army_button=self.get_army_button()
        self.settings_button=self.get_settings_button()
        self.login_button=self.get_login_button()
    
    def get_main_menu_page(self):
        return self.altdriver.wait_for_element('MainMenuWithNavigationPage(Clone)')
    def get_login_button(self):
        return self.altdriver.wait_for_element('Anchor_BottomRight/AreaBarPopup(Clone)/Scaler/Button_Login')
    def get_battle_button(self):
        return self.altdriver.wait_for_element('Panel_BattleSwitch/Button_Battle')
    def get_shop_button(self):
        return self.altdriver.wait_for_element('SideMenuPopup(Clone)/Group/Button_Shop')
    def get_open_packs_button(self):
        return self.altdriver.wait_for_element('SideMenuPopup(Clone)/Group/Button_MyPacks')
    def get_army_button(self):
        return self.altdriver.wait_for_element('SideMenuPopup(Clone)/Group/Button_MyCards')
    def get_settings_button(self):
        return self.altdriver.wait_for_element('AreaBarPopup(Clone)/Scaler/Button_Settings')

    def go_to_login_form(self):
        self.login_button.mobile_tap()
    def press_battle_button(self):
        self.button_pressed(self.battle_button)
    def press_open_packs_button(self):
        self.button_pressed(self.open_packs_button)
