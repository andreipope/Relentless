from .base import CZBTests

class Main_Menu_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.main_menu_page=self.get_main_menu_page()
        self.battle_button=self.get_battle_button()
        self.battle_mode_button=self.get_battle_mode_button()
    
    def get_main_menu_page(self):
        return self.altdriver.wait_for_element('MainMenuWithNavigationPage(Clone)')
    def get_battle_button(self):
        return self.altdriver.wait_for_element('Panel_BattleSwitch/Button_Battle')
    def get_battle_mode_button(self):
        return self.altdriver.wait_for_element(self.main_menu_page.name+'/Anchor_BottomRight/Panel_Battle_Mode')
   
    def press_battle_button(self):
        self.button_pressed(self.battle_button)
    def press_battle_mode_button(self):
        self.button_pressed(self.battle_mode_button)
    
