from .base import CZBTests
class Battle_Mode_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.battle_mode_selection_page=self.get_battle_mode_selection_page()
        self.back_button=self.get_back_button()
        self.solo_mode_button=self.get_solo_mode_button()
        self.pvp_mode_button=self.get_pvp_mode_button()
        self.tutorial_button=self.get_tutorial_button()
    
    def get_battle_mode_selection_page(self):
        return self.altdriver.wait_for_element('PlaySelectionPage(Clone)')
    def get_back_button(self):
        return self.altdriver.wait_for_element(self.battle_mode_selection_page.name+'Button_Back')
    def get_solo_mode_button(self):
        return self.altdriver.wait_for_element(self.battle_mode_selection_page.name+'/Button_SoloMode')
    def get_pvp_mode_button(self):
        return self.altdriver.wait_for_element(self.battle_mode_selection_page.name+'/Button_PvPMode')
    def get_tutorial_button(self):
        return self.altdriver.wait_for_element(self.battle_mode_selection_page.name+'/Button_Tutorial')

    def start_solo_match(self):
        self.button_pressed(self.solo_mode_button)
    def start_pvp_match(self):
        self.button_pressed(self.pvp_mode_button)
    def go_back_to_menu(self):
        self.button_pressed(self.back_button)
    def start_tutorial(self):
        self.button_pressed(self.tutorial_button)
